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
using System.Windows.Forms.VisualStyles;

using KGySoft.CoreLibraries;
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
    /// <item>When rendering with visual styles on high DPI, preferring always the standard check box sizes.
    /// When the standard size cannot be used, improving the rendering quality if <see cref="CheckBoxRenderingQuality"/> is <see cref="RenderingQuality.High"/>.</item>
    /// </list>
    /// </remarks>
    [Description(@"A date-time picker provides the following features in addition to regular DateTimePicker:
- Adjustable colors in disabled state
- Value property is redefined to return DateTime.MaxValue if Checked is false; instead of throwing exceptions, out-of-range values don't change Value
- Auto scaling Font on all platform targets
- Adjustable checkbox rendering quality")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedDateTimePicker : DateTimePicker, ISupportsDisabledColor, IPerMonitorDpiAware
    {
        #region Nested Types

        private sealed class LayoutData
        {
            #region Fields

            internal readonly Rectangle BackgroundBounds;
            internal readonly Rectangle CheckBoxBounds;
            internal readonly Rectangle TranslatedCheckBoxBounds;
            internal readonly Rectangle DropDownBounds;
            internal readonly Rectangle TranslatedDropDownBounds;
            internal readonly Rectangle TextBounds;
            internal readonly bool IsCalendarDropDown;
            internal readonly bool IsRightToLeft;

            #endregion

            #region Constructors

            internal LayoutData(AdvancedDateTimePicker control, Graphics g)
            {
                // 1. background
                Rectangle bounds = control.ClientRectangle;
                Rectangle textRect = bounds;
                bool rtl = IsRightToLeft = control.RightToLeftLayout && control.RightToLeft == RightToLeft.Yes;

                // When EnableVisualStyles was called, the border belongs to the client area (even if visual styles are actually not available),
                // so we could omit this if VisualStyleHelper.InitializedWithVisualStyles is false,
                // but apparently the system rendering applies the same padding to the client rectangle as well.
                textRect.Inflate(-2, -2);
                BackgroundBounds = textRect;

                // 2. check box
                int checkBoxPadding = control.ShowCheckBox ? textRect.Height + 1 : 0;
                if (checkBoxPadding > 0)
                {
                    CheckBoxBounds = new Rectangle(textRect.X, textRect.Y, checkBoxPadding - 1, checkBoxPadding - 1);

                    if (!VisualStyleHelper.InitializedWithVisualStyles)
                    {
                        CheckBoxBounds.Width -= 1;
                        CheckBoxBounds.Height -= 1;
                    }
                    else if (!VisualStyleHelper.RenderWithVisualStyles)
                        CheckBoxBounds.Inflate(-1, -1);

                    textRect.Width -= checkBoxPadding;
                    TranslatedCheckBoxBounds = CheckBoxBounds;

                    // Strange visual style renderer behavior: in RTL mode it mirrors the X coordinates so we always must pretend if the checkbox was on the left side.
                    // Does not happen with ControlPaint though, so without visual styles in RTL mode we need to use translated coordinates.
                    if (rtl)
                        TranslatedCheckBoxBounds.X = textRect.Right + 1;
                    else
                        textRect.X += checkBoxPadding;
                }

                // 3. drop down
                int dropDownSize = control.ScaleWidth(referenceDropDownWidth);

                // checking if we have enough space for the wider calendar drop down button
                if (VisualStyleHelper.RenderWithVisualStyles && !control.ShowUpDown && OSHelper.IsWindowsVistaOrLater)
                {
                    int textWidth = TextRenderer.MeasureText(g, control.Text, control.Font, Size.Empty, control.GetFormatFlags()).Width;
                    if (textWidth + dropDownSize * 2 <= textRect.Width)
                    {
                        IsCalendarDropDown = true;
                        dropDownSize <<= 1;
                    }
                }

                if (!control.ShowUpDown)
                {
                    bool fullHeight = !VisualStyleHelper.InitializedWithVisualStyles || VisualStyleHelper.RenderWithVisualStyles && OSHelper.IsWindowsVistaOrLater;

                    // Strange visual style renderer behavior: in RTL mode it mirrors the X coordinates AND the glyph image.
                    // The image mirroring does not happen for the checkbox rendering though. And ControlPaint does not mirror the X coordinate either.
                    DropDownBounds = new Rectangle(fullHeight ? bounds.Right - dropDownSize : BackgroundBounds.Right - dropDownSize,
                        fullHeight ? 0 : textRect.Y, dropDownSize, fullHeight ? bounds.Height : textRect.Height);
                    TranslatedDropDownBounds = DropDownBounds;
                    if (rtl)
                        TranslatedDropDownBounds.X = fullHeight ? 0 : BackgroundBounds.X;
                }

                // 4. text
                textRect.Width -= dropDownSize;
                if (rtl)
                    textRect.X += dropDownSize;

                // Even stranger TextRenderer behavior: Somehow it recognizes the RTL layout (is it in the native DC somewhere?)
                // so we have to undo the translation that we made for the calculations above.
                // This behavior is different from every other custom rendering that we use with TextRenderer and GetFormatFlags.
                // Note that if we use g.DrawString instead, it needs the original flags and the original rectangle.
                //TranslatedTextBounds = textRect; // TODO: uncomment if it will be needed, e.g. if Mono does not do the translation
                if (rtl)
                    textRect.X -= dropDownSize - checkBoxPadding;

                TextBounds = textRect;
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int referenceDropDownWidth = 17;

        #endregion

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

        private RenderingQuality checkBoxRenderingQuality = RenderingQuality.High;
        private bool isHovered;
        private bool isDropDownHovered;
        private bool isPressed;
        private bool isDroppedDown;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

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

        /// <summary>
        /// Gets or sets the rendering quality of the checkbox visuals when <see cref="DateTimePicker.ShowCheckBox"/> is <see langword="true"/> and visual styles are enabled.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Gets or sets the rendering quality of the check box visuals. Has effect only when ShowCheckBox is true, and rendering with visual styles.")]
        [DefaultValue(RenderingQuality.High)]
        public RenderingQuality CheckBoxRenderingQuality
        {
            get => checkBoxRenderingQuality;
            set
            {
                if (checkBoxRenderingQuality == value)
                    return;

                if (!Enum<RenderingQuality>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                checkBoxRenderingQuality = value;
                Invalidate();
            }
        }


        #endregion

        #region Private Properties

        private bool IsCustomCalendarSize => VisualStyleHelper.RenderWithVisualStyles && !ShowUpDown && (!Focused || (ShowCheckBox && !Checked));

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="AdvancedDateTimePicker"/> instance.
        /// </summary>
        public AdvancedDateTimePicker()
        {
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
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
                    // Needed because there is no [On]CheckedChanged.
                    // It's important that it's before the base.WndProc call, so there will not be extra paint if color changes cause invalidation.
                    ResetColors();
                    CheckDpiChange();

                    // On Vista and above the calendar button can be either a combo box drop down button or the regular calendar button, depending on the text length.
                    // As it's practically impossible to tell the actual button type of the system rendering, we always draw the non-Focused appearance ourselves with our preference.
                    bool fullCustomPaint = isDroppedDown || !Focused || ShowCheckBox && !Checked;
                    if (fullCustomPaint)
                        User32.ValidateRect(m.HWnd, IntPtr.Zero);
                    else
                        base.WndProc(ref m);

                    // Accepting system rendering if the control is focused, rendering without visual styles and RTL mode is not enabled.
                    bool rtl = RightToLeftLayout && RightToLeft == RightToLeft.Yes;

                    using (Graphics g = Graphics.FromHwnd(m.HWnd))
                    {
                        // Strange behavior: if the control is RTL, VisibleClipBounds.X is -1 so the calculated rects are off by one pixel. Fixing it in a compatible way.
                        if (g.VisibleClipBounds.X < 0)
                            g.TranslateTransform(g.VisibleClipBounds.X, g.VisibleClipBounds.Y);

                        var layout = new LayoutData(this, g);

                        // 1. Background and border
                        if (fullCustomPaint)
                            PaintBackground(g, layout);

                        // 2. Check box: When visual styles are enabled, reflecting the hovered state.
                        //    Otherwise, fixing RTL appearance (the borders would be mirrored)
                        if (ShowCheckBox && (fullCustomPaint || rtl || VisualStyleHelper.RenderWithVisualStyles))
                            PaintCheckBox(g, layout, !fullCustomPaint);

                        // 3. Drop down button. With visual styles we may use the wider calendar drop down button more likely than the native rendering.
                        //    With no visual styles we fix the RTL appearance - except when initializing without visual styles, because the button may be redrawn outside a WM_PAINT message...
                        if (!ShowUpDown && (fullCustomPaint || !VisualStyleHelper.RenderWithVisualStyles))
                            PaintDropDownButton(g, layout);

                        // 4. Text. Clearing the Right flag because TextRenderer recognizes The RTL layout somehow and always expects Left alignment.
                        // If we were using Graphics.DrawString with the ToStringFormat extension, the Right flag should not be cleared.
                        if (fullCustomPaint)
                            TextRenderer.DrawText(g, Text, Font, layout.TextBounds, ForeColor, BackColor, this.GetFormatFlags() & ~TextFormatFlags.Right);
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

                    CheckDpiChange();
                    return;

                // If we use the wider calendar drop down button when the system rendering would use the smaller one, we need to adjust the mouse position to make sure
                // to open/close the calendar. If the control is just getting focused, the appearance may change to the narrower button, but it's alright.
                case Constants.WM_LBUTTONDOWN when isDropDownHovered:
                    m.LParam = new IntPtr((m.LParam & (nint)0xFFFF0000) | ((nint)(uint)Width - 5));
                    isPressed = true;
                    base.WndProc(ref m);
                    return;

                case Constants.WM_LBUTTONUP when isDropDownHovered:
                    base.WndProc(ref m);
                    isPressed = false;
                    if (!VisualStyleHelper.RenderWithVisualStyles)
                        Invalidate();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }

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

#if NET6_0_OR_GREATER
            // The parent is rescaling its font due to DPI change without (or before the first) WM_DPICHANGED_BEFOREPARENT message.
            // Occurs in .NET 6+ when the DPI of the primary display was changed after starting the application, but before opening the parent form.
            // Actually works in .NET 7+ only, because in .NET 6 all DeviceDpi are already the new DPI, while Parent.Font is still the old one, despite the event.
            // We accept the broken behavior in .NET 6, because the standard controls are also broken the same way, and we don't need to target .NET 7 specifically just because of this.
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
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = isDropDownHovered = false;
            if (isPressed && !VisualStyleHelper.RenderWithVisualStyles)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            isDropDownHovered = false;
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsCustomCalendarSize && VisualStyleHelper.RenderWithVisualStyles)
                return;

            // We may render the wider calendar drop down button under different conditions,
            // so we need to invalidate the control if its hover state changes according to our custom rendering.
            LayoutData layout;
            using (var g = Graphics.FromHwnd(Handle))
                layout = new LayoutData(this, g);

            bool dropDownHovered = layout.DropDownBounds.Contains(e.Location);
            if (isDropDownHovered == dropDownHovered)
                return;

            isDropDownHovered = dropDownHovered;
            if (VisualStyleHelper.RenderWithVisualStyles)
                Invalidate();
            else
            {
                bool pressed = dropDownHovered && MouseButtons == MouseButtons.Left;
                if (pressed != isPressed)
                    Invalidate();
                isPressed = pressed;
            }
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            Invalidate();
            base.OnSizeChanged(e);
        }

        /// <inheritdoc />
        protected override void OnDropDown(EventArgs eventargs)
        {
            base.OnDropDown(eventargs);
            isDroppedDown = true;
            isDropDownHovered = false;
            if (!VisualStyleHelper.InitializedWithVisualStyles)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnCloseUp(EventArgs eventargs)
        {
            base.OnCloseUp(eventargs);
            isDroppedDown = isPressed = false;
            Invalidate();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
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

        private void PaintBackground(Graphics g, LayoutData layout)
        {
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                int state = (int)(!Enabled ? DATEPICKERSTATES.DPS_DISABLED
                    : isHovered ? DATEPICKERSTATES.DPS_HOT
                    : DATEPICKERSTATES.DPS_NORMAL);

                if (OSHelper.IsWindowsVistaOrLater)
                    VisualStyleHelper.Render(VisualStyleHelper.DatePickerTheme, this, g, (int)DATEPICKERPARTS.DP_DATEBORDER, state, ClientRectangle);
                else // Windows XP: there is no DatePicker theme, using the COMBOBOX instead with Part 0 (EDIT 2 could also work but the disabled state has a strange background)
                    VisualStyleHelper.Render(VisualStyleHelper.ComboBoxTheme, this, g, (int)COMBOBOXPARTS.CP_COMPATIBLEBACKGROUND, state, ClientRectangle);

                // Clearing the background only in disabled state or when a custom back color is specified; otherwise, preserving the theme back color
                if (state == (int)DATEPICKERSTATES.DPS_DISABLED || (ShowCheckBox && !Checked) || !enabledBackColor.IsEmpty)
                    g.FillRectangle(BackColor.GetBrush(), layout.BackgroundBounds);
                return;
            }

            g.Clear(BackColor);

            // If the application was initialized with visual styles (even if they are not enabled), the borders are in the client area, so we need to draw them.
            // Otherwise, the border belongs to the NC area.
            if (VisualStyleHelper.InitializedWithVisualStyles)
                ControlPaint.DrawBorder3D(g, ClientRectangle);
        }

        private void PaintCheckBox(Graphics g, LayoutData layout, bool paintBackground)
        {
            Debug.Assert(ShowCheckBox);
            
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                var checkState = Checked
                    ? !Enabled ? CheckBoxState.CheckedDisabled
                        : isHovered ? CheckBoxState.CheckedHot
                        : CheckBoxState.CheckedNormal
                    : !Enabled ? CheckBoxState.UncheckedDisabled
                        : isHovered ? CheckBoxState.UncheckedHot
                        : CheckBoxState.UncheckedNormal;

                // When the control is not fully custom painted, we already have the system painted checkbox, potentially with different size and quality.
                // Using the Window system color is alright, because we do the clearing only when the control is focused, in which case no custom back color is used.
                if (paintBackground)
                    g.FillRectangle(SystemBrushes.Window, layout.TranslatedCheckBoxBounds);

                Size actualSize = VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, this, g, (int)BUTTONPARTS.BP_CHECKBOX, (int)checkState, true);
                Size drawnSize = layout.CheckBoxBounds.Height < actualSize.Height
                    ? layout.CheckBoxBounds.Size
                    : VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, this, g, (int)BUTTONPARTS.BP_CHECKBOX, (int)checkState, false);
                if (drawnSize.Height > layout.CheckBoxBounds.Height)
                    drawnSize = layout.CheckBoxBounds.Size;

                Rectangle drawnBounds = layout.CheckBoxBounds;
                int diff = (layout.CheckBoxBounds.Height - drawnSize.Height + 1) >> 1;
                drawnBounds.Inflate(-diff, -diff);
                drawnBounds.Size = drawnSize;

                if (checkBoxRenderingQuality == RenderingQuality.High && (drawnSize != actualSize))
                {
                    // As we started from layout.CheckBoxBounds, we do the translation if needed.
                    // No idea why the +2 is needed when drawing images on RTL Graphics, the FillRectangle above does not need it.
                    if (layout.CheckBoxBounds != layout.TranslatedCheckBoxBounds)
                        drawnBounds.X = Width - drawnBounds.Right + 2;
                    VisualStyleHelper.RenderScaled(VisualStyleHelper.ButtonTheme, this, g, (int)BUTTONPARTS.BP_CHECKBOX, (int)checkState, drawnBounds);
                }
                else
                    VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, this, g, (int)BUTTONPARTS.BP_CHECKBOX, (int)checkState, drawnBounds);
            }
            else
            {
                var checkState = ButtonState.Normal;
                if (!Enabled)
                    checkState |= ButtonState.Inactive;
                if (Checked)
                    checkState |= ButtonState.Checked;
                ControlPaint.DrawCheckBox(g, layout.TranslatedCheckBoxBounds, checkState);
            }
        }

        private void PaintDropDownButton(Graphics g, LayoutData layout)
        {
            Debug.Assert(!ShowUpDown);

            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                int state = (int)(!Enabled ? DATEPICKERSTATES.DPS_DISABLED
                    : isDroppedDown || isPressed ? DATEPICKERSTATES.DPS_FOCUSED
                    : isDropDownHovered ? DATEPICKERSTATES.DPS_HOT
                    : DATEPICKERSTATES.DPS_NORMAL);

                IntPtr theme = layout.IsCalendarDropDown ? VisualStyleHelper.DatePickerTheme : VisualStyleHelper.ComboBoxTheme;
                int part = layout.IsCalendarDropDown ? (int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT
                    : !OSHelper.IsWindowsVistaOrLater ? (int)COMBOBOXPARTS.CP_DROPDOWNBUTTON
                    : layout.IsRightToLeft ? (int)COMBOBOXPARTS.CP_DROPDOWNBUTTONLEFT : (int)COMBOBOXPARTS.CP_DROPDOWNBUTTONRIGHT;
                VisualStyleHelper.Render(theme, this, g, part, state, layout.DropDownBounds);
            }
            else
                ControlPaint.DrawComboButton(g, layout.TranslatedDropDownBounds, !Enabled ? ButtonState.Inactive : isPressed ? ButtonState.Pushed : ButtonState.Normal);

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
            if (scale == lastScale || Disposing || IsDisposed)
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
                if (ReferenceEquals(oldFont, newFont.Font))
                    return;

                // Non-reference equality: we are alright if the old font is not disposed...
                // ...except in .NET Core 3.0 - .NET 5.0 when using v1 per-monitor DPI awareness, in which case the font gets corrupted and does not change the size.
#if NETCOREAPP && !NET6_0_OR_GREATER
                if (!oldFont.IsDisposed() && !(OSHelper.IsWindows && !OSHelper.IsMono && ScaleHelper.PerMonitorDpiAwarenessVersion == 1))
#else
                if (!oldFont.IsDisposed())
#endif
                {
                    return;
                }

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

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => Invalidate();

        #endregion

        #endregion
    }
}
