#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DecimalTextBox.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETCOREAPP3_0_OR_GREATER
#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf' - there is no String.Contains(string, StringComparison) method in some targeted platforms
#endif

#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable CS8602 // Dereference of a possibly null reference. - analyzer false alarm for .NET Framework
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a text box to edit decimal values.
    /// </summary>
    /// <remarks>
    /// The <see cref="DecimalTextBox"/> control offers the following features:
    /// <list type="bullet">
    /// <item>Coloring in disabled mode (provided by the base <see cref="AdvancedTextBox"/> control).</item>
    /// <item>A <see cref="decimal">decimal</see>&#160;<see cref="Value"/> property for getting/setting the decimal value.</item>
    /// <item>Multiplier hotkeys: <c>t</c> = thousand; <c>m</c> = million; <c>y</c> = billion (yard).</item>
    /// <item>Configurable limits for <see cref="Value"/>.</item>
    /// <item>Optional blank state if <see cref="BlankEnabled"/> is <see langword="true"/>.</item>
    /// <item>Formatting options (see <see cref="Format"/>).</item>
    /// <item>Configurable number of decimal digits or rounding (see <see cref="DecimalDigits"/>).</item>
    /// </list>
    /// </remarks>
    [Description(@"A text box for decimal values. Some highlighted features:
- A decimal Value property
- Multiplier hotkeys: t = thousand; m = million; y = billion (yard)
- Configurable limits
- Optional blank state (if BlankEnabled is true)
- Formatting options
- Configurable number of decimal digits or rounding
- Coloring in disabled mode (by the base AdvancedTextBox control)
- Auto scaling Font on all platform targets (by the base AdvancedTextBox)")]
    [ToolboxBitmap(typeof(DecimalTextBox), "Resources.Toolbox.DecimalTextBox.png")]
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
                readonly get => minValue;
                set => minValue = value;
            }

            internal decimal MaxValue
            {
                readonly get => maxValue;
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

            public override readonly string ToString() => $"{minValue}; {maxValue}";

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        // We could use BitVector32.CreateMask, but then we should use static fields, whose access is slower than using constants.
        // NOTE: LSB flags are in the base AdvancedTextBox class, so starting with bit 16
        private const int focused = 1 << 16; // needed, because the real Focused is still true in the Leave event
        private const int isBlank = focused << 1;
        private const int blankEnabled = isBlank << 1;
        private const int changeValueOnTextChange = blankEnabled << 1;
        private const int textChanging = changeValueOnTextChange << 1;

        #endregion

        #region Fields

        private readonly char[] multipliers = { 'y', 'm', 't' };
        private readonly char thousandSeparator; // TODO: remove cache or reset on system preferences change
        private readonly char decimalSeparator;
        private readonly char negativeSign;

        private decimal value;
        private DecimalFormat format = DecimalFormat.Number;
        private sbyte decimalDigits; // decimals after the decimal separator
        private string blankText = String.Empty;
        private DecimalRange range = DecimalRange.Any; // when violated, going to Blank, or exception
        private DecimalMinMax rangeMinMax = new DecimalMinMax(0, 0);
        private HorizontalAlignment align = HorizontalAlignment.Right;
        private DecimalValueOnBlank valueOnBlank = DecimalValueOnBlank.Zero;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="Value"/> property has changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when the Value property has changed.")]
        public event EventHandler? ValueChanged
        {
            add => Events.AddHandler(nameof(ValueChanged), value);
            remove => Events.RemoveHandler(nameof(ValueChanged), value);
        }

        /// <summary>
        /// Occurs when the <see cref="Blank"/> property has changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when the Blank property has changed.")]
        public event EventHandler? BlankChanged
        {
            add => Events.AddHandler(nameof(BlankChanged), value);
            remove => Events.RemoveHandler(nameof(BlankChanged), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the <see cref="DecimalTextBox"/> is in blank state.
        /// Can be set only if <see cref="BlankEnabled"/> is <see langword="true"/>.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether the DecimalTextBox is in blank state. Can be set only if BlankEnabled is true.")]
        [DefaultValue(true)]
        public bool Blank
        {
            get => flags[isBlank];
            set
            {
                bool refresh = false;

                if (flags[isBlank] != value && (BlankEnabled || !value))
                {
                    // when turning off blank, making sure Value is in range
                    bool blankOld = flags[isBlank];
                    flags[isBlank] = value;
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
                    if (blankOld != value && this.value != BlankValue)
                        OnValueChanged(EventArgs.Empty);

                }
                else if (value && Text != blankText)
                    RefreshValue();

                AdjustAlignment();
            }
        }

        /// <summary>
        /// Gets or sets the caption in <see cref="Blank"/> state.
        /// <br/>Default value: Empty string.
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
                if (Blank)
                    RefreshValue();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Blank"/> state can be enabled.
        /// When <see langword="true"/>, then the <see cref="DecimalTextBox"/> will be automatically blank if <see cref="Value"/> is out of range.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether Blank state can be enabled. " +
                    "When true, then the DecimalTextBox will be automatically blank if Value is out of range.")]
        [DefaultValue(true)]
        public bool BlankEnabled
        {
            get => flags[blankEnabled];
            set
            {
                flags[blankEnabled] = value;
                if (!value && Blank)
                    Blank = false;
            }

        }

        /// <summary>
        /// Gets or sets what the <see cref="Value"/> property should return in <see cref="Blank"/> state.
        /// <br/>Default value: <see cref="DecimalValueOnBlank.Zero"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets what the Value property should return in Blank state.")]
        [DefaultValue(DecimalValueOnBlank.Zero)]
        [RefreshProperties(RefreshProperties.All)]
        public DecimalValueOnBlank ValueOnBlank
        {
            get => valueOnBlank;
            set => valueOnBlank = value;
        }

        /// <summary>
        /// Gets or sets the numeric formatting of the displayed <see cref="Value"/>.
        /// <br/>Default value: <see cref="DecimalFormat.Number"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the format of the displayed Value.")]
        [DefaultValue(DecimalFormat.Number)]
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
        /// Gets or sets the used fraction digits. When negative, <see cref="Value"/> is rounded to the number of specified digits.
        /// <br/>Default value: 0.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the used fraction digits. When negative, Value is rounded to the number of specified digits.")]
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

                if (!Blank)
                {
                    // because the value may change when decreasing the decimal digits
                    SetValue(this.value, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the valid range for the <see cref="Value"/> property.
        /// If <see cref="Value"/> violates the newly set range, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the the valid range for the Value property. " +
            "If Value violates the newly set range, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
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
        /// Gets or sets the accepted minimum <see cref="Value"/>.
        /// If <see cref="Value"/> violates the newly set minimum value, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets accepted minimum Value. " +
            "If Value violates the newly set minimum value, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
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
        /// Gets or sets the accepted maximum <see cref="Value"/>.
        /// If <see cref="Value"/> violates the newly set maximum value, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the accepted maximum Value. " +
            "If Value violates the newly set maximum value, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
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
        /// <br/>Default value: 0.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the value of the DecimalTextBox.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal Value
        {
            get => !Blank ? value : BlankValue;
            set => SetValue(value, true);
        }

        /// <summary>
        /// Gets or sets the text of the <see cref="DecimalTextBox"/>. It is recommended to use the <see cref="Value"/> property instead.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [AllowNull]
        public override string Text
        {
            get => base.Text;
            set => SetText(value);
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// <br/>Default value: <see cref="HorizontalAlignment.Right"/>.
        /// </summary>
        [Description("Gets or sets the horizontal text alignment.")]
        [Category("DecimalTextBox")]
        [DefaultValue(HorizontalAlignment.Right)]
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
        /// Gets or sets whether <see cref="Value"/> should be changed for every keystroke when the text is edited.
        /// <br/>Default value: <see langword="false"/>, meaning, <see cref="Value"/> changes only when the control is left.
        /// </summary>
        [Description("Gets or sets whether Value should be changed for every keystroke when text is edited. By default, Value changes only when the control is left.")]
        [Category("DecimalTextBox")]
        [DefaultValue(false)]
        public bool ChangeValueOnTextChange
        {
            get => flags[changeValueOnTextChange];
            set => flags[changeValueOnTextChange] = value;
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
                        return Decimal.Zero;
                    case DecimalValueOnBlank.Value:
                        return value;
                    case DecimalValueOnBlank.LowerLimitMinusOne:
                        switch (Range)
                        {
                            case DecimalRange.Positive:
                                return Decimal.Zero;
                            case DecimalRange.PositiveNull:
                                return Decimal.MinusOne;
                            case DecimalRange.MinMax:
                                if (RangeMinValue > Decimal.MinValue)
                                    return RangeMinValue - 1;
                                else
                                    return Decimal.MinValue;
                            default:
                                return Decimal.MinValue;
                        }
                    case DecimalValueOnBlank.UpperLimitPlusOne:
                        switch (Range)
                        {
                            case DecimalRange.Negative:
                                return Decimal.Zero;
                            case DecimalRange.NegativeNull:
                                return Decimal.One;
                            case DecimalRange.MinMax:
                                if (RangeMaxValue < Decimal.MaxValue)
                                    return RangeMaxValue + 1;
                                else
                                    return Decimal.MaxValue;
                            default:
                                return Decimal.MaxValue;
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
                        return Decimal.Zero;
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
            flags[isBlank | blankEnabled] = true;
            TextAlign = HorizontalAlignment.Right;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            thousandSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
            decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            negativeSign = Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeSign[0];
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

        /// <inheritdoc />
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (ReadOnly)
                return;
            flags[focused] = true;
            RefreshValue();
        }

        /// <inheritdoc />
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (ReadOnly)
                return;
            flags[focused] = false;
            if (IsValid(Text, true))
                SetText(Text); // Setting the new value. Validation is needed because Leave executes before Validate
            else if (BlankEnabled && string.IsNullOrEmpty(Text))
                Blank = true;  // and if it's invalid while not blank, Validating does not allow to leave the control
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            // validating key press
            if (ReadOnly)
                return;

            // invalidating: thousands separator (or space)...
            bool blank = Blank;
            if (e.KeyChar == thousandSeparator || e.KeyChar == ' '
                // ...negative sign not at the first position...
                || (!blank && e.KeyChar == negativeSign && Text.IndexOf(negativeSign) >= 0)
                //  ...when the result would not be valid (IsValid allows multiplier at the last position)
                || (!blank && !Char.IsControl(e.KeyChar) &&
                    !IsValid(Text.Substring(0, SelectionStart) + e.KeyChar + Text.Substring(SelectionStart + SelectionLength), false)))
            {
                e.KeyChar = '\0';
            }
            // valid char in Blank: turning off blank mode
            else if (blank && !Char.IsControl(e.KeyChar))
            {
                if (IsValid(e.KeyChar.ToString(), false))
                    BlankOff();
                else
                    e.KeyChar = '\0';
            }

            // applying multipliers
            if (e.KeyChar.ToString().ToLowerInvariant().IndexOfAny(multipliers) >= 0)
            {
                ApplyText(e.KeyChar.ToString(), true);
                SelectionStart = Text.Length;
                e.KeyChar = '\0';
                e.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnValidating(CancelEventArgs e)
        {
            base.OnValidating(e);
            if (ReadOnly || Blank)
                return;
            if (!IsValid(Text, true))
                e.Cancel = true;
        }

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (flags[textChanging])
                return;

            // Switching to Blank if needed
            if (!Blank && BlankEnabled && Text.Length == 0)
            {
                Blank = true;
                return;
            }

            // changing Value property for any Text change if ChangeValueOnTextChange is true
            if (!ChangeValueOnTextChange || ReadOnly)
                return;
            flags[textChanging] = true;
            try
            {
                if (!Blank && IsValid(Text, true))
                    SetValue(Decimal.Parse(Text, CultureInfo.CurrentCulture), false);
            }
            finally
            {
                flags[textChanging] = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected void OnValueChanged(EventArgs e) => Events.GetHandler<EventHandler>(nameof(ValueChanged))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="BlankChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected void OnBlankChanged(EventArgs e) => Events.GetHandler<EventHandler>(nameof(BlankChanged))?.Invoke(this, e);

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Suppressing keys in Blank mode. Further checks are in KeyPress where key can be checked as char.
            base.OnKeyDown(e);

            if (Blank && e.KeyCode is Keys.Delete or Keys.Back)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            // pasting attempt from clipboard
            switch (m.Msg)
            {
                case Constants.WM_PASTE:
                    if (!Clipboard.ContainsText())
                        return;

                    try
                    {
                        bool blank = Blank;
                        string clipboardText = Clipboard.GetText();
                        string text = blank
                            ? clipboardText
                            : $"{Text.Substring(0, SelectionStart)}{clipboardText}{Text.Substring(SelectionStart + SelectionLength)}";
                        if (IsValid(text, false))
                        {
                            int selStart = blank ? 0 : SelectionStart;
                            ApplyText(clipboardText, !blank);
                            SelectionStart = selStart + clipboardText.Length;
                        }
                    }
                    catch (Exception e) when (!e.IsCritical())
                    {
                    }

                    return;

                case Constants.WM_CUT or Constants.WM_CLEAR when Blank:
                    // suppressing editing in Blank mode
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
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
        /// Set value and refresh text.
        /// </summary>
        /// <param name="newValue">The new value to set</param>
        /// <param name="alert">When true exception will be thrown if BlankEnabled is false and value violates range</param>
        private void SetValue(decimal newValue, bool alert)
        {
            decimal rounded = RoundTo(newValue, -decimalDigits);
            Blank = false;
            if (!CheckRange(rounded, alert))
                return;
            if (value != rounded)
            {
                value = rounded;
                OnValueChanged(EventArgs.Empty);
            }
            else
                value = rounded; // because e.g. 1 and 1.0 are different, though they equal
            if (!flags[textChanging])
                RefreshValue();
        }

        private void RefreshValue()
        {
            if (Blank)
            {
                base.Text = blankText;
                return;
            }

            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, true);
            ci.NumberFormat.NumberDecimalDigits = decimalDigits >= 0 ? Convert.ToInt32(decimalDigits) : 0;
            if (flags[focused])
            {
                base.Text = value.ToString("F", ci);
                return;
            }

            base.Text = format switch
            {
                DecimalFormat.Fixed => value.ToString("F", ci),
                DecimalFormat.Number => value.ToString("N", ci),
                _ => base.Text
            };
        }

        /// <summary>
        /// Gets if typed string is valid.
        /// If called from anywhere with strong = false, then call ApplyText or process multipliers in text.
        /// When strong is false, then a single minus sign can be accepted or can contain multipliers at the end.
        /// </summary>
        private bool IsValid(string s, bool strong)
        {
            decimal d = 0;
            bool result = (!strong && (s is "-" or ""
                    || s[s.Length - 1].ToString().ToLowerInvariant().IndexOfAny(multipliers) >= 0))
                || Decimal.TryParse(s, out d);
            if (result && strong)
                result = CheckRange(d, false);
            return result;
        }

        /// <summary>
        /// Checks range. On violation sets Blank or when it is not enabled fixes Value and if alert is true throws exception.
        /// </summary>
        private bool CheckRange(decimal checkedValue, bool alert)
        {
            decimal scale = decimalDigits < 0 ? Convert.ToDecimal(Math.Pow(10, -decimalDigits)) : 1;

            if (Blank)
                return true;

            bool result = range switch
            {
                DecimalRange.Negative => checkedValue <= -scale,
                DecimalRange.NegativeNull => checkedValue <= 0,
                DecimalRange.Positive => checkedValue >= scale,
                DecimalRange.PositiveNull => checkedValue >= 0,
                DecimalRange.MinMax => checkedValue >= rangeMinMax.MinValue && checkedValue <= rangeMinMax.MaxValue,
                _ => true
            };

            if (result)
                return true;
            if (!BlankEnabled)
            {
                // if blank is not enabled, then we may need to correct the Value
                Value = range switch
                {
                    DecimalRange.Negative => -1 * scale,
                    DecimalRange.Positive => 1 * scale,
                    DecimalRange.MinMax => checkedValue < rangeMinMax.MinValue ? rangeMinMax.MinValue : rangeMinMax.MaxValue,
                    _ => 0
                };

                if (alert)
                    throw new OverflowException(Res.DecimalTextBoxInvalidValue(checkedValue));
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
            if (!Blank)
                return;

            flags[isBlank] = false;
            OnBlankChanged(EventArgs.Empty);

            flags[textChanging] = true;
            try
            {
                base.Text = String.Empty;
            }
            finally
            {
                flags[textChanging] = false;
            }
            AdjustAlignment();
        }

        private void SetText(string? txt)
        {
            decimal d;
            if (String.IsNullOrEmpty(txt))
            {
                if (BlankEnabled)
                {
                    Blank = true;
                    return;
                }

                d = 0;
            }
            else
            {
                string text = txt.Trim().ToLowerInvariant();
                char mult = text[text.Length - 1];

                if (mult.ToString().IndexOfAny(multipliers) >= 0)
                    text = text.Substring(0, text.Length - 1);

                if (!Decimal.TryParse(text, out d))
                    throw new InvalidOperationException(Res.DecimalTextBoxInvalidText(txt));

                try
                {
                    if (d != 0)
                    {
                        switch (mult)
                        {
                            case 't': d *= 1000m; break;
                            case 'm': d *= 1_000_000m; break;
                            case 'y': d *= 1_000_000_000m; break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw new OverflowException(Res.DecimalTextBoxOverflow(txt));
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
            if (Blank)
                insert = false;

            if (insert)
                text = $"{Text.Substring(0, SelectionStart)}{text}{Text.Substring(SelectionStart + SelectionLength)}";

            BlankOff();

            if (text == String.Empty)
                return;

            text = text.ToLowerInvariant();
            if (text[text.Length - 1].ToString().IndexOfAny(multipliers) >= 0)
            {
                decimal num = 0;
                try
                {
#if NETCOREAPP
                    if (Decimal.TryParse(text.AsSpan()[..^1], out num) && num != 0) 
#else
                    if (Decimal.TryParse(text.Substring(0, text.Length - 1), out num) && num != 0) 
#endif
                    {
                        switch (text[text.Length - 1])
                        {
                            case 't': num *= 1000; break;
                            case 'm': num *= 1000000; break;
                            case 'y': num *= 1000000000; break;
                        }
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
