#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedDialogTypes.cs
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
    /// Specifies the predefined types of an <see cref="AdvancedMessageDialog"/>.
    /// </summary>
    [Obsolete("This type is used by the obsoleted AdvancedMessageDialog")]
    public enum AdvancedDialogTypes
    {
        /// <summary>
        /// Represents an information dialog.
        /// </summary>
        Information,

        /// <summary>
        /// Represents a confirmation dialog.
        /// </summary>
        Confirmation,

        /// <summary>
        /// Represents a warning dialog.
        /// </summary>
        Warning,

        /// <summary>
        /// Represents an error dialog.
        /// </summary>
        Error,

        /// <summary>
        /// Represents an exception dialog.
        /// </summary>
        Exception,

        /// <summary>
        /// Represents a dialog with a custom image.
        /// </summary>
        CustomImage
    }
}