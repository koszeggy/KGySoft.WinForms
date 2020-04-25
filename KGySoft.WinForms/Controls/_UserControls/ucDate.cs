using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
{
    // TODO: ha a színek nem látszanak/m?ködnek, az ?ssel konform módon megoldani

    /// <summary>
    /// Unified user control version of <see cref="AdvancedDateTimePicker"/>.
    /// </summary>
    [DefaultBindingProperty("Value")]
    [ToolboxItem(true)]    
    public partial class ucDate: ucCaptionedBase
    {
        /// <summary>
        /// Creates a new <see cref="ucDate"/> instance.
        /// </summary>
        public ucDate()
        {
            InitializeComponent();
            this.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDate.EnabledChanged += new System.EventHandler(this.dtpDate_EnabledChanged);
            this.dtpDate.ValueChanged += new System.EventHandler(this.dtpDate_ValueChanged);
            dtpDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ne mentse a mai dátumot mindig a designerbe
        [Browsable(false)]
        public DateTime Value
        {
            get
            {
                return dtpDate.Value;
            }
            set
            {
                dtpDate.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner checkbox of the control.
        /// </summary>
        [
            Category("ucDate"),
            Description("Gets or sets the inner checkbox of the control."),
            DefaultValue(true)
        ]
        public override bool Checked
        {
            get { return dtpDate.Checked; }
            set { dtpDate.Checked = value; }
        }

        /// <summary>
        /// Gets or sets whether the inner checkbox of the control should be shown.
        /// </summary>
        [
            Category("ucDate"),
            Description("Gets or sets whether the inner checkbox of the control should be shown."),
            DefaultValue(false)
        ]
        public override bool ShowCheckBox
        {
            get { return dtpDate.ShowCheckBox; }
            set { dtpDate.ShowCheckBox = value; }
        }

        /// <summary>
        /// Gets or sets of the format of the time and date displayed in the control.
        /// </summary>
        [
            Category("ucDate"),
            Description("Gets or sets of the format of the time and date displayed in the control."),
            DefaultValue(typeof(DateTimePickerFormat), "Short")
        ]
        public DateTimePickerFormat Format
        {
            get { return dtpDate.Format; }
            set { dtpDate.Format = value; }
        }

        /// <summary>
        /// Gets or sets the custom date/time format string.
        /// </summary>
        [
            Category("ucDate"),
            Description("Gets or sets the custom date/time format string."),
            DefaultValue("")
        ]
        public string CustomFormat
        {
            get { return dtpDate.CustomFormat; }
            set { dtpDate.CustomFormat = value; }
        }

        /// <summary>
        /// Gets the inner date time picker control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AdvancedDateTimePicker DateTimePicker
        {
            get { return dtpDate; }
        }

        public override void Clear()
        {
            if (dtpDate.ShowCheckBox)
                dtpDate.Checked = false;

            dtpDate.Value = System.DateTime.Now;
            ResetColor();
        }

        /// <summary>
        /// Gets or sets whether the control is read-only.
        /// </summary>
        public override bool ReadOnly
        {
            get
            {
                return !dtpDate.Enabled;
            }
            set
            {
                dtpDate.Enabled = !value;
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
                Value = Convert.ToDateTime(value);
            }
        }

        /// <summary>
        /// Returns the main inner control of the user control.
        /// </summary>
        protected override Control MainControl
        {
            get { return dtpDate; }
        }

        private void dtpDate_EnabledChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            ResetColor();
        }
    }
}
