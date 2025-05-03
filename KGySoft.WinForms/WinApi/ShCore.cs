#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ShCore.cs
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
using System.ComponentModel;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class ShCore
    {
        #region Nested classes

        #region NativeMethods class

        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Retrieves the dots per inch (dpi) awareness of the specified process.
            /// </summary>
            /// <param name="hprocess">Handle of the process that is being queried. If this parameter is NULL, the current process is queried.</param>
            /// <param name="value">The DPI awareness of the specified process. Possible values are from the PROCESS_DPI_AWARENESS enumeration.</param>
            /// <returns>This function returns one of the following values: S_OK/E_INVALIDARG/E_ACCESSDENIED</returns>
            [DllImport("Shcore.dll")]
            internal static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Requires Windows 8.1 or later.
        /// </summary>
        internal static PROCESS_DPI_AWARENESS GetProcessDpiAwareness()
        {
            int hResult = NativeMethods.GetProcessDpiAwareness(IntPtr.Zero, out PROCESS_DPI_AWARENESS value);
            if (hResult != Constants.S_OK)
                throw new Win32Exception(hResult);
            return value;
        }

        #endregion
    }
}
