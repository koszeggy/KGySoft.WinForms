#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCheckBox.cs
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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{    
    [DefaultBindingProperty("CheckedContent")]
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucCheckBox: ucCaptionedBase
	{
		#region Contructor, Dispose

		public ucCheckBox()
        {
            InitializeComponent();
            this.cbCheck.EnabledChanged += new System.EventHandler(this.cbCheck_EnabledChanged);
            this.cbCheck.CheckedChanged += new System.EventHandler(this.cbCheck_CheckedChanged);
		}

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            cbCheck.EnabledChanged -= cbCheck_EnabledChanged;
            cbCheck.CheckedChanged -= cbCheck_CheckedChanged;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#endregion

		#region Properties

		protected override Control MainControl
        {
            get { return cbCheck; }
        }

        /// <summary>
        /// Gets or sets the caption of the checkbox.
        /// </summary>
		[Description("Gets or sets the caption of the checkbox.")]
		[Category("ucCheckBox")]
		public string CaptionCheckbox
        {
            get { return cbCheck.Text; }
            set { cbCheck.Text = value; }

        }

        /// <summary>
		/// Gets or sets the alignment of the checkbox.
        /// </summary>
		[Description("Gets or sets the alignment of the checkbox.")]
		[Category("ucCheckBox")]
		public ContentAlignment CheckAlign
        {
            get { return cbCheck.CheckAlign; }
            set { cbCheck.CheckAlign = value; }
        }


        /// <summary>
		/// Gets or sets the Checked state of the inner checkbox.
        /// </summary>
        [Category("ucCheckBox")]
        [DefaultValue(false)]
		[Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Gets or sets the Checked state of the inner checkbox.")]
		public bool CheckedContent
        {
            get { return cbCheck.Checked; }
            set
            {
                cbCheck.Checked = value;
                ResetColor();
            }
        }

		/// <summary>
		/// Gets or sets the ReadOnly state of the inner content.
		/// </summary>
        [Category("ucCheckBox")]
		[Description("Gets or sets the ReadOnly state of the inner content.")]
        public override bool ReadOnly
        {
            get { return !cbCheck.Enabled; }
            set
            {
                cbCheck.Enabled = !value;
                base.ReadOnly = value;
            }
        }

		public override object ControlValue
		{
			get
			{
				return CheckedContent;
			}
			set
			{
				CheckedContent = Convert.ToBoolean(value, CultureInfo.CurrentCulture);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the state of the inner check box changes.
		/// </summary>
		[Category("ucCheckBox")]
		[Description("Occurs when the state of the inner check box changes.")]
		public event EventHandler CheckedContentChanged
		{
			add { cbCheck.CheckedChanged += value; }
			remove { cbCheck.CheckedChanged -= value; }
		}

		#endregion

		#region Overridden methods

		public override void Clear()
		{
			cbCheck.Checked = false;
		}

		#endregion

		#region Private implementation

		private void cbCheck_EnabledChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        private void cbCheck_CheckedChanged(object sender, EventArgs e)
        {
            ResetColor();
		}

		#endregion
	}
}
