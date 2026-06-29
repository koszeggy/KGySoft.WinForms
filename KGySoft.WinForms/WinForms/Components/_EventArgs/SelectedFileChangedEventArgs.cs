#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SelectedFileChangedEventArgs.cs
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
    /// Provides arguments for the <see cref="AdvancedSaveFileDialog.SelectedFileChanged"/> event.
    /// </summary>
    [Obsolete("It belongs to the obsoleted AdvancedSaveFileDialog class")]
    public class SelectedFileChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the selected file name
        /// </summary>
        public string FileName { get; }

        #endregion

        #region Constructors

        internal SelectedFileChangedEventArgs(string fileName) => FileName = fileName;

        #endregion
    }
}