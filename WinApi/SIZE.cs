using System.Drawing;
using System.Runtime.InteropServices;

namespace KGySoft.Controls.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        internal int cx;
        internal int cy;

        internal SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }

        internal SIZE(Size size): this(size.Width, size.Height)
        {            
        }

        internal Size ToSize()
        {
            return new Size(cx, cy);
        }
    }
}
