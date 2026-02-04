#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucTextBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Base type of text container user controls.
    /// </summary>
    [DefaultBindingProperty("Text")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucTextBase : ucCaptionedBase
    {
        #region Fields

        private readonly TextBox tbForDesignerOnly = new TextBox(); // dummy TextBox only for the designer
        private string origValue = "";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the text of the inner textbox changes.
        /// </summary>
        [Browsable(true)]
        [Category("ucTextBase")]
        [Description("Occurs when the text of the inner textbox changes.")]
        public new event EventHandler TextChanged
        {
            add => MainControl.TextChanged += value;
            remove => MainControl.TextChanged -= value;
        }

        /// <summary>
        /// Occurs on leave when content differs from the content at getting focused.
        /// </summary>
        [Category("ucTextBase")]
        [Description("Occurs on leave when content differs from the content at getting focused.")]
        public event EventHandler? TextChangedOnLeave;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the Read-Only state.
        /// </summary>
        [Category("ucTextBase")]
        [Description("Gets or sets the Read-Only state.")]
        [DefaultValue(false)]
        public override bool ReadOnly
        {
            get => (MainControl as TextBoxBase)?.ReadOnly ?? true;
            set
            {
                (MainControl as TextBoxBase)?.ReadOnly = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets the text of the inner textbox.
        /// </summary>
        public override object? ControlValue
        {
            get => Text;
            set => Text = value?.ToString();
        }

        /// <summary>
        /// The inner TextBox. It is possible to "override" this member in descendants and hide with a new member
        /// that returns a more specific type. This property is not virtual but it is not a problem because
        /// the value of <see cref="MainControl"/> is returned from here, which is virtual.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Category("ucTextBase")]
        [Description("Gets the inner TextBox")]
        public TextBoxBase? TextBox => MainControl as TextBoxBase;

        /// <summary>
        /// Gets or sets the Text of the inner textbox.
        /// </summary>
        [Category("ucTextBase")]
        [Description("Gets or sets the Text of the inner textbox.")]
        [DefaultValue("")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [AllowNull]
        public override string Text
        {
            get => MainControl.Text;
            set => MainControl.Text = value;
        }

        /// <summary>
        /// Maximal text length when editing text on the user interface.
        /// </summary>
        [Category("ucTextBase")]
        [Description("Maximal text length when editing text on the user interface.")]
        [DefaultValue(32767)]
        public virtual int MaxLength
        {
            get => (MainControl as TextBoxBase)?.MaxLength ?? 0;
            set => (MainControl as TextBoxBase)?.MaxLength = value;
        }

        /// <summary>
        /// Gets or sets the border style of the inner textbox.
        /// </summary>
        [Category("ucTextBase")]
        [Description("Gets or sets the border type of the text box control.")]
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle TextBoxBorderStyle
        {
            get => (MainControl as TextBoxBase)?.BorderStyle ?? BorderStyle.Fixed3D;
            set => (MainControl as TextBoxBase)?.BorderStyle = value;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// This property should be overridden in derived classes.
        /// </summary>
        protected override Control MainControl => tbForDesignerOnly;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucTextBase"/> class.
        /// </summary>
        public ucTextBase()
        {
            InitializeComponent();
            Load += ucTextBase_Load;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Clears the text of the inner textbox.
        /// </summary>
        public override void Clear()
        {
            MainControl.Text = String.Empty;
            base.Clear();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Fires the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTextChangedOnLeave(EventArgs e) => TextChangedOnLeave?.Invoke(this, e);

        #endregion

        #region Event handlers

        void ucTextBase_Load(object? sender, EventArgs e)
        {
            if (DesignMode)
                return;

            if (MainControl is not TextBoxBase || (MainControl == tbForDesignerOnly))
                throw new InvalidOperationException("Derived class from ucTextBase must contain an overridden MainControl, which returns with a TextBoxBase control! You must also define a new TextBox property, which returns with the actual type of your custom TextBox.");

            MainControl.Enter += txtValue_Enter;
            MainControl.Leave += txtValue_Leave;
            MainControl.TextChanged += MainControl_TextChanged;
            MainControl.EnabledChanged += txtValue_EnabledChanged;
        }

        void MainControl_TextChanged(object? sender, EventArgs e)
        {
            if (!MainControl.Focused)
                ResetColor();
        }

        private void txtValue_Enter(object? sender, EventArgs e) => origValue = MainControl.Text;

        private void txtValue_Leave(object? sender, EventArgs e)
        {
            ResetColor();
            if (origValue != MainControl.Text)
                OnTextChangedOnLeave(e);
        }

        private void txtValue_EnabledChanged(object? sender, EventArgs e) => ResetColor();

        #endregion

        #endregion
    }
}
