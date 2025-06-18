#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumThreadWndProc.cs
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

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// An application-defined callback function used with the EnumThreadWindows function. It receives the window handles associated with a thread.
    /// The WNDENUMPROC type defines a pointer to this callback function. EnumThreadWndProc is a placeholder for the application-defined function name.
    /// </summary>
    /// <param name="hWnd">A handle to a window associated with the thread specified in the EnumThreadWindows function.</param>
    /// <param name="lParam">The application-defined value given in the EnumThreadWindows function.</param>
    /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.</returns>
    internal delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);
}