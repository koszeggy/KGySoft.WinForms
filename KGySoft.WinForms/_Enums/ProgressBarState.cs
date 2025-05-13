#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ProgressBarState.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents possible progress bar states.
    /// </summary>
    public enum ProgressBarState
    {
        /// <summary>
        /// Indicates the normal progress bar state.
        /// </summary>
        Normal,

        /// <summary>
        /// Indicates the error progress bar state.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates the paused progress bar state.
        /// </summary>
        Paused
    }
}