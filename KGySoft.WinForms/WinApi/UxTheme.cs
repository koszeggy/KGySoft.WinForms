#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UxTheme.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Native methods for UxTheme.dll
    /// </summary>
    internal static class UxTheme
    {
        #region Methods

        #region Public Methods

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
        [DllImport("uxtheme")]
        public static extern int GetThemeTransitionDuration(IntPtr hTheme, int iPartId, int iStateIdFrom, int iStateIdTo, int iPropId, out int pdwDuration);

        /// <summary>
        /// Retrieves the value of a font property.
        /// </summary>
        /// <param name="hTheme">Handle to a window's specified theme data. Use OpenThemeData to create an HTHEME.</param>
        /// <param name="hdc">HDC. This parameter may be set to NULL.</param>
        /// <param name="iPartId">Value of type int that specifies the part that contains the font property. See Parts and States.</param>
        /// <param name="iStateId">Value of type int that specifies the state of the part. See Parts and States.</param>
        /// <param name="iPropId">Value of type int that specifies the property to retrieve. For a list of possible values, see Property Identifiers.</param>
        /// <param name="pFont">Pointer to a LOGFONT structure that receives the font property value.</param>
        /// <returns></returns>
        [DllImport("uxtheme")]
        public static extern int GetThemeFont(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out LOGFONT pFont);

        #endregion

        #region Internal Methods

        /// <summary>
        /// Initialize buffered painting for the current thread.
        /// </summary>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        /// <remarks>BufferedPaintInit is called before BeginBufferedPaint or BeginBufferedAnimation for each thread that uses these functions.
        /// Each call to BufferedPaintInit should be matched with a call to BufferedPaintUnInit when calls to buffered paint APIs are no longer needed. An application may call this API multiple times, as long as each call to BufferedPaintInit is balanced with a call to BufferedPaintUnInit.
        /// This function only needs to be called once in the lifetime of a thread. Typically, this function is called before creating the main application window, or during WM_CREATE. Call BufferedPaintUnInit after destroying the window, or during WM_NCDESTROY.</remarks>
        [DllImport("uxtheme")]
        internal static extern int BufferedPaintInit();

        /// <summary>
        /// Closes down buffered painting for the current thread. Called once for each call to BufferedPaintInit after calls to BeginBufferedPaint are no longer needed.
        /// </summary>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("uxtheme")]
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
        [DllImport("uxtheme")]
        internal static extern IntPtr BeginBufferedAnimation(IntPtr hwnd, IntPtr hdcTarget, ref RECT rcTarget, BP_BUFFERFORMAT dwFormat, IntPtr pPaintParams, ref BP_ANIMATIONPARAMS pAnimationParams, out IntPtr phdcFrom, out IntPtr phdcTo);

        /// <summary>
        /// Renders the first frame of a buffered animation operation and starts the animation timer.
        /// </summary>
        /// <param name="hbpAnimation">The handle to the buffered animation context that was returned by BeginBufferedAnimation.</param>
        /// <param name="fUpdateTarget">If TRUE, updates the target DC with the animation. If FALSE, the animation is not started, the target DC is not updated, and the hbpAnimation parameter is freed.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("uxtheme")]
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
        [DllImport("uxtheme")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BufferedPaintRenderAnimation(IntPtr hwnd, IntPtr hdcTarget);

        /// <summary>
        /// Stops all buffered animations for the given window.
        /// </summary>
        /// <param name="hwnd">The handle of the window in which to stop all animations.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("uxtheme")]
        internal static extern int BufferedPaintStopAllAnimations(IntPtr hwnd);

        /// <summary>
        /// Opens the theme data for a window and its associated class.
        /// </summary>
        /// <param name="hwnd">Handle of the window for which theme data is required.</param>
        /// <param name="pszClassList">Pointer to a string that contains a semicolon-separated list of classes.</param>
        /// <returns>OpenThemeData tries to match each class, one at a time, to a class data section in the active theme. If a match is found, an associated HTHEME handle is returned. If no match is found NULL is returned.</returns>
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        internal static extern IntPtr OpenThemeData(IntPtr hwnd, string pszClassList);

        #endregion

        #endregion
    }
}
