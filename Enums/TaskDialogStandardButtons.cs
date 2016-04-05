using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a single standard button in <see cref="TaskDialog"/>
    /// </summary>
    public enum TaskDialogStandardButtons
    {
        /// <summary>
        /// Represents none of the buttons.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Identifies the OK button
        /// </summary>
        OK = 1,

        /// <summary>
        /// Identifies the Cancel button
        /// </summary>
        Cancel = 2,

        ///// <summary>
        ///// Identifies the Abort button
        ///// </summary>
        //Abort = 3,

        /// <summary>
        /// Identifies the Retry button
        /// </summary>
        Retry = 4,

        ///// <summary>
        ///// Identifies the Ignore button
        ///// </summary>
        //Ignore = 5,

        /// <summary>
        /// Identifies the Yes button
        /// </summary>
        Yes = 6,

        /// <summary>
        /// Identifies the No button
        /// </summary>
        No = 7,

        /// <summary>
        /// Identifies the Close button
        /// </summary>
        Close = 8
    }
}
