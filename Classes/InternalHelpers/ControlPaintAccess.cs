using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using MethodInvoker = KGySoft.Reflection.MethodInvoker;

namespace KGySoft.Controls
{
    internal static class ControlPaintAccess
    {
        private static MethodInvoker methodDrawImageDisabled;
        private static MethodInvoker methodDrawBackgroundImage;
        private static MethodInvoker methodDrawImageColorized;

        internal static void DrawImageDisabled(Graphics graphics, Image image, Rectangle imageBounds, Color background, bool unscaledImage)
        {
            if (methodDrawImageDisabled == null)
                methodDrawImageDisabled = MethodInvoker.GetMethodInvoker(typeof(ControlPaint).GetMethod("DrawImageDisabled", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color), typeof(bool) }, null));

            methodDrawImageDisabled.Invoke(null, graphics, image, imageBounds, background, unscaledImage);
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (methodDrawBackgroundImage == null)
                methodDrawBackgroundImage = MethodInvoker.GetMethodInvoker(typeof(ControlPaint).GetMethod("DrawBackgroundImage", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft) }, null));

            methodDrawBackgroundImage.Invoke(null, g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft);
        }

        internal static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            if (methodDrawImageColorized == null)
                methodDrawImageColorized = MethodInvoker.GetMethodInvoker(typeof(ControlPaint).GetMethod("DrawImageColorized", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color) }, null));

            methodDrawImageColorized.Invoke(null, graphics, image, destination, replaceBlack);
        }
    }
}
