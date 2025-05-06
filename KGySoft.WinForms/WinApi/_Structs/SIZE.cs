#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SIZE.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        #region Fields

        internal int cx;
        internal int cy;

        #endregion

        #region Constructors

        internal SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }

        internal SIZE(Size size) : this(size.Width, size.Height)
        {
        }

        #endregion

        #region Methods

        internal Size ToSize() => new(cx, cy);

        #endregion
    }
}