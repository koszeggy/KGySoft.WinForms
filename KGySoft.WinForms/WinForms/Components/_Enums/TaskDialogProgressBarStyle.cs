#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogProgressBarStyle.cs
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
    /// Represents the possible progress bar styles of a <see cref="TaskDialog"/>.
    /// </summary>
    public enum TaskDialogProgressBarStyle
    {
        /// <summary>
        /// Indicates that no progress bar should be displayed on the dialog.
        /// </summary>
        None,

        /// <summary>
        /// Represents the continuous progress bar style.
        /// </summary>
        Regular,

        /// <summary>
        /// Represents the marquee progress bar style
        /// </summary>
        Marquee
    }
}