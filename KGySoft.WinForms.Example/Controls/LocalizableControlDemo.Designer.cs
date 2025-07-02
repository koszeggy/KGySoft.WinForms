namespace KGySoft.WinForms.Example.Controls
{
    partial class LocalizableControlDemo
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblLocalizableControlCaption = new KGySoft.WinForms.Controls.AdvancedLabel();
            btnLocalizableControl = new KGySoft.WinForms.Controls.CommandLinkButton();
            SuspendLayout();
            // 
            // lblLocalizableControlCaption
            // 
            lblLocalizableControlCaption.AutoSize = true;
            lblLocalizableControlCaption.Dock = System.Windows.Forms.DockStyle.Top;
            lblLocalizableControlCaption.Location = new System.Drawing.Point(10, 20);
            lblLocalizableControlCaption.Name = "lblLocalizableControlCaption";
            lblLocalizableControlCaption.Size = new System.Drawing.Size(194, 15);
            lblLocalizableControlCaption.TabIndex = 0;
            lblLocalizableControlCaption.Text = "lblLocalizableControlCaption";
            // 
            // btnLocalizableControl
            // 
            btnLocalizableControl.Description = null;
            btnLocalizableControl.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLocalizableControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLocalizableControl.Image = null;
            btnLocalizableControl.Location = new System.Drawing.Point(10, 35);
            btnLocalizableControl.Name = "btnLocalizableControl";
            btnLocalizableControl.Size = new System.Drawing.Size(194, 33);
            btnLocalizableControl.TabIndex = 1;
            btnLocalizableControl.Text = "btnLocalizableControl";
            btnLocalizableControl.UseVisualStyleBackColor = true;
            // 
            // LocalizableControlDemo
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.Add(btnLocalizableControl);
            Controls.Add(lblLocalizableControlCaption);
            Name = "LocalizableControlDemo";
            Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            Size = new System.Drawing.Size(214, 78);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private WinForms.Controls.AdvancedLabel lblLocalizableControlCaption;
        private WinForms.Controls.CommandLinkButton btnLocalizableControl;
    }
}
