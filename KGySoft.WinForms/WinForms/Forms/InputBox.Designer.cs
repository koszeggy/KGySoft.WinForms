namespace KGySoft.WinForms.Forms
{
    partial class InputBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPrompt = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.edtValue = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlButtons
            // 
            this.pnlButtons.Location = new System.Drawing.Point(0, 55);
            this.pnlButtons.Size = new System.Drawing.Size(373, 35);
            this.pnlButtons.TabIndex = 2;
            // 
            // lblPrompt
            // 
            this.lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPrompt.Location = new System.Drawing.Point(10, 0);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Padding = new System.Windows.Forms.Padding(0, 20, 0, 3);
            this.lblPrompt.Size = new System.Drawing.Size(353, 35);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "lblPrompt";
            this.lblPrompt.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // edtValue
            // 
            this.edtValue.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.edtValue.Location = new System.Drawing.Point(10, 35);
            this.edtValue.Name = "edtValue";
            this.edtValue.Size = new System.Drawing.Size(353, 20);
            this.edtValue.TabIndex = 1;
            this.edtValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.edtValue_KeyPress);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.lblPrompt);
            this.pnlContent.Controls.Add(this.edtValue);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlContent.Size = new System.Drawing.Size(373, 55);
            this.pnlContent.TabIndex = 0;
            // 
            // InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 90);
            this.Controls.Add(this.pnlContent);
            this.Name = "InputBox";
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InputBox";
            this.Controls.SetChildIndex(this.pnlButtons, 0);
            this.Controls.SetChildIndex(this.pnlContent, 0);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private KGySoft.WinForms.Controls.AdvancedLabel lblPrompt;
        private KGySoft.WinForms.Controls.AdvancedTextBox edtValue;
        private System.Windows.Forms.Panel pnlContent;
    }
}