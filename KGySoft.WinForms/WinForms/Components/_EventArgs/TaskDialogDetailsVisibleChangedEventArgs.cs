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
    /// Provides arguments for the <see cref="TaskDialog.DetailsVisibleChanged"/> event.
    /// </summary>
    public class TaskDialogDetailsVisibleChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets whether the details text is visible. That is, when <see cref="TaskDialog.DetailsText"/> is not <see langword="null"/>,
        /// and the expando button is in expanded state.
        /// </summary>
        public bool DetailsVisible { get; }

        #endregion

        #region Constructors

        internal TaskDialogDetailsVisibleChangedEventArgs(bool visible) => DetailsVisible = visible;

        #endregion
    }
}