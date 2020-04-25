#region Used namespaces

using System;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NCCALCSIZE_PARAMS
    {
        #region Fields

        public RECT rgrc0, rgrc1, rgrc2;

        public IntPtr lppos;

        #endregion
    }
}
