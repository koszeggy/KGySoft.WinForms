using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Possible border styles of an <see cref="AdvancedPanel"/>.
    /// </summary>
    public enum AdvancedBorderStyle
    {
        /// <summary>
        /// No border.
        /// </summary>
        None = 0,

        /// <summary>
        /// Border is the same as a border of a <see cref="Panel"/> when its border style is <see cref="BorderStyle.FixedSingle"/>.
        /// </summary>
        FixedSingle = 1,

        /// <summary>
        /// Flat border, no 3D effect.
        /// </summary>
        Flat = 16394,

        /// <summary>
        /// Border is slightly raised.
        /// </summary>
        Raised = 4,

        /// <summary>
        /// Border is considerably raised.
        /// </summary>
        RaisedHigh = 5,

        /// <summary>
        /// Border is slightly sunken.
        /// </summary>
        Sunken = 2,

        /// <summary>
        /// Border is considerably sunken. Has the same appearance as a border of a <see cref="Panel"/> when its border style is <see cref="BorderStyle.Fixed3D"/>.
        /// </summary>
        SunkenLow = 10,

        /// <summary>
        /// Border has a raised (bump) frame.
        /// </summary>
        RaisedFrame = 9,

        /// <summary>
        /// Border has a sunken (etched) frame.
        /// </summary>
        SunkenFrame = 6,

    }
}