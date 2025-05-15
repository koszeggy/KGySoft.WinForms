#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonBaseAdapter.cs
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

#region Used Namespaces

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

using KGySoft.Drawing;

#endregion

#region Used Aliases

using ContentAlignment = System.Drawing.ContentAlignment;

#endregion

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

            internal Color ButtonFace;
            internal Color ButtonShadow;
            internal Color ButtonShadowDark;
            internal Color ContrastButtonShadow;
            internal Color WindowText;
            internal Color Highlight;
            internal Color LowHighlight;
            internal Color LowButtonFace;
            internal Color WindowFrame;
            internal bool HighContrast;

            #endregion

            #region Methods

            #region Internal Methods

            internal static ColorData Calculate(Graphics graphics, Color backColor, Color foreColor)
            {
                ColorData colors = new ColorData();
                colors.HighContrast = VisualStyleHelper.HighContrast;

                colors.ButtonFace = backColor;

                if (backColor == SystemColors.Control)
                {
                    colors.ButtonShadow = SystemColors.ControlDark;
                    colors.ButtonShadowDark = SystemColors.ControlDarkDark;
                    colors.Highlight = SystemColors.ControlLightLight;
                }
                else
                {
                    if (!colors.HighContrast)
                    {
                        colors.ButtonShadow = ControlPaint.Dark(backColor);
                        colors.ButtonShadowDark = ControlPaint.DarkDark(backColor);
                        colors.Highlight = ControlPaint.LightLight(backColor);
                    }
                    else
                    {
                        colors.ButtonShadow = ControlPaint.Dark(backColor);
                        colors.ButtonShadowDark = ControlPaint.LightLight(backColor);
                        colors.Highlight = ControlPaint.LightLight(backColor);
                    }
                }

                const float lowlight = .1f;
                float adjust = 1 - lowlight;

                if (colors.ButtonFace.GetBrightness() < .5)
                    adjust = 1 + lowlight * 2;

                colors.LowButtonFace = Color.FromArgb(Adjust255(adjust, colors.ButtonFace.R),
                    Adjust255(adjust, colors.ButtonFace.G),
                    Adjust255(adjust, colors.ButtonFace.B));

                adjust = 1 - lowlight;
                if (colors.Highlight.GetBrightness() < .5)
                    adjust = 1 + lowlight * 2;

                colors.LowHighlight = Color.FromArgb(Adjust255(adjust, colors.Highlight.R),
                    Adjust255(adjust, colors.Highlight.G),
                    Adjust255(adjust, colors.Highlight.B));

                if (colors.HighContrast && backColor != SystemColors.Control)
                    colors.Highlight = colors.LowHighlight;

                colors.WindowFrame = foreColor;

                if (colors.ButtonFace.GetBrightness() < .5)
                    colors.ContrastButtonShadow = colors.LowHighlight;
                else
                    colors.ContrastButtonShadow = colors.ButtonShadow;

                //if (!enabled && disabledTextDim)
                //{
                //    colors.windowText = colors.buttonShadow;
                //}
                //else
                //{
                colors.WindowText = colors.WindowFrame;
                //}

                //IntPtr hdc = this.graphics.GetHdc();

                //try
                //{
                //using (WindowsGraphics g = WindowsGraphics.FromHdc(hdc))
                //    {
                colors.ButtonFace = graphics.GetNearestColor(colors.ButtonFace);
                colors.ButtonShadow = graphics.GetNearestColor(colors.ButtonShadow);
                colors.ButtonShadowDark = graphics.GetNearestColor(colors.ButtonShadowDark);
                colors.ContrastButtonShadow = graphics.GetNearestColor(colors.ContrastButtonShadow);
                colors.WindowText = graphics.GetNearestColor(colors.WindowText);
                colors.Highlight = graphics.GetNearestColor(colors.Highlight);
                colors.LowHighlight = graphics.GetNearestColor(colors.LowHighlight);
                colors.LowButtonFace = graphics.GetNearestColor(colors.LowButtonFace);
                colors.WindowFrame = graphics.GetNearestColor(colors.WindowFrame);
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
                return v > 255 ? 255 : v;
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
            private static readonly TextImageRelation[] imageAlignToRelation = new TextImageRelation[] {
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

            internal Rectangle Client;
            internal int BorderSize;
            internal int PaddingSize;
            internal bool GrowBorderBy1PxWhenDefault;
            internal bool IsDefault;
            internal bool MaxFocus;
            internal bool ForceDoubleFocusWidth;
            internal bool FocusOddEvenFixup;
            internal Font Font = null!;
            internal string? Text;
            internal Size ImageSize;
            internal int CheckSize;
            internal int CheckPaddingSize;
            internal ContentAlignment CheckAlign;
            internal ContentAlignment ImageAlign;
            internal ContentAlignment TextAlign;
            internal TextImageRelation TextImageRelation;
            internal bool HintTextUp;
            internal bool TextOffset;
            internal bool ShadowedText;
            internal bool LayoutRtl;
            internal bool VerticalText = false;
            internal bool UseCompatibleTextRendering;
            internal bool DotNetOneButtonCompat = true;
            internal TextFormatFlags GdiTextFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
            internal StringFormatFlags GdipFormatFlags;
            internal StringTrimming GdipTrimming;
            internal HotkeyPrefix GdipHotkeyPrefix;
            internal StringAlignment GdipAlignment; // horizontal alignment.
            internal StringAlignment GdipLineAlignment; // vertical alignment.
            internal Padding Padding;
            internal PointF Scale;

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

                    format.FormatFlags = GdipFormatFlags;
                    format.Trimming = GdipTrimming;
                    format.HotkeyPrefix = GdipHotkeyPrefix;
                    format.Alignment = GdipAlignment;
                    format.LineAlignment = GdipLineAlignment;

                    //if (disableWordWrapping)
                    //{
                    //    format.FormatFlags |= StringFormatFlags.NoWrap;
                    //}

                    return format;
                }
                set
                {
                    GdipFormatFlags = value.FormatFlags;
                    GdipTrimming = value.Trimming;
                    GdipHotkeyPrefix = value.HotkeyPrefix;
                    GdipAlignment = value.Alignment;
                    GdipLineAlignment = value.LineAlignment;
                }
            }

            #endregion

            #region Private Properties

            /// <devdoc>
            /// </devdoc>
            private TextFormatFlags TextFormatFlags =>
                //if (disableWordWrapping)
                //{
                //    return gdiTextFormatFlags & ~TextFormatFlags.WordBreak;
                //}
                GdiTextFormatFlags;

            private int FullBorderSize => OnePixExtraBorder ? BorderSize++ : BorderSize;
            private bool OnePixExtraBorder => GrowBorderBy1PxWhenDefault && IsDefault;
            private int FullCheckSize => CheckSize + CheckPaddingSize;

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            private static TextImageRelation ImageAlignToRelation(ContentAlignment alignment)
                => imageAlignToRelation[LayoutUtils.ContentAlignmentToIndex(alignment)];

            private static TextImageRelation TextAlignToRelation(ContentAlignment alignment)
                => LayoutUtils.GetOppositeTextImageRelation(ImageAlignToRelation(alignment));

            #endregion

            #region Instance Methods

            #region Internal Methods

            internal Size GetPreferredSizeCore(Graphics g, Size proposedSize)
            {
                // Get space required for border and padding
                //
                int linearBorderAndPadding = BorderSize * 2 + PaddingSize * 2;
                if (GrowBorderBy1PxWhenDefault)
                    linearBorderAndPadding += 2;
                Size bordersAndPadding = new Size(linearBorderAndPadding, linearBorderAndPadding);
                proposedSize -= bordersAndPadding;

                // Get space required for Check
                //
                int checkSizeLinear = FullCheckSize;
                Size checkSize = checkSizeLinear > 0 ? new Size(checkSizeLinear + 1, checkSizeLinear) : Size.Empty;

                // Get space required for Image - textImageInset compensated for by expanding image.
                //
                Size textImageInsetSize = new Size(textImageInset * 2, textImageInset * 2);
                Size requiredImageSize = (ImageSize != Size.Empty) ? ImageSize + textImageInsetSize : Size.Empty;

                // Pack Text into remaning space
                //
                proposedSize -= textImageInsetSize;
                proposedSize = Decompose(checkSize, requiredImageSize, proposedSize);

                Size textSize = Size.Empty;

                if (!string.IsNullOrEmpty(Text))
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
                Size requiredSize = Compose(checkSize, ImageSize, textSize);
                requiredSize += bordersAndPadding;

                return requiredSize;
            }

            internal LayoutData Layout(Graphics g)
            {
                LayoutData layout = new LayoutData(this);
                layout.Client = Client;

                // subtract border size from layout area
                int fullBorderSize = FullBorderSize;
                layout.Face = Rectangle.Inflate(layout.Client, -fullBorderSize, -fullBorderSize);

                // checkBounds, checkArea, field
                //
                CalcCheckmarkRectangle(layout);

                // imageBounds, imageLocation, textBounds
                LayoutTextAndImage(g, layout);

                // focus
                layout.FocusWidth = ForceDoubleFocusWidth || Scale.X >= 1.5f ? 2 : 1;
                if (MaxFocus)
                {
                    layout.Focus = layout.Field;
                    layout.Focus.Inflate(-1, -1);

                    // Adjust for padding.
                    layout.Focus = LayoutUtils.InflateRect(layout.Focus, Padding);
                }
                else
                {
                    Rectangle textAdjusted = new Rectangle(layout.TextBounds.X - 1, layout.TextBounds.Y - 1,
                            layout.TextBounds.Width + 2, layout.TextBounds.Height + 3);
                    layout.Focus = ImageSize != Size.Empty
                        ? Rectangle.Union(textAdjusted, layout.ImageBounds)
                        : textAdjusted;
                }
                if (FocusOddEvenFixup)
                {
                    if (layout.Focus.Height % 2 == 0)
                    {
                        layout.Focus.Y++;
                        layout.Focus.Height--;
                    }
                    if (layout.Focus.Width % 2 == 0)
                    {
                        layout.Focus.X++;
                        layout.Focus.Width--;
                    }
                }


                return layout;
            }

            internal ContentAlignment RtlTranslateContent(ContentAlignment align)
            {
                if (LayoutRtl)
                {
                    ContentAlignment[][] mapping = new ContentAlignment[3][];
                    mapping[0] = new[] { ContentAlignment.TopLeft, ContentAlignment.TopRight };
                    mapping[1] = new[] { ContentAlignment.MiddleLeft, ContentAlignment.MiddleRight };
                    mapping[2] = new[] { ContentAlignment.BottomLeft, ContentAlignment.BottomRight };

                    for (int i = 0; i < 3; ++i)
                    {
                        if (mapping[i][0] == align)
                            return mapping[i][1];
                        if (mapping[i][1] == align)
                            return mapping[i][0];
                    }
                }
                return align;
            }

            internal void LayoutTextAndImage(Graphics g, LayoutData layout)
            {
                // Translate for Rtl applications.  This intentially shadows the member variables.
                ContentAlignment imageAlign = RtlTranslateContent(this.ImageAlign);
                ContentAlignment textAlign = RtlTranslateContent(this.TextAlign);
                TextImageRelation textImageRelation = RtlTranslateRelation(this.TextImageRelation);

                // Figure out the maximum bounds for text & image
                Rectangle maxBounds = Rectangle.Inflate(layout.Field, -textImageInset, -textImageInset);

                // Change to original: not altering the maxBounds for thicker borders because it could cause the text and the image to shift one pixel depending on the button is focused.
                //if (OnePixExtraBorder)
                //    maxBounds.Inflate(1, 1);

                // Compute the final image and text bounds.
                if (ImageSize == Size.Empty || Text == null || Text.Length == 0 || textImageRelation == TextImageRelation.Overlay)
                {
                    // Do not worry about text/image overlaying
                    Size textSize = GetTextSize(g, maxBounds.Size);

                    // For .NET Framework 1.1 compatibility
                    Size size = ImageSize;
                    if (layout.Options.DotNetOneButtonCompat && ImageSize != Size.Empty)
                        size = new Size(size.Width + 1, size.Height + 1);

                    layout.ImageBounds = LayoutUtils.Align(size, maxBounds, imageAlign);
                    layout.TextBounds = LayoutUtils.Align(textSize, maxBounds, textAlign);

                }
                else
                {
                    // Rearrage text/image to prevent overlay.  Pack text into maxBounds - space reserved for image
                    Size maxTextSize = LayoutUtils.SubAlignedRegion(maxBounds.Size, ImageSize, textImageRelation);
                    Size textSize = GetTextSize(g, maxTextSize);
                    Rectangle maxCombinedBounds = maxBounds;

                    // Combine text & image into one rectangle that we center within maxBounds.
                    Size combinedSize = LayoutUtils.AddAlignedRegion(textSize, ImageSize, textImageRelation);
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
                        LayoutUtils.SplitRegion(maxCombinedBounds, ImageSize, (AnchorStyles)textImageRelation, out layout.ImageBounds, out layout.TextBounds);
                    }
                    else if (textEdge)
                    {
                        // Else if textEdge, just split textSize off of maxCombinedBounds.
                        LayoutUtils.SplitRegion(maxCombinedBounds, textSize, (AnchorStyles)LayoutUtils.GetOppositeTextImageRelation(textImageRelation), out layout.TextBounds, out layout.ImageBounds);
                    }
                    else
                    {
                        // Expand the adjacent regions to maxCombinedBounds (centered) and split the rectangle into imageBounds and textBounds.
                        LayoutUtils.SplitRegion(combinedBounds, ImageSize, (AnchorStyles)textImageRelation, out layout.ImageBounds, out layout.TextBounds);
                        LayoutUtils.ExpandRegionsToFillBounds(maxCombinedBounds, (AnchorStyles)textImageRelation, ref layout.ImageBounds, ref layout.TextBounds);
                    }

                    // align text/image within their regions.
                    layout.ImageBounds = LayoutUtils.Align(ImageSize, layout.ImageBounds, imageAlign);
                    layout.TextBounds = LayoutUtils.Align(textSize, layout.TextBounds, textAlign);
                }

                //Don't call "layout.imageBounds = Rectangle.Intersect(layout.imageBounds, maxBounds);"
                // because that is a breaking change that causes images to be scaled to the dimensions of the control.
                //adjust textBounds so that the text is still visible even if the image is larger than the button's size

                //why do we intersect with layout.field for textBounds while we intersect with maxBounds for imageBounds?
                //this is because there are some legacy code which squeezes the button so small that text will get clipped
                //if we intersect with maxBounds. Have to do this for backward compatibility.
                if (textImageRelation == TextImageRelation.TextBeforeImage || textImageRelation == TextImageRelation.ImageBeforeText)
                {
                    //adjust the vertical position of textBounds so that the text doesn't fall off the boundary of the button
                    int textBottom = Math.Min(layout.TextBounds.Bottom, layout.Field.Bottom);
                    layout.TextBounds.Y = Math.Max(Math.Min(layout.TextBounds.Y, layout.Field.Y + (layout.Field.Height - layout.TextBounds.Height) / 2), layout.Field.Y);
                    layout.TextBounds.Height = textBottom - layout.TextBounds.Y;
                }
                if (textImageRelation == TextImageRelation.TextAboveImage || textImageRelation == TextImageRelation.ImageAboveText)
                {
                    //adjust the horizontal position of textBounds so that the text doesn't fall off the boundary of the button
                    int textRight = Math.Min(layout.TextBounds.Right, layout.Field.Right);
                    layout.TextBounds.X = Math.Max(Math.Min(layout.TextBounds.X, layout.Field.X + (layout.Field.Width - layout.TextBounds.Width) / 2), layout.Field.X);
                    layout.TextBounds.Width = textRight - layout.TextBounds.X;
                }
                if (textImageRelation == TextImageRelation.ImageBeforeText && layout.ImageBounds.Size.Width != 0)
                {
                    //squeezes imageBounds.Width so that text is visible
                    layout.ImageBounds.Width = Math.Max(0, Math.Min(maxBounds.Width - layout.TextBounds.Width, layout.ImageBounds.Width));
                    layout.TextBounds.X = layout.ImageBounds.X + layout.ImageBounds.Width;
                }
                if (textImageRelation == TextImageRelation.ImageAboveText && layout.ImageBounds.Size.Height != 0)
                {
                    //squeezes imageBounds.Height so that the text is visible
                    layout.ImageBounds.Height = Math.Max(0, Math.Min(maxBounds.Height - layout.TextBounds.Height, layout.ImageBounds.Height));
                    layout.TextBounds.Y = layout.ImageBounds.Y + layout.ImageBounds.Height;
                }
                //make sure that textBound is contained in layout.field
                layout.TextBounds = Rectangle.Intersect(layout.TextBounds, layout.Field);
                if (HintTextUp)
                    layout.TextBounds.Y--;
                if (TextOffset)
                    layout.TextBounds.Offset(1, 1);

                // For .NET Framework 1.1 compatibility.
                if (layout.Options.DotNetOneButtonCompat)
                {
                    layout.ImageStart = layout.ImageBounds.Location;
                    layout.ImageBounds = Rectangle.Intersect(layout.ImageBounds, layout.Field);
                }
                else if (!VisualStyleHelper.RenderWithVisualStyles)
                {
                    // Not sure why this is here, but we can't remove it, since it might break
                    // ToolStrips on non-themed machines
                    layout.TextBounds.X++;
                }

                // clip
                //
                int bottom;
                // If we are using GDI to measure text, then we can get into a situation, where
                // the proposed height is ignore. In this case, we want to clip it against
                if (!UseCompatibleTextRendering)
                {
                    bottom = Math.Min(layout.TextBounds.Bottom, maxBounds.Bottom);
                    layout.TextBounds.Y = Math.Max(layout.TextBounds.Y, maxBounds.Y);
                }
                else
                {
                    // If we are using GDI+ (like .NET Framework 1.1), then use the old code
                    // This ensures that we have pixel-level rendering compatibility
                    bottom = Math.Min(layout.TextBounds.Bottom, layout.Field.Bottom);
                    layout.TextBounds.Y = Math.Max(layout.TextBounds.Y, layout.Field.Y);
                }

                layout.TextBounds.Height = bottom - layout.TextBounds.Y;

                // Difference from original: the image is shifted just like the text.
                if (!TextOffset)
                    layout.ImageStart.Offset(-1, -1);
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
                proposedSize = LayoutUtils.FlipSizeIf(VerticalText, proposedSize);
                Size textSize = Size.Empty;

                if (UseCompatibleTextRendering)
                {
                    // GDI+ text rendering.
                    //using (Graphics g = WindowsFormsUtils.CreateMeasurementGraphics())
                    //{
                    using StringFormat gdipStringFormat = StringFormat;
                    textSize = Size.Ceiling(g.MeasureString(Text, Font, new SizeF(proposedSize.Width, proposedSize.Height), gdipStringFormat));
                    //}
                }
                else if (!string.IsNullOrEmpty(Text))
                { // GDI text rendering (.NET Framework 2.0 feature).
                    textSize = TextRenderer.MeasureText(g, Text, Font, proposedSize, TextFormatFlags);
                }
                //else skip calling MeasureText, it should return 0,0

                return LayoutUtils.FlipSizeIf(VerticalText, textSize);

            }

            #endregion

            #region Private Methods

            private Size Compose(Size checkSize, Size imageSize, Size textSize)
            {
                Composition hComposition = GetHorizontalComposition();
                Composition vComposition = GetVerticalComposition();
                return new Size(
                    XCompose(hComposition, checkSize.Width, imageSize.Width, textSize.Width),
                    XCompose(vComposition, checkSize.Height, imageSize.Height, textSize.Height)
                );
            }

            private int XCompose(Composition composition, int checkSize, int imageSize, int textSize)
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
                        Debug.Fail(composition.ToString());
                        return -7107;
                }
            }

            private Size Decompose(Size checkSize, Size imageSize, Size proposedSize)
            {
                Composition hComposition = GetHorizontalComposition();
                Composition vComposition = GetVerticalComposition();
                return new Size(
                    XDecompose(hComposition, checkSize.Width, imageSize.Width, proposedSize.Width),
                    XDecompose(vComposition, checkSize.Height, imageSize.Height, proposedSize.Height)
                );
            }

            private int XDecompose(Composition composition, int checkSize, int imageSize, int proposedSize)
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
                        Debug.Fail(composition.ToString());
                        return -7109;
                }
            }

            private Composition GetHorizontalComposition()
            {
                BitVector32 action = new BitVector32();

                // Checks reserve space horizontally if possible, so only AnyLeft/AnyRight prevents combination.
                action[combineCheck] = CheckAlign == ContentAlignment.MiddleCenter || !LayoutUtils.IsHorizontalAlignment(CheckAlign);
                action[combineImageText] = !LayoutUtils.IsHorizontalRelation(TextImageRelation);
                return (Composition)action.Data;
            }

            private Composition GetVerticalComposition()
            {
                BitVector32 action = new BitVector32();

                // Checks reserve space horizontally if possible, so only Top/Bottom prevents combination.
                action[combineCheck] = CheckAlign == ContentAlignment.MiddleCenter || !LayoutUtils.IsVerticalAlignment(CheckAlign);
                action[combineImageText] = !LayoutUtils.IsVerticalRelation(TextImageRelation);
                return (Composition)action.Data;
            }

            TextImageRelation RtlTranslateRelation(TextImageRelation relation)
            {
                // If RTL, we swap ImageBeforeText and TextBeforeImage
                if (LayoutRtl)
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
                layout.CheckBounds = new Rectangle(Client.X, Client.Y, checkSizeFull, checkSizeFull);

                // Translate checkAlign for Rtl applications
                ContentAlignment align = RtlTranslateContent(CheckAlign);

                Rectangle field = Rectangle.Inflate(layout.Face, -PaddingSize, -PaddingSize);

                layout.Field = field;

                if (checkSizeFull > 0)
                {
                    if (align.AnyRight())
                        layout.CheckBounds.X = (field.X + field.Width) - layout.CheckBounds.Width;
                    else if (align.AnyCenter())
                        layout.CheckBounds.X = field.X + (field.Width - layout.CheckBounds.Width) / 2;

                    if (align.AnyBottom())
                        layout.CheckBounds.Y = (field.Y + field.Height) - layout.CheckBounds.Height;
                    else if (align.AnyTop())
                        layout.CheckBounds.Y = field.Y + 2; // + 2: this needs to be aligned to the text (bug 87483)
                    else
                        layout.CheckBounds.Y = field.Y + (field.Height - layout.CheckBounds.Height) / 2;

                    switch (align)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            layout.CheckArea.X = field.X;
                            layout.CheckArea.Width = checkSizeFull + 1;

                            layout.CheckArea.Y = field.Y;
                            layout.CheckArea.Height = field.Height;

                            layout.Field.X += checkSizeFull + 1;
                            layout.Field.Width -= checkSizeFull + 1;
                            break;
                        case ContentAlignment.TopRight:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.BottomRight:
                            layout.CheckArea.X = field.X + field.Width - checkSizeFull;
                            layout.CheckArea.Width = checkSizeFull + 1;

                            layout.CheckArea.Y = field.Y;
                            layout.CheckArea.Height = field.Height;

                            layout.Field.Width -= checkSizeFull + 1;
                            break;
                        case ContentAlignment.TopCenter:
                            layout.CheckArea.X = field.X;
                            layout.CheckArea.Width = field.Width;

                            layout.CheckArea.Y = field.Y;
                            layout.CheckArea.Height = checkSizeFull;

                            layout.Field.Y += checkSizeFull;
                            layout.Field.Height -= checkSizeFull;
                            break;

                        case ContentAlignment.BottomCenter:
                            layout.CheckArea.X = field.X;
                            layout.CheckArea.Width = field.Width;

                            layout.CheckArea.Y = field.Y + field.Height - checkSizeFull;
                            layout.CheckArea.Height = checkSizeFull;

                            layout.Field.Height -= checkSizeFull;
                            break;

                        case ContentAlignment.MiddleCenter:
                            layout.CheckArea = layout.CheckBounds;
                            break;
                    }

                    layout.CheckBounds.Width -= CheckPaddingSize;
                    layout.CheckBounds.Height -= CheckPaddingSize;
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

            internal Rectangle Client;
            internal Rectangle Face;
            internal Rectangle CheckArea;
            internal Rectangle CheckBounds;
            internal Rectangle TextBounds;
            internal Rectangle Field;
            internal Rectangle Focus;
            internal Rectangle ImageBounds;
            internal Point ImageStart;
            internal int FocusWidth;
            internal LayoutOptions Options;

            #endregion

            #region Constructors

            internal LayoutData(LayoutOptions options) => Options = options;

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly ButtonBase control;

        #endregion

        #region Properties

        protected virtual int ButtonBorderSize => 4;

        protected ButtonBase ButtonInstance => control;

        #endregion

        #region Constructors

        internal ButtonBaseAdapter(ButtonBase control) => this.control = control;

        #endregion

        #region Methods

        #region Static Methods

        protected static Brush CreateDitherBrush(Color color1, Color color2)
        {
            // Note: Don't dispose the bitmap here. The texture brush will take ownership
            // of the bitmap. So the bitmap will get disposed by the brush's Dispose().
            using Bitmap b = new Bitmap(2, 2);
            b.SetPixel(0, 0, color1);
            b.SetPixel(0, 1, color2);
            b.SetPixel(1, 1, color1);
            b.SetPixel(1, 0, color2);

            return new TextureBrush(b);
        }

        protected static void DrawDitheredFill(Graphics g, Color color1, Color color2, Rectangle bounds)
        {
            using Brush brush = CreateDitherBrush(color1, color2);
            g.FillRectangle(brush, bounds);
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
            Pen pen = (up ? colors.Highlight : colors.ButtonShadow).GetPen();
            g.DrawLine(pen, p1, p2); // top (right-left)
            g.DrawLine(pen, p2, p3); // left (top-down)

            // bottom, right
            pen = (up ? colors.ButtonShadow : colors.Highlight).GetPen();
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(pen, p3, p4); // bottom (left-right)
            g.DrawLine(pen, p4, p1); // right(bottom-up)
        }

        protected static void DrawFlatBorder(Graphics g, Rectangle r, Color c) => ControlPaint.DrawBorder(g, r, c, ButtonBorderStyle.Solid);

        protected static void DrawDefaultBorder(Graphics g, Rectangle r, Color c, bool isDefault)
        {
            if (isDefault)
            {
                r.Inflate(1, 1);
                g.DrawRectangle(c.GetPen(), r.X, r.Y, r.Width - 1, r.Height - 1);
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
                PaintDown(e);
            else if (e.State.Hovered)
                PaintOver(e);
            else
                PaintUp(e);
        }

        internal virtual Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
            => Layout(g, state).GetPreferredSizeCore(g, proposedSize);

        internal abstract void PaintUp(PaintStateEventArgs e);

        internal abstract void PaintDown(PaintStateEventArgs e);

        internal abstract void PaintOver(PaintStateEventArgs e);

        internal virtual LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions layout = new LayoutOptions();
            layout.Client = LayoutUtils.DeflateRect(control.ClientRectangle, control.Padding);
            layout.Padding = control.Padding;
            layout.Scale = control.GetScale();
            layout.GrowBorderBy1PxWhenDefault = true;
            layout.IsDefault = state.IsDefault;
            layout.BorderSize = 2;
            layout.PaddingSize = 0;
            layout.MaxFocus = true;
            layout.FocusOddEvenFixup = false;
            layout.Font = control.Font;
            layout.Text = state.Text;
            layout.ImageSize = control.Image?.Size ?? Size.Empty;
            layout.CheckSize = 0;
            layout.CheckPaddingSize = 0;
            layout.CheckAlign = ContentAlignment.TopLeft;
            layout.ImageAlign = control.ImageAlign;
            layout.TextAlign = control.TextAlign;
            layout.HintTextUp = false;
            layout.ShadowedText = !state.Enabled;
            layout.LayoutRtl = RightToLeft.Yes == control.RightToLeft;
            layout.TextImageRelation = control.TextImageRelation;
            layout.UseCompatibleTextRendering = control.UseCompatibleTextRendering;

            if (control.FlatStyle != FlatStyle.System)
            {
                if (layout.UseCompatibleTextRendering)
                {
                    using StringFormat format = control.GetFormatFlags().ToStringFormat();
                    layout.StringFormat = format;
                }
                else
                    layout.GdiTextFormatFlags = control.GetFormatFlags();
            }

            return layout;
        }

        #endregion

        #region Protected Methods

        protected abstract LayoutOptions Layout(Graphics graphics, ControlAppearanceState state);

        protected void PaintButtonBackground(PaintEventArgs e, Rectangle bounds, Color backColor)
            => e.Graphics.FillRectangle(backColor.GetBrush(), bounds);

        protected void PaintField(PaintStateEventArgs e, LayoutData layout, ColorData colors, bool drawFocus)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            DrawText(g, layout, colors, state);
            if (drawFocus)
                DrawFocus(g, layout, state);
        }

        protected void PaintImage(PaintStateEventArgs e, LayoutData layout)
        {
            if (control.Image != null)
            {
                //setup new clip region & draw
                DrawImageCore(e.Graphics, control.Image, layout.ImageBounds, layout.ImageStart, layout, e.State);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draws the focus rectangle if the control has focus.
        /// </summary>
        private void DrawFocus(Graphics g, LayoutData layout, ControlAppearanceState state)
        {
            if (!control.Focused || !((ISupportButtonAdapter)control).ShowFocusCues)
                return;

            Rectangle r = layout.Focus;
            for (int i = 0; i < layout.FocusWidth; i++)
            {
                ControlPaint.DrawFocusRectangle(g, r, state.ForeColor, state.BackColor);
                r.Inflate(-1, -1);
            }
        }

        private void DrawImageCore(Graphics graphics, Image image, Rectangle imageBounds, Point imageStart, LayoutData layout, ControlAppearanceState state)
        {
            Region oldClip = graphics.Clip;

            if (!layout.Options.DotNetOneButtonCompat)
            {
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
                imageBounds.Width += 1;
                imageBounds.Height += 1;
                imageBounds.X = imageStart.X + 1;
                imageBounds.Y = imageStart.Y + 1;
            }

            try
            {
                if (!state.Enabled)
                {
                    // this always creates a new bitmap, but this is what happens also in the original ControlPaint.DrawImageDisabled
                    // when the internal overload is called with unscaledImage = true
                    using var disabledImage = image.ToGrayscale();
                    graphics.DrawImage(disabledImage, imageBounds.X, imageBounds.Y, disabledImage.Width, disabledImage.Height);
                }
                else
                    graphics.DrawImage(image, imageBounds.X, imageBounds.Y, image.Width, image.Height);
            }

            finally
            {
                if (!layout.Options.DotNetOneButtonCompat)
                    graphics.Clip = oldClip;
            }
        }

        /// <summary>
        /// Draws the button's text.
        /// </summary>
        void DrawText(Graphics g, LayoutData layout, ColorData colors, ControlAppearanceState state)
        {
            Rectangle r = layout.TextBounds;
            bool disabledText3D = layout.Options.ShadowedText;

            if (control.UseCompatibleTextRendering)
            {
                // Draw text using GDI+
                using StringFormat stringFormat = control.GetFormatFlags().ToStringFormat();
                // DrawString doesn't seem to draw where it says it does
                if (control.TextAlign.AnyCenter())
                    r.X -= 1;
                r.Width += 1;

                if (disabledText3D && !state.Enabled)
                {
                    r.Offset(1, 1);
                    g.DrawString(state.Text, control.Font, colors.Highlight.GetBrush(), r, stringFormat);

                    r.Offset(-1, -1);
                    g.DrawString(state.Text, control.Font, state.ForeColor.GetBrush(), r, stringFormat);
                }
                else
                    g.DrawString(state.Text, control.Font, state.ForeColor.GetBrush(), r, stringFormat);
            }
            else
            {
                // Draw text using GDI (.NET Framework 2.0+ feature).
                TextFormatFlags formatFlags = control.GetFormatFlags();

                if (disabledText3D && !state.Enabled)
                {
                    Color disabledColor = state.ForeColor; // here: DisabledForeColor
                    if (VisualStyleHelper.RenderWithVisualStyles)
                    {
                        //don't draw chiseled text if themed as win32 app does.
                        TextRenderer.DrawText(g, state.Text, control.Font, r, disabledColor, formatFlags);
                    }
                    else
                    {
                        r.Offset(1, 1);
                        TextRenderer.DrawText(g, state.Text, control.Font, r, colors.Highlight, formatFlags);

                        r.Offset(-1, -1);
                        TextRenderer.DrawText(g, state.Text, control.Font, r, disabledColor, formatFlags);
                    }
                }
                else
                    TextRenderer.DrawText(g, state.Text, control.Font, r, state.ForeColor, formatFlags);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
