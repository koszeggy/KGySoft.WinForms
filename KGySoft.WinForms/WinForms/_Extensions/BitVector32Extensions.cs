#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitVector32Extensions.cs
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

using System.Collections.Specialized;

#endregion

namespace KGySoft.WinForms
{
    internal static class BitVector32Extensions
    {
        #region Methods

        internal static bool Any(this BitVector32 data, int bits) => (data.Data & bits) != 0;
        internal static bool None(this BitVector32 data, int bits) => (data.Data & bits) == 0;

        #endregion
    }
}