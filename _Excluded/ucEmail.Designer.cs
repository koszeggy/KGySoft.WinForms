using System.Windows.Forms;
namespace KGySoft.Controls
{
    partial class ucEmail
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
            this.components = new System.ComponentModel.Container();
            this.buttonEmail = new Button();
            this.groupBox.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // textControl
            // 
            this.textControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textControl.Margin = new System.Windows.Forms.Padding(1);
            this.textControl.Size = new System.Drawing.Size(97, 20);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.buttonEmail);
            this.pnlContent.Controls.SetChildIndex(this.buttonEmail, 0);
            this.pnlContent.Controls.SetChildIndex(this.textControl, 0);
            // 
            // buttonEmail
            // 
            this.buttonEmail.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonEmail.Image = global::KGySoft.Controls.FxResource.email1;
            this.buttonEmail.Location = new System.Drawing.Point(97, 0);
            this.buttonEmail.Margin = new System.Windows.Forms.Padding(1);
            this.buttonEmail.Name = "buttonEmail";
            this.buttonEmail.Size = new System.Drawing.Size(26, 21);
            this.buttonEmail.TabIndex = 2;
            this.buttonEmail.UseVisualStyleBackColor = true;
            // 
            // ucEmail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ucEmail";
            this.groupBox.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Button buttonEmail;

    }
}
