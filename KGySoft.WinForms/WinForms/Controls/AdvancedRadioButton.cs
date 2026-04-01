#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedRadioButton.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a radio button with additional features such as disabled colors, fixed auto size, buffered animations and more.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedRadioButton"/> class offers the following features in addition to <see cref="RadioButton"/>:
    /// <list type="bullet">
    /// <item><see cref="ButtonBase.AutoSize"/> property works as expected when radio button is docked</item>
    /// <item>Different rendering qualities (see <see cref="TextRenderingQuality"/> and <see cref="VisualsRenderingQuality"/>) properties.</item>
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(RadioButton))]
    [Description(@"A radio button that provides the following features in addition to regular RadioButton:
- AutoSize works as expected when radio button is docked
- Adjustable rendering qualities
- Adjustable colors in disabled state
- Fading animations
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedRadioButton : RadioButton, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal, IPerMonitorDpiAware
    {
        #region Constants

        // We could use BitVector32.CreateMask, but then we should use static fields, whose access is slower than using constants.
        private const int autoScaleFont = 1;
        private const int suppressFontChanged = autoScaleFont << 1;
        private const int isPerMonitorDpiAwarenessV1 = suppressFontChanged << 1;
        private const int fadingAnimationsEnabled = isPerMonitorDpiAwarenessV1 << 1;
        private const int ignoreNextPaint = fadingAnimationsEnabled << 1;
        private const int hasPaintError = ignoreNextPaint << 1;
        private const int isHovered = hasPaintError << 1;
        private const int isMouseDown = isHovered << 1;
        private const int isPressed = isMouseDown << 1;
        private const int entered = isPressed << 1;
        private const int left = entered << 1;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledForeColor = SystemColors.ControlText;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;

        private BitVector32 flags;
        private RenderingQuality textRenderingQuality;
        private RenderingQuality visualsRenderingQuality = RenderingQuality.High;
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle lastAdapterType;

        // NOTE: Unlike in AdvancedTextBox and AdvancedComboBox, we never set the base colors, because we handle all non-System drawings in the reimplemented adapters.
        // We only need to invoke OnBackColorChanged and OnForeColorChanged when the overriding colors are changed.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private ButtonBaseAdapter? adapter;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private int dpiChangingCount;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedRadioButton")]
        public event EventHandler<PaintStateEventArgs>? PaintState
        {
            add => Events.AddHandler(nameof(PaintState), value);
            remove => Events.RemoveHandler(nameof(PaintState), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedRadioButton"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets the text rendering quality of the advanced radio button. Has effect only when FlatStyle is not System.")]
        [DefaultValue(RenderingQuality.SystemDefault)]
        public RenderingQuality TextRenderingQuality
        {
            get => textRenderingQuality;
            set
            {
                if (textRenderingQuality == value)
                    return;

                if (!Enum<RenderingQuality>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                textRenderingQuality = value;
                Invalidate();
                if (AutoSize)
                {
                    ResetSizeCache();
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the rendering quality of the <see cref="AdvancedRadioButton"/> visuals.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets the rendering quality of the advanced radio button visuals. Has effect only in high DPI mode.")]
        [DefaultValue(RenderingQuality.High)]
        public RenderingQuality VisualsRenderingQuality
        {
            get => visualsRenderingQuality;
            set
            {
                if (visualsRenderingQuality == value)
                    return;

                if (!Enum<RenderingQuality>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                visualsRenderingQuality = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> state.
        /// </summary>
        [Description("The background color in the current Enabled state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => Enabled ? EnabledBackColor : DisabledBackColor;
            set
            {
                if (Enabled)
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current <see cref="Control.Enabled"/> state.
        /// </summary>
        [Description("The text color in the current Enabled state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => Enabled ? EnabledForeColor : DisabledForeColor;
            set
            {
                if (Enabled)
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Determines the background color when the control is Enabled.")]
        public Color EnabledBackColor
        {
            get => !enabledBackColor.IsEmpty ? enabledBackColor : base.BackColor;
            set
            {
                if (enabledBackColor == value)
                    return;
                enabledBackColor = value;
                if (!enabledBackColor.IsEmpty)
                    UseVisualStyleBackColor = false; // because Appearance can be Button
                if (Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor : base.ForeColor;
            set
            {
                if (enabledForeColor == value)
                    return;
                enabledForeColor = value;
                if (Enabled)
                    OnForeColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Determines the disabled background color.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : base.BackColor;
            set
            {
                if (disabledBackColor == value)
                    return;
                disabledBackColor = value;
                if (!disabledBackColor.IsEmpty)
                    UseVisualStyleBackColor = false; // because Appearance can be Button
                if (!Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Determines the disabled text color.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
                if (!Enabled)
                    OnForeColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work on Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(true)]
        [Description("Gets or sets whether fading animations are enabled for the control. Animations work on Windows Vista and above, with non-classic themes.")]
        public bool FadingAnimationsEnabled
        {
            get => flags[fadingAnimationsEnabled];
            set
            {
                if (flags[fadingAnimationsEnabled] == value)
                    return;

                flags[fadingAnimationsEnabled] = value;
                CheckStyles();
            }
        }

        /// <summary>
        /// Gets or sets fading options of the control.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(FadingOptions.StandardEffects)]
        [Description("Gets or sets fading options of the control.")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FadingOptions FadingAnimationOptions
        {
            get => fadingOptions;
            set
            {
                if (fadingOptions == value)
                    return;

                if (!Enum<FadingOptions>.AllFlagsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                fadingOptions = value;

                // storing invisible state so when control turns visible it will fade if enabled
                if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                    fadingPainter.State = GetAppearance();

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(500)]
        [Description("Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.")]
        public int FadingAnimationDefaultSpeed
        {
            get => fadingAnimationDefaultSpeed;
            set
            {
                if (fadingAnimationDefaultSpeed == value)
                    return;

                if (fadingAnimationDefaultSpeed < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                fadingAnimationDefaultSpeed = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
        [DefaultValue(false)]
        public new bool UseCompatibleTextRendering
        {
            get => base.UseCompatibleTextRendering;
            set
            {
                ResetSizeCache();
                base.UseCompatibleTextRendering = value;
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
        [Category("AdvancedRadioButton")]
        [DefaultValue(true)]
        [Description("True to auto scale Font when DPI changes and inherit the font when it's not explicitly set; False to rely on the default behavior of the current executing platform.")]
        public bool AutoScaleFont
        {
            get => flags[autoScaleFont];
            set
            {
                if (flags[autoScaleFont] == value)
                    return;

                flags[autoScaleFont] = value;
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
                if (dpiChangingCount > 0 && AutoScaleFont)
                    return;

                if (!ReferenceEquals(base.Font, value))
                    ResetSizeCache();

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
        /// Gets or sets the flat style appearance of the radio button control.
        /// </summary>
        [DefaultValue(FlatStyle.Standard)]
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get => base.FlatStyle;
            set
            {
                if (base.FlatStyle == value && lastFlatStyle == value)
                    return;

                base.FlatStyle = value;
                lastFlatStyle = value;
                OnFlatStyleChanged();
            }
        }

        #endregion

        #region Explicitly Implemented Interface Properties

        ButtonBaseAdapter ISupportButtonAdapter.Adapter
        {
            get
            {
                if ((adapter == null) || (base.FlatStyle != lastAdapterType))
                {
                    adapter = base.FlatStyle switch
                    {
                        FlatStyle.Flat => new RadioButtonFlatAdapter(this),
                        FlatStyle.Popup => new RadioButtonPopupAdapter(this),
                        FlatStyle.Standard => new RadioButtonStandardAdapter(this),
                        FlatStyle.System when OSHelper.IsFrameworkMono => new RadioButtonStandardAdapter(this),
                        _ => throw new InvalidOperationException()
                    };
                    lastAdapterType = base.FlatStyle;
                }
                return adapter;
            }
        }

        bool ISupportButtonAdapter.ShowFocusCues => ShowFocusCues;
        bool ISupportButtonAdapter.ShowKeyboardCues => ShowKeyboardCues;
        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedRadioButton"/>.
        /// </summary>
        public AdvancedRadioButton()
        {
            flags[autoScaleFont | fadingAnimationsEnabled] = true;
            fadingPainter = new FadingPainterInternal(this, Constants.ThemeClassButton);
            CheckStyles();
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
            flags[isPerMonitorDpiAwarenessV1] = ScaleHelper.PerMonitorDpiAwarenessVersion == 1;
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
            if (OSHelper.IsFrameworkMono)
                SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.
        /// </returns>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (FlatStyle == FlatStyle.System && !OSHelper.IsFrameworkMono)
                return base.GetPreferredSize(proposedSize);

            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            if (proposedSize.Width == 1)
                proposedSize.Width = 0;
            if (proposedSize.Height == 1)
                proposedSize.Height = 0;

            using (Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero))
            {
                g.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);
                preferredSize = ((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedSize, GetAppearance());
            }

            preferredSize = LayoutUtils.UnionSizes(preferredSize + Padding.Size, MinimumSize);
            preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
            return preferredSize;
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnTextChanged(e);
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (flags[suppressFontChanged])
                return;

            ResetSizeCache();
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (EnabledBackColor != DisabledBackColor)
                OnBackColorChanged(EventArgs.Empty);
            if (EnabledForeColor != DisabledForeColor)
                OnForeColorChanged(EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting FlatStyle if needed (in System mode this is in WndProc)
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = base.FlatStyle;
                OnFlatStyleChanged();
                return;
            }

            // when focus is changed with cursor multiple paints occur that may cause flickering
            // leave -> focused (ignored) -> not focused
            // entered -> not focused unchecked (ignored) -> not focused checked (ignored) -> focused
            if (flags.Any(left | entered))
            {
                bool focused = Focused;
                if (flags[left] && focused || flags[entered] && !focused)
                    flags[ignoreNextPaint] = true;

                flags[left] = false;
                if (focused) // clearing entered only when focused because 2 paints have to be ignored
                    flags[entered] = false;
            }

            if (flags[ignoreNextPaint])
            {
                flags[ignoreNextPaint] = false;
                Invalidate();
                return;
            }

            CheckDpiChange();

            try
            {
                fadingPainter.State ??= GetAppearance();
                fadingPainter.Paint(e);
                flags[hasPaintError] = false;
            }
            catch (Exception ex) when (!ex.IsCritical())
            {
                // We tolerate one exception if we can recover from it in the next paint.
                // But if exceptions are thrown in two consecutive paints, we let the second one propagate.
                if (flags[hasPaintError])
                    throw;
                flags[hasPaintError] = true;
                lastScale = PointF.Empty;
                CheckDpiChange();
                Invalidate();
            }
        }

        /// <summary>
        /// This method does nothing on this class. Use the <see cref="OnPaintState"/> method to customize painting.
        /// </summary>
        /// <param name="pevent">Not used</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override sealed void OnPaintBackground(PaintEventArgs pevent)
        {
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
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    // FlatStyle is not overridable property so in case of native rendering reacting for its change here.
                    // (On custom rendering, this is handled in OnPaint)
                    if (base.FlatStyle == FlatStyle.System && base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged();
                    }

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

                    if (AutoSize)
                        PerformLayout();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(EventArgs e)
        {
            flags[isHovered] = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            flags[isHovered] = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            flags[isPressed | isMouseDown] = false;
            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool prevPressed = flags[isPressed];
            flags[isPressed | isMouseDown] = e.Button == MouseButtons.Left;
            base.OnMouseDown(e);

            // workaround for base Invalidate(DownChangeRectangle), where DownChangeRectangle is not scaled properly
            if (flags[isPressed] != prevPressed)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            bool prevPressed = flags[isPressed];
            if (flags[isMouseDown])
                flags[isPressed] = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

            base.OnMouseMove(mevent);

            // workaround for base Invalidate(DownChangeRectangle), where DownChangeRectangle is not scaled properly
            if (flags[isPressed] != prevPressed)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool prevPressed = flags[isPressed];
            if (e.KeyData == Keys.Space && !prevPressed)
                flags[isPressed] = true;

            base.OnKeyDown(e);
            if (flags[isPressed] != prevPressed)
                Invalidate(); // workaround for base ResetFlagsandPaint call, which calls Invalidate(DownChangeRectangle), where DownChangeRectangle is not scaled properly
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && flags[isPressed])
                flags[isPressed] = false;

            base.OnKeyUp(e);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fade if enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            base.OnVisibleChanged(e);
        }

        /// <inheritdoc />
        protected override void OnPaddingChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnPaddingChanged(e);
        }

        /// <inheritdoc />
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (OSHelper.IsFrameworkMono)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (OSHelper.IsFrameworkMono)
            {
                Invalidate();
                CheckStyles();
            }
        }

        /// <summary>
        /// Paints the specified state of this control, and raises the <see cref="PaintState"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintStateEventArgs"/> that contains the event data.</param>
        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            e.Graphics.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);

            // ButtonBase.OnPaint:
            if (AutoEllipsis)
            {
                int preferredHeight = GetPreferredSize(new Size(Width, 0)).Height;
                this.ShowToolTip(Height < preferredHeight);
            }
            else
            {
                this.ShowToolTip(false);
            }

            if (GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                ((ISupportButtonAdapter)this).Adapter.Paint(e);
            }

            // Raising Paint
            if (Accessors.PaintEvent is object paintEventKey)
                Events.GetHandler<PaintEventHandler>(paintEventKey)?.Invoke(this, e);

            // Raising PaintState
            Events.GetHandler<EventHandler<PaintStateEventArgs>>(nameof(PaintState))?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void OnEnter(EventArgs e)
        {
            if (FadingAnimationsEnabled && FadingPainterInternal.IsSupported)
                flags[entered] = true;
            base.OnEnter(e);
        }

        /// <inheritdoc />
        protected override void OnLeave(EventArgs e)
        {
            if (FadingAnimationsEnabled && FadingPainterInternal.IsSupported)
                flags[left] = true;
            base.OnLeave(e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            if (disposing)
            {
                fadingPainter.Dispose();
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

        private void OnFlatStyleChanged()
        {
            ResetSizeCache();
            CheckStyles();
            Invalidate();
            if (AutoScaleFont && base.FlatStyle == FlatStyle.System)
                SetFont(font ?? defaultFont);
            if (AutoSize)
                PerformLayout();
        }

        private void CheckStyles()
        {
            if (FadingAnimationsEnabled && fadingPainter.Enabled)
            {
                // to enable animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (base.FlatStyle != FlatStyle.System || OSHelper.IsFrameworkMono)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private ControlAppearanceState GetAppearance()
        {
            // For non-standard FlatStyles, we use CheckBox part even for Button appearance so we will have nonzero transition speeds for CheckState changes.
            int partId = (int)(Appearance == Appearance.Normal || FlatStyle is FlatStyle.Popup or FlatStyle.Flat ? BUTTONPARTS.BP_RADIOBUTTON : BUTTONPARTS.BP_PUSHBUTTON);
            int stateId = GetSystemState();
            bool isEnabled = Enabled;
            Color foreColor = ForeColor;
            if (lastFlatStyle == FlatStyle.Standard && VisualStyleHelper.RenderWithVisualStyles
                && (isEnabled && foreColor == defaultEnabledForeColor || !isEnabled && foreColor == defaultDisabledForeColor))
            {
                foreColor = VisualStyleHelper.GetTextColor(Constants.ThemeClassButton, this.GetHandleIfCreated(), partId, stateId, foreColor);
            }

            return new ControlAppearanceState(partId, stateId)
            {
                BackColor = BackColor,
                ForeColor = foreColor,
                Enabled = Enabled,
                Hovered = flags[isHovered],
                Pressed = flags[isPressed],
                IsDefault = IsDefault,
                Focused = Focused,
                CheckState = Checked ? CheckState.Checked : CheckState.Unchecked,
                Text = base.Text,
                Visible = Visible,
            };
        }

        private int GetSystemState()
        {
            // For non-standard FlatStyles, we use RadioButton states even for Button appearance so we will have nonzero transition speeds for Checked changes.
            if (Appearance == Appearance.Normal || FlatStyle is FlatStyle.Popup or FlatStyle.Flat)
            {
                RadioButtonState result = RadioButtonState.UncheckedNormal;
                if (!Enabled)
                    result = RadioButtonState.UncheckedDisabled;
                else if (flags[isPressed])
                    result = RadioButtonState.UncheckedPressed;
                else if (flags[isHovered])
                    result = RadioButtonState.UncheckedHot;

                if (Checked)
                    result += (int)RadioButtonState.CheckedNormal - 1;

                return (int)result;
            }

            if (!Enabled)
                return (int)PUSHBUTTONSTATES.PBS_DISABLED;

            // NOTE: The base RadioButton renders checked state with Button appearance as pressed, which is not distinguishable from normal state
            // in high contrast mode when visual styles are enabled, so using the HOT state for a checked radio button instead.
            if (flags[isPressed])
                return (int)PUSHBUTTONSTATES.PBS_PRESSED;

            if (flags[isHovered] || Checked)
                return (int)PUSHBUTTONSTATES.PBS_HOT;

            if (Focused)
                return (int)PUSHBUTTONSTATES.PBS_DEFAULTED;

            return (int)PUSHBUTTONSTATES.PBS_NORMAL;
        }

        private void ResetSizeCache() => preferredSizeCache.Clear();

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

            // explicitly set fonts must be forcibly set in base.Font
            bool force = ReferenceEquals(font, value) || base.FlatStyle == FlatStyle.System;
            Font oldFont = base.Font;
            Font newFont = value.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            if (Equals(oldFont, newFont))
            {
                if (!force)
                {
                    if (ReferenceEquals(oldFont, newFont))
                        return;

                    // Non-reference equality: we are alright if the old font is not disposed...
                    // ...except in .NET Core 3.0 - .NET 5.0 when FlatStyle is System and using v1 per-monitor DPI awareness, in which case the font gets corrupted
#if NETCOREAPP && !NET6_0_OR_GREATER
                    if (!oldFont.IsDisposed() && !(flags[isPerMonitorDpiAwarenessV1] && base.FlatStyle == FlatStyle.System && OSHelper.IsWindows && !OSHelper.IsMono))
#else
                    if (!oldFont.IsDisposed())
#endif 
                    {
                        ResetSizeCache();
                        if (AutoSize)
                            PerformLayout();
                        return;
                    }
                }

                flags[suppressFontChanged] = true;
                try
                {
                    base.Font = null!;

                    // setting base.Font caused reentrancy: not letting the outer call to set the font again
                    if (!flags[suppressFontChanged])
                        return;
                }
                finally
                {
                    flags[suppressFontChanged] = false;
                }
            }

            base.Font = newFont;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
            // system speeds are determined by the painter
            => FadingAnimationDefaultSpeed;

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
            => OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));

        int ISupportsFadingInternal.GetStandardAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo, int defaultSpeed)
            => FlatStyle switch
            {
                // disabling animation when the popup border or text offset changes
                FlatStyle.Popup => stateFrom.Hovered != stateTo.Hovered || Appearance == Appearance.Button && (stateFrom.Pressed != stateTo.Pressed || stateFrom.CheckState != stateTo.CheckState) ? 0 : defaultSpeed,
                FlatStyle.Flat => Appearance == Appearance.Button && stateFrom.CheckState != stateTo.CheckState ? 0 : defaultSpeed,
                _ => defaultSpeed,
            };

        void IPerMonitorDpiAware.ParentFormDpiChanging()
        {
            dpiChangingCount += 1;
            if (flags[isPerMonitorDpiAwarenessV1])
                CheckDpiChange();
        }

        void IPerMonitorDpiAware.ParentFormDpiChanged()
        {
            dpiChangingCount -= 1;
            if (flags[isPerMonitorDpiAwarenessV1] && AutoSize)
                PerformLayout();
        }

        #endregion

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => CheckStyles();

        #endregion

        #endregion
    }
}