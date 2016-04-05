using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    public partial class ucEmail : ucText
    {
        public ucEmail()
        {
            InitializeComponent();
        }

        private void buttonEmail_Click(object sender, EventArgs e)
        {
            try 
            {
                System.Diagnostics.Process.Start("mailto:" + textControl.Text);
            }
            catch
            {
                MessageBox.Show("Initiating e-mailing failed!", "E-mail", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }           
        }
    }
}
