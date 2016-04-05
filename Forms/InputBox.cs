extern alias lang;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Language = lang::KGySoft.Libraries.Language.Language;

namespace KGySoft.Controls
{
	public partial class InputBox: Form
	{
		public InputBox()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				btnOK.Text = Language.Translate("OK");
				btnCancel.Text = Language.Translate("Cancel");
			}
		}

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

		private void edtValue_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
				DialogResult = DialogResult.OK;
			else if (e.KeyChar == (char)Keys.Escape)
				DialogResult = DialogResult.Cancel;
		}

	}
}