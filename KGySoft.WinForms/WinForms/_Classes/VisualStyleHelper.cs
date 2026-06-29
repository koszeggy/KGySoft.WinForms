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
    /// Provides helper methods for working with visual styles. Unlike the public members of the <see cref="VisualStyleRenderer"/> class,
    /// it supports high-DPI or custom themed styles (e.g. Windows 10+ dark theme), newer theme classes (e.g. DatePicker), and some additional visual style values, such as font properties.
    /// </summary>
    public static class VisualStyleHelper
    {
        #region Fields

        private static readonly ThreadSafeDictionary<int, (SynchronizationContext? Context, EventHandler? Handler)> visualStylesChangedHandlers = new();

        private static readonly LockFreeCacheOptions hasDefaultAnimationCacheProfile = new()
        {
            InitialCapacity = 2,
            ThresholdCapacity = 2, // may contain only two entries, one for Button, one for CommandLinkButton
            MergeInterval = TimeSpan.FromMilliseconds(100)
        };

        // If a new theme is added, adjust the ClearCaches method as well
        private static IntPtr? buttonThemeHandle;
        private static IntPtr? datePickerThemeHandle;

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
        /// Unlike <see cref="Control.SystemColorsChanged">Control.SystemColorsChanged</see>, this event is raised for the <see cref="UserPreferenceCategory.VisualStyle"/> category
        /// of the <see cref="SystemEvents.UserPreferenceChanged"/> event, and makes sure that the cached values of <see cref="RenderWithVisualStyles"/> and <see cref="HighContrast"/> are always up-to-date.
        /// The event is raised from the same thread as the thread of the event subscription. Make sure unsubscribing is done from the same thread as subscribing, otherwise the event may leak memory.
        /// </summary>
        public static event EventHandler? VisualStylesChanged
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

        #region Public Properties

        /// <summary>
        /// Gets a cached value indicating whether visual styles are available.
        /// </summary>
        /// <remarks>
        /// <note>When using this property, use the <see cref="VisualStylesChanged"/> event of this class instead of <see cref="Control.SystemColorsChanged">Control.SystemColorsChanged</see>
        /// or <see cref="SystemEvents.UserPreferenceChanged">SystemEvents.UserPreferenceChanged</see> to make sure the delegate of the event subscription
        /// is always called in sync with the update of this property.</note>
        /// </remarks>
        public static bool RenderWithVisualStyles => visualStylesAvailable ??= Application.RenderWithVisualStyles;

        /// <summary>
        /// Gets a cached value indicating whether the operating system uses high contrast colors.
        /// </summary>
        /// <remarks>
        /// <note>When using this property, use the <see cref="VisualStylesChanged"/> event of this class instead of <see cref="Control.SystemColorsChanged">Control.SystemColorsChanged</see>
        /// or <see cref="SystemEvents.UserPreferenceChanged">SystemEvents.UserPreferenceChanged</see> to make sure the delegate of the event subscription
        /// is always called in sync with the update of this property.</note>
        /// </remarks>
        public static bool HighContrast => highContrast ??= SystemInformation.HighContrast;

        #endregion


        #region Internal Properties

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

        internal static IntPtr ButtonTheme => buttonThemeHandle ??= UxTheme.OpenThemeDataGlobal(Constants.ThemeClassButton);
        internal static IntPtr DatePickerTheme => datePickerThemeHandle ??= UxTheme.OpenThemeDataGlobal(Constants.ThemeDatePicker);

        #endregion

        #region Private Properties

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

        #endregion

        #endregion

        #region Constructors

        static VisualStyleHelper() => SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the part size of a themed element.
        /// </summary>
        /// <param name="className">The class name of the visual style element.</param>
        /// <param name="hwnd">A window handle to get the size of a specific control; otherwise, <see cref="IntPtr.Zero">IntPtr.Zero</see>.</param>
        /// <param name="dc">The device context to use for the operation.</param>
        /// <param name="part">An integer identifier that specifies the part to calculate the size of.</param>
        /// <param name="state">An integer identifier that specifies the state of the part.</param>
        /// <param name="actualSize"><see langword="true"/> to get the actual size of the themed glyph;
        /// <see langword="false"/> to get the possibly scaled size when the part is drawn. Can make a difference with high DPI settings.</param>
        /// <returns>A <see cref="Size"/> structure that receives the dimensions of the specified part.</returns>
        /// <remarks>
        /// <para>If <paramref name="hwnd"/> is not <see cref="IntPtr.Zero">IntPtr.Zero</see>, the result can consider the scaling of a
        /// particular control when the application has per-monitor DPI awareness.</para>
        /// <para>For <paramref name="className"/>, <paramref name="part"/> and <paramref name="state"/> you can use
        /// the predefined nested classes of the <see cref="VisualStyleElement"/> class.
        /// For more information see also the <a href="https://learn.microsoft.com/en-us/windows/win32/controls/parts-and-states" target="_blank">Parts and States</a> page.</para>
        /// </remarks>
        public static Size GetPartSize(string className, IntPtr hwnd, IDeviceContext dc, int part, int state, bool actualSize)
        {
            IntPtr hTheme = IntPtr.Zero;
            IntPtr hdc = dc.GetHdc();
            try
            {
                hTheme = UxTheme.OpenThemeDataForWindow(hwnd, className);
                return UxTheme.GetThemePartSize(hTheme, hdc, part, state, (int)(actualSize ? ThemeSizeType.True : ThemeSizeType.Draw));
            }
            finally
            {
                dc.ReleaseHdc();
                if (hTheme != IntPtr.Zero && hwnd != IntPtr.Zero)
                    UxTheme.CloseThemeData(hTheme);
            }
        }


        /// <summary>
        /// Renders the visual style element of the specified class, <paramref name="part"/> and <paramref name="state"/> to the specified device context.
        /// </summary>
        /// <param name="className">The class name of the visual style element.</param>
        /// <param name="hwnd">A window handle to use an instance-specific scaling or theme; otherwise, <see cref="IntPtr.Zero">IntPtr.Zero</see>.</param>
        /// <param name="dc">The device context to use for the operation.</param>
        /// <param name="part">An integer identifier that specifies the part to render.</param>
        /// <param name="state">An integer identifier that specifies the state of the part.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that specifies the bounds of the part to render.</param>
        /// <remarks>
        /// <para>If <paramref name="hwnd"/> is not <see cref="IntPtr.Zero">IntPtr.Zero</see>, the rendering can consider instance-specific
        /// visual style details, such as dark style or the scaling of a particular control when the application has per-monitor DPI awareness.</para>
        /// <para>For <paramref name="className"/>, <paramref name="part"/> and <paramref name="state"/> you can use
        /// the predefined nested classes of the <see cref="VisualStyleElement"/> class.
        /// For more information see also the <a href="https://learn.microsoft.com/en-us/windows/win32/controls/parts-and-states" target="_blank">Parts and States</a> page.</para>
        /// <note>If the size of <paramref name="bounds"/> differs from the actual size of the visual element (see also <see cref="GetPartSize">GetPartSize</see>),
        /// then the quality of the result may not be optimal. To render scaled visual style elements with high quality, use the <see cref="RenderScaled">RenderScaled</see> method.</note>
        /// </remarks>
        public static void Render(string className, IntPtr hwnd, IDeviceContext dc, int part, int state, Rectangle bounds)
        {
            IntPtr hTheme = IntPtr.Zero;
            IntPtr hdc = dc.GetHdc();
            try
            {
                hTheme = UxTheme.OpenThemeDataForWindow(hwnd, className);
                UxTheme.DrawThemeBackground(hTheme, hdc, part, state, bounds);
            }
            finally
            {
                dc.ReleaseHdc();
                if (hTheme != IntPtr.Zero && hwnd != IntPtr.Zero)
                    UxTheme.CloseThemeData(hTheme);
            }
        }

        /// <summary>
        /// Renders the visual style element of the specified class, <paramref name="part"/> and <paramref name="state"/> by scaling the actual glyph
        /// to the desired size specified in the <paramref name="bounds"/> parameter.
        /// </summary>
        /// <param name="className">The class name of the visual style element.</param>
        /// <param name="hwnd">A window handle to use an instance-specific scaling or theme; otherwise, <see cref="IntPtr.Zero">IntPtr.Zero</see>.</param>
        /// <param name="graphics">A <see cref="Graphics"/> instance to use as the target of the rendering.</param>
        /// <param name="part">An integer identifier that specifies the part to render.</param>
        /// <param name="state">An integer identifier that specifies the state of the part.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that specifies the bounds of the part to render.</param>
        /// <remarks>
        /// <para>If <paramref name="hwnd"/> is not <see cref="IntPtr.Zero">IntPtr.Zero</see>, the rendering can consider instance-specific
        /// visual style details, such as dark style or the scaling of a particular control when the application has per-monitor DPI awareness.</para>
        /// <para>For <paramref name="className"/>, <paramref name="part"/> and <paramref name="state"/> you can use
        /// the predefined nested classes of the <see cref="VisualStyleElement"/> class.
        /// For more information see also the <a href="https://learn.microsoft.com/en-us/windows/win32/controls/parts-and-states" target="_blank">Parts and States</a> page.</para>
        /// <note>To render freely scalable visual elements such as push buttons, use always the <see cref="Render">Render</see> method instead.
        /// This method is to scale visual elements of a fix size, such as a checkbox or radio button.</note>
        /// </remarks>
        public static void RenderScaled(string className, IntPtr hwnd, Graphics graphics, int part, int state, Rectangle bounds)
        {
            IntPtr hTheme = IntPtr.Zero;
            GraphicsState gState = graphics.Save();
            try
            {
                hTheme = UxTheme.OpenThemeDataForWindow(hwnd, className);

                // Does not work with UxTheme.GetThemeBitmap, because it ignores DPI and returns the smallest glyphs, even if true size is larger
                //using Bitmap bmp = UxTheme.GetThemeBitmap(hTheme, part, state, realSize);

                // Caching by hTheme is OK, even if we open/close the theme data for the control, because opening with the same DPI/color scheme tends to return the same handle.
                // Even if it wouldn't do so, the cache will drop and dispose the old bitmaps when it's full, or when the theme changes.
                Bitmap bmp = ThemeBitmapsCache[(hTheme, part, state)];
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.DrawImage(bmp, bounds);
            }
            catch (Exception e) when (!e.IsCritical())
            {
                Debug.Fail($"Failed to render scaled theme part {part} state {state}: {e.Message}");
                Render(className, hwnd, graphics, part, state, bounds);
            }
            finally
            {
                graphics.Restore(gState);
                if (hTheme != IntPtr.Zero && hwnd != IntPtr.Zero)
                    UxTheme.CloseThemeData(hTheme);
            }
        }

        /// <summary>
        /// Gets the color of a themed element, or <paramref name="defaultColor"/>, if no color is defined for the specified class, part and state.
        /// </summary>
        /// <param name="className">The class name of the visual style element.</param>
        /// <param name="hwnd">A window handle to use a possibly instance-specific theme; otherwise, <see cref="IntPtr.Zero">IntPtr.Zero</see>.</param>
        /// <param name="part">An integer identifier that specifies the part to render.</param>
        /// <param name="state">An integer identifier that specifies the state of the part.</param>
        /// <param name="defaultColor">The color to return if no color is defined for the specified class, part and state.</param>
        /// <returns>The color of the themed element, or <paramref name="defaultColor"/> if no color is defined for the specified class, part and state.</returns>
        public static Color GetTextColor(string className, IntPtr hwnd, int part, int state, Color defaultColor)
        {
            IntPtr hTheme = IntPtr.Zero;
            try
            {
                hTheme = UxTheme.OpenThemeDataForWindow(hwnd, className);
                return UxTheme.GetThemeColor(hTheme, part, state, Constants.TMT_COLOR, defaultColor);
            }
            finally
            {
                if (hTheme != IntPtr.Zero && hwnd != IntPtr.Zero)
                    UxTheme.CloseThemeData(hTheme);
            }
        }

        /// <summary>
        /// Gets the font of a visual style element, or <see langword="null"/>, if no font is defined for the specified class and part.
        /// </summary>
        /// <param name="className">The class name of the visual style element.</param>
        /// <param name="hwnd">A window handle to use a possibly instance-specific scaling; otherwise, <see cref="IntPtr.Zero">IntPtr.Zero</see>.</param>
        /// <param name="part">An integer identifier that specifies the part to render.</param>
        /// <returns>The font of the themed element, or <see langword="null"/>, if no font is defined for the specified class and part.</returns>
        public static Font? GetFont(string className, IntPtr hwnd, int part)
        {
            IntPtr hTheme = IntPtr.Zero;
            try
            {
                hTheme = UxTheme.OpenThemeDataForWindow(hwnd, className);
                return UxTheme.GetThemeFont(hTheme, part, 0, Constants.TMT_FONT);
            }
            finally
            {
                if (hTheme != IntPtr.Zero && hwnd != IntPtr.Zero)
                    UxTheme.CloseThemeData(hTheme);
            }
        }

        #endregion

        #region Internal Methods

        internal static void ClearCaches()
        {
            buttonThemeHandle = null;
            datePickerThemeHandle = null;
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
            if (!OSHelper.IsFrameworkMono)
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

            // On Windows, Framework Mono throws an exception for BufferedGraphicsContext.Allocate, so going on with the native solution
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
            IntPtr hTheme;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hdc = g.GetHdc();
                try
                {
                    hTheme = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassButton);
                    size = UxTheme.GetThemePartSize(hTheme, hdc, key.PartId, key.StateId1, (int)ThemeSizeType.True);
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }

            using Bitmap? bmp1 = PaintIntoBitmap(hTheme, key.PartId, key.StateId1, Color.White, size);
            using Bitmap? bmp2 = PaintIntoBitmap(hTheme, key.PartId, key.StateId2, Color.White, size);

            return bmp1 != null && !bmp1.EqualsByContent(bmp2);
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
            //        Therefore, this category is not captured here. Btw, Control.SystemColorsChanged is also triggered for the Color category.
            // VisualStyle: Using this instead of Color. It's triggered even when switching between non-visual style themes, not just when toggling visual styles on and off.
            //              Though Application.RenderWithVisualStyles would be alright even after the Color event, some system functions (e.g. BCM_GETIDEALSIZE - used by
            //              CommandLinkButton.GetPreferredSize when FlatStyle is System) still return the old values after Color, but the good ones when VisualStyle is raised.
            // General: Light/Dark mode or DPI. Unfortunately, VisualStyle and Color do not include Light/Dark mode changes, and General may be invoked multiple times.
            if (e.Category is UserPreferenceCategory.VisualStyle or UserPreferenceCategory.General)
            {
                ClearCaches();
                if (e.Category == UserPreferenceCategory.VisualStyle)
                    OnVisualStylesChanged(EventArgs.Empty);
                //else if (e.Category == UserPreferenceCategory.General)
                //    ThemeHelper.OnThemeChanged(EventArgs.Empty);
            }
        }

        #endregion

        #endregion
    }
}
