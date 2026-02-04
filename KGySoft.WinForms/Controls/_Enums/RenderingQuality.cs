#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RenderingQuality.cs
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
    /// Represents the rendering quality of a control.
    /// </summary>
    public enum RenderingQuality
    {
        /// <summary>
        /// Represents the default rendering quality.
        /// </summary>
        SystemDefault,

        /// <summary>
        /// Represents lower quality but fast performance.
        /// </summary>
        Low,

        /// <summary>
        /// Represents high rendering quality.
        /// </summary>
        High
    }
}