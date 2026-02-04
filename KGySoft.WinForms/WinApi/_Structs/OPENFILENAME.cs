#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OPENFILENAME.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct OPENFILENAME
    {
        #region Fields

        internal int lStructSize;
        internal IntPtr hwndOwner;
        internal IntPtr hInstance;
        [MarshalAs(UnmanagedType.LPTStr)]internal string lpstrFilter;
        [MarshalAs(UnmanagedType.LPTStr)]internal string lpstrCustomFilter;
        internal int nMaxCustFilter;
        internal int nFilterIndex;

        /// <summary>
        /// File name edit box
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        internal string lpstrFile;

        internal int nMaxFile;
        /// <summary>
        /// The file name and extension (without path information) of the selected file. This member can be NULL.
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        internal string? lpstrFileTitle;

        internal int nMaxFileTitle;
        [MarshalAs(UnmanagedType.LPTStr)]internal string? lpstrInitialDir;
        [MarshalAs(UnmanagedType.LPTStr)]internal string? lpstrTitle;
        internal int Flags;
        internal short nFileOffset;
        internal short nFileExtension;
        [MarshalAs(UnmanagedType.LPTStr)]internal string? lpstrDefExt;
        internal int lCustData;
        internal OFNHookProcDelegate lpfnHook;
        [MarshalAs(UnmanagedType.LPTStr)]internal string? lpTemplateName;

        // NT 5.0 or higher
        internal int pvReserved;
        internal int dwReserved;
        internal int FlagsEx;

        #endregion
    }
}
