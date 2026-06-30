#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedBorderStyle.cs
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

using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents possible advanced border styles, used by <see cref="AdvancedPanel"/> and <see cref="AdvancedLabel"/> controls, for example.
    /// </summary>
    public enum AdvancedBorderStyle
    {
        /// <summary>
        /// Represents no visible border.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the same single-line border as <see cref="BorderStyle.FixedSingle">BorderStyle.FixedSingle</see>.
        /// </summary>
        FixedSingle = 1,

        /// <summary>
        /// A flat border with no 3D effect.
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
        /// Border is considerably sunken.
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
