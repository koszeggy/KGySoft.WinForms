#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogTickEventArgs.cs
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
    /// Contain arguments of <see cref="TaskDialog.Tick"/> event.
    /// </summary>
    public class TaskDialogTickEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the elapsed time in milliseconds since the dialog is created, or reallocated due to a special property change, or the last reset.
        /// </summary>
        public int Elapsed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that timer count should be reset.
        /// </summary>
        public bool Reset { get; set; }

        #endregion

        #region Constructors

        internal TaskDialogTickEventArgs(int elapsed)
        {
            Elapsed = elapsed;
        }

        #endregion
    }
}