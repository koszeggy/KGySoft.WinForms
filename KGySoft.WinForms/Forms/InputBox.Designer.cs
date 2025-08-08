namespace KGySoft.WinForms.Forms
{
    partial class InputBox
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
            this.lblPrompt = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.edtValue = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.SuspendLayout();
            // 
            // pnlButtons
            // 
            this.pnlButtons.DynamicStringLocalization = KGySoft.WinForms.DynamicStringLocalization.Disabled;
            this.pnlButtons.Location = new System.Drawing.Point(0, 55);
            this.pnlButtons.Size = new System.Drawing.Size(373, 35);
            this.pnlButtons.TabIndex = 2;
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 18);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(50, 13);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "lblPrompt";
            // 
            // edtValue
            // 
            this.edtValue.Location = new System.Drawing.Point(15, 34);
            this.edtValue.Name = "edtValue";
            this.edtValue.Size = new System.Drawing.Size(346, 20);
            this.edtValue.TabIndex = 1;
            this.edtValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.edtValue_KeyPress);
            // 
            // InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 90);
            this.Controls.Add(this.edtValue);
            this.Controls.Add(this.lblPrompt);
            this.Name = "InputBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InputBox";
            this.Controls.SetChildIndex(this.pnlButtons, 0);
            this.Controls.SetChildIndex(this.lblPrompt, 0);
            this.Controls.SetChildIndex(this.edtValue, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private KGySoft.WinForms.Controls.AdvancedLabel lblPrompt;
        private KGySoft.WinForms.Controls.AdvancedTextBox edtValue;
    }
}