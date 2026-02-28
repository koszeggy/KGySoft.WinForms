#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VisualStyleHelper.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

using Microsoft.Win32;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides helper methods for working with visual styles. We could use the VisualStyleRenderer class, but it always re-validates the
    /// class name and part combinations, it does not support some classes (e.g. DatePicker), and does not allow some values, such as font properties.
    /// </summary>
    internal static class VisualStyleHelper
    {
        #region Fields

        private static readonly ThreadSafeDictionary<int, (SynchronizationContext? Context, EventHandler? Handler)> visualStylesChangedHandlers = new();

        private static readonly LockFreeCacheOptions hasDefaultAnimationCacheProfile = new()
        {
            InitialCapacity = 2,
            ThresholdCapacity = 2, // may contain only two entries, one for Button, one for CommandLinkButton
            MergeInterval = TimeSpan.FromMilliseconds(100)
        };

        // If a new theme is added, adjust the GetClassName and ClearCaches methods as well
        private static IntPtr buttonThemeHandle;
        private static IntPtr taskDialogThemeHandle;
        private static IntPtr comboBoxThemeHandle;
        private static IntPtr datePickerThemeHandle;
        private static IntPtr spinThemeHandle;

        private static bool? visualStylesAvailable;
        private static bool? highContrast;
        private static bool? isComCtlV6Available;

        // Using thread-safe caches to support multiple UI threads. For the Bitmaps using a Cache<,> wrapped into a LockingDictionary instead of an IThreadSafeCacheAccessor,
        // so we can dispose the bitmaps when clearing the cache. Locking is not a problem, because we don't expect too many UI threads accessing the caches concurrently.
        private static LockingDictionary<(IntPtr, int, int), Bitmap>? themeBitmapsCache;
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
            add => visualStylesChangedHandlers.AddOrUpdate(ThreadHelper.ManagedThreadId,
                _ => (SynchronizationContext.Current, value),
                (_, v) => (v.Context, v.Handler + value));

            // Removing the handler from the thread where it was added.
            // When the thread is not the same, a new entry may be created with a corresponding context and a null handler.
            remove => visualStylesChangedHandlers.AddOrUpdate(ThreadHelper.ManagedThreadId,
                _ => (SynchronizationContext.Current, null),
                (_, v) => (v.Context, v.Handler - value));
        }

        #endregion

        #region Properties

        private static LockingDictionary<(IntPtr, int, int), Bitmap> ThemeBitmapsCache
        {
            get
            {
                Debug.Assert(OSHelper.IsWindowsXpOrLater);
                var cache = themeBitmapsCache;

                // unlike in HasDefaultAnimationCache, we don't need a while loop here, because this cache is instantiated once
                if (cache is null) // the while is needed because of ClearCaches
                {
                    // Creating a locking cache so DisposeDroppedValues can be set.
                    // Using AsThreadSafe instead of GetThreadSafeAccessor, so we can access not just the indexer, which is needed in ClearCaches.
                    Interlocked.CompareExchange(ref themeBitmapsCache,
                        new Cache<(IntPtr, int, int), Bitmap>(GetThemeBitmap, 32) { EnsureCapacity = true, DisposeDroppedValues = true }.AsThreadSafe(),
                        null);
                    cache = themeBitmapsCache;
                }

                return cache;
            }
        }

        private static IThreadSafeCacheAccessor<(int, int, int), bool> HasDefaultAnimationCache
        {
            get
            {
                Debug.Assert(OSHelper.IsWindowsVistaOrLater);
                var cache = hasDefaultAnimationCache;

                // the while is needed because we can nullify the instance in ClearCaches
                while (cache is null)
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
                if (!OSHelper.IsWindowsXpOrLater)
                {
                    isComCtlV6Available = false;
                    return false;
                }

                // visual styles are actually used
                if (RenderWithVisualStyles)
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

        internal static IntPtr SpinTheme => spinThemeHandle != IntPtr.Zero
            ? spinThemeHandle
            : spinThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeSpin);

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
                if (control?.IsHandleCreated == true && !control.HasDefaultScaling())
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
            Debug.Assert(OSHelper.IsWindowsXpOrLater);

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

        internal static Font? GetFont(IntPtr hTheme, int part) => UxTheme.GetThemeFont(hTheme, part, 0, Constants.TMT_FONT);

        internal static void ClearCaches()
        {
            buttonThemeHandle = IntPtr.Zero;
            taskDialogThemeHandle = IntPtr.Zero;
            comboBoxThemeHandle = IntPtr.Zero;
            datePickerThemeHandle = IntPtr.Zero;
            spinThemeHandle = IntPtr.Zero;
            visualStylesAvailable = null;
            highContrast = null;

            Volatile.Write(ref hasDefaultAnimationCache, null);
            LockingDictionary<(IntPtr, int, int), Bitmap>? bitmapsCache = themeBitmapsCache;
            if (bitmapsCache == null)
                return;

            // disposing the cached bitmaps
            ICollection<Bitmap> bitmaps;
            bitmapsCache.Lock();
            try
            {
                bitmaps = bitmapsCache.Values;
                bitmapsCache.Clear();
            }
            finally
            {
                bitmapsCache.Unlock();
            }

            bitmaps.ForEach(b => b.Dispose());
        }

        internal static bool HasDefaultAnimation(int part, int state1, int state2)
        {
            Debug.Assert(OSHelper.IsWindowsVistaOrLater && RenderWithVisualStyles);
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
            : hTheme == spinThemeHandle ? Constants.ThemeSpin
            : String.Empty; // Not throwing here for performance reasons so the method can be inlined. The exception will be thrown by UxTheme

        private static Bitmap GetThemeBitmap((IntPtr ThemeHandle, int PartId, int StateId) key)
        {
            Debug.Assert(OSHelper.IsWindowsXpOrLater);

            // Cannot use UxTheme.GetThemeBitmap (see the issues there) so as a workaround, drawing into a black and a white bitmap, and restoring alpha.
            var (hTheme, part, state) = key;
            Size realSize = UxTheme.GetThemePartSize(hTheme, IntPtr.Zero, part, state, (int)ThemeSizeType.True);
            using Bitmap? bmpBlack = PaintIntoBitmap(hTheme, part, state, Color.Black, realSize);
            using Bitmap? bmpWhite = PaintIntoBitmap(hTheme, part, state, Color.White, realSize);

            // fallback, although it has issues (always 100%, regardless of DPI)
            if (bmpBlack == null || bmpWhite == null)
                return UxTheme.GetThemeBitmap(hTheme, part, state, realSize);

            return ReconstructWithAlpha(bmpBlack, bmpWhite);
        }

        private static Bitmap? PaintIntoBitmap(IntPtr hTheme, int part, int state, Color backColor, Size size)
        {
            Debug.Assert(OSHelper.IsWindowsXpOrLater);
            if (!OSHelper.IsMono)
            {
                // Using just the hdc of g would cause black alpha-blended pixels, but using BufferedGraphics solves the problem
                var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);
                using Graphics g = Graphics.FromImage(bitmap);

                using var context = new BufferedGraphicsContext();
                using BufferedGraphics bg = context.Allocate(g, new Rectangle(Point.Empty, size));
                bg.Graphics.Clear(backColor);
                IntPtr hdc = bg.Graphics.GetHdc();
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

            // On Windows, Mono throws an exception for BufferedGraphicsContext.Allocate, so going on with the native solution
            // This bitmap is needed just for reference, so 1x1 size is alright
            using var refBitmap = new Bitmap(1, 1, PixelFormat.Format32bppRgb);
            using Graphics refGraphics = Graphics.FromImage(refBitmap);

            IntPtr refHdc = refGraphics.GetHdc();
            IntPtr compatibleDc = Gdi32.CreateCompatibleDC(refHdc);
            if (compatibleDc == IntPtr.Zero)
                return null;
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(refHdc, size.Width, size.Height);
            if (hBitmap == IntPtr.Zero)
                return null;
            try
            {
                Gdi32.SelectObject(compatibleDc, hBitmap);
                refGraphics.ReleaseHdc(refHdc);
                using Graphics g = Graphics.FromHdc(compatibleDc);
                g.Clear(backColor);
                IntPtr hdc = g.GetHdc();
                UxTheme.DrawThemeBackground(hTheme, hdc, part, state, new Rectangle(Point.Empty, size));
                g.ReleaseHdc(hdc);
                return Image.FromHbitmap(hBitmap);
            }
            finally
            {
                Gdi32.DeleteObject(hBitmap);
                Gdi32.DeleteObject(compatibleDc);
            }
        }

        private static bool GetHasDefaultAnimation((int PartId, int StateId1, int StateId2) key)
        {
            Debug.Assert(OSHelper.IsWindowsVistaOrLater);

            // DPI does not matter here, because the animation is the same for all DPIs
            Size size;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                size = GetPartSize(ButtonTheme, null, g, key.PartId, key.StateId1, true);

            using Bitmap? bmp1 = PaintIntoBitmap(ButtonTheme, key.PartId, key.StateId1, Color.White, size);
            using Bitmap? bmp2 = PaintIntoBitmap(ButtonTheme, key.PartId, key.StateId2, Color.White, size);
            return !bmp1.EqualsByContent(bmp2);
        }

        private static Bitmap ReconstructWithAlpha(Bitmap blackBg, Bitmap whiteBg)
        {
            #region Local Methods

            static Color32 RestoreWithAlpha(Color32 colorOnBlack, Color32 colorOnWhite)
            {
                const uint black = 0xFF000000;
                const uint white = 0xFFFFFFFF;

                // colors are the same: no transparency
                if (colorOnBlack == colorOnWhite)
                    return colorOnBlack;

                // colors equal to background: full transparency
                if (colorOnBlack.ToArgbUInt32() == black && colorOnWhite.ToArgbUInt32() == white)
                    return default;

                // colors are different: calculate original color with alpha
                float alphaR = 1f - (colorOnWhite.R - colorOnBlack.R) / 255f;
                float alphaG = 1f - (colorOnWhite.G - colorOnBlack.G) / 255f;
                float alphaB = 1f - (colorOnWhite.B - colorOnBlack.B) / 255f;
                float alpha = (alphaR + alphaG + alphaB) / 3f;
                if (alpha == 0f)
                    return default; // fully transparent

                int r = (int)(colorOnBlack.R / alpha);
                int g = (int)(colorOnBlack.G / alpha);
                int b = (int)(colorOnBlack.B / alpha);
                int a = (int)(alpha * 255);
                return new Color32((byte)a, (byte)r, (byte)g, (byte)b);
            }

            #endregion
            
            using IReadableBitmapData bmpDataWhite = whiteBg.GetReadableBitmapData();
            using IReadableBitmapData bmpDataBlack = blackBg.GetReadableBitmapData();
            int width = bmpDataWhite.Width;
            int height = bmpDataWhite.Height;
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using IWritableBitmapData bmpDataResult = result.GetWritableBitmapData();
            bmpDataBlack.Combine(bmpDataWhite, bmpDataResult, RestoreWithAlpha);

            return result;
        }

        private static void OnVisualStylesChanged(EventArgs e)
        {
            // VisualStylesChanged is a special event that raises the subscribers in the same thread as the subscription was made.
            // This is important because it is based on the UserPreferenceChanged event that can be raised from any thread, at least in .NET Core.
            int threadId = ThreadHelper.ManagedThreadId;
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

        private static void SystemEvents_UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
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
