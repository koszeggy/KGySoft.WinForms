using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    // todoooooooo: belső advanced control, mint ucDecimal. ucTextBase-ből származzon
    /// <summary>
    /// Unified user control version of <see cref="NumericUpDown"/>.
    /// </summary>
    [ToolboxItem(true)]
    public partial class ucDecimalUpDown: ucCaptionedBase
    {
        /// <summary>
        /// Creates a new <see cref="ucDecimalUpDown"/> instance.
        /// </summary>
        public ucDecimalUpDown()
        {
            InitializeComponent();
        }
 
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
        /// Clears the content of the inner control.
        /// </summary>
        public override void Clear()
        {
            nudValue.Text = String.Empty;
        }

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
    }
}
