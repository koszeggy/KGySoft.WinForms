namespace KGySoft.WinForms.Example.Forms
{
    partial class frmEditResources
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
            components = new System.ComponentModel.Container();
            gbResourceSetName = new System.Windows.Forms.GroupBox();
            cmbResourceFiles = new KGySoft.WinForms.Controls.AdvancedComboBox();
            gbResourceEntries = new System.Windows.Forms.GroupBox();
            gridResources = new System.Windows.Forms.DataGridView();
            colResourceKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colOriginalText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTranslatedText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            bindingSource = new System.Windows.Forms.BindingSource(components);
            gbResourceSetName.SuspendLayout();
            gbResourceEntries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridResources).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource).BeginInit();
            SuspendLayout();
            // 
            // gbResourceSetName
            // 
            gbResourceSetName.Controls.Add(cmbResourceFiles);
            gbResourceSetName.Dock = System.Windows.Forms.DockStyle.Top;
            gbResourceSetName.Location = new System.Drawing.Point(0, 0);
            gbResourceSetName.Name = "gbResourceSetName";
            gbResourceSetName.Size = new System.Drawing.Size(723, 52);
            gbResourceSetName.TabIndex = 1;
            gbResourceSetName.TabStop = false;
            gbResourceSetName.Text = "gbResourceSetName";
            // 
            // cmbResourceFiles
            // 
            cmbResourceFiles.Dock = System.Windows.Forms.DockStyle.Top;
            cmbResourceFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbResourceFiles.FormattingEnabled = true;
            cmbResourceFiles.Location = new System.Drawing.Point(3, 19);
            cmbResourceFiles.Name = "cmbResourceFiles";
            cmbResourceFiles.Size = new System.Drawing.Size(717, 23);
            cmbResourceFiles.TabIndex = 0;
            // 
            // gbResourceEntries
            // 
            gbResourceEntries.Controls.Add(gridResources);
            gbResourceEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            gbResourceEntries.Location = new System.Drawing.Point(0, 52);
            gbResourceEntries.Name = "gbResourceEntries";
            gbResourceEntries.Size = new System.Drawing.Size(723, 375);
            gbResourceEntries.TabIndex = 2;
            gbResourceEntries.TabStop = false;
            gbResourceEntries.Text = "gbResourceEntries";
            // 
            // gridResources
            // 
            gridResources.AllowUserToAddRows = false;
            gridResources.AllowUserToDeleteRows = false;
            gridResources.AutoGenerateColumns = false;
            gridResources.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResources.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colResourceKey, colOriginalText, colTranslatedText });
            gridResources.DataSource = bindingSource;
            gridResources.Dock = System.Windows.Forms.DockStyle.Fill;
            gridResources.Location = new System.Drawing.Point(3, 19);
            gridResources.Name = "gridResources";
            gridResources.Size = new System.Drawing.Size(717, 353);
            gridResources.TabIndex = 0;
            // 
            // colResourceKey
            // 
            colResourceKey.DataPropertyName = "Key";
            colResourceKey.HeaderText = "colResourceKey";
            colResourceKey.Name = "colResourceKey";
            colResourceKey.ReadOnly = true;
            colResourceKey.Width = 150;
            // 
            // colOriginalText
            // 
            colOriginalText.DataPropertyName = "OriginalText";
            colOriginalText.HeaderText = "colOriginalText";
            colOriginalText.Name = "colOriginalText";
            colOriginalText.ReadOnly = true;
            colOriginalText.Width = 250;
            // 
            // colTranslatedText
            // 
            colTranslatedText.DataPropertyName = "TranslatedText";
            colTranslatedText.HeaderText = "colTranslatedText";
            colTranslatedText.Name = "colTranslatedText";
            colTranslatedText.Width = 250;
            // 
            // bindingSource
            // 
            bindingSource.DataSource = typeof(ViewModel.ResourceEntry);
            // 
            // frmEditResources
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(723, 467);
            Controls.Add(gbResourceEntries);
            Controls.Add(gbResourceSetName);
            DynamicStringLocalization = DynamicStringLocalization.LocalScope;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            Name = "frmEditResources";
            RightToLeftLayout = true;
            ShowApplyButton = true;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "frmEditResources";
            Controls.SetChildIndex(gbResourceSetName, 0);
            Controls.SetChildIndex(gbResourceEntries, 0);
            gbResourceSetName.ResumeLayout(false);
            gbResourceEntries.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridResources).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gbResourceSetName;
        private WinForms.Controls.AdvancedComboBox cmbResourceFiles;
        private System.Windows.Forms.GroupBox gbResourceEntries;
        private System.Windows.Forms.DataGridView gridResources;
        private System.Windows.Forms.BindingSource bindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResourceKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOriginalText;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTranslatedText;
    }
}