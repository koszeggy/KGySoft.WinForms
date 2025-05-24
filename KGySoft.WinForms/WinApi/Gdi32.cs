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

        /// <summary>
        /// The GetStockObject function retrieves a handle to one of the stock pens, brushes, fonts, or palettes.
        /// </summary>
        /// <param name="i">The type of stock object.</param>
        /// <returns>If the function succeeds, the return value is a handle to the requested logical object. If the function fails, the return value is NULL.</returns>
        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern IntPtr GetStockObject(int i);


        #endregion
    }
}