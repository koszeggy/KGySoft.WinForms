namespace KGySoft.WinForms.Example.Forms
{
    partial class frmAdvancedButton
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAdvancedButton));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.advancedButton3 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton2 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton1 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.advancedButton7 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton8 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton9 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton6 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton5 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton4 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.ucCaptionedContainer1 = new KGySoft.WinForms.Controls.ucCaptionedContainer();
            this.advancedButton13 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton12 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton11 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.advancedButton10 = new KGySoft.WinForms.Controls.AdvancedButton();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pnlTestArea.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.ucCaptionedContainer1.PanelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTestArea
            // 
            this.pnlTestArea.Controls.Add(this.groupBox2);
            this.pnlTestArea.Controls.Add(this.ucCaptionedContainer1);
            this.pnlTestArea.Controls.Add(this.groupBox1);
            this.pnlTestArea.Size = new System.Drawing.Size(407, 519);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox1, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.ucCaptionedContainer1, 0);
            this.pnlTestArea.Controls.SetChildIndex(this.groupBox2, 0);
            // 
            // lblInstuction
            // 
            this.lblInstruction.Size = new System.Drawing.Size(407, 144);
            this.lblInstruction.Text = resources.GetString("lblInstuction.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.advancedButton3);
            this.groupBox1.Controls.Add(this.advancedButton2);
            this.groupBox1.Controls.Add(this.advancedButton1);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 117);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Image in FlatStyle=System mode";
            // 
            // advancedButton3
            // 
            this.advancedButton3.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedButton3.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.advancedButton3.IsElevated = true;
            this.advancedButton3.Location = new System.Drawing.Point(3, 85);
            this.advancedButton3.Name = "advancedButton3";
            this.advancedButton3.Size = new System.Drawing.Size(401, 23);
            this.advancedButton3.TabIndex = 3;
            this.advancedButton3.Text = "Elevated mode (FlatStyle = Standard)";
            this.advancedButton3.UseVisualStyleBackColor = true;
            // 
            // advancedButton2
            // 
            this.advancedButton2.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.advancedButton2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.advancedButton2.IsElevated = true;
            this.advancedButton2.Location = new System.Drawing.Point(3, 62);
            this.advancedButton2.Name = "advancedButton2";
            this.advancedButton2.Size = new System.Drawing.Size(401, 23);
            this.advancedButton2.TabIndex = 2;
            this.advancedButton2.Text = "Elevated mode (FlatStyle = System)";
            this.advancedButton2.UseVisualStyleBackColor = true;
            // 
            // advancedButton1
            // 
            this.advancedButton1.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.advancedButton1.Image = ((System.Drawing.Image)(resources.GetObject("advancedButton1.Image")));
            this.advancedButton1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.advancedButton1.Location = new System.Drawing.Point(3, 39);
            this.advancedButton1.Name = "advancedButton1";
            this.advancedButton1.Size = new System.Drawing.Size(401, 23);
            this.advancedButton1.TabIndex = 1;
            this.advancedButton1.Text = "AdvancedButton: FlatStyle is System (in Vista and above) and Image is visible";
            this.advancedButton1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.Location = new System.Drawing.Point(3, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(401, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Standard button: Image is invisible";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.advancedButton7);
            this.groupBox2.Controls.Add(this.advancedButton8);
            this.groupBox2.Controls.Add(this.advancedButton9);
            this.groupBox2.Controls.Add(this.advancedButton6);
            this.groupBox2.Controls.Add(this.advancedButton5);
            this.groupBox2.Controls.Add(this.advancedButton4);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 269);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(407, 110);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "TextRenderingQuality";
            // 
            // advancedButton7
            // 
            this.advancedButton7.Location = new System.Drawing.Point(152, 77);
            this.advancedButton7.Name = "advancedButton7";
            this.advancedButton7.Size = new System.Drawing.Size(140, 23);
            this.advancedButton7.TabIndex = 5;
            this.advancedButton7.Text = "High Quality, GDI+";
            this.advancedButton7.TextRenderingQuality = KGySoft.WinForms.Controls.RenderingQuality.High;
            this.advancedButton7.UseCompatibleTextRendering = true;
            this.advancedButton7.UseVisualStyleBackColor = true;
            // 
            // advancedButton8
            // 
            this.advancedButton8.Location = new System.Drawing.Point(152, 48);
            this.advancedButton8.Name = "advancedButton8";
            this.advancedButton8.Size = new System.Drawing.Size(140, 23);
            this.advancedButton8.TabIndex = 4;
            this.advancedButton8.Text = "Default Quality, GDI+";
            this.advancedButton8.UseCompatibleTextRendering = true;
            this.advancedButton8.UseVisualStyleBackColor = true;
            // 
            // advancedButton9
            // 
            this.advancedButton9.Location = new System.Drawing.Point(152, 19);
            this.advancedButton9.Name = "advancedButton9";
            this.advancedButton9.Size = new System.Drawing.Size(140, 23);
            this.advancedButton9.TabIndex = 3;
            this.advancedButton9.Text = "Low Quality, GDI+";
            this.advancedButton9.TextRenderingQuality = KGySoft.WinForms.Controls.RenderingQuality.Low;
            this.advancedButton9.UseCompatibleTextRendering = true;
            this.advancedButton9.UseVisualStyleBackColor = true;
            // 
            // advancedButton6
            // 
            this.advancedButton6.Location = new System.Drawing.Point(6, 77);
            this.advancedButton6.Name = "advancedButton6";
            this.advancedButton6.Size = new System.Drawing.Size(140, 23);
            this.advancedButton6.TabIndex = 2;
            this.advancedButton6.Text = "High Quality, GDI";
            this.advancedButton6.TextRenderingQuality = KGySoft.WinForms.Controls.RenderingQuality.High;
            this.advancedButton6.UseVisualStyleBackColor = true;
            // 
            // advancedButton5
            // 
            this.advancedButton5.Location = new System.Drawing.Point(6, 48);
            this.advancedButton5.Name = "advancedButton5";
            this.advancedButton5.Size = new System.Drawing.Size(140, 23);
            this.advancedButton5.TabIndex = 1;
            this.advancedButton5.Text = "Default Quality, GDI";
            this.advancedButton5.UseVisualStyleBackColor = true;
            // 
            // advancedButton4
            // 
            this.advancedButton4.Location = new System.Drawing.Point(6, 19);
            this.advancedButton4.Name = "advancedButton4";
            this.advancedButton4.Size = new System.Drawing.Size(140, 23);
            this.advancedButton4.TabIndex = 0;
            this.advancedButton4.Text = "Low Quality, GDI";
            this.advancedButton4.TextRenderingQuality = KGySoft.WinForms.Controls.RenderingQuality.Low;
            this.advancedButton4.UseVisualStyleBackColor = true;
            // 
            // ucCaptionedContainer1
            // 
            this.ucCaptionedContainer1.Caption = "Fading animations (Vista and above), disabled colors (uncheck to disable)";
            this.ucCaptionedContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.ucCaptionedContainer1.Location = new System.Drawing.Point(0, 144);
            this.ucCaptionedContainer1.Name = "ucCaptionedContainer1";
            // 
            // ucCaptionedContainer1.ContentPanel
            // 
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.advancedButton13);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.advancedButton12);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.advancedButton11);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.advancedButton10);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.button5);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.button4);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.button3);
            this.ucCaptionedContainer1.PanelContent.Controls.Add(this.button2);
            this.ucCaptionedContainer1.ShowCheckBox = true;
            this.ucCaptionedContainer1.Size = new System.Drawing.Size(407, 125);
            this.ucCaptionedContainer1.TabIndex = 2;
            // 
            // advancedButton13
            // 
            this.advancedButton13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.advancedButton13.Location = new System.Drawing.Point(187, 75);
            this.advancedButton13.Name = "advancedButton13";
            this.advancedButton13.Size = new System.Drawing.Size(199, 23);
            this.advancedButton13.TabIndex = 7;
            this.advancedButton13.Text = "AdvancedButton FlatStyle=Flat";
            this.advancedButton13.UseVisualStyleBackColor = true;
            // 
            // advancedButton12
            // 
            this.advancedButton12.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.advancedButton12.Location = new System.Drawing.Point(187, 51);
            this.advancedButton12.Name = "advancedButton12";
            this.advancedButton12.Size = new System.Drawing.Size(199, 23);
            this.advancedButton12.TabIndex = 6;
            this.advancedButton12.Text = "AdvancedButton FlatStyle=Popup";
            this.advancedButton12.UseVisualStyleBackColor = true;
            // 
            // advancedButton11
            // 
            this.advancedButton11.Location = new System.Drawing.Point(187, 27);
            this.advancedButton11.Name = "advancedButton11";
            this.advancedButton11.Size = new System.Drawing.Size(199, 23);
            this.advancedButton11.TabIndex = 5;
            this.advancedButton11.Text = "AdvancedButton FlatStyle=Standard";
            this.advancedButton11.UseVisualStyleBackColor = true;
            // 
            // advancedButton10
            // 
            this.advancedButton10.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.advancedButton10.Location = new System.Drawing.Point(187, 3);
            this.advancedButton10.Name = "advancedButton10";
            this.advancedButton10.Size = new System.Drawing.Size(199, 23);
            this.advancedButton10.TabIndex = 4;
            this.advancedButton10.Text = "AdvancedButton FlatStyle=System";
            this.advancedButton10.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Location = new System.Drawing.Point(7, 75);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(146, 23);
            this.button5.TabIndex = 3;
            this.button5.Text = "Button FlatStyle=Flat";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(7, 51);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(146, 23);
            this.button4.TabIndex = 2;
            this.button4.Text = "Button FlatStyle=Popup";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(7, 27);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(146, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Button FlatStyle=Standard";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(7, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(146, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "Button FlatStyle=System";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // frmAdvancedButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 519);
            this.Name = "frmAdvancedButton";
            this.Text = "frmAdvancedButton";
            this.pnlTestArea.ResumeLayout(false);
            this.pnlTestArea.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ucCaptionedContainer1.PanelContent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox2;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton7;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton8;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton9;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton6;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton5;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton4;
        private KGySoft.WinForms.Controls.ucCaptionedContainer ucCaptionedContainer1;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton13;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton12;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton11;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton10;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton3;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton2;
        private KGySoft.WinForms.Controls.AdvancedButton advancedButton1;

    }
}