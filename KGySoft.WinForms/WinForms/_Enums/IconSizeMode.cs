#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconSizeMode.cs
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

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents the possible <see cref="Icon"/> sizing modes.
    /// </summary>
    public enum IconSizeMode
    {
        /// <summary>
        /// Represents the default icon sizing behavior, which depends on the current executing platform.
        /// </summary>
        SystemDefault,

        /// <summary>
        /// Represents automatic resizing behavior. When using 100% scale (96 DPI), the icons appear in 16 x 16 pixels size,
        /// and they are scaled automatically for other DPI values.
        /// </summary>
        AutoScale,

        /// <summary>
        /// If an icon contains multiple resolutions, selecting always the closest resolution matching the current scale.
        /// The executing platform still may scale the selected icon image.
        /// </summary>
        GetNearestSize,
    }
}