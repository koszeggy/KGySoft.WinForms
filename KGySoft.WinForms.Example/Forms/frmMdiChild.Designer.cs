namespace KGySoft.WinForms.Example.Forms
{
    partial class frmMdiChild
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
            this.msMenu = new System.Windows.Forms.MenuStrip();
            this.miChildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpenChildNormally = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpenChildAsDialog = new System.Windows.Forms.ToolStripMenuItem();
            this.lblStatus = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.miCloseOwnedChildrenNow = new System.Windows.Forms.ToolStripMenuItem();
            this.miCloseOwnedChildrenWhenClosed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.msMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // msMenu
            // 
            this.msMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miChildMenu});
            this.msMenu.Location = new System.Drawing.Point(0, 0);
            this.msMenu.Name = "msMenu";
            this.msMenu.Size = new System.Drawing.Size(241, 24);
            this.msMenu.TabIndex = 0;
            // 
            // miChildMenu
            // 
            this.miChildMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.miChildMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOpenChildNormally,
            this.miOpenChildAsDialog,
            this.toolStripMenuItem1,
            this.miCloseOwnedChildrenWhenClosed,
            this.miCloseOwnedChildrenNow});
            this.miChildMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miChildMenu.Name = "miChildMenu";
            this.miChildMenu.Size = new System.Drawing.Size(92, 20);
            this.miChildMenu.Text = "miChildMenu";
            // 
            // miOpenChildNormally
            // 
            this.miOpenChildNormally.Name = "miOpenChildNormally";
            this.miOpenChildNormally.Size = new System.Drawing.Size(265, 22);
            this.miOpenChildNormally.Text = "Open Child Normally";
            // 
            // miOpenChildAsDialog
            // 
            this.miOpenChildAsDialog.Name = "miOpenChildAsDialog";
            this.miOpenChildAsDialog.Size = new System.Drawing.Size(265, 22);
            this.miOpenChildAsDialog.Text = "Open Child as Dialog";
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Location = new System.Drawing.Point(0, 24);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(241, 79);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "lblStatus";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // miCloseOwnedChildrenNow
            // 
            this.miCloseOwnedChildrenNow.Name = "miCloseOwnedChildrenNow";
            this.miCloseOwnedChildrenNow.Size = new System.Drawing.Size(265, 22);
            this.miCloseOwnedChildrenNow.Text = "Close Owned Children Now";
            // 
            // miCloseOwnedChildrenWhenClosed
            // 
            this.miCloseOwnedChildrenWhenClosed.Checked = true;
            this.miCloseOwnedChildrenWhenClosed.CheckOnClick = true;
            this.miCloseOwnedChildrenWhenClosed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miCloseOwnedChildrenWhenClosed.Name = "miCloseOwnedChildrenWhenClosed";
            this.miCloseOwnedChildrenWhenClosed.Size = new System.Drawing.Size(265, 22);
            this.miCloseOwnedChildrenWhenClosed.Text = "Close Owned Children When Closed";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(262, 6);
            // 
            // frmMdiChild
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 103);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.msMenu);
            this.MainMenuStrip = this.msMenu;
            this.Name = "frmMdiChild";
            this.Text = "frmMdiChild";
            this.msMenu.ResumeLayout(false);
            this.msMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msMenu;
        private System.Windows.Forms.ToolStripMenuItem miChildMenu;
        private System.Windows.Forms.ToolStripMenuItem miOpenChildNormally;
        private System.Windows.Forms.ToolStripMenuItem miOpenChildAsDialog;
        private WinForms.Controls.AdvancedLabel lblStatus;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem miCloseOwnedChildrenNow;
        private System.Windows.Forms.ToolStripMenuItem miCloseOwnedChildrenWhenClosed;
    }
}