#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScaleHelper.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    ///  Helper class for scaling.
    /// </summary>
    internal static class ScaleHelper
    {
        #region Constants

        private const float defaultDpi = 96f;

        #endregion

        #region Fields

        private static readonly bool isProcessPerMonitorAware = WindowsUtils.IsWindows81OrLater && ShCore.GetProcessDpiAwareness() >= PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        private static readonly Point systemInitialDpi = GetDpiForHdc(User32.GetDC(IntPtr.Zero));
        private static readonly PointF systemScale = new PointF(systemInitialDpi.X / defaultDpi, systemInitialDpi.Y / defaultDpi);

        #endregion

        #region Properties

        private static bool IsThreadPerMonitorAware
        {
            get
            {
                // Cannot cache the result in a thread static field because a thread's DPI awareness can be changed by SetThreadDpiAwarenessContext
                if (!isProcessPerMonitorAware)
                    return false;

                if (!WindowsUtils.IsWindows10_1607OrLater)
                    return true;

                IntPtr dpiAwareness = User32.GetThreadDpiAwarenessContext();
                return User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
                    || User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
            }
        }

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Gets whether the display that the specified control is using has a different DPI than the initial DPI of the primary display.
        /// </summary>
        internal static bool HasNonDefaultScaling(this Control control)
            // Avoiding calling IsThreadPerMonitorAware twice, it's called in the GetDpiForHwnd method anyway
            => isProcessPerMonitorAware && GetDpi(control) != systemInitialDpi;

        internal static PointF GetScale(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));
            
            if (!isProcessPerMonitorAware)
                return systemScale;

            Point dpi = GetDpi(control);
            return new PointF(dpi.X / defaultDpi, dpi.Y / defaultDpi);
        }

        internal static PointF GetScale(IntPtr handle)
        {
            if (!isProcessPerMonitorAware)
                return systemScale;
            Point dpi = GetDpiForHwnd(handle);
            return new PointF(dpi.X / defaultDpi, dpi.Y / defaultDpi);
        }

        /// <summary>
        /// NOTE: May not work as expected if the <paramref name="graphics"/> is not created for a window (e.g. belongs to a bitmap or a buffered graphics).
        /// Try to use <see cref="GetScale(Control)"/> instead.
        /// </summary>
        internal static PointF GetScale(this Graphics graphics)
        {
            if (graphics == null!)
                ThrowNull(nameof(graphics));

            if (!isProcessPerMonitorAware)
                return new PointF(graphics.DpiX / defaultDpi, graphics.DpiY / defaultDpi);

            IntPtr hdc = graphics.GetHdc();
            try
            {
                IntPtr hwnd = User32.WindowFromDC(hdc);
                if (hwnd != IntPtr.Zero)
                {
                    Point dpi = GetDpiForHwnd(hwnd);
                    return new PointF(dpi.X / defaultDpi, dpi.Y / defaultDpi);
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }

            return new PointF(graphics.DpiX / defaultDpi, graphics.DpiY / defaultDpi);
        }

        internal static Size ScaleSize(this Control control, Size size) => size.Scale(control.GetScale());
        internal static int ScaleWidth(this Control control, int width) => width.Scale(control.GetScale().X);
        internal static int ScaleHeight(this Control control, int height) => height.Scale(control.GetScale().Y);
        internal static SizeF ScaleF(this Size size, PointF scale) => new SizeF(scale.X * size.Width, scale.Y * size.Height);
        internal static Size Scale(this Size size, PointF scale) => Size.Round(ScaleF(size, scale));
        internal static int Scale(this int size, float scale) => (int)Math.Round(size * scale);

        #endregion

        #region Private Methods

        private static Point GetDpiForHdc(IntPtr hdc) => new(Gdi32.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSX), Gdi32.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSY));

        private static Point GetDpi(Control control)
        {
            if (!isProcessPerMonitorAware || !control.IsHandleCreated)
                return systemInitialDpi;

            // NOTE: we could use control.DeviceDpi here on .NET Framework 4.7 or later, but it fails in some cases:
            // .NET Framework: if app.config is not set to per-monitor DPI aware (even though it's set in the manifest) OR Windows 10 compatibility mode is not set in the manifest
            // .NET [Core]: it ignores every per-monitor DPI awareness setting, except PerMonitorV2
            return GetDpiForHwnd(control.Handle);
        }

        private static Point GetDpiForHwnd(IntPtr hwnd)
        {
            Debug.Assert(isProcessPerMonitorAware);

            if (IsThreadPerMonitorAware)
            {
                // Windows 10 1607 or later
                if (WindowsUtils.IsWindows10_1607OrLater)
                {
                    // NOTE: this always returns a single value, so we assume the same DPI in both dimensions.
                    var dpi = (int)User32.GetDpiForWindow(hwnd);
                    if (dpi != 0)
                        return new Point(dpi, dpi);
                }
                // Windows 8.1 or later
                else
                {
                    IntPtr hMonitor = User32.MonitorFromWindow(hwnd, Constants.MONITOR_DEFAULTTONEAREST);
                    if (ShCore.TryGetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY))
                        return new Point((int)dpiX, (int)dpiY);
                }
            }

            // Not per-monitor aware, or fallback when the WinAPI calls above fail.
            return systemInitialDpi;
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNull(string parameterName) => throw new ArgumentNullException(parameterName, PublicResources.ArgumentNull);

        #endregion

        #endregion
    }
}
