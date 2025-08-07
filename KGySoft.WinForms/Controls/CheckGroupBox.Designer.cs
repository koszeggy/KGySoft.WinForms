namespace KGySoft.WinForms.Controls
{
    partial class CheckGroupBox
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
            this.checkBox = new KGySoft.WinForms.Controls.CheckGroupBox.GroupBoxCheckBox();
            this.contentPanel = new KGySoft.WinForms.Controls.CheckGroupBox.ContentPanel();
            this.SuspendLayout();
            // 
            // checkBox
            // 
            this.checkBox.AutoSize = true;
            this.checkBox.Checked = true;
            this.checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox.DisabledBackColor = System.Drawing.Color.Transparent;
            this.checkBox.EnabledBackColor = System.Drawing.Color.Transparent;
            this.checkBox.Location = new System.Drawing.Point(0, 0);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(104, 24);
            this.checkBox.TabIndex = 0;
            this.checkBox.UseVisualStyleBackColor = false;
            // 
            // contentPanel
            // 
            this.contentPanel.BackColor = System.Drawing.Color.Transparent;
            this.contentPanel.Location = new System.Drawing.Point(0, 0);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(200, 100);
            this.contentPanel.TabIndex = 1;
            this.ResumeLayout(false);

        }
        #endregion

        private GroupBoxCheckBox checkBox;
        private ContentPanel contentPanel;
    }
}