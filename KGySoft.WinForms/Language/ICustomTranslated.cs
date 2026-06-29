#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomTranslated.cs
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

// ReSharper disable once CheckNamespace
namespace KGySoft.Libraries.Language
{
    /// <summary>
    /// Makes a control custom translatable. See <see cref="Language"/>.
    /// </summary>
    [Obsolete("This type belongs to the obsoleted Language class. Use ICustomLocalizable and LocalizationHelper instead.")]
    public interface ICustomTranslated
    {
        #region Methods

        /// <summary>
        /// Translates the control.
        /// </summary>
        /// <param name="translationFinished">If an implementer returns <see langword="true"/>, no further translation will be performed on child elements.</param>
        /// <returns><see langword="false"/> if translation is disabled for the control; otherwise, <see langword="true"/>.</returns>
        bool TranslateControl(out bool translationFinished);

        #endregion
    }
}