using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Defines flags for extra combo items.
    /// </summary>
    [Flags]
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
