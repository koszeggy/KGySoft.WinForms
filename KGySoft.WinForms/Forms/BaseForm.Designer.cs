namespace KGySoft.WinForms.Forms
{
	partial class BaseForm
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
            this.components = new System.ComponentModel.Container();
            this.BaseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // BaseToolTip
            // 
            this.BaseToolTip.AutomaticDelay = 100;
            this.BaseToolTip.AutoPopDelay = 5000;
            this.BaseToolTip.InitialDelay = 100;
            this.BaseToolTip.ReshowDelay = 20;
            // 
            // BaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "BaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BaseForm";
            this.ResumeLayout(false);
		}

		#endregion

		protected System.Windows.Forms.ToolTip BaseToolTip;
	}
}