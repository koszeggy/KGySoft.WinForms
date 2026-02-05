#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UxTheme.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Native methods for UxTheme.dll
    /// </summary>
    internal static class UxTheme
    {
        #region Native Methods class

        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Initialize buffered painting for the current thread.
            /// </summary>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            /// <remarks>BufferedPaintInit is called before BeginBufferedPaint or BeginBufferedAnimation for each thread that uses these functions.
            /// Each call to BufferedPaintInit should be matched with a call to BufferedPaintUnInit when calls to buffered paint APIs are no longer needed. An application may call this API multiple times, as long as each call to BufferedPaintInit is balanced with a call to BufferedPaintUnInit.
            /// This function only needs to be called once in the lifetime of a thread. Typically, this function is called before creating the main application window, or during WM_CREATE. Call BufferedPaintUnInit after destroying the window, or during WM_NCDESTROY.</remarks>
            [DllImport("uxtheme.dll")]
            internal static extern int BufferedPaintInit();

            /// <summary>
            /// Closes down buffered painting for the current thread. Called once for each call to BufferedPaintInit after calls to BeginBufferedPaint are no longer needed.
            /// </summary>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int BufferedPaintUnInit();

            /// <summary>
            /// Begins a buffered animation operation. The animation consists of a cross-fade between the contents of two buffers over a specified period of time.
            /// </summary>
            /// <param name="hwnd">A handle to the window in which the animations play.</param>
            /// <param name="hdcTarget">A handle of the target DC on which the buffer is animated.</param>
            /// <param name="rcTarget">A pointer to a structure that specifies the area of the target DC in which to draw.</param>
            /// <param name="dwFormat">The format of the buffer.</param>
            /// <param name="pPaintParams">A pointer to a structure that defines the paint operation parameters. This value can be NULL.</param>
            /// <param name="pAnimationParams">A pointer to a structure that defines the animation operation parameters.</param>
            /// <param name="phdcFrom">When this function returns, this value points to the handle of the DC where the application should paint the initial state of the animation, if not NULL.</param>
            /// <param name="phdcTo">When this function returns, this value points to the handle of the DC where the application should paint the final state of the animation, if not NULL.</param>
            /// <returns>A handle to the buffered paint animation.</returns>
            /// <remarks>BeginBufferedAnimation will take care of drawing the intermediate frames between those two states by generating multiple WM_PAINT messages.
            /// BeginBufferedAnimation starts a timer that generates WM_PAINT messages on which BufferedPaintRenderAnimation should be called. During these messages, BufferedPaintRenderAnimation will return TRUE when it paints an intermediate frame, to signify that the application has no further painting to do.
            /// If the animation duration is zero, then only phdcTo is returned and phdcFrom is set to NULL. In this case, the application should paint the final state using phdcTo to get the behavior similar to BeginBufferedPaint.</remarks>
            [DllImport("uxtheme.dll")]
            internal static extern IntPtr BeginBufferedAnimation(IntPtr hwnd, IntPtr hdcTarget, [In]ref RECT rcTarget, BP_BUFFERFORMAT dwFormat, IntPtr pPaintParams, [In]ref BP_ANIMATIONPARAMS pAnimationParams, out IntPtr phdcFrom, out IntPtr phdcTo);

            /// <summary>
            /// Renders the first frame of a buffered animation operation and starts the animation timer.
            /// </summary>
            /// <param name="hbpAnimation">The handle to the buffered animation context that was returned by BeginBufferedAnimation.</param>
            /// <param name="fUpdateTarget">If TRUE, updates the target DC with the animation. If FALSE, the animation is not started, the target DC is not updated, and the hbpAnimation parameter is freed.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int EndBufferedAnimation(IntPtr hbpAnimation, bool fUpdateTarget);

            /// <summary>
            /// Paints the next frame of a buffered paint animation.
            /// </summary>
            /// <param name="hwnd">Handle to the window in which the animations play.</param>
            /// <param name="hdcTarget">Handle of the target DC on which the buffer is animated.</param>
            /// <returns>Returns TRUE if the frame has been painted, or FALSE otherwise.</returns>
            /// <remarks>
            /// If this function returns TRUE, the application should do no further painting. If this function returns FALSE, the application should paint normally.
            /// An application calls this function within its WM_PAINT handler. After BufferedPaintRenderAnimation paints an animation frame, an application will typically continue without performing its usual painting operations. If appropriate, an application may choose to render additional user interface (UI) over the top of the animation. The following code example, to be included as part of a larger body of code, shows how to use the animation painting functions.
            /// </remarks>
            [DllImport("uxtheme.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BufferedPaintRenderAnimation(IntPtr hwnd, IntPtr hdcTarget);

            /// <summary>
            /// Stops all buffered animations for the given window.
            /// </summary>
            /// <param name="hwnd">The handle of the window in which to stop all animations.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int BufferedPaintStopAllAnimations(IntPtr hwnd);

            /// <summary>
            /// Opens the theme data for a window and its associated class.
            /// </summary>
            /// <param name="hwnd">Handle of the window for which theme data is required.</param>
            /// <param name="pszClassList">Pointer to a string that contains a semicolon-separated list of classes.</param>
            /// <returns>OpenThemeData tries to match each class, one at a time, to a class data section in the active theme. If a match is found, an associated HTHEME handle is returned. If no match is found NULL is returned.</returns>
            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr OpenThemeData(IntPtr hwnd, string pszClassList);

            /// <summary>
            /// Closes the theme data handle.
            /// </summary>
            /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int CloseThemeData(IntPtr hTheme);

            /// <summary>
            /// Gets the duration for the specified transition.
            /// </summary>
            /// <param name="hTheme">Handle of the theme data.</param>
            /// <param name="iPartId">ID of the part.</param>
            /// <param name="iStateIdFrom">State ID of the part before the transition.</param>
            /// <param name="iStateIdTo">State ID of the part after the transition.</param>
            /// <param name="iPropId">Property ID.</param>
            /// <param name="pdwDuration">Address of a variable that receives the transition duration, in milliseconds.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int GetThemeTransitionDuration(IntPtr hTheme, int iPartId, int iStateIdFrom, int iStateIdTo, int iPropId, out int pdwDuration);

            /// <summary>
            /// Retrieves the value of a font property.
            /// </summary>
            /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
            /// <param name="hdc">HDC. This parameter may be set to NULL.</param>
            /// <param name="iPartId">Value of type int that specifies the part that contains the font property. See Parts and States.</param>
            /// <param name="iStateId">Value of type int that specifies the state of the part. See Parts and States.</param>
            /// <param name="iPropId">Value of type int that specifies the property to retrieve. For a list of possible values, see Property Identifiers.</param>
            /// <param name="pFont">Pointer to a LOGFONT structure that receives the font property value.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int GetThemeFont(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out LOGFONT pFont);

            /// <summary>
            /// Retrieves the value of a color property.
            /// </summary>
            /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
            /// <param name="iPartId">Value of type int that specifies the part that contains the color property. See Parts and States.</param>
            /// <param name="iStateId">Value of type int that specifies the state of the part. See Parts and States.</param>
            /// <param name="iPropId">Value of type int that specifies the property to retrieve. For a list of possible values, see Property Identifiers.</param>
            /// <param name="pColor">Pointer to a COLORREF structure that receives the color value.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int GetThemeColor(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out COLORREF pColor);

            /// <summary>
            /// Calculates the original size of the part defined by a visual style.
            /// </summary>
            /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
            /// <param name="hdc">HDC to select fonts into.</param>
            /// <param name="iPartId">Value of type int that specifies the part to calculate the size of. See Parts and States.</param>
            /// <param name="iStateId">Value of type int that specifies the state of the part. See Parts and States.</param>
            /// <param name="prc">Pointer to a RECT structure that contains the rectangle used for the part drawing destination. This parameter may be set to NULL.</param>
            /// <param name="eSize">Enumerated type that specifies the type of size to retrieve. See THEMESIZE for a list of type values.</param>
            /// <param name="size">Pointer to a SIZE structure that receives the dimensions of the specified part.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int GetThemePartSize(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, IntPtr prc, int eSize, out SIZE size);

            /// <summary>
            /// Draws the border and fill defined by the visual style for the specified control part.
            /// </summary>
            /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
            /// <param name="hdc">HDC used for drawing the theme-defined background image.</param>
            /// <param name="iPartId">Value of type int that specifies the part to draw. See Parts and States.</param>
            /// <param name="iStateId">Value of type int that specifies the state of the part to draw. See Parts and States.</param>
            /// <param name="pRect">Pointer to a RECT structure that contains the rectangle, in logical coordinates, in which the background image is drawn.</param>
            /// <param name="pClipRect">Pointer to a RECT structure that contains a clipping rectangle. This parameter may be set to NULL.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [DllImport("uxtheme.dll")]
            internal static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, [In]ref RECT pRect, IntPtr pClipRect);

            /// <summary>
            /// Retrieves the bitmap associated with a particular theme, part, state, and property.
            /// </summary>
            /// <param name="hTheme">A handle to theme data.</param>
            /// <param name="iPartId">The part that contains the bitmap. See Parts and States.</param>
            /// <param name="iStateId">The state of the part.</param>
            /// <param name="iPropId">The property to retrieve. Pass zero to automatically select the first available bitmap for this part and state, or use one of the following values:
            /// TMT_DIBDATA/TMT_GLYPHDIBDATA/TMT_HBITMAP</param>
            /// <param name="dwFlags">The flags that specify how the bitmap is to be retrieved. Can be one of the following values:
            /// GBF_DIRECT/GBF_COPY/GBF_VALIDBITS</param>
            /// <param name="phBitmap">A pointer that receives a handle to the requested bitmap.</param>
            /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            /// <remarks>If dwFlags is set to GBF_COPY, release the bitmap stored in phBitmap when no longer needed by calling DeleteObject.</remarks>
            [DllImport("uxtheme.dll")]
            internal static extern int GetThemeBitmap(IntPtr hTheme, int iPartId, int iStateId, int iPropId, uint dwFlags, out IntPtr phBitmap);

            #endregion
        }

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Gets a theme handle. It is not pre-validated if visual styles are available. Passes null pointer to hWnd,
        /// so the result should not be closed.
        /// </summary>
        internal static IntPtr OpenThemeDataGlobal(string className)
        {
            IntPtr hTheme = NativeMethods.OpenThemeData(IntPtr.Zero, className);
            if (hTheme == IntPtr.Zero)
                ThrowError(nameof(className));
            return hTheme;
        }

        /// <summary>
        /// The result must be closed with CloseThemeData.
        /// </summary>
        internal static IntPtr OpenThemeDataForWindow(IntPtr hwnd, string className)
        {
            IntPtr hTheme = NativeMethods.OpenThemeData(hwnd, className);
            if (hTheme == IntPtr.Zero)
                ThrowError(nameof(className));
            return hTheme;
        }

        internal static void CloseThemeData(IntPtr hTheme)
        {
            int hResult = NativeMethods.CloseThemeData(hTheme);
            if (hResult != Constants.S_OK)
                ThrowError(hResult);
        }

        internal static Color GetThemeColor(IntPtr hTheme, int part, int state, int prop, Color defaultColor)
        {
            int hResult = NativeMethods.GetThemeColor(hTheme, part, state, prop, out COLORREF color);

            // May occur when element is not defined in the current theme.
            // For example, text color for radio buttons and checkboxes may available in high contrast mode only.
            if (hResult != Constants.S_OK)
                return defaultColor;

            return Color.FromArgb(color.R, color.G, color.B);
        }

        internal static Size GetThemePartSize(IntPtr hTheme, IntPtr hdc, int part, int state, int type)
        {
            int hResult = NativeMethods.GetThemePartSize(hTheme, hdc, part, state, IntPtr.Zero, type, out SIZE size);
            if (hResult != Constants.S_OK)
                ThrowError(hResult);
            return size.ToSize();
        }

        internal static Font? GetThemeFont(IntPtr hTheme, int part, int state, int prop)
        {
            int hResult = NativeMethods.GetThemeFont(hTheme, IntPtr.Zero, part, state, prop, out LOGFONT logFont);
            return hResult != Constants.S_OK ? null : Font.FromLogFont(logFont);
        }

        internal static bool TryGetThemeTransitionDuration(IntPtr hTheme, int part, int stateFrom, int stateTo, int prop, out int duration)
        {
            int hResult = NativeMethods.GetThemeTransitionDuration(hTheme, part, stateFrom, stateTo, prop, out duration);
            return hResult == Constants.S_OK;
        }

        internal static void DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int part, int state, Rectangle bounds)
        {
            var rect = new RECT(bounds);
            int hResult = NativeMethods.DrawThemeBackground(hTheme, hdc, part, state, ref rect, IntPtr.Zero);
            if (hResult != Constants.S_OK)
                ThrowError(hResult);
        }

        // DOES NOT WORK, RETURNS ALWAYS THE BITMAPS OF 100% DPI, EVEN WHEN THE TRUE GLYPH SIZE REPORTS LARGER BITMAPS
        ///// <summary>
        ///// UxTheme.GetThemeBitmap has many issues, so this method uses massive workarounds:
        ///// - None of the documented TMT_* constants work, all of them returns E_INVALIDARG. A StackOverflow answer suggests using 3 instead of TMT_DIBDATA.
        ///// - Using 3 as iPropId returns every state in a single bitmap concatenated vertically.
        ///// - The returned bitmap always has Format32bppRgb pixel format and black background, though reinterpreting it as Format32bppPArgb restores the correct alpha pixels.
        ///// </summary>
        //public static Bitmap GetThemeBitmap(IntPtr hTheme, int part, int state, Size glyphSize)
        //{
        //    // Using 3 instead of TMT_DIBDATA. This returns a single bitmap with all states concatenated vertically.
        //    int hResult = NativeMethods.GetThemeBitmap(hTheme, part, state, /*Constants.TMT_DIBDATA*/3, Constants.GBF_DIRECT, out IntPtr hBitmap);
        //    if (hResult != Constants.S_OK)
        //        ThrowError(hResult);

        //    using Bitmap bmp = Image.FromHbitmap(hBitmap);
        //    Debug.Assert(bmp.Width == glyphSize.Width, $"{glyphSize} vs {bmp.Size}");

        //    // Getting the bitmap data with the original pixel format (Format32bppRgb), so it does not create a copy but provides access to the original bitmap memory area.
        //    var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

        //    // Reinterpreting both the size and the pixel format and returning a bitmap from the narrowed memory area.
        //    var result = new Bitmap(glyphSize.Width, glyphSize.Height, bitmapData.Stride, PixelFormat.Format32bppPArgb, (nint)bitmapData.Scan0 + bitmapData.Stride * glyphSize.Height * state);

        //    // Note that we release the bitmap data and dispose the original bitmap here, still, we return the new bitmap of the original bitmap memory area.
        //    // This is safe because we requested the bitmap with GBF_DIRECT flag, so it is not a copy.
        //    bmp.UnlockBits(bitmapData);
        //    return result;
        //}

        internal static bool BufferedPaintInit() => NativeMethods.BufferedPaintInit() == Constants.S_OK;

        internal static bool BufferedPaintUnInit() => NativeMethods.BufferedPaintUnInit() == Constants.S_OK;

        internal static IntPtr BeginBufferedAnimation(IntPtr hwnd, IntPtr hdc, Rectangle bounds, int speed, out IntPtr phdcFrom, out IntPtr phdcTo)
        {
            var rect = new RECT(bounds);
            var animParams = new BP_ANIMATIONPARAMS();
            animParams.cbSize = MarshalHelper.SizeOf<BP_ANIMATIONPARAMS>();
            animParams.style = BP_ANIMATIONSTYLE.BPAS_LINEAR;
            animParams.dwDuration = speed;
            return NativeMethods.BeginBufferedAnimation(hwnd, hdc, ref rect, BP_BUFFERFORMAT.BPBF_COMPATIBLEBITMAP, IntPtr.Zero, ref animParams, out phdcFrom, out phdcTo);
        }

        internal static bool EndBufferedAnimation(IntPtr hbpAnimation) => NativeMethods.EndBufferedAnimation(hbpAnimation, true) == Constants.S_OK;

        internal static bool BufferedPaintRenderAnimation(IntPtr hwnd, IntPtr hdcTarget) => NativeMethods.BufferedPaintRenderAnimation(hwnd, hdcTarget);

        internal static bool BufferedPaintStopAllAnimations(IntPtr hwnd) => NativeMethods.BufferedPaintStopAllAnimations(hwnd) == Constants.S_OK;

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowError(string paramName)
        {
            VisualStyleHelper.ClearCaches();
            if (!VisualStyleHelper.RenderWithVisualStyles)
                throw new InvalidOperationException(Res.NoVisualStyles);
            throw new ArgumentException(PublicResources.ArgumentInvalid, paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowError(int hResult)
        {
            VisualStyleHelper.ClearCaches();
            if (!VisualStyleHelper.RenderWithVisualStyles)
                throw new InvalidOperationException(Res.NoVisualStyles);
            throw new Win32Exception(hResult);
        }

        #endregion

        #endregion
    }
}
