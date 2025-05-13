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

        // No need to use thread-safe caches here, because they are always read from the UI thread.
        // UserPreferenceChanged can be raised from any thread though (hence volatile), but it is not a problem if we always create a new instance when clearing the caches.
        private static volatile Cache<(IntPtr, int, int), Bitmap> themeBitmapsCache = CreateBitmapsCache();
        private static volatile Cache<(int, int, int), bool> hasDefaultAnimationCache = CreateHasDefaultAnimationCache();

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

        internal static Size GetPartSize(IntPtr hTheme, Control? control, Graphics g, int part, int state, bool actualSize)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control?.HasNonDefaultScaling() == true)
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

        internal static Color GetTextColor(IntPtr hTheme, int part, int state, Color defaultColor)
            => UxTheme.GetThemeColor(hTheme, part, state, Constants.TMT_COLOR, defaultColor);

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
            themeBitmapsCache = CreateBitmapsCache();
            foreach (Bitmap bitmap in oldCache.Values)
                bitmap.Dispose();

            hasDefaultAnimationCache = CreateHasDefaultAnimationCache();
        }

        internal static bool HasDefaultAnimation(int part, int state1, int state2) => hasDefaultAnimationCache[(part, state1, state2)];

        #endregion

        #region Private Methods

        private static Cache<(IntPtr, int, int), Bitmap> CreateBitmapsCache() => new Cache<(IntPtr ThemeHandle, int PartId, int StateId), Bitmap>(GetThemeBitmap, 32)
        {
            DisposeDroppedValues = true
        };

        private static Cache<(int, int, int), bool> CreateHasDefaultAnimationCache() => new(GetHasDefaultAnimation, 2);

        private static string GetClassName(IntPtr hTheme) => hTheme == buttonThemeHandle ? Constants.ThemeClassButton
            : hTheme == taskDialogThemeHandle ? Constants.ThemeClassTaskDialog
            : String.Empty; // Not throwing here for performance reasons so the method can be inlined. The exception will be thrown by UxTheme

        private static Bitmap GetThemeBitmap((IntPtr ThemeHandle, int PartId, int StateId) key)
        {
            // Cannot use UxTheme.GetThemeBitmap (see the issues there) so as a workaround, drawing into a black and a white bitmap, and restoring alpha.
            var (hTheme, part, state) = key;
            Size realSize = UxTheme.GetThemePartSize(hTheme, IntPtr.Zero, part, state, (int)ThemeSizeType.True);
            using Bitmap bmpBlack = PaintIntoBitmap(hTheme, part, state, Color.Black, realSize);
            using Bitmap bmpWhite = PaintIntoBitmap(hTheme, part, state, Color.White, realSize);
            return ReconstructWithAlpha(bmpBlack, bmpWhite);
        }

        private static Bitmap PaintIntoBitmap(IntPtr hTheme, int part, int state, Color backColor, Size size)
        {
            // Using just the hdc of g would cause black alpha-blended pixels, but using BufferedGraphics solves the problem
            var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);
            using var g = Graphics.FromImage(bitmap);
            using BufferedGraphicsContext context = new BufferedGraphicsContext();
            using BufferedGraphics bg = context.Allocate(g, new Rectangle(Point.Empty, size));
            bg.Graphics.Clear(backColor);
            var hdc = bg.Graphics.GetHdc();
            try
            {
                UxTheme.DrawThemeBackground(hTheme, hdc, part, state, new Rectangle(Point.Empty, size));
            }
            finally
            {
                bg.Graphics.ReleaseHdc(hdc);
            }

            bg.Render(g);
            return bitmap;
        }

        private static bool GetHasDefaultAnimation((int PartId, int StateId1, int StateId2) key)
        {
            // DPI does not matter here, because the animation is the same for all DPIs
            Size size;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                size = GetPartSize(ButtonTheme, null, g, key.PartId, key.StateId1, true);

            using Bitmap bmp1 = PaintIntoBitmap(ButtonTheme, key.PartId, key.StateId1, Color.White, size);
            using Bitmap bmp2 = PaintIntoBitmap(ButtonTheme, key.PartId, key.StateId2, Color.White, size);
            return !bmp1.EqualsByContent(bmp2);
        }

        private static Bitmap ReconstructWithAlpha(Bitmap blackBg, Bitmap whiteBg)
        {
            using IReadableBitmapData bmpDataWhite = whiteBg.GetReadableBitmapData();
            using IReadableBitmapData bmpDataBlack = blackBg.GetReadableBitmapData();
            int width = bmpDataWhite.Width;
            int height = bmpDataWhite.Height;
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using IWritableBitmapData bmpDataResult = result.GetWritableBitmapData();

            const uint black = 0xFF000000;
            const uint white = 0xFFFFFFFF;
            var rowBlack = bmpDataBlack.FirstRow;
            var rowWhite = bmpDataWhite.FirstRow;
            var rowResult = bmpDataResult.FirstRow;
            do
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 colorOnWhite = rowWhite[x];
                    Color32 colorOnBlack = rowBlack[x];

                    // colors are the same: no transparency
                    if (colorOnBlack == colorOnWhite)
                    {
                        rowResult[x] = colorOnWhite;
                        continue;
                    }

                    // colors equal to background: full transparency (no need to set the color, it's already transparent)
                    if (colorOnBlack.ToArgbUInt32() == black && colorOnWhite.ToArgbUInt32() == white)
                        continue;

                    // colors are different: calculate original color with alpha
                    rowResult[x] = RestoreAlphaColor(colorOnBlack, colorOnWhite);
                }
            } while (rowBlack.MoveNextRow() && rowWhite.MoveNextRow() && rowResult.MoveNextRow());

            return result;
        }

        private static Color32 RestoreAlphaColor(Color32 cb, Color32 cw)
        {
            float alphaR = 1f - (cw.R - cb.R) / 255f;
            float alphaG = 1f - (cw.G - cb.G) / 255f;
            float alphaB = 1f - (cw.B - cb.B) / 255f;
            float alpha = (alphaR + alphaG + alphaB) / 3f;
            if (alpha == 0f)
                return default; // fully transparent

            int r = (int)(cb.R / alpha);
            int g = (int)(cb.G / alpha);
            int b = (int)(cb.B / alpha);
            int a = (int)(alpha * 255);
            return new Color32((byte)a, (byte)r, (byte)g, (byte)b);
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
