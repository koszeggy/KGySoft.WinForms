using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls.WinApi
{
    internal struct POINT
    {
        internal int x;
        internal int y;

        internal POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
