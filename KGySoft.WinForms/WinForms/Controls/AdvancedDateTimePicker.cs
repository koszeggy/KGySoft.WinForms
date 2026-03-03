#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedDateTimePicker.cs
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.CoreLibraries;
using KGySoft.WinForms.Reflection;
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
            internal readonly Rectangle UpDownBounds; // Only on Mono with visual styles, represents the drawn bounds.
            internal readonly Rectangle TranslatedDropDownBounds;
            internal readonly Rectangle TextBounds;
            internal readonly int HorizontalOffset;
            internal readonly bool IsCalendarDropDown;
            internal readonly bool IsRightToLeft;

            #endregion

            #region Constructors

            internal LayoutData(AdvancedDateTimePicker control, Graphics g)
            {
                // Strange behavior: if the control is RTL, VisibleClipBounds.X is -1 so the calculated rects are off by one pixel.
                // Cannot fix it simply by g.TranslateTransform, because it affects only GDI+ draw operations, but not theme drawing or some ControlPaint methods, so not applying it globally.
                if (g.VisibleClipBounds.X < 0)
                    HorizontalOffset = (int)g.VisibleClipBounds.X;

                // 1. background
                Rectangle bounds = control.ClientRectangle;
                Rectangle textRect = bounds;
                bool rtl = IsRightToLeft = control.RightToLeftLayout && control.RightToLeft == RightToLeft.Yes && !OSHelper.IsFrameworkMono;

                // When EnableVisualStyles was called on Vista+, the border belongs to the client area (even if visual styles are actually not available),
                // so we could omit this if VisualStyleHelper.InitializedWithVisualStyles is false,
                // but apparently the system rendering applies the same padding to the client rectangle as well.
                textRect.Inflate(-2, -2);
                BackgroundBounds = textRect;

                // 2. check box
                int checkBoxPadding = !control.ShowCheckBox ? 0 
                    : OSHelper.IsFrameworkMono ? 21
                    : OSHelper.IsWine ? bounds.Height
                    : textRect.Height + 1;
                if (checkBoxPadding > 0)
                {
                    if (OSHelper.IsRealWindows)
                    {
                        // Real Windows: there is no actual checkbox inside the control, it's just drawn.
                        // So we specify CheckBoxBounds to be drawn manually, and we try to use the same location as Windows uses.
                        CheckBoxBounds = new Rectangle(textRect.X, textRect.Y, checkBoxPadding - 1, checkBoxPadding - 1);
                        if (OSHelper.IsWindowsVistaOrLater)
                        {
                            if (!VisualStyleHelper.InitializedWithVisualStyles)
                            {
                                CheckBoxBounds.Width -= 1;
                                CheckBoxBounds.Height -= 1;
                            }
                            else if (!VisualStyleHelper.RenderWithVisualStyles)
                                CheckBoxBounds.Inflate(-1, -1);
                        }
                        else
                        {
                            if (VisualStyleHelper.RenderWithVisualStyles)
                                CheckBoxBounds.X -= 1;
                            else
                            {
                                CheckBoxBounds.Width -= 1;
                                CheckBoxBounds.Height -= 1;
                            }
                        }
                    }
                    else if (OSHelper.IsFrameworkMono)
                    {
                        // Framework Mono: The checkbox is aligned to the middle vertically, but not scaled.
                        CheckBoxBounds = new Rectangle(textRect.X, (textRect.Y + textRect.Height / 2) - (checkBoxPadding - 5) / 2, checkBoxPadding - 5, checkBoxPadding - 5);
                        if (!VisualStyleHelper.RenderWithVisualStyles)
                        {
                            CheckBoxBounds.Inflate(-1, -1);
                            CheckBoxBounds.X += 1;
                            CheckBoxBounds.Y += 1;
                        }
                    }
                    // else Wine: not setting CheckBoxBounds, because the control has a native checkbox that cannot be overdrawn, so just using the padding for the text.

                    textRect.Width -= checkBoxPadding;
                    TranslatedCheckBoxBounds = CheckBoxBounds;

                    // Strange visual style renderer behavior: in RTL mode it mirrors the X coordinates so we always must pretend if the checkbox was on the left side.
                    // Does not happen with ControlPaint though, so without visual styles in RTL mode we need to use translated coordinates.
                    if (rtl)
                        TranslatedCheckBoxBounds.X = textRect.Right + (VisualStyleHelper.InitializedWithVisualStyles ? 0 : 1);
                    else
                        textRect.X += checkBoxPadding;
                }

                // 3. drop down
                int dropDownSize = control.ScaleWidth(OSHelper.IsWine ? referenceDropDownWidthWine : referenceDropDownWidth);
                if (VisualStyleHelper.RenderWithVisualStyles && !control.ShowUpDown && OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.DatePickerTheme != IntPtr.Zero)
                {
                    if (OSHelper.IsWindowsMono)
                        IsCalendarDropDown = true;
                    else
                    {
                        // Checking if we have enough space for the wider calendar drop down button
                        int textWidth = TextRenderer.MeasureText(g, control.Text, control.Font, Size.Empty, control.GetFormatFlags()).Width;
                        if (textWidth + dropDownSize * 2 <= textRect.Width)
                            IsCalendarDropDown = true;
                    }

                    if (IsCalendarDropDown)
                        dropDownSize <<= 1;
                }

                if (control.ShowUpDown)
                {
                    // Only on Framework Mono. Otherwise, the up/down buttons are actual (native) child controls that cannot be overdrawn.
                    if (OSHelper.IsFrameworkMono)
                    {
                        BackgroundBounds.Width -= 17;
                        UpDownBounds = new Rectangle(BackgroundBounds.Right, BackgroundBounds.Top, 17, BackgroundBounds.Height);
                    }
                }
                else
                {
                    bool fullHeight = OSHelper.IsRealWindows ? !VisualStyleHelper.InitializedWithVisualStyles // EnableVisualStyles was not called: full client area, border is in the NC area
                            || !OSHelper.IsWindowsVistaOrLater // Windows XP: the border belongs to the NC even with visual styles
                            || VisualStyleHelper.RenderWithVisualStyles && OSHelper.IsWindowsVistaOrLater // Vista+ with visual styles: the calendar/drop/down occupies the border in the client area
                        : OSHelper.IsFrameworkMono ? OSHelper.IsWindowsMono && VisualStyleHelper.RenderWithVisualStyles // Framework Mono: border is always in the client area, so using full height with visual styles
                        : VisualStyleHelper.RenderWithVisualStyles || bounds.Height != control.Height; // other (e.g. Wine): using visual styles, or there is an NC area

                    // Strange visual style renderer behavior: in RTL mode it mirrors the X coordinates AND the glyph image.
                    // The image mirroring does not happen for the checkbox rendering though. And ControlPaint does not mirror the X coordinate either.
                    DropDownBounds = new Rectangle(fullHeight ? bounds.Right - dropDownSize : BackgroundBounds.Right - dropDownSize,
                        fullHeight ? 0 : textRect.Y, dropDownSize, fullHeight ? bounds.Height : textRect.Height);

                    // Excluding the drop-down button from the background area on Framework Mono with no visual styles,
                    // so even full repaint leaves the default drop-down drawing remain intact.
                    if (OSHelper.IsFrameworkMono && !VisualStyleHelper.RenderWithVisualStyles)
                        BackgroundBounds.Width -= DropDownBounds.Width;
                    TranslatedDropDownBounds = DropDownBounds;
                    if (rtl)
                        TranslatedDropDownBounds.X = fullHeight ? HorizontalOffset : BackgroundBounds.X + HorizontalOffset;
                }

                // 4. text
                textRect.Width -= dropDownSize;
                if (rtl)
                    textRect.X += dropDownSize - HorizontalOffset;

                // Even stranger TextRenderer behavior: Somehow it recognizes the RTL layout (is it in the native DC somewhere?),
                // so we have to undo the translation that we made for the calculations above.
                // This behavior is different from every other custom rendering that we use with TextRenderer and GetFormatFlags.
                // Note that if we used g.DrawString instead, it would need the original flags and the original rectangle.
                //TranslatedTextBounds = textRect; // TODO: uncomment if it will be needed, e.g. if Mono will support RTL, and it does not do the translation
                if (rtl)
                    textRect.X -= dropDownSize - checkBoxPadding;
                else if (OSHelper.IsFrameworkMono)
                {
                    if (VisualStyleHelper.RenderWithVisualStyles)
                        textRect.Y -= 2;
                    else
                        textRect.X += 2;
                }

                TextBounds = textRect;
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int referenceDropDownWidth = 17;
        private const int referenceDropDownWidthWine = 15;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;

        private static readonly int isHovered = BitVector32.CreateMask();
        private static readonly int isDropDownHovered = BitVector32.CreateMask(isHovered);
        private static readonly int isPressed = BitVector32.CreateMask(isDropDownHovered);
        private static readonly int isDroppedDown = BitVector32.CreateMask(isPressed);
        private static readonly int isUpHovered = BitVector32.CreateMask(isDroppedDown);
        private static readonly int isDownHovered = BitVector32.CreateMask(isUpHovered);

        #endregion

        #region Instance Fields

        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        // NOTE: Similar to AdvancedTextBox, we always set the base back (and fore) colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        // The control doesn't use the fore color in enabled state at all, even with disabled visual styles, and I don't even plan to implement it.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;
        private int dpiChangingCount;

        private RenderingQuality checkBoxRenderingQuality = RenderingQuality.High;
        private BitVector32 flags;

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
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the background color when the control is Enabled.")]
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
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the background when the control is not Enabled.")]
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

        private bool IsCustomDropDownHovering => !ShowUpDown
            && ((OSHelper.IsRealWindows || OSHelper.IsWine) && VisualStyleHelper.RenderWithVisualStyles && (!Focused || (ShowCheckBox && !Checked)) // custom calendar width on real Windows or just tracking hovered status on Wine
                || OSHelper.IsWindowsMono && VisualStyleHelper.RenderWithVisualStyles // custom calendar height on Windows Mono
                || !VisualStyleHelper.RenderWithVisualStyles && !OSHelper.IsFrameworkMono); // fixing pressed rendering also in RTL mode with no visual styles - except in Framework Mono, whose ControlPaint draws a transparent button (and RTL is not supported anyway)

        private bool IsCustomUpDownBounds => OSHelper.IsWindowsMono && VisualStyleHelper.RenderWithVisualStyles && ShowUpDown; // fixing region on Mono/Windows with visual styles

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

            // Needed because in Framework Mono the base ctor calls the overridden BackColor/ForeColor setters
            if (OSHelper.IsFrameworkMono)
            {
                EnabledBackColor = default;
                EnabledForeColor = default;
            }
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

                    // - On Vista and above the calendar button can be either a combo box drop down button or the regular calendar button, depending on the text length.
                    //   As it's practically impossible to tell the actual button type of the system rendering, we always draw the non-Focused appearance ourselves with our threshold.
                    // - On Framework Mono with visual styles, the buttons are not scaled to a larger font, and also the up/down buttons are rendered incorrectly
                    // - On Framework Mono with no visual styles, we never paint the buttons, even with full custom paint
                    bool fullCustomPaint = flags[isDroppedDown] || !Focused || ShowCheckBox && !Checked;
                    if (fullCustomPaint && OSHelper.IsWindows && (!OSHelper.IsFrameworkMono || VisualStyleHelper.RenderWithVisualStyles))
                        User32.ValidateRect(m.HWnd, IntPtr.Zero);
                    else
                        base.WndProc(ref m);

                    bool rtl = RightToLeftLayout && RightToLeft == RightToLeft.Yes && !OSHelper.IsFrameworkMono;
                    using (Graphics g = Graphics.FromHwnd(m.HWnd))
                    {
                        var layout = new LayoutData(this, g);

                        // 1. Background and border
                        PaintBackground(g, layout, fullCustomPaint);

                        // 2. Check box: When visual styles are enabled, reflecting the hovered state. Otherwise, fixing RTL appearance (the borders would be mirrored)
                        //    NOTE: using CheckBoxBounds instead of ShowCheckBox, because bounds are empty if the original checkbox cannot be painted over (on Wine).
                        if (!layout.CheckBoxBounds.IsEmpty() && (fullCustomPaint || rtl || VisualStyleHelper.RenderWithVisualStyles))
                            PaintCheckBox(g, layout, !fullCustomPaint);

                        // 3.a. Drop-down button. With visual styles we may use the wider calendar drop down button more likely than the native rendering.
                        //    With no visual styles we fix the RTL appearance - except when initializing without visual styles, because the button may be redrawn outside a WM_PAINT message...
                        if (!ShowUpDown && (fullCustomPaint || !OSHelper.IsFrameworkMono && !VisualStyleHelper.RenderWithVisualStyles || OSHelper.IsFrameworkMono && VisualStyleHelper.RenderWithVisualStyles))
                            PaintDropDownButton(g, layout);
                        // 3.b. up/down button - only with Mono/Windows with visual styles, where it's totally broken
                        else if (!layout.UpDownBounds.IsEmpty())
                            PaintUpDownButton(g, layout); // On Mono with visual styles the Up/Down button has a terrible quality by default

                        // 4. Text. Clearing the Right flag because TextRenderer recognizes The RTL layout somehow and always expects Left alignment.
                        // If we were using Graphics.DrawString with the ToStringFormat extension, the Right flag should not be cleared.
                        if (fullCustomPaint)
                            TextRenderer.DrawText(g, Text, Font, layout.TextBounds, ForeColor, BackColor, this.GetFormatFlags() & ~TextFormatFlags.Right);
                    }

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

                // If we use the wider calendar drop down button when the system rendering would use the smaller one, we need to adjust the mouse position to make sure
                // to open/close the calendar. If the control is just getting focused, the appearance may change to the narrower button, but it's alright.
                case Constants.WM_LBUTTONDOWN:
                    // lParam: LO: X coordinate; HI: Y coordinate

                    if (flags.Any(isDropDownHovered | isUpHovered | isDownHovered))
                    {
                        flags[isPressed] = true;

                        // On .NET, we need to adjust the X coordinate to apply our threshold of wide/normal drop-down.
                        // On Mono, we need to adjust the Y coordinate to stretch the calendar down to the whole area.
                        // Setting the mouse to (Width - 5, 5) fixes both issues
                        if (flags[isDropDownHovered])
                            m.LParam = new IntPtr(0x00005_0000 | ((nint)(uint)Width - 5));
                        // For Up/Down buttons adjusting the coordinates only if we can get the internal Mono calculation for the up/down area. It's halved horizontally.
                        else if (this.DropDownArrowRect() is Rectangle upDownBounds)
                            m.LParam = new IntPtr(((upDownBounds.Top + (flags[isUpHovered] ? 0 : upDownBounds.Height - 1)) << 16) | ((nint)(uint)Width - 5));
                    }

                    base.WndProc(ref m);
                    return;

                case Constants.WM_LBUTTONUP when flags[isPressed]:
                    base.WndProc(ref m);
                    flags[isPressed] = false;
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
            if (!VisualStyleHelper.RenderWithVisualStyles || !OSHelper.IsWindowsVistaOrLater || !OSHelper.IsRealWindows)
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

        /// <inheritdoc />
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            bool dropDownHoveredChange = flags[isDropDownHovered];
            flags[isHovered | isDropDownHovered | isUpHovered | isDownHovered] = false;
            if (flags[isPressed] && !VisualStyleHelper.RenderWithVisualStyles || dropDownHoveredChange && VisualStyleHelper.RenderWithVisualStyles)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            flags[isHovered] = true;
            flags[isDropDownHovered | isUpHovered | isDownHovered] = false;
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool customDropDown = IsCustomDropDownHovering;
            bool customUpDown = IsCustomUpDownBounds;
            if (!(customDropDown || customUpDown))
                return;

            LayoutData layout;
            using (var g = Graphics.FromHwnd(Handle))
                layout = new LayoutData(this, g);

            // custom drop down: everywhere with visual styles, and everywhere but Framework Mono with no visual styles
            if (customDropDown)
            {
                bool dropDownHovered = layout.DropDownBounds.Contains(e.Location);
                if (flags[isDropDownHovered] == dropDownHovered && (!OSHelper.IsFrameworkMono || !flags[isPressed]))
                    return;

                flags[isDropDownHovered] = dropDownHovered;
                if (VisualStyleHelper.RenderWithVisualStyles)
                {
                    // Clearing the flag is relevant on Mono, where the OnCloseUp is not called. Receiving OnMouseMove means the calendar is no longer dropped.
                    flags[isPressed] = false;
                    Invalidate(layout.DropDownBounds);
                    return;
                }

                // No visual styles on .NET: while the left mouse button is pressed, we update the pressed state of the drop-down button.
                bool pressed = dropDownHovered && MouseButtons == MouseButtons.Left;
                if (pressed != flags[isPressed])
                    Invalidate(layout.DropDownBounds);
                flags[isPressed] = pressed;

                return;
            }

            if (Capture)
                return;

            // custom up/down: on Framework Mono with visual styles
            flags[isPressed] = false;
            bool upDownHovered = layout.UpDownBounds.Contains(e.Location) // custom drawn bounds (can be higher than drawn by Framework Mono)
                || this.DropDownArrowRect()?.Contains(e.Location) == true; // calculated bounds by Mono (can be wider and vertically shorter than actually drawn)

            // our vertically fixed hovered flags
            bool up = upDownHovered && e.Y < layout.UpDownBounds.Top + layout.UpDownBounds.Height / 2;
            bool down = upDownHovered && !up;
            if (flags[isUpHovered] == up && flags[isDownHovered] == down)
                return;

            flags[isUpHovered] = up;
            flags[isDownHovered] = down;
            Invalidate(layout.UpDownBounds);
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
            flags[isDroppedDown] = true;
            flags[isDropDownHovered] = false;
            if (!VisualStyleHelper.InitializedWithVisualStyles)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnCloseUp(EventArgs eventargs)
        {
            base.OnCloseUp(eventargs);
            flags[isDroppedDown | isPressed] = false;
            Invalidate();
        }

        /// <inheritdoc />
        protected override void OnLeave(EventArgs e)
        {
            // On Framework Mono the OnCloseUp is never called, so using this method as a workaround
            if (OSHelper.IsFrameworkMono)
            {
                flags[isPressed | isDroppedDown] = false;
                Invalidate();
            }

            base.OnLeave(e);
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
            if (disposing)
                Events.Dispose();
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

        private void PaintBackground(Graphics g, LayoutData layout, bool fullPaint)
        {
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                // partial paint with visual styles everywhere but on Framework Mono: omitting the background, and using the default drawing
                if ((!fullPaint && !OSHelper.IsFrameworkMono))
                    return;

                int state = (int)(!Enabled ? DATEPICKERSTATES.DPS_DISABLED
                    : flags[isHovered] ? DATEPICKERSTATES.DPS_HOT
                    : DATEPICKERSTATES.DPS_NORMAL);

                if (fullPaint)
                {
                    if (OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.DatePickerTheme != IntPtr.Zero) // both real Windows and Mono on Windows
                        VisualStyleHelper.Render(VisualStyleHelper.DatePickerTheme, this, g, (int)DATEPICKERPARTS.DP_DATEBORDER, state, ClientRectangle);
                    else // Windows XP or Wine: there is no DatePicker theme, but as the border is in the NC area, we can simply fill the background with back color
                    {
                        g.Clear(BackColor);
                        return;
                    }
                }

                // Clearing the background only in disabled state, or when a custom back color is specified; otherwise, preserving the theme back color
                // When there is partial paint, we paint the possibly specified custom back color. On Mono only, because on Windows the drop-down size change threshold is different.
                if (state == (int)DATEPICKERSTATES.DPS_DISABLED || (ShowCheckBox && !Checked) || !enabledBackColor.IsEmpty || !fullPaint)
                {
                    // Non-full paint here means that we paint only the background of the drop-down button area. Needed for Framework Mono to fix the messed-up calendar and up/down bounds.
                    // NOTE: This paints the custom back color for the button area. Doing this on Mono only, because on Windows the width of the drop-down button cannot be predicted.
                    Rectangle bounds = fullPaint ? layout.BackgroundBounds
                        : IsCustomUpDownBounds ? layout.UpDownBounds
                        : Rectangle.Intersect(layout.BackgroundBounds, layout.DropDownBounds);

                    if (g.VisibleClipBounds.X < 0)
                        bounds.Offset(layout.HorizontalOffset, 0);
                    g.FillRectangle(BackColor.GetBrush(), bounds);
                }
                return;
            }

            if (fullPaint)
            {
                // On Framework Mono not clearing the drop-down button area
                if (OSHelper.IsFrameworkMono)
                    g.FillRectangle(BackColor.GetBrush(), layout.BackgroundBounds);
                else
                    g.Clear(BackColor);
            }

            // If the application was initialized with visual styles (even if they are not enabled), the borders are in the client area, so we need to draw them.
            // On Framework Mono the border is always in the client area, and we always draw it.
            // Otherwise, the border belongs to the NC area, including the case when executing on Wine.
            // Not applying layout.HorizontalOffset here, because ControlPaint seems to be unaffected by the possible offset in RTL mode
            if (fullPaint && VisualStyleHelper.InitializedWithVisualStyles && OSHelper.IsWindowsVistaOrLater || OSHelper.IsFrameworkMono)
                ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Sunken);
        }

        private void PaintCheckBox(Graphics g, LayoutData layout, bool paintBackground)
        {
            Debug.Assert(ShowCheckBox && !OSHelper.IsWine);
            
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                var checkState = Checked
                    ? !Enabled ? CheckBoxState.CheckedDisabled
                        : flags[isHovered] ? CheckBoxState.CheckedHot
                        : CheckBoxState.CheckedNormal
                    : !Enabled ? CheckBoxState.UncheckedDisabled
                        : flags[isHovered] ? CheckBoxState.UncheckedHot
                        : CheckBoxState.UncheckedNormal;

                // When the control is not fully custom painted, we already have the system painted checkbox, potentially with different size and quality.
                // Using the Window system color is alright, because we do the clearing only when the control is focused, in which case no custom back color is used.
                if (paintBackground)
                    g.FillRectangle(OSHelper.IsWindowsVistaOrLater ? SystemBrushes.Window : BackColor.GetBrush(), layout.TranslatedCheckBoxBounds);

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

                return;
            }
            
            var buttonState = ButtonState.Normal;
            if (!Enabled)
                buttonState |= ButtonState.Inactive;
            if (Checked)
                buttonState |= ButtonState.Checked;

            if (!layout.IsRightToLeft || OSHelper.IsWindowsVistaOrLater || VisualStyleHelper.RenderWithVisualStyles || OSHelper.IsFrameworkMono)
            {
                ControlPaint.DrawCheckBox(g, layout.TranslatedCheckBoxBounds, buttonState);
                return;
            }

            // Windows XP with no visual styles in RTL mode: the checkbox is drawn mirrored by ControlPaint.
            // NOTE: This is the case also with Wine, though we cannot paint a fixed checkbox there, because it's a real embedded native control that we can't paint over.
            using var bmpCheckBox = new Bitmap(layout.CheckBoxBounds.Width, layout.CheckBoxBounds.Height, PixelFormat.Format32bppPArgb);
            using (Graphics gBitmap = Graphics.FromImage(bmpCheckBox))
                ControlPaint.DrawCheckBox(gBitmap, 0, 0, layout.CheckBoxBounds.Width, layout.CheckBoxBounds.Height, buttonState);
            bmpCheckBox.RotateFlip(RotateFlipType.RotateNoneFlipX);
            g.DrawImage(bmpCheckBox, layout.CheckBoxBounds);
        }

        private void PaintDropDownButton(Graphics g, LayoutData layout)
        {
            Debug.Assert(!ShowUpDown);

            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                int state = (int)(!Enabled ? DATEPICKERSTATES.DPS_DISABLED
                    : flags.Any(isDroppedDown | isPressed) ? DATEPICKERSTATES.DPS_FOCUSED
                    : flags[isDropDownHovered] ? DATEPICKERSTATES.DPS_HOT
                    : DATEPICKERSTATES.DPS_NORMAL);

                IntPtr theme = layout.IsCalendarDropDown ? VisualStyleHelper.DatePickerTheme : VisualStyleHelper.ComboBoxTheme;
                int part = layout.IsCalendarDropDown ? (int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT
                    : !OSHelper.IsWindowsVistaOrLater || OSHelper.IsWine ? (int)COMBOBOXPARTS.CP_DROPDOWNBUTTON
                    : layout.IsRightToLeft ? (int)COMBOBOXPARTS.CP_DROPDOWNBUTTONLEFT : (int)COMBOBOXPARTS.CP_DROPDOWNBUTTONRIGHT;
                VisualStyleHelper.Render(theme, this, g, part, state, layout.DropDownBounds);
                return;
            }

            // Framework mono with no visual styles: not drawing over the dropdown button, because ControlPaint draws with transparent background
            if (OSHelper.IsFrameworkMono)
                return;

            Rectangle bounds = OSHelper.IsWindowsVistaOrLater ? layout.TranslatedDropDownBounds : layout.DropDownBounds;
            ControlPaint.DrawComboButton(g, bounds, !Enabled ? ButtonState.Inactive : flags[isPressed] ? ButtonState.Pushed : ButtonState.Normal);
        }

        private void PaintUpDownButton(Graphics g, LayoutData layout)
        {
            Debug.Assert(ShowUpDown && OSHelper.IsFrameworkMono);
            if (!VisualStyleHelper.RenderWithVisualStyles)
                return;

            Rectangle boundsUp = layout.UpDownBounds;
            boundsUp.Height /= 2;
            Rectangle boundsDown = boundsUp;
            boundsDown.Y += boundsDown.Height;
            int stateUp = (int)(!Enabled ? SPINSTATES.SPNS_DISABLED
                : flags[isUpHovered] ? flags[isPressed] ? SPINSTATES.SPNS_PRESSED : SPINSTATES.SPNS_HOT
                : SPINSTATES.SPNS_NORMAL);
            int stateDown = (int)(!Enabled ? SPINSTATES.SPNS_DISABLED
                : flags[isDownHovered] ? flags[isPressed] ? SPINSTATES.SPNS_PRESSED : SPINSTATES.SPNS_HOT
                : SPINSTATES.SPNS_NORMAL);
            VisualStyleHelper.Render(VisualStyleHelper.SpinTheme, this, g, (int)SPINPARTS.SPNP_UP, stateUp, boundsUp);
            VisualStyleHelper.Render(VisualStyleHelper.SpinTheme, this, g, (int)SPINPARTS.SPNP_DOWN, stateDown, boundsDown);
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
            // No optimization with reference equality for the AdvancedDateTimePicker, because otherwise it can happen that the displayed
            // font gets corrupted. Occurs typically in .NET 6/7/8 if Form.StartPosition is WindowsDefaultLocation and the form is opened
            // on a different screen with a different DPI than the owner form's screen.
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

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => Invalidate();

        #endregion

        #endregion
    }
}
