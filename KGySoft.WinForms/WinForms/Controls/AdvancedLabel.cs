#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedLabel.cs
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type. - false alarm in .NET Core 3.0 for Match[Collection]
#pragma warning disable CS8602 // Dereference of a possibly null reference. - false alarm in .NET Core 3.0 for Match[Collection]
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a label with additional features such as disabled colors, correct auto sizing, fixed auto size, advanced border styles and more.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedLabel"/> class offers the following features in addition to <see cref="LinkLabel"/>:
    /// <list type="bullet">
    /// <item><see cref="Label.AutoSize"/> property works as expected when label is docked</item>
    /// <item>Different rendering qualities (see <see cref="TextRenderingQuality"/>) property.</item>
    /// <item>Advanced border styles.</item>
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</item>
    /// <item>Automatic resolve of hyperlinks.</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// <item>Fixing some Mono-specific <see cref="Label"/>/<see cref="LinkLabel"/> issues, such as non-visible text with border, wrong rendering with padding, random
    /// exceptions from mouse events when links are used.</item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(AdvancedLabel), "Resources.Toolbox.AdvancedLabel.png")]
    [Description(@"A label that provides the following features in addition to regular Label:
- AutoSize works as expected when label is docked
- Adjustable rendering qualities
- Advanced border styles
- Adjustable colors in disabled state
- Fading animations
- Automatic resolving of hyperlinks
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedLabel : LinkLabel, ISupportsDisabledColor, ISupportsFadingInternal, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Regex rxHref = new Regex(@"<a\s+href=""(?<href>.*?)"">(?<caption>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxUrl = new Regex(@"(\w+:\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled);

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;
        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        private AdvancedBorderStyle borderStyle;
        private int borderWidth;
        private RenderingQuality textRenderingQuality;
        private Size lastProposedSize;

        // NOTE: Unlike in AdvancedTextBox and AdvancedComboBox, we never set the base colors, because we handle all non-System drawings in the reimplemented OnPaint.
        // We only need to invoke OnBackColorChanged and OnForeColorChanged when the overriding colors are changed.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        
        private HyperlinkResolveMode resolveHyperlinks;
        private string? rawText;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private bool fadingAnimationsEnabled = true;
        private bool hasPaintError;

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private int dpiChangingCount;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a link is clicked.
        /// To handle clicked links automatically, set <see cref="AutoHandleUrls"/>&#160;<see langword="true"/>, and if this event is subscribed, set <see cref="HandledEventArgs.Handled"/> <see langword="false"/> in the event handler.
        /// </summary>
        [Description("Occurs when a link is clicked. To handle clicked links automatically, set AutoHandleUrls true, and if this event is subsribed, set HyperlinkClickedEventArgs.Handled false in the event handler.")]
        [Category("AdvancedLabel")]
        public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked
        {
            add => Events.AddHandler(nameof(HyperlinkClicked), value);
            remove => Events.RemoveHandler(nameof(HyperlinkClicked), value);
        }

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedLabel")]
        public event EventHandler<PaintStateEventArgs>? PaintState
        {
            add => Events.AddHandler(nameof(PaintState), value);
            remove => Events.RemoveHandler(nameof(PaintState), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether clicked links should be handled automatically or when <see cref="HandledEventArgs.Handled"/> is set to <see langword="false"/>.
        /// <note type="caution">Caution: Setting this property to <see langword="true"/> may cause security issues. Use only in secure circumstances!</note>
        /// </summary>
        [Category("AdvancedLabel")]
        [DefaultValue(false)]
        [Description("Gets or sets whether clicked links should be handled automatically or when HyperlinkClickedEventArgs.Handled is set to false. Caution: Setting this property to true may cause security issues. Use only in secure circumstances!")]
        public bool AutoHandleUrls { get; set; }

        /// <inheritdoc cref="LinkLabel.LinkArea" />
        public new LinkArea LinkArea
        {
            get => base.LinkArea;
            set
            {
                if (resolveHyperlinks == HyperlinkResolveMode.None)
                    base.LinkArea = value;
            }
        }

        /// <summary>
        /// Gets or sets whether hyperlinks should be resolved.
        /// </summary>
        /// <remarks>
        /// <para>When value is <see cref="HyperlinkResolveMode.ResolveHrefsOnly"/>, hyperlinks will be resolved only in the following form:
        /// <example><c>This is a &lt;a href="https://kgysoft.net"&gt;hyperlink&lt;/a&gt;</c></example>
        /// </para>
        /// <para>When value is <see cref="HyperlinkResolveMode.ResolveAll"/>, simple inline hyperlinks will be resolved, too.</para>
        /// <para>When value is <see cref="HyperlinkResolveMode.None"/>, you need to explicitly set <see cref="LinkArea"/> to specify a
        /// link in the text. If it should contain more than one links, you can use the <see cref="LinkLabel.Links"/> property.</para>
        /// </remarks>
        [Category("AdvancedLabel")]
        [DefaultValue(HyperlinkResolveMode.None)]
        [Description(@"Gets or sets whether hyperlinks should be resolved.
When value is ""ResolveHrefsOnly"", hyperlinks will be resolved only in the following form:
This is a <a href=""https://kgysoft.net"">hyperlink</a>
When value is ""ResolveAll"", simple inline hyperlinks will be resolved, too.")]
        public HyperlinkResolveMode ResolveHyperlinks
        {
            get => resolveHyperlinks;
            set
            {
                if (resolveHyperlinks == value)
                    return;

                if (!Enum<HyperlinkResolveMode>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                // when switching back to None, removing previous links
                if (value == HyperlinkResolveMode.None)
                    base.LinkArea = default;
                resolveHyperlinks = value;
                ResetHyperlinkText();
            }
        }

        /// <summary>
        /// Gets or sets the border style of the <see cref="AdvancedLabel"/> panel.
        /// </summary>
        [Category("AdvancedLabel")]
        [Description("Gets or sets the border style of the AdvancedLabel.")]
        [DefaultValue(AdvancedBorderStyle.None)]
        public new AdvancedBorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                if (borderStyle == value)
                    return;

                // setting base border style in cases just for rendering the text into the right position
                borderStyle = value;
                int previousWidth = borderWidth;
                borderWidth = value switch
                {
                    AdvancedBorderStyle.None => 0,
                    AdvancedBorderStyle.FixedSingle or AdvancedBorderStyle.Raised or AdvancedBorderStyle.Sunken => 1,
                    AdvancedBorderStyle.Flat or AdvancedBorderStyle.RaisedHigh or AdvancedBorderStyle.SunkenLow
                        or AdvancedBorderStyle.RaisedFrame or AdvancedBorderStyle.SunkenFrame => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value))
                };

                ResetSizeCache();
                if (OSHelper.IsWindows)
                    InvalidateNC();
                Invalidate();

                if (AutoSize)
                    ResetSize();
            }
        }

        /// <summary>
        /// Gets or sets text of the label. When <see cref="ResolveHyperlinks"/> is not <see cref="HyperlinkResolveMode.None"/>,
        /// hyperlinks in text like the following will be converted to hyperlinks:
        /// <example><c>This is a &lt;a href="http://kgysoft.net"&gt;hyperlink&lt;/a&gt;</c></example>
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("AdvancedLabel")]
        [Description(@"Gets or sets text of the label. When ResolveHyperlinks is set, hyperlinks in text like the following will be converted to hyperlinks:
This is a <a href=""http://kgysoft.net"">hyperlink</a>")]
        [AllowNull]
        public override string Text
        {
            get => base.Text;
            set => RawText = value;
        }

        /// <summary>
        /// Gets or sets raw text of the label. When <see cref="ResolveHyperlinks"/> is not <see cref="HyperlinkResolveMode.None"/>,
        /// value of this property may differ from <see cref="Text"/>.
        /// </summary>
        [Category("AdvancedLabel")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Description("Gets or sets raw text of the label. When ResolveHyperlinks is not HyperlinkResolveModes.None, value of this property may differ from Text.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string? RawText
        {
            get => rawText;
            set
            {
                if (rawText == value)
                    return;

                rawText = value;
                ResetHyperlinkText();
            }
        }

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedLabel"/>.
        /// </summary>
        [Category("AdvancedLabel")]
        [Description("Gets or sets the text rendering quality of the advanced label. Has effect only when FlatStyle is not System.")]
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
                    if (OSHelper.IsWindows)
                        InvalidateNC();
                    ResetSize();
                }
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
        [Category("AdvancedLabel")]
        [Description("Determines the background color when the control is Enabled.")]
        public Color EnabledBackColor
        {
            get => !enabledBackColor.IsEmpty ? enabledBackColor : base.BackColor;
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
        [Category("AdvancedLabel")]
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
        [Category("AdvancedLabel")]
        [Description("Determines the disabled background color.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : base.BackColor;
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
        [Category("AdvancedLabel")]
        [Description("Determines the disabled text color.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : SystemColors.GrayText;
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
        [Category("AdvancedLabel")]
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
        [Category("AdvancedLabel")]
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
        [Category("AdvancedLabel")]
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
        /// Gets or sets whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
#if NET9_0_OR_GREATER
        [SuppressMessage("WinForms Security", "WFO1000:Property does not configure the code serialization for its property content.",
            Justification = "False alarm, inherited from the base. Cannot redefine easily because LinkLabel uses a ShouldSerialize method calling internal members.")] 
#endif
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
        [Category("AdvancedLabel")]
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

        #endregion

        #region Explicitly Implemented Interface Properties

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

        #endregion
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedLabel"/>.
        /// </summary>
        public AdvancedLabel()
        {
            CheckStyles();
            fadingPainter = new FadingPainterInternal(this, Constants.ThemeClassButton); // using button timings for enabling/disabling
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
            base.LinkArea = default;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <inheritdoc />
        public override Size GetPreferredSize(Size proposedSize)
        {
            // Workaround: Immediately after calculating preferred size (e.g. Dock == Top), another request arrives with empty proposedSize, which ruins the constrained result.
            if (proposedSize == Size.Empty && lastProposedSize != Size.Empty && Dock != DockStyle.None)
            {
                proposedSize = lastProposedSize;

                // in design mode further Empty proposedSizes may arrive so clearing only at runtime
                if (!DesignMode)
                    lastProposedSize = Size.Empty;
            }
            else
                lastProposedSize = proposedSize;

            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            TextFormatFlags formatFlags = this.GetFormatFlags();
            bool useGdi = FlatStyle == FlatStyle.System || !UseCompatibleTextRendering;

            Size padding = GetBordersAndPadding();
            Size proposedTextSize = proposedSize - padding;

            // 0 or 1 means unbounded
            if (proposedTextSize.Width <= 1)
                proposedTextSize.Width = Int32.MaxValue;
            if (proposedTextSize.Height <= 1)
                proposedTextSize.Height = Int32.MaxValue;

            using (Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero))
            {
                g.SetTextRenderingQuality(textRenderingQuality, !useGdi);

                if (String.IsNullOrEmpty(base.Text))
                {
                    preferredSize = TextRenderer.MeasureText(g, "0", base.Font);
                    preferredSize.Width = 0;
                }
                else
                {
                    preferredSize = useGdi
                        ? TextRenderer.MeasureText(g, base.Text, base.Font, proposedTextSize, formatFlags)
                        : g.MeasureString(base.Text, base.Font, proposedTextSize, formatFlags.ToStringFormat()).Ceiling();
                }
            }

            preferredSize += padding;
            if (proposedSize.Width > preferredSize.Width)
                preferredSize.Width = proposedSize.Width;
            if (proposedSize.Height > preferredSize.Height)
                preferredSize.Height = proposedSize.Height;

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
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;

            ResetSizeCache();
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);
            }
            catch (NullReferenceException) when (OSHelper.IsMono) // workaround for Mono bug
            {
                // at System.Windows.Forms.LinkLabel.PointInLink (System.Int32 x, System.Int32 y)
                return;
            }

            // If the base class decided to show the ugly hand cursor
            if (OSHelper.IsWindows && OverrideCursor == Cursors.Hand)
            {
                // Show the system hand cursor instead
                OverrideCursor = new Cursor(User32.LoadCursor(IntPtr.Zero, Constants.IDC_HAND));
            }
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                base.OnMouseDown(e);
            }
            catch (NullReferenceException) when (OSHelper.IsMono) // workaround for Mono bug
            {
            }
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);
            }
            catch (NullReferenceException) when (OSHelper.IsMono) // workaround for Mono bug
            {
            }
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
            if (OSHelper.IsMono)
                Invalidate();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
        {
            base.OnLinkClicked(e);
            if (e.Link?.LinkData is string url)
            {
                HyperlinkClickedEventArgs args = new HyperlinkClickedEventArgs(url);
                OnHyperlinkClicked(args);
            }
        }

        /// <summary>
        /// Raises the <see cref="HyperlinkClicked"/> event.
        /// </summary>
        /// <param name="args">A <see cref="HyperlinkClickedEventArgs"/> that contains the event data.</param>
        protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs args)
        {
            if (Events.GetHandler<EventHandler<HyperlinkClickedEventArgs>>(nameof(HyperlinkClicked)) is { } handler)
                handler.Invoke(this, args);
            else
                args.Handled = false;

            if (!args.Handled && AutoHandleUrls)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(args.Hyperlink) { UseShellExecute = true });
                }
                catch (Win32Exception)
                {
                    // link could not be resolved
                }
            }
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
                if (hasPaintError)
                    throw;
                hasPaintError = true;
                lastScale = PointF.Empty;
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
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // if the parent control is rescaling its font due to DPI change, then ignoring the event
            // (we do our scaling in CheckDpiChange or in OnHandleCreated if handle is not created yet)
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
                case Constants.WM_NCCALCSIZE when OSHelper.IsWindows:
                    base.WndProc(ref m);
                    if (m.WParam == IntPtr.Zero || m.WParam == new IntPtr(1))
                        NCHelper.CalcSizeNC(m.LParam, borderWidth);
                    return;

                case Constants.WM_NCPAINT when OSHelper.IsWindows:
                    base.WndProc(ref m);
                    NCHelper.DrawBorderNC(m.HWnd, Size, borderStyle);
                    return;

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

                    if (AutoSize)
                        PerformLayout();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (OSHelper.IsWindows)
                InvalidateNC();
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

        /// <summary>
        /// Paints the specified state of this control, and raises the <see cref="PaintState"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintStateEventArgs"/> that contains the event data.</param>
        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            ControlAppearanceState state = e.State;
            e.Graphics.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);

            try
            {
                if (!state.Visible)
                {
                    this.PaintTransparentBackground(e);
                        return;
                }

                Rectangle backRect = ClientRectangle;
                this.PaintBackground(e, backRect, state.BackColor);

                // drawing image
                Image? image = Image;
                if (image != null)
                {
                    Region oldClip = e.Graphics.Clip;
                    Rectangle imageBounds = CalcImageRenderBounds(image, ClientRectangle, RtlTranslateAlignment(ImageAlign));
                    e.Graphics.IntersectClip(imageBounds);
                    try
                    {
                        DrawImage(e.Graphics, image, ClientRectangle, RtlTranslateAlignment(ImageAlign));
                    }
                    finally
                    {
                        e.Graphics.Clip = oldClip;
                    }
                }

                // When there are links, drawing the text regularly (this does not draw image again, that is in base.OnPaintBackground)
                if (state.Enabled && base.LinkArea.Length != 0)
                {
                    base.OnPaint(e);
                    return;
                }

                Rectangle rect = new Rectangle(ClientRectangle.X + Padding.Left, ClientRectangle.Y + Padding.Top, ClientRectangle.Width - Padding.Horizontal, ClientRectangle.Height - Padding.Vertical);
                TextFormatFlags formatFlags = this.GetFormatFlags();
                if (UseCompatibleTextRendering)
                    e.Graphics.DrawString(state.Text, Font, state.ForeColor.GetBrush(), rect, formatFlags.ToStringFormat());
                else
                    TextRenderer.DrawText(e.Graphics, state.Text, Font, rect, state.ForeColor, image == null ? state.BackColor : Color.Transparent, formatFlags);
            }
            finally
            {
                Events.GetHandler<EventHandler<PaintStateEventArgs>>(nameof(PaintState))?.Invoke(this, e);
                if (!OSHelper.IsWindows && borderStyle != AdvancedBorderStyle.None)
                    e.Graphics.DrawBorder(borderStyle, ClientRectangle);
            }
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

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => CheckStyles();

        #endregion

        #region Private Methods

        private bool ShouldSerializeText() => resolveHyperlinks == HyperlinkResolveMode.None;
        private bool ShouldSerializeRawText() => !ShouldSerializeText();
        private bool ShouldSerializeFont() => font != null;
        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;
        private bool ShouldSerializeLinkArea() => ResolveHyperlinks == HyperlinkResolveMode.None && !LinkArea.IsEmpty;

        private void ResetHyperlinkText()
        {
            // Once the handle is created, for empty text Links.Count changes to 1,
            // which causes that the whole text turns into a hyperlink when setting a new value
            if (resolveHyperlinks != HyperlinkResolveMode.None || IsHandleCreated && String.IsNullOrEmpty(base.Text))
                Links.Clear();
            ResetSizeCache();

            try
            {
                if (String.IsNullOrEmpty(rawText) || resolveHyperlinks == HyperlinkResolveMode.None)
                {
                    base.Text = rawText;
                    return;
                }

                // resolving hrefs
                StringBuilder rest = new StringBuilder(rawText);
                Match matchHref;
                StringBuilder displayText = new StringBuilder();
                List<Link> links = new List<Link>();
                while ((matchHref = rxHref.Match(rest.ToString())).Success)
                {
                    // adding pre-match part to result
                    if (matchHref.Index > 0)
                        displayText.Append(rest.ToString(0, matchHref.Index));

                    Group href = matchHref.Groups["href"];
                    Group caption = matchHref.Groups["caption"];
                    links.Add(new Link(displayText.Length, caption.Length, href.Value));

                    // adding caption of the link
                    displayText.Append(caption.Value);

                    rest.Remove(0, matchHref.Index + matchHref.Length);
                }

                displayText.Append(rest);

                if (resolveHyperlinks == HyperlinkResolveMode.ResolveAll)
                {
                    foreach (Match matchUrl in rxUrl.Matches(displayText.ToString()))
                    {
                        // checking for overlapping
                        if (links.All(l => l.Start > matchUrl.Index + matchUrl.Length
                            || l.Start + l.Length < matchUrl.Index))
                        {
                            links.Add(new Link(matchUrl.Index, matchUrl.Length, matchUrl.Value));
                        }
                    }
                }

                // setting text and links
                base.Text = displayText.ToString();
                foreach (Link link in links)
                {
                    Links.Add(link);
                }
            }
            finally
            {
                if (AutoSize)
                    ResetSize();
            }
        }

        private void ResetSizeCache()
        {
            lastProposedSize = Size.Empty;
            preferredSizeCache.Clear();
        }

        private void ResetSize()
        {
            Debug.Assert(AutoSize, "ResetSize is expected to be called only when AutoSize is true.");
            ResetSizeCache();

            // bug: Otherwise PerformLayout wouldn't work.
            Size = Size.Empty;
            PerformLayout();
        }

        private void CheckStyles()
        {
            if (fadingAnimationsEnabled && FadingPainterInternal.IsSupported)
            {
                // to enable animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (FlatStyle != FlatStyle.System)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private ControlAppearanceState GetAppearance()
        {
            // PUSHBUTTON: Using button timings for enabled/disabled fading animations
            return new ControlAppearanceState((int)BUTTONPARTS.BP_PUSHBUTTON, (int)(Enabled ? PUSHBUTTONSTATES.PBS_NORMAL : PUSHBUTTONSTATES.PBS_DISABLED))
            {
                BackColor = BackColor,
                ForeColor = ForeColor,
                Enabled = Enabled,
                Text = base.Text,
                Visible = Visible,
            };
        }

        private Size GetBordersAndPadding()
        {
            Size size = Padding.Size;
            if (UseCompatibleTextRendering)
            {
                if (BorderStyle != AdvancedBorderStyle.None)
                {
                    size.Height += 6;
                    size.Width += borderWidth << 1;
                    return size;
                }

                size.Height += 3;
                return size;
            }

            if (BorderStyle != AdvancedBorderStyle.None)
                size += new Size(borderWidth << 1, borderWidth << 1);

            return size;
        }

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();

            // The Font check is needed for .NET 6, where WinForms' (bad) auto font scaling may occur without notification
            if ((scale == lastScale && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            if (!lastScale.IsEmpty)
                ResetSizeCache();
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
            bool force = ReferenceEquals(font, value);
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

        private void InvalidateNC()
        {
            if (IsHandleCreated)
                NCHelper.InvalidateNC(Handle);
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
            // system speeds are determined by the painter
            => FadingAnimationDefaultSpeed;

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
            => OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));

        int ISupportsFadingInternal.GetStandardAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo, int defaultSpeed) => defaultSpeed;

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

        #endregion
    }
}
