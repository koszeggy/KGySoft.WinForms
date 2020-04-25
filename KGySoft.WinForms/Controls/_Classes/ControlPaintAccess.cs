using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using KGySoft.Reflection;

namespace KGySoft.WinForms.Controls
{
    internal static class ControlPaintAccess
    {
        private static MethodAccessor methodDrawImageDisabled;
        private static MethodAccessor methodDrawBackgroundImage;
        private static MethodAccessor methodDrawImageColorized;

        internal static void DrawImageDisabled(Graphics graphics, Image image, Rectangle imageBounds, Color background, bool unscaledImage)
        {
            if (methodDrawImageDisabled == null)
                methodDrawImageDisabled = MethodAccessor.GetAccessor(typeof(ControlPaint).GetMethod("DrawImageDisabled", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color), typeof(bool) }, null));

            methodDrawImageDisabled.Invoke(null, graphics, image, imageBounds, background, unscaledImage);
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (methodDrawBackgroundImage == null)
                methodDrawBackgroundImage = MethodAccessor.GetAccessor(typeof(ControlPaint).GetMethod("DrawBackgroundImage", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft) }, null));

            methodDrawBackgroundImage.Invoke(null, g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft);
        }

        internal static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            if (methodDrawImageColorized == null)
                methodDrawImageColorized = MethodAccessor.GetAccessor(typeof(ControlPaint).GetMethod("DrawImageColorized", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color) }, null));

            methodDrawImageColorized.Invoke(null, graphics, image, destination, replaceBlack);
        }
    }
}
