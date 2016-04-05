using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contains argurments of <see cref="TaskDialog.DetailsVisibleChanged"/> event.
    /// </summary>
    public class TaskDialogDetailsVisibleChangedEventArgs: EventArgs
    {
        /// <summary>
        /// Gets whether details text is visible.
        /// </summary>
        public bool DetailsVisible { get; private set; }

        internal TaskDialogDetailsVisibleChangedEventArgs(bool visible)
        {
            DetailsVisible = visible;
        }
    }
}
