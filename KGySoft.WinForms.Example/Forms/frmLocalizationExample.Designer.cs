namespace KGySoft.WinForms.Example.Forms
{
    partial class frmLocalizationExample
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
            localizableControlDemo1 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            localizableControlDemo2 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            localizableControlDemo3 = new KGySoft.WinForms.Example.Controls.LocalizableControlDemo();
            pnlTestArea.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTestArea
            // 
            pnlTestArea.Controls.Add(localizableControlDemo3);
            pnlTestArea.Controls.Add(localizableControlDemo2);
            pnlTestArea.Controls.Add(localizableControlDemo1);
            pnlTestArea.Size = new System.Drawing.Size(548, 450);
            pnlTestArea.Controls.SetChildIndex(lblInstruction, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo1, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo2, 0);
            pnlTestArea.Controls.SetChildIndex(localizableControlDemo3, 0);
            // 
            // lblInstruction
            // 
            lblInstruction.Size = new System.Drawing.Size(548, 30);
            // 
            // localizableControlDemo1
            // 
            localizableControlDemo1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo1.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo1.Location = new System.Drawing.Point(0, 30);
            localizableControlDemo1.Name = "localizableControlDemo1";
            localizableControlDemo1.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo1.Size = new System.Drawing.Size(548, 110);
            localizableControlDemo1.TabIndex = 1;
            localizableControlDemo1.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // localizableControlDemo2
            // 
            localizableControlDemo2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo2.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo2.DynamicStringLocalization = DynamicStringLocalization.LocalScope;
            localizableControlDemo2.Location = new System.Drawing.Point(0, 140);
            localizableControlDemo2.Name = "localizableControlDemo2";
            localizableControlDemo2.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo2.Size = new System.Drawing.Size(548, 110);
            localizableControlDemo2.TabIndex = 2;
            localizableControlDemo2.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // localizableControlDemo3
            // 
            localizableControlDemo3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localizableControlDemo3.Dock = System.Windows.Forms.DockStyle.Top;
            localizableControlDemo3.DynamicStringLocalization = DynamicStringLocalization.AssemblyScope;
            localizableControlDemo3.Location = new System.Drawing.Point(0, 250);
            localizableControlDemo3.Name = "localizableControlDemo3";
            localizableControlDemo3.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            localizableControlDemo3.Size = new System.Drawing.Size(548, 110);
            localizableControlDemo3.TabIndex = 3;
            localizableControlDemo3.DynamicStringLocalizationChanged += localizableControlDemo_DynamicStringLocalizationChanged;
            // 
            // frmLocalizationExample
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            DynamicStringLocalization = DynamicStringLocalization.LocalScope;
            Name = "frmLocalizationExample";
            Text = "frmLocalizationExample";
            pnlTestArea.ResumeLayout(false);
            pnlTestArea.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Controls.LocalizableControlDemo localizableControlDemo2;
        private Controls.LocalizableControlDemo localizableControlDemo1;
        private Controls.LocalizableControlDemo localizableControlDemo3;
    }
}