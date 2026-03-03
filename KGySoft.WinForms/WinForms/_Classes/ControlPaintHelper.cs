#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ControlPaintHelper.cs
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

#region Used Namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Reflection;

#endregion

#region Used Aliases

using GdiPen = System.Drawing.Pen;
using KGyPen = KGySoft.Drawing.Shapes.Pen;

#endregion

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides a similar helper class as <see cref="ControlPaint"/>, including some functionality
    /// that is internal there, or has no platform-independent implementation.
    /// </summary>
    internal static class ControlPaintHelper
    {
        #region Nested Types

        private enum ControlElement
        {
            CheckMark,
            ArrowUp,
            ArrowDown,
            CommandLinkArrow,
            CommandLinkArrowRtl,
        }

        #endregion

        #region Fields

        // Need to use a locking cache to be able to use DisposeDroppedValues, but it shouldn't be an issue as we don't expect many concurrent UI threads.
        // Contains black drawings on a transparent background. Use GraphicsExtensions.DrawImageColorized[Alpha] to paint the result with a custom color.
        private static readonly IThreadSafeCacheAccessor<(ControlElement, int, int), Bitmap> bitmapsCache = new Cache<(ControlElement, int, int), Bitmap>(GetBitmap, 8)
        {
            EnsureCapacity = true,
            DisposeDroppedValues = true
        }.GetThreadSafeAccessor();

        #endregion

        #region Methods
        
        #region Internal Methods

        internal static void DrawBorder(this Graphics g, AdvancedBorderStyle borderStyle, Rectangle bounds)
        {
            switch (borderStyle)
            {
                case AdvancedBorderStyle.FixedSingle:
                    g.DrawRectangle(SystemPens.WindowFrame, 0, 0, bounds.Width - 1, bounds.Height - 1);
                    break;
                case AdvancedBorderStyle.Raised:
                case AdvancedBorderStyle.Flat:
                case AdvancedBorderStyle.RaisedHigh:
                case AdvancedBorderStyle.Sunken:
                case AdvancedBorderStyle.SunkenLow:
                    ControlPaint.DrawBorder3D(g, bounds, (Border3DStyle)borderStyle);
                    break;
                case AdvancedBorderStyle.SunkenFrame:
                    ControlPaint.DrawBorder(g, bounds, SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid);
                    ControlPaint.DrawBorder(g, new Rectangle(1, 1, bounds.Width - 2, bounds.Height - 2),
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid);
                    //ControlPaint.DrawBorder3D(g, rect, Border3DStyle.SunkenOuter);
                    //ControlPaint.DrawBorder3D(g, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.RaisedInner);
                    break;
                case AdvancedBorderStyle.RaisedFrame:
                    ControlPaint.DrawBorder(g, bounds, SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid);
                    ControlPaint.DrawBorder(g, new Rectangle(1, 1, bounds.Width - 2, bounds.Height - 2),
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                        SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid);
                    //ControlPaint.DrawBorder3D(g, rect, Border3DStyle.RaisedInner);
                    //ControlPaint.DrawBorder3D(g, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.SunkenOuter);
                    break;
            }
        }

        internal static void DrawBackgroundImage(this Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (g.TryDrawBackgroundImage(backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft))
                return;

            // If we reach this point, we are most likely on Mono, so mainly using its solutions to be conform with its own behavior elsewhere
            // (e.g. when a transparent background is also painted, which calls OnPaintBackground, potentially drawing background images using Mono's own logic).

            // filling with backColor, except for an opaque tiled image
            if (backColor.A != 0 && (backgroundImageLayout != ImageLayout.Tile || (backgroundImage.Flags & (int)ImageFlags.HasAlpha) != 0))
                g.FillRectangle(backColor.GetBrush(), clipRect);

            var imageBounds = new Rectangle();
            switch (backgroundImageLayout)
            {
                case ImageLayout.Tile:
                    using (var brush = new TextureBrush(backgroundImage, WrapMode.Tile)) // NOTE: ignoring scrollOffset here
                    {
                        g.FillRectangle(brush, clipRect);

                        if (scrollOffset != Point.Empty)
                        {
                            Matrix transform = brush.Transform;
                            transform.Translate(scrollOffset.X, scrollOffset.Y);
                            brush.Transform = transform;
                        }
                    }

#if NET5_0_OR_GREATER
                    // Workaround for https://github.com/dotnet/winforms/issues/13784, because the texture brush resets the HDC offset origin
                    g.GetHdc();
                    g.ReleaseHdc();
#endif

                    return;

                case ImageLayout.Center:
                    imageBounds.Location = new Point(bounds.Width / 2 - backgroundImage.Width / 2, bounds.Height / 2 - backgroundImage.Height / 2);
                    imageBounds.Size = backgroundImage.Size;
                    break;

                case ImageLayout.None:
                    imageBounds.Location = Point.Empty;
                    imageBounds.Size = backgroundImage.Size;
                    break;

                case ImageLayout.Stretch:
                    imageBounds = bounds;
                    break;

                case ImageLayout.Zoom:
                    imageBounds = bounds;
                    if (backgroundImage.Width / (float)backgroundImage.Height < imageBounds.Width / (float)imageBounds.Height)
                    {
                        imageBounds.Width = (int)(backgroundImage.Width * (imageBounds.Height / (float)backgroundImage.Height));
                        imageBounds.X = (bounds.Width - imageBounds.Width) / 2;
                    }
                    else
                    {
                        imageBounds.Height = (int)(backgroundImage.Height * (imageBounds.Width / (float)backgroundImage.Width));
                        imageBounds.Y = (bounds.Height - imageBounds.Height) / 2;
                    }

                    break;
            }

            g.DrawImage(backgroundImage, imageBounds);
        }

        internal static void DrawHighContrastFocusRectangle(this Graphics graphics, Rectangle rectangle, Color color)
        {
            if (graphics.TryDrawHighContrastFocusRectangle(rectangle, color))
                return;

            // fallback solution: manually drawing a simple focus rectangle, ignoring such fine details like rounding, etc.
            using GdiPen pen = new(color);
            pen.DashStyle = DashStyle.Dot;
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        }

        internal static Bitmap GetCheckImage(Size checkBoxSize) => bitmapsCache[(ControlElement.CheckMark, checkBoxSize.Width, checkBoxSize.Height)];
        internal static Bitmap GetArrowImage(Size size, bool isUp) => bitmapsCache[(isUp ? ControlElement.ArrowUp : ControlElement.ArrowDown, size.Width, size.Height)];
        internal static Bitmap GetCommandLinkArrowImage(Size size, bool isRightToLeft) => bitmapsCache[(isRightToLeft ? ControlElement.CommandLinkArrowRtl : ControlElement.CommandLinkArrow, size.Width, size.Height)];

        #endregion

        #region Private Methods

        private static Bitmap GetBitmap((ControlElement Element, int Width, int Height) key) => key.Element switch
        {
            ControlElement.CheckMark => GetCheckBitmap(new Size(key.Width, key.Height)),
            ControlElement.ArrowUp or ControlElement.ArrowDown => GetArrowBitmap(new Size(key.Width, key.Height), key.Element),
            ControlElement.CommandLinkArrow or ControlElement.CommandLinkArrowRtl => GetCommandLinkArrow(new Size(key.Width, key.Height), key.Element is ControlElement.CommandLinkArrowRtl),
            _ => throw new InvalidOperationException(Res.InternalError($"Unexpected element: {key.Element}"))
        };

        private static Bitmap GetCheckBitmap(Size checkBoxSize)
        {
            // The original code in CheckBoxBase adapter used User32.DrawFrameControl(hdc, RECT, 2, 1) to paint a check mark.
            (int width, int height) = (checkBoxSize.Width, checkBoxSize.Height);
            var result = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using IReadWriteBitmapData bitmapData = result.GetReadWriteBitmapData();
            int checkHeight = height / 6;
            Color32 c = Color.Black;

            int start = (int)(width * 0.2f);
            int mid = (int)(width * 0.35f);
            int end = (int)(width * 0.7f);
            int y = (int)(height * 0.42f);
            for (int x = start; x < end; x++)
            {
                bitmapData.DrawLine(c, x, y, x, y + checkHeight);
                y += x < mid ? +1 : -1;
            }

            return result;
        }

        private static Bitmap GetArrowBitmap(Size size, ControlElement element)
        {
            Debug.Assert(element is ControlElement.ArrowUp or ControlElement.ArrowDown);
            
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using var bitmapData = result.GetReadWriteBitmapData();
            
            // ensuring odd width
            int width = (int)(Math.Min(size.Width, size.Height) * 0.6f) | 1;
            int height = width / 2 + 1;
            int top = size.Height / 2 - height / 2;
            int left = size.Width / 2 - width / 2;
            Color32 black = Color.Black;

            for (int i = 0; i < height; i++)
            {
                int y = element == ControlElement.ArrowDown ? top + i : top + height - i;
                bitmapData.DrawLine(black, left + i, y, left + width - i - 1, y);
            }

            return result;
        }

        private static Bitmap GetCommandLinkArrow(Size size, bool isRightToLeft)
        {
            // Originally this wad drawn directly into the Graphics, but some platforms behave differently (e.g. Wine does not support antialiasing, or on Mono the offsets are slightly off)
            // So using out managed drawing to provide the same result on all platforms.
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using var bitmapData = result.GetReadWriteBitmapData();
            var options = new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half };

            float unit = size.Width / 20f;
            var pen = new KGyPen(Color.Black, Math.Max(2, 1.5f * unit));
            var y = 12 * unit + 0.5f;
            var x1 = (isRightToLeft ? 7 : 12) * unit;
            var x2 = (isRightToLeft ? 1 : 18) * unit;
            bitmapData.DrawLine(pen, new PointF(unit, y), new PointF(18 * unit, y), options);
            bitmapData.DrawLines(pen, new PointF[] { new(x1, 6 * unit + 0.5f), new(x2, y), new(x1, 18 * unit + 0.5f) }, options);
            return result;
        }


        #endregion

        #endregion
    }
}
