#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LocalizationRequestedEventArgs.cs
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

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides data for the <see cref="LocalizationHelper.LocalizationRequested"/> event.
    /// </summary>
    public sealed class LocalizationRequestedEventArgs : EventArgs
    {
        #region Fields

        private string key;
        private object? target;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the context of the localization request.
        /// Can be <see langword="null"/> if the request is not associated with any specific context.
        /// </summary>
        public LocalizationContext? Context { get; }

        /// <summary>
        /// Gets the target object of the localization request.
        /// It is usually the control or object whose property is being localized, but can be <see langword="null"/> if the request is not associated with any specific target.
        /// </summary>
        public object? Target => target;

        /// <summary>
        /// Gets the key of the requested localization string.
        /// </summary>
        public string Key => key;

        /// <summary>
        /// Gets or sets the value of the requested localization string.
        /// If left <see langword="null"/>, the localization request might be handled by the default mechanism, depending on the <see cref="LocalizationContext.LocalizationScope"/> value.
        /// If there are multiple handlers of the <see cref="LocalizationHelper.LocalizationRequested"/> event, this property may already be set by a previous handler.
        /// </summary>
        public string? Value { get; set; }

        #endregion

        #region Constructors

        internal LocalizationRequestedEventArgs(LocalizationContext context, object target)
        {
            Context = context;
            this.target = target;
            key = String.Empty; // actually will be set later
        }

        internal LocalizationRequestedEventArgs(LocalizationContext? context, string key)
        {
            Context = context;
            this.key = key;
        }

        #endregion

        #region Methods

        internal void Reset(string newKey)
        {
            key = newKey;
            Value = null;
        }

        #endregion
    }
}