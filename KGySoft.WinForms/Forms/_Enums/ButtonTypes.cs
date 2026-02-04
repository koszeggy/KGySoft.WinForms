#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonTypes.cs
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

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Specifies the buttons of an <see cref="AdvancedMessageDialog"/>.
    /// </summary>
    [Obsolete("This type is used by the obsoleted AdvancedMessageDialog")]
    public enum ButtonTypes
    {
        // Standard types with DialogResult return
        /// <summary>
        /// Represents a message box that displays a single OK button.
        /// </summary>
        OK,

        /// <summary>
        /// Represents a message box that displays Yes and No buttons.
        /// </summary>
        YesNo,

        /// <summary>
        /// Represents a message box that displays Yes, No and Cancel buttons.
        /// </summary>
        YesNoCancel,

        /// <summary>
        /// Represents a message box that displays OK and Cancel buttons.
        /// </summary>
        OKCancel,

        /// <summary>
        /// Represents a message box that displays Retry and Cancel buttons.
        /// </summary>
        RetryCancel,

        /// <summary>
        /// Represents a message box that displays Abort, Retry and Ignore buttons.
        /// </summary>
        AbortRetryIgnore,

        // Special types with DialogResult.None return
        /// <summary>
        /// Represents a message box that displays a Close button with a door icon.
        /// </summary>
        Closewin,

        /// <summary>
        /// Represents a message box that displays a Close button with a door icon and a Send Report button.
        /// </summary>
        ClosewinSendreport,

        /// <summary>
        /// Represents a message box that displays a Close button with a door icon, a Send Report button, and a Close Application button.
        /// </summary>
        ClosewinSendreportCloseapp
    }
}