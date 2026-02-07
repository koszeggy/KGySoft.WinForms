namespace KGySoft.WinForms.Controls
{
    partial class ucText
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
			this.textControl = new AdvancedTextBox();
			this.groupBox.SuspendLayout();
			this.pnlContent.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlContent
			// 
			this.pnlContent.Controls.Add(this.textControl);
			// 
			// textControl
			// 
			this.textControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textControl.Location = new System.Drawing.Point(0, 0);
			this.textControl.Name = "textControl";
			this.textControl.Size = new System.Drawing.Size(123, 20);
			this.textControl.TabIndex = 1;
			// 
			// ucText
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "ucText";
			this.groupBox.ResumeLayout(false);
			this.pnlContent.ResumeLayout(false);
			this.pnlContent.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private AdvancedTextBox textControl;



    }
}
