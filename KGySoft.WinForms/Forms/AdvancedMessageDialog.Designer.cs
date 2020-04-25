using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Forms
{
    partial class AdvancedMessageDialog
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
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.pnlDetails = new AdvancedPanel();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.pnlDetailsHeader = new System.Windows.Forms.Panel();
            this.lblDetails = new System.Windows.Forms.Label();
            this.splitter = new System.Windows.Forms.Splitter();
            this.pnlMessage = new AdvancedPanel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.pnlMessageHeader = new System.Windows.Forms.Panel();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnDetails = new System.Windows.Forms.Button();
            this.pnlSidePadding = new System.Windows.Forms.Panel();
            this.pnlImage = new AdvancedPanel();
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.pnlErrorButtons = new System.Windows.Forms.Panel();
            this.btnCloseApp = new System.Windows.Forms.Button();
            this.btnIgnore = new System.Windows.Forms.Button();
            this.btnSendReport = new System.Windows.Forms.Button();
            this.pnlStandardButtons = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBackground.SuspendLayout();
            this.pnlDetails.SuspendLayout();
            this.pnlDetailsHeader.SuspendLayout();
            this.pnlMessage.SuspendLayout();
            this.pnlMessageHeader.SuspendLayout();
            this.pnlImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.pnlErrorButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlBackground.Controls.Add(this.pnlDetails);
            this.pnlBackground.Controls.Add(this.pnlDetailsHeader);
            this.pnlBackground.Controls.Add(this.splitter);
            this.pnlBackground.Controls.Add(this.pnlMessage);
            this.pnlBackground.Controls.Add(this.pnlMessageHeader);
            this.pnlBackground.Controls.Add(this.pnlSidePadding);
            this.pnlBackground.Controls.Add(this.pnlImage);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(5);
            this.pnlBackground.Size = new System.Drawing.Size(514, 222);
            this.pnlBackground.TabIndex = 2;
            // 
            // pnlDetails
            // 
            this.pnlDetails.BorderStyle = AdvancedBorderStyle.SunkenFrame;
            this.pnlDetails.Controls.Add(this.txtDetails);
            this.pnlDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetails.Location = new System.Drawing.Point(145, 123);
            this.pnlDetails.Name = "pnlDetails";
            this.pnlDetails.Size = new System.Drawing.Size(360, 90);
            this.pnlDetails.TabIndex = 9;
            // 
            // txtDetails
            // 
            this.txtDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.txtDetails.Location = new System.Drawing.Point(0, 0);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDetails.Size = new System.Drawing.Size(356, 86);
            this.txtDetails.TabIndex = 2;
            this.txtDetails.WordWrap = false;
            // 
            // pnlDetailsHeader
            // 
            this.pnlDetailsHeader.Controls.Add(this.lblDetails);
            this.pnlDetailsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailsHeader.Location = new System.Drawing.Point(145, 103);
            this.pnlDetailsHeader.Name = "pnlDetailsHeader";
            this.pnlDetailsHeader.Size = new System.Drawing.Size(360, 20);
            this.pnlDetailsHeader.TabIndex = 10;
            // 
            // lblDetails
            // 
            this.lblDetails.AutoSize = true;
            this.lblDetails.Location = new System.Drawing.Point(3, 5);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(89, 13);
            this.lblDetails.TabIndex = 4;
            this.lblDetails.Text = "Details:__Dialogs";
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter.Location = new System.Drawing.Point(145, 100);
            this.splitter.MinExtra = 50;
            this.splitter.MinSize = 50;
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(360, 3);
            this.splitter.TabIndex = 11;
            this.splitter.TabStop = false;
            // 
            // pnlMessage
            // 
            this.pnlMessage.BorderStyle = AdvancedBorderStyle.SunkenFrame;
            this.pnlMessage.Controls.Add(this.txtMessage);
            this.pnlMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMessage.Location = new System.Drawing.Point(145, 25);
            this.pnlMessage.Name = "pnlMessage";
            this.pnlMessage.Size = new System.Drawing.Size(360, 75);
            this.pnlMessage.TabIndex = 7;
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.txtMessage.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtMessage.Location = new System.Drawing.Point(0, 0);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(356, 71);
            this.txtMessage.TabIndex = 1;
            // 
            // pnlMessageHeader
            // 
            this.pnlMessageHeader.Controls.Add(this.lblMessage);
            this.pnlMessageHeader.Controls.Add(this.btnDetails);
            this.pnlMessageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMessageHeader.Location = new System.Drawing.Point(145, 5);
            this.pnlMessageHeader.Name = "pnlMessageHeader";
            this.pnlMessageHeader.Size = new System.Drawing.Size(360, 20);
            this.pnlMessageHeader.TabIndex = 6;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(3, 5);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(146, 13);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "Message Summary:__Dialogs";
            // 
            // btnDetails
            // 
            this.btnDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetails.Location = new System.Drawing.Point(229, 0);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(128, 19);
            this.btnDetails.TabIndex = 0;
            this.btnDetails.Text = "Show Details__Dialogs";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // pnlSidePadding
            // 
            this.pnlSidePadding.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidePadding.Location = new System.Drawing.Point(137, 5);
            this.pnlSidePadding.Name = "pnlSidePadding";
            this.pnlSidePadding.Size = new System.Drawing.Size(8, 208);
            this.pnlSidePadding.TabIndex = 8;
            // 
            // pnlImage
            // 
            this.pnlImage.BorderStyle = AdvancedBorderStyle.SunkenFrame;
            this.pnlImage.Controls.Add(this.pbImage);
            this.pnlImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlImage.Location = new System.Drawing.Point(5, 5);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(132, 208);
            this.pnlImage.TabIndex = 5;
            // 
            // pbImage
            // 
            this.pbImage.Location = new System.Drawing.Point(0, 35);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(128, 128);
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbImage.TabIndex = 0;
            this.pbImage.TabStop = false;
            // 
            // pnlErrorButtons
            // 
            this.pnlErrorButtons.Controls.Add(this.btnCloseApp);
            this.pnlErrorButtons.Controls.Add(this.btnIgnore);
            this.pnlErrorButtons.Controls.Add(this.btnSendReport);
            this.pnlErrorButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlErrorButtons.Location = new System.Drawing.Point(0, 267);
            this.pnlErrorButtons.Name = "pnlErrorButtons";
            this.pnlErrorButtons.Size = new System.Drawing.Size(514, 51);
            this.pnlErrorButtons.TabIndex = 1;
            // 
            // btnCloseApp
            // 
            this.btnCloseApp.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCloseApp.Location = new System.Drawing.Point(178, 11);
            this.btnCloseApp.Name = "btnCloseApp";
            this.btnCloseApp.Size = new System.Drawing.Size(160, 28);
            this.btnCloseApp.TabIndex = 1;
            this.btnCloseApp.Text = "Close application__Dialogs";
            this.btnCloseApp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCloseApp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCloseApp.UseVisualStyleBackColor = true;
            this.btnCloseApp.Click += new System.EventHandler(this.btnCloseApp_Click);
            // 
            // btnIgnore
            // 
            this.btnIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIgnore.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnIgnore.Location = new System.Drawing.Point(343, 11);
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.Size = new System.Drawing.Size(160, 28);
            this.btnIgnore.TabIndex = 2;
            this.btnIgnore.Text = "Continue work__Dialogs";
            this.btnIgnore.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnIgnore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnIgnore.UseVisualStyleBackColor = true;
            this.btnIgnore.Click += new System.EventHandler(this.btnIgnore_Click);
            // 
            // btnSendReport
            // 
            this.btnSendReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendReport.Location = new System.Drawing.Point(13, 11);
            this.btnSendReport.Name = "btnSendReport";
            this.btnSendReport.Size = new System.Drawing.Size(160, 28);
            this.btnSendReport.TabIndex = 0;
            this.btnSendReport.Text = "Send error report__Dialogs";
            this.btnSendReport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSendReport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSendReport.UseVisualStyleBackColor = true;
            this.btnSendReport.Click += new System.EventHandler(this.btnSendReport_Click);
            // 
            // pnlStandardButtons
            // 
            this.pnlStandardButtons.ColumnCount = 1;
            this.pnlStandardButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 725F));
            this.pnlStandardButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 705F));
            this.pnlStandardButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStandardButtons.Location = new System.Drawing.Point(0, 222);
            this.pnlStandardButtons.Name = "pnlStandardButtons";
            this.pnlStandardButtons.RowCount = 1;
            this.pnlStandardButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlStandardButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlStandardButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlStandardButtons.Size = new System.Drawing.Size(514, 45);
            this.pnlStandardButtons.TabIndex = 0;
            // 
            // AdvancedMessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CancelButton = this.btnIgnore;
            this.ClientSize = new System.Drawing.Size(514, 318);
            this.Controls.Add(this.pnlBackground);
            this.Controls.Add(this.pnlStandardButtons);
            this.Controls.Add(this.pnlErrorButtons);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 350);
            this.Name = "AdvancedMessageDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "";
            this.TopMost = true;
            this.TranslateControls = true;
            this.pnlBackground.ResumeLayout(false);
            this.pnlDetails.ResumeLayout(false);
            this.pnlDetails.PerformLayout();
            this.pnlDetailsHeader.ResumeLayout(false);
            this.pnlDetailsHeader.PerformLayout();
            this.pnlMessage.ResumeLayout(false);
            this.pnlMessage.PerformLayout();
            this.pnlMessageHeader.ResumeLayout(false);
            this.pnlMessageHeader.PerformLayout();
            this.pnlImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.pnlErrorButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel pnlBackground;
        internal System.Windows.Forms.Button btnDetails;
        internal System.Windows.Forms.TextBox txtMessage;
        internal System.Windows.Forms.Label lblDetails;
        internal System.Windows.Forms.Label lblMessage;
        internal System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Panel pnlErrorButtons;
        internal System.Windows.Forms.Button btnCloseApp;
        internal System.Windows.Forms.Button btnIgnore;
        internal System.Windows.Forms.Button btnSendReport;
        private System.Windows.Forms.TableLayoutPanel pnlStandardButtons;
        private AdvancedPanel pnlImage;
        private AdvancedPanel pnlMessage;
        private System.Windows.Forms.Panel pnlMessageHeader;
        private System.Windows.Forms.Panel pnlDetailsHeader;
        private AdvancedPanel pnlDetails;
        private System.Windows.Forms.Panel pnlSidePadding;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.PictureBox pbImage;
    }
}
