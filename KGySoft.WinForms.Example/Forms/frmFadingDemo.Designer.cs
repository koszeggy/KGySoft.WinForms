namespace KGySoft.WinForms.Example.Forms
{
    partial class frmFadingDemo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFadingDemo));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.fadingLabelDemo1 = new FadingLabelDemo();
            this.pnlTestArea.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.groupBox1);
            this.pnlTestArea.Controls.Add(this.panel1);
            this.pnlTestArea.Size = new System.Drawing.Size(512, 534);
            this.pnlTestArea.Controls.SetChildIndex(this.lblInstuction, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.panel1, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox1, 0);
            // 
            // lblInstuction
            // 
            this.lblInstuction.Size = new System.Drawing.Size(512, 67);
            this.lblInstuction.Text = resources.GetString("lblInstuction.Text");
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
            // panel1
            // 
            this.panel1.Controls.Add(this.fadingLabelDemo1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 67);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(512, 124);
            this.panel1.TabIndex = 3;
            // 
            // fadingLabelDemo1
            // 
            this.fadingLabelDemo1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fadingLabelDemo1.FadingAnimationDefaultSpeed = 500;
            this.fadingLabelDemo1.FadingAnimationsEnabled = true;
            this.fadingLabelDemo1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.fadingLabelDemo1.Location = new System.Drawing.Point(0, 0);
            this.fadingLabelDemo1.Name = "fadingLabelDemo1";
            this.fadingLabelDemo1.Size = new System.Drawing.Size(512, 124);
            this.fadingLabelDemo1.TabIndex = 2;
            this.fadingLabelDemo1.Text = resources.GetString("fadingLabelDemo1.Text");
            // 
            // frmFadingDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 534);
            this.Name = "frmFadingDemo";
            this.Text = "frmFadingDemo";
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel1;
        private FadingLabelDemo fadingLabelDemo1;
    }
}