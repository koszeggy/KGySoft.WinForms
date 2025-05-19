#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucTextBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucTextBase : ucCaptionedBase
    {
        #region Fields

        private string origValue = "";
        private TextBox tbForDesignerOnly = new TextBox(); // dummy TextBox only for the designer

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the text of the inner textbox changes.
        /// </summary>
        [
            Browsable(true),
            Category("ucTextBase"),
            Description("Occurs when the text of the inner textbox changes.")
        ]
        public new event EventHandler TextChanged
        {
            add
            {
                MainControl.TextChanged += value;
            }
            remove
            {
                MainControl.TextChanged -= value;
            }
        }

        /// <summary>
        /// Occurs on leave when content differs from the content at getting focused.
        /// </summary>
        [
            Category("ucTextBase"),
            Description("Occurs on leave when content differs from the content at getting focused.")
        ]
        public event EventHandler TextChangedOnLeave;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the Read-Only state.
        /// </summary>
        [
            Category("ucTextBase"),
            Description("Gets or sets the Read-Only state."),
            DefaultValue(false)
        ]
        public override bool ReadOnly
        {
            get { return (MainControl as TextBoxBase).ReadOnly; }
            set
            {
                (MainControl as TextBoxBase).ReadOnly = value;
                ResetColor();
            }
        }

        public override object ControlValue
        {
            get { return Text; }
            set { Text = value.ToString(); }
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
        public TextBoxBase TextBox
        {
            get
            {
                if (MainControl is TextBoxBase)
                    return (TextBoxBase)MainControl;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the Text of the inner textbox.
        /// <remarks>If needed, can be ReadOnly in descendants.</remarks>
        /// </summary>
        [
            Category("ucTextBase"),
            Description("Gets or sets the Text of the inner textbox."),
            DefaultValue(""),
            Browsable(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
            Bindable(BindableSupport.Default, BindingDirection.TwoWay),
        ]
        public override string Text
        {
            get { return MainControl.Text; }
            set { MainControl.Text = value; }
        }

        /// <summary>
        /// Maximal text length when editing text on the user interface.
        /// </summary>
        [
            Category("ucTextBase"),
            Description("Maximal text length when editing text on the user interface."),
            DefaultValue(32767)
        ]
        public virtual int MaxLength
        {
            get { return (MainControl as TextBoxBase).MaxLength; }
            set { (MainControl as TextBoxBase).MaxLength = value; }
        }

        /// <summary>
        /// Maximal text length when editing text on the user interface.
        /// </summary>
        [
            Category("ucTextBase"),
            Description("Gets or sets the border type of the text box control."),
            DefaultValue(typeof(BorderStyle), "Fixed3D")
        ]
        public BorderStyle TextBoxBorderStyle
        {
            get { return (MainControl as TextBoxBase).BorderStyle; }
            set { (MainControl as TextBoxBase).BorderStyle = value; }
        }

        #endregion

        #region Protected Properties

        protected override Control MainControl
        {
            get { return tbForDesignerOnly; } // Must override in further descendants!
        }

        #endregion

        #endregion

        #region Constructors

        public ucTextBase()
        {
            InitializeComponent();
            Load += new EventHandler(ucTextBase_Load);
        }

        #endregion

        #region Methods

        #region Public Methods

        public override void Clear()
        {
            MainControl.Text = "";
            base.Clear();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Fires the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTextChangedOnLeave(EventArgs e)
        {
            if (TextChangedOnLeave != null)
                TextChangedOnLeave(this, e);
        }

        #endregion

        #region Event handlers

        void ucTextBase_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            if (!(MainControl is TextBoxBase) || (MainControl == tbForDesignerOnly))
                throw new InvalidOperationException("Derived class from ucTextBase must contain an overridden MainControl, which returns with a TextBoxBase control! You must also define a new TextBox property, which returns with the actual type of your custom TextBox.");

            MainControl.Enter += new System.EventHandler(this.txtValue_Enter);
            MainControl.Leave += new System.EventHandler(this.txtValue_Leave);
            MainControl.TextChanged += new EventHandler(MainControl_TextChanged);
            MainControl.EnabledChanged += new System.EventHandler(this.txtValue_EnabledChanged);
        }

        void MainControl_TextChanged(object sender, EventArgs e)
        {
            if (MainControl.Focused)
                return;
            else
                ResetColor();
        }

        private void txtValue_Enter(object sender, EventArgs e)
        {
            origValue = MainControl.Text;
        }

        private void txtValue_Leave(object sender, EventArgs e)
        {
            ResetColor();
            if (origValue != MainControl.Text)
                OnTextChangedOnLeave(e);
        }

        private void txtValue_EnabledChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        #endregion

        #endregion
    }
}
