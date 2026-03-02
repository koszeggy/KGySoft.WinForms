namespace KGySoft.WinForms.Example.Forms
{
    partial class ControlsTestBaseForm
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
            this.components = new System.ComponentModel.Container();
            this.pnlTestArea = new System.Windows.Forms.Panel();
            this.lblInstruction = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.cmsGridMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miResetValue = new System.Windows.Forms.ToolStripMenuItem();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.grdProperties = new System.Windows.Forms.PropertyGrid();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.customPropertySetter = new KGySoft.WinForms.Example.Controls.CustomPropertySetter();
            this.lblSelection = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.pnlTestArea.SuspendLayout();
            this.cmsGridMenu.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.lblInstruction);
            this.pnlTestArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTestArea.Location = new System.Drawing.Point(0, 0);
            this.pnlTestArea.Name = "pnlTestArea";
            this.pnlTestArea.Size = new System.Drawing.Size(214, 260);
            this.pnlTestArea.TabIndex = 0;
            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = true;
            this.lblInstruction.BorderStyle = KGySoft.WinForms.Controls.AdvancedBorderStyle.Flat;
            this.lblInstruction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstruction.EnabledBackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.EnabledForeColor = System.Drawing.SystemColors.WindowText;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInstruction.Location = new System.Drawing.Point(0, 0);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Padding = new System.Windows.Forms.Padding(5);
            this.lblInstruction.Size = new System.Drawing.Size(214, 42);
            this.lblInstruction.TabIndex = 0;
            this.lblInstruction.Text = "Click the items to see their properties";
            // 
            // cmsGridMenu
            // 
            this.cmsGridMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miResetValue});
            this.cmsGridMenu.Name = "cmsGridMenu";
            this.cmsGridMenu.Size = new System.Drawing.Size(134, 26);
            // 
            // miResetValue
            // 
            this.miResetValue.Name = "miResetValue";
            this.miResetValue.Size = new System.Drawing.Size(133, 22);
            this.miResetValue.Text = "Reset Value";
            this.miResetValue.Click += new System.EventHandler(this.miResetValue_Click);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(214, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 260);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // pnlProperties
            // 
            this.pnlProperties.Controls.Add(this.grdProperties);
            this.pnlProperties.Controls.Add(this.splitter2);
            this.pnlProperties.Controls.Add(this.customPropertySetter);
            this.pnlProperties.Controls.Add(this.lblSelection);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Location = new System.Drawing.Point(217, 0);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(213, 260);
            this.pnlProperties.TabIndex = 3;
            // 
            // grdProperties
            // 
            this.grdProperties.ContextMenuStrip = this.cmsGridMenu;
            this.grdProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdProperties.Location = new System.Drawing.Point(0, 65);
            this.grdProperties.Name = "grdProperties";
            this.grdProperties.Size = new System.Drawing.Size(213, 195);
            this.grdProperties.TabIndex = 2;
            this.grdProperties.SelectedObjectsChanged += new System.EventHandler(this.grdProperties_SelectedObjectsChanged);
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter2.Location = new System.Drawing.Point(0, 62);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(213, 3);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            // 
            // customPropertySetter
            // 
            this.customPropertySetter.Dock = System.Windows.Forms.DockStyle.Top;
            this.customPropertySetter.DynamicStringLocalization = KGySoft.WinForms.DynamicStringLocalization.Custom;
            this.customPropertySetter.Location = new System.Drawing.Point(0, 18);
            this.customPropertySetter.Name = "customPropertySetter";
            this.customPropertySetter.Size = new System.Drawing.Size(213, 44);
            this.customPropertySetter.TabIndex = 6;
            this.customPropertySetter.SelectedObjectsPropertyChanged += new System.EventHandler<System.ComponentModel.PropertyChangedEventArgs>(this.customPropertySetter_SelectedObjectsPropertyChanged);
            // 
            // lblSelection
            // 
            this.lblSelection.AutoSize = true;
            this.lblSelection.BorderStyle = KGySoft.WinForms.Controls.AdvancedBorderStyle.Flat;
            this.lblSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSelection.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblSelection.Location = new System.Drawing.Point(0, 0);
            this.lblSelection.Name = "lblSelection";
            this.lblSelection.Size = new System.Drawing.Size(213, 18);
            this.lblSelection.TabIndex = 3;
            this.lblSelection.Text = "";
            // 
            // ControlsTestBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 260);
            this.Controls.Add(this.pnlTestArea);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.pnlProperties);
            this.Name = "ControlsTestBaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ControlsTestBaseForm";
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.cmsGridMenu.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.pnlProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        protected System.Windows.Forms.Panel pnlTestArea;
        private System.Windows.Forms.Splitter splitter1;
        protected KGySoft.WinForms.Controls.AdvancedLabel lblInstruction;
        private System.Windows.Forms.ContextMenuStrip cmsGridMenu;
        private System.Windows.Forms.ToolStripMenuItem miResetValue;
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.PropertyGrid grdProperties;
        private KGySoft.WinForms.Controls.AdvancedLabel lblSelection;
        private System.Windows.Forms.Splitter splitter2;
        private Controls.CustomPropertySetter customPropertySetter;
    }
}