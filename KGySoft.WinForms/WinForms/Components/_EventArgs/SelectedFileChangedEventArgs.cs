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
    /// Contains arguments of <see cref="AdvancedSaveFileDialog.SelectedFileChanged"/> event.
    /// </summary>
    public class SelectedFileChangedEventArgs : EventArgs
    {
        #region Fields

        private readonly string fileName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets selected file name
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        #endregion

        #region Constructors

        internal SelectedFileChangedEventArgs(string fileName)
        {
            this.fileName = fileName;
        }

        #endregion
    }
}