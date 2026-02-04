#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucDate.cs
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
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// The unified user control version of <see cref="AdvancedDateTimePicker"/>.
    /// </summary>
    [DefaultBindingProperty("Value")]
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucDate : ucCaptionedBase
    {
        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime Value
        {
            get => dtpDate.Value;
            set => dtpDate.Value = value;
        }

        /// <summary>
        /// Gets or sets the inner checkbox of the control.
        /// </summary>
        [Category("ucDate")]
        [Description("Gets or sets the inner checkbox of the control.")]
        [DefaultValue(true)]
        public override bool Checked
        {
            get => dtpDate.Checked;
            set => dtpDate.Checked = value;
        }

        /// <summary>
        /// Gets or sets whether the inner checkbox of the control should be shown.
        /// </summary>
        [Category("ucDate")]
        [Description("Gets or sets whether the inner checkbox of the control should be shown.")]
        [DefaultValue(false)]
        public override bool ShowCheckBox
        {
            get => dtpDate.ShowCheckBox;
            set => dtpDate.ShowCheckBox = value;
        }

        /// <summary>
        /// Gets or sets of the format of the time and date displayed in the control.
        /// </summary>
        [Category("ucDate")]
        [Description("Gets or sets of the format of the time and date displayed in the control.")]
        [DefaultValue(typeof(DateTimePickerFormat), "Short")]
        public DateTimePickerFormat Format
        {
            get => dtpDate.Format;
            set => dtpDate.Format = value;
        }

        /// <summary>
        /// Gets or sets the custom date/time format string.
        /// </summary>
        [Category("ucDate")]
        [Description("Gets or sets the custom date/time format string.")]
        [DefaultValue("")]
        public string? CustomFormat
        {
            get => dtpDate.CustomFormat;
            set => dtpDate.CustomFormat = value;
        }

        /// <summary>
        /// Gets the inner date time picker control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AdvancedDateTimePicker DateTimePicker => dtpDate;

        /// <summary>
        /// Gets or sets whether the control is read-only.
        /// </summary>
        public override bool ReadOnly
        {
            get => !dtpDate.Enabled;
            set
            {
                dtpDate.Enabled = !value;
                base.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// </summary>
        public override object? ControlValue
        {
            get => Value;
            set => Value = Convert.ToDateTime(value, CultureInfo.CurrentCulture);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the wrapped <see cref="AdvancedDateTimePicker"/> control.
        /// </summary>
        protected override Control MainControl => dtpDate;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="ucDate"/> instance.
        /// </summary>
        public ucDate()
        {
            InitializeComponent();
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.EnabledChanged += dtpDate_EnabledChanged;
            dtpDate.ValueChanged += dtpDate_ValueChanged;
            dtpDate.Value = DateTime.Now;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Resets the date picker to its default state, and unchecks it if <see cref="ShowCheckBox"/> is <see langword="true"/>.
        /// </summary>
        public override void Clear()
        {
            if (dtpDate.ShowCheckBox)
                dtpDate.Checked = false;

            dtpDate.Value = DateTime.Now;
            ResetColor();
        }

        #endregion

        #region Event handlers

        private void dtpDate_EnabledChanged(object? sender, EventArgs e)
        {
            ResetColor();
        }

        private void dtpDate_ValueChanged(object? sender, EventArgs e)
        {
            ResetColor();
        }

        #endregion

        #endregion
    }
}
