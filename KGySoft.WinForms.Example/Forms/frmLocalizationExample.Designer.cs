namespace KGySoft.WinForms.Example.Forms
{
    partial class frmLocalizationExample
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLocalizationExample));
            localizableControlDemo1 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            localizableControlDemo2 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            localizableControlDemo3 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            tsMenu = new System.Windows.Forms.ToolStrip();
            chbFilter = new System.Windows.Forms.ToolStripButton();
            lblLanguage = new System.Windows.Forms.ToolStripLabel();
            cmbLanguages = new System.Windows.Forms.ToolStripComboBox();
            btnApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            chbCustom = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            btnEdit = new System.Windows.Forms.ToolStripButton();
            pnlTestArea.SuspendLayout();
            tsMenu.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTestArea
            // 
            pnlTestArea.Controls.Add(localizableControlDemo3);
            pnlTestArea.Controls.Add(localizableControlDemo2);
            pnlTestArea.Controls.Add(localizableControlDemo1);
            pnlTestArea.Controls.Add(tsMenu);
            pnlTestArea.Size = new System.Drawing.Size(532, 511);
            pnlTestArea.Controls.SetChildIndex(lblInstruction, 0);
            pnlTestArea.Controls.SetChildIndex(tsMenu, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo1, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo2, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo3, 0);
            // 
            // lblInstruction
            // 
            lblInstruction.Size = new System.Drawing.Size(532, 30);
            // 
            // localizableControlDemo1
            // 
            localizableControlDemo1.AutoSize = true;
            localizableControlDemo1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            localizableControlDemo1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo1.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo1.Location = new System.Drawing.Point(0, 55);
            localizableControlDemo1.Name = "localizableControlDemo1";
            localizableControlDemo1.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo1.Size = new System.Drawing.Size(532, 80);
            localizableControlDemo1.TabIndex = 1;
            localizableControlDemo1.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // localizableControlDemo2
            // 
            localizableControlDemo2.AutoSize = true;
            localizableControlDemo2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            localizableControlDemo2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo2.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo2.DynamicStringLocalization = DynamicStringLocalization.LocalScope;
            localizableControlDemo2.Location = new System.Drawing.Point(0, 135);
            localizableControlDemo2.Name = "localizableControlDemo2";
            localizableControlDemo2.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo2.Size = new System.Drawing.Size(532, 80);
            localizableControlDemo2.TabIndex = 2;
            localizableControlDemo2.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // localizableControlDemo3
            // 
            localizableControlDemo3.AutoSize = true;
            localizableControlDemo3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            localizableControlDemo3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo3.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo3.DynamicStringLocalization = DynamicStringLocalization.AssemblyScope;
            localizableControlDemo3.Location = new System.Drawing.Point(0, 215);
            localizableControlDemo3.Name = "localizableControlDemo3";
            localizableControlDemo3.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo3.Size = new System.Drawing.Size(532, 80);
            localizableControlDemo3.TabIndex = 3;
            localizableControlDemo3.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // tsMenu
            // 
            tsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { chbFilter, lblLanguage, cmbLanguages, btnApply, toolStripSeparator1, btnEdit, toolStripSeparator2, chbCustom });
            tsMenu.Location = new System.Drawing.Point(0, 30);
            tsMenu.Name = "tsMenu";
            tsMenu.Size = new System.Drawing.Size(532, 25);
            tsMenu.TabIndex = 5;
            // 
            // chbFilter
            // 
            chbFilter.Checked = true;
            chbFilter.CheckOnClick = true;
            chbFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            chbFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            chbFilter.Image = (System.Drawing.Image)resources.GetObject("chbFilter.Image");
            chbFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            chbFilter.Name = "chbFilter";
            chbFilter.Size = new System.Drawing.Size(57, 22);
            chbFilter.Text = "chbFilter";
            // 
            // lblLanguage
            // 
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new System.Drawing.Size(72, 22);
            lblLanguage.Text = "lblLanguage";
            // 
            // cmbLanguages
            // 
            cmbLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLanguages.Name = "cmbLanguages";
            cmbLanguages.Size = new System.Drawing.Size(121, 25);
            // 
            // btnApply
            // 
            btnApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnApply.Image = (System.Drawing.Image)resources.GetObject("btnApply.Image");
            btnApply.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnApply.Name = "btnApply";
            btnApply.Size = new System.Drawing.Size(60, 22);
            btnApply.Text = "btnApply";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // chbCustom
            // 
            chbCustom.CheckOnClick = true;
            chbCustom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            chbCustom.Image = (System.Drawing.Image)resources.GetObject("chbCustom.Image");
            chbCustom.ImageTransparentColor = System.Drawing.Color.Magenta;
            chbCustom.Name = "chbCustom";
            chbCustom.Size = new System.Drawing.Size(73, 22);
            chbCustom.Text = "chbCustom";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnEdit
            // 
            btnEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnEdit.Image = (System.Drawing.Image)resources.GetObject("btnEdit.Image");
            btnEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(49, 22);
            btnEdit.Text = "btnEdit";
            // 
            // frmLocalizationExample
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 550);
            DynamicStringLocalization = DynamicStringLocalization.LocalScope;
            Name = "frmLocalizationExample";
            RightToLeftLayout = true;
            Text = "frmLocalizationExample";
            pnlTestArea.ResumeLayout(false);
            pnlTestArea.PerformLayout();
            tsMenu.ResumeLayout(false);
            tsMenu.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Controls.LocalizableControlDemo localizableControlDemo2;
        private Controls.LocalizableControlDemo localizableControlDemo1;
        private Controls.LocalizableControlDemo localizableControlDemo3;
        private System.Windows.Forms.ToolStrip tsMenu;
        private System.Windows.Forms.ToolStripLabel lblLanguage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton chbCustom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnEdit;
        private System.Windows.Forms.ToolStripComboBox cmbLanguages;
        private System.Windows.Forms.ToolStripButton btnApply;
        private System.Windows.Forms.ToolStripButton chbFilter;
    }
}