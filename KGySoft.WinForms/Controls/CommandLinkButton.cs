#region Used namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;
using ContentAlignment = System.Drawing.ContentAlignment;

#endregion

namespace KGySoft.WinForms.Controls
{
    using Resources = Properties.Resources;

    /// <summary>
    /// Represents a command link button. Works also in compatibility mode in a pre-Vista Windows.
    /// To force system rendering set <see cref="FlatStyle"/> to <see cref="System.Windows.Forms.FlatStyle.System"/> (only in case of Windows Vista and above).
    /// </summary>
    [ToolboxBitmap(typeof(CommandLinkButton), "Resources.Toolbox.CommandLinkButton.png")]
    [Description("Vista-like CommandLink button that works also in compatibility mode. In Vista and above you may set FlatStyle to System to render the button by the Windows.")]
    public class CommandLinkButton : Button, ISupportsDisabledColor, ISupportsFadingInternal
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultForeColor = Color.FromArgb(21, 28, 85); // + focusedColor
        private static readonly Color defaultDisabledColor = Color.FromArgb(126, 133, 156);
        private static readonly Color defaultHoveredColor = Color.FromArgb(7, 74, 229);
        private static readonly Color defaultPressedColor = Color.FromArgb(6, 32, 115);
        private static readonly Color pressedBackColor = Color.FromArgb(96, 230, 230, 230);
        private static readonly Color pressedEdgeColor = Color.FromArgb(96, 160, 160, 160);
        private static readonly Color hoveredBackColor = Color.FromArgb(96, 222, 222, 222);
        private static readonly Color selectedFrameColor = Color.FromArgb(64, 0, 204, 255);
        private static readonly Color selectedFrameColorAlternative = Color.FromArgb(192, 0, 204, 255);

        private static Bitmap noGlyph;
        private static Image securityShield;
        private static Image securityShieldGray;

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);

        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool isElevated;
        private bool useDefaultGlyph = true;
        private bool isImageUpToDate = true;
        private bool? isThemed;
        private string description;

        private Brush pressedBrush;
        private Brush hoveredBrush;
        private Pen hoveredFrameOuterPen;
        private Pen hoveredFrameInnerPen;
        private Pen pressedFramePen;
        private GraphicsPath outerBorder;
        private GraphicsPath innerBorder;
        private GraphicsPath selectionBorder;
        private Font themedFontLarge;
        private Font themedFontSmall;
        private Font descriptionFont;
        private Image? currentImage;
        private Image? disabledImage;

        private Color foreColor;
        private Color descriptionColor;
        private Color highlightTextColor;
        private Color highlightDescriptionColor;
        private Color pressedTextColor;
        private Color pressedDescriptionColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle reportedFlatStyle = FlatStyle.Standard;
        private ContentAlignment lastImageAlign;

        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingPainterInternal fadingPainter;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private Timer defaultAnimationTimer;
        private bool isAlternativeDefaultImage;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("CommandLinkButton")]
        public event EventHandler<PaintStateEventArgs> PaintState;

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

        private static Image SecurityShield
        {
            get
            {
                if (securityShield != null)
                    return securityShield;

                securityShield = Icons.SecurityShield.ExtractNearestBitmap(new Size(16, 16), PixelFormat.Format32bppArgb); // TODO: ToMultiResBitmap, and handle GetPreferredSize correctly
                return securityShield;
            }
        }

        private static Image SecurityShieldGray
        {
            get
            {
                if (securityShieldGray != null)
                    return securityShieldGray;

                securityShieldGray = SecurityShield.ToGrayscale();
                return securityShieldGray;
            }
        }

        private static bool IsNativelySupported
        {
            get { return WindowsUtils.IsVistaOrLater && WindowsUtils.IsComCtlV6Available; }
        }

        #endregion

        #region Instance Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether an elevated shield icon should be displayed.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets whether an elevated shield icon should be displayed.")]
        [DefaultValue(false)]
        public bool IsElevated
        {
            get { return isElevated; }
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
            get { return useDefaultGlyph; }
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
        [DefaultValue(true)] // This is the only reson for redefining.
        public new bool AutoEllipsis
        {
            get { return base.AutoEllipsis; }
            set { base.AutoEllipsis = value; }
        }

        /// <summary>
        /// Gets or sets whether the command link button automatically resizes itself to its content.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets whether the command link button automatically resizes itself to its content.")]
        [DefaultValue(true)]
        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                if (base.AutoSize == value)
                    return;

                // turning on ButtonBase.AutoSize would turn off AutoEllipsis
                bool autoEllipis = base.AutoEllipsis;
                ResetSizeCache();
                base.AutoSize = value;
                base.AutoEllipsis = autoEllipis;
            }
        }

        /// <summary>
        /// Gets or sets the mode by which the <see cref="CommandLinkButton"/> automatically resizes itself.
        /// </summary>
        [DefaultValue(AutoSizeMode.GrowAndShrink)] // "overridden" only because of this.
        public new AutoSizeMode AutoSizeMode
        {
            get { return base.AutoSizeMode; }
            set { base.AutoSizeMode = value; }
        }

        /// <summary>
        /// Gets or sets text of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets text of the command link button.")]
        public override string Text
        {
            get { return base.Text; }
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
            get { return base.TextAlign; }
            set { base.TextAlign = value; }
        }

        /// <summary>
        /// Gets or sets the alignment of the image on the command link button control.
        /// </summary>
        [DefaultValue(ContentAlignment.TopLeft)] // "overridden" only because of default value
        [Description("Gets or sets the alignment of the image on the command link button control. Has effect only when FlatStyle is not System.")]
        public new ContentAlignment ImageAlign
        {
            get { return base.ImageAlign; }
            set { base.ImageAlign = value; }
        }

        /// <summary>
        /// Gets or sets description text for the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets description text for the command link button.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Description
        {
            get { return description; }
            set
            {
                if (description == value)
                    return;

                description = value;
                ResetSizeCache();
                if (IsNativeRendering)
                {
                    User32.SendMessage(Handle, Constants.BCM_SETNOTE, IntPtr.Zero, description);
                }

                Invalidate();
                if (base.AutoSize)
                    PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the font of the text displayed by the control. Has effect only when FlatStyle is not System.")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                ResetSizeCache();
                base.Font = value ?? DefaultTextFont;
            }
        }

        /// <summary>
        /// Gets or sets the font of the description displayed by the control.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the font of the description displayed by the control. Has effect only when FlatStyle is not System.")]
        public Font DescriptionFont
        {
            get { return descriptionFont; }
            set
            {
                ResetSizeCache();
                descriptionFont = value ?? DefaultDescriptionFont;
                Invalidate();
                if (AutoSize)
                    PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets the text color of the command link button.
        /// </summary>
        [Description("Gets or sets the text color of the command link button. Has effect only when FlatStyle is not System.")]
        public override Color ForeColor
        {
            get
            {
                if (foreColor != Color.Empty)
                    return foreColor;

                return !IsThemed ? base.ForeColor : defaultForeColor;
            }
            set
            {
                if (foreColor == value)
                    return;

                base.ForeColor = foreColor = value;
                Invalidate(); // in Windows XP invalidating is explicitly needed
            }
        }

        /// <summary>
        /// Gets or sets the description color of the command link button.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets the description color of the command link button. Has effect only when FlatStyle is not System.")]
        public Color DescriptionColor
        {
            get
            {
                if (descriptionColor != Color.Empty)
                    return descriptionColor;

                return !IsThemed ? base.ForeColor : defaultForeColor;
            }
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
            get
            {
                if (highlightTextColor != Color.Empty)
                    return highlightTextColor;

                return !IsThemed ? base.ForeColor : defaultHoveredColor;
            }
            set
            {
                if (highlightTextColor == value)
                    return;

                highlightTextColor = value;
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
            get
            {
                if (highlightDescriptionColor != Color.Empty)
                    return highlightDescriptionColor;

                return !IsThemed ? base.ForeColor : defaultHoveredColor;
            }
            set
            {
                if (highlightDescriptionColor == value)
                    return;

                highlightDescriptionColor = value;
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
            get
            {
                if (pressedTextColor != Color.Empty)
                    return pressedTextColor;

                return !IsThemed ? base.ForeColor : defaultPressedColor;

            }
            set
            {
                if (pressedTextColor == value)
                    return;

                pressedTextColor = value;
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
            get
            {
                if (pressedDescriptionColor != Color.Empty)
                    return pressedDescriptionColor;

                return !IsThemed ? base.ForeColor : defaultPressedColor;
            }
            set
            {
                if (pressedDescriptionColor == value)
                    return;

                pressedDescriptionColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("CommandLinkButton")]
        [Description("Gets or sets disabled fore color. Has effect only when FlatStyle is not System.")]
        public Color DisabledForeColor
        {
            get
            {
                if (disabledForeColor != Color.Empty)
                    return disabledForeColor;

                if (!IsThemed)
                {
                    return ControlPaint.DarkDark(BackColor);
                }

                return defaultDisabledColor;
            }
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
        [Category("CommandLinkButton")]
        [Description("Gets or sets disabled back color. Has effect only when FlatStyle is Popup or Flat, or when visual styles are not enabled and FlatStyle is Standard.")]
        public Color DisabledBackColor
        {
            get { return disabledBackColor != Color.Empty ? disabledBackColor : BackColor; }
            set
            {
                if (value == disabledBackColor)
                    return;

                disabledBackColor = value;
                FreeBrushes();
                if (!Enabled)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
        public new bool UseCompatibleTextRendering
        {
            get { return base.UseCompatibleTextRendering; }
            set
            {
                ResetSizeCache();
                base.UseCompatibleTextRendering = value;
            }
        }

        /// <summary>
        /// Gets or sets the flat style state of the button control.
        /// </summary>
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get { return reportedFlatStyle; }
            set
            {
                if (reportedFlatStyle == value && base.FlatStyle == value && lastFlatStyle == value)
                    return;

                bool recreateHandle = IsNativelySupported &&
                    ((base.FlatStyle == FlatStyle.System && value != FlatStyle.System) || (base.FlatStyle != FlatStyle.System && value == FlatStyle.System));
                base.FlatStyle = lastFlatStyle = reportedFlatStyle = value;
                OnFlatStyleChanged(false, recreateHandle);
            }
        }

        /// <summary>
        /// Gets or sets the image that is displayed on the button control.
        /// </summary>
        public new Image Image // it is also detected when base.Image changes but reacting onto that in OnPaint has a performance cost
        {
            get { return base.Image; }
            set
            {
                base.Image = value;
                isImageUpToDate = false;
                CheckImage();
            }
        }

        #endregion

        #region Protected Properties

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

        protected override Size DefaultSize
        {
            get
            {
                return new Size(160, 41);
            }
        }

        #endregion

        #region Private Properties

        private bool IsNativeRendering
        {
            get { return base.FlatStyle == FlatStyle.System && IsNativelySupported; }
        }

        private bool IsCustomRendering
        {
            get { return base.FlatStyle != FlatStyle.System; }
        }

        private bool IsThemed
        {
            get
            {
                if (isThemed.HasValue)
                    return isThemed.Value;

                isThemed = Application.RenderWithVisualStyles;
                return isThemed.Value;
            }
        }

        private Font DefaultTextFont
        {
            get
            {
                if (!IsThemed)
                {
                    return new Font(SystemFonts.DialogFont, FontStyle.Bold);
                }

                if (themedFontLarge != null)
                    return themedFontLarge;

                themedFontLarge = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
                if (themedFontLarge.Name != "Segoe UI")
                {
                    themedFontLarge.Dispose();
                    themedFontLarge = new Font("MS Shell Dlg 2", 12f, FontStyle.Regular, GraphicsUnit.Point);
                }

                return themedFontLarge;
            }
        }

        private Font DefaultDescriptionFont
        {
            get
            {
                if (!IsThemed)
                {
                    return SystemFonts.DialogFont;
                }

                if (themedFontSmall != null)
                    return themedFontSmall;

                themedFontSmall = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
                if (themedFontSmall.Name != "Segoe UI")
                {
                    themedFontSmall.Dispose();
                    themedFontSmall = new Font("MS Shell Dlg 2", 9f, FontStyle.Regular, GraphicsUnit.Point);
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

                // classic style
                if (base.FlatStyle == FlatStyle.Popup || (base.FlatStyle == FlatStyle.Standard && !IsThemed))
                {
                    pressedBrush = new SolidBrush(SystemColors.Control);
                    return pressedBrush;
                }

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
                        Factors = new float[] { 0f, 1f, 1f, 0.5f },
                        Positions = new float[] { 0f, p1, p2, 1f }
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

                // classic style
                if (base.FlatStyle == FlatStyle.Popup || (base.FlatStyle == FlatStyle.Standard && !IsThemed))
                {
                    hoveredBrush = new SolidBrush(SystemColors.Control);
                    return hoveredBrush;
                }

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
                        Factors = new float[] { 0f, 0.8f, 0.8f, 1f },
                        Positions = new float[] { 0f, p1, p2, 1f }
                    }
                };
                return hoveredBrush;
            }
        }

        private Pen HoveredFrameOuterPen
        {
            get
            {
                if (hoveredFrameOuterPen != null)
                    return hoveredFrameOuterPen;

                // themed mode
                return hoveredFrameOuterPen = new Pen(Color.FromArgb(128, 160, 160, 160), 1f);
            }
        }

        private Pen HoveredFrameInnerPen
        {
            get
            {
                if (hoveredFrameInnerPen != null)
                    return hoveredFrameInnerPen;

                // themed mode
                return hoveredFrameInnerPen = new Pen(Color.FromArgb(128, 255, 255, 255), 1f);
            }
        }

        private Pen PressedFramePen
        {
            get
            {
                if (pressedFramePen != null)
                    return pressedFramePen;

                // themed mode
                return pressedFramePen = new Pen(Color.FromArgb(128, 128, 128, 128), 1f);
            }
        }

        private GraphicsPath OuterBorder
        {
            get
            {
                if (outerBorder != null)
                    return outerBorder;

                return outerBorder = Accessors.CreateRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 3);
            }
        }

        private GraphicsPath InnerBorder
        {
            get
            {
                if (innerBorder != null)
                    return innerBorder;

                return innerBorder = Accessors.CreateRoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 2);
            }
        }

        private GraphicsPath SelectionBorder
        {
            get
            {
                if (selectionBorder != null)
                    return selectionBorder;

                // classic style
                if (base.FlatStyle == FlatStyle.Popup || (base.FlatStyle == FlatStyle.Standard && !IsThemed))
                {
                    selectionBorder = new GraphicsPath();
                    selectionBorder.AddRectangle(new Rectangle(0, 0, Width - 1, Height - 1));
                    return selectionBorder;
                }

                // themed/flat selection
                return selectionBorder = Accessors.CreateRoundedRectangle(new Rectangle(1, 0, Width - 3, Height - 1), 3);
            }
        }

        private Size BordersAndPadding
        {
            get { return new Size(HorizontalPadding, VerticalPadding); }
        }

        private int HorizontalPadding
        {
            get { return (HorizontalBasePadding << 1) + ImagePadding; }
        }

        private int HorizontalBasePadding
        {
            get { return base.FlatStyle == FlatStyle.Standard && !IsThemed || base.FlatStyle == FlatStyle.Popup ? 3 : 2; }
        }

        private int VerticalPadding
        {
            get
            {
                return VerticalBasePadding << 1;
            }
        }

        private int VerticalBasePadding
        {
            get { return base.FlatStyle == FlatStyle.Standard && !IsThemed || base.FlatStyle == FlatStyle.Popup ? 6 : 10; }
        }

        private int ImagePadding
        {
            get
            {
                if (base.Image == null && !isElevated && !useDefaultGlyph
                    || ImageAlign.AnyCenter())
                {
                    return 7;
                }

                if (base.Image != null)
                    return base.Image.Width + 6;

                if (isElevated)
                    return 22;

                if (useDefaultGlyph)
                    return base.FlatStyle == FlatStyle.Standard && !IsThemed || base.FlatStyle == FlatStyle.Flat ? 23 : 26;

                return 7;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Construction and Destruction

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
            ResetTheme();
            fadingPainter = new FadingPainterInternal(this, "BUTTON");
        }

        #endregion

        #region Explicit Disposing

        protected override void Dispose(bool disposing)
        {
            descriptionFont = null; // disposed by owner, if needed

            if (disposing)
            {
                FreeBrushes();
                FreePens();
                FreeRegions();

                if (fadingPainter != null)
                {
                    fadingPainter.Dispose();
                    fadingPainter = null;
                }

                if (themedFontLarge != null)
                {
                    themedFontLarge.Dispose();
                    themedFontLarge = null;
                }

                if (themedFontSmall != null)
                {
                    themedFontSmall.Dispose();
                    themedFontSmall = null;
                }

                currentImage = null;
                if (disabledImage != null)
                {
                    disabledImage.Dispose();
                    disabledImage = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | proposedSize.Width, out preferredSize))
            {
                return preferredSize;
            }

            if (IsNativeRendering)
            {
                SIZE s = new SIZE(
                    proposedSize.Width == 0 ? Int32.MaxValue : proposedSize.Width,
                    proposedSize.Height == 0 ? Int32.MaxValue : proposedSize.Height);

                string origText = base.Text;
                if (String.IsNullOrEmpty(origText))
                    base.Text = @" ";

                IntPtr ps = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SIZE)));
                Marshal.StructureToPtr(s, ps, false);
                try
                {
                    // getting the ideal native size from the Windows
                    User32.SendMessage(Handle, Constants.BCM_GETIDEALSIZE, IntPtr.Zero, ps);
                    s = (SIZE)Marshal.PtrToStructure(ps, typeof(SIZE));
                }
                finally
                {
                    Marshal.DestroyStructure(ps, typeof(SIZE));
                    Marshal.FreeHGlobal(ps);
                }

                if (String.IsNullOrEmpty(origText))
                    base.Text = origText;

                preferredSize = s.ToSize();
                preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
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

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                bool gdiPlusTextRendering = UseCompatibleTextRendering;
                g.SetQuality();

                Size textSize = Size.Empty;
                StringFormat sf = gdiPlusTextRendering ? formatFlags.ToStringFormat() : null;
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
                       ? g.MeasureString(description, descriptionFont, proposedTextSize, sf).Ceiling()
                       : TextRenderer.MeasureText(g, description, descriptionFont, proposedTextSize, formatFlags);
                }

                preferredSize = new Size(Math.Max(textSize.Width, descSize.Width), textSize.Height + (descSize.Height > 0 ? descSize.Height + 1 : 0)) + padding;
                if (!IsThemed && base.FlatStyle == FlatStyle.Standard || base.FlatStyle == FlatStyle.Popup)
                    preferredSize.Height++;

                // HorizonatalPadding already contains image width. Height is calculated here.
                int preferredImageHeight = 0;
                if (base.Image != null)
                    preferredImageHeight = base.Image.Height + VerticalPadding;
                else if (isElevated)
                    preferredImageHeight = 16 + VerticalPadding;
                else if (useDefaultGlyph)
                    preferredImageHeight = 17 + VerticalPadding;

                if (preferredImageHeight > preferredSize.Height)
                    preferredSize.Height = preferredImageHeight;

                preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
                return preferredSize;
            }

        }

        #endregion

        #region Protected Methods

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            isThemed = null;
            ResetTheme();
            OnFlatStyleChanged(false, false);
            CheckStyles();
        }

        protected override void WndProc(ref Message m)
        {
            if (base.FlatStyle != FlatStyle.System && WindowsUtils.IsVistaOrLater)
            {
                // Problem: When toggling Enabled, a GETTEXT arrives. The base.WndProc returns the Text only, so description flickers (invisible for a moment)
                // Problem 2: When including description like below, the description part will be so as large as the text
                // Fix: See CreateParams
                //switch (m.Msg)
                //{
                //    case Constants.WM_GETTEXT:
                //    case Constants.WM_GETTEXTLENGTH:
                //        string text = Text;
                //        int length = text.Length;
                //        if (!String.IsNullOrEmpty(description))
                //        {
                //            length += Environment.NewLine.Length + description.Length;
                //        }

                //        m.Result = new IntPtr(length);
                //        if (m.Msg == Constants.WM_GETTEXT)
                //        {
                //            if (!String.IsNullOrEmpty(description))
                //            {
                //                text += Environment.NewLine + description;
                //            }

                //            Marshal.Copy(text.ToCharArray(), 0, m.LParam, text.Length);
                //        }

                //        return;

                //    default:
                base.WndProc(ref m);
                return;
                //}
            }

            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    // Image and FlatStyle are not overridable properties so in case of native rendering reacting their change here.
                    // (On custom rendering, image change is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        bool recreateHandle = (base.FlatStyle == FlatStyle.System && lastFlatStyle != FlatStyle.System)
                            || (base.FlatStyle != FlatStyle.System && lastFlatStyle == FlatStyle.System);
                        lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged(true, recreateHandle);
                    }

                    if (CheckImage() && AutoSize)
                        PerformLayout();

                    base.WndProc(ref m);
                    return;
            }

            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CheckStyles();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting flatstyle if needed (in System mode this is in WndProc)
            bool invalidated = false;
            if (base.FlatStyle != lastFlatStyle)
            {
                bool recreateHandle = (base.FlatStyle == FlatStyle.System && lastFlatStyle != FlatStyle.System)
                    || (base.FlatStyle != FlatStyle.System && lastFlatStyle == FlatStyle.System);
                lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                OnFlatStyleChanged(true, recreateHandle);
                invalidated = true;
            }

            if (CheckImage() && AutoSize)
            {
                PerformLayout();
                invalidated = true;
            }

            CheckDefaultAnimation();

            // in this case new paint will be triggered
            if (invalidated)
                return;

            if (fadingPainter.State == null)
                fadingPainter.State = GetAppearance();

            fadingPainter.Paint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            isMouseDown = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if (isMouseDown)
                isPressed = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

            base.OnMouseMove(mevent);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && !isPressed)
            {
                isPressed = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;
            }

            base.OnKeyUp(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            CheckDefaultAnimation();
            base.OnVisibleChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            isMouseDown = false;
            FreeBrushes();
            base.OnEnabledChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            FreeBrushes();
            FreeRegions();
            ResetSizeCache();
            base.OnSizeChanged(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnPaddingChanged(e);
        }

        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            e.Graphics.SetQuality();

            if (!e.State.Visible)
            {
                this.PaintTransparentBackground(e);
                return;
            }

            // ButtonBase.OnPaint:
            if (AutoEllipsis)
            {
                int preferredHeight = GetPreferredSize(new Size(Width, 0)).Height;
                this.SetShowToolTip(Height < preferredHeight);
            }
            else
            {
                this.SetShowToolTip(false);
            }

            if (GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                DoPaint(e);
            }

            // Raising PaintState
            if (PaintState != null)
                PaintState.Invoke(this, e);

            // Control.OnPaint:
            PaintEventHandler handler = (PaintEventHandler)Events[Accessors.PaintEvent];
            if (handler != null)
            {
                handler(this, e);
            }
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
                BackColor = Enabled ? BackColor : DisabledBackColor,
                ForeColor = Enabled ? ForeColor : DisabledForeColor,
                Enabled = Enabled,
                Hovered = isHovered,
                Pressed = isPressed,
                IsDefault = IsDefault,
                Text = base.Text,
                Visible = Visible,
                CustomState = Description
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
            if (fadingAnimationsEnabled && FadingPainterInternal.IsSupported)
            {
                // to enabling animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, base.FlatStyle != FlatStyle.System);
        }

        private void CheckDefaultAnimation()
        {
            if (!WindowsUtils.IsVistaOrLater)
                return;

            bool enabled = base.FlatStyle == FlatStyle.Standard && !isPressed && !isHovered && IsDefault && Application.RenderWithVisualStyles;

            if (enabled && (defaultAnimationTimer == null || !defaultAnimationTimer.Enabled))
            {
                if (defaultAnimationTimer == null)
                {
                    defaultAnimationTimer = new Timer();
                    IntPtr hTheme = UxTheme.OpenThemeData(Handle, "BUTTON");
                    int duration;
                    if (UxTheme.GetThemeTransitionDuration(hTheme, (int)BUTTONPARTS.BP_COMMANDLINK, (int)COMMANDLINKSTATES.CMDLS_DEFAULTED, (int)COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING, Constants.TMT_TRANSITIONDURATIONS, out duration) == 0)
                        defaultAnimationTimer.Interval = duration;
                    else
                        defaultAnimationTimer.Interval = 1000;
                    defaultAnimationTimer.Tick += new EventHandler(defaultAnimationTimer_Tick);
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

            if (recreateHandle)
                RecreateHandle();
            CheckDefaultAnimation();

            // adjusting description
            if (IsNativeRendering)
                User32.SendMessage(Handle, Constants.BCM_SETNOTE, IntPtr.Zero, description);

            isImageUpToDate = false;
            if (!ignoreCheckImage)
                CheckImage();

            ResetSizeCache();
            FreeBrushes();
            FreePens();
            FreeRegions();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void ResetSizeCache()
        {
            preferredSizeCache.Clear();
        }

        private void DoPaint(PaintStateEventArgs e)
        {
            //// DEBUG: render to image
            //Size sizeImage = ClientSize;
            //Bitmap image = new Bitmap(sizeImage.Width, sizeImage.Height, e.Graphics);
            //Graphics graphicsOrig = e.Graphics;
            //Graphics graphicsImage = Graphics.FromImage(image);
            //e = new PaintStateEventArgs(graphicsImage, e.ClipRectangle, e.State);

            // Choosing image
            Image img = base.Image;

            ControlAppearanceState state = e.State;
            if (img != null && !state.Enabled)
                img = disabledImage ??= img.ToGrayscale();

            bool useTheming = IsThemed && base.FlatStyle == FlatStyle.Standard;

            // setting colors
            Color textColor = state.ForeColor;
            Color descColor = DescriptionColor;
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
            else
            {
                if (!state.Enabled)
                {
                    if (state.ForeColor != Color.Empty)
                        textColor = descColor = state.ForeColor;
                    else
                    {
                        if (useTheming)
                            textColor = descColor = defaultDisabledColor;
                        else
                            textColor = descColor = SystemColors.GrayText;
                    }
                }
            }

            bool gdiPlusTextRendering = UseCompatibleTextRendering;
            e.Graphics.SetQuality();

            // painting background and image
            Padding imageMargin;
            if (useTheming)
                PaintThemedAppearance(e, img, out imageMargin);
            else if (base.FlatStyle == FlatStyle.Flat)
                PaintFlatAppearance(e, img, out imageMargin);
            else
                PaintClassicAppearance(e, img, out imageMargin);

            // drawing text
            TextFormatFlags formatFlags = this.GetFormatFlags();
            StringFormat sf = gdiPlusTextRendering ? formatFlags.ToStringFormat() : null;

            Size proposedSize = Size - BordersAndPadding;
            Size textSize = Size.Empty;
            if (!String.IsNullOrEmpty(state.Text))
                textSize = gdiPlusTextRendering
                    ? e.Graphics.MeasureString(state.Text, Font, proposedSize, sf).ToSize()
                    : TextRenderer.MeasureText(e.Graphics, state.Text, Font, proposedSize, formatFlags);

            Size descSize = Size.Empty;
            if (!String.IsNullOrEmpty(description) && textSize.Height < proposedSize.Height)
            {
                Size size = new Size(proposedSize.Width, proposedSize.Height - textSize.Height);
                descSize = gdiPlusTextRendering
                    ? e.Graphics.MeasureString(description, descriptionFont, size, sf).ToSize()
                    : TextRenderer.MeasureText(e.Graphics, description, descriptionFont, size, formatFlags);
            }

            Size combinedSize = new Size(proposedSize.Width, Math.Min(proposedSize.Height, textSize.Height + descSize.Height));
            int verticalOffset = 0;
            if ((formatFlags & TextFormatFlags.Bottom) != 0)
                verticalOffset = Math.Max(proposedSize.Height - combinedSize.Height, 0);
            else if (((formatFlags & TextFormatFlags.VerticalCenter) != 0))
                verticalOffset = Math.Max(proposedSize.Height / 2 - combinedSize.Height / 2, 0);

            int imagePadding = RtlTranslateContent(ImageAlign).AnyLeft() ? ImagePadding : 0;
            if (!String.IsNullOrEmpty(Text))
            {
                Rectangle rectangle = new Rectangle(imageMargin.Left + imagePadding + (useTheming ? 1 : 0), imageMargin.Top + verticalOffset, proposedSize.Width, Math.Min(textSize.Height, proposedSize.Height));
                if (gdiPlusTextRendering)
                {
                    using Brush b = new SolidBrush(textColor);
                    e.Graphics.DrawString(state.Text, Font, b, rectangle, sf);
                }
                else
                    TextRenderer.DrawText(e.Graphics, state.Text, Font, rectangle, textColor, formatFlags);
            }

            if (!String.IsNullOrEmpty(description) && proposedSize.Height > textSize.Height)
            {
                Rectangle rectangle = new Rectangle(imageMargin.Left + imagePadding + (useTheming ? 2 : 0), imageMargin.Top + textSize.Height + verticalOffset + (useTheming ? 1 : 2), proposedSize.Width, Math.Min(descSize.Height, proposedSize.Height - textSize.Height));
                if (gdiPlusTextRendering)
                {
                    using Brush b = new SolidBrush(descColor);
                    e.Graphics.DrawString(description, descriptionFont, b, rectangle, sf);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, description, descriptionFont, rectangle, descColor, formatFlags);
                }
            }

            //// DEBUG: render to image
            //graphicsImage.Dispose();
            //graphicsOrig.DrawImage(image, ClientRectangle);
            //image.Save(String.Format(@"d:\temp\{0}{1}.png", DateTime.UtcNow.ToFileTime(), e.State.ToString().Replace(":","=")), System.Drawing.Imaging.ImageFormat.Png);
            //image.Dispose();
        }

        private void PaintThemedAppearance(PaintStateEventArgs e, Image? image, out Padding imageMargin)
        {
            ControlAppearanceState state = e.State;
            Rectangle backRect = new Rectangle(ClientRectangle.X - 1, ClientRectangle.Y - 1, ClientRectangle.Width + 1, ClientRectangle.Height + 1);

            // painting the background (underlying part of the parent control)
            if (Parent != null)
            {
                this.PaintTransparentBackground(e);
            }
            else
            {
                using Brush b = new SolidBrush(state.BackColor);
                e.Graphics.FillRectangle(b, backRect);
            }

            // Native rendering
            if (WindowsUtils.IsVistaOrLater)
            {
                // background
                VisualStyleRenderer renderer = new VisualStyleRenderer("BUTTON", state.SystemPartId, state.SystemStateId);
                renderer.DrawBackground(e.Graphics, ClientRectangle);

                // image
                if (isElevated || image != null)
                    PaintImage(e, image, out imageMargin);
                else if (useDefaultGlyph)
                {
                    renderer = new VisualStyleRenderer("BUTTON", (int)BUTTONPARTS.BP_COMMANDLINKGLYPH, state.SystemStateId);
                    renderer.DrawBackground(e.Graphics, GetImageBounds(state, renderer.GetPartSize(e.Graphics, ThemeSizeType.Draw), true, out imageMargin));
                }
                else
                    imageMargin = Padding.Empty;
            }
            // Compatibility rendering
            else
            {
                if (state.Pressed)
                {
                    e.Graphics.FillRectangle(
                        PressedBrush,
                        new Rectangle(
                            ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1));
                    e.Graphics.DrawPath(PressedFramePen, OuterBorder);
                }
                else if (state.Hovered)
                {
                    e.Graphics.FillRectangle(HoveredBrush, new Rectangle(1, 1, Width - 2, Height - 2));
                    e.Graphics.DrawPath(HoveredFrameInnerPen, InnerBorder);
                    e.Graphics.DrawPath(HoveredFrameOuterPen, OuterBorder);
                }
                else // normal state
                {
                    // no drawing needed in normal state unless if focused or default
                    if (state.Enabled && (Focused || state.IsDefault))
                    {
                        using (Pen selectedFramePen = new Pen(!FadingPainterInternal.IsSupported || state.SystemStateId == (int)COMMANDLINKSTATES.CMDLS_DEFAULTED_ANIMATING
                            ? selectedFrameColorAlternative
                            : selectedFrameColor))
                        {
                            e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                        }
                    }
                }

                PaintImage(e, image, out imageMargin);
            }

            if (state.Enabled && Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(backRect.X + 4, backRect.Y + 4, backRect.Width - 7, backRect.Height - 7), state.ForeColor, state.BackColor);
        }

        private void PaintClassicAppearance(PaintStateEventArgs e, Image? image, out Padding imageMargin)
        {
            ControlAppearanceState state = e.State;
            Rectangle backRect = new Rectangle(ClientRectangle.X - 1, ClientRectangle.Y - 1, ClientRectangle.Width + 1, ClientRectangle.Height + 1);

            // Background
            using (Pen selectedFramePen = new Pen(SystemColors.ControlText, 1f))
            {
                if (state.Pressed)
                {
                    e.Graphics.FillRectangle(PressedBrush, backRect);
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
                    e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                }
                else if (state.Hovered)
                {
                    e.Graphics.FillRectangle(HoveredBrush, backRect);
                    ControlPaint.DrawBorder3D(e.Graphics, backRect, Border3DStyle.Raised);

                    // with classic state selection is drawn even if button is hovered
                    if (state.Enabled && (Focused || state.IsDefault))
                        e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                }
                else // normal state
                {
                    using (Brush b = new SolidBrush(state.BackColor))
                    {
                        e.Graphics.FillRectangle(b, backRect);
                    }

                    if (state.Enabled && (Focused || state.IsDefault))
                    {
                        e.Graphics.DrawPath(selectedFramePen, SelectionBorder);
                    }
                }
            }

            // Image
            PaintImage(e, image, out imageMargin);

            if (state.Enabled && Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(backRect.X + 4, backRect.Y + 4, backRect.Width - 7, backRect.Height - 7), state.ForeColor, state.BackColor);
        }

        private void PaintFlatAppearance(PaintStateEventArgs e, Image? image, out Padding imageMargin)
        {
            ControlAppearanceState state = e.State;
            Rectangle backRect = new Rectangle(ClientRectangle.X - 1, ClientRectangle.Y - 1, ClientRectangle.Width + 1, ClientRectangle.Height + 1);
            int borderWidth = FlatAppearance.BorderSize;
            Color backColor = state.BackColor;

            // unlike other styles, these colors are calculated on the fly because FlatApperance changes would be painful to track
            if (state.Pressed)
            {
                if (borderWidth != 0)
                    borderWidth++;

                if (FlatAppearance.MouseDownBackColor != Color.Empty)
                    backColor = FlatAppearance.MouseDownBackColor;
                else
                {
                    if (SystemInformation.HighContrast)
                    {
                        if (state.BackColor == SystemColors.Control)
                            backColor = SystemColors.ControlDark;
                        else
                            backColor = ControlPaint.Dark(backColor);
                    }
                    else
                    {
                        if (state.BackColor == SystemColors.Control)
                            backColor = SystemColors.ControlLightLight;
                        else
                            backColor = ControlPaint.LightLight(backColor);

                        float percentage = 0.9f;
                        if (backColor.GetBrightness() < 0.5f)
                            percentage = 1.2f;

                        backColor = Color.FromArgb(Adjust255(percentage, backColor.R), Adjust255(percentage, backColor.G), Adjust255(percentage, backColor.B));
                    }
                }
            }
            else if (state.Hovered)
            {
                if (borderWidth != 0 && Focused)
                    borderWidth++;

                if (FlatAppearance.MouseOverBackColor != Color.Empty)
                    backColor = FlatAppearance.MouseOverBackColor;
                else
                {
                    float percentage = 0.9f;
                    if (backColor.GetBrightness() < 0.5f)
                        percentage = 1.2f;

                    backColor = Color.FromArgb(Adjust255(percentage, backColor.R), Adjust255(percentage, backColor.G), Adjust255(percentage, backColor.B));
                }
            }
            else // normal state
            {
                // no matter if button is enabled or not, border is the same
                if (Focused)
                {
                    if (borderWidth != 0)
                        borderWidth++;
                }
                else if (state.IsDefault)
                    borderWidth++;
            }

            using (Brush b = new SolidBrush(state.BackColor))
            {
                e.Graphics.FillRectangle(b, backRect);
            }

            if (backColor != state.BackColor)
            {
                using (Brush b = new SolidBrush(backColor))
                {
                    backRect.Inflate(-(borderWidth / 2 + 3), -(borderWidth / 2 + 2));
                    e.Graphics.FillRectangle(b, backRect);
                }
            }

            if (borderWidth > 0)
            {
                // pen is created locally because its width is variable and its color cannot be tracked by events
                using (Pen pen = new Pen(FlatAppearance.BorderColor == Color.Empty ? SystemColors.ControlText : FlatAppearance.BorderColor, borderWidth))
                {
                    e.Graphics.DrawPath(pen, SelectionBorder);
                }
            }

            // Image
            PaintImage(e, image, out imageMargin);

            if (state.Enabled && Focused && ShowFocusCues)
            {
                Color focusColor = SystemInformation.HighContrast ? SystemColors.WindowText
                    : (BackColor.GetBrightness() < 0.5f ? ControlPaint.Light(state.BackColor) : ControlPaint.Dark(state.BackColor));

                using (Pen pen = new Pen(focusColor))
                {
                    int borderSize = FlatAppearance.BorderSize;
                    Rectangle rectangle = new Rectangle(ClientRectangle.X + borderSize + 4, ClientRectangle.Y + borderSize + 3, Width - borderSize * 2 - 9, Height - borderSize * 2 - 7);
                    e.Graphics.DrawRectangle(pen, rectangle);
                }
            }
        }

        private void PaintImage(PaintStateEventArgs e, Image? image, out Padding imageMargin)
        {
            var state = e.State;
            bool isDefaultGlyph = false;
            var img = image;
            bool dispose = false;
            if (img == null)
            {
                if (isElevated)
                    img = state.Enabled ? SecurityShield : SecurityShieldGray;
                else if (useDefaultGlyph)
                {
                    isDefaultGlyph = true;
                    if (!state.Enabled)
                        img = Resources.CommandLinkDisabled;
                    else if (state.Hovered && !state.Pressed)
                        img = Resources.CommandLinkHovered;
                    else
                        img = Resources.CommandLinkNormal;

                    // mirroring glyph
                    if (RtlTranslateContent(ImageAlign).AnyRight())
                    {
                        img = (Bitmap)img.Clone();
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        dispose = true;
                    }
                }
            }

            imageMargin = Padding.Empty;
            if (img != null)
                e.Graphics.DrawImage(img, GetImageBounds(state, img.Size, isDefaultGlyph, out imageMargin));
            if (dispose)
                img!.Dispose();
        }

        private Rectangle GetImageBounds(ControlAppearanceState state, Size imageSize, bool isDefaultGlyph, out Padding imageMargin)
        {
            bool useTheming = IsThemed && base.FlatStyle == FlatStyle.Standard;
            ContentAlignment imageAlignment = RtlTranslateContent(ImageAlign);
            bool isClassicPressed = !useTheming && state.Pressed && base.FlatStyle != FlatStyle.Flat;

            var bounds = new Rectangle(Point.Empty, imageSize);
            int leftMargin = HorizontalBasePadding;
            int topMargin = VerticalBasePadding;
            int imageOffsetH = 0;
            int imageOffsetV = 0;
            if (state.Pressed && !useTheming && base.FlatStyle != FlatStyle.Flat)
            {
                leftMargin++;
                topMargin++;
            }

            if (isElevated)
            {
                if (!useTheming)
                    imageOffsetH = -2;
            }
            else if (isDefaultGlyph)
            {
                if (useTheming)
                {
                    imageOffsetH = 1;
                    imageOffsetV = 2;
                }
                else
                    imageOffsetH = -3;
            }
            else if (!useTheming)
                imageOffsetH = -2;

            if (imageAlignment.AnyLeft())
                bounds.X = leftMargin + 5 + imageOffsetH;
            else if (imageAlignment.AnyCenter())
            {
                bounds.X = Width / 2 - imageSize.Width / 2;
                if (isClassicPressed)
                    bounds.X++;
            }
            else // any right
            {
                bounds.X = Width - (leftMargin + 5 + imageOffsetH + imageSize.Width);
                if (isClassicPressed)
                    bounds.X += 2;
            }

            if (imageAlignment.AnyTop())
                bounds.Y = topMargin + imageOffsetV + Math.Max(0, FontHeight / 2 - imageSize.Height / 2 - 1);
            else if (imageAlignment.AnyMiddle())
            {
                bounds.Y = Height / 2 - imageSize.Height / 2;
                if (isClassicPressed)
                    bounds.Y++;
            }
            else // any bottom
            {
                bounds.Y = Math.Max(topMargin, Height - topMargin - imageSize.Height);
                if (isClassicPressed)
                    bounds.Y += 2;
            }

            imageMargin = new Padding(leftMargin, topMargin, 0, 0);
            return bounds;
        }

        /// <summary>
        /// Checks image consistency. Returns true if image update has been performed.
        /// </summary>
        private bool CheckImage()
        {
            // if image is up-to-date checking consistency only (to handle setting base.Image)
            if (isImageUpToDate)
            {
                if (lastImageAlign == ImageAlign &&
                    (currentImage == base.Image
                    || currentImage == null && base.Image == null
                    || currentImage == NoGlyph && base.Image == null))
                    return false;
            }

            // Image > Elevated > default glyph > no glyph
            if (disabledImage != null)
            {
                disabledImage.Dispose();
                disabledImage = null;
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
            if (IsCustomRendering)
                return true;

            if (isElevated)
            {
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
            }
            else if (useDefaultGlyph)
            {
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                currentImage = NoGlyph;
                User32.SendMessage(Handle, Constants.BM_SETIMAGE, new IntPtr(1), NoGlyph.GetHicon());
            }

            return true;
        }

        private void ResetTheme()
        {
            Font = DefaultTextFont;
            DescriptionFont = DefaultDescriptionFont;
            //ForeColor = DefaultTextColor;
            //DescriptionColor = DefaultTextColor;
            //HighlightTextColor = DefaultHighlightColor;
            //HighlightDescriptionColor = DefaultHighlightColor;
            //PressedTextColor = DefaultPressedColor;
            //PressedDescriptionColor = DefaultPressedColor;
        }

        private bool ShouldSerializeFont()
        {
            return !Equals(Font, DefaultTextFont);
        }

        private bool ShouldSerializeDescriptionFont()
        {
            return !Equals(DescriptionFont, DefaultDescriptionFont);
        }

        private bool ShouldSerializeForeColor()
        {
            return foreColor != Color.Empty;
        }

        private bool ShouldSerializeDescriptionColor()
        {
            return descriptionColor != Color.Empty;
        }

        private bool ShouldSerializeHighlightTextColor()
        {
            return highlightTextColor != Color.Empty;
        }

        private bool ShouldSerializeHighlightDescriptionColor()
        {
            return highlightDescriptionColor != Color.Empty;
        }

        private bool ShouldSerializePressedTextColor()
        {
            return pressedTextColor != Color.Empty;
        }

        private bool ShouldSerializePressedDescriptionColor()
        {
            return pressedDescriptionColor != Color.Empty;
        }

        private bool ShouldSerializeDisabledForeColor()
        {
            return disabledForeColor != Color.Empty;
        }

        private bool ShouldSerializeDisabledBackColor()
        {
            return disabledBackColor != Color.Empty;
        }

        private void FreeBrushes()
        {
            if (pressedBrush != null)
            {
                pressedBrush.Dispose();
                pressedBrush = null;
            }

            if (hoveredBrush != null)
            {
                hoveredBrush.Dispose();
                hoveredBrush = null;
            }
        }

        private void FreePens()
        {
            if (hoveredFrameOuterPen != null)
            {
                hoveredFrameOuterPen.Dispose();
                hoveredFrameOuterPen = null;
            }

            if (hoveredFrameInnerPen != null)
            {
                hoveredFrameInnerPen.Dispose();
                hoveredFrameInnerPen = null;
            }

            if (pressedFramePen != null)
            {
                pressedFramePen.Dispose();
                pressedFramePen = null;
            }
        }

        private void FreeRegions()
        {
            if (outerBorder != null)
            {
                outerBorder.Dispose();
                outerBorder = null;
            }

            if (innerBorder != null)
            {
                innerBorder.Dispose();
                innerBorder = null;
            }

            if (selectionBorder != null)
            {
                selectionBorder.Dispose();
                selectionBorder = null;
            }
        }

        #endregion

        #region Event Handlers
        // ReSharper disable InconsistentNaming

        void defaultAnimationTimer_Tick(object sender, EventArgs e)
        {
            isAlternativeDefaultImage = !isAlternativeDefaultImage;
            Invalidate();
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #endregion

        #endregion

        #region ISupportsFading Members

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("CommandLinkButton")]
        [DefaultValue(true)]
        [Description("Gets or sets whether fading animations are enabled for the control. Animations work in Windows Vista and above, with non-classic themes.")]
        public bool FadingAnimationsEnabled
        {
            get { return fadingAnimationsEnabled; }
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
            get { return fadingOptions; }
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
        [Category("CommandLinkButton")]
        [DefaultValue(500)]
        [Description("Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.")]
        public int FadingAnimationDefaultSpeed
        {
            get { return fadingAnimationDefaultSpeed; }
            set
            {
                if (fadingAnimationDefaultSpeed == value)
                    return;

                if (fadingAnimationDefaultSpeed < 0)
                    throw new ArgumentOutOfRangeException("value");

                fadingAnimationDefaultSpeed = value;
            }
        }

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State
        {
            get { return GetAppearance(); }
        }

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
