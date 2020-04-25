using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Unified user control version of <see cref="DecimalTextBox"/>.
    /// </summary>
    [ToolboxItem(true)]
    public partial class ucDecimal : ucTextBase
    {

        #region Kötelezően override-olt property-k

        protected override Control MainControl
        {
            get { return decimalControl; }
        }

        /// <summary>
        /// Gets the inner <see cref="DecimalTextBox"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ne állítsuk át, inkább vezessük ki, amit szükségessé válik
        [Browsable(false)]
        public new DecimalTextBox TextBox
        {
            get { return decimalControl; }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// This can be a text or number or anything else in derived controls.
        /// </summary>
        public override object ControlValue
        {
            get { return Value; }
            set
            {
                if (value is decimal)
                    Value = (decimal)value;
                else
                    Text = (value == null ? null : value.ToString());
            }
        }

        #endregion

        #region ucDecimal property-k

        /// <summary>
        /// Gets or sets whether the <see cref="DecimalTextBox"/> is in blank state.
        /// Can be set on ly if <see cref="BlankEnabled"/> is <see langword="true"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets whether the DecimalTextBox is in blank state. Can be set only if BlankEnabled is true.")]
        [DefaultValue(true)]
        public bool Blank
        {
            get { return decimalControl.Blank; }
            set { decimalControl.Blank = value; }
        }

        /// <summary>
        /// Gets or sets the caption in <see cref="Blank"/> state.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the caption in Blank state.")]
        [DefaultValue("")]
        public string BlankText
        {
            get { return decimalControl.BlankText; }
            set { decimalControl.BlankText = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Blank"/> state can be enabled.
        /// When <see langword="true"/>, then the <see cref="DecimalTextBox"/> will be automatically blank if <see cref="Value"/> is out of range.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets whether Blank state can be enabled. " +
            "When true, then the DecimalTextBox will be automatically blank if Value is out of range.")]
        [DefaultValue(true)]
        public bool BlankEnabled
        {
            get { return decimalControl.BlankEnabled; }
            set { decimalControl.BlankEnabled = value; }
        }

        /// <summary>
        /// Gets or sets what <see cref="Value"/> should be returned in <see cref="Blank"/> state.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets what Value should be returned in Blank state.")]
        [DefaultValue(typeof(DecimalValueOnBlank), "Zero")]
        [RefreshProperties(RefreshProperties.All)]
        public DecimalValueOnBlank ValueOnBlank
        {
            get { return decimalControl.ValueOnBlank; }
            set { decimalControl.ValueOnBlank = value; }
        }

        /// <summary>
        /// Gets or sets the format of the displayed <see cref="Text"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the format of the displayed Text.")]
        [DefaultValue(typeof(DecimalFormat), "Number")]
        public DecimalFormat Format
        {
            get { return decimalControl.Format; }
            set { decimalControl.Format = value; }
        }

        /// <summary>
        /// Gets or sets the used fraction digits. When negative, then <see cref="Value"/> is rounded to the number of specified digits.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the used fraction digits. When negative, then Value is rounded to the number of specified digits.")]
        [DefaultValue(typeof(sbyte), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public sbyte DecimalDigits
        {
            get { return decimalControl.DecimalDigits; }
            set { decimalControl.DecimalDigits = value; }
        }

        /// <summary>
        /// Gets or sets the the valid range of <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set range, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the the valid range of Value. " +
            "If Value violates newly set range, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(DecimalRange), "Any")]
        public DecimalRange Range
        {
            get { return decimalControl.Range; }
            set { decimalControl.Range = value; }
        }

        /// <summary>
        /// Gets or sets accepted minimum <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set minimum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets accepted minimum Value. " +
            "If Value violates newly set minimum value, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMinValue
        {
            get { return decimalControl.RangeMinValue; }
            set { decimalControl.RangeMinValue = value; }
        }

        /// <summary>
        /// Gets or sets accepted maximum <see cref="Value"/>.
        /// If <see cref="Value"/> violates newly set maximum value, then <see cref="Blank"/> will be set or <see cref="Value"/> will be corrigied if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets accepted maximum Value. " +
            "If Value violates newly set maximum value, then Blank will be set or Value will be corrigied if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMaxValue
        {
            get { return decimalControl.RangeMaxValue; }
            set { decimalControl.RangeMaxValue = value; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="DecimalTextBox"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the value of the DecimalTextBox.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public decimal Value
        {
            get { return decimalControl.Value; }
            set { decimalControl.Value = value; }
        }

        /// <summary>
        /// Gets or sets text of the <see cref="DecimalTextBox"/>. Whenever possible use <see cref="Value"/> property instead.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return decimalControl.Text; }
            set { decimalControl.Text = value; }
        }

        /// <summary>
        /// Gets or sets text align.
        /// </summary>
        [Description("Gets or sets text align.")]
        [Category("ucDecimal")]
        [DefaultValue(typeof(HorizontalAlignment), "Right")]
        public HorizontalAlignment TextAlign
        {
            get { return decimalControl.TextAlign; }
            set { decimalControl.TextAlign = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Value"/> should be changed for every keystroke when text is edited.
        /// By default, Value changes only when the control is left.
        /// </summary>
        [Description("Gets or sets whether Value should be changed for every keystroke when text is edited. By default, Value changes only when the control is left.")]
        [Category("ucDecimal")]
        [DefaultValue(false)]
        public bool ChangeValueOnTextChange
        {
            get { return decimalControl.ChangeValueOnTextChange; }
            set { decimalControl.ChangeValueOnTextChange = value; }
        }

        #endregion

        #region ucDecimal-ban definiált események

        /// <summary>
        /// Occurs when <see cref="Value"/> has been changed.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Occurs when Value has been changed.")]
        public event EventHandler ValueChanged
        {
            add { decimalControl.ValueChanged += value; }
            remove { decimalControl.ValueChanged -= value; }
        }

        /// <summary>
        /// Occurs when <see cref="Blank"/> has been changed.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Occurs when Blank has been changed.")]
        public event EventHandler BlankChanged
        {
            add { decimalControl.BlankChanged += value; }
            remove { decimalControl.BlankChanged -= value; }
        }

        #endregion

        #region ucDecimal Konstruktor és metódusok

        /// <summary>
        /// Creates a new instance of <see cref="ucDecimal"/> control.
        /// </summary>
        public ucDecimal()
        {
            InitializeComponent();
            // todooooo: ValueChanged-re ResetColor!!!!
        }

        #endregion
    }
}
