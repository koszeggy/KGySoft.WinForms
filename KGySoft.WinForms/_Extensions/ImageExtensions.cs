#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensions.cs
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
using System.Drawing.Imaging;

using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.WinForms
{
    internal static class ImageExtensions
    {
        #region Methods

        // TODO: Remove when KGySoft.Drawing.ImageExtensions.ToGrayscale is fixed (does not return solid background anymore)
        internal static Image ToGrayscale(this Image image)
            => image.ConvertPixelFormat(PixelFormat.Format32bppArgb, PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray(), default, 0, autoBlend: false));

        #endregion
    }
}