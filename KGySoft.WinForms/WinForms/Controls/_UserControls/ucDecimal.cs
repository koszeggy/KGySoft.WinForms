#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucDecimal.cs
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
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// The unified user control version of <see cref="DecimalTextBox"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucDecimal : ucTextBase
    {
        #region Events

        /// <summary>
        /// Occurs when the <see cref="Value"/> property has changed.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Occurs when Value has been changed.")]
        public event EventHandler ValueChanged
        {
            add => decimalControl.ValueChanged += value;
            remove => decimalControl.ValueChanged -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="Blank"/> property has changed.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Occurs when Blank has been changed.")]
        public event EventHandler BlankChanged
        {
            add => decimalControl.BlankChanged += value;
            remove => decimalControl.BlankChanged -= value;
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the inner <see cref="DecimalTextBox"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new DecimalTextBox TextBox => decimalControl;

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// </summary>
        public override object? ControlValue
        {
            get => Value;
            set
            {
                if (value is decimal d)
                    Value = d;
                else
                    Text = value?.ToString();
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="DecimalTextBox"/> is in blank state.
        /// Can be set only if <see cref="BlankEnabled"/> is <see langword="true"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets whether the DecimalTextBox is in blank state. Can be set only if BlankEnabled is true.")]
        [DefaultValue(true)]
        public bool Blank
        {
            get => decimalControl.Blank;
            set => decimalControl.Blank = value;
        }

        /// <summary>
        /// Gets or sets the caption in <see cref="Blank"/> state.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the caption in Blank state.")]
        [DefaultValue("")]
        public string BlankText
        {
            get => decimalControl.BlankText;
            set => decimalControl.BlankText = value;
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
            get => decimalControl.BlankEnabled;
            set => decimalControl.BlankEnabled = value;
        }

        /// <summary>
        /// Gets or sets what the <see cref="Value"/> property should return in <see cref="Blank"/> state.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets what Value should be returned in Blank state.")]
        [DefaultValue(typeof(DecimalValueOnBlank), "Zero")]
        [RefreshProperties(RefreshProperties.All)]
        public DecimalValueOnBlank ValueOnBlank
        {
            get => decimalControl.ValueOnBlank;
            set => decimalControl.ValueOnBlank = value;
        }

        /// <summary>
        /// Gets or sets the format of the displayed <see cref="Text"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the format of the displayed Text.")]
        [DefaultValue(typeof(DecimalFormat), "Number")]
        public DecimalFormat Format
        {
            get => decimalControl.Format;
            set => decimalControl.Format = value;
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
            get => decimalControl.DecimalDigits;
            set => decimalControl.DecimalDigits = value;
        }

        /// <summary>
        /// Gets or sets the valid range for the <see cref="Value"/> property.
        /// If <see cref="Value"/> violates the newly set range, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the the valid range for the Value property. " +
            "If Value violates the newly set range, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(DecimalRange), "Any")]
        public DecimalRange Range
        {
            get => decimalControl.Range;
            set => decimalControl.Range = value;
        }

        /// <summary>
        /// Gets or sets the accepted minimum <see cref="Value"/>.
        /// If <see cref="Value"/> violates the newly set minimum value, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets accepted minimum Value. " +
            "If Value violates the newly set minimum value, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMinValue
        {
            get => decimalControl.RangeMinValue;
            set => decimalControl.RangeMinValue = value;
        }

        /// <summary>
        /// Gets or sets the accepted maximum <see cref="Value"/>.
        /// If <see cref="Value"/> violates the newly set maximum value, then either <see cref="Blank"/> will be set, or <see cref="Value"/> will be corrected if <see cref="BlankEnabled"/> is <see langword="false"/>.
        /// </summary>
        [Category("ucDecimal")]
        [Description("Gets or sets the accepted maximum Value. " +
            "If Value violates the newly set maximum value, then either Blank will be set, or Value will be corrected if BlankEnabled is false.")]
        [DefaultValue(typeof(decimal), "0")]
        [RefreshProperties(RefreshProperties.All)]
        public decimal RangeMaxValue
        {
            get => decimalControl.RangeMaxValue;
            set => decimalControl.RangeMaxValue = value;
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
            get => decimalControl.Value;
            set => decimalControl.Value = value;
        }

        /// <summary>
        /// Gets or sets the text of the <see cref="DecimalTextBox"/>. It is recommended to use the <see cref="Value"/> property instead.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [AllowNull]
        public override string Text
        {
            get => decimalControl.Text;
            set => decimalControl.Text = value;
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// </summary>
        [Description("Gets or sets text align.")]
        [Category("ucDecimal")]
        [DefaultValue(typeof(HorizontalAlignment), "Right")]
        public HorizontalAlignment TextAlign
        {
            get => decimalControl.TextAlign;
            set => decimalControl.TextAlign = value;
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
            get => decimalControl.ChangeValueOnTextChange;
            set => decimalControl.ChangeValueOnTextChange = value;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the wrapped <see cref="DecimalTextBox"/> control.
        /// </summary>
        protected override Control MainControl => decimalControl;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="ucDecimal"/> control.
        /// </summary>
        public ucDecimal()
        {
            InitializeComponent();
            // todooooo: ResetColor on ValueChanged!!!!
        }

        #endregion
    }
}
