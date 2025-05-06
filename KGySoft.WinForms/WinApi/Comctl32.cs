#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Comctl32.cs
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

using KGySoft.WinForms.Components;

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class Comctl32
    {
        #region Methods

        /// <summary>
        /// The TaskDialogIndirect function creates, displays, and operates a task dialog.
        /// The task dialog contains application-defined icons, messages, title,
        /// verification check box, command links, push buttons, and radio buttons.
        /// This function can register a callback function to receive notification messages.
        /// </summary>
        /// <param name="pTaskConfig">Pointer to a <see cref="TASKDIALOGCONFIG"/> structure that contains information used to display the task dialog.</param>
        /// <param name="pnButton">Address of a variable that receives either one of the button IDs specified in the pButtons member of the pTaskConfig parameter,
        /// or one of the values of <see cref="TaskDialogStandardButtons"/></param>
        /// <param name="pnRadioButton">Address of a variable that receives one of the button IDs specified in the pRadioButtons member of the pTaskConfig parameter. If this parameter is NULL, no value is returned.</param>
        /// <param name="pfVerificationFlagChecked">Address of a variable that indicates whether the verification checkbox was checked when the dialog was dismissed.</param>
        /// <returns></returns>
        [DllImport("Comctl32.dll", SetLastError = true)]
        internal static extern int TaskDialogIndirect(
            [In] ref TASKDIALOGCONFIG pTaskConfig,
            [Out] out int pnButton,
            [Out] out int pnRadioButton,
            [MarshalAs(UnmanagedType.Bool), Out] out bool pfVerificationFlagChecked);

        #endregion
    }
}
