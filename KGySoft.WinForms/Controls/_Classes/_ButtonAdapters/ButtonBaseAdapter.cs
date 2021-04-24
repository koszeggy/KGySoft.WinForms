#region Used namespaces

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class ButtonBaseAdapter
    {
        #region Nested classes

        #region ColorData class

        internal class ColorData
        {
            #region Fields

            internal Color buttonFace;
            internal Color buttonShadow;
            internal Color buttonShadowDark;
            internal Color constrastButtonShadow;
            internal Color windowText;
            internal Color highlight;
            internal Color lowHighlight;
            internal Color lowButtonFace;
            internal Color windowFrame;
            internal bool highContrast;

            #endregion

            #region Methods

            #region Internal Methods

            internal static ColorData Calculate(Graphics graphics, Color backColor, Color foreColor)
            {
                ColorData colors = new ColorData();
                colors.highContrast = SystemInformation.HighContrast;

                colors.buttonFace = backColor;

                if (backColor == SystemColors.Control)
                {
                    colors.buttonShadow = SystemColors.ControlDark;
                    colors.buttonShadowDark = SystemColors.ControlDarkDark;
                    colors.highlight = SystemColors.ControlLightLight;
                }
                else
                {
                    if (!colors.highContrast)
                    {
                        colors.buttonShadow = ControlPaint.Dark(backColor);
                        colors.buttonShadowDark = ControlPaint.DarkDark(backColor);
                        colors.highlight = ControlPaint.LightLight(backColor);
                    }
                    else
                    {
                        colors.buttonShadow = ControlPaint.Dark(backColor);
                        colors.buttonShadowDark = ControlPaint.LightLight(backColor);
                        colors.highlight = ControlPaint.LightLight(backColor);
                    }
                }

                const float lowlight = .1f;
                float adjust = 1 - lowlight;

                if (colors.buttonFace.GetBrightness() < .5)
                {
                    adjust = 1 + lowlight * 2;
                }
                colors.lowButtonFace = Color.FromArgb(Adjust255(adjust, colors.buttonFace.R),
                    Adjust255(adjust, colors.buttonFace.G),
                    Adjust255(adjust, colors.buttonFace.B));

                adjust = 1 - lowlight;
                if (colors.highlight.GetBrightness() < .5)
                {
                    adjust = 1 + lowlight * 2;
                }
                colors.lowHighlight = Color.FromArgb(Adjust255(adjust, colors.highlight.R),
                    Adjust255(adjust, colors.highlight.G),
                    Adjust255(adjust, colors.highlight.B));

                if (colors.highContrast && backColor != SystemColors.Control)
                {
                    colors.highlight = colors.lowHighlight;
                }

                colors.windowFrame = foreColor;

                if (colors.buttonFace.GetBrightness() < .5)
                {
                    colors.constrastButtonShadow = colors.lowHighlight;
                }
                else
                {
                    colors.constrastButtonShadow = colors.buttonShadow;
                }

                //if (!enabled && disabledTextDim)
                //{
                //    colors.windowText = colors.buttonShadow;
                //}
                //else
                //{
                colors.windowText = colors.windowFrame;
                //}

                //IntPtr hdc = this.graphics.GetHdc();

                //try
                //{
                //using (WindowsGraphics g = WindowsGraphics.FromHdc(hdc))
                //    {
                colors.buttonFace = graphics.GetNearestColor(colors.buttonFace);
                colors.buttonShadow = graphics.GetNearestColor(colors.buttonShadow);
                colors.buttonShadowDark = graphics.GetNearestColor(colors.buttonShadowDark);
                colors.constrastButtonShadow = graphics.GetNearestColor(colors.constrastButtonShadow);
                colors.windowText = graphics.GetNearestColor(colors.windowText);
                colors.highlight = graphics.GetNearestColor(colors.highlight);
                colors.lowHighlight = graphics.GetNearestColor(colors.lowHighlight);
                colors.lowButtonFace = graphics.GetNearestColor(colors.lowButtonFace);
                colors.windowFrame = graphics.GetNearestColor(colors.windowFrame);
                //}
                //}
                //finally
                //{
                //    this.graphics.ReleaseHdc();
                //}

                return colors;

            }

            #endregion

            #region Private Methods

            private static int Adjust255(float percentage, int value)
            {
                int v = (int)(percentage * value);
                if (v > 255)
                {
                    return 255;
                }
                return v;
            }

            #endregion

            #endregion
        }

        #endregion

        #region LayoutOptions class

        internal class LayoutOptions
        {
            #region Enumerations

            private enum Composition
            {
                NoneCombined = 0x00,
                CheckCombined = 0x01,
                TextImageCombined = 0x02,
                AllCombined = 0x03
            }

            #endregion

            #region Constants

            private const int textImageInset = 2;

            #endregion

            #region Fields

            #region Static Fields

            private static readonly int combineCheck = BitVector32.CreateMask();
            private static readonly int combineImageText = BitVector32.CreateMask(combineCheck);
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            private static readonly TextImageRelation[] _imageAlignToRelation = new TextImageRelation[] {
                /* TopLeft = */       TextImageRelation.ImageAboveText | TextImageRelation.ImageBeforeText,
                /* TopCenter = */     TextImageRelation.ImageAboveText,
                /* TopRight = */      TextImageRelation.ImageAboveText | TextImageRelation.TextBeforeImage,
                /* Invalid */         0,
                /* MiddleLeft = */    TextImageRelation.ImageBeforeText,
                /* MiddleCenter = */  0,
                /* MiddleRight = */   TextImageRelation.TextBeforeImage,
                /* Invalid */         0,
                /* BottomLeft = */    TextImageRelation.TextAboveImage | TextImageRelation.ImageBeforeText,
                /* BottomCenter = */  TextImageRelation.TextAboveImage,
                /* BottomRight = */   TextImageRelation.TextAboveImage | TextImageRelation.TextBeforeImage
            };
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

            #endregion

            #region Instance Fields

            #region Internal Fields

            internal Rectangle client;
            internal bool growBorderBy1PxWhenDefault;
            internal bool isDefault;
            internal int borderSize;
            internal int paddingSize;
            internal bool maxFocus;
            internal bool focusOddEvenFixup;
            internal Font font;
            internal string text;
            internal Size imageSize;
            internal int checkSize;
            internal int checkPaddingSize;
            internal ContentAlignment checkAlign;
            internal ContentAlignment imageAlign;
            internal ContentAlignment textAlign;
            internal TextImageRelation textImageRelation;
            internal bool hintTextUp;
            internal bool textOffset;
            internal bool shadowedText;
            internal bool layoutRTL;
            internal bool verticalText = false;
            internal bool useCompatibleTextRendering = false;
            internal bool everettButtonCompat = true;
            internal TextFormatFlags gdiTextFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
            internal StringFormatFlags gdipFormatFlags;
            internal StringTrimming gdipTrimming;
            internal HotkeyPrefix gdipHotkeyPrefix;
            internal StringAlignment gdipAlignment; // horizontal alignment.
            internal StringAlignment gdipLineAlignment; // vertical alignment.
            internal Padding padding;

            #endregion

            //#region Private Fields

            //private bool disableWordWrapping;

            //#endregion

            #endregion

            #endregion

            #region Properties

            #region Internal Properties

            /// <devdoc>
            ///     We don't cache the StringFormat itself because we don't have a deterministic way of disposing it, instead
            ///     we cache the flags that make it up and create it on demand so it can be disposed by calling code.
            /// </devdoc>
            internal StringFormat StringFormat
            {
                private get
                {
                    StringFormat format = new StringFormat();

                    format.FormatFlags = gdipFormatFlags;
                    format.Trimming = gdipTrimming;
                    format.HotkeyPrefix = gdipHotkeyPrefix;
                    format.Alignment = gdipAlignment;
                    format.LineAlignment = gdipLineAlignment;

                    //if (disableWordWrapping)
                    //{
                    //    format.FormatFlags |= StringFormatFlags.NoWrap;
                    //}

                    return format;
                }
                set
                {
                    gdipFormatFlags = value.FormatFlags;
                    gdipTrimming = value.Trimming;
                    gdipHotkeyPrefix = value.HotkeyPrefix;
                    gdipAlignment = value.Alignment;
                    gdipLineAlignment = value.LineAlignment;
                }
            }

            #endregion

            #region Private Properties

            /// <devdoc>
            /// </devdoc>
            private TextFormatFlags TextFormatFlags
            {
                get
                {
                    //if (disableWordWrapping)
                    //{
                    //    return gdiTextFormatFlags & ~TextFormatFlags.WordBreak;
                    //}

                    return gdiTextFormatFlags;
                }
            }

            private int FullBorderSize
            {
                get
                {
                    int result = borderSize;
                    if (OnePixExtraBorder)
                    {
                        borderSize++;
                    }
                    return borderSize;
                }
            }

            private bool OnePixExtraBorder
            {
                get { return growBorderBy1PxWhenDefault && isDefault; }
            }

            private int FullCheckSize
            {
                get
                {
                    return checkSize + checkPaddingSize;
                }
            }

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            private static TextImageRelation ImageAlignToRelation(ContentAlignment alignment)
            {
                return _imageAlignToRelation[LayoutUtils.ContentAlignmentToIndex(alignment)];
            }

            private static TextImageRelation TextAlignToRelation(ContentAlignment alignment)
            {
                return LayoutUtils.GetOppositeTextImageRelation(ImageAlignToRelation(alignment));
            }

            #endregion

            #region Instance Methods

            #region Public Methods

#if DEBUG
            public override string ToString()
            {
                return
                    "{ client = " + client + "\n" +
                    "OnePixExtraBorder = " + OnePixExtraBorder + "\n" +
                    "borderSize = " + borderSize + "\n" +
                    "paddingSize = " + paddingSize + "\n" +
                    "maxFocus = " + maxFocus + "\n" +
                    "font = " + font + "\n" +
                    "text = " + text + "\n" +
                    "imageSize = " + imageSize + "\n" +
                    "checkSize = " + checkSize + "\n" +
                    "checkPaddingSize = " + checkPaddingSize + "\n" +
                    "checkAlign = " + checkAlign + "\n" +
                    "imageAlign = " + imageAlign + "\n" +
                    "textAlign = " + textAlign + "\n" +
                    "textOffset = " + textOffset + "\n" +
                    "shadowedText = " + shadowedText + "\n" +
                    "textImageRelation = " + textImageRelation + "\n" +
                    "layoutRTL = " + layoutRTL + " }";
            }
#endif

            #endregion

            #region Internal Methods

            internal Size GetPreferredSizeCore(Graphics g, Size proposedSize)
            {
                // Get space required for border and padding
                //
                int linearBorderAndPadding = borderSize * 2 + paddingSize * 2;
                if (growBorderBy1PxWhenDefault)
                {
                    linearBorderAndPadding += 2;
                }
                Size bordersAndPadding = new Size(linearBorderAndPadding, linearBorderAndPadding);
                proposedSize -= bordersAndPadding;

                // Get space required for Check
                //
                int checkSizeLinear = FullCheckSize;
                Size checkSize = checkSizeLinear > 0 ? new Size(checkSizeLinear + 1, checkSizeLinear) : Size.Empty;

                // Get space required for Image - textImageInset compensated for by expanding image.
                //
                Size textImageInsetSize = new Size(textImageInset * 2, textImageInset * 2);
                Size requiredImageSize = (imageSize != Size.Empty) ? imageSize + textImageInsetSize : Size.Empty;

                // Pack Text into remaning space
                //
                proposedSize -= textImageInsetSize;
                proposedSize = Decompose(checkSize, requiredImageSize, proposedSize);

                Size textSize = Size.Empty;

                if (!string.IsNullOrEmpty(text))
                {
                    // When Button.AutoSizeMode is set to GrowOnly TableLayoutPanel expects buttons not to automatically wrap on word break. If
                    // there's enough room for the text to word-wrap then it will happen but the layout would not be adjusted to allow text wrapping.
                    // If someone has a carriage return in the text we'll honor that for preferred size, but we wont wrap based on constraints.
                    // See VSW#542448,537840,515227.
                    //try
                    //{
                    //    //disableWordWrapping = true;
                    textSize = GetTextSize(g, proposedSize) + textImageInsetSize;
                    //}
                    //finally
                    //{
                    //    //disableWordWrapping = false;
                    //}
                }

                // Combine pieces to get final preferred size
                //
                Size requiredSize = Compose(checkSize, imageSize, textSize);
                requiredSize += bordersAndPadding;

                return requiredSize;
            }

            internal LayoutData Layout(Graphics g)
            {
                LayoutData layout = new LayoutData(this);
                layout.client = client;

                // subtract border size from layout area
                int fullBorderSize = FullBorderSize;
                layout.face = Rectangle.Inflate(layout.client, -fullBorderSize, -fullBorderSize);

                // checkBounds, checkArea, field
                //
                CalcCheckmarkRectangle(layout);

                // imageBounds, imageLocation, textBounds
                LayoutTextAndImage(g, layout);

                // focus
                //
                if (maxFocus)
                {
                    layout.focus = layout.field;
                    layout.focus.Inflate(-1, -1);

                    // Adjust for padding. VSWhidbey #387208
                    layout.focus = LayoutUtils.InflateRect(layout.focus, padding);
                }
                else
                {
                    Rectangle textAdjusted = new Rectangle(layout.textBounds.X - 1, layout.textBounds.Y - 1,
                        layout.textBounds.Width + 2, layout.textBounds.Height + 3);
                    if (imageSize != Size.Empty)
                    {
                        layout.focus = Rectangle.Union(textAdjusted, layout.imageBounds);
                    }
                    else
                    {
                        layout.focus = textAdjusted;
                    }
                }
                if (focusOddEvenFixup)
                {
                    if (layout.focus.Height % 2 == 0)
                    {
                        layout.focus.Y++;
                        layout.focus.Height--;
                    }
                    if (layout.focus.Width % 2 == 0)
                    {
                        layout.focus.X++;
                        layout.focus.Width--;
                    }
                }


                return layout;
            }

            internal ContentAlignment RtlTranslateContent(ContentAlignment align)
            {

                if (layoutRTL)
                {
                    ContentAlignment[][] mapping = new ContentAlignment[3][];
                    mapping[0] = new ContentAlignment[2] { ContentAlignment.TopLeft, ContentAlignment.TopRight };
                    mapping[1] = new ContentAlignment[2] { ContentAlignment.MiddleLeft, ContentAlignment.MiddleRight };
                    mapping[2] = new ContentAlignment[2] { ContentAlignment.BottomLeft, ContentAlignment.BottomRight };

                    for (int i = 0; i < 3; ++i)
                    {
                        if (mapping[i][0] == align)
                        {
                            return mapping[i][1];
                        }
                        else if (mapping[i][1] == align)
                        {
                            return mapping[i][0];
                        }
                    }
                }
                return align;
            }

            internal void LayoutTextAndImage(Graphics g, LayoutData layout)
            {
                // Translate for Rtl applications.  This intentially shadows the member variables.
                ContentAlignment imageAlign = RtlTranslateContent(this.imageAlign);
                ContentAlignment textAlign = RtlTranslateContent(this.textAlign);
                TextImageRelation textImageRelation = RtlTranslateRelation(this.textImageRelation);

                // Figure out the maximum bounds for text & image
                Rectangle maxBounds = Rectangle.Inflate(layout.field, -textImageInset, -textImageInset);
                if (OnePixExtraBorder)
                {
                    maxBounds.Inflate(1, 1);
                }

                // Compute the final image and text bounds.
                if (imageSize == Size.Empty || text == null || text.Length == 0 || textImageRelation == TextImageRelation.Overlay)
                {
                    // Do not worry about text/image overlaying
                    Size textSize = GetTextSize(g, maxBounds.Size);

                    // FOR EVERETT COMPATIBILITY - DO NOT CHANGE
                    Size size = imageSize;
                    if (layout.options.everettButtonCompat && imageSize != Size.Empty)
                    {
                        size = new Size(size.Width + 1, size.Height + 1);
                    }

                    layout.imageBounds = LayoutUtils.Align(size, maxBounds, imageAlign);
                    layout.textBounds = LayoutUtils.Align(textSize, maxBounds, textAlign);

                }
                else
                {
                    // Rearrage text/image to prevent overlay.  Pack text into maxBounds - space reserved for image
                    Size maxTextSize = LayoutUtils.SubAlignedRegion(maxBounds.Size, imageSize, textImageRelation);
                    Size textSize = GetTextSize(g, maxTextSize);
                    Rectangle maxCombinedBounds = maxBounds;

                    // Combine text & image into one rectangle that we center within maxBounds.
                    Size combinedSize = LayoutUtils.AddAlignedRegion(textSize, imageSize, textImageRelation);
                    maxCombinedBounds.Size = LayoutUtils.UnionSizes(maxCombinedBounds.Size, combinedSize);
                    Rectangle combinedBounds = LayoutUtils.Align(combinedSize, maxCombinedBounds, ContentAlignment.MiddleCenter);

                    // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                    // imageEdge indicates whether the combination of imageAlign and textImageRelation place
                    // the image along the edge of the control.  If so, we can increase the space for text.
                    bool imageEdge = (AnchorStyles)(ImageAlignToRelation(imageAlign) & textImageRelation) != AnchorStyles.None;

                    // textEdge indicates whether the combination of textAlign and textImageRelation place
                    // the text along the edge of the control.  If so, we can increase the space for image.
                    bool textEdge = (AnchorStyles)(TextAlignToRelation(textAlign) & textImageRelation) != AnchorStyles.None;
                    // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

                    if (imageEdge)
                    {
                        // If imageEdge, just split imageSize off of maxCombinedBounds.
                        LayoutUtils.SplitRegion(maxCombinedBounds, imageSize, (AnchorStyles)textImageRelation, out layout.imageBounds, out layout.textBounds);
                    }
                    else if (textEdge)
                    {
                        // Else if textEdge, just split textSize off of maxCombinedBounds.
                        LayoutUtils.SplitRegion(maxCombinedBounds, textSize, (AnchorStyles)LayoutUtils.GetOppositeTextImageRelation(textImageRelation), out layout.textBounds, out layout.imageBounds);
                    }
                    else
                    {
                        // Expand the adjacent regions to maxCombinedBounds (centered) and split the rectangle into imageBounds and textBounds.
                        LayoutUtils.SplitRegion(combinedBounds, imageSize, (AnchorStyles)textImageRelation, out layout.imageBounds, out layout.textBounds);
                        LayoutUtils.ExpandRegionsToFillBounds(maxCombinedBounds, (AnchorStyles)textImageRelation, ref layout.imageBounds, ref layout.textBounds);
                    }

                    // align text/image within their regions.
                    layout.imageBounds = LayoutUtils.Align(imageSize, layout.imageBounds, imageAlign);
                    layout.textBounds = LayoutUtils.Align(textSize, layout.textBounds, textAlign);
                }

                //Don't call "layout.imageBounds = Rectangle.Intersect(layout.imageBounds, maxBounds);"
                // because that is a breaking change that causes images to be scaled to the dimensions of the control.
                //adjust textBounds so that the text is still visible even if the image is larger than the button's size
                //fixes Whidbey 234985
                //why do we intersect with layout.field for textBounds while we intersect with maxBounds for imageBounds?
                //this is because there are some legacy code which squeezes the button so small that text will get clipped
                //if we intersect with maxBounds. Have to do this for backward compatibility.
                //See Whidbey 341480
                if (textImageRelation == TextImageRelation.TextBeforeImage || textImageRelation == TextImageRelation.ImageBeforeText)
                {
                    //adjust the vertical position of textBounds so that the text doesn't fall off the boundary of the button
                    int textBottom = Math.Min(layout.textBounds.Bottom, layout.field.Bottom);
                    layout.textBounds.Y = Math.Max(Math.Min(layout.textBounds.Y, layout.field.Y + (layout.field.Height - layout.textBounds.Height) / 2), layout.field.Y);
                    layout.textBounds.Height = textBottom - layout.textBounds.Y;
                }
                if (textImageRelation == TextImageRelation.TextAboveImage || textImageRelation == TextImageRelation.ImageAboveText)
                {
                    //adjust the horizontal position of textBounds so that the text doesn't fall off the boundary of the button
                    int textRight = Math.Min(layout.textBounds.Right, layout.field.Right);
                    layout.textBounds.X = Math.Max(Math.Min(layout.textBounds.X, layout.field.X + (layout.field.Width - layout.textBounds.Width) / 2), layout.field.X);
                    layout.textBounds.Width = textRight - layout.textBounds.X;
                }
                if (textImageRelation == TextImageRelation.ImageBeforeText && layout.imageBounds.Size.Width != 0)
                {
                    //squeezes imageBounds.Width so that text is visible
                    layout.imageBounds.Width = Math.Max(0, Math.Min(maxBounds.Width - layout.textBounds.Width, layout.imageBounds.Width));
                    layout.textBounds.X = layout.imageBounds.X + layout.imageBounds.Width;
                }
                if (textImageRelation == TextImageRelation.ImageAboveText && layout.imageBounds.Size.Height != 0)
                {
                    //squeezes imageBounds.Height so that the text is visible
                    layout.imageBounds.Height = Math.Max(0, Math.Min(maxBounds.Height - layout.textBounds.Height, layout.imageBounds.Height));
                    layout.textBounds.Y = layout.imageBounds.Y + layout.imageBounds.Height;
                }
                //make sure that textBound is contained in layout.field
                layout.textBounds = Rectangle.Intersect(layout.textBounds, layout.field);
                if (hintTextUp)
                {
                    layout.textBounds.Y--;
                }
                if (textOffset)
                {
                    layout.textBounds.Offset(1, 1);
                }

                // FOR EVERETT COMPATIBILITY - DO NOT CHANGE
                if (layout.options.everettButtonCompat)
                {
                    layout.imageStart = layout.imageBounds.Location;
                    layout.imageBounds = Rectangle.Intersect(layout.imageBounds, layout.field);
                }
                else if (!Application.RenderWithVisualStyles)
                {
                    // Not sure why this is here, but we can't remove it, since it might break
                    // ToolStrips on non-themed machines
                    layout.textBounds.X++;
                }

                // clip
                //
                int bottom;
                // If we are using GDI to measure text, then we can get into a situation, where
                // the proposed height is ignore. In this case, we want to clip it against
                // maxbounds. VSWhidbey #480670
                if (!useCompatibleTextRendering)
                {
                    bottom = Math.Min(layout.textBounds.Bottom, maxBounds.Bottom);
                    layout.textBounds.Y = Math.Max(layout.textBounds.Y, maxBounds.Y);
                }
                else
                {
                    // If we are using GDI+ (like Everett), then use the old Everett code
                    // This ensures that we have pixel-level rendering compatibility
                    bottom = Math.Min(layout.textBounds.Bottom, layout.field.Bottom);
                    layout.textBounds.Y = Math.Max(layout.textBounds.Y, layout.field.Y);
                }
                layout.textBounds.Height = bottom - layout.textBounds.Y;

                //This causes a breaking change because images get shrunk to the new clipped size instead of clipped.
                //********** bottom = Math.Min(layout.imageBounds.Bottom, maxBounds.Bottom);
                //********** layout.imageBounds.Y = Math.Max(layout.imageBounds.Y, maxBounds.Y);
                //********** layout.imageBounds.Height = bottom - layout.imageBounds.Y;

            }

            #endregion

            #region Protected Methods

            protected virtual Size GetTextSize(Graphics g, Size proposedSize)
            {
                // 0 or 1 means unbounded
                if (proposedSize.Width <= 1)
                    proposedSize.Width = Int32.MaxValue;
                if (proposedSize.Height <= 1)
                    proposedSize.Height = Int32.MaxValue;

                //set the Prefix field of TextFormatFlags
                proposedSize = LayoutUtils.FlipSizeIf(verticalText, proposedSize);
                Size textSize = Size.Empty;

                if (useCompatibleTextRendering)
                { // GDI+ text rendering.
                    //using (Graphics g = WindowsFormsUtils.CreateMeasurementGraphics())
                    //{
                    using (StringFormat gdipStringFormat = StringFormat)
                    {
                        textSize = Size.Ceiling(g.MeasureString(text, font, new SizeF(proposedSize.Width, proposedSize.Height), gdipStringFormat));
                    }
                    //}
                }
                else if (!string.IsNullOrEmpty(text))
                { // GDI text rendering (Whidbey feature).
                    textSize = TextRenderer.MeasureText(g, text, font, proposedSize, TextFormatFlags);
                }
                //else skip calling MeasureText, it should return 0,0

                return LayoutUtils.FlipSizeIf(verticalText, textSize);

            }

            #endregion

            #region Private Methods

            private Size Compose(Size checkSize, Size imageSize, Size textSize)
            {
                Composition hComposition = GetHorizontalComposition();
                Composition vComposition = GetVerticalComposition();
                return new Size(
                    xCompose(hComposition, checkSize.Width, imageSize.Width, textSize.Width),
                    xCompose(vComposition, checkSize.Height, imageSize.Height, textSize.Height)
                    );
            }

            private int xCompose(Composition composition, int checkSize, int imageSize, int textSize)
            {
                switch (composition)
                {
                    case Composition.NoneCombined:
                        return checkSize + imageSize + textSize;
                    case Composition.CheckCombined:
                        return Math.Max(checkSize, imageSize + textSize);
                    case Composition.TextImageCombined:
                        return Math.Max(imageSize, textSize) + checkSize;
                    case Composition.AllCombined:
                        return Math.Max(Math.Max(checkSize, imageSize), textSize);
                    default:
                        Debug.Fail("composition", composition.ToString());
                        return -7107;
                }
            }

            private Size Decompose(Size checkSize, Size imageSize, Size proposedSize)
            {
                Composition hComposition = GetHorizontalComposition();
                Composition vComposition = GetVerticalComposition();
                return new Size(
                    xDecompose(hComposition, checkSize.Width, imageSize.Width, proposedSize.Width),
                    xDecompose(vComposition, checkSize.Height, imageSize.Height, proposedSize.Height)
                    );
            }

            private int xDecompose(Composition composition, int checkSize, int imageSize, int proposedSize)
            {
                switch (composition)
                {
                    case Composition.NoneCombined:
                        return proposedSize - (checkSize + imageSize);
                    case Composition.CheckCombined:
                        return proposedSize - imageSize;
                    case Composition.TextImageCombined:
                        return proposedSize - checkSize;
                    case Composition.AllCombined:
                        return proposedSize;
                    default:
                        Debug.Fail("composition", composition.ToString());
                        return -7109;
                }
            }

            private Composition GetHorizontalComposition()
            {
                BitVector32 action = new BitVector32();

                // Checks reserve space horizontally if possible, so only AnyLeft/AnyRight prevents combination.
                action[combineCheck] = checkAlign == ContentAlignment.MiddleCenter || !LayoutUtils.IsHorizontalAlignment(checkAlign);
                action[combineImageText] = !LayoutUtils.IsHorizontalRelation(textImageRelation);
                return (Composition)action.Data;
            }

            private Composition GetVerticalComposition()
            {
                BitVector32 action = new BitVector32();

                // Checks reserve space horizontally if possible, so only Top/Bottom prevents combination.
                action[combineCheck] = checkAlign == ContentAlignment.MiddleCenter || !LayoutUtils.IsVerticalAlignment(checkAlign);
                action[combineImageText] = !LayoutUtils.IsVerticalRelation(textImageRelation);
                return (Composition)action.Data;
            }

            TextImageRelation RtlTranslateRelation(TextImageRelation relation)
            {
                // If RTL, we swap ImageBeforeText and TextBeforeImage
                if (layoutRTL)
                {
                    switch (relation)
                    {
                        case TextImageRelation.ImageBeforeText:
                            return TextImageRelation.TextBeforeImage;
                        case TextImageRelation.TextBeforeImage:
                            return TextImageRelation.ImageBeforeText;
                    }
                }
                return relation;
            }

            void CalcCheckmarkRectangle(LayoutData layout)
            {
                int checkSizeFull = FullCheckSize;
                layout.checkBounds = new Rectangle(client.X, client.Y, checkSizeFull, checkSizeFull);

                // Translate checkAlign for Rtl applications
                ContentAlignment align = RtlTranslateContent(checkAlign);

                Rectangle field = Rectangle.Inflate(layout.face, -paddingSize, -paddingSize);

                layout.field = field;

                if (checkSizeFull > 0)
                {
                    if (align.AnyRight())
                    {
                        layout.checkBounds.X = (field.X + field.Width) - layout.checkBounds.Width;
                    }
                    else if (align.AnyCenter())
                    {
                        layout.checkBounds.X = field.X + (field.Width - layout.checkBounds.Width) / 2;
                    }

                    if (align.AnyBottom())
                    {
                        layout.checkBounds.Y = (field.Y + field.Height) - layout.checkBounds.Height;
                    }
                    else if (align.AnyTop())
                    {
                        layout.checkBounds.Y = field.Y + 2; // + 2: this needs to be aligned to the text (bug 87483)
                    }
                    else
                    {
                        layout.checkBounds.Y = field.Y + (field.Height - layout.checkBounds.Height) / 2;
                    }

                    switch (align)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            layout.checkArea.X = field.X;
                            layout.checkArea.Width = checkSizeFull + 1;

                            layout.checkArea.Y = field.Y;
                            layout.checkArea.Height = field.Height;

                            layout.field.X += checkSizeFull + 1;
                            layout.field.Width -= checkSizeFull + 1;
                            break;
                        case ContentAlignment.TopRight:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.BottomRight:
                            layout.checkArea.X = field.X + field.Width - checkSizeFull;
                            layout.checkArea.Width = checkSizeFull + 1;

                            layout.checkArea.Y = field.Y;
                            layout.checkArea.Height = field.Height;

                            layout.field.Width -= checkSizeFull + 1;
                            break;
                        case ContentAlignment.TopCenter:
                            layout.checkArea.X = field.X;
                            layout.checkArea.Width = field.Width;

                            layout.checkArea.Y = field.Y;
                            layout.checkArea.Height = checkSizeFull;

                            layout.field.Y += checkSizeFull;
                            layout.field.Height -= checkSizeFull;
                            break;

                        case ContentAlignment.BottomCenter:
                            layout.checkArea.X = field.X;
                            layout.checkArea.Width = field.Width;

                            layout.checkArea.Y = field.Y + field.Height - checkSizeFull;
                            layout.checkArea.Height = checkSizeFull;

                            layout.field.Height -= checkSizeFull;
                            break;

                        case ContentAlignment.MiddleCenter:
                            layout.checkArea = layout.checkBounds;
                            break;
                    }

                    layout.checkBounds.Width -= checkPaddingSize;
                    layout.checkBounds.Height -= checkPaddingSize;
                }
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region LayoutData class

        internal class LayoutData
        {
            #region Fields

            internal Rectangle client;
            internal Rectangle face;
            internal Rectangle checkArea;
            internal Rectangle checkBounds;
            internal Rectangle textBounds;
            internal Rectangle field;
            internal Rectangle focus;
            internal Rectangle imageBounds;
            internal Point imageStart; // FOR EVERETT COMPATIBILITY - DO NOT CHANGE
            internal LayoutOptions options;

            #endregion

            #region Constructors

            internal LayoutData(LayoutOptions options)
            {
                Debug.Assert(options != null, "must have options");
                this.options = options;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly ButtonBase control;

        #endregion

        #region Properties

        protected virtual int ButtonBorderSize
        {
            get { return 4; }
        }

        protected ButtonBase ButtonInstance
        {
            get { return control; }
        }

        #endregion

        #region Constructors

        internal ButtonBaseAdapter(ButtonBase control)
        {
            this.control = control;
        }

        #endregion

        #region Methods

        #region Static Methods

        protected static Brush CreateDitherBrush(Color color1, Color color2)
        {
            // Note: Don't dispose the bitmap here. The texture brush will take ownership
            // of the bitmap. So the bitmap will get disposed by the brush's Dispose().
            using (Bitmap b = new Bitmap(2, 2))
            {
                b.SetPixel(0, 0, color1);
                b.SetPixel(0, 1, color2);
                b.SetPixel(1, 1, color1);
                b.SetPixel(1, 0, color2);

                return new TextureBrush(b);
            }
        }

        protected static void DrawDitheredFill(Graphics g, Color color1, Color color2, Rectangle bounds)
        {
            using (Brush brush = CreateDitherBrush(color1, color2))
            {
                g.FillRectangle(brush, bounds);
            }
        }

        /// <summary>
        /// Draws a border for the in the 3D style of the popup button.
        /// </summary>
        protected static void Draw3DLiteBorder(Graphics g, Rectangle r, ColorData colors, bool up)
        {
            // Draw counter-clock-wise.
            Point p1 = new Point(r.Right - 1, r.Top);  // upper inner right.
            Point p2 = new Point(r.Left, r.Top);  // upper left.
            Point p3 = new Point(r.Left, r.Bottom - 1);  // bottom inner left.
            Point p4 = new Point(r.Right - 1, r.Bottom - 1);  // inner bottom right.

            // top, left
            Pen pen = up ? new Pen(colors.highlight) : new Pen(colors.buttonShadow);

            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left (top-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom, right
            pen = up ? new Pen(colors.buttonShadow) : new Pen(colors.highlight);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4); // bottom (left-right)
                g.DrawLine(pen, p4, p1); // right(bottom-up)
            }
            finally
            {
                pen.Dispose();
            }
        }

        protected static void DrawFlatBorder(Graphics g, Rectangle r, Color c)
        {
            ControlPaint.DrawBorder(g, r, c, ButtonBorderStyle.Solid);
        }

        protected static void DrawDefaultBorder(Graphics g, Rectangle r, Color c, bool isDefault)
        {
            if (isDefault)
            {
                r.Inflate(1, 1);

                Pen pen;
                if (c.IsSystemColor)
                {
                    pen = SystemPens.FromSystemColor(c);
                }
                else
                {
                    pen = new Pen(c);
                }
                g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                if (!c.IsSystemColor)
                {
                    pen.Dispose();
                }
            }
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal void Paint(PaintStateEventArgs e)
        {
            if (!e.State.Visible)
            {
                control.PaintTransparentBackground(e);
                return;
            }

            if (e.State.Pressed)
            {
                PaintDown(e);
            }
            else if (e.State.Hovered)
            {
                PaintOver(e);
            }
            else
            {
                PaintUp(e);
            }
        }

        internal virtual Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
        {
            return Layout(g, state).GetPreferredSizeCore(g, proposedSize);
        }

        internal abstract void PaintUp(PaintStateEventArgs e);

        internal abstract void PaintDown(PaintStateEventArgs e);

        internal abstract void PaintOver(PaintStateEventArgs e);

        internal virtual LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions layout = new LayoutOptions();
            layout.client = LayoutUtils.DeflateRect(control.ClientRectangle, control.Padding);
            layout.padding = control.Padding;
            layout.growBorderBy1PxWhenDefault = true;
            layout.isDefault = state.IsDefault;
            layout.borderSize = 2;
            layout.paddingSize = 0;
            layout.maxFocus = true;
            layout.focusOddEvenFixup = false;
            layout.font = control.Font;
            layout.text = state.Text;
            layout.imageSize = (control.Image == null) ? Size.Empty : control.Image.Size;
            layout.checkSize = 0;
            layout.checkPaddingSize = 0;
            layout.checkAlign = ContentAlignment.TopLeft;
            layout.imageAlign = control.ImageAlign;
            layout.textAlign = control.TextAlign;
            layout.hintTextUp = false;
            layout.shadowedText = !state.Enabled;
            layout.layoutRTL = RightToLeft.Yes == control.RightToLeft;
            layout.textImageRelation = control.TextImageRelation;
            layout.useCompatibleTextRendering = control.UseCompatibleTextRendering;

            if (control.FlatStyle != FlatStyle.System)
            {
                if (layout.useCompatibleTextRendering)
                {
                    using (StringFormat format = control.GetFormatFlags().ToStringFormat())
                    {
                        layout.StringFormat = format;
                    }
                }
                else
                {
                    layout.gdiTextFormatFlags = control.GetFormatFlags();
                }
            }

            return layout;
        }

        #endregion

        #region Protected Methods

        protected abstract LayoutOptions Layout(Graphics graphics, ControlAppearanceState state);

        protected void PaintButtonBackground(PaintEventArgs e, Rectangle bounds, Color backColor)
        {
            bool isSystemColor = backColor.IsSystemColor;
            Brush brush = isSystemColor ? SystemBrushes.FromSystemColor(backColor) : new SolidBrush(backColor);
            try
            {
                e.Graphics.FillRectangle(brush, bounds);
            }
            finally
            {
                if (!isSystemColor)
                    brush.Dispose();
            }
        }

        protected void PaintField(PaintStateEventArgs e, LayoutData layout, ColorData colors, bool drawFocus)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            Rectangle maxFocus = layout.focus;
            DrawText(g, layout, colors, state);
            if (drawFocus)
            {
                DrawFocus(g, maxFocus, state);
            }
        }

        protected void PaintImage(PaintStateEventArgs e, LayoutData layout)
        {
            DrawImage(e.Graphics, layout, e.State);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draws the focus rectangle if the control has focus.
        /// </summary>
        void DrawFocus(Graphics g, Rectangle r, ControlAppearanceState state)
        {
            if (control.Focused && ((ISupportButtonAdapter)control).ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, r, state.ForeColor, state.BackColor);
            }
        }

        /// <summary>
        /// Draws the button's image.
        /// </summary>
        void DrawImage(Graphics graphics, LayoutData layout, ControlAppearanceState state)
        {
            if (control.Image != null)
            {
                //setup new clip region & draw
                DrawImageCore(graphics, control.Image, layout.imageBounds, layout.imageStart, layout, state);
            }
        }

        private void DrawImageCore(Graphics graphics, Image image, Rectangle imageBounds, Point imageStart, LayoutData layout, ControlAppearanceState state)
        {
            Region oldClip = graphics.Clip;

            if (!layout.options.everettButtonCompat)
            { // FOR EVERETT COMPATIBILITY - DO NOT CHANGE
                Rectangle bounds = new Rectangle(ButtonBorderSize, ButtonBorderSize, control.Width - (2 * ButtonBorderSize), control.Height - (2 * ButtonBorderSize));

                Region newClip = oldClip.Clone();
                newClip.Intersect(bounds);

                // If we don't do this, DrawImageUnscaled will happily draw the entire image, even though imageBounds
                // is smaller than the image size.
                newClip.Intersect(imageBounds);
                graphics.Clip = newClip;
            }
            else
            {
                // FOR EVERETT COMPATIBILITY - DO NOT CHANGE
                imageBounds.Width += 1;
                imageBounds.Height += 1;
                imageBounds.X = imageStart.X + 1;
                imageBounds.Y = imageStart.Y + 1;
            }


            try
            {
                if (!state.Enabled)
                    // need to specify width and height
                    graphics.DrawImageDisabled(image, imageBounds, state.BackColor, true);
                else
                    graphics.DrawImage(image, imageBounds.X, imageBounds.Y, image.Width, image.Height);
            }

            finally
            {
                if (!layout.options.everettButtonCompat)
                {// FOR EVERETT COMPATIBILITY - DO NOT CHANGE
                    graphics.Clip = oldClip;
                }
            }
        }

        /// <summary>
        /// Draws the button's text.
        /// </summary>
        void DrawText(Graphics g, LayoutData layout, ColorData colors, ControlAppearanceState state)
        {
            Rectangle r = layout.textBounds;
            bool disabledText3D = layout.options.shadowedText;

            if (control.UseCompatibleTextRendering)
            { // Draw text using GDI+
                using (StringFormat stringFormat = control.GetFormatFlags().ToStringFormat())
                {
                    // DrawString doesn't seem to draw where it says it does
                    if (control.TextAlign.AnyCenter())
                    {
                        r.X -= 1;
                    }
                    r.Width += 1;

                    if (disabledText3D && !state.Enabled)
                    {
                        r.Offset(1, 1);
                        using (SolidBrush brush = new SolidBrush(colors.highlight))
                        {
                            g.DrawString(state.Text, control.Font, brush, r, stringFormat);

                            r.Offset(-1, -1);
                            brush.Color = state.ForeColor; // here: DisabledForeColor
                            g.DrawString(state.Text, control.Font, brush, r, stringFormat);
                        }
                    }
                    else
                    {
                        Brush brush;

                        brush = state.ForeColor.IsSystemColor
                            ? SystemBrushes.FromSystemColor(state.ForeColor)
                            : new SolidBrush(state.ForeColor);
                        g.DrawString(state.Text, control.Font, brush, r, stringFormat);

                        if (!state.ForeColor.IsSystemColor)
                        {
                            brush.Dispose();
                        }
                    }
                }
            }
            else
            { // Draw text using GDI (Whidbey feature).
                TextFormatFlags formatFlags = control.GetFormatFlags();

                if (disabledText3D && !state.Enabled)
                {
                    Color disabledColor = state.ForeColor; // here: DisabledForeColor
                    if (Application.RenderWithVisualStyles)
                    {
                        //don't draw chiseled text if themed as win32 app does.
                        TextRenderer.DrawText(g, state.Text, control.Font, r, disabledColor, formatFlags);
                    }
                    else
                    {
                        r.Offset(1, 1);
                        TextRenderer.DrawText(g, state.Text, control.Font, r, colors.highlight, formatFlags);

                        r.Offset(-1, -1);
                        TextRenderer.DrawText(g, state.Text, control.Font, r, disabledColor, formatFlags);
                    }
                }
                else
                {
                    TextRenderer.DrawText(g, state.Text, control.Font, r, state.ForeColor, formatFlags);
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
