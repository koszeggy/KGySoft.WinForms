#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CommandLinkButton.cs
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
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    #region Usings

    using Resources = Properties.Resources;

    #endregion

    /// <summary>
    /// Represents a command link button. Works also on Windows XP in compatibility mode. Supports flat styles, elevated mode, high contrast mode,
    /// custom colors even in disabled mode and even for the default glyph (on Windows 10 and above), buffered animations and more.
    /// </summary>
    [ToolboxBitmap(typeof(CommandLinkButton), "Resources.Toolbox.CommandLinkButton.png")]
    [Description("Vista-like CommandLink button that works also in compatibility mode. On Vista and above you can set FlatStyle to System to render the button by the Windows.")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class CommandLinkButton : Button, ISupportsDisabledColor, ISupportsFadingInternal, IPerMonitorDpiAware
    {
        #region Nested Classes

        private sealed class CustomAppearanceState
        {
            #region Fields

            internal string? DescriptionText;
            internal Color DescriptionColor;
            internal FadingOptions FadingOptions;

            #endregion

            #region Methods

            public override bool Equals(object? obj)
            {
                if (obj is not CustomAppearanceState other)
                    return false;

                if ((FadingOptions & FadingOptions.TextChange) != FadingOptions.None && DescriptionText != other.DescriptionText)
                    return false;
                if ((FadingOptions & FadingOptions.ColorChange) != FadingOptions.None && DescriptionColor != other.DescriptionColor)
                    return false;
                return true;
            }

            // Never used in a dictionary
            public override int GetHashCode() => 0;

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color defaultEnabledThemedForeColor = Color.FromArgb(21, 28, 85);
        private static readonly Color defaultDisabledThemedForeColor = Color.FromArgb(126, 133, 156);
        private static readonly Color defaultHoveredColor = Color.FromArgb(7, 74, 229);
        private static readonly Color defaultPressedColor = Color.FromArgb(6, 32, 115);
        private static readonly Color pressedBackColor = Color.FromArgb(96, 230, 230, 230);
        private static readonly Color pressedEdgeColor = Color.FromArgb(96, 160, 160, 160);
        private static readonly Color hoveredBackColor = Color.FromArgb(96, 222, 222, 222);
        private static readonly Color selectedFrameColor = Color.FromArgb(64, 0, 204, 255);
        private static readonly Color selectedFrameColorAlternative = Color.FromArgb(192, 0, 204, 255);
        private static readonly Size referenceThemedGlyphSize = new Size(20, 20);
        private static readonly Size referenceNonThemedGlyphSize = new Size(17, 17);

        private static Bitmap? noGlyph;
        private static Font? defaultNonThemedTextFont;

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;
        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        // Unlike in AdvancedButton, we always have default fonts, even when AutoScaleFont is not set, because the fonts are not inherited from the parent.
        private readonly ScalingFont defaultTextFont;
        private readonly ScalingFont defaultDescriptionFont;

        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool isElevated;
        private bool useDefaultGlyph = true;
        private bool isImageUpToDate = true;
        private bool hasPaintError;
        private int dpiChangingCount;
        //private bool isLoaded; // see the commented OnCreateControl
        private string? description;

        private Brush? pressedBrush;
        private Brush? hoveredBrush;
        private GraphicsPath? outerBorder;
        private GraphicsPath? innerBorder;
        private GraphicsPath? selectionBorder;
        private Font? themedFontLarge;
        private Font? themedFontSmall;
        private ScalingFont? textFont;
        private ScalingFont? descriptionFont;
        private Image? currentImage;

        // these must not be disposed, they are just references to statically cached images
        private Image? cachedSecurityShieldImage;
        private Image? cachedDefaultGlyphNormal;
        private Image? cachedDefaultGlyphHovered;
        private Image? cachedDefaultGlyphDisabled;
        private Size defaultGlyphSize;
        private PointF lastScale;

        // NOTE: Unlike in AdvancedTextBox and AdvancedComboBox, we never set the base colors, because we handle all non-System drawings in the reimplemented adapters.
        // We only need to invoke OnBackColorChanged and OnForeColorChanged when the overriding colors are changed.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        private Color descriptionColor;
        private Color highlightTextColor;
        private Color highlightDescriptionColor;
        private Color pressedTextColor;
        private Color pressedDescriptionColor;

        private FlatStyle lastFlatStyle = FlatStyle.Standard; // the explicitly set or the detected flat style changed in base
        private FlatStyle reportedFlatStyle = FlatStyle.Standard; // the flat style that is reported by the control (can be different when base does not support System)
        private ContentAlignment lastImageAlign;
        private RenderingQuality textRenderingQuality;
        private RenderingQuality visualsRenderingQuality = RenderingQuality.High;

        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private int fadingAnimationDefaultSpeed = 500;
        private Timer? defaultAnimationTimer;
        private bool fadingAnimationsEnabled = true;
        private bool isAlternativeDefaultImage;
        private bool suppressFontChanged;
        private bool autoScaleFont = true;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("CommandLinkButton")]
        public event EventHandler<PaintStateEventArgs>? PaintState
        {
            add => Events.AddHandler(nameof(PaintState), value);
            remove => Events.RemoveHandler(nameof(PaintState), value);
        }

        #endregion

        #region Properties

        #region Static Properties

        private static Bitmap NoGlyph
        {
            get
            {
                if (noGlyph != null)
                    return noGlyph;

                noGlyph = new Bitmap(1, 1);
                noGlyph.SetPixel(0, 0, Color.Transparent);
                return noGlyph;
            }
        }

        /// <summary>
        /// Gets whether the current operating system supports command link buttons natively.
        /// That is on Windows Vista or later, when Application.EnableVisualStyles() was called (even on Mono).
        /// NOTE: It does not mean that visual styles are actually used (use <see cref="IsNativeVisualStylesRenderingAvailable"/> to check that).
        ///       It also does not mean that native rendering is actually used (use <see cref="IsNativeRendering"/> to check that)
        /// </summary>
        private static bool IsNativelySupported => OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.InitializedWithVisualStyles;

        private static Font DefaultNonThemedTextFont => defaultNonThemedTextFont ??= new Font(ScaleHelper.DialogFont, FontStyle.Bold);
        private static bool IsNativeVisualStylesRenderingAvailable => IsNativelySupported && VisualStyleHelper.RenderWithVisualStyles;
        private static Color ThemedForeColor => !IsNativeVisualStylesRenderingAvailable ? defaultEnabledThemedForeColor : GetDefaultTextColor(COMMANDLINKSTATES.CMDLS_NORMAL, defaultEnabledThemedForeColor);
        private static Color ThemedHoveredColor => !IsNativeVisualStylesRenderingAvailable ? defaultHoveredColor : GetDefaultTextColor(COMMANDLINKSTATES.CMDLS_HOT, defaultHoveredColor);
        private static Color ThemedPressedColor => !IsNativeVisualStylesRenderingAvailable ? defaultPressedColor : GetDefaultTextColor(COMMANDLINKSTATES.CMDLS_PRESSED, defaultPressedColor);
        private static Color ThemedDisabledColor => !IsNativeVisualStylesRenderingAvailable ? defaultDisabledThemedForeColor : GetDefaultTextColor(COMMANDLINKSTATES.CMDLS_DISABLED, defaultDisabledThemedForeColor);

        #endregion

        #region Instance Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work on Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("CommandLinkButton")]
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
        [Category("CommandLinkButton")]
        [DefaultValue(FadingOptions.StandardEffects)]
        [Description("Gets or sets fading options of the control.")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FadingOptions FadingAnimationOptions
        {
            // publicly not including CustomChange, but it is returned by the explicit implementation of ISupportsFadingInternal
            get => fadingOptions & ~ControlAppearanceState.CustomChange;
            set
            {
                if (fadingOptions == value)
                    return;

                if (!Enum<FadingOptions>.AllFlagsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                fadingOptions = value;

                // Including custom change in fading options when TextChange or ColorChange is set, so we can fade also on Description text/color changes
                if ((value & (FadingOptions.TextChange | FadingOptions.ColorChange)) != FadingOptions.None)
                    fadingOptions |= ControlAppearanceState.CustomChange;

                // storing invisible state so when control turns visible it will fade on when enabled
                if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                    fadingPainter.State = GetAppearance();

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.
        /// </summary>
        [Category("CommandLinkButton")]
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
        /// Gets or sets whether an elevated shield icon should be displayed.
        /// </summary>
        [Category("CommandLinkButton")]
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
                isImageUpToDate = false;
                CheckImage();

                Invalidate();
                if (AutoSize)
                    PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets whether the default arrow glyph should be displayed.
        /// </summary>
        [Description("Gets or sets whether the default arrow glyph should be displayed.")]
        [Category("CommandLinkButton")]
        [DefaultValue(true)]
        public bool UseDefaultGlyph
        {
            get => useDefaultGlyph;
            set
            {
                if (useDefaultGlyph == value)
                    return;

                useDefaultGlyph = value;
                isImageUpToDate = false;
                CheckImage();

                Invalidate();
                if (AutoSize)
                    PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ellipsis character (...) appears at the right edge of the control, denoting that the control text extends beyond the specified length of the control.
        /// </summary>
        [DefaultValue(true)] // This is the only reason for redefining.
        public new bool AutoEllipsis
        {
            get => base.AutoEllipsis;
            set => base.AutoEllipsis = value;
        }

        /// <summary>
        /// Gets or sets whether the command link button automatically resizes itself to its content.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets whether the command link button automatically resizes itself to its content.")]
        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                if (base.AutoSize == value)
                    return;

                // turning on ButtonBase.AutoSize would turn off AutoEllipsis
                bool autoEllipsis = base.AutoEllipsis;
                ResetSizeCache();
                base.AutoSize = value;
                base.AutoEllipsis = autoEllipsis;
            }
        }

        /// <summary>
        /// Gets or sets the mode by which the <see cref="CommandLinkButton"/> automatically resizes itself.
        /// </summary>
        [DefaultValue(AutoSizeMode.GrowAndShrink)] // "overridden" only because of this.
        public new AutoSizeMode AutoSizeMode
        {
            get => base.AutoSizeMode;
            set => base.AutoSizeMode = value;
        }

        /// <summary>
        /// Gets or sets text of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets text of the command link button.")]
        [AllowNull]
        public override string Text
        {
            get => base.Text;
            set
            {
                ResetSizeCache();
                base.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the text on the command link button control.
        /// </summary>
        [DefaultValue(ContentAlignment.TopLeft)] // overridden only because of default value
        [Description("Gets or sets the alignment of the text on the command link button control. Has effect only when FlatStyle is not System.")]
        public override ContentAlignment TextAlign
        {
            get => base.TextAlign;
            set => base.TextAlign = value;
        }

        /// <summary>
        /// Gets or sets the alignment of the image on the command link button control. A Top alignment attempts to align the image to the middle of the first row of <see cref="Text"/>.
        /// </summary>
        [DefaultValue(ContentAlignment.TopLeft)] // "overridden" only because of default value
        [Description("Gets or sets the alignment of the image on the command link button control. Has effect only when FlatStyle is not System. "
            + "A Top alignment attempts to align the image to the middle of the first row of Text.")]
        public new ContentAlignment ImageAlign
        {
            get => base.ImageAlign;
            set => base.ImageAlign = value;
        }

        /// <summary>
        /// Gets or sets description text for the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets description text for the command link button.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Localizable(true)]
        [DefaultValue(null)]
        public string? Description
        {
            get => description;
            set
            {
                if (description == value)
                    return;

                description = value;
                ResetSizeCache();
                ResetNativeDescription();

                Invalidate();
                if (base.AutoSize)
                    PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Font"/> and <see cref="DescriptionFont"/> should be automatically scaled when DPI changes and the current thread has per-monitor DPI awareness.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see langword="true"/>, <see cref="Font"/> and <see cref="DescriptionFont"/>
        /// are automatically scaled to the current DPI of the corresponding display on every executing platform.</para>
        /// <para>When <see langword="false"/>, the <see cref="Font"/> may or may not be scaled, depending on the default behavior of the executing platform, and <see cref="DescriptionFont"/> is never scaled.</para>
        /// <note>Please note that this property affects the font only. The default glyph and the elevated icon (see the <see cref="IsElevated"/> property) are always scaled with V2 awareness,
        /// whereas scaling the size (when <see cref="AutoSize"/> is <see langword="false"/>) and location always depends on the executing platform behavior.</note>
        /// </remarks>
        [Category("CommandLinkButton")]
        [DefaultValue(true)]
        [Description("True to auto scale Font and DescriptionFont when DPI changes; False to rely on the default behavior of the current executing platform, "
            + "which scales Font on the newer .NET versions only and never scales DescriptionFont.")]
        public bool AutoScaleFont
        {
            get => autoScaleFont;
            set
            {
                if (autoScaleFont == value)
                    return;

                autoScaleFont = value;
                PointF scale = value ? this.GetScale() : ScaleHelper.SystemScale;
                textFont?.ResetFrom(textFont.Font, scale);
                descriptionFont?.ResetFrom(descriptionFont.Font, scale);

                if (!value)
                    return;

                // resetting the default fonts if no explicit fonts are set
                bool changed = false;
                if (textFont == null)
                {
                    defaultTextFont.Scale(scale);
                    SetFont(defaultTextFont);
                    changed = true;
                }

                if (descriptionFont == null)
                {
                    defaultDescriptionFont.Scale(scale);
                    changed = true;
                }

                if (changed)
                {
                    Invalidate();
                    ResetSizeCache();
                    if (AutoSize)
                        PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the font of the text displayed by the control. Has effect only when FlatStyle is not System.")]
        [AllowNull]
        public override Font Font
        {
            get => (textFont ?? defaultTextFont).Font;
            set
            {
                if (dpiChangingCount > 0 && AutoScaleFont)
                    return;

                if (!ReferenceEquals(textFont?.Font, value))
                    ResetSizeCache();

                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;

                // resetting the default font
                if (value is null)
                {
                    textFont?.Dispose();
                    textFont = null;
                    defaultTextFont.Scale(scale);
                }
                // setting a font explicitly
                else
                {
                    if (textFont == null)
                        textFont = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                    else
                        textFont.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                }

                SetFont(textFont ?? defaultTextFont);
            }
        }

        /// <summary>
        /// Gets or sets the font of the description displayed by the control.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the font of the description displayed by the control. Has effect only when FlatStyle is not System.")]
        [AllowNull]
        public Font DescriptionFont
        {
            get => (descriptionFont ?? defaultDescriptionFont).Font;
            set
            {
                if (ReferenceEquals(descriptionFont?.Font, value))
                    return;

                ResetSizeCache();
                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;

                // resetting the default font
                if (value is null)
                {
                    descriptionFont?.Dispose();
                    descriptionFont = null;
                    defaultDescriptionFont.Scale(scale);
                }
                // setting a font explicitly
                else
                {
                    if (descriptionFont == null)
                        descriptionFont = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                    else
                        descriptionFont.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                }

                Invalidate();
                if (AutoSize)
                    PerformLayout();
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
        [Category("CommandLinkButton")]
        [Description("Determines the background color when the control is Enabled.")]
        public Color EnabledBackColor
        {
            // Standard/System flat style with visual styles: reporting transparent background. This fixes the ugly stripes issue
            // when the parent is enlarged while the control is partially invisible, for example.
            // Not applying on Mono, because it turns fading animations off to prevent flickering.
            get => !OSHelper.IsMono && !DesignMode && VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? Color.Transparent
                : !enabledBackColor.IsEmpty ? enabledBackColor
                : base.BackColor;
            set
            {
                if (enabledBackColor == value)
                    return;
                enabledBackColor = value;
                if (Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor
                : VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedForeColor
                : base.ForeColor;
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
        [Category("CommandLinkButton")]
        [Description("Determines the disabled background color. Has effect only when FlatStyle is Popup or Flat, or when visual styles are not enabled and FlatStyle is Standard.")]
        public Color DisabledBackColor
        {
            // Standard/System flat style with visual styles: reporting transparent background. This fixes the ugly stripes issue
            // when the parent is enlarged while the control is partially invisible, for example.
            // Not applying on Mono, because it turns fading animations off to prevent flickering.
            get => !OSHelper.IsMono && !DesignMode && VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? Color.Transparent
                : !disabledBackColor.IsEmpty ? disabledBackColor
                : base.BackColor;
            set
            {
                if (disabledBackColor == value)
                    return;
                disabledBackColor = value;
                if (!Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Determines the disabled text color. Has effect only when FlatStyle is not System.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor
                : VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedDisabledColor
                : defaultDisabledForeColor;
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
        /// Gets or sets the description color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the description color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color DescriptionColor
        {
            get => !descriptionColor.IsEmpty ? descriptionColor : EnabledForeColor;
            set
            {
                if (descriptionColor == value)
                    return;
                descriptionColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the highlighted text color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the highlighted text color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color HighlightTextColor
        {
            get => !highlightTextColor.IsEmpty ? highlightTextColor
                : VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedHoveredColor
                : base.ForeColor;
            set
            {
                if (highlightTextColor == value)
                    return;
                highlightTextColor = value;
                if (isHovered)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the highlighted description color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the highlighted description color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color HighlightDescriptionColor
        {
            get => !highlightTextColor.IsEmpty ? highlightTextColor : HighlightTextColor;
            set
            {
                if (highlightDescriptionColor == value)
                    return;
                highlightDescriptionColor = value;
                if (isHovered)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the pressed text color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the pressed text color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color PressedTextColor
        {
            get => !pressedTextColor.IsEmpty ? pressedTextColor
                : VisualStyleHelper.RenderWithVisualStyles && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedPressedColor
                : base.ForeColor;
            set
            {
                if (pressedTextColor == value)
                    return;
                pressedTextColor = value;
                if (isPressed)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the pressed description color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the pressed description color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color PressedDescriptionColor
        {
            get => !pressedTextColor.IsEmpty ? pressedTextColor : PressedTextColor;
            set
            {
                if (pressedDescriptionColor == value)
                    return;
                pressedDescriptionColor = value;
                if (isPressed)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="CommandLinkButton"/>.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the rendering text quality of the command link button. Has effect only when FlatStyle is not System.")]
        [DefaultValue(RenderingQuality.High)]
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
        /// Gets or sets the rendering quality of the <see cref="CommandLinkButton"/> visuals.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the rendering quality of the command link button visuals. Affects the default glyph rendering in high DPI mode.")]
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
                ResetGlyphCache();
                Invalidate();
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
        /// Gets or sets the flat style state of the command link button.
        /// </summary>
        [DefaultValue(FlatStyle.Standard)]
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get => reportedFlatStyle;
            set
            {
                if (reportedFlatStyle == value && base.FlatStyle == value && lastFlatStyle == value)
                    return;

                bool recreateHandle = IsNativelySupported && IsHandleCreated &&
                    ((base.FlatStyle == FlatStyle.System && value != FlatStyle.System) || (base.FlatStyle != FlatStyle.System && value == FlatStyle.System));
                base.FlatStyle = lastFlatStyle = reportedFlatStyle = value;
                OnFlatStyleChanged(false, recreateHandle);
            }
        }

        /// <summary>
        /// Gets or sets the image that is displayed on the button control.
        /// </summary>
        [DefaultValue(null)]
        public new Image? Image // it is also detected when base.Image changes but reacting onto that in OnPaint has a performance cost
        {
            get => base.Image;
            set
            {
                base.Image = value;
                isImageUpToDate = false;
                CheckImage();
                PerformLayout();
            }
        }

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                // Adding command link style only in native rendering; otherwise, an uncontrollable paint occurs on enabled change (directly, without WM_PAINT),
                // which draws the control in System style without description, which causes flickering in non-system modes
                CreateParams cp = base.CreateParams;
                if (IsNativeRendering)
                    cp.Style |= Constants.BS_COMMANDLINK;
                return cp;
            }
        }

        /// <inheritdoc />
        protected override Size DefaultSize => new(160, 41);

        #endregion

        #region Private Properties

        private Image SecurityShieldImage
        {
            get
            {
                if (cachedSecurityShieldImage == null)
                {
                    Size size = this.ScaleSize(IconsHelper.SmallIconReferenceSize);
                    using var icon = Icons.SystemShield;
                    cachedSecurityShieldImage = icon.GetCachedBitmap(nameof(Icons.SystemShield), size);
                }

                return cachedSecurityShieldImage;
            }
        }

        /// <summary>
        /// Gets whether Vista+ system rendering is used.
        /// NOTE: it does NOT mean that theming is also used.
        /// </summary>
        private bool IsNativeRendering => base.FlatStyle == FlatStyle.System && IsNativelySupported && !OSHelper.IsMono;

        private Font DefaultTextFont
        {
            get
            {
                if (!VisualStyleHelper.RenderWithVisualStyles)
                    return DefaultNonThemedTextFont;

                if (themedFontLarge == null)
                {
                    if (IsNativeVisualStylesRenderingAvailable)
                        themedFontLarge = VisualStyleHelper.GetFont(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_COMMANDLINK);

                    if (themedFontLarge == null)
                    {
                        themedFontLarge = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
                        if (themedFontLarge.Name != "Segoe UI")
                        {
                            themedFontLarge.Dispose();
                            themedFontLarge = new Font("MS Shell Dlg 2", 12f, FontStyle.Regular, GraphicsUnit.Point);
                        }
                    }
                }

                return themedFontLarge;
            }
        }

        private Font DefaultDescriptionFont
        {
            get
            {
                if (!VisualStyleHelper.RenderWithVisualStyles)
                    return ScaleHelper.DialogFont;

                if (themedFontSmall == null)
                {
                    if (IsNativeVisualStylesRenderingAvailable)
                    {
                        var largeFont = DefaultTextFont;
                        themedFontSmall = new Font(largeFont.FontFamily, largeFont.SizeInPoints * 0.75f, largeFont.Style, GraphicsUnit.Point, largeFont.GdiCharSet, largeFont.GdiVerticalFont);
                    }
                    else
                    {
                        themedFontSmall = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
                        if (themedFontSmall.Name != "Segoe UI")
                        {
                            themedFontSmall.Dispose();
                            themedFontSmall = new Font("MS Shell Dlg 2", 9f, FontStyle.Regular, GraphicsUnit.Point);
                        }
                    }
                }

                return themedFontSmall;
            }
        }

        private Brush PressedBrush
        {
            get
            {
                if (pressedBrush != null)
                    return pressedBrush;

                // standard style
                float p1, p2;
                if (Height <= 6)
                    p1 = p2 = 0.5f;
                else
                {
                    p1 = 100f / Height * 0.03f;
                    p2 = 1f - p1;
                }

                pressedBrush = new LinearGradientBrush(ClientRectangle, pressedEdgeColor, pressedBackColor, LinearGradientMode.Vertical)
                {
                    Blend = new Blend
                    {
                        Factors = [0f, 1f, 1f, 0.5f],
                        Positions = [0f, p1, p2, 1f]
                    }
                };

                return pressedBrush;
            }
        }

        private Brush HoveredBrush
        {
            get
            {
                if (hoveredBrush != null)
                    return hoveredBrush;

                // Standard mode
                float p1, p2;
                if (Height <= 40)
                    p1 = p2 = 0.5f;
                else
                {
                    p1 = 100f / Height * 0.2f;
                    p2 = 1f - p1;
                }

                hoveredBrush = new LinearGradientBrush(ClientRectangle, Color.White, hoveredBackColor, LinearGradientMode.Vertical)
                {
                    Blend = new Blend
                    {
                        Factors = [0f, 0.8f, 0.8f, 1f],
                        Positions = [0f, p1, p2, 1f]
                    }
                };
                return hoveredBrush;
            }
        }

        private GraphicsPath OuterBorder
        {
            get
            {
                if (outerBorder == null)
                {
                    outerBorder = new GraphicsPath();
                    outerBorder.AddRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 3);
                }

                return outerBorder;
            }
        }

        private GraphicsPath InnerBorder
        {
            get
            {
                if (innerBorder == null)
                {
                    innerBorder = new GraphicsPath();
                    innerBorder.AddRoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 2);
                }

                return innerBorder;
            }
        }

        private GraphicsPath SelectionBorder
        {
            get
            {
                if (selectionBorder != null)
                    return selectionBorder;

                // classic style
                if (base.FlatStyle == FlatStyle.Popup || (base.FlatStyle == FlatStyle.Standard && !VisualStyleHelper.RenderWithVisualStyles))
                {
                    selectionBorder = new GraphicsPath();
                    selectionBorder.AddRectangle(new Rectangle(0, 0, Width - 1, Height - 1));
                    return selectionBorder;
                }

                // themed/flat selection
                selectionBorder = new GraphicsPath();
                selectionBorder.AddRoundedRectangle(new Rectangle(1, 0, Width - 3, Height - 1), 3);
                return selectionBorder;
            }
        }

        /// <summary>
        /// The size of every non-text content, including image, borders and padding.
        /// </summary>
        private Size BordersAndPadding => new(HorizontalPadding, VerticalPadding);
        private int HorizontalPadding => (HorizontalBasePadding << 1) + (ImageAlign.AnyCenter() ? 0 : ImagePadding + ImageSize.Width + ImageTextMargin);
        private int HorizontalBasePadding => UsesTheming ? 2 : 3;
        private int VerticalPadding => VerticalBasePadding << 1;
        private int VerticalBasePadding => UsesTheming ? 10 : 6;

        private bool UsesTheming => VisualStyleHelper.RenderWithVisualStyles && base.FlatStyle is FlatStyle.Standard or FlatStyle.System;
        private int ImagePadding => UsesTheming ? 5 : 3;
        private int ImageTextMargin => UsesTheming ? 1 : 4;

        private Size ImageSize => base.Image != null ? base.Image.Size
            : isElevated ? SecurityShieldImage.Size // note: cachedSecurityShieldImageSize is the scaled reference size, not necessarily the actual extracted size
            : useDefaultGlyph ? DefaultGlyphSize
            : new Size(1, 1);

        private Image DefaultGlyphNormal => cachedDefaultGlyphNormal ??= GetScaledDefaultGlyph(Resources.CommandLinkNormal, nameof(Resources.CommandLinkNormal));
        private Image DefaultGlyphHovered => cachedDefaultGlyphHovered ??= GetScaledDefaultGlyph(Resources.CommandLinkHovered, nameof(Resources.CommandLinkHovered));
        private Image DefaultGlyphDisabled => cachedDefaultGlyphDisabled ??= GetScaledDefaultGlyph(Resources.CommandLinkDisabled, nameof(Resources.CommandLinkDisabled));

        private Size DefaultGlyphSize
        {
            get
            {
                if (defaultGlyphSize == Size.Empty)
                {
                    if (IsNativeVisualStylesRenderingAvailable)
                    {
                        if (ScaleHelper.PerMonitorDpiAwarenessVersion == 1)
                            defaultGlyphSize = this.ScaleSize(referenceThemedGlyphSize);
                        else
                        {
                            using Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero);
                            defaultGlyphSize = VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, this, g, (int)BUTTONPARTS.BP_COMMANDLINKGLYPH, 1, false);
                        }
                    }
                    else
                        defaultGlyphSize = DefaultGlyphNormal.Size;
                }

                return defaultGlyphSize;
            }
        }

        #endregion

        #region Explicitly Implemented Interface Properties

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

        #endregion

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="CommandLinkButton"/> instance.
        /// </summary>
        public CommandLinkButton()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.AutoEllipsis = true;
            base.TextAlign = ContentAlignment.TopLeft;
            base.ImageAlign = lastImageAlign = ContentAlignment.TopLeft;
            textRenderingQuality = RenderingQuality.High;
            fadingPainter = new FadingPainterInternal(this, Constants.ThemeClassButton);
            CheckStyles();
            defaultTextFont = new ScalingFont(DefaultTextFont, ScaleHelper.SystemScale);
            defaultDescriptionFont = new ScalingFont(DefaultDescriptionFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();

            // Using this instead of overriding OnSystemColorsChanged so GetPreferredSize
            // works correctly when FlayStyle is System and visual styles are turned on or off.
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
        }

        #endregion

        #region Methods

        #region Static Methods

        private static Color GetDefaultTextColor(COMMANDLINKSTATES state, Color defaultColor) =>
            VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_COMMANDLINK, (int)state, defaultColor);

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            if (IsNativeRendering)
            {
                SIZE s = new SIZE(
                    proposedSize.Width == 0 ? Int32.MaxValue : proposedSize.Width,
                    proposedSize.Height == 0 ? Int32.MaxValue : proposedSize.Height);

                string origText = base.Text;
                if (String.IsNullOrEmpty(origText))
                    base.Text = @" ";

                // getting the ideal native size from Windows
                unsafe
                {
                    User32.SendMessage(Handle, Constants.BCM_GETIDEALSIZE, IntPtr.Zero, new IntPtr(&s));
                }

                if (String.IsNullOrEmpty(origText))
                    base.Text = origText;

                preferredSize = s.ToSize();
                preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
                return preferredSize;
            }

            TextFormatFlags formatFlags = this.GetFormatFlags();

            Size padding = BordersAndPadding;
            Size proposedTextSize = proposedSize - padding;

            // 0 or 1 means unbounded
            if (proposedTextSize.Width <= 1)
                proposedTextSize.Width = Int32.MaxValue;
            if (proposedTextSize.Height <= 1)
                proposedTextSize.Height = Int32.MaxValue;

            using Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero);
            bool gdiPlusTextRendering = UseCompatibleTextRendering;
            g.SetTextRenderingQuality(textRenderingQuality, gdiPlusTextRendering);

            Size textSize = Size.Empty;
            StringFormat? sf = gdiPlusTextRendering ? formatFlags.ToStringFormat() : null;
            if (!String.IsNullOrEmpty(Text))
            {
                textSize = gdiPlusTextRendering
                    ? g.MeasureString(Text, Font, proposedTextSize, sf).Ceiling()
                    : TextRenderer.MeasureText(g, Text, Font, proposedTextSize, formatFlags);
            }

            Size descSize = Size.Empty;
            if (!String.IsNullOrEmpty(description))
            {
                descSize = gdiPlusTextRendering
                    ? g.MeasureString(description, DescriptionFont, proposedTextSize, sf).Ceiling()
                    : TextRenderer.MeasureText(g, description, DescriptionFont, proposedTextSize, formatFlags);
            }

            bool useTheming = UsesTheming;
            preferredSize = new Size(Math.Max(textSize.Width, descSize.Width), textSize.Height + (descSize.Height > 0 ? descSize.Height + (useTheming ? 1 : 2) : 0)) + padding;

            // HorizontalPadding already contains image width. Height is calculated here.
            int preferredImageHeight = ImageSize.Height + VerticalPadding;

            if (preferredImageHeight > preferredSize.Height)
                preferredSize.Height = preferredImageHeight;

            preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
            return preferredSize;
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT when base.FlatStyle == FlatStyle.System && !OSHelper.IsMono:
                    // Image and FlatStyle are not overridable properties so in case of native rendering reacting their change here.
                    // (On custom rendering, image change is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        bool recreateHandle = (base.FlatStyle == FlatStyle.System && lastFlatStyle != FlatStyle.System)
                            || (base.FlatStyle != FlatStyle.System && lastFlatStyle == FlatStyle.System);
                        lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged(true, recreateHandle);
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

                    CheckDpiChange();
                    return;

                // Known issue: Security shield icon size is not updated with non-V2 awareness
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

                    if (IsNativeRendering)
                    {
                        // Without this the custom image is replaced by the shield icon on DPI change if it was ever displayed
                        if (isElevated && base.Image != null)
                        {
                            isImageUpToDate = false;
                            Invalidate();
                        }
#if NETFRAMEWORK
                        // .NET Framework: Font, Glyph and Elevated icon size is not updated on DPI change, so we need to recreate the handle
                        // Note: Would not be needed for .NET Framework 4.7+ when V2 awareness is set both in the app.config and the manifest
                        if (Created)
                            RecreateHandle();
                        isImageUpToDate = false;
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
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CheckStyles();
            ResetNativeDescription();

            // Adjusting default fonts even if AutoScaleFont is false.
            // Then calling CheckDpiChange so if there are explicitly set fonts (and AutoScaleFont is true), they will be scaled to the parent.
            PointF scale = this.GetScale();
            if (textFont == null)
                defaultTextFont.Scale(scale);
            if (descriptionFont == null)
                defaultDescriptionFont.Scale(scale);
            CheckDpiChange();
        }

        //  ISSUE: This would fix the appearance with System FlatStyle when DPI was changed after launching the application,
        //  and showing the control on the primary display. But in same cases it causes Win32Exceptions "Failed to set Win32 parent window of the Control".
        ///// <inheritdoc />
        //protected override void OnCreateControl()
        //{
        //    base.OnCreateControl();
        //    if (isLoaded)
        //        return;

        //    isLoaded = true;
        //    if (IsNativeRendering && this.GetScale() != ScaleHelper.SystemScale)
        //        RecreateHandle();
        //}

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting FlatStyle if needed (in System mode this is in WndProc)
            bool invalidated = false;
            if (base.FlatStyle != lastFlatStyle)
            {
                bool recreateHandle = (base.FlatStyle == FlatStyle.System && lastFlatStyle != FlatStyle.System)
                    || (base.FlatStyle != FlatStyle.System && lastFlatStyle == FlatStyle.System);
                lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                OnFlatStyleChanged(true, recreateHandle);
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
                // We tolerate one exception if we can recover from it in the next paint.
                // But if exceptions are thrown in two consecutive paints, we let the second one propagate.
                // A recoverable exception may occur on Windows 7 when switching from Aero to classic or high contrast theme,
                // when visual styles are turned off in the middle of the painting session.
                if (hasPaintError)
                    throw;
                hasPaintError = true;
                ResetCaches();
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
            if (!IsHandleCreated)
                return;

            // As we don't actually rely on the parent font, just inheriting the scaling of the new parent and adjusting default fonts even if AutoScaleFont is false.
            // Then calling CheckDpiChange so if there are explicitly set fonts (and AutoScaleFont is true), they will be scaled to the new parent.
            PointF scale = this.GetScale();
            if (textFont == null)
                defaultTextFont.Scale(scale);
            if (descriptionFont == null)
                defaultDescriptionFont.Scale(scale);
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
            // storing invisible state so when control turns visible it will fade on when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            CheckDefaultAnimation();
            base.OnVisibleChanged(e);
        }

        /// <inheritdoc />
        protected override void OnEnabledChanged(EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            isMouseDown = false;
            FreeBrushes();
            base.OnEnabledChanged(e);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            FreeBrushes();
            FreeRegions();
            ResetSizeCache();
            base.OnSizeChanged(e);
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
            if (OSHelper.IsMono)
                Invalidate();
        }

        /// <inheritdoc />
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!OSHelper.IsMono)
                return;
            Invalidate();
            CheckStyles();
        }

        /// <summary>
        /// Paints the specified state of this control, and raises the <see cref="PaintState"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintStateEventArgs"/> that contains the event data.</param>
        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            e.Graphics.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);

            if (!e.State.Visible)
                this.PaintTransparentBackground(e);
            else
            {
                // ButtonBase.OnPaint:
                if (AutoEllipsis)
                {
                    int preferredHeight = GetPreferredSize(new Size(Width, 0)).Height;
                    this.ShowToolTip(Height < preferredHeight);
                }
                else
                    this.ShowToolTip(false);

                if (GetStyle(ControlStyles.UserPaint))
                {
                    this.Animate();
                    ImageAnimator.UpdateFrames();
                    DoPaint(e);
                }
            }

            // Raising Paint
            if (Accessors.PaintEvent is object paintEventKey)
                Events.GetHandler<PaintEventHandler>(paintEventKey)?.Invoke(this, e);

            // Raising PaintState
            Events.GetHandler<EventHandler<PaintStateEventArgs>>(nameof(PaintState))?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            textFont = null; // disposed by owner, if needed
            descriptionFont = null; // disposed by owner, if needed
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;

            if (disposing)
            {
                FreeBrushes();
                FreeRegions();

                fadingPainter.Dispose();
                themedFontLarge?.Dispose();
                themedFontLarge = null;
                themedFontSmall?.Dispose();
                themedFontSmall = null;
                defaultTextFont.Dispose();
                defaultDescriptionFont.Dispose();
                textFont?.Dispose();
                descriptionFont?.Dispose();
                currentImage = null;
                cachedSecurityShieldImage = null;
                cachedDefaultGlyphDisabled = null;
                cachedDefaultGlyphNormal = null;
                cachedDefaultGlyphHovered = null;
            }

            base.Dispose(disposing);
            if (disposing)
                Events.Dispose();
        }

        #endregion

        #region Private Methods

        #region Static Methods

        private static int Adjust255(float percentage, int value)
        {
            int result = (int)(percentage * value);
            return result > 255 ? 255 : result;
        }

        #endregion

        #region Instance Methods

        private ControlAppearanceState GetAppearance()
        {
            return new ControlAppearanceState((int)BUTTONPARTS.BP_COMMANDLINK, (int)GetSystemState())
            {
                BackColor = BackColor,
                ForeColor = ForeColor,
                Enabled = Enabled,
                Hovered = isHovered,
                Pressed = isPressed,
                IsDefault = IsDefault,
                Focused = Focused,
                Text = base.Text,
                Visible = Visible,
                CustomState = new CustomAppearanceState
                {
                    FadingOptions = fadingOptions,
                    DescriptionText = Description,
                    DescriptionColor = Enabled ? DescriptionColor : DisabledForeColor
                }
            };
        }

        private COMMANDLINKSTATES GetSystemState()
        {
            if (!Enabled)
                return COMMANDLINKSTATES.CMDLS_DISABLED;

            if (isPressed)
                return COMMANDLINKSTATES.CMDLS_PRESSED;

            if (isHovered)
                return COMMANDLINKSTATES.CMDLS_HOT;

            if (IsDefault)
                return fadingAnimationsEnabled && (fadingOptions & FadingOptions.StandardEffects) != FadingOptions.None && isAlternativeDefaultImage
                ? COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING
                : COMMANDLINKSTATES.CMDLS_DEFAULTED;

            return COMMANDLINKSTATES.CMDLS_NORMAL;
        }

        private void CheckStyles()
        {
            if (fadingAnimationsEnabled && fadingPainter.Enabled)
            {
                // to enable animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, base.FlatStyle != FlatStyle.System || OSHelper.IsMono);
        }

        private void CheckDefaultAnimation()
        {
            if (!OSHelper.IsWindowsVistaOrLater || !VisualStyleHelper.RenderWithVisualStyles
                || !VisualStyleHelper.HasDefaultAnimation((int)BUTTONPARTS.BP_COMMANDLINK, (int)COMMANDLINKSTATES.CMDLS_DEFAULTED, (int)COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING))
            {
                return;
            }

            bool enabled = base.FlatStyle == FlatStyle.Standard && !isPressed && !isHovered && IsDefault && VisualStyleHelper.RenderWithVisualStyles && !VisualStyleHelper.HighContrast;
            if (enabled && (defaultAnimationTimer == null || !defaultAnimationTimer.Enabled))
            {
                if (defaultAnimationTimer == null)
                {
                    defaultAnimationTimer = new Timer();
                    defaultAnimationTimer.Interval = UxTheme.TryGetThemeTransitionDuration(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_COMMANDLINK,
                        (int)COMMANDLINKSTATES.CMDLS_DEFAULTED,
                        (int)COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING,
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

        private void OnFlatStyleChanged(bool ignoreCheckImage, bool recreateHandle)
        {
            if (base.FlatStyle == FlatStyle.System && !IsNativelySupported)
            {
                // note: this will not change the reported FlatStyle in designer
                base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
            }

            if (recreateHandle && !OSHelper.IsMono)
                RecreateHandle();
            CheckDefaultAnimation();

            ResetNativeDescription();
            isImageUpToDate = false;
            if (!ignoreCheckImage)
                CheckImage();

            ResetSizeCache();
            FreeBrushes();
            FreeRegions();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void ResetSizeCache() => preferredSizeCache.Clear();

        private void ResetNativeDescription()
        {
            if (IsNativeRendering && IsHandleCreated)
                User32.SendMessage(Handle, Constants.BCM_SETNOTE, IntPtr.Zero, description);
        }

        private void DoPaint(PaintStateEventArgs e)
        {
            // Choosing image
            Image? img = base.Image;

            // painting background and image
            if (UsesTheming)
                PaintThemedAppearance(e, img);
            else if (base.FlatStyle == FlatStyle.Flat)
                PaintFlatAppearance(e, img);
            else
                PaintClassicAppearance(e, img);

            // drawing text
            DrawText(e);
        }

        private void PaintThemedAppearance(PaintStateEventArgs e, Image? image)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            ControlAppearanceState state = e.State;
            Rectangle clientRectangle = ClientRectangle;

            // painting the background (underlying part of the parent control)
            // NOTE: using PaintBackground with transparent backColor instead of PaintTransparentBackground to paint also the self background image if exists
            if (Parent != null)
                this.PaintBackground(e, clientRectangle, Color.Transparent);
            else
                e.Graphics.FillRectangle(state.BackColor.GetBrush(), new Rectangle(clientRectangle.X - 1, clientRectangle.Y - 1, clientRectangle.Width + 1, clientRectangle.Height + 1));

            // Native rendering
            if (OSHelper.IsWindowsVistaOrLater)
                VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, this, e.Graphics, state.SystemPartId, state.SystemStateId, clientRectangle);
            else
            {
                // Compatible rendering (Windows XP - mimicking the Vista appearance)
                if (state.Pressed)
                {
                    e.Graphics.FillRectangle(PressedBrush, new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1));
                    e.Graphics.DrawPath(Color.FromArgb(128, 128, 128, 128).GetPen(), OuterBorder);
                }
                else if (state.Hovered)
                {
                    e.Graphics.FillRectangle(HoveredBrush, new Rectangle(1, 1, clientRectangle.Width - 2, clientRectangle.Height - 2));
                    e.Graphics.DrawPath(Color.FromArgb(128, 255, 255, 255).GetPen(), InnerBorder);
                    e.Graphics.DrawPath(Color.FromArgb(128, 160, 160, 160).GetPen(), OuterBorder);
                }
                else // normal state
                {
                    // no drawing needed in normal state unless if focused or default
                    if (state.Enabled && (state.Focused || state.IsDefault))
                    {
                        Pen selectedFramePen = (!FadingPainterInternal.IsSupported || state.SystemStateId == (int)COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING
                            ? selectedFrameColorAlternative
                            : selectedFrameColor).GetPen();
                        e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                    }
                }
            }

            // Image
            PaintImage(e, image);

            // System FlatStyle does not animate the focus rectangle, so we don't take Focused from state to behave the same way
            if (state.Enabled && /*state.*/Focused && ShowFocusCues)
                DrawFocusRectangle(e);
        }

        private void PaintClassicAppearance(PaintStateEventArgs e, Image? image)
        {
            ControlAppearanceState state = e.State;
            Rectangle backRect = ClientRectangle;

            // Background
            Pen selectedFramePen = SystemPens.WindowFrame;
            this.PaintBackground(e, backRect, state.BackColor);

            if (state.Pressed)
            {
                e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                backRect.Inflate(-1, -1);
                ControlPaint.DrawBorder3D(e.Graphics, backRect, Border3DStyle.SunkenOuter);
            }
            else if (state.Hovered)
            {
                ControlPaint.DrawBorder3D(e.Graphics, backRect, Border3DStyle.Raised);

                // with classic state selection is drawn even if button is hovered
                if (state.Enabled && (Focused || state.IsDefault))
                    e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
            }
            else // normal state
            {
                if (state.Enabled && (Focused || state.IsDefault))
                    e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
            }

            // Image
            PaintImage(e, image);

            // Not taking Focused from state so it will not participate in fading animations (we allow it only for flat appearance)
            if (state.Enabled && Focused && ShowFocusCues)
                DrawFocusRectangle(e);
        }

        private void PaintFlatAppearance(PaintStateEventArgs e, Image? image)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            ControlAppearanceState state = e.State;
            int borderWidth = FlatAppearance.BorderSize;
            Color backColor = state.BackColor;

            // unlike other styles, these colors are calculated on the fly because FlatAppearance changes would be painful to track
            if (state.Pressed)
            {
                if (borderWidth != 0)
                    borderWidth++;

                if (!FlatAppearance.MouseDownBackColor.IsEmpty)
                    backColor = FlatAppearance.MouseDownBackColor;
                else
                {
                    if (VisualStyleHelper.HighContrast)
                    {
                        backColor = state.BackColor == SystemColors.Control
                            ? SystemColors.ControlDark
                            : ControlPaint.Dark(backColor);
                    }
                    else
                    {
                        backColor = state.BackColor == SystemColors.Control
                            ? SystemColors.ControlLightLight
                            : ControlPaint.LightLight(backColor);

                        float percentage = backColor.GetBrightness() < 0.5f ? 1.2f : 0.8f;
                        backColor = Color.FromArgb(Adjust255(percentage, backColor.R), Adjust255(percentage, backColor.G), Adjust255(percentage, backColor.B));
                    }
                }
            }
            else if (state.Hovered)
            {
                if (borderWidth != 0 && state.Focused)
                    borderWidth++;

                if (!FlatAppearance.MouseOverBackColor.IsEmpty)
                    backColor = FlatAppearance.MouseOverBackColor;
                else
                {
                    float percentage = 0.9f;
                    if (backColor.GetBrightness() < 0.5f)
                    {
                        backColor = ControlPaint.Light(backColor);
                        percentage = 1.2f;
                    }

                    backColor = Color.FromArgb(Adjust255(percentage, backColor.R), Adjust255(percentage, backColor.G), Adjust255(percentage, backColor.B));
                }
            }
            else // normal state
            {
                // no matter if button is enabled or not, border is the same
                if (state.Focused)
                {
                    if (borderWidth != 0)
                        borderWidth++;
                }
                else if (state.IsDefault)
                    borderWidth++;
            }

            this.PaintTransparentBackground(e);
            if (backColor.A != 0)
                e.Graphics.FillPath(backColor.GetBrush(), SelectionBorder);

            if (borderWidth > 0)
            {
                // pen is created locally because its width is variable and its color cannot be tracked by events
                using Pen pen = new Pen(FlatAppearance.BorderColor.IsEmpty ? SystemColors.ControlText : FlatAppearance.BorderColor, borderWidth);
                e.Graphics.DrawPath(pen, SelectionBorder);
            }

            // Image
            PaintImage(e, image);

            if (state.Enabled && state.Focused && ShowFocusCues)
            {
                Color focusColor = VisualStyleHelper.HighContrast ? SystemColors.WindowText
                    : (BackColor.GetBrightness() < 0.5f ? ControlPaint.Light(state.BackColor) : ControlPaint.Dark(state.BackColor));

                int borderSize = FlatAppearance.BorderSize;
                Rectangle rectangle = new Rectangle(ClientRectangle.X + borderSize + 4, ClientRectangle.Y + borderSize + 3, Width - borderSize * 2 - 9, Height - borderSize * 2 - 7);
                e.Graphics.DrawRectangle(focusColor.GetPen(), rectangle);
            }
        }

        private void DrawFocusRectangle(PaintStateEventArgs e)
        {
            var state = e.State;
            int width = OSHelper.IsWindows10OrLater || this.GetScale().X >= 1.5f ? 2 : 1;
            width = Math.Min(HorizontalBasePadding, this.ScaleWidth(width));
            Rectangle rect = ClientRectangle;
            rect.Inflate(-3, -3);
            for (int i = 0; i < width; i++)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, rect, state.ForeColor, state.BackColor);
                rect.Inflate(-1, -1);
            }
        }

        private void PaintImage(PaintStateEventArgs e, Image? image)
        {
            Rectangle bounds = GetImageBounds(e);
            var state = e.State;

            // default glyph when visual styles are available
            if (IsNativeVisualStylesRenderingAvailable && !isElevated && image == null && useDefaultGlyph)
            {
                bool isSimpleArrow = OSHelper.IsWindows10OrLater;
                bool isRightToLeft = RightToLeft == RightToLeft.Yes;
                bool isNonNativeSize = bounds.Size != VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, this, e.Graphics, (int)BUTTONPARTS.BP_COMMANDLINKGLYPH, state.SystemStateId, true);
                bool isCustomDrawnArrow = isSimpleArrow
                    && (VisualStyleHelper.HighContrast // high contrast with visual styles on Windows 10 or later: always drawing the arrow manually so it matches the theme colors
                        || isNonNativeSize
                        || !state.Enabled && DisabledForeColor != ThemedDisabledColor
                        || state.Enabled && (ForeColor != ThemedForeColor || HighlightTextColor != ThemedHoveredColor || PressedTextColor != ThemedPressedColor));

                // only Windows 10 and later: manually drawing the glyph if
                // - it has custom colors
                // - in RTL mode
                // - in high contrast mode
                // - if the size is not the same as the default size
                if (isSimpleArrow && (isRightToLeft || isCustomDrawnArrow))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    var color = !state.Enabled ? DisabledForeColor
                        : state.Pressed ? PressedTextColor
                        : state.Hovered ? HighlightTextColor
                        : ForeColor;
                    float unit = bounds.Width / 20f;
                    using Pen pen = new Pen(color, Math.Max(2, 1.5f * unit));
                    var y = bounds.Y + 12 * unit;
                    var x1 = bounds.X + (isRightToLeft ? 7 : 12) * unit;
                    var x2 = bounds.X + (isRightToLeft ? 1 : 18) * unit;
                    e.Graphics.DrawLine(pen, bounds.X + unit, y, bounds.X + 18 * unit, y);
                    e.Graphics.DrawLines(pen, new PointF[] { new(x1, bounds.Y + 6 * unit), new(x2, y), new(x1, bounds.Y + 18 * unit) });

                    return;
                }

                // drawing the default glyph natively
                if (!isRightToLeft)
                {
                    if (isNonNativeSize && visualsRenderingQuality == RenderingQuality.High)
                        VisualStyleHelper.RenderScaled(VisualStyleHelper.ButtonTheme, this, e.Graphics, (int)BUTTONPARTS.BP_COMMANDLINKGLYPH, state.SystemStateId, bounds);
                    else
                        VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, this, e.Graphics, (int)BUTTONPARTS.BP_COMMANDLINKGLYPH, state.SystemStateId, bounds);
                    return;
                }
            }

            Image? img = image;
            bool dispose = false;
            bool asDisabled = !state.Enabled;
            if (img == null)
            {
                if (isElevated)
                    img = SecurityShieldImage;
                else if (useDefaultGlyph)
                {
                    asDisabled = false; // we have specific disabled image
                    img = !state.Enabled ? DefaultGlyphDisabled
                        : state.Hovered && !state.Pressed ? DefaultGlyphHovered
                        : DefaultGlyphNormal;

                    // mirroring glyph
                    if (RightToLeft == RightToLeft.Yes)
                    {
                        img = (Bitmap)img.Clone();
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        dispose = true;
                    }
                }
            }

            if (img != null)
            {
                if (asDisabled)
                    e.Graphics.DrawImageGrayscale(img, bounds);
                else
                    e.Graphics.DrawImage(img, bounds);
            }

            if (dispose)
                img!.Dispose();
        }

        private void DrawText(PaintStateEventArgs e)
        {
            ControlAppearanceState state = e.State;
            CustomAppearanceState customState = (CustomAppearanceState)state.CustomState!;
            e.Graphics.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);

            // Setting colors. Note: these must not be differentiated in GetAppearance because they would mean non-standard differences,
            // which would be rendered as an immediate change if color changes are not included in the fading animations.
            Color textColor = state.ForeColor;
            Color descColor = customState.DescriptionColor;
            if (state.Pressed)
            {
                textColor = PressedTextColor;
                descColor = PressedDescriptionColor;
            }
            else if (state.Hovered)
            {
                textColor = HighlightTextColor;
                descColor = HighlightDescriptionColor;
            }

            var useTheming = UsesTheming;
            TextFormatFlags formatFlags = this.GetFormatFlags();
            bool gdiPlusTextRendering = UseCompatibleTextRendering;
            StringFormat? sf = gdiPlusTextRendering ? formatFlags.ToStringFormat() : null;

            Size proposedSize = Size - BordersAndPadding;
            Size textSize = Size.Empty;
            if (!String.IsNullOrEmpty(state.Text))
                textSize = gdiPlusTextRendering
                    ? e.Graphics.MeasureString(state.Text, Font, proposedSize, sf).ToSize()
                    : TextRenderer.MeasureText(e.Graphics, state.Text, Font, proposedSize, formatFlags);

            Size descSize = Size.Empty;
            if (!String.IsNullOrEmpty(customState.DescriptionText) && textSize.Height < proposedSize.Height)
            {
                Size size = new Size(proposedSize.Width, proposedSize.Height - textSize.Height);
                descSize = gdiPlusTextRendering
                    ? e.Graphics.MeasureString(customState.DescriptionText, DescriptionFont, size, sf).ToSize()
                    : TextRenderer.MeasureText(e.Graphics, customState.DescriptionText, DescriptionFont, size, formatFlags);
            }

            Size combinedSize = new Size(proposedSize.Width, Math.Min(proposedSize.Height, textSize.Height + descSize.Height));
            int offset = !useTheming && state.Pressed && base.FlatStyle != FlatStyle.Flat ? 1 : 0;
            int left = HorizontalBasePadding + (RtlTranslateContent(ImageAlign).AnyLeft() ? ImagePadding + ImageSize.Width + ImageTextMargin : 0) + offset;
            int top = VerticalBasePadding + offset;
            if ((formatFlags & TextFormatFlags.Bottom) != 0)
                top += Math.Max(proposedSize.Height - combinedSize.Height, 0);
            else if (((formatFlags & TextFormatFlags.VerticalCenter) != 0))
                top += Math.Max(proposedSize.Height / 2 - combinedSize.Height / 2, 0);

            if (!String.IsNullOrEmpty(Text))
            {
                Rectangle rectangle = new Rectangle(left + (useTheming ? 1 : 0), top, proposedSize.Width, Math.Min(textSize.Height, proposedSize.Height));
                if (gdiPlusTextRendering)
                    e.Graphics.DrawString(state.Text, Font, textColor.GetBrush(), rectangle, sf);
                else
                    TextRenderer.DrawText(e.Graphics, state.Text, Font, rectangle, textColor, formatFlags);
            }

            if (!String.IsNullOrEmpty(customState.DescriptionText) && proposedSize.Height > textSize.Height)
            {
                Rectangle rectangle = new Rectangle(left + (useTheming ? 2 : 0), top + textSize.Height + (useTheming ? 1 : 2), proposedSize.Width, Math.Min(descSize.Height, proposedSize.Height - textSize.Height));
                if (gdiPlusTextRendering)
                    e.Graphics.DrawString(customState.DescriptionText, DescriptionFont, descColor.GetBrush(), rectangle, sf);
                else
                    TextRenderer.DrawText(e.Graphics, customState.DescriptionText, DescriptionFont, rectangle, descColor, formatFlags);
            }
        }

        private Rectangle GetImageBounds(PaintStateEventArgs e)
        {
            ControlAppearanceState state = e.State;
            bool useTheming = UsesTheming;
            ContentAlignment imageAlignment = RtlTranslateContent(ImageAlign);
            bool isClassicPressed = !useTheming && state.Pressed && base.FlatStyle != FlatStyle.Flat;

            Size imageSize = ImageSize;
            var bounds = new Rectangle(Point.Empty, imageSize);
            var offset = isClassicPressed ? new Size(1, 1) : Size.Empty;

            if (imageAlignment.AnyLeft())
                bounds.X = HorizontalBasePadding + ImagePadding + offset.Width;
            else if (imageAlignment.AnyCenter())
                bounds.X = Width / 2 - imageSize.Width / 2 + offset.Width;
            else // any right
                bounds.X = Width - (HorizontalBasePadding + ImagePadding + imageSize.Width) + offset.Width;

            // Top: actually to the middle of the first row of Text - that's how System rendering also works
            if (imageAlignment.AnyTop())
                // Not using FontHeight because some platforms do not raise FontChanged from SetFont so the base does not always have the correct Font.
                bounds.Y = VerticalBasePadding + Math.Max(0, (int)Math.Ceiling(Font.SizeInPoints) / 2 - imageSize.Height / 2 - 1) + offset.Height;
            else if (imageAlignment.AnyMiddle())
                bounds.Y = Height / 2 - imageSize.Height / 2 + offset.Height;
            else // any bottom
                bounds.Y = Math.Max(VerticalBasePadding, Height - VerticalBasePadding - imageSize.Height) + offset.Height;

            return bounds;
        }

        /// <summary>
        /// Checks image consistency. Returns true if image update has been performed.
        /// </summary>
        private bool CheckImage()
        {
            if (!IsHandleCreated)
                return true;

            // if image is up-to-date checking consistency only (to handle setting base.Image)
            if (isImageUpToDate)
            {
                if (lastImageAlign == ImageAlign &&
                    (currentImage == base.Image
                    || currentImage == null && base.Image == null
                    || currentImage == NoGlyph && base.Image == null))
                    return false;
            }

            Invalidate();
            ResetSizeCache();
            lastImageAlign = ImageAlign;
            isImageUpToDate = true;
            if (base.Image != null)
            {
                currentImage = base.Image;
                if (IsNativeRendering)
                {
                    Bitmap bmp = base.Image as Bitmap ?? new Bitmap(base.Image);
                    User32.SendMessage(Handle, Constants.BM_SETIMAGE, new IntPtr(1), bmp.GetHicon());
                }

                return true;
            }

            currentImage = null;
            if (!IsNativeRendering)
                return true;

            if (isElevated)
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
            else if (useDefaultGlyph)
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, IntPtr.Zero);
            else
            {
                currentImage = NoGlyph;
                User32.SendMessage(Handle, Constants.BM_SETIMAGE, new IntPtr(1), NoGlyph.GetHicon());
            }

            return true;
        }

        private void ResetCaches()
        {
            ResetGlyphCache();
            ResetSizeCache();
            defaultGlyphSize = Size.Empty;
        }

        private void ResetTheme()
        {
            ResetCaches();

            // Resetting default fonts
            themedFontLarge?.Dispose();
            themedFontSmall?.Dispose();
            themedFontLarge = null;
            themedFontSmall = null;
            defaultTextFont.ResetFrom(DefaultTextFont, ScaleHelper.SystemScale);
            defaultDescriptionFont.ResetFrom(DefaultDescriptionFont, ScaleHelper.SystemScale);

            // When no explicit fonts are set, here we don't care about AutoScaleFont and always reset the correctly sized default fonts.
            // This is like assuming that the parent control has the correctly sized fonts, even though we don't actually rely on parent font.
            PointF scale = this.GetScale();
            if (textFont == null)
                defaultTextFont.Scale(scale);
            if (descriptionFont == null)
                defaultDescriptionFont.Scale(scale);

            SetFont(textFont ?? defaultTextFont);
            Invalidate();

            // Handling possible enabling/disabling of visual styles
            OnFlatStyleChanged(false, false);
            CheckStyles();
        }

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();
            if (scale == lastScale || Disposing || IsDisposed)
                return;

            ResetCaches();
            lastScale = scale;

            if (!AutoScaleFont)
                return;

            if (textFont is ScalingFont explicitTextFont)
                explicitTextFont.Scale(scale);
            else
                defaultTextFont.Scale(scale);

            if (descriptionFont is ScalingFont explicitDescFont)
                explicitDescFont.Scale(scale);
            else
                defaultDescriptionFont.Scale(scale);
            SetFont(textFont ?? defaultTextFont);

            if (AutoSize)
                PerformLayout();
        }

        private void ResetGlyphCache()
        {
            cachedDefaultGlyphDisabled = null;
            cachedDefaultGlyphNormal = null;
            cachedDefaultGlyphHovered = null;
            cachedSecurityShieldImage = null;
        }

        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;
        private bool ShouldSerializeFont() => textFont != null;
        private bool ShouldSerializeDescriptionFont() => descriptionFont != null;
        private bool ShouldSerializeDescriptionColor() => !descriptionColor.IsEmpty;
        private bool ShouldSerializeHighlightTextColor() => !highlightTextColor.IsEmpty;
        private bool ShouldSerializeHighlightDescriptionColor() => !highlightDescriptionColor.IsEmpty;
        private bool ShouldSerializePressedTextColor() => !pressedTextColor.IsEmpty;
        private bool ShouldSerializePressedDescriptionColor() => !pressedDescriptionColor.IsEmpty;

        private void FreeBrushes()
        {
            pressedBrush?.Dispose();
            pressedBrush = null;
            hoveredBrush?.Dispose();
            hoveredBrush = null;
        }

        private void FreeRegions()
        {
            outerBorder?.Dispose();
            outerBorder = null;
            innerBorder?.Dispose();
            innerBorder = null;
            selectionBorder?.Dispose();
            selectionBorder = null;
        }

        private Bitmap GetScaledDefaultGlyph(Icon icon, string name)
        {
            try
            {
                Size desiredSize = this.ScaleSize(VisualStyleHelper.RenderWithVisualStyles ? referenceThemedGlyphSize : referenceNonThemedGlyphSize);
                return visualsRenderingQuality == RenderingQuality.High
                    ? icon.GetCachedBitmap(name, desiredSize)
                    : icon.GetCachedBitmap(name, desiredSize, ScalingMode.NearestNeighbor);
            }
            finally
            {
                icon.Dispose();
            }
        }

        private void SetFont(ScalingFont? value)
        {
            if (value == null)
            {
                base.Font = null!;
                return;
            }

            // explicitly set text font must be forcibly set in base.Font
            bool force = ReferenceEquals(textFont, value);
            Font oldFont = base.Font;
            Font newFont = value.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            if (Equals(oldFont, newFont))
            {
                if (!force && (ReferenceEquals(oldFont, newFont) || !oldFont.IsDisposed()))
                    return;

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

        private new ContentAlignment RtlTranslateContent(ContentAlignment alignment)
        {
            // Not calling ContentAlignmentExtensions.RtlTranslateContent to avoid reflection
            if (OSHelper.IsMono && RightToLeft != RightToLeft.Yes)
                return alignment;
            return base.RtlTranslateContent(alignment);
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
        {
            // system speeds are determined by the painter
            return FadingAnimationDefaultSpeed;
        }

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

        void defaultAnimationTimer_Tick(object? sender, EventArgs e)
        {
            isAlternativeDefaultImage = !isAlternativeDefaultImage;
            Invalidate();
        }

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => ResetTheme();

        // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
