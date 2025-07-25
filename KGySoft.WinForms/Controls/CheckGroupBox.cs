#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckGroupBox.cs
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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type. - Controls items are never null
#pragma warning disable CS8604 // Possible null reference argument. - Controls items are never null
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a <see cref="GroupBox"/> control with a <see cref="CheckBox"/> that can be checked or unchecked to enable or disable the content of the group box.
    /// </summary>
    public partial class CheckGroupBox : GroupBox, ICustomLocalizable, IToolTipTargetProvider, IObservableParent
    {
        #region Nested Classes

        /// <summary>
        /// Represents a collection of controls contained within a <see cref="CheckGroupBox"/>.
        /// </summary>
        protected new class ControlCollection : Control.ControlCollection
        {
            #region Fields

            private readonly CheckGroupBox owner;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlCollection"/> class with the specified owner.
            /// </summary>
            /// <param name="owner">The <see cref="CheckGroupBox"/> that owns this collection.</param>
            public ControlCollection(CheckGroupBox owner)
                : base(owner ?? throw new ArgumentNullException(nameof(owner), PublicResources.ArgumentNull))
            {
                this.owner = owner;
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public override void Add(Control value)
            {
                owner.isAddingControl = true;
                try
                {
                    if (owner.DesignMode || value == owner.checkBox || value == owner.contentPanel
                        // Linux/Mono workaround: prevent disabling ErrorProvider's user control when the content is disabled
                        || (!OSHelper.IsWindows || OSHelper.IsMono) && value.GetType().DeclaringType == typeof(ErrorProvider))
                    {
                        base.Add(value);
                    }
                    else
                    {
                        // When not in design mode, adding custom controls to a panel so we can toggle its Enabled with preserving their original state.
                        // Also, translating the control's location so it appears in the same place as in the designer. Doing it only after initialization is complete,
                        // because in the designer the child controls' location may be set after adding them to the group box.
                        owner.contentPanel.Parent ??= owner;
                        owner.contentPanel.Controls.Add(value);
                    }
                }
                finally
                {
                    owner.isAddingControl = false;
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private bool isInitialized;
        private bool isRendering;
        private bool changingBaseText;
        private bool isAddingControl;

        private Color explicitForeColor;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="CheckBox"/> is checked or unchecked.
        /// </summary>
        internal event EventHandler CheckedChanged
        {
            add => Events.AddHandler(nameof(CheckedChanged), value);
            remove => Events.RemoveHandler(nameof(CheckedChanged), value);
        }

        #endregion

        #region Properties
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the text of the <see cref="CheckGroupBox"/>. That is, the text of the <see cref="CheckBox"/> control.
        /// </summary>
        [Localizable(true)]
        [AllowNull]
        public override string Text
        {
            get => isRendering ? base.Text : checkBox.Text;
            set => checkBox.Text = value;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="CheckBox"/> of the <see cref="CheckGroupBox"/> control is checked.
        /// When checked, the content of the group box is enabled, otherwise it is disabled.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        [Category("CheckGroupBox")]
        [DefaultValue(true)]
        public bool Checked
        {
            get => checkBox.Checked;
            set => checkBox.Checked = value;
        }

        /// <inheritdoc />
        public override Color ForeColor
        {
            get => explicitForeColor.IsEmpty ? base.ForeColor : explicitForeColor;
            set
            {
                if (explicitForeColor == value)
                    return;

                base.ForeColor = value;
                if (value.IsEmpty)
                    OnForeColorChanged(EventArgs.Empty);
                explicitForeColor = value;
                ResetCheckBoxColor();
            }
        }

        /// <summary>
        /// Gets or sets the flat style appearance of the <see cref="CheckBox"/> and the <see cref="GroupBox"/>.
        /// </summary>
        [DefaultValue(FlatStyle.Standard)]
        [Description("Gets or sets the flat style appearance of the check box and the group box.")]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set
            {
                if (value == base.FlatStyle)
                    return;

                // validation is performed by the base class
                base.FlatStyle = value;
                checkBox.FlatStyle = value;
                checkBox.Invalidate();
            }
        }

        /// <summary>
        /// Gets the <see cref="AdvancedCheckBox"/> control, which serves as the checkbox of the <see cref="CheckGroupBox"/> control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AdvancedCheckBox CheckBox => checkBox;

        #endregion

        #region Explicitly Implemented Interface Properties

        bool IObservableParent.IsAddingControl => isAddingControl;
        bool IObservableParent.IsChangingFont => false;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckGroupBox"/> class.
        /// </summary>
        public CheckGroupBox()
        {
            InitializeComponent();
            Controls.Add(checkBox);
            checkBox.SizeChanged += CheckBox_SizeChanged;
            checkBox.CheckedChanged += CheckBox_CheckedChanged;
            checkBox.TextChanged += CheckBox_TextChanged;
            ResetCheckBoxColor();
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override Control.ControlCollection CreateControlsInstance() => new ControlCollection(this);

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Making sure there is enough space before the CheckBox at every DPI
            // Needed to reset each time when the handle is recreated, because otherwise the base.Text is set to the CheckBox's text
            changingBaseText = true;
            base.Text = @"   ";
            changingBaseText = false;
            if (isInitialized)
                return;

            isInitialized = true;
            ResetCheckBoxLocation();
            foreach (Control control in contentPanel.Controls)
                control.Location = new Point(control.Left - contentPanel.Left, control.Top - contentPanel.Top);
        }

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            if (changingBaseText)
                return;
            base.OnTextChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="CheckedChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnCheckedChanged(EventArgs e) => (Events[nameof(CheckedChanged)] as EventHandler)?.Invoke(this, e);

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (RightToLeft == RightToLeft.Yes)
                ResetCheckBoxLocation();
        }

        /// <inheritdoc />
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            ResetCheckBoxLocation();
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    isRendering = true;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        isRendering = false;
                    }
                    break;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    base.WndProc(ref m);
                    checkBox.Top = 0;
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            checkBox.CheckedChanged -= CheckBox_CheckedChanged;
            checkBox.SizeChanged -= CheckBox_SizeChanged;
            checkBox.TextChanged -= CheckBox_TextChanged;
            if (disposing)
            {
                components?.Dispose();
                if (contentPanel.Parent == null)
                    contentPanel.Dispose();
                Events.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void ResetCheckBoxLocation()
        {
            // Skipping if the handle has never been created yet (GetScale uses the Handle).
            // Without this, the focus rectangle may not be rendered when pressing TAB, and not even the ShowFocusCues is called.
            // Can happen if the CheckBox is unchecked and the GroupBox is inside a user control.
            if (!isInitialized)
                return;

            checkBox.Left = RightToLeft == RightToLeft.No
                ? (int)(10 * this.GetScale().X)
                : Width - checkBox.Width - (int)(10 * this.GetScale().X);
        }

        private void ResetCheckBoxColor() => checkBox.EnabledForeColor = !explicitForeColor.IsEmpty ? explicitForeColor
            : VisualStyleHelper.RenderWithVisualStyles ? VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_GROUPBOX, (int)GroupBoxState.Normal, default)
            : default;

        #endregion

        #region Event handlers

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            // Toggling the Enabled state of the content. This method preserves the original Enabled state of the controls.
            contentPanel.Enabled = checkBox.Checked;
            OnCheckedChanged(EventArgs.Empty);
        }

        private void CheckBox_SizeChanged(object? sender, EventArgs e) => ResetCheckBoxLocation();
        private void CheckBox_TextChanged(object? sender, EventArgs e) => OnTextChanged(e);
        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => ResetCheckBoxColor();

        #endregion

        #region Explicitly Implemented Interface Methods

        bool ICustomLocalizable.ApplyStringResources(LocalizationContext context)
        {
            // Self properties
            LocalizationHelper.LocalizeStringProperties(this, Name, context);

            // children: only contentPanel controls so checkBox is skipped (otherwise, could be overwritten by checkbox.Name)
            foreach (Control child in contentPanel.Controls)
                LocalizationHelper.ApplyStringResources(child, context);

            return true;
        }

        Control IToolTipTargetProvider.GetToolTipTarget() => checkBox;

        #endregion

        #endregion
    }
}
