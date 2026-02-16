#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsExtensions.cs
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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Reflection;

#endregion

namespace KGySoft.WinForms
{
    internal static class GraphicsExtensions
    {
        #region Methods

        internal static void SetTextRenderingQuality(this Graphics graphics, RenderingQuality quality, bool isCompatibleTextRendering)
        {
            graphics.TextRenderingHint = quality switch
            {
                RenderingQuality.High => TextRenderingHint.ClearTypeGridFit,
                RenderingQuality.Low => isCompatibleTextRendering ? TextRenderingHint.SingleBitPerPixelGridFit : TextRenderingHint.AntiAlias,
                _ => TextRenderingHint.SystemDefault,
            };
        }

        internal static void DrawImageGrayscale(this Graphics graphics, Image image, Rectangle destRect)
        {
            // Grayscale color matrix
            var colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            using var attrs = new ImageAttributes();
            attrs.SetColorMatrix(colorMatrix);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
        }

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
            if (backColor.A != 0 && (backgroundImageLayout != ImageLayout.Tile || backgroundImage is not Bitmap bmp || (bmp.Flags & (int)ImageFlags.HasAlpha) != 0))
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

        internal static void DrawImageColorized(this Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            if (graphics.TryDrawImageColorized(image, destination, replaceBlack))
                return;

            // fallback solution: manually drawing the recolored image
            Bitmap? recolored = null;
            try
            {
                if (replaceBlack.ToArgb() != Color.Black.ToArgb())
                {
                    recolored = new Bitmap(image);
                    recolored.ReplaceColor(Color.Black, replaceBlack);
                }

                graphics.DrawImage(recolored ?? image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }
            finally
            {
                recolored?.Dispose();
            }
        }

        internal static void DrawHighContrastFocusRectangle(this Graphics graphics, Rectangle rectangle, Color color)
        {
            if (graphics.TryDrawHighContrastFocusRectangle(rectangle, color))
                return;

            // fallback solution: manually drawing a simple focus rectangle, ignoring such fine details like rounding, etc.
            using Pen pen = new(color);
            pen.DashStyle = DashStyle.Dot;
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        }

        #endregion
    }
}