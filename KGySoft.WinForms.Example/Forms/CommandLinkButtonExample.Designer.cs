namespace KGySoft.WinForms.Example.Forms
{
    partial class CommandLinkButtonExample
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandLinkButtonExample));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.commandLinkButton13 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton11 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton12 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton3 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton4 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton2 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton1 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.commandLinkButton10 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton9 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton8 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.commandLinkButton7 = new KGySoft.WinForms.Controls.CommandLinkButton();
            this.pnlTestArea.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.groupBox1);
            this.pnlTestArea.Controls.Add(this.groupBox2);
            this.pnlTestArea.Margin = new System.Windows.Forms.Padding(4);
            this.pnlTestArea.Size = new System.Drawing.Size(625, 566);
            this.pnlTestArea.Controls.SetChildIndex(this.lblInstruction, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox2, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox1, 0);
            // 
            // lblInstuction
            // 
            this.lblInstruction.Size = new System.Drawing.Size(623, 79);
            this.lblInstruction.Text = resources.GetString("lblInstruction.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.commandLinkButton13);
            this.groupBox1.Controls.Add(this.commandLinkButton11);
            this.groupBox1.Controls.Add(this.commandLinkButton12);
            this.groupBox1.Controls.Add(this.commandLinkButton3);
            this.groupBox1.Controls.Add(this.commandLinkButton4);
            this.groupBox1.Controls.Add(this.commandLinkButton2);
            this.groupBox1.Controls.Add(this.commandLinkButton1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 321);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(625, 170);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Icons";
            // 
            // commandLinkButton13
            // 
            this.commandLinkButton13.Description = "(Non-System modes only)";
            this.commandLinkButton13.Image = null;
            this.commandLinkButton13.Location = new System.Drawing.Point(426, 19);
            this.commandLinkButton13.Name = "commandLinkButton13";
            this.commandLinkButton13.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.commandLinkButton13.Size = new System.Drawing.Size(176, 57);
            this.commandLinkButton13.TabIndex = 6;
            this.commandLinkButton13.Text = "RightToLeft";
            this.commandLinkButton13.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton11
            // 
            this.commandLinkButton11.Description = null;
            this.commandLinkButton11.Image = ((System.Drawing.Image)(resources.GetObject("commandLinkButton11.Image")));
            this.commandLinkButton11.Location = new System.Drawing.Point(233, 113);
            this.commandLinkButton11.Name = "commandLinkButton11";
            this.commandLinkButton11.Size = new System.Drawing.Size(213, 41);
            this.commandLinkButton11.TabIndex = 5;
            this.commandLinkButton11.Text = "Custom Image (Standard)";
            this.commandLinkButton11.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton12
            // 
            this.commandLinkButton12.Description = null;
            this.commandLinkButton12.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.commandLinkButton12.Image = ((System.Drawing.Image)(resources.GetObject("commandLinkButton12.Image")));
            this.commandLinkButton12.Location = new System.Drawing.Point(16, 113);
            this.commandLinkButton12.Name = "commandLinkButton12";
            this.commandLinkButton12.Size = new System.Drawing.Size(202, 41);
            this.commandLinkButton12.TabIndex = 2;
            this.commandLinkButton12.Text = "Custom Image (System)";
            this.commandLinkButton12.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton3
            // 
            this.commandLinkButton3.Description = null;
            this.commandLinkButton3.Image = null;
            this.commandLinkButton3.IsElevated = true;
            this.commandLinkButton3.Location = new System.Drawing.Point(233, 66);
            this.commandLinkButton3.Name = "commandLinkButton3";
            this.commandLinkButton3.Size = new System.Drawing.Size(170, 41);
            this.commandLinkButton3.TabIndex = 4;
            this.commandLinkButton3.Text = "Elevated (Standard)";
            this.commandLinkButton3.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton4
            // 
            this.commandLinkButton4.Description = null;
            this.commandLinkButton4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.commandLinkButton4.Image = null;
            this.commandLinkButton4.IsElevated = true;
            this.commandLinkButton4.Location = new System.Drawing.Point(16, 66);
            this.commandLinkButton4.Name = "commandLinkButton4";
            this.commandLinkButton4.Size = new System.Drawing.Size(159, 41);
            this.commandLinkButton4.TabIndex = 1;
            this.commandLinkButton4.Text = "Elevated (System)";
            this.commandLinkButton4.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton2
            // 
            this.commandLinkButton2.Description = "";
            this.commandLinkButton2.Image = null;
            this.commandLinkButton2.Location = new System.Drawing.Point(233, 19);
            this.commandLinkButton2.Name = "commandLinkButton2";
            this.commandLinkButton2.Size = new System.Drawing.Size(163, 41);
            this.commandLinkButton2.TabIndex = 3;
            this.commandLinkButton2.Text = "No Glyph (Standard)";
            this.commandLinkButton2.UseDefaultGlyph = false;
            this.commandLinkButton2.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton1
            // 
            this.commandLinkButton1.Description = "";
            this.commandLinkButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.commandLinkButton1.Image = null;
            this.commandLinkButton1.Location = new System.Drawing.Point(16, 20);
            this.commandLinkButton1.Name = "commandLinkButton1";
            this.commandLinkButton1.Size = new System.Drawing.Size(152, 41);
            this.commandLinkButton1.TabIndex = 0;
            this.commandLinkButton1.Text = "No Glyph (System)";
            this.commandLinkButton1.UseDefaultGlyph = false;
            this.commandLinkButton1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.commandLinkButton10);
            this.groupBox2.Controls.Add(this.commandLinkButton9);
            this.groupBox2.Controls.Add(this.commandLinkButton8);
            this.groupBox2.Controls.Add(this.commandLinkButton7);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 28);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(625, 293);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rendering modes";
            // 
            // commandLinkButton10
            // 
            this.commandLinkButton10.Description = "Flat style rendering. Similarly to common flat style rendering, FlatAppearance ca" +
    "n be adjusted.";
            this.commandLinkButton10.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandLinkButton10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.commandLinkButton10.Image = null;
            this.commandLinkButton10.Location = new System.Drawing.Point(3, 240);
            this.commandLinkButton10.Name = "commandLinkButton10";
            this.commandLinkButton10.Size = new System.Drawing.Size(619, 50);
            this.commandLinkButton10.TabIndex = 3;
            this.commandLinkButton10.Text = "FlatStyle = Flat";
            this.commandLinkButton10.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton9
            // 
            this.commandLinkButton9.Description = "This button is rendered with no visual styles (Windows classic mode).\r\nThere is n" +
    "o transparency so BackColor is adjustable, too.";
            this.commandLinkButton9.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandLinkButton9.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.commandLinkButton9.Image = null;
            this.commandLinkButton9.Location = new System.Drawing.Point(3, 175);
            this.commandLinkButton9.Name = "commandLinkButton9";
            this.commandLinkButton9.Size = new System.Drawing.Size(619, 65);
            this.commandLinkButton9.TabIndex = 2;
            this.commandLinkButton9.Text = "FlatStyle = Popup";
            this.commandLinkButton9.UseVisualStyleBackColor = false;
            // 
            // commandLinkButton8
            // 
            this.commandLinkButton8.Description = resources.GetString("commandLinkButton8.Description");
            this.commandLinkButton8.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandLinkButton8.Image = null;
            this.commandLinkButton8.Location = new System.Drawing.Point(3, 88);
            this.commandLinkButton8.Name = "commandLinkButton8";
            this.commandLinkButton8.Size = new System.Drawing.Size(619, 87);
            this.commandLinkButton8.TabIndex = 1;
            this.commandLinkButton8.Text = "FlatStyle = Standard";
            this.commandLinkButton8.UseVisualStyleBackColor = true;
            // 
            // commandLinkButton7
            // 
            this.commandLinkButton7.Description = "This button is rendered by Windows. If Windows version is under Windows Vista, Fl" +
    "atStyle is automatically switched to Standard internally to use compatible rende" +
    "ring.";
            this.commandLinkButton7.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandLinkButton7.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.commandLinkButton7.Image = null;
            this.commandLinkButton7.Location = new System.Drawing.Point(3, 16);
            this.commandLinkButton7.Name = "commandLinkButton7";
            this.commandLinkButton7.Size = new System.Drawing.Size(619, 72);
            this.commandLinkButton7.TabIndex = 0;
            this.commandLinkButton7.Text = "FlatStyle = System";
            this.commandLinkButton7.UseVisualStyleBackColor = true;
            // 
            // CommandLinkButtonExample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 500);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CommandLinkButtonExample";
            this.Text = "CommandLinkTest";
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton11;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton12;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton3;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton4;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton2;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton1;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton13;
        private System.Windows.Forms.GroupBox groupBox2;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton10;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton9;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton8;
        private KGySoft.WinForms.Controls.CommandLinkButton commandLinkButton7;






    }
}