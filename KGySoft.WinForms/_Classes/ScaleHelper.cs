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
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    ///  Helper class for high DPI scaling.
    /// </summary>
    public static class ScaleHelper
    {
        #region FormDpiChangeNotifier class

        private sealed class FormDpiChangeNotifier : NativeWindow, IDisposable
        {
            #region Fields

            private readonly Control childControl;

            private Form? parentForm;

            #endregion

            #region Constructors

            internal FormDpiChangeNotifier(Control host)
            {
                childControl = host;
                host.ParentChanged += Host_ParentChanged;
                host.Disposed += Host_Disposed;
                ResetParent();
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                ReleaseHandle();
                childControl.ParentChanged -= Host_ParentChanged;
                childControl.Disposed -= Host_Disposed;
            }

            #endregion

            #region Protected Methods

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == Constants.WM_DPICHANGED)
                {
                    if (childControl is IPerMonitorDpiAware dpiAwareControl)
                        dpiAwareControl.ParentFormDpiChanged();
                    else
                        childControl.Invalidate();
                }

                base.WndProc(ref m);
            }

            #endregion

            #region Private Methods

            private void Host_ParentChanged(object? sender, EventArgs e) => ResetParent();

            private void ResetParent()
            {
                Form? newForm = childControl.FindForm();
                if (parentForm != null)
                {
                    ReleaseHandle();
                    parentForm.HandleCreated -= ParentForm_HandleCreated;
                }

                if (newForm != null)
                {
                    parentForm = newForm;
                    parentForm.HandleCreated += ParentForm_HandleCreated;
                    if (parentForm.IsHandleCreated)
                        AssignHandle(parentForm.Handle);
                }
            }

            #endregion

            #region Event handlers

            private void ParentForm_HandleCreated(object? sender, EventArgs e)
            {
                ReleaseHandle();
                if (sender is Form form)
                    AssignHandle(form.Handle);
            }

            private void Host_Disposed(object? sender, EventArgs e) => Dispose();

            #endregion

            #endregion
        }

        #endregion

        #region Constants

        internal const float DefaultDpi = 96f;

        #endregion

        #region Fields

        private static readonly bool isProcessPerMonitorAware = OSUtils.IsWindows81OrLater && ShCore.GetProcessDpiAwareness() >= PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        private static readonly Point systemInitialDpi = GetDpiForHdc(User32.GetDC(IntPtr.Zero));
        private static readonly PointF systemScale = new PointF(systemInitialDpi.X / DefaultDpi, systemInitialDpi.Y / DefaultDpi);
        private static readonly PointF defaultScale = new PointF(1f, 1f);
        private static readonly Size scrollbarFallbackReferenceSize = new Size(16, 16);

        private static Font? defaultFont;
        private static Font? dialogFont;

        #endregion

        #region Properties

        #region Public Properties

        public static PointF DefaultScale => defaultScale;
        public static PointF SystemScale => systemScale;

        public static bool IsProcessPerMonitorAware => isProcessPerMonitorAware;

        public static bool IsThreadPerMonitorAware
        {
            get
            {
                // Cannot cache the result in a thread static field because a thread's DPI awareness can be changed by SetThreadDpiAwarenessContext
                if (!isProcessPerMonitorAware)
                    return false;

                if (!OSUtils.IsWindows10Build1607OrLater)
                    return true;

                IntPtr dpiAwareness = User32.GetThreadDpiAwarenessContext();
                return User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
                    || User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
            }
        }

        public static int PerMonitorDpiAwarenessVersion
        {
            get
            {
                if (!isProcessPerMonitorAware)
                    return 0;
                if (!OSUtils.IsWindows10Build1607OrLater)
                    return 1;

                IntPtr dpiAwareness = User32.GetThreadDpiAwarenessContext();
                if (User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                    return 2;
                if (User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE))
                    return 1;
                return 0;
            }
        }

        #endregion

        #region Internal Properties

#if NETFRAMEWORK
        internal static Font DefaultFont
        {
            get
            {
                if (defaultFont == null)
                {
                    if (IsDefaultSystemScale)
                        defaultFont = Control.DefaultFont;
                    else
                    {
                        try
                        {
                            // Providing a workaround in .NET Framework for the case when the default font is not scaled correctly.
                            // This occurs when SystemFonts.DefaultFont returns the stock font DEFAULT_GUI_FONT, whose PointSize is smaller on higher DPIs.
                            // This is not always the case, e.g. with Arabic or Japanese locales the default font has a constant size in Points.
                            IntPtr stockFont = Gdi32.GetStockObject(Constants.DEFAULT_GUI_FONT);
                            using Font defaultGuiFont = Font.FromHfont(stockFont);
                            Font defaultControlFont = Control.DefaultFont;

                            defaultFont = Equals(defaultGuiFont.FontFamily, defaultControlFont.FontFamily) && defaultGuiFont.SizeInPoints.Equals(defaultControlFont.SizeInPoints)
                                ? new Font(defaultGuiFont.FontFamily, defaultGuiFont.Size / DefaultDpi * 72f, defaultGuiFont.Style, GraphicsUnit.Point, defaultGuiFont.GdiCharSet, defaultGuiFont.GdiVerticalFont)
                                : defaultControlFont;
                        }
                        catch (Exception e) when (!e.IsCritical())
                        {
                            defaultFont = Control.DefaultFont;
                        }
                    }
                }

                return defaultFont;
            }
        }

#else
        internal static Font DefaultFont => defaultFont ??= Control.DefaultFont;
#endif
        internal static Font DialogFont => dialogFont ??= SystemFonts.DialogFont;

        internal static bool IsDefaultSystemScale => systemScale == defaultScale;

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets whether the display that the specified control is using has the same DPI as the initial DPI of the primary display.
        /// </summary>
        public static bool HasDefaultScaling(this Control control)
            // Avoiding calling IsThreadPerMonitorAware twice, it's called in the GetDpiForHwnd method anyway
            => !isProcessPerMonitorAware || GetDpi(control) == systemInitialDpi;

        public static PointF GetScale(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));
            
            if (!isProcessPerMonitorAware)
                return systemScale;

            Point dpi = GetDpi(control);
            return new PointF(dpi.X / DefaultDpi, dpi.Y / DefaultDpi);
        }

        public static PointF GetScale(IntPtr hWnd)
        {
            if (!isProcessPerMonitorAware || hWnd == IntPtr.Zero)
                return systemScale;
            Point dpi = GetDpiForHwnd(hWnd);
            return new PointF(dpi.X / DefaultDpi, dpi.Y / DefaultDpi);
        }

        /// <summary>
        /// NOTE: May not work as expected if the <paramref name="graphics"/> is not created for a window (e.g. belongs to a bitmap or a buffered graphics).
        /// Try to use <see cref="GetScale(Control)"/> instead.
        /// </summary>
        public static PointF GetScale(this Graphics graphics)
        {
            if (graphics == null!)
                ThrowNull(nameof(graphics));

            if (!isProcessPerMonitorAware)
                return new PointF(graphics.DpiX / DefaultDpi, graphics.DpiY / DefaultDpi);

            IntPtr hdc = graphics.GetHdc();
            try
            {
                IntPtr hwnd = User32.WindowFromDC(hdc);
                if (hwnd != IntPtr.Zero)
                {
                    Point dpi = GetDpiForHwnd(hwnd);
                    return new PointF(dpi.X / DefaultDpi, dpi.Y / DefaultDpi);
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }

            return new PointF(graphics.DpiX / DefaultDpi, graphics.DpiY / DefaultDpi);
        }

        public static Size ScaleSize(this Control control, Size size) => size.Scale(control.GetScale());
        public static int ScaleWidth(this Control control, int width) => width.Scale(control.GetScale().X);
        public static int ScaleHeight(this Control control, int height) => height.Scale(control.GetScale().Y);
        public static SizeF ScaleF(this Size size, PointF scale) => new SizeF(scale.X * size.Width, scale.Y * size.Height);
        public static Size Scale(this Size size, PointF scale) => Size.Round(ScaleF(size, scale));
        public static int Scale(this int size, float scale) => (int)Math.Round(size * scale);
        public static Size Scale(this Size size, float scale) => size.Scale(new PointF(scale, scale));
        public static SizeF ScaleF(this Size size, float scale) => size.ScaleF(new PointF(scale, scale));

        public static Padding Scale(this Padding padding, PointF scale) => new Padding(
            padding.Left.Scale(scale.X),
            padding.Top.Scale(scale.Y),
            padding.Right.Scale(scale.X),
            padding.Bottom.Scale(scale.Y));

        public static Font GetFontOrDefault(Font? font)
        {
            if (font == null)
                return DefaultFont;

#if NETFRAMEWORK
            // NOTE: this is a workaround for the case when the default font is not scaled correctly.
            // It is important to compare to Control.DefaultFont and not SystemFonts.DefaultFont, because the latter always returns a new instance.
            if (ReferenceEquals(font, Control.DefaultFont))
                return DefaultFont;
#endif
            return font;
        }

        public static Size GetScrollbarSize(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));

            if (OSUtils.IsMono)
                return scrollbarFallbackReferenceSize.Scale(control.GetScale());

            int perMonitorDpiAwarenessVersion = PerMonitorDpiAwarenessVersion;
            if (perMonitorDpiAwarenessVersion == 0)
                return new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);

            return GetScrollbarSizeForDpi(GetDpi(control), perMonitorDpiAwarenessVersion);
        }

        public static Size GetScrollbarSize(IntPtr hWnd)
        {
            if (OSUtils.IsMono)
                return scrollbarFallbackReferenceSize.Scale(GetScale(hWnd));

            int perMonitorDpiAwarenessVersion = PerMonitorDpiAwarenessVersion;
            if (perMonitorDpiAwarenessVersion == 0 || hWnd == IntPtr.Zero)
                return new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);

            return GetScrollbarSizeForDpi(GetDpiForHwnd(hWnd), perMonitorDpiAwarenessVersion);
        }

        #endregion

        #region Internal Methods

        internal static void RegisterPerMonitorAwarenessNotifications(this Control control)
        {
            // Registering the notifier is required only for V1 awareness level. V2 provides direct notifications for the controls.
            if (PerMonitorDpiAwarenessVersion != 1)
                return;

            // No need to store a reference - the notifier will be disposed when the control is disposed.
            var _ = new FormDpiChangeNotifier(control);
        }

        #endregion

        #region Private Methods

        private static Point GetDpiForHdc(IntPtr hdc) => new(Gdi32.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSX), Gdi32.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSY));

        private static Point GetDpi(Control control)
        {
            if (!isProcessPerMonitorAware)
                return systemInitialDpi;

            if (!control.IsHandleCreated)
            {
                control = control.TopLevelControl ?? control;
                if (!control.IsHandleCreated)
                    return systemInitialDpi;
            }

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
                if (OSUtils.IsWindows10Build1607OrLater)
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

        private static Size GetScrollbarSizeForDpi(Point dpi, int perMonitorDpiAwarenessVersion)
        {
            Debug.Assert(IsThreadPerMonitorAware);

            if (perMonitorDpiAwarenessVersion >= 2)
            {
#if NET47_OR_GREATER || NETCOREAPP
                return new Size(SystemInformation.GetVerticalScrollBarWidthForDpi(dpi.X), SystemInformation.GetHorizontalScrollBarHeightForDpi(dpi.Y));
#else
                var result = new Size(User32.GetSystemMetricsForDpi(Constants.SM_CXVSCROLL, (uint)dpi.X),
                    User32.GetSystemMetricsForDpi(Constants.SM_CYHSCROLL, (uint)dpi.Y));
                if (result.Width > 0 && result.Height > 0)
                    return result;
#endif
            }

            // V1 awareness level
            var referenceSize = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
            if (dpi == systemInitialDpi)
                return referenceSize;
            PointF scale = new PointF(dpi.X / (float)systemInitialDpi.X, dpi.Y / (float)systemInitialDpi.Y);
            return referenceSize.Scale(scale);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNull(string parameterName) => throw new ArgumentNullException(parameterName, PublicResources.ArgumentNull);

        #endregion

        #endregion
    }
}
