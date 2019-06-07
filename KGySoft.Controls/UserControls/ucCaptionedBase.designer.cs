using System.Windows.Forms;
namespace KGySoft.Controls
{
    partial class ucCaptionedBase
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.chkCheckBox = new System.Windows.Forms.CheckBox();
            this.pnlTopPadding = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.pnlContent);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 1);
            this.groupBox.Name = "groupBox";
            this.groupBox.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox.Size = new System.Drawing.Size(133, 39);
            this.groupBox.TabIndex = 12;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Caption";
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(5, 13);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(123, 21);
            this.pnlContent.TabIndex = 0;
            // 
            // chkCheckBox
            // 
            this.chkCheckBox.Location = new System.Drawing.Point(5, 0);
            this.chkCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.chkCheckBox.Name = "chkCheckBox";
            this.chkCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chkCheckBox.Size = new System.Drawing.Size(15, 13);
            this.chkCheckBox.TabIndex = 13;
            this.chkCheckBox.TabStop = false;
            this.chkCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkCheckBox.UseVisualStyleBackColor = true;
            this.chkCheckBox.Visible = false;
            // 
            // pnlTopPadding
            // 
            this.pnlTopPadding.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopPadding.Location = new System.Drawing.Point(0, 0);
            this.pnlTopPadding.Name = "pnlTopPadding";
            this.pnlTopPadding.Size = new System.Drawing.Size(133, 1);
            this.pnlTopPadding.TabIndex = 14;
            // 
            // lblCaption
            // 
            this.lblCaption.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCaption.Location = new System.Drawing.Point(0, 1);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(0, 39);
            this.lblCaption.TabIndex = 15;
            this.lblCaption.Text = "Caption";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucCaptionedBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkCheckBox);
            this.Controls.Add(this.lblCaption);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.pnlTopPadding);
            this.Name = "ucCaptionedBase";
            this.Size = new System.Drawing.Size(133, 40);
            this.groupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.GroupBox groupBox;
        protected System.Windows.Forms.CheckBox chkCheckBox;
        private System.Windows.Forms.Panel pnlTopPadding;
        private System.Windows.Forms.Label lblCaption;
		protected Panel pnlContent;		
    }
}
