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

#endregion

namespace KGySoft.WinForms
{
    internal static class FontExtensions
    {
        #region Methods

        internal static bool IsDisposed(this Font font)
        {
            // TODO: We could access the internal NativeFont (Mono: NativeObject) property by reflection, and use fallback only if the property is not found
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