#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: POINT.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal struct POINT
    {
        #region Fields

        internal int x;
        internal int y;

        #endregion

        #region Constructors

        internal POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion
    }
}