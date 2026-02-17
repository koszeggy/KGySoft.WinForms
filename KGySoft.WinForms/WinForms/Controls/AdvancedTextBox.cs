#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedTextBox.cs
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

using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETCOREAPP
#pragma warning disable CA1846 // Prefer 'AsSpan' over 'Substring' when span-based overloads are available - spans are not available in every targeted platform
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Advanced version of <see cref="TextBox"/> control that provides some advanced features and fixes for the original <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedTextBox"/> control offers the following features in addition to <see cref="TextBox"/>:
    /// <list type="bullet">
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item><see cref="TextBoxBase.AcceptsTab"/> and <see cref="TextBox.AcceptsReturn"/> are ignored in <see cref="TextBoxBase.ReadOnly"/> mode.</item>
    /// <item><see cref="TextChangedOnLeave"/> event: occurs when leaving the control and <see cref="TextBox.Text"/> is different from the value when the control received focus.</item>
    /// <item>Ctrl+A (Select All) works even if auto appending is enabled.</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [Description(@"A text box that provides the following features in addition to regular TextBox:
- Adjustable colors in disabled state
- AcceptsTab and AcceptsReturn are ignored in ReadOnly mode
- TextChangedOnLeave event
- Ctrl+A works even if auto appending is enabled
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedTextBox : TextBox, ISupportsDisabledColor, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledOrReadOnlyBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color defaultReadOnlyForeColor = SystemColors.ControlText;

        #endregion

        #region Instance Fields

        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        // NOTE: Unlike in ButtonBase descendants, we always set the base back and fore colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        private string origValue = String.Empty; // content at getting focused

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;
        private int dpiChangingCount;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs on leaving the control when content is different from the original one when the control was focused.
        /// It fires after the <see cref="Control.Leave"/> and before the <see cref="Control.Validating"/> event.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused. "
            + "It fires after the Leave and before the Validating event.")]
        public event EventHandler? TextChangedOnLeave
        {
            add => Events.AddHandler(nameof(TextChangedOnLeave), value);
            remove => Events.RemoveHandler(nameof(TextChangedOnLeave), value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> and <see cref="TextBoxBase.ReadOnly"/> state.
        /// </summary>
        [Description("The background color in the current Enabled/ReadOnly state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (!ReadOnly && Enabled)
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current state.
        /// </summary>
        [Description("The text color in the current Enabled state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (Enabled)
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/> and not <see cref="TextBoxBase.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the background color when the control is Enabled and not ReadOnly.")]
        public Color EnabledBackColor
        {
            get => !enabledBackColor.IsEmpty ? enabledBackColor : defaultEnabledBackColor;
            set
            {
                if (enabledBackColor == value)
                    return;
                enabledBackColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor
                : ReadOnly ? defaultReadOnlyForeColor
                : defaultEnabledForeColor;
            set
            {
                if (enabledForeColor == value)
                    return;
                enabledForeColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/> or is <see cref="TextBoxBase.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the background when the control is not Enabled or is ReadOnly.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : defaultDisabledOrReadOnlyBackColor;
            set
            {
                if (disabledBackColor == value)
                    return;
                disabledBackColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the text color when the control is not Enabled.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
                CheckStyles();
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Font"/> should be automatically scaled when DPI changes and the current thread has per-monitor DPI awareness.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see langword="true"/>, the <see cref="Font"/> is automatically scaled to the current DPI of the corresponding display on every executing platform.
        /// It also ensures that without an explicitly set font it is inherited from <see cref="Control.Parent"/>, which would be the normal behavior, but is broken in .NET 6+ and above.</para>
        /// <para>When <see langword="false"/>, the <see cref="Font"/> may or may not be scaled, and the font of the parent control may or may not be applied correctly, depending on the default behavior of the executing platform.</para>
        /// <note>Please note that this property affects the font only. Scaling the size and location always depends on the executing platform behavior.</note>
        /// </remarks>
        [Category("AdvancedTextBox")]
        [DefaultValue(true)]
        [Description("True to auto scale Font when DPI changes and inherit the font when it's not explicitly set; False to rely on the default behavior of the current executing platform.")]
        public bool AutoScaleFont
        {
            get => autoScaleFont;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (autoScaleFont == value)
                    return;

                autoScaleFont = value;
                font?.ResetFrom(font.Font, value ? this.GetScale() : ScaleHelper.SystemScale);
                if (value)
                {
                    Control? parent = Parent;
                    defaultFont = new ScalingFont(ScaleHelper.GetFontOrDefault(parent?.Font), parent?.GetScale() ?? ScaleHelper.SystemScale);

                    // theoretically this would not be needed, but in .NET 6+ the default font handling gets broken after the first DPI change
                    SetFont(font ?? defaultFont);
                    return;
                }

                defaultFont?.Dispose();
                defaultFont = null;
                if (font == null)
                    base.Font = null!;
            }
        }

        /// <inheritdoc />
        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (dpiChangingCount > 0 && AutoScaleFont)
                    return;

                // resetting the default font; or null, when AutoScaleFont is false
                if (value is null)
                {
                    font?.Dispose();
                    font = null;
                    Control? parent = Parent;
                    PointF parentScale = parent?.GetScale() ?? ScaleHelper.SystemScale;
                    defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent?.Font), parentScale);
                    SetFont(defaultFont);
                    return;
                }

                // setting a font explicitly - always setting base.Font, even if it is the same as value
                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;
                if (font == null)
                    font = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                else
                    font.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                SetFont(font);
            }
        }

        #endregion

        #region Constructors

        ///<summary>
        /// Creates a new instance of <see cref="AdvancedTextBox"/>.
        ///</summary>
        public AdvancedTextBox()
        {
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CheckDpiChange();
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            CheckStyles();
            ResetColors();
        }

        /// <inheritdoc/>
        protected override void OnReadOnlyChanged(EventArgs e)
        {
            base.OnReadOnlyChanged(e);
            ResetColors();
        }

        /// <inheritdoc/>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            origValue = Text;
        }

        /// <inheritdoc/>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (origValue != Text)
                OnTextChangedOnLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        protected virtual void OnTextChangedOnLeave(EventArgs e) => Events.GetHandler<EventHandler>(nameof(TextChangedOnLeave))?.Invoke(this, e);

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled)
            {
                base.OnPaint(e);
                return;
            }

            // Painting with disabled colors
            var clientRect = ClientRectangle;
            e.Graphics.FillRectangle(BackColor.GetBrush(), clientRect);

            Rectangle textRect = clientRect;
            TextFormatFlags flags = this.GetFormatFlags();
            textRect.Inflate(-1, -1);

            // The multiline textbox is rendered with some settings that are impossible to reproduce with TextFormatFlags.
            // The current settings are adjusted for Segoe UI, which is the default font in .NET Core, but the padding apparently changes from font to font.
            // To minimize chance of the visual difference, we do manual painting only when DisabledForeColor is different from the default color.
            if (Multiline)
                textRect.Width += 1;
            if (!UseSystemPasswordChar)
                TextRenderer.DrawText(e.Graphics, Text.Substring(GetFirstCharIndexFromLine(GetFirstVisibleLine())), Font, textRect, ForeColor, flags);
            else
                TextRenderer.DrawText(e.Graphics, new String(PasswordChar, Text.Length), Font, textRect, ForeColor, flags);
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    CheckDpiChange();
                    base.WndProc(ref m);
                    return;

                case Constants.WM_DPICHANGED_BEFOREPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }

                    CheckDpiChange();
                    return;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Control? parent = Parent;
            if (parent == null)
                return;

            // Setting default font from new parent font without scaling
            if (font == null)
            {
                PointF scale = this.GetScaleForParentChanged();
                defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent.Font), scale);
                if (this.GetScale() != scale)
                    lastScale = PointF.Empty;
            }

            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // if the parent control is rescaling its font due to DPI change, then ignoring the event (we do our scaling in CheckDpiChange)
            if (dpiChangingCount > 0 || !AutoScaleFont)
                return;

#if NET47_OR_GREATER || NETCOREAPP
            // The parent is rescaling its font out of a WM_DPICHANGED event (occurs typically in .NET 7+ during form handle creation)
            if (this.IsParentScalingWhileCreated())
                return;
#endif

            // but if the parent font is changing not because of scaling, then we reset our default font as well
            PointF scale = this.GetScaleForParentFontChanged();
            defaultFont!.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), scale);

            if (font != null)
                return;

            // setting default font from new parent font without scaling
            SetFont(defaultFont);

            // the parent has different scale: invalidating lastScale, so CheckDpiChange will adjust the scale if needed
            if (this.GetScale() != scale)
                lastScale = PointF.Empty;
        }

        /// <inheritdoc/>
        protected override bool IsInputKey(Keys keyData)
        {
            if (Multiline && ReadOnly)
            {
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Return when (keyData & Keys.Alt) == 0:
                        return false;
                    case Keys.Tab when (keyData & Keys.Control) == 0:
                        return false;
                }
            }

            return base.IsInputKey(keyData);
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.A when ShortcutsEnabled:
                    SelectAll();
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                font?.Dispose();
                defaultFont?.Dispose();
                font = null;
                defaultFont = null;
            }

            base.Dispose(disposing);
            if (disposing)
                Events.Dispose();
        }

        #endregion

        #region Private Methods

        private void CheckStyles() => SetStyle(ControlStyles.UserPaint, !Enabled && DisabledForeColor != defaultDisabledForeColor);

        private int GetFirstVisibleLine() => OSHelper.IsWindows
            ? User32.SendMessage(Handle, Constants.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32()
            : GetLineFromCharIndex(GetCharIndexFromPosition(Point.Empty));

        private void ResetColors()
        {
            bool enabled = Enabled;
            bool readOnly = ReadOnly;
            Color baseBackColor = base.BackColor;
            Color baseForeColor = base.ForeColor;

            if (enabled && !readOnly && EnabledBackColor is Color enabledBgColor && enabledBgColor != baseBackColor)
                base.BackColor = enabledBgColor;
            else if ((readOnly || !enabled) && DisabledBackColor is Color disabledBgColor && disabledBgColor != baseBackColor)
                base.BackColor = disabledBgColor;

            if (enabled && EnabledForeColor is Color enabledFgColor && enabledFgColor != baseForeColor)
                base.ForeColor = enabledFgColor;
            else if (!enabled && DisabledForeColor is Color disabledFgColor && disabledFgColor != baseForeColor)
                base.ForeColor = disabledFgColor;
        }

        private bool ShouldSerializeFont() => font != null;
        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();

            // The Font check is needed for .NET 6, where WinForms' (bad) auto font scaling may occur without notification
            if ((scale == lastScale && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            lastScale = scale;
            if (!AutoScaleFont)
                return;

            if (font is ScalingFont explicitFont)
                explicitFont.Scale(scale);
            else
                defaultFont!.Scale(scale);
            SetFont(font ?? defaultFont);
        }

        private void SetFont(ScalingFont? value)
        {
            if (value == null)
            {
                base.Font = null!;
                return;
            }

            Font oldFont = base.Font;
            Font newFont = value.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            // No optimization with reference equality for the AdvancedTextBox, because it can happen that the displayed font size is different
            // from the one that the base.Font property returns. Occurs typically in .NET 6/7/8 if Form.StartPosition is WindowsDefaultLocation
            // and the form is opened on a different screen with a different DPI than the owner form's screen.
            if (Equals(oldFont, newFont))
            {
                suppressFontChanged = true;
                try
                {
                    base.Font = null!;

                    // setting base.Font caused reentrancy: not letting the outer call to set the font again
                    if (!suppressFontChanged)
                        return;
                }
                finally
                {
                    suppressFontChanged = false;
                }
            }

            base.Font = newFont;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanging()
        {
            dpiChangingCount += 1;
            if (isPerMonitorDpiAwarenessV1)
                CheckDpiChange();
        }

        void IPerMonitorDpiAware.ParentFormDpiChanged()
        {
            Debug.Assert(dpiChangingCount > 0);
            dpiChangingCount -= 1;
        }

        #endregion

        #endregion
    }
}
