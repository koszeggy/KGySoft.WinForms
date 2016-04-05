/*******************************************
 * DecimalTextBox - KGy
 * 
 * Fícsörök:
 * - Disabled módú színezhet?ség (mivel AdvancedTextBox-ból származik)
 * - Value (decimal) property
 * - Integer, Float, Text, Object típusú Value overloadok (AsInteger, AsText, AsObject, stb)
 * - Szorzó karakterek (t = ezer; m = millió; y = milliárd) támogatása
 * - Beállítható értékhatárok
 * - Blank állapot (kikapcsolható); hozzá BlankText (default: "", de lehet pl. "0" vagy "(Blank)")
 * - DecimalFormat (Number: ezres elválasztókkal való megjelenítés; Fixed: fixpontos, formázatlan)
 * - DecimalDigits: tizedesek száma, negatív érték esetén kerekítés adott jegyre
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using KGySoft.Controls.WinApi;
using KGySoft.Libraries;

namespace KGySoft.Controls
{

    #region namespace szintű típusok (enum, struct)

    /// <summary>
    /// Possible formats of <see cref="DecimalTextBox"/> control.
    /// </summary>
    public enum DecimalFormat
    {
        /// <summary>
        /// Represents fixed size formatting.
        /// </summary>
        Fixed,

        /// <summary>
        /// Represents general number formatting.
        /// </summary>
        Number
    }

    /// <summary>
    /// Represents possible ranges of <see cref="DecimalTextBox"/> control.
    /// </summary>
    public enum DecimalRange
    {
        /// <summary>
        /// Any value is accepted.
        /// </summary>
        Any,

        /// <summary>
        /// Positive values are accepted, excluding zero value.
        /// </summary>
        Positive,

        /// <summary>
        /// Negative values are accepted, excluding zero value.
        /// </summary>
        Negative,

        /// <summary>
        /// Positive values are accepted, including zero value.
        /// </summary>
        PositiveNull,

        /// <summary>
        /// Negative values are accepted, including zero value.
        /// </summary>
        NegativeNull,

        /// <summary>
        /// Accepted values are controlled by <see cref="DecimalTextBox.RangeMinValue"/> and <see cref="DecimalTextBox.RangeMaxValue"/> properties.
        /// </summary>
        MinMax
    }

    /// <summary>
    /// Controls <see cref="DecimalTextBox.Value"/> in <see cref="DecimalTextBox.Blank"/> state.
    /// </summary>
    public enum DecimalValueOnBlank
    {
        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return zero in <see cref="DecimalTextBox.Blank"/> state
        /// </summary>
        Zero,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return the internally stored value in <see cref="DecimalTextBox.Blank"/> state
        /// </summary>
        Value,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return lower limit minus one or <see cref="Decimal.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        LowerLimitMinusOne,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return upper limit plus one or <see cref="Decimal.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        UpperLimitPlusOne,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="Int32.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MinInt,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="Int32.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MaxInt,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="Decimal.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MinDecimal,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="Decimal.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MaxDecimal
    }

    #endregion

    // TODO: + ne csak BS, hanem Del esetén is visszaváltson Blank-re (olyan, mintha KeyUp-ban ez nem detektálódna, úgyhogy talán TextChanged-be kéne tenni)
    //       + Blank módban ne csak a paste, hanem Cut (del/backspace) letiltása is megtörténjen
    //       - A szorzók opcionálisak (és esetleg konfigurálhatóak) legyenek
    //       - ThousandSeparator, DecimalSeparator, NegativeSign jöhessen Language-b?l, CurrentCulture-b?l (mint most, csak ne szálból), vagy lehessen custom
    //         -> TODO: összes Decimal.Parse-ban Language/CurrentCulture...
    /// <summary>
    /// A text box to edit decimal values.
    /// </summary>
    public class DecimalTextBox: AdvancedTextBox
    {
        #region Osztályon belüli típusok

        private struct DecimalMinMax
        {
            private decimal minValue;
            private decimal maxValue;

            internal decimal MinValue
            {
                get { return minValue; }
                set { minValue = value; }
            }

            internal decimal MaxValue
            {
                get { return maxValue; }
                set { maxValue = value; }
            }

            internal DecimalMinMax(decimal min, decimal max)
            {
                minValue = min;
                maxValue = max;
            }

            public override string ToString()
            {
                return string.Format("{0}; {1}", minValue, maxValue);
            }
        }

        #endregion

        #region Objektumváltozók

        private char[] multipliers = { 'y', 'm', 't' };
        private decimal value = 0; // a tárolt érték
        private DecimalFormat format = DecimalFormat.Number; // a formátum
        private sbyte decimalDigits = 0; // tizedesek száma
        private bool focused = false;  // mert az igazi fókusz még a Leave-kor igaz
        private bool blank = true;     // a "semmilyen érték" beállítása
        private string blankText = ""; // a "semmilyen érték" szövege
        private bool blankEnabled = true; // engedett-e a "semmilyen érték" állapot
        private char thousandSeparator = '\0'; // hogy ne kelljen mindig a CultureInfo-t kreálni
        private char decimalSeparator = '\0';
        private char negativeSign = '\0';
        private DecimalRange range = DecimalRange.Any; // megengedett tartomány (megsértése esetén Blank lesz vagy ha az nem engedett, hiba dobódik)
        private DecimalMinMax rangeMinMax = new DecimalMinMax(0, 0); // megengedett minimum / maximum érték (csak MinMax Range esetén)
        private HorizontalAlignment align = HorizontalAlignment.Right;
        private DecimalValueOnBlank valueOnBlank = DecimalValueOnBlank.Zero;
        private bool changeValueOnTextChange;
        private bool textChanging;

        #endregion

        #region DecimalTextBox property-k

        /// <summary>
        /// Gets or sets whether the <see cref="DecimalTextBox"/> is in blank state.
        /// Can be set on ly if <see cref="BlankEnabled"/> is <c>true</c>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether the DecimalTextBox is in blank state. Can be set only if BlankEnabled is true.")]
        [DefaultValue(true)]
        public bool Blank
        {
            get { return blank; }
            set
            {
                bool refresh = false;

                if (blank != value && (blankEnabled || !value))
                {
                    // ha not blank beállítás van, lehet, hogy korrigálni kell a value-t
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
                    OnBlankChanged(new EventArgs());
                    if (blankOld != blank && this.value != BlankValue)
                        OnValueChanged(new EventArgs());

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
            get { return blankText; }
            set
            {
                blankText = value;
                if (blank) RefreshValue();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Blank"/> state can be enabled.
        /// When <c>true</c>, then the <see cref="DecimalTextBox"/> will be automatically blank if <see cref="Value"/> is out of range.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets whether Blank state can be enabled. " +
            "When true, then the DecimalTextBox will be automatically blank if Value is out of range.")]
        [DefaultValue(true)]
        public bool BlankEnabled
        {
            get { return blankEnabled; }
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
            get { return valueOnBlank; }
            set { valueOnBlank = value; }
        }

        /// <summary>
        /// Gets or sets the format of the displayed <see cref="Text"/>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the format of the displayed Text.")]
        [DefaultValue(typeof(DecimalFormat), "Number")]
        public DecimalFormat Format
        {
            get { return format; }
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
            get { return decimalDigits; }
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
                    // mert lehet, hogy tizedes csökkentéskor változik az érték
                    SetValue(this.value, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the the valid range of <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set range, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <c>false</c>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets the the valid range of Value. " +
            "If Value violates newly set range, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(DecimalRange), "Any")]
        public DecimalRange Range
        {
            get { return range; }
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
        /// If <see cref="Value"/> violates newly set minimum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <c>false</c>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets accepted minimum Value. " +
            "If Value violates newly set minimum value, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMinValue
        {
            get
            {
                return rangeMinMax.MinValue;
            }
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
        /// If <see cref="Value"/> violates newly set maximum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <c>false</c>.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Gets or sets accepted maximum Value. " +
            "If Value violates newly set maximum value, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMaxValue
        {
            get
            {
                return rangeMinMax.MaxValue;
            }
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
            get { return !blank ? value : BlankValue; }
            set { SetValue(value, true); }
        }

        /// <summary>
        /// Gets or sets text of the <see cref="DecimalTextBox"/>. Whenever possible use <see cref="Value"/> property instead.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return base.Text; }
            set { SetText(value); }
        }

        /// <summary>
        /// Gets or sets text align.
        /// </summary>
        [Description("Gets or sets text align.")]
        [Category("DecimalTextBox")]
        [DefaultValue(typeof(HorizontalAlignment), "Right")] // csak az alapértelmezés miatt van felülbírálva
        public new HorizontalAlignment TextAlign
        {
            get { return align; }
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
            get { return changeValueOnTextChange; }
            set { changeValueOnTextChange = value; }
        }

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

        #region DecimalTextBox-ban definiált események

        /// <summary>
        /// Occurs when <see cref="Value"/> has been changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when Value has been changed.")]
        public event EventHandler ValueChanged;

        /// <summary>
        /// Occurs when <see cref="Blank"/> has been changed.
        /// </summary>
        [Category("DecimalTextBox")]
        [Description("Occurs when Blank has been changed.")]
        public event EventHandler BlankChanged;

        /// <summary>
        /// Invokes <see cref="ValueChanged"/> event.
        /// </summary>
        protected void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        /// <summary>
        /// Invokes <see cref="BlankChanged"/> event.
        /// </summary>
        protected void OnBlankChanged(EventArgs e)
        {
            if (BlankChanged != null)
                BlankChanged(this, e);
        }

        #endregion

        #region DecimalTextBox Konstruktor és metódusok

        /// <summary>
        /// Creates a new instance of <see cref="DecimalTextBox"/> control.
        /// </summary>
        public DecimalTextBox()
        {
            this.Enter += new EventHandler(DecimalTextBox_Enter);
            this.Leave += new System.EventHandler(this.DecimalTextBox_Leave);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DecimalTextBox_KeyPress);
            this.Validating += new System.ComponentModel.CancelEventHandler(this.DecimalTextBox_Validating);
            this.TextChanged += new EventHandler(DecimalTextBox_TextChanged);
            TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            if (!DesignMode)
            {
                thousandSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
                decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
                negativeSign = Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeSign[0];
            }
        }

        private void AdjustAlignment()
        {
            // Itt csinálhatnánk meg, hogy pl. a Blank állapot mindig balra alignolt legyen, de aztánk kiszedtem. Mindenestre a lehet?ség adott

            // Blank esetén balra igazított a BlankText
            //if (blank)
            //    base.TextAlign = HorizontalAlignment.Left;
            //else
            base.TextAlign = align;
        }

        private decimal RoundTo(decimal value, int order)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, true);

            ci.NumberFormat.NumberDecimalDigits = order <= 0 ? Convert.ToInt32(-order) : 0;
            if (order > 0)
            {
                decimal scale = Convert.ToDecimal(Math.Pow(10, order));
                return decimal.Parse((value / scale).ToString("F", ci)) * scale;
            }
            else
                return decimal.Parse(value.ToString("F", ci));
        }

        /// <summary>
        /// Set value and refreshes text.
        /// </summary>
        /// <param name="value">Value to set</param>
        /// <param name="alert">When true exception will be thrown if BlankEnabled is false and value violates range</param>
        private void SetValue(decimal value, bool alert)
        {
            decimal rounded = RoundTo(value, -decimalDigits); // azért kell, hogy az ábrázolt érték valóban egyezzen az adott tizedesjeggyel való megjelenítettel
            Blank = false;
            if (!CheckRange(rounded, alert)) return;
            if (this.value != rounded)
            {
                this.value = rounded;
                OnValueChanged(new EventArgs());
            }
            else this.value = rounded; // mert bár az 1,0 és az 1,00 egyenlő, tárolásban eltérnek, és ez kell a tervezési idejű frissítéshez            
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
            bool Result = (!strong && (s == "-" || s == ""
                || s[s.Length - 1].ToString().ToLower().IndexOfAny(multipliers) >= 0))
                || decimal.TryParse(s, out d);
            if (Result && strong)
                Result = CheckRange(d, false);
            return Result;
        }

        /// <summary>
        /// Checks range. On violation sets Blank or when it is not enabled fixes Value and if alert is true throws exception.
        /// </summary>
        private bool CheckRange(decimal value, bool alert)
        {
            bool Result;
            decimal scale = decimalDigits < 0 ? Convert.ToDecimal(Math.Pow(10, -decimalDigits)) : 1;

            if (blank) return true;

            switch (range)
            {
                case DecimalRange.Negative: Result = value <= -scale; break;
                case DecimalRange.NegativeNull: Result = value <= 0; break;
                case DecimalRange.Positive: Result = value >= scale; break;
                case DecimalRange.PositiveNull: Result = value >= 0; break;
                case DecimalRange.MinMax: Result = value >= rangeMinMax.MinValue && value <= rangeMinMax.MaxValue; break;
                default: Result = true; break;
            }
            if (Result) return true;
            if (!blankEnabled)
            {
                // ha nem lehet blank-ot állítani, korrigáljuk az értéket,
                // (direkt a property-t állítjuk), és exception-t dobunk

                switch (range)
                {
                    case DecimalRange.Negative: Value = -1 * scale; break;
                    case DecimalRange.Positive: Value = 1 * scale; break;
                    case DecimalRange.MinMax:
                        if (value < rangeMinMax.MinValue)
                            Value = rangeMinMax.MinValue;
                        else Value = rangeMinMax.MaxValue;
                        break;
                    default: Value = 0; break;
                }

                if (alert)
                    throw new Exception("Value \"" + value.ToString() + "\" violates current Range");
            }
            else Blank = true;

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
            OnBlankChanged(new EventArgs());

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
                string text = value.Trim().ToLower();
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

            text = text.ToLower();
            if (text[text.Length - 1].ToString().IndexOfAny(multipliers) >= 0)
            {
                decimal num = 0;
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
                catch
                {
                    // itt lenyeljük, mert ez csak szerkesztéskor van - ilyenkor egyszerűen nincs felszorzás
                    // Text-nek való értékadáskor már dobnánk hibát
                }
                text = num.ToString();
                while (text.IndexOf(decimalSeparator) >= 0 && text[text.Length - 1].In(decimalSeparator, '0'))
                    text = text.Remove(text.Length - 1);
            }

            base.Text = text;
        }

        #endregion

        #region DecimalTextBox-ban lekezelt események

        private void DecimalTextBox_Enter(object sender, EventArgs e)
        {
            if (ReadOnly) return;
            focused = true;
            RefreshValue();
        }

        private void DecimalTextBox_Leave(object sender, EventArgs e)
        {
            if (ReadOnly) return;
            focused = false;
            if (IsValid(Text, true))
                SetText(Text); // Ez beállítja az új értéket. A validálás azért kell, mert a Leave a Validate el?tt is lefut
            else if (blankEnabled && string.IsNullOrEmpty(Text))
                Blank = true;  // ha meg nem üres és úgy érvénytelen, a Validating majd visszarántja a fókuszt
        }

        void DecimalTextBox_TextChanged(object sender, EventArgs e)
        {
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
                {
                    // TODO: Parse by Language/CurrentCulture...
                    SetValue(Decimal.Parse(Text), false);
                }
            }
            finally
            {
                textChanging = false;
            }
        }

        private void DecimalTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (ReadOnly || blank) return;
            if (!IsValid(Text, true))
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Suppressing keys in Blank mode. Further checks are in KeyPress where key can be chacked as char.
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

        private void DecimalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // beírás ellen?rzés
            if (ReadOnly)
                return;

            // érvénytelenítés: ha ezres elválasztó szóköz...
            if (e.KeyChar == thousandSeparator || e.KeyChar == ' '
                // ...vagy mínusz jel nem az els? helyen...
                || (!blank && e.KeyChar == negativeSign && Text.IndexOf(negativeSign) >= 0)
                //  ...vagy az eredmény nem lenne érvényes szám (IsValid enged utolsó karakteren szorzó értéket)
                || (!blank && !char.IsControl(e.KeyChar) &&
                    !IsValid(Text.Substring(0, SelectionStart) + e.KeyChar + Text.Substring(SelectionStart + SelectionLength), false)))
                e.KeyChar = '\0';

            // ha blank állapotban érvényes billt nyomtunk, váltani kell
            else if (blank && !char.IsControl(e.KeyChar))
            {
                if (IsValid(e.KeyChar.ToString(), false))
                {
                    BlankOff();
                }
                else e.KeyChar = '\0';
            }

            // szorzók alkalmazása
            if (e.KeyChar.ToString().ToLower().IndexOfAny(multipliers) >= 0)
            {
                ApplyText(e.KeyChar.ToString(), true);
                SelectionStart = Text.Length;
                e.KeyChar = '\0';
            }
        }

        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            // vágólapról történ? beillesztés ellen?rzése
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

    }
}
