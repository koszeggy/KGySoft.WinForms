using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Notifications of a <see cref="TaskDialogCallbackProc"/> function.
    /// </summary>
    internal enum TASKDIALOG_NOTIFICATIONS: uint
    {
        /// <summary>
        /// Indicates that the Task Dialog has been created.
        /// Sent once the dialog has been created and before it is displayed.
        /// The value returned by the callback is ignored.
        /// </summary>
        TDN_CREATED = 0,

        /// <summary>
        /// Sent by the Task Dialog when a navigation has occurred.
        /// The value returned by the callback is ignored.
        /// </summary>   
        TDN_NAVIGATED = 1,

        /// <summary>
        /// Indicates that a button has been selected. The command ID of the button is specified by wParam.
        /// To prevent the Task Dialog from closing, the application must
        /// return true, otherwise the Task Dialog will be closed and the button ID returned to via
        /// the original application call.
        /// wParam = Button ID
        /// </summary>
        TDN_BUTTON_CLICKED = 2,

        /// <summary>
        /// Indicates that a hyperlink has been selected. A pointer to the link text is specified by lParam.
        /// To prevent the TaskDialog from shell executing the hyperlink,
        /// the application must return TRUE, otherwise ShellExecute will be called.
        /// lParam = (LPCWSTR)pszHREF
        /// </summary>
        TDN_HYPERLINK_CLICKED = 3,

        /// <summary>
        /// Indicates that the Task Dialog timer has fired. The total elapsed time is specified by wParam.
        /// You can update the progress bar by sending a TDM_SET_PROGRESS_BAR_POS message to the window specified by the hwnd parameter.
        /// To reset the tickcount, the application must return true, otherwise the tickcount will continue to increment.
        /// wParam = Milliseconds since dialog created or timer reset
        /// </summary>
        TDN_TIMER = 4,

        /// <summary>
        /// Sent by the Task Dialog when it is destroyed and its window handle no longer valid.
        /// The value returned by the callback is ignored.
        /// </summary>
        TDN_DESTROYED = 5,

        /// <summary>
        /// Indicates that a radio button has been selected. The command ID of the radio button is specified by wParam.
        /// The value returned by the callback is ignored.
        /// wParam = Radio Button ID
        /// </summary>
        TDN_RADIO_BUTTON_CLICKED = 6,

        /// <summary>
        /// Indicates that the Task Dialog has been created but has not been displayed yet.
        /// The value returned by the callback is ignored.
        /// </summary>
        TDN_DIALOG_CONSTRUCTED = 7,

        /// <summary>
        /// Indicates that the user checks or unchecks the verification checkbox.
        /// The value returned by the callback is ignored.
        /// wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
        /// </summary>
        TDN_VERIFICATION_CLICKED = 8,

        /// <summary>
        /// Indicates that the F1 key has been pressed while the Task Dialog has focus.
        /// The value returned by the callback is ignored.
        /// </summary>
        TDN_HELP = 9,

        /// <summary>
        /// Indicates that the exando button has been selected.
        /// The value returned by the callback is ignored.
        /// wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
        /// </summary>
        TDN_EXPANDO_BUTTON_CLICKED = 10
    }
}
