#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SelectorButtons.cs
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

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents the buttons that can appear in a <see cref="ucCustomSelector"/>
    /// </summary>
    [Flags]
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public enum SelectorButtons
    {
        /// <summary>
        /// Represents no buttons.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the Clear Selection button.
        /// </summary>
        ClearSelection = 1 << 0,

        /// <summary>
        /// Represents the Select All button.
        /// </summary>
        SelectAll = 1 << 1,

        /// <summary>
        /// Represents the Select None button.
        /// </summary>
        SelectNone = 1 << 2,

        /// <summary>
        /// Represents the Browse button.
        /// </summary>
        Browse = 1 << 3,

        /// <summary>
        /// Represents the Edit button.
        /// </summary>
        Editor = 1 << 4,

        /// <summary>
        /// Represents the New button.
        /// </summary>
        New = 1 << 5
    }
}