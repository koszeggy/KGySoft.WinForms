#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogResult.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents possible dialog results of a <see cref="TaskDialog"/>.
    /// </summary>
    public enum TaskDialogResult
    {
        /// <summary>
        /// Indicates none of the possible results.
        /// Usually means that the dialog is not closed yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the "OK" dialog result.
        /// Usually means that the dialog was closed by clicking the "OK" button.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Indicates the "Cancel" dialog result.
        /// Usually means that the dialog was closed by either clicking the "Cancel" button, or pressing Esc or Alt+F4 buttons.
        /// </summary>
        Cancel = 2,

        ///// <summary>
        ///// Identifies the Abort button
        ///// </summary>
        //Abort = 3,

        /// Indicates the "Retry" dialog result.
        /// Usually means that the dialog was closed by clicking the "Retry" button.
        Retry = 4,

        ///// <summary>
        ///// Identifies the Ignore button
        ///// </summary>
        //Ignore = 5,

        /// Indicates the "Yes" dialog result.
        /// Usually means that the dialog was closed by clicking the "Yes" button.
        Yes = 6,

        /// <summary>
        /// Indicates the "No" dialog result.
        /// Usually means that the dialog was closed by clicking the "No" button.
        /// </summary>
        No = 7,

        /// <summary>
        /// Indicates the "Close" dialog result.
        /// Usually means that the dialog was closed by clicking the "Close" button.
        /// </summary>
        Close = 8,

        /// <summary>
        /// Indicates a custom dialog result.
        /// Usually means that the dialog was closed by clicking one of the custom buttons.
        /// </summary>
        Custom = 9
    }
}
