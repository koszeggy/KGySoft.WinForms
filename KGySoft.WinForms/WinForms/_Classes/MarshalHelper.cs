#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MarshalHelper.cs
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

using System;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms
{
    // To avoid CA2263
    internal static class MarshalHelper
    {
        #region Methods

        internal static T? PtrToStructure<T>(IntPtr ptr)
        {
#if NET451_OR_GREATER || NETCOREAPP
            return Marshal.PtrToStructure<T>(ptr);
#else
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
#endif
        }

        internal static void DestroyStructure<T>(IntPtr ptr)
        {
#if NET451_OR_GREATER || NETCOREAPP
            Marshal.DestroyStructure<T>(ptr);
#else
            Marshal.DestroyStructure(ptr, typeof(T));
#endif
        }

        internal static int SizeOf<T>()
        {
#if NET451_OR_GREATER || NETCOREAPP
            return Marshal.SizeOf<T>();
#else
            return Marshal.SizeOf(typeof(T));
#endif
        }

        #endregion
    }
}