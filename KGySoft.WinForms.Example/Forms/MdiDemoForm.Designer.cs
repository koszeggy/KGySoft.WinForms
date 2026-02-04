namespace KGySoft.WinForms.Example.Forms
{
    partial class MdiDemoForm
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
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.miAddRootChild = new System.Windows.Forms.ToolStripMenuItem();
            this.msMenu = new System.Windows.Forms.MenuStrip();
            this.miWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.miArrange = new System.Windows.Forms.ToolStripMenuItem();
            this.miCascade = new System.Windows.Forms.ToolStripMenuItem();
            this.miTileHorizontally = new System.Windows.Forms.ToolStripMenuItem();
            this.miTileVertically = new System.Windows.Forms.ToolStripMenuItem();
            this.miMinimizeAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.gbLog.SuspendLayout();
            this.msMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLog
            // 
            this.gbLog.Controls.Add(this.txtLog);
            this.gbLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbLog.Location = new System.Drawing.Point(0, 295);
            this.gbLog.Name = "gbLog";
            this.gbLog.Size = new System.Drawing.Size(800, 155);
            this.gbLog.TabIndex = 3;
            this.gbLog.TabStop = false;
            this.gbLog.Text = "Event Log";
            // 
            // txtLog
            // 
            this.txtLog.DisabledBackColor = System.Drawing.SystemColors.Window;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.EnabledForeColor = System.Drawing.SystemColors.WindowText;
            this.txtLog.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(3, 16);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(794, 136);
            this.txtLog.TabIndex = 0;
            this.txtLog.WordWrap = false;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 292);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(800, 3);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // miAddRootChild
            // 
            this.miAddRootChild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.miAddRootChild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAddRootChild.Name = "miAddRootChild";
            this.miAddRootChild.Size = new System.Drawing.Size(100, 20);
            this.miAddRootChild.Text = "Add Root Child";
            // 
            // msMenu
            // 
            this.msMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAddRootChild,
            this.miWindows});
            this.msMenu.Location = new System.Drawing.Point(0, 0);
            this.msMenu.MdiWindowListItem = this.miWindows;
            this.msMenu.Name = "msMenu";
            this.msMenu.Size = new System.Drawing.Size(800, 24);
            this.msMenu.TabIndex = 1;
            this.msMenu.Text = "toolStrip1";
            // 
            // miWindows
            // 
            this.miWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miArrange,
            this.miCloseAll});
            this.miWindows.Name = "miWindows";
            this.miWindows.Size = new System.Drawing.Size(68, 20);
            this.miWindows.Text = "Windows";
            // 
            // miArrange
            // 
            this.miArrange.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCascade,
            this.miTileHorizontally,
            this.miTileVertically,
            this.miMinimizeAll});
            this.miArrange.Name = "miArrange";
            this.miArrange.Size = new System.Drawing.Size(180, 22);
            this.miArrange.Text = "Arrange";
            // 
            // miCascade
            // 
            this.miCascade.Name = "miCascade";
            this.miCascade.Size = new System.Drawing.Size(180, 22);
            this.miCascade.Text = "Cascade";
            // 
            // miTileHorizontally
            // 
            this.miTileHorizontally.Name = "miTileHorizontally";
            this.miTileHorizontally.Size = new System.Drawing.Size(180, 22);
            this.miTileHorizontally.Text = "Tile Horizontally";
            // 
            // miTileVertically
            // 
            this.miTileVertically.Name = "miTileVertically";
            this.miTileVertically.Size = new System.Drawing.Size(180, 22);
            this.miTileVertically.Text = "Tile Vertically";
            // 
            // miMinimizeAll
            // 
            this.miMinimizeAll.Name = "miMinimizeAll";
            this.miMinimizeAll.Size = new System.Drawing.Size(180, 22);
            this.miMinimizeAll.Text = "Minimize All";
            // 
            // miCloseAll
            // 
            this.miCloseAll.Name = "miCloseAll";
            this.miCloseAll.Size = new System.Drawing.Size(180, 22);
            this.miCloseAll.Text = "Close All";
            // 
            // MdiDemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.gbLog);
            this.Controls.Add(this.msMenu);
            this.DynamicStringLocalization = KGySoft.WinForms.DynamicStringLocalization.Disabled;
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.msMenu;
            this.Name = "MdiDemoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MDI Application Demo";
            this.gbLog.ResumeLayout(false);
            this.gbLog.PerformLayout();
            this.msMenu.ResumeLayout(false);
            this.msMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbLog;
        private WinForms.Controls.AdvancedTextBox txtLog;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripMenuItem miAddRootChild;
        private System.Windows.Forms.MenuStrip msMenu;
        private System.Windows.Forms.ToolStripMenuItem miWindows;
        private System.Windows.Forms.ToolStripMenuItem miArrange;
        private System.Windows.Forms.ToolStripMenuItem miCascade;
        private System.Windows.Forms.ToolStripMenuItem miTileHorizontally;
        private System.Windows.Forms.ToolStripMenuItem miTileVertically;
        private System.Windows.Forms.ToolStripMenuItem miCloseAll;
        private System.Windows.Forms.ToolStripMenuItem miMinimizeAll;
    }
}