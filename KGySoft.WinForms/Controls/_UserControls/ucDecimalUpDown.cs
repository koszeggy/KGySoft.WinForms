#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucDecimalUpDown.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Unified user control version of <see cref="NumericUpDown"/>.
    /// </summary>
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucDecimalUpDown : ucCaptionedBase
    {
        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the inner control.
        /// </summary>
        [
            Category("ucDecimalUpDown"),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Description("Belso textbox")
        ]
        public NumericUpDown NumericUpDown
        {
            get { return nudValue; }
        }

        /// <summary>
        /// Gets or sets the value assigned to the up-down control.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public decimal Value
        {
            get
            {
                return nudValue.Value;
            }
            set
            {
                nudValue.Value = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed value for the up-down control.
        /// </summary>
        public decimal Minimum
        {
            get
            {
                return nudValue.Minimum;
            }
            set
            {
                nudValue.Minimum = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed value for the up-down control.
        /// </summary>
        public decimal Maximum
        {
            get
            {
                return nudValue.Maximum;
            }
            set
            {
                nudValue.Maximum = value;
            }
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// </summary>
        public override bool ReadOnly
        {
            get
            {
                return nudValue.ReadOnly;
            }
            set
            {
                nudValue.ReadOnly = value;
                base.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// </summary>
        public override object ControlValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = Convert.ToDecimal(value);
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Returns the main inner control of the user control.
        /// </summary>
        protected override Control MainControl
        {
            get
            {
                return nudValue;
            }
        }

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
