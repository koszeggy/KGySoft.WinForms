using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    /// <summary>
    /// Extension methods for <see cref="Graphics"/> class.
    /// </summary>
    public static class GraphicsTools
    {
        /// <summary>
        /// Sets requested <paramref name="quality"/> for a <see cref="Graphics"/> instance.
        /// </summary>
        /// <param name="graphics">The graphics to set the quality.</param>
        /// <param name="quality">Requested quality.</param>
        /// <param name="useGdiPlusTextRendering"><c>true</c>, when GDI+ is required for text rendering instead of GDI (that is, when <c>UseCompatibleTextRendering</c> is <c>true</c> for a control).</param>
        public static void SetQuality(this Graphics graphics, RenderingQuality quality, bool useGdiPlusTextRendering)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            switch (quality)
            {
                case RenderingQuality.SystemDefault:
                    graphics.SmoothingMode = SmoothingMode.Default;
                    graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                    graphics.InterpolationMode = InterpolationMode.Default;
                    graphics.CompositingQuality = CompositingQuality.Default;
                    //graphics.PixelOffsetMode = PixelOffsetMode.Default;
                    break;
                case RenderingQuality.Low:
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.TextRenderingHint = useGdiPlusTextRendering ? TextRenderingHint.SingleBitPerPixelGridFit : TextRenderingHint.AntiAliasGridFit;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    //graphics.PixelOffsetMode = PixelOffsetMode.None;
                    break;
                case RenderingQuality.Medium:
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = useGdiPlusTextRendering ? TextRenderingHint.AntiAlias : TextRenderingHint.ClearTypeGridFit;
                    graphics.InterpolationMode = InterpolationMode.Bilinear;
                    graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                    //graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    break;
                case RenderingQuality.High:
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("quality");
            }            
        }

        public static Bitmap ToBitmap(this Graphics graphics)
        {
            IntPtr hdc = graphics.GetHdc();
            IntPtr comapitbleDc = Gdi32.CreateCompatibleDC(hdc);
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdc, size.Width, size.Height);
            Gdi32.SelectObject(comapitbleDc, hBitmap);
            e.Graphics.ReleaseHdc(hdc);

            using (Graphics g = Graphics.FromHdc(comapitbleDc))
            {
                Host.PaintState(Host.State, new PaintEventArgs(g, e.ClipRectangle));
                newStateImage = Image.FromHbitmap(hBitmap);
            }

            Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteObject(comapitbleDc);

        }
    }
}
