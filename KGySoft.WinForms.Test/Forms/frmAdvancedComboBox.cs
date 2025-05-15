namespace KGySoft.WinForms.Test.Forms
{
    internal partial class frmAdvancedComboBox : ControlsTestBaseForm
    {
        public frmAdvancedComboBox()
        {
            // NOTE: the designer in VS2022 keeps changing the Simple ComboBox heights to 150. Just reset it to 21 when this happens.
            InitializeComponent();
            lblInstuction.SendToBack();

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
    }
}
