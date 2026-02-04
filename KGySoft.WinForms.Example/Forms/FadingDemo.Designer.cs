namespace KGySoft.WinForms.Example.Forms
{
    partial class FadingDemo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FadingDemo));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkGroupBox1 = new KGySoft.WinForms.Controls.CheckGroupBox();
            this.fadingPaintControl1 = new KGySoft.WinForms.Example.Controls.FadingPaintControl();
            this.pnlTestArea.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.checkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.groupBox1);
            this.pnlTestArea.Controls.Add(this.checkGroupBox1);
            this.pnlTestArea.Size = new System.Drawing.Size(512, 534);
            this.pnlTestArea.Controls.SetChildIndex(this.lblInstruction, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.checkGroupBox1, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox1, 0);
            // 
            // lblInstruction
            // 
            this.lblInstruction.Size = new System.Drawing.Size(512, 67);
            this.lblInstruction.Text = resources.GetString("lblInstruction.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 191);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 343);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Example Source";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox1.Location = new System.Drawing.Point(3, 16);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(506, 324);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            this.textBox1.WordWrap = false;
            // 
            // checkGroupBox1
            // 
            this.checkGroupBox1.Controls.Add(this.fadingPaintControl1);
            this.checkGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkGroupBox1.Location = new System.Drawing.Point(0, 67);
            this.checkGroupBox1.Name = "checkGroupBox1";
            this.checkGroupBox1.Size = new System.Drawing.Size(512, 124);
            this.checkGroupBox1.Text = "Enabled";
            this.checkGroupBox1.TabIndex = 3;
            // 
            // fadingLabelDemo1
            // 
            this.fadingPaintControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fadingPaintControl1.FadingAnimationDefaultSpeed = 500;
            this.fadingPaintControl1.FadingAnimationsEnabled = true;
            this.fadingPaintControl1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.fadingPaintControl1.Location = new System.Drawing.Point(0, 0);
            this.fadingPaintControl1.Name = "fadingPaintControl1";
            this.fadingPaintControl1.Size = new System.Drawing.Size(512, 124);
            this.fadingPaintControl1.TabIndex = 2;
            // 
            // FadingDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 534);
            this.Name = "FadingDemo";
            this.Text = "FadingDemo";
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.checkGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private KGySoft.WinForms.Controls.CheckGroupBox checkGroupBox1;
        private KGySoft.WinForms.Example.Controls.FadingPaintControl fadingPaintControl1;
    }
}