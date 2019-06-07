using System.Windows.Forms;
namespace KGySoft.Controls
{
    partial class ucAllInvertNone
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
			this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
			this.buttonNone = new System.Windows.Forms.Button();
			this.buttonInvert = new System.Windows.Forms.Button();
			this.buttonAll = new System.Windows.Forms.Button();
			this.pnlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlButtons
			// 
			this.pnlButtons.ColumnCount = 3;
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.pnlButtons.Controls.Add(this.buttonNone, 2, 0);
			this.pnlButtons.Controls.Add(this.buttonInvert, 1, 0);
			this.pnlButtons.Controls.Add(this.buttonAll, 0, 0);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlButtons.Location = new System.Drawing.Point(0, 0);
			this.pnlButtons.Margin = new System.Windows.Forms.Padding(0);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.RowCount = 1;
			this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.pnlButtons.Size = new System.Drawing.Size(77, 27);
			this.pnlButtons.TabIndex = 0;
			// 
			// buttonNone
			// 
			this.buttonNone.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonNone.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.buttonNone.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonNone.Location = new System.Drawing.Point(53, 3);
			this.buttonNone.Margin = new System.Windows.Forms.Padding(0);
			this.buttonNone.Name = "buttonNone";
			this.buttonNone.Size = new System.Drawing.Size(21, 21);
			this.buttonNone.TabIndex = 2;
			this.buttonNone.UseVisualStyleBackColor = true;
			// 
			// buttonInvert
			// 
			this.buttonInvert.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonInvert.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.buttonInvert.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonInvert.Location = new System.Drawing.Point(27, 3);
			this.buttonInvert.Margin = new System.Windows.Forms.Padding(0);
			this.buttonInvert.Name = "buttonInvert";
			this.buttonInvert.Size = new System.Drawing.Size(21, 21);
			this.buttonInvert.TabIndex = 1;
			this.buttonInvert.UseVisualStyleBackColor = true;
			// 
			// buttonAll
			// 
			this.buttonAll.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.buttonAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonAll.Location = new System.Drawing.Point(2, 3);
			this.buttonAll.Margin = new System.Windows.Forms.Padding(0);
			this.buttonAll.Name = "buttonAll";
			this.buttonAll.Size = new System.Drawing.Size(21, 21);
			this.buttonAll.TabIndex = 0;
			this.buttonAll.UseVisualStyleBackColor = true;
			// 
			// ucAllInvertNone
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlButtons);
			this.Name = "ucAllInvertNone";
			this.Size = new System.Drawing.Size(77, 27);
			this.pnlButtons.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlButtons;
        private Button buttonAll;
        private Button buttonNone;
        private Button buttonInvert;
    }
}
