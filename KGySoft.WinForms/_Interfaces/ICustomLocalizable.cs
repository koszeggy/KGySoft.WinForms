#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomLocalizable.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a custom localizable control that can be localized by the <see cref="LocalizationHelper"/>.
    /// </summary>
    public interface ICustomLocalizable
    {
        #region Methods

        /// <summary>
        /// Applies the string resources to the control based on the provided <see cref="LocalizationContext"/>.
        /// </summary>
        /// <param name="context">The context containing information about the localization operation.</param>
        /// <returns><see langword="true"/>, if the call was handled and the default localization behavior should not be applied;
        /// <see langword="false"/> to apply the default localization behavior.
        /// </returns>
        bool ApplyStringResources(LocalizationContext context);

        #endregion
    }
}