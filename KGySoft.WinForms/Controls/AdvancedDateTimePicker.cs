#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedDateTimePicker.cs
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

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Advanced version of <see cref="DateTimePicker"/> control that provides some advanced features and fixes for the original <see cref="DateTimePicker"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="DateTimePicker"/> control offers the following features in addition to <see cref="DateTimePicker"/>:
    /// <list type="bullet">
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item>Its <see cref="Value"/> property is redefined so it returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.Checked"/> is <see langword="false"/> and
    /// instead of throwing exception when invalid date is assigned to it, it simpy changes <see cref="DateTimePicker.Checked"/> false (if checkbox is visible), or just ignores the value.</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [Description(@"A date-time picker provides the following features in addition to regular DateTimePicker:
- Adjustable colors in disabled state
- Value property is redefined to return DateTime.MaxValue if Checked is false; instead of throwing exceptions, out-of-range values don't change Value
- Auto scaling Font on all platform targets")]
    public class AdvancedDateTimePicker : DateTimePicker, ISupportsDisabledColor, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;

        #endregion

        #region Instance Fields

        // NOTE: Similar to AdvancedTextBox, we always set the base back (and fore) colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        // The control doesn't use the fore color in enabled state at all, even with disabled visual styles, and I don't even plan to implement it.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private bool dpiChanging;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        /// <value>Returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.ShowCheckBox"/> is <see langword="true"/> and <see cref="DateTimePicker.Checked"/> is false.</value>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new DateTime Value
        {
            get
            {
                if (ShowCheckBox && !Checked)
                    return DateTime.MaxValue;
                else
                    return base.Value;
            }
            set
            {
                // ignoring invalid value (e.g. when control is data bound, DateTime.MinValue may come)
                if (value < MinDate || value > MaxDate)
                {
                    if (ShowCheckBox)
                        Checked = false;
                }
                else
                    base.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> and <see cref="DateTimePicker.Checked"/> state.
        /// </summary>
        [Browsable(true)]
        [Description("The background color in the current Enabled/Checked state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (Enabled && (!ShowCheckBox || Checked))
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current <see cref="Control.Enabled"/> and <see cref="DateTimePicker.Checked"/> state.
        /// </summary>
        [Browsable(true)]
        [Description("The text color in the current Enabled/Checked state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (Enabled && (!ShowCheckBox || Checked))
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/> and not <see cref="TextBox.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
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
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor : defaultEnabledForeColor;
            set
            {
                if (enabledForeColor == value)
                    return;
                enabledForeColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/> or is <see cref="TextBox.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the background when the control is not Enabled or is ReadOnly.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : defaultDisabledBackColor;
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
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the text color when the control is not Enabled.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
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
        [Category("AdvancedDateTimePicker")]
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
                PointF scale = value ? this.GetScale() : ScaleHelper.SystemScale;
                font?.ResetFrom(font.Font, scale);
                if (value)
                {
                    defaultFont = new ScalingFont(ScaleHelper.GetFontOrDefault(Parent?.Font), scale);

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
                if (ReferenceEquals(base.Font, value))
                    return;

                // Workaround for .NET Framework 4.7+ behavior when V2 awareness is set both in the app.config and the manifest file:
                // The base WM_DPICHANGED_BEFOREPARENT handling sets the Font property, in which case we want to avoid setting font if it was null.
                // .NET Core 3.0+ behaves differently: sets the Font only in base and even calls OnFontChanged but does not set the derived property.
                if (dpiChanging && AutoScaleFont)
                    return;

                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;

                // resetting the default font; or null, when AutoScaleFont is false
                if (value is null)
                {
                    font?.Dispose();
                    font = null;
                    defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), scale);
                    SetFont(defaultFont);
                    return;
                }

                // setting a font explicitly
                if (font == null)
                    font = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                else
                    font.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                SetFont(font);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="AdvancedDateTimePicker"/> instance.
        /// </summary>
        public AdvancedDateTimePicker()
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

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_ERASEBKGND: // matters only when visual styles are not enabled; otherwise, the background is redrawn by the system anyway
                    using (Graphics g = Graphics.FromHdc(m.WParam))
                        g.FillRectangle(BackColor.GetBrush(), ClientRectangle);
                    return;

                case Constants.WM_PAINT:
                    if (Focused)
                    {
                        CheckDpiChange();
                        base.WndProc(ref m);
                        return;
                    }

                    // Needed because there is no [On]CheckedChanged.
                    // It's important that it's before the base.WndProc call, so there will not be extra paint if color changes cause invalidation.
                    ResetColors();

                    CheckDpiChange();
                    base.WndProc(ref m);
                    Rectangle rect = ClientRectangle;

                    // When EnableVisualStyles was called, the border belongs to the client area (even if visual styles are actually not available)
                    if (VisualStyleHelper.InitializedWithVisualStyles)
                        rect.Inflate(-2, -2);

                    int paddingLeft = VisualStyleHelper.RenderWithVisualStyles
                        ? ShowCheckBox ? 17 : 0
                        : ShowCheckBox ? 19 : 0;
                    int paddingRight = VisualStyleHelper.RenderWithVisualStyles
                        ? ShowUpDown ? 18 : 33
                        : 17;
                    bool rtl = RightToLeftLayout && RightToLeft == RightToLeft.Yes;
                    if (rtl)
                        (paddingLeft, paddingRight) = (paddingRight, paddingLeft);
                    rect.X += paddingLeft;
                    rect.Width -= paddingLeft + paddingRight;
                    using (Graphics g = Graphics.FromHwnd(Handle))
                    {
                        // Strange behavior: if the control is RTL, VisibleClipBounds.X is -1 so the calculated rect is off by one pixel.
                        // Fixing it in a compatible way.
                        if (g.VisibleClipBounds.X < 0)
                            g.TranslateTransform(g.VisibleClipBounds.X, g.VisibleClipBounds.Y);
                        var flags =  this.GetFormatFlags();
                        g.FillRectangle(BackColor.GetBrush(), rect);

                        // Even stranger TextRenderer behavior: Somehow it recognizes the RTL layout (is it in the native DC somewhere?)
                        // so we have to undo the translation that we made for filling the background.
                        // This behavior is different from every other custom rendering that we use with TextRenderer and GetFormatFlags
                        if (rtl)
                            rect.X -= paddingLeft - paddingRight;
                        TextRenderer.DrawText(g, Text, Font, rect, ForeColor, BackColor, flags & ~TextFormatFlags.Right);

                        // Note that if we use g.DrawString instead, it needs the original flags and the original rectangle.
                        //g.DrawString(Text, Font, ForeColor.GetBrush(), rect, flags.ToStringFormat());
                    }

                    return;

                case Constants.WM_DPICHANGED_BEFOREPARENT:
                    dpiChanging = true;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChanging = false;
                    }

                    return;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    base.WndProc(ref m);
                    CheckDpiChange();
                    if (AutoSize)
                        PerformLayout();
                    return;
            }

            base.WndProc(ref m);
        }

        /// <inheritdoc />
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (!VisualStyleHelper.RenderWithVisualStyles)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // Setting default font from new parent font without scaling (using current scaling of the new parent), and then
            // calling CheckDpiChange so if there is an explicitly set font, it will be scaled to the new parent.
            if (font == null)
                defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), this.GetScale());
            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // if the parent control is rescaling its font due to DPI change, then ignoring the event (we do our scaling in CheckDpiChange)
            if (dpiChanging || !AutoScaleFont)
                return;

#if NET7_0_OR_GREATER
            // The parent is rescaling its font due to DPI change without (or before the first) WM_DPICHANGED_BEFOREPARENT message.
            // Occurs in .NET 7+ when the DPI of the primary display was changed after starting the application, but before opening the parent form.
            int deviceDpi = DeviceDpi;
            if (Parent is Control parent && parent.DeviceDpi != deviceDpi || TopLevelControl is Control top && top.DeviceDpi != deviceDpi)
                return;
#endif

            // but if the parent font is changing not because of scaling, then we reset our default font as well
            defaultFont!.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), this.GetScale());

            // if font is null, setting default font from new parent font without scaling
            if (font == null)
                SetFont(defaultFont);
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
        }

        #endregion

        #region Private Methods

        private void ResetColors()
        {
            bool enabled = Enabled && (!ShowCheckBox || Checked);
            Color baseBackColor = base.BackColor;
            Color baseForeColor = base.ForeColor;

            if (enabled && EnabledBackColor is Color enabledBgColor && enabledBgColor != baseBackColor)
                base.BackColor = enabledBgColor;
            else if (!enabled && DisabledBackColor is Color disabledBgColor && disabledBgColor != baseBackColor)
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
            if (scale == lastScale)
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

        private void SetFont(ScalingFont? newFont)
        {
            if (newFont == null)
            {
                base.Font = null!;
                return;
            }

            Font oldFont = base.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            if (Equals(oldFont, newFont.Font))
            {
                if (ReferenceEquals(oldFont, newFont.Font) || !oldFont.IsDisposed())
                    return;

                suppressFontChanged = true;
                try
                {
                    base.Font = null!;
                }
                finally
                {
                    suppressFontChanged = false;
                }
            }

            base.Font = newFont.Font;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanged() => CheckDpiChange();

        #endregion

        #endregion
    }
}
