namespace KGySoft.WinForms.Controls
{
    partial class ucDecimal
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
            this.decimalControl = new DecimalTextBox();
            this.groupBox.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.decimalControl);
            // 
            // decimalControl
            // 
            this.decimalControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decimalControl.Location = new System.Drawing.Point(0, 0);
            this.decimalControl.Name = "decimalControl";
            this.decimalControl.Size = new System.Drawing.Size(123, 20);
            this.decimalControl.TabIndex = 1;
            // 
            // ucDecimal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ucDecimal";
            this.groupBox.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DecimalTextBox decimalControl;




    }
}
