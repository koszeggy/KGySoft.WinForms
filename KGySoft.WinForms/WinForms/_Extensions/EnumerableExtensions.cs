#if NETFRAMEWORK
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumerableExtensions.cs
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

using System.Collections.Generic;

#endregion

namespace KGySoft.WinForms
{
    internal static class EnumerableExtensions
    {
        #region Methods

        internal static IEnumerable<(TFirst, TSecond)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
            using IEnumerator<TFirst> e1 = first.GetEnumerator();
            using IEnumerator<TSecond> e2 = second.GetEnumerator();
            while (e1.MoveNext() && e2.MoveNext())
                yield return (e1.Current, e2.Current);
        }

        #endregion
    }
}
#endif