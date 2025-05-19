#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FileTypeChangedEventArgs.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    /// Contains arguments of <see cref="AdvancedSaveFileDialog.FileTypeChanged"/> event.
    /// </summary>
    public sealed class FileTypeChangedEventArgs : EventArgs
    {
        #region Fields

        private readonly int selectedIndex;
        private readonly string extension;

        #endregion

        #region Properties

        /// <summary>
        /// Gets selected index of the file type combo box in <see cref="AdvancedSaveFileDialog"/>.
        /// Unlike as <see cref="AdvancedSaveFileDialog.FilterIndex"/>, this is a zero-based index.
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
        }

        /// <summary>
        /// Gets selected file type filter in <see cref="AdvancedSaveFileDialog"/> or null when
        /// <see cref="AdvancedSaveFileDialog.Filter"/> was not correctly defined.
        /// </summary>
        public string Filter
        {
            get { return extension; }
        }

        #endregion

        #region Constructors

        internal FileTypeChangedEventArgs(int index, string filter)
        {
            selectedIndex = index;
            extension = filter;
        }

        #endregion
    }
}