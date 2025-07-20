#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WINDOWPOS.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Contains information about the size and position of a window.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPOS
    {
        #region Fields

        internal IntPtr hwnd;
        internal IntPtr hwndInsertAfter;
        internal int x;
        internal int y;
        internal int cx;
        internal int cy;
        internal uint flags;

        #endregion
    }
}