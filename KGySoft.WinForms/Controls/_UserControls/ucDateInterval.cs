using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
{
    [DefaultBindingProperty("ValueFrom")]
    [ToolboxItem(true)]
    public partial class ucDateInterval: ucCaptionedBase
    {       
        public ucDateInterval()
        {
            InitializeComponent();

			SetToolTip(this.dtpDateFrom, "From");
			SetToolTip(this.dtpDateTo, "To");

            dtpDateFrom.Value = DateTime.Now;
            dtpDateTo.Value = DateTime.Now;

            dtpDateFrom.ValueChanged += new EventHandler(dtpDateFrom_ValueChanged);
            dtpDateTo.ValueChanged += new EventHandler(dtpDateTo_ValueChanged);

            List<object> dates = new List<object>();
            dates.Add(dtpDateFrom.Value);
            dates.Add(dtpDateTo.Value);


        }

        protected override Control MainControl
        {
            get { return flowLayoutPanel1; }
        }

        void dtpDateTo_ValueChanged(object sender, EventArgs e)
        {
            dtpDateTo.Checked = dtpDateTo.Checked;
        }

        void dtpDateFrom_ValueChanged(object sender, EventArgs e)
        {
            dtpDateFrom.Checked = dtpDateFrom.Checked;
        }

        private bool m_HasHourFilter = true;

        [
        Category("ucDateInterval"),
        DefaultValue(true)
        ]

        public bool HasHourFilter
        {
            get { return m_HasHourFilter; }
            set 
            { 
                m_HasHourFilter = value;
                upHourFrom.Visible = value;
                upHourTo.Visible = value;
                lblHour1.Visible = value;
                lblHour2.Visible = value;
            }
        }

        bool hasHyphen = true;
        [
        Category("ucDateInterval"),
        Description("Gets or sets whether hyphen is visible."),
        DefaultValue(true)
        ]
        public bool HasHyphen
        {
            get { return hasHyphen; }
            set { hasHyphen = value; lblMinus.Visible = value; }
        }

        /// <summary>
        /// From Date
        /// </summary>
        [
        Category("ucDateInterval"),
        Bindable(BindableSupport.Default, BindingDirection.TwoWay)
        ]
        public DateTime ValueFrom
        {
            get
            {
                return m_HasHourFilter ? dtpDateFrom.Value.Date.AddHours((double)upHourFrom.Value) : dtpDateFrom.Value.Date;                    
            }
            set
            {
                dtpDateFrom.Value = value;
            }
        }

        /// <summary>
        /// To Date
        /// </summary>
        [
        Category("ucDateInterval"),
        Bindable(BindableSupport.Yes, BindingDirection.TwoWay)
        ]
        public DateTime ValueTo
        {
            get
            {
                return m_HasHourFilter ? dtpDateTo.Value.Date.AddHours((double)upHourTo.Value) : dtpDateTo.Value.Date;
            }
            set
            {
                dtpDateTo.Value = value;
            }
        }

        [Category("ucDateInterval")]
        public decimal HourFrom
        {
            get 
            {
                return upHourFrom.Value;
            }
            set 
            {
                upHourFrom.Value = value;
            }
        }

        [Category("ucDateInterval")]
        public decimal HourTo
        {
            get
            {
                return upHourTo.Value;
            }
            set
            {
                upHourTo.Value = value;
            }
        }

        /// <summary>
        /// From Date Checked
        /// </summary>
        [Category("ucDateInterval")]
        public bool CheckedFrom
        {
            get
            {
                return dtpDateFrom.Checked;
            }
            set
            {
                dtpDateFrom.Checked = value;
            }
        }

        /// <summary>
        /// To Date Checked
        /// </summary>
        [Category("ucDateInterval")]
        public bool CheckedTo
        {
            get
            {
                return dtpDateTo.Checked;
            }
            set
            {
                dtpDateTo.Checked = value;
            }
        }
        
        public override bool ReadOnly
        {
            get
            {
                return !dtpDateFrom.Enabled;
            }
            set
            {
                dtpDateFrom.Enabled = !value;
                dtpDateTo.Enabled = !value;
            }
        }
        public override void Clear()
        {
            base.Clear();
            dtpDateTo.Checked = false;
            dtpDateFrom.Checked = false;
        }
    }
}
