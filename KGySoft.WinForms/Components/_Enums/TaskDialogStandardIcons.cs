#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogStandardIcons.cs
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

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents possible standard icons for <see cref="TaskDialog"/>.
    /// </summary>
    public enum TaskDialogStandardIcons
    {
        /// <summary>
        /// Represents no icon.
        /// </summary>
        None,

        /// <summary>
        /// Represents the system information icon
        /// </summary>
        Information = UInt16.MaxValue - 2,

        /// <summary>
        /// Represents the system warning icon
        /// </summary>
        Warning = UInt16.MaxValue,

        /// <summary>
        /// Represents the system error icon
        /// </summary>
        Error = UInt16.MaxValue - 1,

        /// <summary>
        /// Represents the system question icon
        /// </summary>
        Question = -1,

        /// <summary>
        /// Represents the system security success icon with green background
        /// </summary>
        SecuritySuccess = UInt16.MaxValue - 7,

        /// <summary>
        /// Represents the system security warning icon with yellow background
        /// </summary>
        SecurityWarning = UInt16.MaxValue - 5,

        /// <summary>
        /// Represents the system security error icon with red background
        /// </summary>
        SecurityError = UInt16.MaxValue - 6,

        /// <summary>
        /// Represents the system security shield icon with white background
        /// </summary>
        SecurityShield = UInt16.MaxValue - 3,

        /// <summary>
        /// Represents the system security shield icon with blue background
        /// </summary>
        SecurityShieldBlue = UInt16.MaxValue - 4,

        /// <summary>
        /// Represents the system security shield icon with gray background
        /// </summary>
        SecurityShieldGray = UInt16.MaxValue - 8,

        /// <summary>
        /// Represents the system security question icon with blue background
        /// </summary>
        SecurityQuestion = -2
    }
}
