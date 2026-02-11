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
using System.Windows.Forms;

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

        #endregion
    }
}