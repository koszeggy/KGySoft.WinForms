#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogCallbackProc.cs
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

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// The signature of the callback that receives messages from the Task Dialog when various events occur.
    /// </summary>
    /// <param name="hwnd">The window handle of the </param>
    /// <param name="uNotification">The message being passed.</param>
    /// <param name="wParam">wParam which is interpreted differently depending on the message.</param>
    /// <param name="lParam">wParam which is interpreted differently depending on the message.</param>
    /// <param name="refData">The refrence data that was set to TaskDialog.CallbackData.</param>
    /// <returns>A HRESULT value. The return value is specific to the message being processed. </returns>
    internal delegate int TaskDialogCallbackProc(IntPtr hwnd, TASKDIALOG_NOTIFICATIONS uNotification, IntPtr wParam, IntPtr lParam, IntPtr refData);
}