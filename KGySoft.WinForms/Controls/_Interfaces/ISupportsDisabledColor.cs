#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportsDisabledColor.cs
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

using System.Drawing;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents separated enabled/disabled color capability.
    /// </summary>
    internal interface ISupportsDisabledColor
    {
        #region Properties

        /// <summary>
        /// Gets or sets enabled back color.
        /// </summary>
        Color EnabledBackColor { get; set; }

        /// <summary>
        /// Gets or sets enabled fore color.
        /// </summary>
        Color EnabledForeColor { get; set; }

        /// <summary>
        /// Gets or sets disabled back color.
        /// </summary>
        Color DisabledBackColor { get; set; }

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        Color DisabledForeColor { get; set; }

        #endregion
    }
}