#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NCHelper.cs
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
        internal static void DrawBorderNC(IntPtr hWnd, Size size, AdvancedBorderStyle borderStyle)
        {
            if (borderStyle == AdvancedBorderStyle.None)
                return;

            IntPtr hDC = User32.GetWindowDC(hWnd);
            try
            {
                using (Graphics g = Graphics.FromHdc(hDC))
                {
                    Rectangle rect = new Rectangle(Point.Empty, size);
                    switch (borderStyle)
                    {
                        case AdvancedBorderStyle.FixedSingle:
                            g.DrawRectangle(SystemPens.WindowFrame, 0, 0, size.Width - 1, size.Height - 1);
                            break;
                        case AdvancedBorderStyle.Raised:
                        case AdvancedBorderStyle.Flat:
                        case AdvancedBorderStyle.RaisedHigh:
                        case AdvancedBorderStyle.Sunken:
                        case AdvancedBorderStyle.SunkenLow:
                            ControlPaint.DrawBorder3D(g, rect, (Border3DStyle)borderStyle);
                            break;
                        case AdvancedBorderStyle.SunkenFrame:
                            ControlPaint.DrawBorder(g, rect, SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid);
                            ControlPaint.DrawBorder(g, new Rectangle(1, 1, size.Width - 2, size.Height - 2),
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid);
                            //ControlPaint.DrawBorder3D(g, rect, Border3DStyle.SunkenOuter);
                            //ControlPaint.DrawBorder3D(g, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.RaisedInner);
                            break;
                        case AdvancedBorderStyle.RaisedFrame:
                            ControlPaint.DrawBorder(g, rect, SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid);
                            ControlPaint.DrawBorder(g, new Rectangle(1, 1, size.Width - 2, size.Height - 2),
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
                                SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid);
                            //ControlPaint.DrawBorder3D(g, rect, Border3DStyle.RaisedInner);
                            //ControlPaint.DrawBorder3D(g, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.SunkenOuter);
                            break;
                    }
                }
            }
            finally
            {
                User32.ReleaseDC(hWnd, hDC);
            }
        }

        /// <summary>
        /// Invalidates non-client area
        /// </summary>
        internal static void InvalidateNC(IntPtr handle)
        {
            User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                Constants.SWP_NOMOVE | Constants.SWP_NOSIZE | Constants.SWP_NOZORDER |
                    Constants.SWP_NOACTIVATE | Constants.SWP_DRAWFRAME);
        }

        #endregion
    }
}
