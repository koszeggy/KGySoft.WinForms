using System;
using System.Windows.Forms;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmAdvancedTextBox : ControlsTestBaseForm
    {
        public frmAdvancedTextBox()
        {
            InitializeComponent();

            var bindingDecimalToText = new Binding(nameof(txtValue.Text), decimalTextBox1, nameof(decimalTextBox1.Value), true);
            bindingDecimalToText.Parse += (sender, e) =>
            {
                // OR: just Parse, letting the FormatException be thrown so the TextBox doesn't let the focus leave
                if (Decimal.TryParse((string)e.Value, out decimal value))
                    e.Value = value;
                else
                    ((DecimalTextBox)((Binding)sender).DataSource).Blank = true;
            };

            txtValue.DataBindings.Add(bindingDecimalToText);
            lblValue.DataBindings.Add(nameof(lblValue.Text), decimalTextBox1, nameof(decimalTextBox1.Value));
        }
    }
}
