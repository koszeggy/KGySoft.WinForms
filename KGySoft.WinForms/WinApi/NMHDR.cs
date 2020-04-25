using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Contains information about a notification message.
    /// </summary>
    internal struct NMHDR
    {
        public IntPtr HwndFrom;
        public IntPtr IdFrom;
        public int Code;
    }
}
