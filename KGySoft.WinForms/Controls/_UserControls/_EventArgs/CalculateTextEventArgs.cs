#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CalculateTextEventArgs.cs
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
using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Arguments for handling an <see cref="ucCustomSelector.CalculateText"/> event.
    /// </summary>
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public class CalculateTextEventArgs : EventArgs
    {
        #region Fields

        private readonly object? value;
        
        private string text;

        #endregion

        #region Properties

        /// <summary>
        /// Get or sets the text that is associated by <see cref="Value"/>.
        /// </summary>
        [AllowNull]
        public string Text
        {
            get => text;
            set => text = value ?? String.Empty;
        }

        /// <summary>
        /// Gets the value that is associated with the found or selected item.
        /// </summary>
        public object? Value => value;

        #endregion

        #region Constructors

        internal CalculateTextEventArgs(object? value, string? text)
        {
            this.value = value;
            this.text = text ?? String.Empty;
        }

        #endregion
    }
}