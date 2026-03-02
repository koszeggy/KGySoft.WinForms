namespace KGySoft.WinForms.Example.Controls
{
    partial class CustomPropertySetter
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbProperty = new KGySoft.WinForms.Controls.AdvancedComboBox();
            this.txtValue = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClear = new KGySoft.WinForms.Controls.AdvancedButton();
            this.btnSet = new KGySoft.WinForms.Controls.AdvancedButton();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbProperty
            // 
            this.cmbProperty.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbProperty.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbProperty.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbProperty.FormattingEnabled = true;
            this.cmbProperty.Location = new System.Drawing.Point(0, 0);
            this.cmbProperty.Name = "cmbProperty";
            this.cmbProperty.Size = new System.Drawing.Size(154, 21);
            this.cmbProperty.Sorted = true;
            this.cmbProperty.TabIndex = 0;
            // 
            // txtValue
            // 
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.Location = new System.Drawing.Point(0, 21);
            this.txtValue.Multiline = true;
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(154, 30);
            this.txtValue.TabIndex = 1;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnSet);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlButtons.Enabled = false;
            this.pnlButtons.Location = new System.Drawing.Point(154, 0);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(20, 51);
            this.pnlButtons.TabIndex = 2;
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnClear.EnabledForeColor = System.Drawing.Color.Maroon;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClear.Location = new System.Drawing.Point(0, 20);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(20, 20);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "✖";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // btnSet
            // 
            this.btnSet.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSet.EnabledForeColor = System.Drawing.Color.Green;
            this.btnSet.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSet.Location = new System.Drawing.Point(0, 0);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(20, 20);
            this.btnSet.TabIndex = 0;
            this.btnSet.Text = "✔";
            this.btnSet.UseVisualStyleBackColor = false;
            // 
            // CustomPropertySetter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.cmbProperty);
            this.Controls.Add(this.pnlButtons);
            this.Name = "CustomPropertySetter";
            this.Size = new System.Drawing.Size(174, 51);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WinForms.Controls.AdvancedComboBox cmbProperty;
        private WinForms.Controls.AdvancedTextBox txtValue;
        private System.Windows.Forms.Panel pnlButtons;
        private WinForms.Controls.AdvancedButton btnClear;
        private WinForms.Controls.AdvancedButton btnSet;
    }
}
