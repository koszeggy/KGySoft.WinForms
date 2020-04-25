using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace KGySoft.WinForms
{
    internal static class SizeFExtensions
    {
        internal static Size Ceiling(this SizeF sizeF)
        {
            return new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
        }
    }
}
