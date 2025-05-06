#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Comdlg32.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    static class Comdlg32
    {
        #region Methods

        /// <summary>
        /// Creates a Save dialog box that lets the user specify the drive, directory, and name of a file to save.
        /// </summary>
        /// <param name="lpofn">A pointer to an <see cref="OPENFILENAME"/> structure that contains information used to initialize the dialog box. When GetSaveFileName returns, this structure contains information about the user's file selection.</param>
        /// <returns>If the user specifies a file name and clicks the OK button and the function is successful, the return value is nonzero. The buffer pointed to by the lpstrFile member of the OPENFILENAME structure contains the full path and file name specified by the user.
        /// If the user cancels or closes the Save dialog box or an error such as the file name buffer being too small occurs, the return value is zero. To get extended error information, call the <see cref="CommDlgExtendedError"/> function.
        /// </returns>
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetSaveFileName(ref OPENFILENAME lpofn);

        /// <summary>
        /// Returns a common dialog box error code. This code indicates the most recent error to occur during the execution of one of the common dialog box functions.
        /// </summary>
        /// <returns>If the most recent call to a common dialog box function succeeded, the return value is undefined. If the common dialog box function returned FALSE because the user closed or canceled the dialog box, the return value is zero. Otherwise, the return value is a nonzero error code.
        /// The CommDlgExtendedError function can return general error codes for any of the common dialog box functions. In addition, there are error codes that are returned only for a specific common dialog box. All of these error codes are defined in Cderr.h.
        /// </returns>
        [DllImport("Comdlg32.dll")]
        internal static extern int CommDlgExtendedError();

        #endregion
    }
}
