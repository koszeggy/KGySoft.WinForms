#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SizeFExtensions.cs
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
    internal static class SizeFExtensions
    {
        #region Methods

        internal static Size Ceiling(this SizeF sizeF)
        {
            return new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
        }

        #endregion
    }
}