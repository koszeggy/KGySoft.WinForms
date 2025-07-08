#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: frmAdvancedComboBox.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmAdvancedComboBox : ControlsTestBaseForm
    {
        #region Constructors

        public frmAdvancedComboBox()
        {
            InitializeComponent();

            var items = new[]
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5",
            };

            comboBox1.DataSource = items;
            comboBox2.DataSource = items;
            comboBox3.DataSource = items;
            advancedComboBox1.DataSource = items;
            advancedComboBox2.DataSource = items;
            advancedComboBox3.DataSource = items;
            advancedComboBox4.DataSource = items;
            advancedComboBox5.DataSource = items;
            advancedComboBox6.DataSource = items;
            advancedComboBox7.DataSource = items;
            advancedComboBox8.DataSource = items;
        }

        #endregion
    }
}