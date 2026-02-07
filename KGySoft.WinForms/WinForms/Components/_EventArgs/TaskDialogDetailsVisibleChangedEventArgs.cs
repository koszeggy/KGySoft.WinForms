#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogDetailsVisibleChangedEventArgs.cs
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
    /// Contains arguments of <see cref="TaskDialog.DetailsVisibleChanged"/> event.
    /// </summary>
    public class TaskDialogDetailsVisibleChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets whether details text is visible.
        /// </summary>
        public bool DetailsVisible { get; private set; }

        #endregion

        #region Constructors

        internal TaskDialogDetailsVisibleChangedEventArgs(bool visible)
        {
            DetailsVisible = visible;
        }

        #endregion
    }
}