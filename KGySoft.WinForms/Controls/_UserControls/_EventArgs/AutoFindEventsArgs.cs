#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AutoFindEventArgs.cs
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

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Arguments for handling an <see cref="ucCustomSelector.AutoFind"/> event.
    /// </summary>
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public class AutoFindEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Get the text that was typed into the text field.
        /// </summary>
        public string SearchPattern { get; private set; }

        /// <summary>
        /// Gets or sets the value that is associated with the found or selected item.
        /// Set this property to associate a value with the found text. By setting this property
        /// text of the selector will be calculated by <see cref="ucCustomSelector.GetTextByValue"/> or by its derived method.
        /// By default, value of this property is the object that represents the not selected value.
        /// If in the used scenario <see cref="ucCustomSelector.Value"/> has no special meaning,
        /// then you may set this property to <see cref="ControlExtensions.UndefinedValue"/> so <see cref="ucCustomSelector.Text"/>
        /// will not be changed - you have to do it manually.
        /// To fallback to default logic set <see cref="DefaultAutoFind"/> to <see langword="true"/>.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets whether <see cref="ucCustomSelector.DefaultAutoFind"/> should be called.
        /// Set this property to <see langword="true"/> to fallback to default logic instead of accepting <see cref="Value"/>.
        /// </summary>
        public bool DefaultAutoFind { get; set; }

        #endregion

        #region Constructors

        internal AutoFindEventArgs(string searchPattern, object notSelectedValue)
        {
            this.Value = notSelectedValue;
            this.SearchPattern = searchPattern;
        }

        #endregion
    }
}
