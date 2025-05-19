#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogStatus.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Components
{
    internal enum TaskDialogStatus
    {
        /// <summary>
        /// Initializing state
        /// </summary>
        Initializing,

        /// <summary>
        /// Currently Showing
        /// </summary>
        Showing,

        /// <summary>
        /// Currently Closing
        /// </summary>
        Closing,

        /// <summary>
        /// Closed
        /// </summary>
        Closed
    }
}