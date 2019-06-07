namespace KGySoft.Controls
{
    partial class ucCustomSelector
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
			this.pnlPadding = new System.Windows.Forms.Panel();
			this.pbImage = new System.Windows.Forms.PictureBox();
			this.pnlActionPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.cmbCombo = new KGySoft.Controls.AdvancedComboBox();
			this.groupBox.SuspendLayout();
			this.pnlContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox
			// 
			this.groupBox.Padding = new System.Windows.Forms.Padding(5, 1, 5, 3);
			this.groupBox.Size = new System.Drawing.Size(240, 39);
			// 
			// pnlContent
			// 
			this.pnlContent.Controls.Add(this.cmbCombo);
			this.pnlContent.Controls.Add(this.pbImage);
			this.pnlContent.Controls.Add(this.pnlPadding);
			this.pnlContent.Controls.Add(this.pnlActionPanel);
			// 
			// pnlPadding
			// 
			this.pnlPadding.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlPadding.Location = new System.Drawing.Point(0, 0);
			this.pnlPadding.Name = "pnlPadding";
			this.pnlPadding.Size = new System.Drawing.Size(230, 1);
			this.pnlPadding.TabIndex = 3;
			// 
			// pbImage
			// 
			this.pbImage.Dock = System.Windows.Forms.DockStyle.Left;
			this.pbImage.ErrorImage = null;
			this.pbImage.InitialImage = null;
			this.pbImage.Location = new System.Drawing.Point(0, 1);
			this.pbImage.Name = "pbImage";
			this.pbImage.Size = new System.Drawing.Size(22, 21);
			this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbImage.TabIndex = 4;
			this.pbImage.TabStop = false;
			this.pbImage.Visible = false;
			// 
			// pnlActionPanel
			// 
			this.pnlActionPanel.AutoSize = true;
			this.pnlActionPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlActionPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlActionPanel.Location = new System.Drawing.Point(230, 0);
			this.pnlActionPanel.Name = "pnlActionPanel";
			this.pnlActionPanel.Size = new System.Drawing.Size(0, 22);
			this.pnlActionPanel.TabIndex = 5;
			this.pnlActionPanel.WrapContents = false;
			// 
			// textControl
			// 
			this.cmbCombo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbCombo.Location = new System.Drawing.Point(22, 1);
			this.cmbCombo.Name = "textControl";
			this.cmbCombo.Size = new System.Drawing.Size(208, 20);
			this.cmbCombo.TabIndex = 6;
			// 
			// ucCustomSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "ucCustomSelector";
			this.Size = new System.Drawing.Size(240, 40);
			this.Load += new System.EventHandler(this.ucCustomSelector_Load);
			this.groupBox.ResumeLayout(false);
			this.pnlContent.ResumeLayout(false);
			this.pnlContent.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private AdvancedComboBox cmbCombo;
        private System.Windows.Forms.Panel pnlPadding;
        private System.Windows.Forms.FlowLayoutPanel pnlActionPanel;
        private System.Windows.Forms.PictureBox pbImage;

    }
}
