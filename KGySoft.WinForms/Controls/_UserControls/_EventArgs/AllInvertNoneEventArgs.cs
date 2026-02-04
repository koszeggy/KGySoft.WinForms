#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AllInvertNoneEventArgs.cs
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
    /// Provides data for the <see cref="ucAllInvertNone.ButtonPressed">ucAllInvertNone.ButtonPressed</see> event.
    /// </summary>
    [Obsolete("This class belongs to the obsoleted ucAllInvertNone class")]
    public class AllInvertNoneEventArgs : EventArgs
    {
        #region Fields

        private readonly InvertButtonTypes buttonType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the button type that was pressed.
        /// </summary>
        public InvertButtonTypes ButtonType => buttonType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AllInvertNoneEventArgs"/> class.
        /// </summary>
        /// <param name="buttonType">The button that triggered the event.</param>
        public AllInvertNoneEventArgs(InvertButtonTypes buttonType) => this.buttonType = buttonType;

        #endregion
    }
}