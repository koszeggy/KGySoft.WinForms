#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SetMessageEventArgs.cs
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
using System.ComponentModel;

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Provides arguments for the <see cref="AdvancedErrorProvider.SetMessage">AdvancedErrorProvider.SetMessage</see> event.
    /// </summary>
    /// <seealso cref="AdvancedErrorProvider" />
    public class SetMessageEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the current item of the underlying data source.
        /// Can be <see langword="null"/> if the message is required for a binding error.
        /// </summary>
        public object? Current { get; }

        /// <summary>
        /// Gets the name of the property for which the message is requested.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets or sets the message to be displayed. If the message is retrieved due to a binding error or the <see cref="Current"/> item
        /// implements the <see cref="IDataErrorInfo"/> interface, then this property may already contain a value.
        /// </summary>
        public string? Message { get; set; }

        #endregion

        #region Constructors

        internal SetMessageEventArgs(object? current, string propertyName, string? message)
        {
            Current = current;
            PropertyName = propertyName;
            Message = message;
        }

        #endregion
    }
}
