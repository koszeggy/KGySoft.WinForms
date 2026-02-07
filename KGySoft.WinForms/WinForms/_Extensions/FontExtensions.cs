#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FontExtensions.cs
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
using System.Drawing;

using KGySoft.Reflection;
using KGySoft.WinForms.Reflection;

#endregion

namespace KGySoft.WinForms
{
    internal static class FontExtensions
    {
        #region Methods

        internal static bool IsDisposed(this Font font)
        {
            // Trying to access the inner native font, which is null if the font is already disposed.
            // NOTE: NativeFont is IntPtr on older frameworks and an unmanaged pointer on newer frameworks, but the returned value is IntPtr in both cases.
            if (font.GetNativeFont() is IntPtr ptr)
                return ptr == IntPtr.Zero;

            try
            {
                // The Height property reads the internal NativeFont property, which throws ArgumentException if the font is disposed.
                var _ = font.Height;
                return false;
            }
            catch (Exception e) when (!e.IsCritical())
            {
                return true;
            }
        }

        #endregion
    }
}