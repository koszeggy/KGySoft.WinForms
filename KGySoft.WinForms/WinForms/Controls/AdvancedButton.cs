#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedButton.cs
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a button with additional features such as disabled colors, elevated mode, buffered animations and more.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedButton"/> class offers the following features in addition to <see cref="Button"/>:
    /// <list type="bullet">
    /// <item>Images are displayed also when the <see cref="FlatStyle"/> property is <see cref="FlatStyle.System"/>.</item>
    /// <item>Elevated mode (see <see cref="IsElevated"/> property). The shield icon is rendered also on a pre-Vista Windows.</item>
    /// <item>Different rendering qualities (see <see cref="TextRenderingQuality"/>) property.</item>
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item>Fading animations (only on Vista and above with visual styles enabled, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</item>
    /// <item>Slightly different appearance in some cases (e.g. focus rectangle size and width, image shifts along with text in classic or popup appearance,
    /// fixed highlight fore color in high contrast mode with visual styles enabled, etc.).</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(Button))]
    [Description(@"A button that provides the following features in addition to regular Button:
- Allows using images even if FlatStyle is System
- IsElevated property (shield icon)
- Different rendering qualities
- Adjustable colors in disabled state
- Fading animations
- Fixed appearance in several cases
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedButton : Button, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledForeColor = SystemColors.ControlText;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly string nbsp = '\u00A0'.ToString(null);

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;
        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        private bool isElevated;
        private bool isImageUpToDate = true;
        private bool isAlternativeDefaultImage;
        private bool hasPaintError;
        private bool isLoaded;
        private Image? currentImage; // the actual displayed image, including the shield icon when base.Image is null
        private FlatStyle lastFlatStyle = FlatStyle.Standard; // the explicitly set or the detected flat style changed in base
        private FlatStyle reportedFlatStyle = FlatStyle.Standard; // the flat style that is reported by the control (can be different when base does not support System)
        private FlatStyle lastAdapterType;
        private RenderingQuality textRenderingQuality;

        // NOTE: Unlike in AdvancedTextBox and AdvancedComboBox, we never set the base colors, because we handle all non-System drawings in the reimplemented adapters.
        // We only need to invoke OnBackColorChanged and OnForeColorChanged when the overriding colors are changed.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private ButtonBaseAdapter? adapter;
        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private Timer? defaultAnimationTimer;
        private Bitmap? cachedSecurityShieldImage; // an instance from IconsCache, should not be disposed
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;
        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private int dpiChangingCount;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedButton")]
        public event EventHandler<PaintStateEventArgs>? PaintState
        {
            add => Events.AddHandler(nameof(PaintState), value);
            remove => Events.RemoveHandler(nameof(PaintState), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether an elevated shield icon should be displayed.
        /// </summary>
        [Category("AdvancedButton")]
        [Description("Gets or sets whether an elevated shield icon should be displayed.")]
        [DefaultValue(false)]
        public bool IsElevated
        {
            get => isElevated;
            set
            {
                if (isElevated == value)
                    return;

                isElevated = value;
                if (!isElevated && ReferenceEquals(currentImage, cachedSecurityShieldImage))
                    base.Image = null;

                isImageUpToDate = false;
                CheckImage();

                Invalidate();
                if (AutoSize)
                    PerformLayout();
            }
        }

        /// <returns>
        /// The text associated with this control.
        /// </returns>
        [AllowNull]
        public override string Text
        {
            get
            {
                string result = base.Text;
                return result == nbsp ? String.Empty : base.Text;
            }
            set
            {
                // this fixes the issue that in System mode there can be no image without text
                if (String.IsNullOrEmpty(value))
                    value = nbsp;

                ResetSizeCache();
                base.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode by which the <see cref="AdvancedButton"/> automatically resizes itself.
        /// </summary>
        [DefaultValue(AutoSizeMode.GrowOnly)]
        public new AutoSizeMode AutoSizeMode
        {
            get => base.AutoSizeMode;
            set
            {
                ResetSizeCache();
                base.AutoSizeMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of text and image relative to each other.
        /// </summary>
        [DefaultValue(TextImageRelation.ImageBeforeText)]
        public new TextImageRelation TextImageRelation
        {
            get => base.TextImageRelation;
            set
            {
                ResetSizeCache();
                base.TextImageRelation = value;
            }
        }

        /// <summary>
        /// Gets or sets the flat style appearance of the button control.
        /// </summary>
        [DefaultValue(FlatStyle.Standard)]
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get => reportedFlatStyle;
            set
            {
                if (reportedFlatStyle == value && base.FlatStyle == value && lastFlatStyle == value)
                    return;

                base.FlatStyle = lastFlatStyle = reportedFlatStyle = value;
                OnFlatStyleChanged(true);
            }
        }

        /// <summary>
        /// Gets or sets the image that is displayed on the button control.
        /// </summary>
        public new Image? Image // it is also detected when base.Image changes but reacting onto that in OnPaint has a performance cost
        {
            get
            {
                CheckImage();
                return base.Image;
            }
            set
            {
                base.Image = value;
                isImageUpToDate = false;
                CheckImage();
            }
        }

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedButton"/>.
        /// </summary>
        [Category("AdvancedButton")]
        [Description("Gets or sets the text rendering quality of the button control. Has effect only when FlatStyle is not System.")]
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
        /// <note>Please note that this property affects the font only. The elevated icon (see the <see cref="IsElevated"/> property) is always scaled with V2 awareness,
        /// whereas scaling the size and location always depends on the executing platform behavior.</note>
        /// </remarks>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
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
                    UseVisualStyleBackColor = false;
                if (Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
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
                    UseVisualStyleBackColor = false;
                if (!Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
        [DefaultValue(true)]
        [Description("Gets or sets whether fading animations are enabled for the control. Animations work on Windows Vista and above, with non-classic themes.")]
        public bool FadingAnimationsEnabled
        {
            get => fadingAnimationsEnabled;
            set
            {
                if (fadingAnimationsEnabled == value)
                    return;

                fadingAnimationsEnabled = value;
                CheckStyles();
            }
        }

        /// <summary>
        /// Gets or sets fading options of the control.
        /// </summary>
        [Category("AdvancedButton")]
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

                Invalidate(); // delete if ResetOptions is uncommented
            }
        }

        /// <summary>
        /// Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.
        /// </summary>
        [Category("AdvancedButton")]
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

        /// <inheritdoc />
        public override Size MinimumSize
        {
            get => base.MinimumSize;
            set
            {
                if (base.MinimumSize == value)
                    return;
                ResetSizeCache();
                base.MinimumSize = value;
            }
        }

        /// <inheritdoc />
        public override Size MaximumSize
        {
            get => base.MaximumSize;
            set
            {
                if (base.MaximumSize == value)
                    return;
                ResetSizeCache();
                base.MaximumSize = value;
            }
        }

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override Size DefaultSize => new(100, base.DefaultSize.Height);

        #endregion

        #region Private Properties

        private Image SecurityShieldImage
        {
            get
            {
                // unlike in CommandLinkButton, the elevated icon may be set in base.Image, so it is publicly accessible (and disposable)
                if (cachedSecurityShieldImage == null || cachedSecurityShieldImage.IsDisposed())
                {
                    Size size = this.ScaleSize(IconsHelper.SmallIconReferenceSize);
                    using var icon = Icons.SystemShield;
                    cachedSecurityShieldImage = icon.GetCachedBitmap(nameof(Icons.SystemShield), size);
                }

                return cachedSecurityShieldImage;
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
                        FlatStyle.Flat => new ButtonFlatAdapter(this),
                        FlatStyle.Popup => new ButtonPopupAdapter(this),
                        FlatStyle.Standard => new ButtonStandardAdapter(this),
                        FlatStyle.System when OSHelper.IsMono => new ButtonStandardAdapter(this),
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
        /// Creates a new instance of <see cref="AdvancedButton"/>.
        /// </summary>
        public AdvancedButton()
        {
            base.TextImageRelation = TextImageRelation.ImageBeforeText;
            CheckStyles();
            fadingPainter = new FadingPainterInternal(this, Constants.ThemeClassButton);
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <inheritdoc />
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            // System mode
            if (base.FlatStyle == FlatStyle.System)
            {
                if (base.Image == null && !isElevated)
                    preferredSize = base.GetPreferredSize(proposedSize);
                else
                {
                    // in system mode we must calculate with the image so hacking base.systemSize field
                    Size systemSize = this.GetSystemSize();
                    if (systemSize.Width == Int32.MinValue)
                    {
                        systemSize = SizeFromClientSize(TextRenderer.MeasureText(base.Text, base.Font));
                        systemSize.Width += 14;
                        systemSize.Height += 9;
                        Size imageSize = base.Image != null ? base.Image.Size : SecurityShieldImage.Size;
                        if (imageSize.Height + 7 > systemSize.Height)
                            systemSize.Height = imageSize.Height + 7;
                        this.SetSystemSize(systemSize);
                    }

                    // now base.GetPreferredSize will return correct result
                    preferredSize = base.GetPreferredSize(proposedSize);
                }

                preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
                return preferredSize;
            }

            // Non-System mode: we must calculate with the current rendering quality so reimplementing base logic
            Size proposedConstraints = proposedSize;
            if (proposedConstraints.Width == 1)
                proposedConstraints.Width = 0;
            if (proposedConstraints.Height == 1)
                proposedConstraints.Height = 0;

            using (Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero))
            {
                CheckImage();
                g.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);
                preferredSize = LayoutUtils.UnionSizes(((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedConstraints, GetAppearance()) + Padding.Size, MinimumSize);
            }

            if (AutoSize && AutoSizeMode == AutoSizeMode.GrowOnly)
                preferredSize = LayoutUtils.UnionSizes(preferredSize, Size);

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
            CheckImage();
        }

        /// <inheritdoc />
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            isLoaded = true;
        }

        /// <inheritdoc />
        protected override void OnHandleDestroyed(EventArgs e)
        {
            isLoaded = false;
            base.OnHandleDestroyed(e);
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
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT when base.FlatStyle == FlatStyle.System:
                    // Image and FlatStyle are not overridable properties so in case of native rendering reacting their change here.
                    // (On custom rendering, image change is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged(false);
                    }

                    CheckDpiChange();
                    if (CheckImage() && AutoSize)
                        PerformLayout();

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

                    // This autoscales font when needed
                    CheckDpiChange();
                    return;

                // Known issue: Security shield icon size is not updated with non-V2 awareness (System FlatStyle)
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

                    // System FlatStyle: the WM_DPICHANGED_AFTERPARENT resets the elevated icon, but we want to prevent that if an image is set.
                    // Doing it even if IsElevated is false, because if it was true before, then the shield icon is still displayed.
                    if (base.FlatStyle == FlatStyle.System)
                    {
                        if (base.Image != null)
                        {
                            isImageUpToDate = false;
                            Invalidate();
                        }
#if NETFRAMEWORK
                        // .NET Framework: The Elevated icon size is not updated, so we need to recreate the handle
                        // Would not be needed for .NET Framework 4.7+ when app.config awareness is also set to V2.
                        else if (isElevated && Created)
                        {
                            RecreateHandle();
                            return;
                        }
#endif
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
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting FlatStyle if needed (in System mode this is in WndProc)
            bool invalidated = false;
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                OnFlatStyleChanged(false);
                invalidated = true;
            }

            CheckDpiChange();
            if (CheckImage() && AutoSize)
            {
                PerformLayout();
                invalidated = true;
            }

            CheckDefaultAnimation();

            // in this case new paint will be triggered
            if (invalidated)
                return;

            try
            {
                fadingPainter.State ??= GetAppearance();
                fadingPainter.Paint(e);
                hasPaintError = false;
            }
            catch (Exception ex) when (!ex.IsCritical())
            {
                // We tolerate one exception if we can recover from it in the next paint. May occur on Windows 7 when the theme is changed.
                // But if exceptions are thrown in two consecutive paints, we let the second one propagate.
                if (hasPaintError)
                    throw;
                hasPaintError = true;
                ResetScale();
                CheckDpiChange();
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
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
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;

            ResetSizeCache();
            base.OnFontChanged(e);
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
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            isMouseDown = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            Invalidate();
            base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if (isMouseDown)
                isPressed = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

            base.OnMouseMove(mevent);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && !isPressed)
            {
                isPressed = true;
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;
            }

            base.OnKeyUp(e);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fade if enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            CheckDefaultAnimation();
            base.OnVisibleChanged(e);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnSizeChanged(e);
        }

        /// <inheritdoc />
        protected override void OnPaddingChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnPaddingChanged(e);
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
                this.SetShowToolTip(Height < preferredHeight);
            }
            else
                this.SetShowToolTip(false);

            if (GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                ((ISupportButtonAdapter)this).Adapter.Paint(e);
            }

            // Raising PaintState
            Events.GetHandler<EventHandler<PaintStateEventArgs>>(nameof(PaintState))?.Invoke(this, e);

            // Control.OnPaint:
            if (Accessors.PaintEvent is not object paintEventKey)
                return;
            PaintEventHandler? handler = (PaintEventHandler?)Events[paintEventKey];
            handler?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            if (disposing)
            {
                fadingPainter.Dispose();
                defaultAnimationTimer?.Dispose();
                defaultAnimationTimer = null;
                cachedSecurityShieldImage = null;
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

        private ControlAppearanceState GetAppearance()
        {
            int partId = (int)BUTTONPARTS.BP_PUSHBUTTON;
            int stateId = (int)GetSystemState();
            bool isEnabled = Enabled;
            Color foreColor = ForeColor;
            if (lastFlatStyle == FlatStyle.Standard && VisualStyleHelper.RenderWithVisualStyles
                && (isEnabled && foreColor == defaultEnabledForeColor || !isEnabled && foreColor == defaultDisabledForeColor))
            {
                foreColor = VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, partId, stateId, foreColor);
            }

            //VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedDisabledColor
            return new ControlAppearanceState(partId, stateId)
            {
                BackColor = BackColor,
                ForeColor = foreColor,
                Enabled = Enabled,
                Hovered = isHovered,
                Pressed = isPressed,
                IsDefault = IsDefault,
                Focused = Focused,
                Text = base.Text,
                Visible = Visible,
            };
        }

        private PUSHBUTTONSTATES GetSystemState()
        {
            if (!Enabled)
                return PUSHBUTTONSTATES.PBS_DISABLED;

            if (isPressed)
                return PUSHBUTTONSTATES.PBS_PRESSED;

            if (isHovered)
                return PUSHBUTTONSTATES.PBS_HOT;

            if (IsDefault)
                return fadingAnimationsEnabled && (fadingOptions & FadingOptions.StandardEffects) != FadingOptions.None && isAlternativeDefaultImage
                ? PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING
                : PUSHBUTTONSTATES.PBS_DEFAULTED;

            return PUSHBUTTONSTATES.PBS_NORMAL;
        }

        private void CheckStyles()
        {
            if (fadingAnimationsEnabled && FadingPainterInternal.IsSupported)
            {
                // to enable animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (base.FlatStyle != FlatStyle.System)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private void CheckDefaultAnimation()
        {
            if (!OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.RenderWithVisualStyles
                || !VisualStyleHelper.HasDefaultAnimation((int)BUTTONPARTS.BP_PUSHBUTTON, (int)PUSHBUTTONSTATES.PBS_DEFAULTED, (int)PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING))
            {
                return;
            }

            bool enabled = base.FlatStyle == FlatStyle.Standard && !isPressed && !isHovered && IsDefault && VisualStyleHelper.RenderWithVisualStyles && !VisualStyleHelper.HighContrast;
            if (enabled && (defaultAnimationTimer == null || !defaultAnimationTimer.Enabled))
            {
                if (defaultAnimationTimer == null)
                {
                    defaultAnimationTimer = new Timer();
                    defaultAnimationTimer.Interval = UxTheme.TryGetThemeTransitionDuration(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_PUSHBUTTON,
                        (int)PUSHBUTTONSTATES.PBS_DEFAULTED,
                        (int)PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING,
                        Constants.TMT_TRANSITIONDURATIONS, out int duration) && duration != 0
                        ? duration
                        : 1000;
                    defaultAnimationTimer.Tick += defaultAnimationTimer_Tick;
                }

                isAlternativeDefaultImage = false;
                defaultAnimationTimer.Enabled = true;
            }
            else if (!enabled && defaultAnimationTimer != null && defaultAnimationTimer.Enabled)
            {
                defaultAnimationTimer.Enabled = false;
                isAlternativeDefaultImage = false;
            }
        }
        
        /// <summary>
        /// Checks image consistency. Returns true if image update has been performed.
        /// </summary>
        private bool CheckImage()
        {
            // While isLoaded is true, it is dangerous to change the FlatStyle, because it may cause an exception when the control is created.
            if ((!IsHandleCreated || !isLoaded) && base.FlatStyle == FlatStyle.System)
                return true;

            // if image is up-to-date checking consistency only (to handle setting base.Image)
            if (isImageUpToDate)
            {
                if (!isElevated && currentImage == base.Image
                    || currentImage == null && base.Image == null
                    || isElevated && (base.FlatStyle == FlatStyle.System ^ base.Image != null) && ReferenceEquals(currentImage, SecurityShieldImage))
                    return false;
            }

            // Resetting System FlatStyle if it was faked and there is no image anymore
            if (reportedFlatStyle == FlatStyle.System && base.FlatStyle != reportedFlatStyle && base.Image == null && !isElevated)
                base.FlatStyle = lastFlatStyle = FlatStyle.System;

            // Image > Elevated > no image
            if (base.FlatStyle == FlatStyle.System && OSHelper.IsWindowsVistaOrLater && !OSHelper.IsMono)
                this.SetSystemSize(new Size(Int32.MinValue, Int32.MinValue));

            Invalidate();
            ResetSizeCache();
            isImageUpToDate = true;
            if (base.Image != null)
            {
                currentImage = base.Image;
                if (base.FlatStyle == FlatStyle.System)
                {
                    if (!OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.InitializedWithVisualStyles)
                    {
                        base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
                        return true;
                    }

                    Bitmap bmp = base.Image as Bitmap ?? new Bitmap(base.Image); // TODO
                    User32.SendMessage(Handle, Constants.BM_SETIMAGE, new IntPtr(1), bmp.GetHicon());
                }

                return true;
            }

            currentImage = null;

            if (isElevated)
            {
                currentImage = SecurityShieldImage;

                if (base.FlatStyle != FlatStyle.System || !OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.InitializedWithVisualStyles)
                {
                    base.Image = currentImage;

                    if (!OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.InitializedWithVisualStyles)
                        base.FlatStyle = lastFlatStyle = FlatStyle.Standard;

                    return true;
                }

                if (IsHandleCreated)
                    User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
            }
            else if (base.FlatStyle == FlatStyle.System && OSHelper.IsWindowsVistaOrLater && IsHandleCreated)
            {
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, IntPtr.Zero);
            }

            return true;
        }

        private void OnFlatStyleChanged(bool checkImage)
        {
            CheckDefaultAnimation();

            // Images are supported only in Vista and above in System mode when Application.EnableVisualStyles was called
            if (base.FlatStyle == FlatStyle.System && (base.Image != null || isElevated) && (!OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.InitializedWithVisualStyles))
            {
                // note: this will not change the reported FlatStyle in designer
                base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
                ImageAlign = ContentAlignment.MiddleRight;
            }

            isImageUpToDate = false;
            if (checkImage)
                CheckImage();

            if (base.FlatStyle == FlatStyle.System)
            {
                if (AutoScaleFont)
                    SetFont(font ?? defaultFont);
                if (isElevated && base.Image.EqualsByContent(SecurityShieldImage))
                    base.Image = null;
            }

            CheckStyles();
            ResetSizeCache();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void ResetSizeCache() => preferredSizeCache.Clear();

        private bool ShouldSerializeFont() => font != null;
        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        private bool ShouldSerializeImage()
        {
            if (currentImage == null)
                return false;
            return !ReferenceEquals(currentImage, cachedSecurityShieldImage);
        }

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();

            // The Font check is needed for .NET 6, where WinForms' (bad) auto font scaling may occur without notification
            if ((scale == lastScale && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            if (!lastScale.IsEmpty)
                ResetScale();
            lastScale = scale;

            if (!AutoScaleFont)
                return;

            if (font is ScalingFont explicitFont)
                explicitFont.Scale(scale);
            else
                defaultFont!.Scale(scale);
            SetFont(font ?? defaultFont);
        }

        private void ResetScale()
        {
            if (isElevated && ReferenceEquals(currentImage, cachedSecurityShieldImage))
            {
                base.Image = null;
                isImageUpToDate = false;
                Invalidate();
            }

            cachedSecurityShieldImage = null;
            lastScale = PointF.Empty;
            ResetSizeCache();
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
                    if (!oldFont.IsDisposed() && !(isPerMonitorDpiAwarenessV1 && base.FlatStyle == FlatStyle.System && OSHelper.IsWindows && !OSHelper.IsMono))
#else
                    if (!oldFont.IsDisposed())
#endif
                    {
                        return;
                    }
                }

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

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
            // system speeds are determined by the painter
            => FadingAnimationDefaultSpeed;

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
            => OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));

        int ISupportsFadingInternal.GetStandardAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo, int defaultSpeed)
            => FlatStyle switch
            {
                // disabling animation when the popup border or text offset changes
                FlatStyle.Popup => stateFrom.Hovered != stateTo.Hovered || stateFrom.Pressed != stateTo.Pressed ? 0 : defaultSpeed,
                _ => defaultSpeed
            };

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
            if (isPerMonitorDpiAwarenessV1 && AutoSize)
                PerformLayout();
        }

        #endregion

        #region Event Handlers
#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable InconsistentNaming

        private void defaultAnimationTimer_Tick(object? sender, EventArgs e)
        {
            isAlternativeDefaultImage = !isAlternativeDefaultImage;
            Invalidate();
        }

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => CheckStyles();

        // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion
    }
}
