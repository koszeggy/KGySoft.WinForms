namespace KGySoft.WinForms.Test.Forms
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
            this.pnlTestArea = new System.Windows.Forms.Panel();
            this.lblInstuction = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.grdProperties = new System.Windows.Forms.PropertyGrid();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pnlTestArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.lblInstuction);
            this.pnlTestArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTestArea.Location = new System.Drawing.Point(0, 0);
            this.pnlTestArea.Name = "pnlTestArea";
            this.pnlTestArea.Size = new System.Drawing.Size(64, 260);
            this.pnlTestArea.TabIndex = 0;
            // 
            // lblInstuction
            // 
            this.lblInstuction.AutoSize = true;
            this.lblInstuction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstuction.BorderStyle = KGySoft.WinForms.Controls.AdvancedBorderStyle.Flat;
            this.lblInstuction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstuction.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInstuction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblInstuction.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblInstuction.Location = new System.Drawing.Point(0, 0);
            this.lblInstuction.Name = "lblInstuction";
            this.lblInstuction.Padding = new System.Windows.Forms.Padding(5);
            this.lblInstuction.Size = new System.Drawing.Size(64, 105);
            this.lblInstuction.TabIndex = 0;
            this.lblInstuction.Text = "Click the items to see their properties";
            // 
            // grdProperties
            // 
            this.grdProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.grdProperties.Location = new System.Drawing.Point(67, 0);
            this.grdProperties.Name = "grdProperties";
            this.grdProperties.Size = new System.Drawing.Size(215, 260);
            this.grdProperties.TabIndex = 1;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(64, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 260);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // ControlsTestBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 260);
            this.Controls.Add(this.pnlTestArea);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.grdProperties);
            this.Name = "ControlsTestBaseForm";
            this.Text = "ControlsTestBaseForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlsTestBaseForm_FormClosing);
            this.Load += new System.EventHandler(this.ControlsTestBaseForm_Load);
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid grdProperties;
        protected System.Windows.Forms.Panel pnlTestArea;
        private System.Windows.Forms.Splitter splitter1;
        protected KGySoft.WinForms.Controls.AdvancedLabel lblInstuction;
    }
}