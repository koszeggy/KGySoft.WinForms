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

using KGySoft.WinForms.Controls;

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

        /// <summary>
        /// Applies settings that ensure the same result for Draw/Fill operations on every platform, regardless of smoothing mode, RTL layout or zooming.
        /// </summary>
        /// <param name="g">The graphics. PixelOffsetMode should not be changed after the call.</param>
        /// <param name="drawOffset">The offset to be applied to Draw operations where normally use integer coordinates would be used.</param>
        internal static void EnsureCrossPlatformCorrectness(this Graphics g, out float drawOffset)
        {
            if (OSHelper.IsRealWindows || OSHelper.IsWindowsMono)
            {
                // PixelOffsetMode.Half + 0.5 drawOffset for Graphics.Draw* solves the offset issues between RTL/LTR on Windows
                g.PixelOffsetMode = PixelOffsetMode.Half;
                drawOffset = 0.5f;
                return;
            }

            // Wine or Mono on non-Windows platforms
            drawOffset = 0f;
            g.PixelOffsetMode = PixelOffsetMode.None;
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

        internal static void DrawImageColorized(this Graphics graphics, Image image, Rectangle destRect, Color targetColor)
        {
            ImageAttributes? attr = null;
            try
            {
                if (targetColor.ToArgb() != Color.Black.ToArgb())
                {
                    attr = new ImageAttributes();
                    var map = new ColorMap { OldColor = Color.Black, NewColor = targetColor };
                    attr.SetRemapTable([map], ColorAdjustType.Bitmap);
                }

                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
            }
            finally
            {
                attr?.Dispose();
            }
        }

        internal static void DrawImageColorizedAlpha(this Graphics graphics, Image image, Rectangle destRect, Color targetColor)
        {
            ImageAttributes? attr = null;
            try
            {
                if (targetColor.ToArgb() != Color.Black.ToArgb())
                {
                    attr = new ImageAttributes();
                    var colorMatrix = new ColorMatrix(new float[][]
                    {
                        // Using the identity matrix for the RGBA multipliers, and the target color RGB for the added values.
                        // This works if the original color is black.
                        new float[] { 1f, 0f, 0f, 0f, 0f },
                        new float[] { 0f, 1f, 0f, 0f, 0f },
                        new float[] { 0f, 0f, 1f, 0f, 0f },
                        new float[] { 0f, 0f, 0f, 1f, 0f },
                        new float[] { targetColor.R / 255f, targetColor.G / 255f, targetColor.B / 255f, 0, 1f }
                    });

                    attr.SetColorMatrix(colorMatrix);
                }

                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
            }
            finally
            {
                attr?.Dispose();
            }
        }

        #endregion
    }
}