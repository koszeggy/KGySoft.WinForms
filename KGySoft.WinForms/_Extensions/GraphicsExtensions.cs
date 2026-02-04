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

        #endregion
    }
}