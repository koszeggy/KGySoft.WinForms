#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedLabel.cs
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
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a label that supports correct auto sizing, different rendering qualites, disabled coloring, advanced border styles, and
    /// is able to automatically resolve hyperlinks like the following example:
    /// <example>
    /// <c>This is a &lt;a href="http://kgysoft.try.hu"&gt;hyperlink&lt;/a&gt;</c>
    /// </example>
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedLabel"/> class offers the following features in addition to <see cref="LinkLabel"/>:
    /// <list type="bullet">
    /// <item><description><see cref="Label.AutoSize"/> property works as expected when label is docked</description></item>
    /// <item><description>Different rendering qualities (see <see cref="TextRenderingQuality"/>) property.</description></item>
    /// <item><description>Advanced border styles.</description></item>
    /// <item><description>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</description></item>
    /// <item><description>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</description></item>
    /// <item><description>Automatic resolving of hyperlinks.</description></item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(AdvancedLabel), "Resources.Toolbox.AdvancedLabel.png")]
    [Description(@"A label that provides the following features in addition to regular Label:
- AutoSize works as expected when label is docked
- Adjustable rendering qualities
- Advanced border styles
- Adjustable colors in disabled state
- Fading animations
- Automatic resolving of hyperlinks")]
    public class AdvancedLabel : LinkLabel, ISupportsDisabledColor, ISupportsFadingInternal
    {
        #region Fields

        #region Static Fields

        private static readonly Regex rxHref = new Regex(@"<a\s+href=""(?<href>.*?)"">(?<caption>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxUrl = new Regex(@"(\w+:\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled);

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;

        private AdvancedBorderStyle borderStyle;
        private int borderWidth;
        private RenderingQuality textRenderingQuality;
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private Size lastProposedSize;
        private Color disabledForeColor;
        private Color disabledBackColor;
        private HyperlinkResolveMode resolveHyperlinks;
        private string? rawText;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a link is clicked.
        /// To handle clicked links automatically, set <see cref="AutoHandleUrls"/>&#160;<see langword="true"/>, and if this event is subsribed, set <see cref="HyperlinkClickedEventArgs.Handled"/>&#160;<see langword="false"/>&#160;in the event handler.
        /// </summary>
        [Description("Occurs when a link is clicked. To handle clicked links automatically, set AutoHandleUrls true, and if this event is subsribed, set HyperlinkClickedEventArgs.Handled false in the event handler.")]
        [Category("AdvancedLabel")]
        public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedLabel")]
        public event EventHandler<PaintStateEventArgs>? PaintState;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether clicked links should be handled automatically or when <see cref="HyperlinkClickedEventArgs.Handled"/> is set to <see langword="false"/>.
        /// <note type="caution">Caution: Setting this property to <see langword="true"/>&#160;may cause security issues. Use only in secure circumstances!</note>
        /// </summary>
        [Category("AdvancedLabel")]
        [DefaultValue(false)]
        [Description("Gets or sets whether clicked links should be handled automatically or when HyperlinkClickedEventArgs.Handled is set to false. Caution: Setting this property to true may cause security issues. Use only in secure circumstances!")]
        public bool AutoHandleUrls { get; set; }

        /// <summary>
        /// Gets or sets whether hyperlinks should be resolved.
        /// </summary>
        /// <remarks>
        /// <para>When value is <see cref="HyperlinkResolveMode.ResolveHrefsOnly"/>, hyperlinks will be resolved only in the following form:
        /// <example><c>This is a &lt;a href="http://kgysoft.try.hu"&gt;hyperlink&lt;/a&gt;</c></example>
        /// </para>
        /// <para>When value is <see cref="HyperlinkResolveMode.ResolveAll"/>, simple inline hyperlinks will be resolved, too.
        /// </para>
        /// </remarks>
        [Category("AdvancedLabel")]
        [DefaultValue(HyperlinkResolveMode.None)]
        [Description(@"Gets or sets whether hyperlinks should be resolved.
When value is ""ResolveHrefsOnly"", hyperlinks will be resolved only in the following form:
This is a <a href=""http://kgysoft.try.hu"">hyperlink</a>
When value is ""ResolveAll"", simple inline hyperlinks will be resolved, too.")]
        public HyperlinkResolveMode ResolveHyperlinks
        {
            get => resolveHyperlinks;
            set
            {
                if (resolveHyperlinks == value)
                    return;

                if (!Enum<HyperlinkResolveMode>.IsDefined(value))
                    throw new ArgumentOutOfRangeException("value");

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
                switch (value)
                {
                    case AdvancedBorderStyle.None:
                        borderWidth = 0;
                        //base.BorderStyle = System.Windows.Forms.BorderStyle.None;
                        break;
                    case AdvancedBorderStyle.FixedSingle:
                    case AdvancedBorderStyle.Raised:
                    case AdvancedBorderStyle.Sunken:
                        borderWidth = 1;
                        //base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        break;
                    case AdvancedBorderStyle.Flat:
                    case AdvancedBorderStyle.RaisedHigh:
                    case AdvancedBorderStyle.SunkenLow:
                    case AdvancedBorderStyle.RaisedFrame:
                    case AdvancedBorderStyle.SunkenFrame:
                        borderWidth = 2;
                        //base.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                }

                ResetSizeCache();
                NCHelper.InvalidateNC(Handle);
                Invalidate();

                if (AutoSize)
                {
                    // bug: Otherwise PerformLayout wouldn't work.
                    Size = Size.Empty;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets text of the label. When <see cref="ResolveHyperlinks"/> is not <see cref="HyperlinkResolveMode.None"/>,
        /// hyperlinks in text like the following will be converted to hyperlinks:
        /// <example><c>This is a &lt;a href="http://kgysoft.try.hu"&gt;hyperlink&lt;/a&gt;</c></example>
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("AdvancedLabel")]
        [Description(@"Gets or sets text of the label. When ResolveHyperlinks is true, hyperlinks in text like the following will be converted to hyperlinks:
This is a <a href=""http://kgysoft.try.hu"">hyperlink</a>")]
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
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Drawing.Font"/> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont"/> property.
        /// </returns>
        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set
            {
                ResetSizeCache();
                base.Font = value;
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
                    NCHelper.InvalidateNC(Handle);

                    // bug: Otherwise PerformLayout wouldn't work.
                    Size = Size.Empty;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedLabel")]
        [Description("Gets or sets disabled fore color.")]
        public Color DisabledForeColor
        {
            get => disabledForeColor != Color.Empty ? disabledForeColor : ControlPaint.DarkDark(BackColor);
            set
            {
                if (disabledForeColor == value)
                    return;

                disabledForeColor = value;
                if (!Enabled)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets disabled back color.
        /// </summary>
        [Category("AdvancedLabel")]
        [Description("Gets or sets disabled back color.")]
        public Color DisabledBackColor
        {
            get => disabledBackColor != Color.Empty ? disabledBackColor : BackColor;
            set
            {
                if (disabledBackColor == value)
                    return;

                disabledBackColor = value;
                if (!Enabled)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
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
        /// Gets or sets the flat style appearance of the button control.
        /// </summary>
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

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedLabel"/>.
        /// </summary>
        public AdvancedLabel()
        {
            LinkClicked += AdvancedLabel_LinkClicked;
            fadingPainter = new FadingPainterInternal(this, "BUTTON"); // using button timings for enabling/disabling
            CheckStyles();
        }

        #endregion

        #region Explicit Disposing

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            LinkClicked -= AdvancedLabel_LinkClicked;

            if (disposing)
                fadingPainter.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <inheritdoc />
        public override Size GetPreferredSize(Size proposedSize)
        {
            // Workaround: Immediately after calculating preferred size (eg. Dock == Top), another request arrives with empty proposedSize, which ruins the constrained result.
            if (proposedSize == Size.Empty && lastProposedSize != Size.Empty && Dock != DockStyle.None)
            {
                proposedSize = lastProposedSize;

                // in design mode further Empty proposedSizes may arrive so clearing only at runtime
                if (!DesignMode)
                    lastProposedSize = Size.Empty;
            }
            else
            {
                lastProposedSize = proposedSize;
            }

            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            TextFormatFlags formatFlags = this.GetFormatFlags();
            bool useGdi = base.FlatStyle == FlatStyle.System || !UseCompatibleTextRendering;

            Size padding = GetBordersAndPadding();
            Size proposedTextSize = proposedSize - padding;

            // 0 or 1 means unbounded
            if (proposedTextSize.Width <= 1)
                proposedTextSize.Width = Int32.MaxValue;
            if (proposedTextSize.Height <= 1)
                proposedTextSize.Height = Int32.MaxValue;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                g.SetTextRenderingQuality(textRenderingQuality, !useGdi);

                if (String.IsNullOrEmpty(base.Text))
                {
                    preferredSize = g.MeasureString("0", base.Font, 0).Ceiling();
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
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // If the base class decided to show the ugly hand cursor
            if (OverrideCursor == Cursors.Hand)
            {
                // Show the system hand cursor instead
                OverrideCursor = new Cursor(User32.LoadCursor(IntPtr.Zero, Constants.IDC_HAND));
            }
        }

        /// <summary>
        /// Raises the <see cref="HyperlinkClicked"/> event.
        /// </summary>
        /// <param name="args">A <see cref="HyperlinkClickedEventArgs"/> that contains the event data.</param>
        protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs args)
        {
            if (HyperlinkClicked != null)
                HyperlinkClicked.Invoke(this, args);
            else
                args.Handled = false;

            if (!args.Handled && AutoHandleUrls)
            {
                try
                {
                    Process.Start(args.Hyperlink);
                }
                catch (Win32Exception)
                {
                    // link could not be resolved
                }
            }
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting flatstyle if needed (in System mode this is in WndProc)
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = base.FlatStyle;
                OnFlatStyleChanged();
                return;
            }

            fadingPainter.State ??= GetAppearance();
            fadingPainter.Paint(e);
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_NCCALCSIZE:
                    if (m.WParam == IntPtr.Zero || m.WParam == new IntPtr(1))
                    {
                        NCHelper.CalcSizeNC(m.LParam, borderWidth);
                    }
                    break;

                case Constants.WM_NCPAINT:
                    NCHelper.DrawBorderNC(m.HWnd, Size, borderStyle);
                    break;

                case Constants.WM_PAINT:
                    // FlatStyle is not overridable property so in case of native rendering reacting for its change here.
                    // (On custom rendering, this is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged();
                    }
                    break;

                //case Constants.WM_SETCURSOR:
                //    //IDC_HAND == 32649
                //    SetCursor(LoadCursor(0, 32649));

                //    //the message has been handled
                //    m.Result = IntPtr.Zero;
                //    return;
            }

            base.WndProc(ref m);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            NCHelper.InvalidateNC(Handle);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
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
                if (!state.Visible || state.BackColor == Color.Transparent)
                {
                    this.PaintTransparentBackground(e);
                    if (!state.Visible)
                        return;
                }

                Rectangle backRect = ClientRectangle;
                if (state.BackColor != Color.Transparent)
                {
                    using (Brush b = new SolidBrush(state.BackColor))
                    {
                        e.Graphics.FillRectangle(b, backRect);
                    }
                }

                if (!state.Visible)
                    return;

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

                // drawing text regularly (this does not draw image again, that is in base.OnPaintBackground)
                if (state.Enabled)
                {
                    base.OnPaint(e);
                    return;
                }

                Rectangle rect = new Rectangle(ClientRectangle.X + Padding.Left, ClientRectangle.Y + Padding.Top, ClientRectangle.Width - Padding.Horizontal, ClientRectangle.Height - Padding.Vertical);
                TextFormatFlags formatFlags = this.GetFormatFlags();
                if (UseCompatibleTextRendering)
                {
                    using (Brush b = new SolidBrush(state.ForeColor))
                    {
                        e.Graphics.DrawString(state.Text, Font, b, rect, formatFlags.ToStringFormat());
                    }
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, state.Text, Font, rect, state.ForeColor, image == null ? state.BackColor : Color.Transparent, formatFlags);
                }
            }
            finally
            {
                if (PaintState != null)
                    PaintState.Invoke(this, e);
            }
        }

        #endregion

        #region Private Methods

        void AdvancedLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link?.LinkData is string url)
            {
                HyperlinkClickedEventArgs args = new HyperlinkClickedEventArgs(url);
                OnHyperlinkClicked(args);
            }
        }

        private bool ShouldSerializeText()
        {
            return resolveHyperlinks == HyperlinkResolveMode.None;
        }

        private bool ShouldSerializeRawText()
        {
            return !ShouldSerializeText();
        }

        private bool ShouldSerializeDisabledBackColor()
        {
            return disabledBackColor != Color.Empty;
        }

        private bool ShouldSerializeDisabledForeColor()
        {
            return disabledForeColor != Color.Empty;
        }

        private void ResetHyperlinkText()
        {
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
                    PerformLayout();
            }
        }

        private void ResetSizeCache()
        {
            lastProposedSize = Size.Empty;
            preferredSizeCache.Clear();
        }

        private void OnFlatStyleChanged()
        {
            ResetSizeCache();
            CheckStyles();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void CheckStyles()
        {
            if (fadingAnimationsEnabled && FadingPainterInternal.IsSupported)
            {
                // to enabling animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (base.FlatStyle != FlatStyle.System)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private ControlAppearanceState GetAppearance()
        {
            return new ControlAppearanceState((int)BUTTONPARTS.BP_PUSHBUTTON, (int)(Enabled ? PUSHBUTTONSTATES.PBS_NORMAL : PUSHBUTTONSTATES.PBS_DISABLED))
            {
                BackColor = Enabled ? BackColor : DisabledBackColor,
                ForeColor = Enabled ? ForeColor : DisabledForeColor,
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

            size += SizeFromClientSize(Size.Empty);

            if (BorderStyle != AdvancedBorderStyle.None)
            {
                size += new Size(borderWidth << 1, borderWidth << 1);
            }

            return size;
        }

        #endregion

        #endregion

        #region ISupportsFading Members

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("AdvancedLabel")]
        [DefaultValue(true)]
        [Description("Gets or sets whether fading animations are enabled for the control. Animations work in Windows Vista and above, with non-classic themes.")]
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
                    throw new ArgumentOutOfRangeException("value");

                fadingOptions = value;

                // storing invisible state so when control turns visible it will fading when enabled
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
                    throw new ArgumentOutOfRangeException("value");

                fadingAnimationDefaultSpeed = value;
            }
        }

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
        {
            // system speeds are determined by the painter
            return FadingAnimationDefaultSpeed;
        }

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
        {
            OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));
        }

        #endregion
    }
}
