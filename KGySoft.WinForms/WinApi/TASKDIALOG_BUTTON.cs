using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// The TASKDIALOG_BUTTON structure contains information used to display a button in a task dialog. The <see cref="TASKDIALOGCONFIG"/> structure uses this structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct TASKDIALOG_BUTTON
    {
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
    }
}
