using System.Runtime.InteropServices;
using KGySoft.WinForms.Components;

namespace KGySoft.WinForms.WinApi
{
    internal static class Comctl32
    {
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

        ///// <summary>
        ///// Implemented by many of the Windows Shell DLLs to allow applications to obtain DLL-specific version information.
        ///// </summary>
        ///// <param name="version">A pointer to a DLLVERSIONINFO structure that receives the version information. The cbSize member must be filled in before calling the function.</param>
        //[DllImport("Comctl32.dll")]
        //internal static extern int DllGetVersion(ref DLLVERSIONINFO version);
    }
}
