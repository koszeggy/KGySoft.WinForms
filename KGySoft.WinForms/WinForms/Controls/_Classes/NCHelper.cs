#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NCHelper.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Helper class for NC drawing routines
    /// </summary>
    internal static class NCHelper
    {
        #region Methods

        internal static unsafe void CalcSizeNC(IntPtr lParam, int borderWidth)
        {
            // actually if WParam is 1, the LParam points to an NCCALCSIZE_PARAMS structure rather than a RECT,
            // but as we only use the first field, we can always cast it to RECT after all
            var rect = (RECT*)lParam;
            rect->Top += borderWidth;
            rect->Bottom -= borderWidth;
            rect->Left += borderWidth;
            rect->Right -= borderWidth;
        }

        /// <summary>
        /// Draws a border in nonclient area.
        /// </summary>
        internal static void DrawBorderNC(IntPtr hWnd, Size size, AdvancedBorderStyle borderStyle, bool disableMirroring = false)
        {
            Debug.Assert(OSHelper.IsWindows);
            if (borderStyle == AdvancedBorderStyle.None)
                return;

            IntPtr hdc = User32.GetWindowDC(hWnd);
            try
            {
                // NOTE: Not passing disableMirroring to DrawBorder, which is intended. It is cheaper to disable mirroring before creating the Graphics instance.
                if (disableMirroring)
                    Gdi32.SetLayout(hdc, 0);
                using Graphics g = Graphics.FromHdc(hdc);
                g.DrawBorder(borderStyle, new Rectangle(Point.Empty, size));
            }
            finally
            {
                User32.ReleaseDC(hWnd, hdc);
            }
        }

        /// <summary>
        /// Invalidates non-client area
        /// </summary>
        internal static void InvalidateNC(IntPtr handle)
        {
            Debug.Assert(OSHelper.IsWindows);
            User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                Constants.SWP_NOMOVE | Constants.SWP_NOSIZE | Constants.SWP_NOZORDER |
                    Constants.SWP_NOACTIVATE | Constants.SWP_DRAWFRAME);
        }

        #endregion
    }
}
