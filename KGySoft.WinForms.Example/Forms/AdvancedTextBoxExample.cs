#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedTextBoxExample.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class AdvancedTextBoxExample : ControlsTestBaseForm
    {
        #region Constructors

        public AdvancedTextBoxExample()
        {
            InitializeComponent();

            var bindingDecimalToText = new Binding(nameof(txtValue.Text), decimalTextBox1, nameof(decimalTextBox1.Value), true);
            bindingDecimalToText.Parse += (sender, e) =>
            {
                // OR: just Parse, letting the FormatException be thrown so the TextBox doesn't let the focus leave
                if (Decimal.TryParse((string)e.Value!, out decimal value))
                    e.Value = value;
                else
                    ((DecimalTextBox)((Binding)sender!).DataSource!).Blank = true;
            };

            txtValue.DataBindings.Add(bindingDecimalToText);
            lblValue.DataBindings.Add(nameof(lblValue.Text), decimalTextBox1, nameof(decimalTextBox1.Value));
        }

        #endregion
    }
}