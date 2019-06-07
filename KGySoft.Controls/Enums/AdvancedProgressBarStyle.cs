using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
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
