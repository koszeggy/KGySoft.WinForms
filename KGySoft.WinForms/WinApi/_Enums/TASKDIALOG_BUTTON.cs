#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TASKDIALOG_BUTTON.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// The TASKDIALOG_BUTTON structure contains information used to display a button in a task dialog. The <see cref="TASKDIALOGCONFIG"/> structure uses this structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct TASKDIALOG_BUTTON
    {
        #region Fields

        /// <summary>
        /// Indicates the value to be returned when this button is selected.
        /// </summary>
        internal int nButtonID;
        /// <summary>
        /// Pointer that references the string to be used to label the button.
        /// When using Command Links, you delineate the command from the note by placing a new line character in the string.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszButtonText;

        #endregion
    }
}