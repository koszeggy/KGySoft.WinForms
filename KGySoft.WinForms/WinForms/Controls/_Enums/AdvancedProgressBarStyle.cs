#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedProgressBarStyle.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents the possible styles of an <see cref="AdvancedProgressBar"/>.
    /// </summary>
    public enum AdvancedProgressBarStyle
    {
        /// <summary>
        /// Represents the system-rendered mode.
        /// </summary>
        System,

        /// <summary>
        /// Represents the custom rendered "shiny" mode. When visual styles are not enabled,
        /// defaults to <see cref="Classic"/> style.
        /// </summary>
        ThemedShiny,

        /// <summary>
        /// Represents the custom rendered "flat" mode. When visual styles are not enabled,
        /// defaults to <see cref="Classic"/> style.
        /// </summary>
        ThemedFlat,

        /// <summary>
        /// Represents the custom rendered "classic" mode.
        /// </summary>
        Classic
    }
}