namespace KGySoft.WinForms.Example
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkGroupBox1 = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.advancedButton1 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedCheckBox1 = new KGySoft.WinForms.Controls.AdvancedCheckBox();
            this.checkGroupBox2 = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.cgbChanging = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.dtbChangingH = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.dtbChangingW = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.advancedLabel2 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.advancedLabel1 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.cgbChanged = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.dtbChangedH = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.dtbChangedW = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.advancedLabel3 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.advancedLabel4 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.cgbAutoResized = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.dtbResizedH = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.dtbResizedW = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.advancedLabel5 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.advancedLabel6 = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.checkGroupBox1.SuspendLayout();
            this.cgbChanging.SuspendLayout();
            this.cgbChanged.SuspendLayout();
            this.cgbAutoResized.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkGroupBox1
            // 
            this.checkGroupBox1.Controls.Add(this.button1);
            this.checkGroupBox1.Controls.Add(this.advancedButton1);
            this.checkGroupBox1.Location = new System.Drawing.Point(12, 52);
            this.checkGroupBox1.Name = "checkGroupBox1";
            this.checkGroupBox1.Size = new System.Drawing.Size(200, 100);
            this.checkGroupBox1.TabIndex = 0;
            this.checkGroupBox1.TabStop = false;
            this.checkGroupBox1.Text = "checkGroupBox1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(21, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // advancedButton1
            // 
            this.advancedButton1.Location = new System.Drawing.Point(21, 19);
            this.advancedButton1.Name = "advancedButton1";
            this.advancedButton1.Size = new System.Drawing.Size(100, 23);
            this.advancedButton1.TabIndex = 1;
            this.advancedButton1.Text = "advancedButton1";
            this.advancedButton1.UseVisualStyleBackColor = true;
            // 
            // advancedCheckBox1
            // 
            this.advancedCheckBox1.AutoSize = true;
            this.advancedCheckBox1.Checked = true;
            this.advancedCheckBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.advancedCheckBox1.Location = new System.Drawing.Point(12, 12);
            this.advancedCheckBox1.Name = "advancedCheckBox1";
            this.advancedCheckBox1.Size = new System.Drawing.Size(129, 17);
            this.advancedCheckBox1.TabIndex = 1;
            this.advancedCheckBox1.Text = "advancedCheckBox1";
            this.advancedCheckBox1.UseVisualStyleBackColor = true;
            this.advancedCheckBox1.CheckedChanged += new System.EventHandler(this.advancedCheckBox1_CheckedChanged);
            // 
            // checkGroupBox2
            // 
            this.checkGroupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkGroupBox2.Location = new System.Drawing.Point(12, 162);
            this.checkGroupBox2.Name = "checkGroupBox2";
            this.checkGroupBox2.Size = new System.Drawing.Size(200, 100);
            this.checkGroupBox2.TabIndex = 2;
            this.checkGroupBox2.TabStop = false;
            this.checkGroupBox2.Text = "checkGroupBox2";
            // 
            // cgbChanging
            // 
            this.cgbChanging.Checked = false;
            this.cgbChanging.Controls.Add(this.dtbChangingH);
            this.cgbChanging.Controls.Add(this.dtbChangingW);
            this.cgbChanging.Controls.Add(this.advancedLabel2);
            this.cgbChanging.Controls.Add(this.advancedLabel1);
            this.cgbChanging.Location = new System.Drawing.Point(222, 12);
            this.cgbChanging.Name = "cgbChanging";
            this.cgbChanging.Size = new System.Drawing.Size(123, 63);
            this.cgbChanging.TabIndex = 6;
            this.cgbChanging.TabStop = false;
            this.cgbChanging.Text = "OnChanging";
            // 
            // dtbChangingH
            // 
            this.dtbChangingH.ChangeValueOnTextChange = true;
            this.dtbChangingH.Location = new System.Drawing.Point(30, 37);
            this.dtbChangingH.Name = "dtbChangingH";
            this.dtbChangingH.Size = new System.Drawing.Size(75, 20);
            this.dtbChangingH.TabIndex = 4;
            // 
            // dtbChangingW
            // 
            this.dtbChangingW.ChangeValueOnTextChange = true;
            this.dtbChangingW.Location = new System.Drawing.Point(30, 15);
            this.dtbChangingW.Name = "dtbChangingW";
            this.dtbChangingW.Size = new System.Drawing.Size(75, 20);
            this.dtbChangingW.TabIndex = 3;
            // 
            // advancedLabel2
            // 
            this.advancedLabel2.AutoSize = true;
            this.advancedLabel2.Location = new System.Drawing.Point(6, 40);
            this.advancedLabel2.Name = "advancedLabel2";
            this.advancedLabel2.Size = new System.Drawing.Size(15, 13);
            this.advancedLabel2.TabIndex = 2;
            this.advancedLabel2.Text = "H";
            // 
            // advancedLabel1
            // 
            this.advancedLabel1.AutoSize = true;
            this.advancedLabel1.Location = new System.Drawing.Point(6, 18);
            this.advancedLabel1.Name = "advancedLabel1";
            this.advancedLabel1.Size = new System.Drawing.Size(18, 13);
            this.advancedLabel1.TabIndex = 1;
            this.advancedLabel1.Text = "W";
            // 
            // cgbChanged
            // 
            this.cgbChanged.Checked = false;
            this.cgbChanged.Controls.Add(this.dtbChangedH);
            this.cgbChanged.Controls.Add(this.dtbChangedW);
            this.cgbChanged.Controls.Add(this.advancedLabel3);
            this.cgbChanged.Controls.Add(this.advancedLabel4);
            this.cgbChanged.Location = new System.Drawing.Point(222, 81);
            this.cgbChanged.Name = "cgbChanged";
            this.cgbChanged.Size = new System.Drawing.Size(123, 63);
            this.cgbChanged.TabIndex = 7;
            this.cgbChanged.TabStop = false;
            this.cgbChanged.Text = "OnChanged";
            // 
            // dtbChangedH
            // 
            this.dtbChangedH.ChangeValueOnTextChange = true;
            this.dtbChangedH.Location = new System.Drawing.Point(30, 37);
            this.dtbChangedH.Name = "dtbChangedH";
            this.dtbChangedH.Size = new System.Drawing.Size(75, 20);
            this.dtbChangedH.TabIndex = 4;
            // 
            // dtbChangedW
            // 
            this.dtbChangedW.ChangeValueOnTextChange = true;
            this.dtbChangedW.Location = new System.Drawing.Point(30, 15);
            this.dtbChangedW.Name = "dtbChangedW";
            this.dtbChangedW.Size = new System.Drawing.Size(75, 20);
            this.dtbChangedW.TabIndex = 3;
            // 
            // advancedLabel3
            // 
            this.advancedLabel3.AutoSize = true;
            this.advancedLabel3.Location = new System.Drawing.Point(6, 40);
            this.advancedLabel3.Name = "advancedLabel3";
            this.advancedLabel3.Size = new System.Drawing.Size(15, 13);
            this.advancedLabel3.TabIndex = 2;
            this.advancedLabel3.Text = "H";
            // 
            // advancedLabel4
            // 
            this.advancedLabel4.AutoSize = true;
            this.advancedLabel4.Location = new System.Drawing.Point(6, 18);
            this.advancedLabel4.Name = "advancedLabel4";
            this.advancedLabel4.Size = new System.Drawing.Size(18, 13);
            this.advancedLabel4.TabIndex = 1;
            this.advancedLabel4.Text = "W";
            // 
            // cgbAutoResized
            // 
            this.cgbAutoResized.Checked = false;
            this.cgbAutoResized.Controls.Add(this.dtbResizedH);
            this.cgbAutoResized.Controls.Add(this.dtbResizedW);
            this.cgbAutoResized.Controls.Add(this.advancedLabel5);
            this.cgbAutoResized.Controls.Add(this.advancedLabel6);
            this.cgbAutoResized.Location = new System.Drawing.Point(222, 150);
            this.cgbAutoResized.Name = "cgbAutoResized";
            this.cgbAutoResized.Size = new System.Drawing.Size(123, 63);
            this.cgbAutoResized.TabIndex = 8;
            this.cgbAutoResized.TabStop = false;
            this.cgbAutoResized.Text = "OnAutoResized";
            // 
            // dtbResizedH
            // 
            this.dtbResizedH.ChangeValueOnTextChange = true;
            this.dtbResizedH.Location = new System.Drawing.Point(30, 37);
            this.dtbResizedH.Name = "dtbResizedH";
            this.dtbResizedH.Size = new System.Drawing.Size(75, 20);
            this.dtbResizedH.TabIndex = 4;
            // 
            // dtbResizedW
            // 
            this.dtbResizedW.ChangeValueOnTextChange = true;
            this.dtbResizedW.Location = new System.Drawing.Point(30, 15);
            this.dtbResizedW.Name = "dtbResizedW";
            this.dtbResizedW.Size = new System.Drawing.Size(75, 20);
            this.dtbResizedW.TabIndex = 3;
            // 
            // advancedLabel5
            // 
            this.advancedLabel5.AutoSize = true;
            this.advancedLabel5.Location = new System.Drawing.Point(6, 40);
            this.advancedLabel5.Name = "advancedLabel5";
            this.advancedLabel5.Size = new System.Drawing.Size(15, 13);
            this.advancedLabel5.TabIndex = 2;
            this.advancedLabel5.Text = "H";
            // 
            // advancedLabel6
            // 
            this.advancedLabel6.AutoSize = true;
            this.advancedLabel6.Location = new System.Drawing.Point(6, 18);
            this.advancedLabel6.Name = "advancedLabel6";
            this.advancedLabel6.Size = new System.Drawing.Size(18, 13);
            this.advancedLabel6.TabIndex = 1;
            this.advancedLabel6.Text = "W";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 274);
            this.Controls.Add(this.cgbAutoResized);
            this.Controls.Add(this.cgbChanged);
            this.Controls.Add(this.cgbChanging);
            this.Controls.Add(this.checkGroupBox2);
            this.Controls.Add(this.checkGroupBox1);
            this.Controls.Add(this.advancedCheckBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.checkGroupBox1.ResumeLayout(false);
            this.checkGroupBox1.PerformLayout();
            this.cgbChanging.ResumeLayout(false);
            this.cgbChanging.PerformLayout();
            this.cgbChanged.ResumeLayout(false);
            this.cgbChanged.PerformLayout();
            this.cgbAutoResized.ResumeLayout(false);
            this.cgbAutoResized.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WinForms.Controls.CheckGroupBox checkGroupBox1;
        private WinForms.Controls.AdvancedCheckBox advancedCheckBox1;
        private WinForms.Controls.AdvancedButton advancedButton1;
        private WinForms.Controls.CheckGroupBox checkGroupBox2;
        private System.Windows.Forms.Button button1;
        private WinForms.Controls.CheckGroupBox cgbChanging;
        private WinForms.Controls.DecimalTextBox dtbChangingH;
        private WinForms.Controls.DecimalTextBox dtbChangingW;
        private WinForms.Controls.AdvancedLabel advancedLabel2;
        private WinForms.Controls.AdvancedLabel advancedLabel1;
        private WinForms.Controls.CheckGroupBox cgbChanged;
        private WinForms.Controls.DecimalTextBox dtbChangedH;
        private WinForms.Controls.DecimalTextBox dtbChangedW;
        private WinForms.Controls.AdvancedLabel advancedLabel3;
        private WinForms.Controls.AdvancedLabel advancedLabel4;
        private WinForms.Controls.CheckGroupBox cgbAutoResized;
        private WinForms.Controls.DecimalTextBox dtbResizedH;
        private WinForms.Controls.DecimalTextBox dtbResizedW;
        private WinForms.Controls.AdvancedLabel advancedLabel5;
        private WinForms.Controls.AdvancedLabel advancedLabel6;
    }
}