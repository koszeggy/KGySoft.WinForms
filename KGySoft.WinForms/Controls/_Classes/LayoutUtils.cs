#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LayoutUtils.cs
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
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Mainly decompiled code from the <c>LayoutUtils</c> class of the .NET Framework. Used by the reimplemented adapters.
    /// </summary>
    internal static class LayoutUtils
    {
        #region Methods

        #region Public Methods

        public static Size AddAlignedRegion(Size textSize, Size imageSize, TextImageRelation relation)
            => AddAlignedRegionCore(textSize, imageSize, IsVerticalRelation(relation));

        public static Size AddAlignedRegionCore(Size currentSize, Size contentSize, bool vertical)
        {
            if (vertical)
            {
                currentSize.Width = Math.Max(currentSize.Width, contentSize.Width);
                currentSize.Height += contentSize.Height;
                return currentSize;
            }

            currentSize.Width += contentSize.Width;
            currentSize.Height = Math.Max(currentSize.Height, contentSize.Height);
            return currentSize;
        }

        public static Rectangle Align(Size alignThis, Rectangle withinThis, ContentAlignment align)
            => VAlign(alignThis, HAlign(alignThis, withinThis, align), align);

        public static int ContentAlignmentToIndex(ContentAlignment alignment)
        {
            int num = ContentAlignmentToIndex(((int)alignment) & 15);
            int num2 = ContentAlignmentToIndex((((int)alignment) >> 4) & 15);
            int num3 = ContentAlignmentToIndex((((int)alignment) >> 8) & 15);
            int num4 = (((((num2 != 0) ? 4 : 0) | ((num3 != 0) ? 8 : 0)) | num) | num2) | num3;
            num4--;
            return num4;
        }

        public static Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

        public static void ExpandRegionsToFillBounds(Rectangle bounds, AnchorStyles region1Align, ref Rectangle region1, ref Rectangle region2)
        {
            switch (region1Align)
            {
                case AnchorStyles.Top:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Bottom);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Top);
                    return;

                case AnchorStyles.Bottom:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Top);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Bottom);
                    break;

                case AnchorStyles.Bottom | AnchorStyles.Top:
                    break;

                case AnchorStyles.Left:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Right);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Left);
                    return;

                case AnchorStyles.Right:
                    region1 = SubstituteSpecifiedBounds(bounds, region1, AnchorStyles.Left);
                    region2 = SubstituteSpecifiedBounds(bounds, region2, AnchorStyles.Right);
                    return;

                default:
                    return;
            }
        }

        public static Size FlipSize(Size size)
        {
            (size.Width, size.Height) = (size.Height, size.Width);
            return size;
        }

        public static Size FlipSizeIf(bool condition, Size size) => !condition ? size : FlipSize(size);

        public static TextImageRelation GetOppositeTextImageRelation(TextImageRelation relation) => (TextImageRelation)GetOppositeAnchor((AnchorStyles)relation);

        public static Rectangle InflateRect(Rectangle rect, Padding padding)
        {
            rect.X -= padding.Left;
            rect.Y -= padding.Top;
            rect.Width += padding.Horizontal;
            rect.Height += padding.Vertical;
            return rect;
        }

        public static bool IsHorizontalAlignment(ContentAlignment align) => !IsVerticalAlignment(align);

        public static bool IsHorizontalRelation(TextImageRelation relation)
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            return ((relation & (TextImageRelation.TextBeforeImage | TextImageRelation.ImageBeforeText)) != TextImageRelation.Overlay);
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
        }

        public static bool IsVerticalAlignment(ContentAlignment align)
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            return ((align & (ContentAlignment.BottomCenter | ContentAlignment.TopCenter)) != ((ContentAlignment)0));
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
        }

        public static bool IsVerticalRelation(TextImageRelation relation)
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            return ((relation & (TextImageRelation.TextAboveImage | TextImageRelation.ImageAboveText)) != TextImageRelation.Overlay);
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
        }

        public static void SplitRegion(Rectangle bounds, Size specifiedContent, AnchorStyles region1Align, out Rectangle region1, out Rectangle region2)
        {
            region1 = region2 = bounds;
            switch (region1Align)
            {
                case AnchorStyles.Top:
                    region1.Height = specifiedContent.Height;
                    region2.Y += specifiedContent.Height;
                    region2.Height -= specifiedContent.Height;
                    return;

                case AnchorStyles.Bottom:
                    region1.Y += bounds.Height - specifiedContent.Height;
                    region1.Height = specifiedContent.Height;
                    region2.Height -= specifiedContent.Height;
                    break;

                case (AnchorStyles.Bottom | AnchorStyles.Top):
                    break;

                case AnchorStyles.Left:
                    region1.Width = specifiedContent.Width;
                    region2.X += specifiedContent.Width;
                    region2.Width -= specifiedContent.Width;
                    return;

                case AnchorStyles.Right:
                    region1.X += bounds.Width - specifiedContent.Width;
                    region1.Width = specifiedContent.Width;
                    region2.Width -= specifiedContent.Width;
                    return;

                default:
                    return;
            }
        }

        public static Size SubAlignedRegion(Size currentSize, Size contentSize, TextImageRelation relation)
            => SubAlignedRegionCore(currentSize, contentSize, IsVerticalRelation(relation));

        public static Size SubAlignedRegionCore(Size currentSize, Size contentSize, bool vertical)
        {
            if (vertical)
            {
                currentSize.Height -= contentSize.Height;
                return currentSize;
            }

            currentSize.Width -= contentSize.Width;
            return currentSize;
        }

        public static Size UnionSizes(Size a, Size b) => new(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));

        public static Rectangle VAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            if ((align & (ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft)) != ((ContentAlignment)0))
            {
                withinThis.Y += withinThis.Height - alignThis.Height;
            }
            else if ((align & (ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft)) != ((ContentAlignment)0))
            {
                withinThis.Y += (withinThis.Height - alignThis.Height) / 2;
            }
            withinThis.Height = alignThis.Height;
            return withinThis;
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
        }

        #endregion

        #region Private Methods

        private static AnchorStyles GetOppositeAnchor(AnchorStyles anchor)
        {
            AnchorStyles none = AnchorStyles.None;
            if (anchor != AnchorStyles.None)
            {
                for (int i = 1; i <= 8; i = i << 1)
                {
                    switch ((anchor & (AnchorStyles)i))
                    {
                        case AnchorStyles.Top:
                            none |= AnchorStyles.Bottom;
                            break;

                        case AnchorStyles.Bottom:
                            none |= AnchorStyles.Top;
                            break;

                        case AnchorStyles.Left:
                            none |= AnchorStyles.Right;
                            break;

                        case AnchorStyles.Right:
                            none |= AnchorStyles.Left;
                            break;
                    }
                }
            }
            return none;
        }

        private static Rectangle HAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            if (align.AnyRight())
                withinThis.X += withinThis.Width - alignThis.Width;
            else if (align.AnyCenter())
                withinThis.X += (withinThis.Width - alignThis.Width) / 2;
            withinThis.Width = alignThis.Width;
            return withinThis;
        }

        private static Rectangle SubstituteSpecifiedBounds(Rectangle originalBounds, Rectangle substitutionBounds, AnchorStyles specified)
        {
            int left = ((specified & AnchorStyles.Left) != AnchorStyles.None) ? substitutionBounds.Left : originalBounds.Left;
            int top = ((specified & AnchorStyles.Top) != AnchorStyles.None) ? substitutionBounds.Top : originalBounds.Top;
            int right = ((specified & AnchorStyles.Right) != AnchorStyles.None) ? substitutionBounds.Right : originalBounds.Right;
            int bottom = ((specified & AnchorStyles.Bottom) != AnchorStyles.None) ? substitutionBounds.Bottom : originalBounds.Bottom;
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private static byte ContentAlignmentToIndex(int threeBitFlag) => ((threeBitFlag == 4) ? ((byte)3) : ((byte)threeBitFlag));

        #endregion

        #endregion
    }
}
