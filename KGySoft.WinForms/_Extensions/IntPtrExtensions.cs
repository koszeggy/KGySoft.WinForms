#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IntPtrExtensions.cs
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

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.WinForms
{
    [SuppressMessage("ReSharper", "RedundantCast", Justification = "False alarm, needed for .NET Framework")]
    internal static class IntPtrExtensions
    {
        #region Methods

        internal static int SignedLOWORD(this IntPtr value) => (short)((nint)value & 0xffff);
        internal static int SignedHIWORD(this IntPtr value) => (short)(((nint)value >> 16) & 0xffff);
        internal static int LOWORD(this IntPtr value) => (ushort)((nint)value & 0xffff);
        internal static int HIWORD(this IntPtr value) => (ushort)(((nint)value >> 16) & 0xffff);

        #endregion
    }
}