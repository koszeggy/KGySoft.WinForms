namespace KGySoft.Controls
{
    partial class ucTextWithPicture
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbImg = new System.Windows.Forms.PictureBox();
            this.groupBox.SuspendLayout();
            this.pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImg)).BeginInit();
            this.SuspendLayout();
            // 
            // textControl
            // 
            this.textControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textControl.Location = new System.Drawing.Point(36, 0);
            this.textControl.Size = new System.Drawing.Size(53, 20);
            // 
            // groupBox
            // 
            this.groupBox.Size = new System.Drawing.Size(99, 38);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pbImg);
            this.pnlContent.Size = new System.Drawing.Size(89, 20);
            this.pnlContent.Controls.SetChildIndex(this.pbImg, 0);
            this.pnlContent.Controls.SetChildIndex(this.textControl, 0);
            // 
            // pbImg
            // 
            this.pbImg.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbImg.InitialImage = null;
            this.pbImg.Location = new System.Drawing.Point(0, 0);
            this.pbImg.Name = "pbImg";
            this.pbImg.Size = new System.Drawing.Size(36, 20);
            this.pbImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbImg.TabIndex = 2;
            this.pbImg.TabStop = false;
            // 
            // ucTextWithPicture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ucTextWithPicture";
            this.Size = new System.Drawing.Size(99, 39);
            this.groupBox.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pbImg;





    }
}
