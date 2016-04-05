using System;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    /// <summary>
    /// Base form for OK/Cancel dialogs.
    /// </summary>
	public partial class DialogBaseForm: BaseForm
	{
        /// <summary>
        /// Creates a new instance of <see cref="DialogBaseForm"/>.
        /// </summary>
		public DialogBaseForm()
		{
			InitializeComponent();
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            btnOK.Click -= btnOK_Click;
            btnCancel.Click -= btnCancel_Click;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		/// <summary>
		/// Executes the dialog window.
		/// </summary>
		/// <returns>Returns true, when the OK button was pressed, otherwise, false.</returns>
		public virtual bool Execute()
		{
			return ShowDialog() == DialogResult.OK;
		}

		/// <summary>
		/// Override this method when anything needs to be performed when the OK button is pressed.
		/// Call base method to close the window with positive result.
		/// </summary>
		protected virtual void OKPressed()
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Override this method when anything needs to be performed when the Cancel button is pressed.
		/// Call base method to close the window with negative result.
		/// </summary>
		protected virtual void CancelPressed()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			OKPressed();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			CancelPressed();
		}
	}
}