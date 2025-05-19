#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogStandardButtonFlags.cs
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

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Identifies the possible standard buttons that
    /// can be displayed via <see cref="TaskDialog"/>.
    /// </summary>
    [Flags]
    public enum TaskDialogStandardButtonFlags
    {
        /// <summary>
        /// Represents none of the standard buttons.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// The task dialog contains the push button: OK.
        /// </summary>
        OK = 0x0001,

        /// <summary>
        /// The task dialog contains the push button: Yes.
        /// </summary>
        Yes = 0x0002,

        /// <summary>
        /// The task dialog contains the push button: No.
        /// </summary>
        No = 0x0004,

        /// <summary>
        /// The task dialog contains the push button: Cancel.
        /// If this button is specified, the task dialog will respond to typical cancel actions (Alt-F4 and Escape).
        /// </summary>
        Cancel = 0x0008,

        /// <summary>
        /// The task dialog contains the push button: Retry.
        /// </summary>
        Retry = 0x0010,

        /// <summary>
        /// The task dialog contains the push button: Close.
        /// </summary>
        Close = 0x0020
    }
}