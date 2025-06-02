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
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.Collections;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.WinForms.Reflection;
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

        private static readonly ThreadSafeDictionary<int, (SynchronizationContext? Context, EventHandler? Handler)> visualStylesChangedHandlers = new();

        private static readonly LockFreeCacheOptions themeBitmapsCacheProfile = new()
        {
            InitialCapacity = 4,
            ThresholdCapacity = 32,
            MergeInterval = TimeSpan.FromMilliseconds(100)
        };

        private static readonly LockFreeCacheOptions hasDefaultAnimationCacheProfile = new()
        {
            InitialCapacity = 2,
            ThresholdCapacity = 2, // may contain only two entries, one for Button, one for CommandLinkButton
            MergeInterval = TimeSpan.FromMilliseconds(100)
        };

        // If a new theme is added, adjust the GetClassName method as well
        private static IntPtr buttonThemeHandle;
        private static IntPtr taskDialogThemeHandle;
        private static IntPtr comboBoxThemeHandle;
        private static IntPtr datePickerThemeHandle;

        private static bool? visualStylesAvailable;
        private static bool? highContrast;
        private static bool? isComCtlV6Available;

        // Using thread-safe caches to support multiple UI threads
        private static IThreadSafeCacheAccessor<(IntPtr, int, int), Bitmap>? themeBitmapsCache;
        private static IThreadSafeCacheAccessor<(int, int, int), bool>? hasDefaultAnimationCache;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the visual styles have changed.
        /// Unlike Control.SystemColorsChanged, this event is raised for the VisualStyle category of UserPreferenceChanged, and
        /// makes sure that the cached value of <see cref="RenderWithVisualStyles"/> is always up-to-date.
        /// The event is raised from the same thread as event subscription. Make sure unsubscribing is done on the same thread as subscribing, otherwise the event may leak memory.
        /// </summary>
        internal static event EventHandler? VisualStylesChanged
        {
            // Capturing the context when adding the first handler from a thread.
            // No need to combine the delegates in a thread-safe way, because the values themselves are always accessed from the same thread.
            add => visualStylesChangedHandlers.AddOrUpdate(Thread.CurrentThread.ManagedThreadId,
                _ => (SynchronizationContext.Current, value),
                (_,v) => (v.Context, v.Handler + value));

            // Removing the handler from the thread where it was added.
            // When the thread is not the same, a new entry may be created with a corresponding context and a null handler.
            remove => visualStylesChangedHandlers.AddOrUpdate(Thread.CurrentThread.ManagedThreadId,
                _ => (SynchronizationContext.Current, null),
                (_, v) => (v.Context, v.Handler - value));
        }

        #endregion

        #region Properties

        private static IThreadSafeCacheAccessor<(IntPtr, int, int), Bitmap> ThemeBitmapsCache
        {
            get
            {
                var cache = themeBitmapsCache;
                while (cache is null) // the while is needed because of ClearCaches
                {
                    Interlocked.CompareExchange(ref themeBitmapsCache, ThreadSafeCacheFactory.Create<(IntPtr, int, int), Bitmap>(GetThemeBitmap, themeBitmapsCacheProfile), null);
                    cache = themeBitmapsCache;
                }

                return cache;
            }
        }

        private static IThreadSafeCacheAccessor<(int, int, int), bool> HasDefaultAnimationCache
        {
            get
            {
                var cache = hasDefaultAnimationCache;
                while (cache is null) // the while is needed because of ClearCaches
                {
                    Interlocked.CompareExchange(ref hasDefaultAnimationCache, ThreadSafeCacheFactory.Create<(int, int, int), bool>(GetHasDefaultAnimation, hasDefaultAnimationCacheProfile), null);
                    cache = hasDefaultAnimationCache;
                }

                return cache;
            }
        }

        /// <summary>
        /// Gets a cached value indicating whether visual styles are available.
        /// NOTE: when using this property, use VisualStylesChanged of this class instead of Control.SystemColorsChanged or SystemEvents.UserPreferenceChanged
        ///       to make sure the delegate of the event subscription is always called in sync with the update of this property.
        /// </summary>
        internal static bool RenderWithVisualStyles => visualStylesAvailable ??= Application.RenderWithVisualStyles;

        /// <summary>
        /// Gets whether comctl32.dll V6 is available, without loading it explicitly.
        /// After all tells, whether <see cref="Application.EnableVisualStyles"/> was already called in the current application.
        /// </summary>
        internal static bool InitializedWithVisualStyles
        {
            get
            {
                if (isComCtlV6Available.HasValue)
                    return isComCtlV6Available.Value;

                // pre-XP: no visual styles
                if (!WindowsUtils.IsWindowsXpOrLater)
                {
                    isComCtlV6Available = false;
                    return false;
                }

                // visual styles are actually used
                if (VisualStyleHelper.RenderWithVisualStyles)
                {
                    isComCtlV6Available = true;
                    return true;
                }

                // Here EnableVisualStyles was either called but classic theme is used (true result) or visual styles were not enabled at all (false result)
                // We could use the Comctl32ActivationContext and get the dll version of comctl32, but then V6 would be loaded accidentally, causing that controls
                // begin to use visual styles in non-System mode.
                isComCtlV6Available = Accessors.ComCtlSupportsVisualStyles;
                return isComCtlV6Available.Value;
            }
        }

        internal static bool HighContrast => highContrast ??= SystemInformation.HighContrast;

        internal static IntPtr ButtonTheme => buttonThemeHandle != IntPtr.Zero
            ? buttonThemeHandle
            : buttonThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassButton);

        internal static IntPtr TaskDialogTheme => taskDialogThemeHandle != IntPtr.Zero
            ? taskDialogThemeHandle
            : taskDialogThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassTaskDialog);

        internal static IntPtr ComboBoxTheme => comboBoxThemeHandle != IntPtr.Zero
            ? comboBoxThemeHandle
            : comboBoxThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassComboBox);

        internal static IntPtr DatePickerTheme => datePickerThemeHandle != IntPtr.Zero
            ? datePickerThemeHandle
            : datePickerThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeDatePicker);

        #endregion

        #region Constructors

        static VisualStyleHelper() => SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        #endregion

        #region Methods

        #region Internal Methods

        internal static Size GetPartSize(IntPtr hTheme, Control? control, Graphics g, int part, int state, bool actualSize)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control?.HasDefaultScaling() == false)
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
                if (!control.HasDefaultScaling())
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
                if (!control.HasDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                // Does not work with UxTheme.GetThemeBitmap, because it ignores DPI and returns the smallest glyphs, even if true size is larger
                //using Bitmap bmp = UxTheme.GetThemeBitmap(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, part, state, realSize);

                // Caching by hTheme is OK, even if we open/close the theme data for the control, because opening with the same DPI/color scheme tends to return the same handle.
                // Even if it wouldn't do so, the cache will drop and dispose the old bitmaps when it's full, or when the theme changes.
                Bitmap bmp = ThemeBitmapsCache[(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, part, state)];
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

        internal static Font? GetFont(IntPtr hTheme, Graphics g, int part)
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

            // Not disposing the cached bitmaps - they will be freed by the GC (unless a caller or GetThemeBitmap holds a reference to them).
            Volatile.Write(ref themeBitmapsCache, null);
            Volatile.Write(ref hasDefaultAnimationCache, null);
        }

        internal static bool HasDefaultAnimation(int part, int state1, int state2)
        {
            Debug.Assert(RenderWithVisualStyles);
            if (!RenderWithVisualStyles)
                return false;
            return HasDefaultAnimationCache[(part, state1, state2)];
        }

        #endregion

        #region Private Methods

        private static string GetClassName(IntPtr hTheme) => hTheme == buttonThemeHandle ? Constants.ThemeClassButton
            : hTheme == taskDialogThemeHandle ? Constants.ThemeClassTaskDialog
            : hTheme == comboBoxThemeHandle ? Constants.ThemeClassComboBox
            : hTheme == datePickerThemeHandle ? Constants.ThemeDatePicker
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

        private static void OnVisualStylesChanged(EventArgs e)
        {
            // VisualStylesChanged is a special event that raises the subscribers in the same thread as the subscription was made.
            // This is important because it is based on the UserPreferenceChanged event that can be raised from any thread, at least in .NET Core.
            int threadId = Thread.CurrentThread.ManagedThreadId;
            foreach (var handlersPerThread in visualStylesChangedHandlers)
            {
                // If the thread is the same or the context is null, invoking the event handler directly; otherwise, using the context to invoke it.
                if (threadId == handlersPerThread.Key || handlersPerThread.Value.Context == null)
                    handlersPerThread.Value.Handler?.Invoke(null, e);
                else
                    handlersPerThread.Value.Context.Send(_ => handlersPerThread.Value.Handler?.Invoke(null, e), null);
            }
        }

        #endregion

        #region Event handlers

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Color: For compatibility reasons, Color is always raised besides VisualStyle when visual styles change, and Color change is emitted before VisualStyle.
            //        Control.SystemColorsChanged is also triggered for the Color category.
            // VisualStyle: Using this instead of Color. It's triggered even when switching between non-visual style themes, not just when toggling visual styles on and off.
            //              Though Application.RenderWithVisualStyles would be alright even after the Color event, some system functions (e.g. BCM_GETIDEALSIZE - used by
            //              CommandLinkButton.GetPreferredSize when FlatStyle is System) still return the old values after Color, but the good ones when VisualStyle is raised.
            // General: Light/Dark mode or DPI. Unfortunately, VisualStyle and Color do not include Light/Dark mode changes, and General may be invoked multiple times.
            if (e.Category is UserPreferenceCategory.VisualStyle or UserPreferenceCategory.General)
            {
                ClearCaches();
                if (e.Category == UserPreferenceCategory.VisualStyle)
                    OnVisualStylesChanged(EventArgs.Empty);
            }
        }

        #endregion

        #endregion
    }
}
