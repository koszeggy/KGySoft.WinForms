namespace KGySoft.WinForms.Forms
{
	partial class DialogBaseForm
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
            this.okCancelButtons = new KGySoft.WinForms.Controls.OkCancelButtons();
            this.SuspendLayout();
            // 
            // okCancelButtons
            // 
            this.okCancelButtons.BackColor = System.Drawing.Color.Transparent;
            this.okCancelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.okCancelButtons.Location = new System.Drawing.Point(0, 115);
            this.okCancelButtons.Name = "okCancelButtons";
            this.okCancelButtons.Size = new System.Drawing.Size(287, 35);
            this.okCancelButtons.TabIndex = 0;
            // 
            // DialogBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 150);
            this.Controls.Add(this.okCancelButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogBaseForm";
            this.Text = "DialogBaseForm";
            this.ResumeLayout(false);

		}

        #endregion

        private Controls.OkCancelButtons okCancelButtons;
    }
}