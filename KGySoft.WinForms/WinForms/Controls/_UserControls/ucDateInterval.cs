#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucDateInterval.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a user control for selecting a date interval, optionally including hour selection, with support for
    /// specifying start and end dates and times.
    /// </summary>
    [DefaultBindingProperty("ValueFrom")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucDateInterval : ucCaptionedBase
    {
        #region Fields

        private bool hasHourFilter = true;
        bool hasHyphen = true;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the hour filter controls are visible.
        /// </summary>
        /// <remarks>Set this property to <see langword="true"/> to allow users to filter by hour range.
        /// When set to <see langword="false"/>, the hour filter controls are hidden and disabled.</remarks>
        [Category("ucDateInterval")]
        [DefaultValue(true)]
        public bool HasHourFilter
        {
            get => hasHourFilter;
            set
            {
                hasHourFilter = value;
                upHourFrom.Visible = value;
                upHourTo.Visible = value;
                lblHour1.Visible = value;
                lblHour2.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets whether a hyphen is displayed in the control between the start and end values.
        /// </summary>
        [Category("ucDateInterval")]
        [Description("Gets or sets whether hyphen is visible.")]
        [DefaultValue(true)]
        public bool HasHyphen
        {
            get => hasHyphen;
            set
            {
                hasHyphen = value;
                lblHyphen.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the start date-time
        /// </summary>
        [Category("ucDateInterval")]
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public DateTime ValueFrom
        {
            get => hasHourFilter ? dtpDateFrom.Value.Date.AddHours((double)upHourFrom.Value) : dtpDateFrom.Value.Date;
            set => dtpDateFrom.Value = value;
        }

        /// <summary>
        /// Gets or sets the end date-time
        /// </summary>
        [Category("ucDateInterval")]
        [Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public DateTime ValueTo
        {
            get => hasHourFilter ? dtpDateTo.Value.Date.AddHours((double)upHourTo.Value) : dtpDateTo.Value.Date;
            set => dtpDateTo.Value = value;
        }

        /// <summary>
        /// Gets or sets the start hours
        /// </summary>
        [Category("ucDateInterval")]
        public decimal HourFrom
        {
            get => upHourFrom.Value;
            set => upHourFrom.Value = value;
        }

        /// <summary>
        /// Gets or sets the end hours
        /// </summary>
        [Category("ucDateInterval")]
        public decimal HourTo
        {
            get => upHourTo.Value;
            set => upHourTo.Value = value;
        }

        /// <summary>
        /// Gets or sets the checkbox for the start date.
        /// </summary>
        [Category("ucDateInterval")]
        [DefaultValue(true)]
        public bool CheckedFrom
        {
            get => dtpDateFrom.Checked;
            set => dtpDateFrom.Checked = value;
        }

        /// <summary>
        /// Gets or sets the checkbox for the end date.
        /// </summary>
        [Category("ucDateInterval")]
        [DefaultValue(true)]
        public bool CheckedTo
        {
            get => dtpDateTo.Checked;
            set => dtpDateTo.Checked = value;
        }

        /// <summary>
        /// In this control sets the enabled state of the inner controls
        /// </summary>
        public override bool ReadOnly
        {
            get => !dtpDateFrom.Enabled;
            set
            {
                dtpDateFrom.Enabled = !value;
                dtpDateTo.Enabled = !value;
                upHourFrom.Enabled = !value;
                upHourTo.Enabled = !value;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Returns the panel that contains the inner controls.
        /// </summary>
        protected override Control MainControl => flowLayoutPanel1;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucDateInterval"/> class.
        /// </summary>
        public ucDateInterval()
        {
            InitializeComponent();

            SetToolTip(dtpDateFrom, "From");
            SetToolTip(dtpDateTo, "To");

            dtpDateFrom.Value = DateTime.Now;
            dtpDateTo.Value = DateTime.Now;

            dtpDateFrom.ValueChanged += dtpDateFrom_ValueChanged;
            dtpDateTo.ValueChanged += dtpDateTo_ValueChanged;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Clears the checkboxes of the date pickers.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            dtpDateTo.Checked = false;
            dtpDateFrom.Checked = false;
        }

        #endregion

        #region Private Methods

        private bool ShouldSerializeHourFrom() => hasHourFilter;
        private bool ShouldSerializeHourTo() => hasHourFilter;

        #endregion

        #region Event handlers

        void dtpDateTo_ValueChanged(object? sender, EventArgs e)
        {
            dtpDateTo.Checked = dtpDateTo.Checked;
        }

        void dtpDateFrom_ValueChanged(object? sender, EventArgs e)
        {
            dtpDateFrom.Checked = dtpDateFrom.Checked;
        }

        #endregion

        #endregion
    }
}
