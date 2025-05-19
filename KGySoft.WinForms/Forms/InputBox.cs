#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: InputBox.cs
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

using System.Drawing;
using System.Windows.Forms;

using KGySoft.Libraries.Language;

#endregion

namespace KGySoft.WinForms.Forms
{
    public partial class InputBox : Form
    {
        #region Constructors

        public InputBox()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                btnOK.Text = Language.Translate("OK");
                btnCancel.Text = Language.Translate("Cancel");
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        public static bool Show(string caption, string prompt, ref string value, int x, int y)
        {
            using (InputBox inputBox = new InputBox())
            {
                inputBox.Text = caption;
                inputBox.lblPrompt.Text = prompt;
                inputBox.edtValue.Text = value;
                if (x >= 0 && y >= 0)
                {
                    inputBox.StartPosition = FormStartPosition.Manual;
                    inputBox.Location = new Point(x, y);
                }
                if (inputBox.ShowDialog() == DialogResult.OK)
                {
                    value = inputBox.edtValue.Text;
                    return true;
                }
                return false;
            }
        }

        public static bool Show(string caption, string prompt, ref string value)
        {
            return Show(caption, prompt, ref value, -1, -1);
        }

        #endregion

        #region Instance Methods

        private void edtValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    DialogResult = DialogResult.OK;
                    e.Handled = true;
                    break;
                case (char)Keys.Escape:
                    DialogResult = DialogResult.Cancel;
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #endregion
    }
}
