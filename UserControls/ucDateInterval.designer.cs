namespace KGySoft.Controls
{
    partial class ucDateInterval
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.dtpDateFrom = new System.Windows.Forms.DateTimePicker();
            this.upHourFrom = new System.Windows.Forms.NumericUpDown();
            this.lblHour1 = new System.Windows.Forms.Label();
            this.lblMinus = new System.Windows.Forms.Label();
            this.dtpDateTo = new System.Windows.Forms.DateTimePicker();
            this.upHourTo = new System.Windows.Forms.NumericUpDown();
            this.lblHour2 = new System.Windows.Forms.Label();
            this.groupBox.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upHourFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upHourTo)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Size = new System.Drawing.Size(340, 40);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.flowLayoutPanel1);
            this.pnlContent.Size = new System.Drawing.Size(330, 22);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.dtpDateFrom);
            this.flowLayoutPanel1.Controls.Add(this.upHourFrom);
            this.flowLayoutPanel1.Controls.Add(this.lblHour1);
            this.flowLayoutPanel1.Controls.Add(this.lblMinus);
            this.flowLayoutPanel1.Controls.Add(this.dtpDateTo);
            this.flowLayoutPanel1.Controls.Add(this.upHourTo);
            this.flowLayoutPanel1.Controls.Add(this.lblHour2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(330, 22);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // dtpDateFrom
            // 
            this.dtpDateFrom.CustomFormat = "";
            this.dtpDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateFrom.Location = new System.Drawing.Point(0, 0);
            this.dtpDateFrom.Margin = new System.Windows.Forms.Padding(0);
            this.dtpDateFrom.Name = "dtpDateFrom";
            this.dtpDateFrom.ShowCheckBox = true;
            this.dtpDateFrom.Size = new System.Drawing.Size(105, 20);
            this.dtpDateFrom.TabIndex = 10;
            // 
            // upHourFrom
            // 
            this.upHourFrom.Location = new System.Drawing.Point(106, 0);
            this.upHourFrom.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.upHourFrom.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.upHourFrom.Name = "upHourFrom";
            this.upHourFrom.Size = new System.Drawing.Size(38, 20);
            this.upHourFrom.TabIndex = 13;
            // 
            // lblHour1
            // 
            this.lblHour1.Location = new System.Drawing.Point(145, 0);
            this.lblHour1.Margin = new System.Windows.Forms.Padding(0);
            this.lblHour1.Name = "lblHour1";
            this.lblHour1.Size = new System.Drawing.Size(12, 23);
            this.lblHour1.TabIndex = 15;
            this.lblHour1.Text = "h";
            this.lblHour1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMinus
            // 
            this.lblMinus.AutoSize = true;
            this.lblMinus.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblMinus.Location = new System.Drawing.Point(157, 0);
            this.lblMinus.Margin = new System.Windows.Forms.Padding(0);
            this.lblMinus.Name = "lblMinus";
            this.lblMinus.Size = new System.Drawing.Size(13, 18);
            this.lblMinus.TabIndex = 12;
            this.lblMinus.Text = "-";
            // 
            // dtpDateTo
            // 
            this.dtpDateTo.CustomFormat = "";
            this.dtpDateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateTo.Location = new System.Drawing.Point(170, 0);
            this.dtpDateTo.Margin = new System.Windows.Forms.Padding(0);
            this.dtpDateTo.Name = "dtpDateTo";
            this.dtpDateTo.ShowCheckBox = true;
            this.dtpDateTo.Size = new System.Drawing.Size(105, 20);
            this.dtpDateTo.TabIndex = 11;
            // 
            // upHourTo
            // 
            this.upHourTo.Location = new System.Drawing.Point(276, 0);
            this.upHourTo.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.upHourTo.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.upHourTo.Name = "upHourTo";
            this.upHourTo.Size = new System.Drawing.Size(38, 20);
            this.upHourTo.TabIndex = 14;
            this.upHourTo.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // lblHour2
            // 
            this.lblHour2.Location = new System.Drawing.Point(315, 0);
            this.lblHour2.Margin = new System.Windows.Forms.Padding(0);
            this.lblHour2.Name = "lblHour2";
            this.lblHour2.Size = new System.Drawing.Size(12, 23);
            this.lblHour2.TabIndex = 16;
            this.lblHour2.Text = "h";
            this.lblHour2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucDateInterval
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ucDateInterval";
            this.Size = new System.Drawing.Size(340, 40);
            this.groupBox.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upHourFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upHourTo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.DateTimePicker dtpDateFrom;
        private System.Windows.Forms.NumericUpDown upHourFrom;
        private System.Windows.Forms.Label lblHour1;
        private System.Windows.Forms.Label lblMinus;
        private System.Windows.Forms.DateTimePicker dtpDateTo;
        private System.Windows.Forms.NumericUpDown upHourTo;
        private System.Windows.Forms.Label lblHour2;

    }
}
