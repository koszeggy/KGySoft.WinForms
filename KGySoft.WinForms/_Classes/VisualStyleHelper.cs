#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VisualStyleHelper.cs
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.Collections;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

using Microsoft.Win32;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides helper methods for working with visual styles. We could use the VisualStyleRenderer class,
    /// but it always re-validates the class name and part combinations, and does not allow some values, such as font properties.
    /// </summary>
    internal static class VisualStyleHelper
    {
        #region Fields

        private static IntPtr buttonThemeHandle;
        private static IntPtr taskDialogThemeHandle;
        private static bool? visualStylesAvailable;
        private static bool? highContrast;

        private static Cache<(IntPtr, int, int), Bitmap> themeBitmapsCache = CreateCache();

        #endregion

        #region Properties

        internal static bool RenderWithVisualStyles => visualStylesAvailable ??= Application.RenderWithVisualStyles;

        internal static bool HighContrast => highContrast ??= SystemInformation.HighContrast;

        internal static IntPtr ButtonTheme => buttonThemeHandle != IntPtr.Zero
            ? buttonThemeHandle
            : buttonThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassButton);

        internal static IntPtr TaskDialogTheme => taskDialogThemeHandle != IntPtr.Zero
            ? taskDialogThemeHandle
            : taskDialogThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassTaskDialog);

        #endregion

        #region Constructors

        static VisualStyleHelper()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal static Size GetPartSize(IntPtr hTheme, Control control, Graphics g, int part, int state, bool actualSize)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control.HasNonDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                return UxTheme.GetThemePartSize(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, hdc, part, state,
                    (int)(actualSize ? ThemeSizeType.True : ThemeSizeType.Draw));
            }
            finally
            {
                g.ReleaseHdc(hdc);
                if (hThemeWindow != IntPtr.Zero && hTheme != hThemeWindow)
                    UxTheme.CloseThemeData(hThemeWindow);
            }
        }

        internal static void Render(IntPtr hTheme, Control control, Graphics g, int part, int state, Rectangle bounds)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control.HasNonDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                UxTheme.DrawThemeBackground(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, hdc, part, state, bounds);
            }
            finally
            {
                g.ReleaseHdc(hdc);
                if (hThemeWindow != IntPtr.Zero && hTheme != hThemeWindow)
                    UxTheme.CloseThemeData(hThemeWindow);
            }
        }

        internal static void RenderScaled(IntPtr hTheme, Control control, Graphics g, int part, int state, Rectangle bounds)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            GraphicsState gState = g.Save();
            try
            {
                if (control.HasNonDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                // Does not work with UxTheme.GetThemeBitmap, because it ignores DPI and returns the smallest glyphs, even if true size is larger
                //using Bitmap bmp = UxTheme.GetThemeBitmap(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, part, state, realSize);

                // Caching by hTheme is OK, even if we open/close the theme data for the control, because opening with the same DPI/color scheme tends to return the same handle.
                // Even if it wouldn't do so, the cache will drop and dispose the old bitmaps when it's full, or when the theme changes.
                Bitmap bmp = themeBitmapsCache[(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, part, state)];
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(bmp, bounds);
            }
            catch (Exception e) when (!e.IsCritical())
            {
                Debug.Fail($"Failed to render scaled theme part {part} state {state}: {e.Message}");
                Render(hTheme, control, g, part, state, bounds);
            }
            finally
            {
                g.Restore(gState);
                if (hThemeWindow != IntPtr.Zero && hTheme != hThemeWindow)
                    UxTheme.CloseThemeData(hThemeWindow);
            }
        }

        internal static Color GetTextColor(IntPtr hTheme, int part, int state) => UxTheme.GetThemeColor(hTheme, part, state, Constants.TMT_COLOR);

        internal static Font GetFont(IntPtr hTheme, Graphics g, int part)
        {
            IntPtr hdc = g.GetHdc();
            try
            {
                return UxTheme.GetThemeFont(hTheme, hdc, part, 0, Constants.TMT_FONT);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        internal static void ClearCaches()
        {
            buttonThemeHandle = IntPtr.Zero;
            taskDialogThemeHandle = IntPtr.Zero;
            visualStylesAvailable = null;
            highContrast = null;

            var oldCache = themeBitmapsCache;
            themeBitmapsCache = CreateCache();
            foreach (Bitmap bitmap in oldCache.Values)
                bitmap.Dispose();
        }

        #endregion

        #region Private Methods

        private static Cache<(IntPtr, int, int), Bitmap> CreateCache() => new Cache<(IntPtr ThemeHandle, int PartId, int StateId), Bitmap>(GetThemeBitmap, 32)
        {
            DisposeDroppedValues = true
        };

        private static string GetClassName(IntPtr hTheme) => hTheme == buttonThemeHandle ? Constants.ThemeClassButton
            : hTheme == taskDialogThemeHandle ? Constants.ThemeClassTaskDialog
            : String.Empty; // Not throwing here for performance reasons so the method can be inlined. The exception will be thrown by UxTheme

        private static Bitmap GetThemeBitmap((IntPtr ThemeHandle, int PartId, int StateId) key)
        {
            // Cannot use UxTheme.GetThemeBitmap (see the issues there) so as a workaround, drawing into a black and a white bitmap, and restoring alpha.

            var (hTheme, part, state) = key;
            Size realSize = UxTheme.GetThemePartSize(hTheme, IntPtr.Zero, part, state, (int)ThemeSizeType.True);
            using Bitmap bmpBlack = new Bitmap(realSize.Width, realSize.Height);
            using (var g = Graphics.FromImage(bmpBlack))
            {
                g.Clear(Color.Black);
                var hdc = g.GetHdc();
                try
                {
                    UxTheme.DrawThemeBackground(hTheme, hdc, part, state, new Rectangle(Point.Empty, realSize));
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }

            using Bitmap bmpWhite = new Bitmap(realSize.Width, realSize.Height);
            using (var g = Graphics.FromImage(bmpWhite))
            {
                g.Clear(Color.White);
                var hdc = g.GetHdc();
                try
                {
                    UxTheme.DrawThemeBackground(hTheme, hdc, part, state, new Rectangle(Point.Empty, realSize));
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }

            return ReconstructWithAlpha(bmpBlack, bmpWhite);
        }

        private static Bitmap ReconstructWithAlpha(Bitmap blackBg, Bitmap whiteBg)
        {
            using var bmpDataWhite = whiteBg.GetReadableBitmapData();
            using var bmpDataBlack = blackBg.GetReadableBitmapData();
            int width = bmpDataWhite.Width;
            int height = bmpDataWhite.Height;
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var bmpDataResult = result.GetWritableBitmapData();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 colorOnWhite = bmpDataWhite.GetColor32(x, y);
                    Color32 colorOnBlack = bmpDataBlack.GetColor32(x, y);

                    // colors are the same: no transparency
                    if (colorOnBlack == colorOnWhite)
                    {
                        bmpDataResult.SetColor32(x, y, colorOnWhite);
                        continue;
                    }

                    // colors equal to background: full transparency
                    if (colorOnBlack.ToArgb() == Color.Black.ToArgb() && colorOnWhite.ToArgb() == Color.White.ToArgb())
                    {
                        bmpDataResult.SetColor32(x, y, default);
                        continue;
                    }

                    // colors are different: calculate original color with alpha
                    bmpDataResult.SetColor32(x, y, RestoreAlphaColor(colorOnBlack, colorOnWhite));
                }
            }

            return result;
        }

        private static Color32 RestoreAlphaColor(Color32 cb, Color32 cw)
        {
            static int Clamp(int value) => value < 0 ? 0 : (value > 255 ? 255 : value);

            int alphaR = (cw.R == cb.R) ? 255 : (255 * cb.R) / Math.Max(1, cw.R - cb.R);
            int alphaG = (cw.G == cb.G) ? 255 : (255 * cb.G) / Math.Max(1, cw.G - cb.G);
            int alphaB = (cw.B == cb.B) ? 255 : (255 * cb.B) / Math.Max(1, cw.B - cb.B);

            int alpha = Clamp((alphaR + alphaG + alphaB) / 3);

            if (alpha == 0)
                return Color.Empty;

            int r = Clamp((cb.R * 255) / alpha);
            int g = Clamp((cb.G * 255) / alpha);
            int b = Clamp((cb.B * 255) / alpha);

            return Color.FromArgb(alpha, r, g, b);
        }

        #endregion

        #region Event handlers

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category is UserPreferenceCategory.VisualStyle or UserPreferenceCategory.General) // General: Light/Dark mode or DPI
                ClearCaches();
        }

        #endregion

        #endregion
    }
}
