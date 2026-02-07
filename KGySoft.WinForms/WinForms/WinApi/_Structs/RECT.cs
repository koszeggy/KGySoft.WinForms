#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RECT.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        #region Fields

        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;

        #endregion

        #region Constructors

        internal RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        internal RECT(Rectangle r)
        {
            Left = r.Left;
            Top = r.Top;
            Right = r.Right;
            Bottom = r.Bottom;
        }

        #endregion

        #region Methods

        #region Static Methods

        internal static RECT FromXYWH(int x, int y, int width, int height) => new(x, y, x + width, y + height);

        #endregion

        #region Instance Methods

        internal Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);

        #endregion

        #endregion
    }
}