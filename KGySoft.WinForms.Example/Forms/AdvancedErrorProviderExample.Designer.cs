namespace KGySoft.WinForms.Example.Forms
{
    partial class AdvancedErrorProviderExample
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label userNameLabel;
            System.Windows.Forms.Label dateOfBirthLabel;
            System.Windows.Forms.Label passwordLabel;
            System.Windows.Forms.Label accountBalanceLabel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedErrorProviderExample));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.errorProvider = new KGySoft.WinForms.Components.AdvancedErrorProvider(this.components);
            this.bindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.warningProvider = new KGySoft.WinForms.Components.AdvancedErrorProvider(this.components);
            this.infoProvider = new KGySoft.WinForms.Components.AdvancedErrorProvider(this.components);
            this.userNameTextBox = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.dateOfBirthDateTimePicker = new KGySoft.WinForms.Controls.AdvancedDateTimePicker();
            this.passwordTextBox = new KGySoft.WinForms.Controls.AdvancedTextBox();
            this.accountBalanceTextBox = new KGySoft.WinForms.Controls.DecimalTextBox();
            this.lblInstruction = new KGySoft.WinForms.Controls.AdvancedLabel();
            this.gbCurrent = new System.Windows.Forms.GroupBox();
            this.validatingObjectExampleDataGridView = new KGySoft.WinForms.Example.Controls.ValidatingDataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            userNameLabel = new System.Windows.Forms.Label();
            dateOfBirthLabel = new System.Windows.Forms.Label();
            passwordLabel = new System.Windows.Forms.Label();
            accountBalanceLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.infoProvider)).BeginInit();
            this.gbCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.validatingObjectExampleDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // userNameLabel
            // 
            userNameLabel.AutoSize = true;
            userNameLabel.Location = new System.Drawing.Point(6, 30);
            userNameLabel.Name = "userNameLabel";
            userNameLabel.Size = new System.Drawing.Size(63, 13);
            userNameLabel.TabIndex = 2;
            userNameLabel.Text = "User Name:";
            // 
            // dateOfBirthLabel
            // 
            dateOfBirthLabel.AutoSize = true;
            dateOfBirthLabel.Location = new System.Drawing.Point(4, 62);
            dateOfBirthLabel.Name = "dateOfBirthLabel";
            dateOfBirthLabel.Size = new System.Drawing.Size(69, 13);
            dateOfBirthLabel.TabIndex = 4;
            dateOfBirthLabel.Text = "Date of Birth:";
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Location = new System.Drawing.Point(4, 93);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new System.Drawing.Size(56, 13);
            passwordLabel.TabIndex = 6;
            passwordLabel.Text = "Password:";
            // 
            // accountBalanceLabel
            // 
            accountBalanceLabel.AutoSize = true;
            accountBalanceLabel.Location = new System.Drawing.Point(2, 123);
            accountBalanceLabel.Name = "accountBalanceLabel";
            accountBalanceLabel.Size = new System.Drawing.Size(92, 13);
            accountBalanceLabel.TabIndex = 8;
            accountBalanceLabel.Text = "Account Balance:";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            this.errorProvider.DataSource = this.bindingSource;
            // 
            // bindingSource
            // 
            this.bindingSource.DataSource = typeof(KGySoft.WinForms.Example.ViewModel.ValidatingObjectExample);
            // 
            // warningProvider
            // 
            this.warningProvider.ContainerControl = this;
            this.warningProvider.DataSource = this.bindingSource;
            this.warningProvider.ShowBindingErrors = false;
            this.warningProvider.SetMessage += new System.EventHandler<KGySoft.WinForms.Components.SetMessageEventArgs>(this.warningProvider_SetMessage);
            // 
            // infoProvider
            // 
            this.infoProvider.ContainerControl = this;
            this.infoProvider.DataSource = this.bindingSource;
            this.infoProvider.ShowBindingErrors = false;
            this.infoProvider.SetMessage += new System.EventHandler<KGySoft.WinForms.Components.SetMessageEventArgs>(this.infoProvider_SetMessage);
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSource, "UserName", true));
            this.userNameTextBox.Location = new System.Drawing.Point(100, 27);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(189, 20);
            this.userNameTextBox.TabIndex = 3;
            // 
            // dateOfBirthDateTimePicker
            // 
            this.dateOfBirthDateTimePicker.CustomFormat = "";
            this.dateOfBirthDateTimePicker.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingSource, "DateOfBirth", true));
            this.dateOfBirthDateTimePicker.Location = new System.Drawing.Point(100, 58);
            this.dateOfBirthDateTimePicker.Name = "dateOfBirthDateTimePicker";
            this.dateOfBirthDateTimePicker.Size = new System.Drawing.Size(189, 20);
            this.dateOfBirthDateTimePicker.TabIndex = 5;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSource, "Password", true));
            this.passwordTextBox.Location = new System.Drawing.Point(100, 90);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(189, 20);
            this.passwordTextBox.TabIndex = 7;
            // 
            // accountBalanceTextBox
            // 
            this.accountBalanceTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingSource, "AccountBalance", true));
            this.accountBalanceTextBox.DecimalDigits = ((sbyte)(2));
            this.accountBalanceTextBox.Location = new System.Drawing.Point(100, 120);
            this.accountBalanceTextBox.Name = "accountBalanceTextBox";
            this.accountBalanceTextBox.Size = new System.Drawing.Size(189, 20);
            this.accountBalanceTextBox.TabIndex = 9;
            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoHandleUrls = true;
            this.lblInstruction.AutoSize = true;
            this.lblInstruction.BorderStyle = KGySoft.WinForms.Controls.AdvancedBorderStyle.Flat;
            this.lblInstruction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstruction.EnabledBackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.EnabledForeColor = System.Drawing.SystemColors.WindowText;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInstruction.Location = new System.Drawing.Point(0, 0);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Padding = new System.Windows.Forms.Padding(5);
            this.lblInstruction.RawText = resources.GetString("lblInstruction.RawText");
            this.lblInstruction.ResolveHyperlinks = KGySoft.WinForms.HyperlinkResolveMode.ResolveHrefsOnly;
            this.lblInstruction.Size = new System.Drawing.Size(553, 106);
            this.lblInstruction.TabIndex = 10;
            this.lblInstruction.TabStop = true;
            this.lblInstruction.UseCompatibleTextRendering = true;
            // 
            // gbCurrent
            // 
            this.gbCurrent.Controls.Add(userNameLabel);
            this.gbCurrent.Controls.Add(this.userNameTextBox);
            this.gbCurrent.Controls.Add(this.dateOfBirthDateTimePicker);
            this.gbCurrent.Controls.Add(accountBalanceLabel);
            this.gbCurrent.Controls.Add(dateOfBirthLabel);
            this.gbCurrent.Controls.Add(this.accountBalanceTextBox);
            this.gbCurrent.Controls.Add(this.passwordTextBox);
            this.gbCurrent.Controls.Add(passwordLabel);
            this.gbCurrent.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbCurrent.Location = new System.Drawing.Point(0, 239);
            this.gbCurrent.Name = "gbCurrent";
            this.gbCurrent.Size = new System.Drawing.Size(553, 157);
            this.gbCurrent.TabIndex = 11;
            this.gbCurrent.TabStop = false;
            this.gbCurrent.Text = "Selected User";
            // 
            // validatingObjectExampleDataGridView
            // 
            this.validatingObjectExampleDataGridView.AutoGenerateColumns = false;
            this.validatingObjectExampleDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.validatingObjectExampleDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.validatingObjectExampleDataGridView.DataSource = this.bindingSource;
            this.validatingObjectExampleDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.validatingObjectExampleDataGridView.Location = new System.Drawing.Point(0, 106);
            this.validatingObjectExampleDataGridView.Name = "validatingObjectExampleDataGridView";
            this.validatingObjectExampleDataGridView.Size = new System.Drawing.Size(553, 133);
            this.validatingObjectExampleDataGridView.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "UserName";
            this.dataGridViewTextBoxColumn1.HeaderText = "User name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "DateOfBirth";
            this.dataGridViewTextBoxColumn2.HeaderText = "Birth date";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Password";
            this.dataGridViewTextBoxColumn3.HeaderText = "Password";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "AccountBalance";
            dataGridViewCellStyle1.Format = "N2";
            dataGridViewCellStyle1.NullValue = null;
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn4.HeaderText = "Balance";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // AdvancedErrorProviderExample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 396);
            this.Controls.Add(this.validatingObjectExampleDataGridView);
            this.Controls.Add(this.gbCurrent);
            this.Controls.Add(this.lblInstruction);
            this.Name = "AdvancedErrorProviderExample";
            this.Text = "AdvancedErrorProvider Example";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.infoProvider)).EndInit();
            this.gbCurrent.ResumeLayout(false);
            this.gbCurrent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.validatingObjectExampleDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSource;
        private Controls.ValidatingDataGridView validatingObjectExampleDataGridView;
        private Components.AdvancedErrorProvider errorProvider;
        private Components.AdvancedErrorProvider warningProvider;
        private Components.AdvancedErrorProvider infoProvider;
        private WinForms.Controls.DecimalTextBox accountBalanceTextBox;
        private WinForms.Controls.AdvancedTextBox passwordTextBox;
        private WinForms.Controls.AdvancedDateTimePicker dateOfBirthDateTimePicker;
        private WinForms.Controls.AdvancedTextBox userNameTextBox;
        private WinForms.Controls.AdvancedLabel lblInstruction;
        private System.Windows.Forms.GroupBox gbCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    }
}