#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsExtensions.cs
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
using System.Drawing.Text;

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    internal static class GraphicsExtensions
    {
        #region Methods

        internal static void SetTextRenderingQuality(this Graphics graphics, RenderingQuality quality, bool isCompatibleTextRendering)
        {
            graphics.TextRenderingHint = quality switch
            {
                RenderingQuality.High => TextRenderingHint.ClearTypeGridFit,
                RenderingQuality.Low => isCompatibleTextRendering ? TextRenderingHint.SingleBitPerPixelGridFit : TextRenderingHint.AntiAlias,
                _ => TextRenderingHint.SystemDefault,
            };
        }

        #endregion
    }
}