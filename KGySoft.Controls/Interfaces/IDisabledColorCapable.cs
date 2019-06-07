using System.Drawing;

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents disabled color capability.
    /// </summary>
    internal interface IDisabledColorCapable
    {
        // az összes implementációban empty legyen a default, a shouldserialize adja vissza, hogy a field nem empty,
        // és ha a field empty, a disabledback = back, disabledfore = back.darkdark
        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        Color DisabledForeColor { get; set; }

        /// <summary>
        /// Gets or sets disabled back color.
        /// </summary>
        Color DisabledBackColor { get; set; }
    }
}
