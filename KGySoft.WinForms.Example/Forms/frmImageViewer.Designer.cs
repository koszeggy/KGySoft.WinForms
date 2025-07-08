namespace KGySoft.WinForms.Example.Forms
{
    partial class frmImageViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new System.Windows.Forms.GroupBox();
            rbMetafile = new KGySoft.WinForms.Controls.AdvancedRadioButton();
            rbLargeBitmap = new KGySoft.WinForms.Controls.AdvancedRadioButton();
            rbSmallBitmap = new KGySoft.WinForms.Controls.AdvancedRadioButton();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            advancedLabel2 = new KGySoft.WinForms.Controls.AdvancedLabel();
            advancedLabel1 = new KGySoft.WinForms.Controls.AdvancedLabel();
            imageViewer = new KGySoft.WinForms.Controls.ImageViewer();
            pictureBox = new System.Windows.Forms.PictureBox();
            pnlTestArea.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // pnlTestArea
            // 
            pnlTestArea.Controls.Add(tableLayoutPanel1);
            pnlTestArea.Controls.Add(groupBox1);
            pnlTestArea.Size = new System.Drawing.Size(545, 450);
            pnlTestArea.Controls.SetChildIndex(lblInstruction, 0);
            pnlTestArea.Controls.SetChildIndex(groupBox1, 0);
            pnlTestArea.Controls.SetChildIndex(tableLayoutPanel1, 0);
            // 
            // lblInstuction
            // 
            lblInstruction.Size = new System.Drawing.Size(545, 68);
            lblInstruction.Text = "ImageViewer supports custom zooming and panning, as well as toggling smoothing (even for metafiles).\r\n"
                + "Use the predefined options or click the controls to set their properties.";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(rbMetafile);
            groupBox1.Controls.Add(rbLargeBitmap);
            groupBox1.Controls.Add(rbSmallBitmap);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox1.Location = new System.Drawing.Point(0, 68);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(545, 88);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Presets";
            // 
            // rbMetafile
            // 
            rbMetafile.AutoSize = true;
            rbMetafile.Dock = System.Windows.Forms.DockStyle.Top;
            rbMetafile.Location = new System.Drawing.Point(3, 57);
            rbMetafile.Name = "rbMetafile";
            rbMetafile.Size = new System.Drawing.Size(539, 19);
            rbMetafile.TabIndex = 2;
            rbMetafile.Text = "Metafile";
            rbMetafile.UseVisualStyleBackColor = true;
            rbMetafile.CheckedChanged += AdvancedRadioButton_CheckedChanged;
            // 
            // rbLargeBitmap
            // 
            rbLargeBitmap.AutoSize = true;
            rbLargeBitmap.Dock = System.Windows.Forms.DockStyle.Top;
            rbLargeBitmap.Location = new System.Drawing.Point(3, 38);
            rbLargeBitmap.Name = "rbLargeBitmap";
            rbLargeBitmap.Size = new System.Drawing.Size(539, 19);
            rbLargeBitmap.TabIndex = 1;
            rbLargeBitmap.Text = "Large Bitmap";
            rbLargeBitmap.UseVisualStyleBackColor = true;
            rbLargeBitmap.CheckedChanged += AdvancedRadioButton_CheckedChanged;
            // 
            // rbSmallBitmap
            // 
            rbSmallBitmap.AutoSize = true;
            rbSmallBitmap.Dock = System.Windows.Forms.DockStyle.Top;
            rbSmallBitmap.Location = new System.Drawing.Point(3, 19);
            rbSmallBitmap.Name = "rbSmallBitmap";
            rbSmallBitmap.Size = new System.Drawing.Size(539, 19);
            rbSmallBitmap.TabIndex = 0;
            rbSmallBitmap.Text = "Small Bitmap";
            rbSmallBitmap.UseVisualStyleBackColor = true;
            rbSmallBitmap.CheckedChanged += AdvancedRadioButton_CheckedChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(advancedLabel2, 1, 0);
            tableLayoutPanel1.Controls.Add(advancedLabel1, 0, 0);
            tableLayoutPanel1.Controls.Add(imageViewer, 1, 1);
            tableLayoutPanel1.Controls.Add(pictureBox, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 156);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(545, 294);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // advancedLabel2
            // 
            advancedLabel2.AutoSize = true;
            advancedLabel2.BorderStyle = WinForms.Controls.AdvancedBorderStyle.Sunken;
            advancedLabel2.Dock = System.Windows.Forms.DockStyle.Top;
            advancedLabel2.Location = new System.Drawing.Point(275, 0);
            advancedLabel2.Name = "advancedLabel2";
            advancedLabel2.Size = new System.Drawing.Size(267, 47);
            advancedLabel2.TabIndex = 5;
            advancedLabel2.Text = "KGy SOFT ImageViewer\r\nSee AutoZoom, Zoom, SmoothingEnabled.\r\nUse Ctrl+Mouse Wheel to zoom by the mouse";
            // 
            // advancedLabel1
            // 
            advancedLabel1.AutoSize = true;
            advancedLabel1.BorderStyle = WinForms.Controls.AdvancedBorderStyle.Sunken;
            advancedLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            advancedLabel1.Location = new System.Drawing.Point(3, 0);
            advancedLabel1.Name = "advancedLabel1";
            advancedLabel1.Size = new System.Drawing.Size(266, 47);
            advancedLabel1.TabIndex = 4;
            advancedLabel1.Text = "Windows Forms PictureBox\r\nSet the SizeMode property for the different options";
            // 
            // imageViewer
            // 
            imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            imageViewer.Location = new System.Drawing.Point(275, 50);
            imageViewer.Name = "imageViewer";
            imageViewer.Size = new System.Drawing.Size(267, 241);
            imageViewer.TabIndex = 3;
            imageViewer.Text = "imageViewer1";
            // 
            // pictureBox
            // 
            pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(3, 50);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(266, 241);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox.TabIndex = 2;
            pictureBox.TabStop = false;
            // 
            // frmImageViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(820, 450);
            Name = "frmImageViewer";
            Text = "frmImageViewer";
            pnlTestArea.ResumeLayout(false);
            pnlTestArea.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private KGySoft.WinForms.Controls.ImageViewer imageViewer;
        private System.Windows.Forms.PictureBox pictureBox;
        private KGySoft.WinForms.Controls.AdvancedLabel advancedLabel2;
        private KGySoft.WinForms.Controls.AdvancedLabel advancedLabel1;
        private KGySoft.WinForms.Controls.AdvancedRadioButton rbMetafile;
        private KGySoft.WinForms.Controls.AdvancedRadioButton rbLargeBitmap;
        private KGySoft.WinForms.Controls.AdvancedRadioButton rbSmallBitmap;
    }
}