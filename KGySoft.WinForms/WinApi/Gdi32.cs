#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Gdi32.cs
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
    internal static class Gdi32
    {
        #region Methods

        /// <summary>
        /// Retrieves device-specific information for the specified device.
        /// </summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="nIndex">The item to be returned.</param>
        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

        #endregion
    }
}