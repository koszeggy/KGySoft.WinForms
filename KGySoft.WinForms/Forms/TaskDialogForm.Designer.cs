using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Forms
{
    partial class TaskDialogForm
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
            this.components = new System.ComponentModel.Container();
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.pnlCommandLinks = new System.Windows.Forms.Panel();
            this.pnlRadioButtons = new System.Windows.Forms.Panel();
            this.pnlProgressBar = new System.Windows.Forms.Panel();
            this.pbProgress = new KGySoft.WinForms.Controls.AdvancedProgressBar();
            this.pnlMainTexts = new System.Windows.Forms.Panel();
            this.lblDetailsMain = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.lblMessage = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.pnlMainInstruction = new KGySoft.WinForms.Forms.TaskDialogForm.MainInstructionPanel();
            this.lblMainInstruction = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.pnlMainIcon = new System.Windows.Forms.Panel();
            this.pnlMainIconBackground = new System.Windows.Forms.Panel();
            this.pbMainIcon = new System.Windows.Forms.PictureBox();
            this.pnlMainControls = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlChecks = new System.Windows.Forms.TableLayoutPanel();
            this.btnShowHideDetails = new KGySoft.WinForms.Forms.TaskDialogForm.ExpandoButton();
            this.cbCheckBox = new KGySoft.WinForms.Controls.AdvancedCheckBox();
            this.pnlDividerMainBottom = new System.Windows.Forms.Panel();
            this.pnlDividerControlsBottom = new System.Windows.Forms.Panel();
            this.pnlDividerFooterTop = new System.Windows.Forms.Panel();
            this.pnlFooter = new System.Windows.Forms.TableLayoutPanel();
            this.pnlFooterIcon = new System.Windows.Forms.Panel();
            this.pbFooterIcon = new System.Windows.Forms.PictureBox();
            this.lblFooter = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.pnlDividerFooterBottom = new System.Windows.Forms.Panel();
            this.pnlDividerDetailsFooterTop = new System.Windows.Forms.Panel();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.lblDetailsFooter = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.pnlMain.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.pnlProgressBar.SuspendLayout();
            this.pnlMainTexts.SuspendLayout();
            this.pnlMainInstruction.SuspendLayout();
            this.pnlMainIcon.SuspendLayout();
            this.pnlMainIconBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbMainIcon)).BeginInit();
            this.pnlMainControls.SuspendLayout();
            this.pnlChecks.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlFooterIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFooterIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.ColumnCount = 2;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Controls.Add(this.pnlMainContent, 1, 0);
            this.pnlMain.Controls.Add(this.pnlMainIcon, 0, 0);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 1;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Size = new System.Drawing.Size(359, 170);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.pnlCommandLinks);
            this.pnlMainContent.Controls.Add(this.pnlRadioButtons);
            this.pnlMainContent.Controls.Add(this.pnlProgressBar);
            this.pnlMainContent.Controls.Add(this.pnlMainTexts);
            this.pnlMainContent.Controls.Add(this.pnlMainInstruction);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(50, 0);
            this.pnlMainContent.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(309, 170);
            this.pnlMainContent.TabIndex = 3;
            // 
            // pnlCommandLinks
            // 
            this.pnlCommandLinks.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCommandLinks.Location = new System.Drawing.Point(0, 160);
            this.pnlCommandLinks.Name = "pnlCommandLinks";
            this.pnlCommandLinks.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlCommandLinks.Size = new System.Drawing.Size(309, 10);
            this.pnlCommandLinks.TabIndex = 7;
            // 
            // pnlRadioButtons
            // 
            this.pnlRadioButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRadioButtons.Location = new System.Drawing.Point(0, 150);
            this.pnlRadioButtons.Name = "pnlRadioButtons";
            this.pnlRadioButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlRadioButtons.Size = new System.Drawing.Size(309, 10);
            this.pnlRadioButtons.TabIndex = 6;
            // 
            // pnlProgressBar
            // 
            this.pnlProgressBar.Controls.Add(this.pbProgress);
            this.pnlProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProgressBar.Location = new System.Drawing.Point(0, 119);
            this.pnlProgressBar.Name = "pnlProgressBar";
            this.pnlProgressBar.Padding = new System.Windows.Forms.Padding(5);
            this.pnlProgressBar.Size = new System.Drawing.Size(309, 31);
            this.pnlProgressBar.TabIndex = 3;
            // 
            // pbProgress
            // 
            this.pbProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.pbProgress.Location = new System.Drawing.Point(5, 5);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.RightToLeftLayout = true;
            this.pbProgress.Size = new System.Drawing.Size(299, 15);
            this.pbProgress.TabIndex = 0;
            // 
            // pnlMainTexts
            // 
            this.pnlMainTexts.Controls.Add(this.lblDetailsMain);
            this.pnlMainTexts.Controls.Add(this.lblMessage);
            this.pnlMainTexts.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMainTexts.Location = new System.Drawing.Point(0, 49);
            this.pnlMainTexts.Name = "pnlMainTexts";
            this.pnlMainTexts.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.pnlMainTexts.Size = new System.Drawing.Size(309, 70);
            this.pnlMainTexts.TabIndex = 5;
            // 
            // lblDetailsMain
            // 
            this.lblDetailsMain.AutoSize = true;
            this.lblDetailsMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailsMain.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblDetailsMain.Location = new System.Drawing.Point(0, 33);
            this.lblDetailsMain.Name = "lblDetailsMain";
            this.lblDetailsMain.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblDetailsMain.Size = new System.Drawing.Size(309, 23);
            this.lblDetailsMain.TabIndex = 4;
            this.lblDetailsMain.Text = "lblDetailsMain";
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMessage.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblMessage.Location = new System.Drawing.Point(0, 10);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblMessage.Size = new System.Drawing.Size(309, 23);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "lblMessage";
            // 
            // pnlMainInstruction
            // 
            this.pnlMainInstruction.Controls.Add(this.lblMainInstruction);
            this.pnlMainInstruction.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMainInstruction.Location = new System.Drawing.Point(0, 0);
            this.pnlMainInstruction.Name = "pnlMainInstruction";
            this.pnlMainInstruction.Size = new System.Drawing.Size(309, 49);
            this.pnlMainInstruction.TabIndex = 4;
            // 
            // lblMainInstruction
            // 
            this.lblMainInstruction.AutoEllipsis = true;
            this.lblMainInstruction.AutoSize = true;
            this.lblMainInstruction.BackColor = System.Drawing.Color.Transparent;
            this.lblMainInstruction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMainInstruction.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblMainInstruction.Location = new System.Drawing.Point(0, 0);
            this.lblMainInstruction.Name = "lblMainInstruction";
            this.lblMainInstruction.Padding = new System.Windows.Forms.Padding(8);
            this.lblMainInstruction.Size = new System.Drawing.Size(309, 29);
            this.lblMainInstruction.TabIndex = 1;
            this.lblMainInstruction.Text = "lblMainInstruction";
            // 
            // pnlMainIcon
            // 
            this.pnlMainIcon.AutoSize = true;
            this.pnlMainIcon.Controls.Add(this.pnlMainIconBackground);
            this.pnlMainIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainIcon.Location = new System.Drawing.Point(0, 0);
            this.pnlMainIcon.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMainIcon.Name = "pnlMainIcon";
            this.pnlMainIcon.Size = new System.Drawing.Size(50, 170);
            this.pnlMainIcon.TabIndex = 2;
            // 
            // pnlMainIconBackground
            // 
            this.pnlMainIconBackground.Controls.Add(this.pbMainIcon);
            this.pnlMainIconBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMainIconBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlMainIconBackground.Name = "pnlMainIconBackground";
            this.pnlMainIconBackground.Size = new System.Drawing.Size(50, 49);
            this.pnlMainIconBackground.TabIndex = 1;
            // 
            // pbMainIcon
            // 
            this.pbMainIcon.Location = new System.Drawing.Point(8, 9);
            this.pbMainIcon.Name = "pbMainIcon";
            this.pbMainIcon.Size = new System.Drawing.Size(32, 32);
            this.pbMainIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbMainIcon.TabIndex = 0;
            this.pbMainIcon.TabStop = false;
            // 
            // pnlMainControls
            // 
            this.pnlMainControls.ColumnCount = 2;
            this.pnlMainControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.pnlMainControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMainControls.Controls.Add(this.pnlButtons, 1, 0);
            this.pnlMainControls.Controls.Add(this.pnlChecks, 0, 0);
            this.pnlMainControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMainControls.Location = new System.Drawing.Point(0, 171);
            this.pnlMainControls.Name = "pnlMainControls";
            this.pnlMainControls.RowCount = 1;
            this.pnlMainControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMainControls.Size = new System.Drawing.Size(359, 59);
            this.pnlMainControls.TabIndex = 2;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlButtons.Location = new System.Drawing.Point(350, 3);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(3);
            this.pnlButtons.Size = new System.Drawing.Size(6, 53);
            this.pnlButtons.TabIndex = 2;
            // 
            // pnlChecks
            // 
            this.pnlChecks.ColumnCount = 1;
            this.pnlChecks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlChecks.Controls.Add(this.btnShowHideDetails, 0, 0);
            this.pnlChecks.Controls.Add(this.cbCheckBox, 0, 1);
            this.pnlChecks.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlChecks.Location = new System.Drawing.Point(3, 3);
            this.pnlChecks.Name = "pnlChecks";
            this.pnlChecks.RowCount = 2;
            this.pnlChecks.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlChecks.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlChecks.Size = new System.Drawing.Size(174, 53);
            this.pnlChecks.TabIndex = 0;
            // 
            // btnShowHideDetails
            // 
            this.btnShowHideDetails.AutoSize = true;
            this.btnShowHideDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnShowHideDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnShowHideDetails.FadingAnimationsEnabled = false;
            this.btnShowHideDetails.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.btnShowHideDetails.Location = new System.Drawing.Point(3, 3);
            this.btnShowHideDetails.Name = "btnShowHideDetails";
            this.btnShowHideDetails.Size = new System.Drawing.Size(168, 24);
            this.btnShowHideDetails.TabIndex = 0;
            this.btnShowHideDetails.Text = "btnShowHideDetails";
            this.btnShowHideDetails.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // cbCheckBox
            // 
            this.cbCheckBox.AutoSize = true;
            this.cbCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbCheckBox.Location = new System.Drawing.Point(8, 33);
            this.cbCheckBox.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.cbCheckBox.Name = "cbCheckBox";
            this.cbCheckBox.Size = new System.Drawing.Size(163, 17);
            this.cbCheckBox.TabIndex = 1;
            this.cbCheckBox.Text = "cbCheckBox";
            this.cbCheckBox.UseVisualStyleBackColor = true;
            // 
            // pnlDividerMainBottom
            // 
            this.pnlDividerMainBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDividerMainBottom.Location = new System.Drawing.Point(0, 170);
            this.pnlDividerMainBottom.Name = "pnlDividerMainBottom";
            this.pnlDividerMainBottom.Size = new System.Drawing.Size(359, 1);
            this.pnlDividerMainBottom.TabIndex = 1;
            // 
            // pnlDividerControlsBottom
            // 
            this.pnlDividerControlsBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDividerControlsBottom.Location = new System.Drawing.Point(0, 230);
            this.pnlDividerControlsBottom.Name = "pnlDividerControlsBottom";
            this.pnlDividerControlsBottom.Size = new System.Drawing.Size(359, 1);
            this.pnlDividerControlsBottom.TabIndex = 3;
            // 
            // pnlDividerFooterTop
            // 
            this.pnlDividerFooterTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDividerFooterTop.Location = new System.Drawing.Point(0, 231);
            this.pnlDividerFooterTop.Name = "pnlDividerFooterTop";
            this.pnlDividerFooterTop.Size = new System.Drawing.Size(359, 1);
            this.pnlDividerFooterTop.TabIndex = 4;
            // 
            // pnlFooter
            // 
            this.pnlFooter.ColumnCount = 2;
            this.pnlFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.pnlFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlFooter.Controls.Add(this.pnlFooterIcon, 0, 0);
            this.pnlFooter.Controls.Add(this.lblFooter, 1, 0);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFooter.Location = new System.Drawing.Point(0, 232);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.RowCount = 1;
            this.pnlFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlFooter.Size = new System.Drawing.Size(359, 27);
            this.pnlFooter.TabIndex = 5;
            // 
            // pnlFooterIcon
            // 
            this.pnlFooterIcon.Controls.Add(this.pbFooterIcon);
            this.pnlFooterIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFooterIcon.Location = new System.Drawing.Point(0, 3);
            this.pnlFooterIcon.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.pnlFooterIcon.Name = "pnlFooterIcon";
            this.pnlFooterIcon.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlFooterIcon.Size = new System.Drawing.Size(24, 24);
            this.pnlFooterIcon.TabIndex = 0;
            // 
            // pbFooterIcon
            // 
            this.pbFooterIcon.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbFooterIcon.Location = new System.Drawing.Point(8, 4);
            this.pbFooterIcon.Name = "pbFooterIcon";
            this.pbFooterIcon.Size = new System.Drawing.Size(16, 16);
            this.pbFooterIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbFooterIcon.TabIndex = 0;
            this.pbFooterIcon.TabStop = false;
            // 
            // lblFooter
            // 
            this.lblFooter.AutoSize = true;
            this.lblFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFooter.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblFooter.Location = new System.Drawing.Point(27, 0);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Padding = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.lblFooter.Size = new System.Drawing.Size(329, 27);
            this.lblFooter.TabIndex = 1;
            this.lblFooter.Text = "lblFooter";
            // 
            // pnlDividerFooterBottom
            // 
            this.pnlDividerFooterBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDividerFooterBottom.Location = new System.Drawing.Point(0, 259);
            this.pnlDividerFooterBottom.Name = "pnlDividerFooterBottom";
            this.pnlDividerFooterBottom.Size = new System.Drawing.Size(359, 1);
            this.pnlDividerFooterBottom.TabIndex = 6;
            // 
            // pnlDividerDetailsFooterTop
            // 
            this.pnlDividerDetailsFooterTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDividerDetailsFooterTop.Location = new System.Drawing.Point(0, 260);
            this.pnlDividerDetailsFooterTop.Name = "pnlDividerDetailsFooterTop";
            this.pnlDividerDetailsFooterTop.Size = new System.Drawing.Size(359, 1);
            this.pnlDividerDetailsFooterTop.TabIndex = 7;
            // 
            // timer
            // 
            this.timer.Interval = 200;
            // 
            // lblDetailsFooter
            // 
            this.lblDetailsFooter.AutoSize = true;
            this.lblDetailsFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailsFooter.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.lblDetailsFooter.Location = new System.Drawing.Point(0, 261);
            this.lblDetailsFooter.Name = "lblDetailsFooter";
            this.lblDetailsFooter.Padding = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.lblDetailsFooter.Size = new System.Drawing.Size(359, 27);
            this.lblDetailsFooter.TabIndex = 8;
            this.lblDetailsFooter.Text = "lblDetailsFooter";
            // 
            // TaskDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(359, 296);
            this.Controls.Add(this.lblDetailsFooter);
            this.Controls.Add(this.pnlDividerDetailsFooterTop);
            this.Controls.Add(this.pnlDividerFooterBottom);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlDividerFooterTop);
            this.Controls.Add(this.pnlDividerControlsBottom);
            this.Controls.Add(this.pnlMainControls);
            this.Controls.Add(this.pnlDividerMainBottom);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "TaskDialogForm";
            this.RightToLeftLayout = true;
            this.Text = "TaskDialogForm";
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlMainContent.ResumeLayout(false);
            this.pnlProgressBar.ResumeLayout(false);
            this.pnlMainTexts.ResumeLayout(false);
            this.pnlMainTexts.PerformLayout();
            this.pnlMainInstruction.ResumeLayout(false);
            this.pnlMainInstruction.PerformLayout();
            this.pnlMainIcon.ResumeLayout(false);
            this.pnlMainIconBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbMainIcon)).EndInit();
            this.pnlMainControls.ResumeLayout(false);
            this.pnlMainControls.PerformLayout();
            this.pnlChecks.ResumeLayout(false);
            this.pnlChecks.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.pnlFooterIcon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbFooterIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlMain;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Panel pnlMainIcon;
        private System.Windows.Forms.PictureBox pbMainIcon;
        private System.Windows.Forms.Panel pnlProgressBar;
        private AdvancedProgressBar pbProgress;
        private System.Windows.Forms.Panel pnlMainTexts;
        private AdvancedLabel lblDetailsMain;
        private AdvancedLabel lblMessage;
        private MainInstructionPanel pnlMainInstruction;
        private AdvancedLabel lblMainInstruction;
        private System.Windows.Forms.Panel pnlMainIconBackground;
        private System.Windows.Forms.Panel pnlCommandLinks;
        private System.Windows.Forms.Panel pnlRadioButtons;
        private System.Windows.Forms.TableLayoutPanel pnlMainControls;
        private System.Windows.Forms.Panel pnlDividerMainBottom;
        private System.Windows.Forms.TableLayoutPanel pnlChecks;
        private ExpandoButton btnShowHideDetails;
        private AdvancedCheckBox cbCheckBox;
        private System.Windows.Forms.Panel pnlDividerControlsBottom;
        private System.Windows.Forms.Panel pnlDividerFooterTop;
        private System.Windows.Forms.TableLayoutPanel pnlFooter;
        private System.Windows.Forms.Panel pnlFooterIcon;
        private System.Windows.Forms.PictureBox pbFooterIcon;
        private System.Windows.Forms.Panel pnlDividerFooterBottom;
        private System.Windows.Forms.Panel pnlDividerDetailsFooterTop;
        private System.Windows.Forms.Timer timer;
        private AdvancedLabel lblDetailsFooter;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private AdvancedLabel lblFooter;



    }
}