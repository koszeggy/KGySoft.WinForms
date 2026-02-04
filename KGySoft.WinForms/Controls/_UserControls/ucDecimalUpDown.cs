#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucDecimalUpDown.cs
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
    /// The unified user control version of <see cref="System.Windows.Forms.NumericUpDown"/>.
    /// </summary>
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucDecimalUpDown : ucCaptionedBase
    {
        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the inner control.
        /// </summary>
        [Category("ucDecimalUpDown")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Belso textbox")]
        public NumericUpDown NumericUpDown => nudValue;

        /// <summary>
        /// Gets or sets the value assigned to the up-down control.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DefaultValue(typeof(decimal), "0")]
        public decimal Value
        {
            get => nudValue.Value;
            set
            {
                nudValue.Value = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed value for the up-down control.
        /// </summary>
        [DefaultValue(typeof(decimal), "0")]
        public decimal Minimum
        {
            get => nudValue.Minimum;
            set => nudValue.Minimum = value;
        }

        /// <summary>
        /// Gets or sets the maximum allowed value for the up-down control.
        /// </summary>
        [DefaultValue(typeof(decimal), "100")]
        public decimal Maximum
        {
            get => nudValue.Maximum;
            set => nudValue.Maximum = value;
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// </summary>
        public override bool ReadOnly
        {
            get => nudValue.ReadOnly;
            set
            {
                nudValue.ReadOnly = value;
                base.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// </summary>
        public override object? ControlValue
        {
            get => Value;
            set => Value = Convert.ToDecimal(value, CultureInfo.CurrentCulture);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the wrapped <see cref="NumericUpDown"/> control.
        /// </summary>
        protected override Control MainControl => nudValue;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="ucDecimalUpDown"/> instance.
        /// </summary>
        public ucDecimalUpDown()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the content of the inner control.
        /// </summary>
        public override void Clear()
        {
            nudValue.Text = String.Empty;
        }

        #endregion
    }
}
