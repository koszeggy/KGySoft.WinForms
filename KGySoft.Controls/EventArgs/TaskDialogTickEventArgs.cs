using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contain arguments of <see cref="TaskDialog.Tick"/> event.
    /// </summary>
    public class TaskDialogTickEventArgs: EventArgs
    {
        internal TaskDialogTickEventArgs(int elapsed)
        {
            Elapsed = elapsed;
        }

        /// <summary>
        /// Gets the elapsed time in milliseconds since the dialog is created, or reallocated due to a special property change, or the last reset.
        /// </summary>
        public int Elapsed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that timer count should be reset.
        /// </summary>
        public bool Reset { get; set; }
    }
}
