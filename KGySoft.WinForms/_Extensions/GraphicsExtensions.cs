using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace KGySoft.WinForms
{
    internal static class GraphicsExtensions
    {
        internal static void SetQuality(this Graphics graphics)
        {
            graphics.TextContrast = 4;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            //graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }
    }
}
