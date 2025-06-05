#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DecimalTextBox.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    // TODO: features into remarks:
    // - Coloring in disabled mode (by the base AdvancedTextBox)
    // - Value (decimal) property
    // - multiplier characters (t = thousand; m = million; y = billion (yard)) support
    // - Settable value limits
    // - Blank state (can be set only if BlankEnabled is true)
    // - DecimalFormat
    // - DecimalDigits: number of decimal digits, negative value means rounding
    /// <summary>
    /// A text box to edit decimal values.
    /// </summary>
    public class DecimalTextBox : AdvancedTextBox
    {
        #region Nested structs

        #region DecimalMinMax struct

        private struct DecimalMinMax
        {
            #region Fields

            private decimal minValue;
            private decimal maxValue;

            #endregion

            #region Properties

            internal decimal MinValue
            {
                get => minValue;
                set => minValue = value;
            }

            internal decimal MaxValue
            {
                get => maxValue;
                set => maxValue = value;
            }

            #endregion

            #region Constructors

            internal DecimalMinMax(decimal min, decimal max)
            {
                minValue = min;
                maxValue = max;
            }

            #endregion

            #region Methods

            public override string ToString() => $"{minValue}; {maxValue}";

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly char[] multipliers = { 'y', 'm', 't' };
        private readonly char thousandSeparator; // TODO: remove cache or reset on system preferences change
        private readonly char decimalSeparator;
        private readonly char negativeSign;

        private decimal value;
        private DecimalFormat format = DecimalFormat.Number;
        private sbyte decimalDigits; // decimals after the decimal separator
        private bool focused;  // because the real Focused is still true in the Leave event
        private bool blank = true;
        private string blankText = "";
        private bool blankEnabled = true;
        private DecimalRange range = DecimalRange.Any; // when violated, going to Blank, or exception
        private DecimalMinMax rangeMinMax = new DecimalMinMax(0, 0);
        private HorizontalAlignment align = HorizontalAlignment.Right;
        private DecimalValueOnBlank valueOnBlank = DecimalValueOnBlank.Zero;
        private bool changeValueOnTextChange;
        private bool textChanging;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when <see cref="Value"/> has been changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when Value has been changed.")]
        public event EventHandler? ValueChanged;

        /// <summary>
        /// Occurs when <see cref="Blank"/> has been changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when Blank has been changed.")]
        public event EventHandler? BlankChanged;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the <see cref="DecimalTextBox"/> is in blank state.
        /// Can be set on ly if <see cref="BlankEnabled"/> is <see langword="true"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether the DecimalTextBox is in blank state. Can be set only if BlankEnabled is true.")]
        [DefaultValue(true)]
        public bool Blank
        {
            get => blank;
            set
            {
                bool refresh = false;

                if (blank != value && (blankEnabled || !value))
                {
                    // when turning off blank, making sure Value is in range
                    bool blankOld = blank;
                    blank = value;
                    if (!value)
                    {
                        decimal scale = decimalDigits < 0 ? Convert.ToDecimal(Math.Pow(10, -decimalDigits)) : 1;

                        if (range == DecimalRange.Negative && this.value > -scale)
                            Value = -scale;
                        else if (range == DecimalRange.Positive && this.value < scale)
                            Value = scale;
                        if ((range == DecimalRange.NegativeNull && this.value > 0)
                            || (range == DecimalRange.PositiveNull && this.value < 0))
                            Value = 0;
                        else if (range == DecimalRange.MinMax && !(this.value >= rangeMinMax.MinValue && this.value <= rangeMinMax.MaxValue))
                            if (this.value < rangeMinMax.MinValue)
                                Value = rangeMinMax.MinValue;
                            else Value = rangeMinMax.MaxValue;
                        else refresh = true;
                    }
                    else refresh = true;
                    if (refresh)
                        RefreshValue();
                    OnBlankChanged(EventArgs.Empty);
                    if (blankOld != blank && this.value != BlankValue)
                        OnValueChanged(EventArgs.Empty);

                }
                else if (blank && Text != blankText)
                    RefreshValue();

                AdjustAlignment();
            }
        }

        /// <summary>
        /// Gets or sets the caption in <see cref="Blank"/> state.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the caption in Blank state.")]
        [DefaultValue("")]
        public string BlankText
        {
            get => blankText;
            set
            {
                blankText = value;
                if (blank) RefreshValue();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Blank"/> state can be enabled.
        /// When <see langword="true"/>, then the <see cref="DecimalTextBox"/> will be automatically blank if <see cref="Value"/> is out of range.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether Blank state can be enabled. " +
                    "When true, then the DecimalTextBox will be automatically blank if Value is out of range.")]
        [DefaultValue(true)]
        public bool BlankEnabled
        {
            get => blankEnabled;
            set
            {
                blankEnabled = value;
                if (!value && blank)
                {
                    Blank = false;
                }
            }

        }

        /// <summary>
        /// Gets or sets what <see cref="Value"/> should be returned in <see cref="Blank"/> state.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets what Value should be returned in Blank state.")]
        [DefaultValue(typeof(DecimalValueOnBlank), "Zero")]
        [RefreshProperties(RefreshProperties.All)]
        public DecimalValueOnBlank ValueOnBlank
        {
            get => valueOnBlank;
            set => valueOnBlank = value;
        }

        /// <summary>
        /// Gets or sets the format of the displayed <see cref="Text"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the format of the displayed Text.")]
        [DefaultValue(typeof(DecimalFormat), "Number")]
        public DecimalFormat Format
        {
            get => format;
            set
            {
                format = value;
                RefreshValue();
            }
        }

        /// <summary>
        /// Gets or sets the used fraction digits. When negative, then <see cref="Value"/> is rounded to the number of specified digits.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the used fraction digits. When negative, then Value is rounded to the number of specified digits.")]
        [DefaultValue(typeof(sbyte), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public sbyte DecimalDigits
        {
            get => decimalDigits;
            set
            {
                decimalDigits = value;
                if (range == DecimalRange.MinMax)
                {
                    rangeMinMax.MinValue = RoundTo(rangeMinMax.MinValue, -decimalDigits);
                    rangeMinMax.MaxValue = RoundTo(rangeMinMax.MaxValue, -decimalDigits);
                }
                if (!blank)
                {
                    // because the value may change when decreasing the decimal digits
                    SetValue(this.value, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the the valid range of <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set range, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the the valid range of Value. " +
                    "If Value violates newly set range, then Blank will be set or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(DecimalRange), "Any")]
        public DecimalRange Range
        {
            get => range;
            set
            {
                range = value;
                if (value != DecimalRange.MinMax)
                    rangeMinMax = new DecimalMinMax(0, 0);
                CheckRange(this.value, false);
                RefreshValue();
            }
        }

        /// <summary>
        /// Gets or sets accepted minimum <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set minimum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets accepted minimum Value. " +
                    "If Value violates newly set minimum value, then Blank will be set or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMinValue
        {
            get => rangeMinMax.MinValue;
            set
            {
                decimal rounded = RoundTo(value, -decimalDigits);
                if (rangeMinMax.MaxValue < rounded)
                    rangeMinMax.MaxValue = rounded;
                rangeMinMax.MinValue = rounded;
                Range = DecimalRange.MinMax;
            }
        }

        /// <summary>
        /// Gets or sets accepted maximum <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set maximum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets accepted maximum Value. " +
                    "If Value violates newly set maximum value, then Blank will be set or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMaxValue
        {
            get => rangeMinMax.MaxValue;
            set
            {
                decimal rounded = RoundTo(value, -decimalDigits);
                if (rangeMinMax.MinValue > rounded)
                    rangeMinMax.MinValue = rounded;
                rangeMinMax.MaxValue = rounded;
                Range = DecimalRange.MinMax;
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="DecimalTextBox"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the value of the DecimalTextBox.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal Value
        {
            get => !blank ? value : BlankValue;
            set => SetValue(value, true);
        }

        /// <summary>
        /// Gets or sets text of the <see cref="DecimalTextBox"/>. Whenever possible use <see cref="Value"/> property instead.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get => base.Text;
            set => SetText(value);
        }

        /// <summary>
        /// Gets or sets text align.
        /// </summary>
        [Description("Gets or sets text align.")]
        [Category("DecimalTextBox")]
        [DefaultValue(typeof(HorizontalAlignment), "Right")]
        public new HorizontalAlignment TextAlign
        {
            get => align;
            set
            {
                align = value;
                AdjustAlignment();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Value"/> should be changed for every keystroke when text is edited.
        /// By default, Value changes only when the control is left.
        /// </summary>
        [Description("Gets or sets whether Value should be changed for every keystroke when text is edited. By default, Value changes only when the control is left.")]
        [Category("DecimalTextBox")]
        [DefaultValue(false)]
        public bool ChangeValueOnTextChange
        {
            get => changeValueOnTextChange;
            set => changeValueOnTextChange = value;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets value in blank state.
        /// </summary>
        private decimal BlankValue
        {
            get
            {
                switch (valueOnBlank)
                {
                    case DecimalValueOnBlank.Zero:
                        return decimal.Zero;
                    case DecimalValueOnBlank.Value:
                        return value;
                    case DecimalValueOnBlank.LowerLimitMinusOne:
                        switch (Range)
                        {
                            case DecimalRange.Positive:
                                return decimal.Zero;
                            case DecimalRange.PositiveNull:
                                return decimal.MinusOne;
                            case DecimalRange.MinMax:
                                if (RangeMinValue > decimal.MinValue)
                                    return RangeMinValue - 1;
                                else
                                    return decimal.MinValue;
                            default:
                                return decimal.MinValue;
                        }
                    case DecimalValueOnBlank.UpperLimitPlusOne:
                        switch (Range)
                        {
                            case DecimalRange.Negative:
                                return decimal.Zero;
                            case DecimalRange.NegativeNull:
                                return decimal.One;
                            case DecimalRange.MinMax:
                                if (RangeMaxValue < decimal.MaxValue)
                                    return RangeMaxValue + 1;
                                else
                                    return decimal.MaxValue;
                            default:
                                return decimal.MaxValue;
                        }
                    case DecimalValueOnBlank.MinInt:
                        return Int32.MinValue;
                    case DecimalValueOnBlank.MaxInt:
                        return Int32.MaxValue;
                    case DecimalValueOnBlank.MinDecimal:
                        return Decimal.MinValue;
                    case DecimalValueOnBlank.MaxDecimal:
                        return Decimal.MaxValue;
                    default:
                        return decimal.Zero;
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DecimalTextBox"/> control.
        /// </summary>
        public DecimalTextBox()
        {
            TextAlign = HorizontalAlignment.Right;
            if (!DesignMode)
            {
                thousandSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
                decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
                negativeSign = Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeSign[0];
            }
        }

        #endregion

        #region Methods

        #region Instance Methods

        #region Static Methods

        private static decimal RoundTo(decimal value, int order)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, true);

            ci.NumberFormat.NumberDecimalDigits = order <= 0 ? -order : 0;
            if (order > 0)
            {
                decimal scale = 10m.Pow(order);
                return Decimal.Parse((value / scale).ToString("F", ci), ci) * scale;
            }
            else
                return Decimal.Parse(value.ToString("F", ci), ci);
        }

        #endregion

        #region Protected Methods

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (ReadOnly)
                return;
            focused = true;
            RefreshValue();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (ReadOnly)
                return;
            focused = false;
            if (IsValid(Text, true))
                SetText(Text); // Setting the new value. Validation is needed because Leave executes before Validate
            else if (blankEnabled && string.IsNullOrEmpty(Text))
                Blank = true;  // and if it's invalid while not blank, Validating does not allow to leave the control
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            // validating key press
            if (ReadOnly)
                return;

            // invalidating: thousands separator (or space)...
            if (e.KeyChar == thousandSeparator || e.KeyChar == ' '
                // ...negative sign not at the first position...
                || (!blank && e.KeyChar == negativeSign && Text.IndexOf(negativeSign) >= 0)
                //  ...when the result would not be valid (IsValid allows multiplier at the last position)
                || (!blank && !char.IsControl(e.KeyChar) &&
                    !IsValid(Text.Substring(0, SelectionStart) + e.KeyChar + Text.Substring(SelectionStart + SelectionLength), false)))
                e.KeyChar = '\0';

            // valid char in Blank: turning off blank mode
            else if (blank && !char.IsControl(e.KeyChar))
            {
                if (IsValid(e.KeyChar.ToString(), false))
                {
                    BlankOff();
                }
                else e.KeyChar = '\0';
            }

            // applying multipliers
            if (e.KeyChar.ToString().ToLowerInvariant().IndexOfAny(multipliers) >= 0)
            {
                ApplyText(e.KeyChar.ToString(), true);
                SelectionStart = Text.Length;
                e.KeyChar = '\0';
            }
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            base.OnValidating(e);
            if (ReadOnly || blank)
                return;
            if (!IsValid(Text, true))
            {
                e.Cancel = true;
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (textChanging)
                return;

            // Switching to Blank if needed
            if (!blank && blankEnabled && Text.Length == 0)
            {
                Blank = true;
                return;
            }

            // changing Value property for any Text change if ChangeValueOnTextChange is true
            if (!changeValueOnTextChange || ReadOnly)
                return;
            textChanging = true;
            try
            {
                if (!Blank && IsValid(Text, true))
                    SetValue(Decimal.Parse(Text, CultureInfo.CurrentCulture), false);
            }
            finally
            {
                textChanging = false;
            }
        }

        /// <summary>
        /// Invokes <see cref="ValueChanged"/> event.
        /// </summary>
        protected void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        /// <summary>
        /// Invokes <see cref="BlankChanged"/> event.
        /// </summary>
        protected void OnBlankChanged(EventArgs e) => BlankChanged?.Invoke(this, e);

        /// <summary>
        /// Suppressing keys in Blank mode. Further checks are in KeyPress where key can be checked as char.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (blank && e.KeyCode.In(Keys.Delete, Keys.Back))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override void WndProc(ref Message m)
        {
            // pasting attempt from clipboard
            if (m.Msg == Constants.WM_PASTE) // WM_PASTE
            {
                if (!Clipboard.ContainsText())
                    return;

                string text = blank ? Clipboard.GetText()
                        : (Text.Substring(0, SelectionStart) + Clipboard.GetText() + Text.Substring(SelectionStart + SelectionLength));

                if (IsValid(text, false))
                {
                    int selstart = blank ? 0 : SelectionStart;
                    ApplyText(Clipboard.GetText(), !blank);
                    SelectionStart = selstart + Clipboard.GetText().Length;
                }
            }
            // suppressing editing in Blank mode
            else if (blank && m.Msg.In(Constants.WM_CUT, Constants.WM_CLEAR))
                return;
            else base.WndProc(ref m);
        }

        #endregion

        #region Private Methods

        private void AdjustAlignment()
        {
            // We could align the text to the left in blank mode here. It has been removed.

            //if (blank)
            //    base.TextAlign = HorizontalAlignment.Left;
            //else
            base.TextAlign = align;
        }

        /// <summary>
        /// Set value and refreshes text.
        /// </summary>
        /// <param name="value">Value to set</param>
        /// <param name="alert">When true exception will be thrown if BlankEnabled is false and value violates range</param>
        private void SetValue(decimal value, bool alert)
        {
            decimal rounded = RoundTo(value, -decimalDigits);
            Blank = false;
            if (!CheckRange(rounded, alert))
                return;
            if (this.value != rounded)
            {
                this.value = rounded;
                OnValueChanged(EventArgs.Empty);
            }
            else
                this.value = rounded; // because e.g. 1 and 1.0 are different, though they equal
            if (!textChanging)
                RefreshValue();
        }

        private void RefreshValue()
        {
            if (blank)
            {
                base.Text = blankText;
                return;
            }
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, true);
            ci.NumberFormat.NumberDecimalDigits = decimalDigits >= 0 ? Convert.ToInt32(decimalDigits) : 0;
            if (focused)
            {
                base.Text = value.ToString("F", ci);
            }
            else
            {
                switch (format)
                {
                    case DecimalFormat.Fixed:
                        base.Text = value.ToString("F", ci);
                        break;
                    case DecimalFormat.Number:
                        base.Text = value.ToString("N", ci);
                        break;
                }
            }
        }

        /// <summary>
        /// Is typed string valid.
        /// If called from anywhere with strong = false, then call ApplyText or process multipliers in text.
        /// </summary>
        /// <param name="strong">When false, then a single minus sign can be accepted or can contain multipliers at the end.</param>
        /// <returns>Valid state</returns>
        private bool IsValid(string s, bool strong)
        {
            decimal d = 0;
            bool result = (!strong && (s == "-" || s == ""
                            || s[s.Length - 1].ToString().ToLowerInvariant().IndexOfAny(multipliers) >= 0))
                    || decimal.TryParse(s, out d);
            if (result && strong)
                result = CheckRange(d, false);
            return result;
        }

        /// <summary>
        /// Checks range. On violation sets Blank or when it is not enabled fixes Value and if alert is true throws exception.
        /// </summary>
        private bool CheckRange(decimal value, bool alert)
        {
            bool result;
            decimal scale = decimalDigits < 0 ? Convert.ToDecimal(Math.Pow(10, -decimalDigits)) : 1;

            if (blank) return true;

            result = range switch
            {
                DecimalRange.Negative => value <= -scale,
                DecimalRange.NegativeNull => value <= 0,
                DecimalRange.Positive => value >= scale,
                DecimalRange.PositiveNull => value >= 0,
                DecimalRange.MinMax => value >= rangeMinMax.MinValue && value <= rangeMinMax.MaxValue,
                _ => true
            };

            if (result)
                return true;
            if (!blankEnabled)
            {
                // if blank is not enabled, then we may need to correct the Value
                Value = range switch
                {
                    DecimalRange.Negative => -1 * scale,
                    DecimalRange.Positive => 1 * scale,
                    DecimalRange.MinMax => value < rangeMinMax.MinValue ? rangeMinMax.MinValue : rangeMinMax.MaxValue,
                    _ => 0
                };

                if (alert)
                    throw new OverflowException("Value \"" + value + "\" violates current Range");
            }
            else
                Blank = true;

            return false;
        }

        /// <summary>
        /// Setting off Blank without changing Value. Can be called only during typing.
        /// </summary>
        private void BlankOff()
        {
            if (!blank)
                return;

            blank = false;
            OnBlankChanged(EventArgs.Empty);

            textChanging = true;
            try
            {
                base.Text = String.Empty;
            }
            finally
            {
                textChanging = false;
            }
            AdjustAlignment();
        }

        private void SetText(string value)
        {
            decimal d;
            if (String.IsNullOrEmpty(value))
            {
                if (blankEnabled)
                {
                    Blank = true;
                    return;
                }
                else d = 0;
            }
            else
            {
                string text = value.Trim().ToLowerInvariant();
                char mult = text[text.Length - 1];

                if (mult.ToString().IndexOfAny(multipliers) >= 0)
                    text = text.Substring(0, text.Length - 1);

                if (!decimal.TryParse(text, out d))
                    throw new InvalidOperationException("Cannot assign value as decimal number: " + value);

                try
                {
                    if (d != 0)
                        switch (mult)
                        {
                            case 't': d *= 1000M; break;
                            case 'm': d *= 1000000M; break;
                            case 'y': d *= 1000000000M; break;
                        }
                }
                catch (Exception e)
                {
                    throw new OverflowException("Value does not fit in a decimal number's range: " + value, e);
                }
            }
            SetValue(d, true);
        }

        /// <summary>
        /// Inserting text as number into the text field.
        /// </summary>
        /// <param name="text">Text to insert</param>
        /// <param name="insert">When true replaces only selected text; otherwise, replaces thr whole text</param>
        private void ApplyText(string text, bool insert)
        {
            if (blank)
                insert = false;

            if (insert)
            {
                text = Text.Substring(0, SelectionStart) +
                    text + Text.Substring(SelectionStart +
                            SelectionLength);
            }

            BlankOff();

            if (text == "")
                return;

            text = text.ToLowerInvariant();
            if (text[text.Length - 1].ToString().IndexOfAny(multipliers) >= 0)
            {
                decimal num;
                decimal.TryParse(text.Substring(0, text.Length - 1), out num);
                try
                {
                    if (num != 0)
                        switch (text[text.Length - 1])
                        {
                            case 't': num *= 1000; break;
                            case 'm': num *= 1000000; break;
                            case 'y': num *= 1000000000; break;
                        }
                }
                catch (Exception e) when (!e.IsCritical())
                {
                    // During editing we can suppress the exception - in this case simply there is no multiplication.
                    // (When setting Text, we would throw though)
                }
                text = num.ToString(CultureInfo.CurrentCulture);
                while (text.IndexOf(decimalSeparator) >= 0 && text[text.Length - 1].In(decimalSeparator, '0'))
                    text = text.Remove(text.Length - 1);
            }

            base.Text = text;
        }

        #endregion

        #endregion

        #endregion
    }
}
