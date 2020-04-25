using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Unified user control version of <see cref="AdvancedTextBox"/>.
    /// </summary>
    [ToolboxItem(true)]
    public partial class ucText: ucTextBase
    {
        #region Overridden properties

        protected override Control MainControl
        {
            get { return textControl; }
        }

        /// <summary>
        /// Gets the inner <see cref="TextBox"/>.
        /// </summary>
        [Category("ucText")]
        [Description("The inner TextBox")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // Do not change this! If something is needed, make a new property instead.
        [Browsable(false)]
        public new AdvancedTextBox TextBox
        {
            get { return textControl; }
        }

        #endregion

        #region ucText Properties

        /// <summary>
        /// Gets or sets the Text of the inner textbox.
        /// </summary>
        [Category("ucText")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
		/// Gets or sets a value indicating whether this is a multiline textbox.
        /// </summary>
        [
            Category("ucText"),
			Description("Gets or sets a value indicating whether this is a multiline textbox."),
            DefaultValue(false)
        ]
        public bool Multiline
        {
            get { return textControl.Multiline; }
            set
            {
                textControl.Multiline = value;
                if (value) textControl.ScrollBars = ScrollBars.Vertical;
            }
        }

		/// <summary>
		/// Gets or sets which scroll bars should appear in a multiline TextBox.
		/// </summary>
        [
            Category("ucText"),
			Description("Gets or sets which scroll bars should appear in a multiline TextBox.")
        ]
        public ScrollBars ScrollBars
        {
            get { return textControl.ScrollBars; }
            set { textControl.ScrollBars = value; }
        }

		/// <summary>
		/// Gets or sets the character used to mask characters of a password in a single-line TextBox control.
		/// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue('\0')]
        [Category("ucText")]
		[Description("Gets or sets the character used to mask characters of a password in a single-line TextBox control.")]
        public char PasswordChar
        {
            get { return textControl.PasswordChar; }
            set { textControl.PasswordChar = value; }
        }

		/// <summary>
		/// Gets or sets a value indicating whether the text in the TextBox
		/// control should appear as the default password character.
		/// </summary>
        [Category("ucText")]
        [Description("Gets or sets a value indicating whether the text in the TextBox control should appear as the default password character.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get { return textControl.UseSystemPasswordChar; }
            set { textControl.UseSystemPasswordChar = value; }
        }

		/// <summary>
		/// Gets or sets how text is aligned in a TextBox control.
		/// </summary>
        [Category("ucText")]
		[Description("Gets or sets how text is aligned in a TextBox control.")]
        [DefaultValue(typeof(HorizontalAlignment), "Left")]
        public HorizontalAlignment TextAlign
        {
            get { return textControl.TextAlign; }
            set { textControl.TextAlign = value; }
        }

        /// <summary>
        /// Indicates whether a multiline text box control automatically wraps words
        /// to the beginning of the next line when necessary.
        /// </summary>
        [Category("ucText")]
        [Description("Indicates whether a multiline text box control automatically wraps words to the beginning of the next line when necessary.")]
        [DefaultValue(true)]
        public bool WordWrap
        {
            get { return textControl.WordWrap; }
            set { textControl.WordWrap = value; }
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner text box.
        /// </summary>
        [Category("ucText")]
        [Description("Gets or sets an option that controls how automatic completion works for the inner text box.")]
        [DefaultValue(AutoCompleteMode.None)]
        public AutoCompleteMode AutoCompleteMode
        {
            get { return textControl.AutoCompleteMode; }
            set { textControl.AutoCompleteMode = value; }
        }

        ///<summary>
        /// Gets or sets a value specifying the source of complete strings used for automatic completion.
        ///</summary>
        [Category("ucText")]
        [Description("Gets or sets a value specifying the source of complete strings used for automatic completion.")]
        [DefaultValue(AutoCompleteSource.None)]
        public AutoCompleteSource AutoCompleteSource
        {
            get { return textControl.AutoCompleteSource; }
            set { textControl.AutoCompleteSource = value; }
        }

        ///<summary>
        /// Gets or sets a custom <see cref="AutoCompleteStringCollection"/> to <see cref="AutoCompleteSource"/> property is <see cref="System.Windows.Forms.AutoCompleteSource.CustomSource"/>.
        ///</summary>
        [Category("ucText")]
        [Description("Gets or sets a custom AutoCompleteStringCollection to AutoCompleteSource property is CustomSource.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get { return textControl.AutoCompleteCustomSource; }
            set { textControl.AutoCompleteCustomSource = value; }
        }

        #endregion

        #region Constructor

        public ucText()
        {
            InitializeComponent();
        }

        #endregion
    }
}
