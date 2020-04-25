using System;
using System.Windows.Forms;

namespace KGySoft.WinForms.Test.Forms
{
    internal partial class ControlsTestBaseForm : Form
    {
        public ControlsTestBaseForm()
        {
            InitializeComponent();
        }

        private void ControlsTestBaseForm_Load(object sender, EventArgs e)
        {
            Subscribe(pnlTestArea, true);
        }

        private void Subscribe(Control parentControl, bool add)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (add)
                    control.Click += new EventHandler(control_Click);
                else
                    control.Click -= control_Click;

                if (control.HasChildren)
                    Subscribe(control, add);
            }
        }

        void control_Click(object sender, EventArgs e)
        {
            grdProperties.SelectedObject = sender;
        }

        private void ControlsTestBaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Subscribe(pnlTestArea, false);
        }
    }
}
