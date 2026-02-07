#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SelectionPlusItems.cs
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

namespace KGySoft.WinForms
{
    /// <summary>
    /// Defines flags for extra combo items.
    /// </summary>
    [Flags]
    [Obsolete("This type is obsolete. The additional items are not auto-translated anymore. Such items should come from a view-model with data binding.")]
    public enum SelectionPlusItems
    {
        /// <summary>
        /// Represents no extra items
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the "Not Selected" item
        /// </summary>
        ItemNotSelected = 1,

        /// <summary>
        /// Represents the "All" item
        /// </summary>
        ItemAll = ItemNotSelected << 1,

        /// <summary>
        /// Represents the "None" item
        /// </summary>
        ItemNone = ItemNotSelected << 2
    }
}