#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckGroupBox.cs
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
    [ToolboxBitmap(typeof(CheckGroupBox), "Resources.Toolbox.CheckGroupBox.png")]
    public partial class CheckGroupBox : GroupBox, ICustomLocalizable, IToolTipTargetProvider, IObservableParent, ISafePaintBackground // TODO: ISafePaintBackground into an AdvancedGroupBox
    {
        #region Nested Classes

        #region ControlCollection class

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
            public override void Add(Control? value)
            {
                if (value == null)
                    return;
                owner.isAddingControl = true;
                try
                {
                    if (owner.DesignMode || value == owner.checkBox || value == owner.contentPanel
                        // Linux/Mono workaround: prevent disabling ErrorProvider's user control when the content is disabled
                        || (!OSHelper.IsWindows || OSHelper.IsFrameworkMono) && value.GetType().DeclaringType == typeof(ErrorProvider))
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

        #region GroupBoxCheckBox class

        private sealed class GroupBoxCheckBox : AdvancedCheckBox
        {
            #region Fields

            private CheckGroupBox? owner;
            private bool hasAlpha;

            #endregion

            #region Properties

            private CheckGroupBox? Owner => owner ??= Parent as CheckGroupBox;

            #endregion

            #region Methods

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                if (Owner?.IsHandleCreated == true)
                    Owner.ResetBaseText(); // only when both self and owner handles are created
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Owner is not CheckGroupBox checkGroupBox)
                {
                    base.OnPaint(e);
                    return;
                }

                checkGroupBox.isRendering = true;
                try
                {
                    base.OnPaint(e);
                }
                finally
                {
                    checkGroupBox.isRendering = false;
                }
            }

            protected override void OnCheckedChanged(EventArgs e)
            {
                base.OnCheckedChanged(e);
                Owner?.OnCheckedChanged(e);
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                Owner?.CheckBoxSizeChanged();
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                Owner?.OnTextChanged(e);
            }

            protected override void OnParentBackgroundImageChanged(EventArgs e)
            {
                // if there is a direct background image, making the checkbox background explicitly transparent; otherwise, inheriting the groupbox back color
                base.OnParentBackgroundImageChanged(e);
                EnabledBackColor = DisabledBackColor = Parent?.BackgroundImage == null ? Color.Empty : Color.Transparent;
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                bool newHasAlpha = BackColor.A != Byte.MaxValue;
                if (newHasAlpha != hasAlpha)
                    Owner?.ResetBaseText();
                hasAlpha = newHasAlpha;
            }

            #endregion
        }

        #endregion

        #region ContentPanel class

        private sealed class ContentPanel : Panel, ISafePaintBackground
        {
            #region Fields

            private CheckGroupBox? owner;

            #endregion

            #region Properties

            #region Public Properties
            
            public override Rectangle DisplayRectangle => Owner?.DisplayRectangle ?? base.DisplayRectangle;

            #endregion

            #region Private Properties

            private CheckGroupBox? Owner => owner ??= Parent as CheckGroupBox;

            #endregion

            #endregion

            #region Constructors

            internal ContentPanel() => DoubleBuffered = true;

            #endregion

            #region Methods

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (Owner is not CheckGroupBox checkGroupBox)
                {
                    base.OnPaintBackground(e);
                    return;
                }

                checkGroupBox.isRendering = true;
                try
                {
                    // NOTE: no need for the Graphics.GetHdc() workaround here, because the background image is never set directly for the content panel, but rather for the CheckGroupBox itself.
                    checkGroupBox.OnPaintBackground(e);
                }
                finally
                {
                    checkGroupBox.isRendering = false;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Owner is not CheckGroupBox checkGroupBox)
                {
                    base.OnPaint(e);
                    return;
                }

                checkGroupBox.isRendering = true;
                try
                {
                    checkGroupBox.OnPaint(e);
                }
                finally
                {
                    checkGroupBox.isRendering = false;
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const int referenceIndent = 10; // The reference indent for the CheckBox, used to calculate its Left position (or Right position in RTL mode)
        private const int referencePlaceholderPadding = 4; // The additional padding for the CheckBox's text when the base.Text is set to spaces, used to calculate its width

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

        /// <summary>
        /// Gets or sets a value that determines whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
        [DefaultValue(false)]
        public new bool UseCompatibleTextRendering
        {
            get => base.UseCompatibleTextRendering;
            set => checkBox.UseCompatibleTextRendering = base.UseCompatibleTextRendering = value;
        }

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
            
            // Needed to be reset each time when the handle is recreated, because otherwise the base.Text is set to the CheckBox's text
            if (checkBox.IsHandleCreated)
                ResetBaseText(); // needed only when both self and checkBox handles are created
            if (isInitialized)
                return;

            isInitialized = true;
            ResetCheckBoxColor();
            ResetCheckBoxLocation();
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
        protected virtual void OnCheckedChanged(EventArgs e)
        {
            contentPanel.Enabled = checkBox.Checked;
            (Events[nameof(CheckedChanged)] as EventHandler)?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            contentPanel.Size = ClientRectangle.Size;
            contentPanel.Invalidate();
            base.OnSizeChanged(e);
            if (RightToLeft == RightToLeft.Yes)
                ResetCheckBoxLocation();
        }

        /// <inheritdoc />
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            ResetCheckBoxLocation();
            if (OSHelper.IsFrameworkMono)
                ResetBaseText();
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_DPICHANGED_AFTERPARENT:
                    base.WndProc(ref m);
                    checkBox.Top = 0;
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

#if NETCOREAPP && !NET10_0_OR_GREATER
        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // workaround for https://github.com/dotnet/winforms/issues/13784
            base.OnPaintBackground(pevent);
            pevent.Graphics.GetHdc();
            pevent.Graphics.ReleaseHdc(); 
        }
#endif

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            isRendering = true;
            try
            {
                base.OnPaint(e);
            }
            finally
            {
                isRendering = false;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            base.Dispose(disposing);
            if (disposing)
            {
                components?.Dispose();
                if (contentPanel.Parent == null)
                    contentPanel.Dispose();
                Events.Dispose();
            }
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

            int indent = referenceIndent.Scale(this.GetScale().X);
            checkBox.Left = RightToLeft == RightToLeft.No
                ? indent
                : Width - checkBox.Width - indent;
        }

        private void ResetCheckBoxColor() => checkBox.EnabledForeColor = !explicitForeColor.IsEmpty ? explicitForeColor
            : VisualStyleHelper.RenderWithVisualStyles ? VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_GROUPBOX, (int)GroupBoxState.Normal, default)
            : default;

        private void CheckBoxSizeChanged()
        {
            ResetCheckBoxLocation();
            ResetBaseText();
        }

        /// <summary>
        /// Setting base.Text to as many spaces as the CheckBox's text width so even a transparent CheckBox will not have double text and crossed-out text from the frame.
        /// </summary>
        private void ResetBaseText()
        {
            if (!IsHandleCreated)
                return;

            // Mono GroupBox does not support RTL, so preventing the rendering of a big gap at the lift side in RTL mode when the back color is transparent.
            // TODO: Remove the Mono condition after implementing an AdvancedGroupBox with RTL support on all platforms
            if (checkBox.BackColor.A == Byte.MaxValue || OSHelper.IsFrameworkMono && RightToLeft == RightToLeft.Yes)
            {
                changingBaseText = true;

                // On Mono with visual styles, or on Linux we must use som non-empty text to avoid stretching the frame, so using a zero-width space character.
                // Unfortunately, this still causes a visible gap, though matters only when using Right-to-Left layout.
                base.Text = OSHelper.IsFrameworkMono && (VisualStyleHelper.RenderWithVisualStyles || !OSHelper.IsWindows) ? "\u200b" : String.Empty;
                changingBaseText = false;
                return;
            }

            // TextRenderer usage:
            // - when using visual styles, TextRenderer provides a closer result, even when rendering with GDI+
            // - Mono/Linux: TextRenderer.MeasureText ignores spaces so we would go into an infinite loop
            // - Mono/Windows/NoVisualStyles: TextRenderer provides a closer result, even when rendering with GDI+
            // - Relying on UseCompatibleTextRendering on non-Mono-Windows with no VisualStyles only
            bool useTextRenderer = VisualStyleHelper.RenderWithVisualStyles || OSHelper.IsWindows && (OSHelper.IsFrameworkMono || !UseCompatibleTextRendering);
            Font font = checkBox.Font;
            int desiredWidth = checkBox.Width + referencePlaceholderPadding.Scale(this.GetScale().X);
            using Graphics g = CreateGraphics();
            StringFormat? format = useTextRenderer ? null : TextFormatFlags.Default.ToStringFormat(); // from the internal cache, it includes MeasureTrailingSpaces

            // Initial measurement: set the same number of spaces as the CheckBox's text length. Most likely it will be smaller than the desired width,
            // but we can use it to guess a good length.
            int len = checkBox.Text.Length;
            var spaces = new String(' ', Math.Max(1, len));
            int actualWidth = useTextRenderer ? TextRenderer.MeasureText(g, spaces, font).Width : (int)g.MeasureString(spaces, font, PointF.Empty, format).Width;

            if (actualWidth != desiredWidth)
            {
                len = (int)(len * (float)desiredWidth / actualWidth) + 1;

                // len should be quite close to the desired space count now, but refining it
                spaces = new String(' ', len);
                actualWidth = useTextRenderer ? TextRenderer.MeasureText(g, spaces, font).Width : (int)g.MeasureString(spaces, font, PointF.Empty, format).Width;

                while (actualWidth > desiredWidth && len > 1)
                {
                    len -= 1;
                    spaces = new String(' ', len);
                    actualWidth = useTextRenderer ? TextRenderer.MeasureText(g, spaces, font).Width : (int)g.MeasureString(spaces, font, PointF.Empty, format).Width;
                }

                while (actualWidth < desiredWidth)
                {
                    len += 1;
                    spaces = new String(' ', len);
                    actualWidth = useTextRenderer ? TextRenderer.MeasureText(g, spaces, font).Width : (int)g.MeasureString(spaces, font, PointF.Empty, format).Width;
                }
            }

            changingBaseText = true;
            string text = spaces;

            // adding ZWJ, because in some cases (Mono or no visual styles with compatible rendering) multiple spaces are ignored by GroupBox.Text
            if (OSHelper.IsRealWindows && !VisualStyleHelper.RenderWithVisualStyles && UseCompatibleTextRendering
                || OSHelper.IsFrameworkMono && !VisualStyleHelper.RenderWithVisualStyles)
            {
                text += '\u200d';
            }

            base.Text = text;
            changingBaseText = false;
        }

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

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => ResetCheckBoxColor();

        #endregion

        #endregion
    }
}
