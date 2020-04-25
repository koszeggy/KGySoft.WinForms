using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using KGySoft.Reflection;

namespace KGySoft.WinForms
{
    internal static class Accessors
    {
        private static MethodAccessor methodGraphicsExtensions_CreateRoundedRectangle;

        private static MethodAccessor GraphicsExtensions_CreateRoundedRectangle => methodGraphicsExtensions_CreateRoundedRectangle ?? (methodGraphicsExtensions_CreateRoundedRectangle = MethodAccessor.GetAccessor(typeof(Drawing.GraphicsExtensions).GetMethod("CreateRoundedRectangle", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Rectangle), typeof(int) }, null)));

        internal static GraphicsPath GraphicsExtensions_CallCreateRoundedRectangle(Rectangle bounds, int radius)
            => (GraphicsPath)GraphicsExtensions_CreateRoundedRectangle.Invoke(null, bounds, radius);
    }
}
