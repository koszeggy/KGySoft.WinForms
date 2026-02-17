#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScaleHelper.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;
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

        private sealed class FormDpiChangeNotifier : IDisposable
        {
            #region FormNativeListener class

            private sealed class FormNativeListener : NativeWindow, IDisposable
            {
                #region Fields
                
                private readonly Control childControl;
                private readonly Form form;

                #endregion

                #region Constructors

                internal FormNativeListener(Control control, Form form)
                {
                    childControl = control;
                    this.form = form;
                    form.HandleCreated += Form_HandleCreated;
                    if (form.IsHandleCreated)
                        AssignHandle(form.Handle);
                }

                #endregion

                #region Methods

                #region Public Methods

                public void Dispose()
                {
                    ReleaseHandle();
                    form.HandleCreated -= Form_HandleCreated;
                }

                #endregion

                #region Protected Methods

                protected override void WndProc(ref Message m)
                {
                    switch (m.Msg)
                    {
                        case Constants.WM_DPICHANGED: // when form is a top-level form
                        case Constants.WM_DPICHANGED_BEFOREPARENT or Constants.WM_DPICHANGED_AFTERPARENT: // when form is an MDI child form
                            if (childControl is IPerMonitorDpiAware dpiAwareControl)
                                dpiAwareControl.ParentFormDpiChanging();
                            else
                                childControl.Invalidate();
                            base.WndProc(ref m);
                            (childControl as IPerMonitorDpiAware)?.ParentFormDpiChanged();
                            break;

                        default:
                            base.WndProc(ref m);
                            break;
                    }
                }

                #endregion

                #region Event Handlers

                private void Form_HandleCreated(object? sender, EventArgs e)
                {
                    Debug.Assert(sender == form);
                    Debug.Assert(form is not BaseForm, "Not expected to be subscribed when parent form is a BaseForm");
                    if (Handle == form.Handle)
                        return;
                    ReleaseHandle();
                    AssignHandle(form.Handle);
                }

                #endregion

                #endregion
            }

            #endregion

            #region Fields

            private readonly Control childControl;
            private readonly List<Control> parents = new();

            private Form? topLevelForm;
            private Form? mdiChildForm;
            private FormNativeListener? topLevelFormListener;
            private FormNativeListener? mdiChildFormListener;

            #endregion

            #region Constructors

            internal FormDpiChangeNotifier(Control host)
            {
                childControl = host;
                host.Disposed += Host_Disposed;
                ResetParents(true);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                childControl.Disposed -= Host_Disposed;
                ResetParents(false);
            }

            #endregion

            #region Private Methods

            private void ResetParents(bool registerCurrentParents)
            {
                // [When the top form is not BaseForm,] checking just HandleCreated of the top form is not enough, because (typically in .NET 7+) the form's
                // HandleCreated event is raised only after scaling all controls. Hence, we handle FontChanged as well (which is called when the controls are scaled
                // during the form creation), so we can detect the parent form's handle creation earlier, before the first WM_DPICHANGED message arrives.
                // Not needed when the parent form is a BaseForm, because subscribing to DeviceScaleChanging/DeviceScaleChanged events can be done before having a handle.
                foreach (Control control in parents)
                    control.ParentChanged -= Control_ParentChanged;
                if (parents.Count > 1 && topLevelForm != null)
                    parents[1].FontChanged -= Parent_FontChanged;
                parents.Clear();

                Form? topForm = null;
                Form? childForm = null;
                if (registerCurrentParents)
                {
                    for (Control? c = childControl; c != null; c = c.Parent)
                    {
                        if (c is Form form)
                        {
                            if (form.Parent == null)
                                topForm = form;
                            else
                            {
                                Debug.Assert(childForm == null, "Nested MDI forms are not expected");
                                childForm = form;
                            }
                        }

                        // If we have two forms, it means topForm is and MDI parent form: no need to subscribe its parent change
                        if (topForm != null && childForm != null)
                            break;

                        parents.Add(c);
                        c.ParentChanged += Control_ParentChanged;
                    }

                    if (parents.Count > 1 && topForm != null && topForm is not BaseForm)
                        parents[1].FontChanged += Parent_FontChanged;

                    if (ReferenceEquals(topForm, topLevelForm) && ReferenceEquals(childForm, mdiChildForm))
                        return;
                }

                if (topForm != topLevelForm && topForm != childControl)
                {
                    ReleaseForm(ref topLevelForm, ref topLevelFormListener);
                    if (topForm != null)
                        topLevelForm = RegisterForm(topForm, ref topLevelFormListener);
                }

                if (childForm != mdiChildForm && childForm != childControl)
                {
                    ReleaseForm(ref mdiChildForm, ref mdiChildFormListener);
                    if (childForm != null)
                        mdiChildForm = RegisterForm(childForm, ref mdiChildFormListener);
                }
            }

            private Form RegisterForm(Form form, ref FormNativeListener? nativeListener)
            {
                // BaseForm simplification: using DeviceScaleChanging/DeviceScaleChanged events instead of hooking the form's WndProc.
                // Not needed for functionality, but helps to avoid building up deep call stacks due to chaining, caused by multiple notification registrations.
                if (form is BaseForm baseForm)
                {
                    baseForm.DeviceScaleChanging += BaseForm_DeviceScaleChanging;
                    baseForm.DeviceScaleChanged += BaseForm_DeviceScaleChanged;
                }
                else
                {
                    // We are here when an IPerMonitorDpiAware implementing control is hosted in a non-BaseForm parent form.
                    // To be able to call the Before/After notifications, we need to hook the form's WndProc. In case of many controls, this can lead to deep call stacks.
                    nativeListener = new FormNativeListener(childControl, form);
                }

                return form;
            }

            private void ReleaseForm(ref Form? form, ref FormNativeListener? nativeListener)
            {
                if (form == null)
                    return;

                if (form is BaseForm baseForm)
                {
                    baseForm.DeviceScaleChanging -= BaseForm_DeviceScaleChanging;
                    baseForm.DeviceScaleChanged -= BaseForm_DeviceScaleChanged;
                }
                else
                    nativeListener?.Dispose();

                form = null;
                nativeListener = null;
            }

            #endregion

            #region Event handlers

            private void Control_ParentChanged(object? sender, EventArgs e) => ResetParents(true);

            private void BaseForm_DeviceScaleChanging(object? sender, DeviceScaleChangeEventArgs e)
            {
                if (childControl is IPerMonitorDpiAware dpiAwareControl)
                    dpiAwareControl.ParentFormDpiChanging();
                else
                    childControl.Invalidate();
            }

            private void BaseForm_DeviceScaleChanged(object? sender, DeviceScaleChangeEventArgs e) => (childControl as IPerMonitorDpiAware)?.ParentFormDpiChanged();

            private void Parent_FontChanged(object? sender, EventArgs e)
            {
                Debug.Assert(topLevelForm != null && topLevelForm is not BaseForm && topLevelFormListener != null, "Not expected to be subscribed when parent form is a BaseForm");
                Form? form = topLevelForm;
                if (form?.IsHandleCreated != true || topLevelFormListener!.Handle == form.Handle)
                    return;
                topLevelFormListener.ReleaseHandle();
                topLevelFormListener.AssignHandle(form.Handle);
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

        private static readonly bool isProcessPerMonitorAware = OSHelper.IsWindows81OrLater && ShCore.GetProcessDpiAwareness() >= PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        private static readonly Point systemInitialDpi = OSHelper.IsWindows ? GetDpiForHdc(User32.GetDC(IntPtr.Zero)) : GetDpiForHwnd(IntPtr.Zero);
        private static readonly PointF systemScale = new PointF(systemInitialDpi.X / DefaultDpi, systemInitialDpi.Y / DefaultDpi);
        private static readonly PointF defaultScale = new PointF(1f, 1f);
        private static readonly Size scrollbarFallbackReferenceSize = new Size(16, 16);

        private static Font? defaultFont;
        private static Font? dialogFont;
        private static Font? messageBoxFont;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the scale factor for 100% (1.0) scaling.
        /// </summary>
        public static PointF DefaultScale => defaultScale;

        /// <summary>
        /// If the application is DPI aware, gets the scale factor of the primary display at application startup. Otherwise, it returns the default scale factor of 100% (1.0).
        /// </summary>
        /// <remarks>
        /// <note>Even if the application has per-monitor DPI awareness enabled, this property always returns the same value, which is the scale factor of the primary display at application startup.
        /// To get the current scale factor of a control, use the <see cref="GetScale(Control)"/> method.</note>
        /// </remarks>
        public static PointF SystemScale => systemScale;

        /// <summary>
        /// Gets whether the process is per-monitor DPI aware. Per-monitor DPI awareness is available on Windows 8.1 or later.
        /// </summary>
        public static bool IsProcessPerMonitorAware => isProcessPerMonitorAware;

        /// <summary>
        /// Gets whether the current thread is per-monitor DPI aware. Thread-based per-monitor DPI awareness is available on Windows 10 Anniversary Update (1607) or later.
        /// </summary>
        public static bool IsThreadPerMonitorAware
        {
            get
            {
                // Cannot cache the result in a thread static field because a thread's DPI awareness can be changed by SetThreadDpiAwarenessContext
                if (!isProcessPerMonitorAware)
                    return false;

                if (!OSHelper.IsWindows10Build1607OrLater)
                    return true;

                IntPtr dpiAwareness = User32.GetThreadDpiAwarenessContext();
                return User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
                    || User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
            }
        }

        /// <summary>
        /// Gets the version of per-monitor DPI awareness of the current thread.
        /// Per-monitor DPI awareness V1 is available on Windows 8.1 or later, whereas
        /// per-monitor DPI awareness V2 and is available on Windows 10 Anniversary Update (1607) or later.
        /// </summary>
        public static int PerMonitorDpiAwarenessVersion
        {
            get
            {
                if (!isProcessPerMonitorAware)
                    return 0;
                if (!OSHelper.IsWindows10Build1607OrLater)
                    return 1;

                IntPtr dpiAwareness = User32.GetThreadDpiAwarenessContext();
                if (User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                    return 2;
                if (User32.AreDpiAwarenessContextsEqual(dpiAwareness, Constants.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE))
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// Practically gets the same value as <see cref="Control.DefaultFont">Control.DefaultFont</see>, but the result of this property is always in points that can be scaled correctly.
        /// </summary>
        /// <remarks>
        /// <note>This property may return different fonts on .NET Framework and .NET [Core], just like the <see cref="Control.DefaultFont">Control.DefaultFont</see> property. Use this property only
        /// to ensure to get a correctly scalable version of <see cref="Control.DefaultFont">Control.DefaultFont</see>. If you target both .NET Framework and .NET [Core] and you want to use the same font on both platforms,
        /// set the <see cref="Control.Font"/> property of your forms explicitly. You can use the <see cref="SystemFonts.MessageBoxFont">SystemFonts.MessageBoxFont</see> property, which returns the same font on both platforms,
        /// and returns a correctly scalable <see cref="Font"/> in points.</note>
        /// </remarks>
        public static Font DefaultFont
        {
            get
            {
                // NOTE: the result is cached, just like Control.DefaultFont, even though it can be risky if anyone can dispose it.
#if NETFRAMEWORK
                if (defaultFont == null)
                {
                    if (!OSHelper.IsWindows || !IsProcessPerMonitorAware && IsDefaultSystemScale)
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
#else
                return defaultFont ??= Control.DefaultFont;
#endif
            }
        }

        /// <summary>
        /// Gets whether the <see cref="SystemScale"/> property returns the default 100% (1.0) scale factor.
        /// </summary>
        public static bool IsDefaultSystemScale => systemScale == defaultScale;

        #endregion

        #region Internal Properties

        // Not making these properties public because the names are somewhat misleading, and they are cached, which could be an issue if they are misused.
        internal static Font DialogFont => dialogFont ??= SystemFonts.DialogFont;
        internal static Font MessageBoxFont => messageBoxFont ??= SystemFonts.MessageBoxFont ?? SystemFonts.DialogFont;

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets whether the display that the specified control is using has the same DPI as the initial DPI of the primary display.
        /// </summary>
        public static bool HasDefaultScaling(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));

            // Avoiding calling IsThreadPerMonitorAware twice, it's called in the GetDpiForHwnd method anyway
            return !isProcessPerMonitorAware || GetDpi(control) == systemInitialDpi;
        }

        /// <summary>
        /// Gets the current scale factor of the specified control. If per-monitor DPI awareness is not enabled, it always returns the same value as <see cref="SystemScale"/>.
        /// </summary>
        /// <param name="control">The control for which the scale factor is requested.</param>
        /// <returns>A <see cref="PointF"/> representing the scale factor of the control, where X and Y are the horizontal and vertical scale factors, respectively.</returns>.
        /// <remarks>
        /// <para>If the handle of the <paramref name="control"/> is not created yet, or if the control is not hosted in a window, this method returns the value of the <see cref="SystemScale"/> property.</para>
        /// </remarks>
        public static PointF GetScale(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));
            
            if (!isProcessPerMonitorAware)
                return systemScale;

            Point dpi = GetDpi(control);
            return new PointF(dpi.X / DefaultDpi, dpi.Y / DefaultDpi);
        }

        /// <summary>
        /// Gets the current scale factor of the specified window handle. If the handle is invalid or per-monitor DPI awareness is not enabled, it always returns the same value as <see cref="SystemScale"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the window for which the scale factor is requested.</param>
        /// <returns>A <see cref="PointF"/> representing the scale factor of the window, where X and Y are the horizontal and vertical scale factors, respectively.</returns>.
        public static PointF GetScale(IntPtr hWnd)
        {
            if (!isProcessPerMonitorAware || hWnd == IntPtr.Zero)
                return systemScale;
            Point dpi = GetDpiForHwnd(hWnd);
            return new PointF(dpi.X / DefaultDpi, dpi.Y / DefaultDpi);
        }

        /// <summary>
        /// Gets the current scale factor of the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object for which the scale factor is requested.</param>
        /// <returns>A <see cref="PointF"/> representing the scale factor of the graphics object, where X and Y are the horizontal and vertical scale factors, respectively.</returns>.
        /// <remarks>
        /// <para>If the process is not per-monitor DPI aware, the result is based on the <see cref="Graphics.DpiX"/> and <see cref="Graphics.DpiY"/> properties of the <paramref name="graphics"/> object.
        /// Otherwise, it attempts to retrieve the scaling of the window that the <paramref name="graphics"/> object is associated with.
        /// If no such window is found, it falls back to the <see cref="Graphics.DpiX"/> and <see cref="Graphics.DpiY"/> properties.</para>
        /// <note>Always try to use the <see cref="GetScale(Control)"/> overload in the first place. If the <paramref name="graphics"/> object is not associated with a control (e.g. when it is created from a bitmap),
        /// it may return unexpected results.</note>
        /// </remarks>
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

        /// <summary>
        /// Gets the current scale factor of the specified <see cref="Screen"/>.
        /// </summary>
        /// <param name="screen">The <see cref="Screen"/> for which the scale factor is requested.</param>
        /// <returns>A <see cref="PointF"/> representing the scale factor of the screen, where X and Y are the horizontal and vertical scale factors, respectively.</returns>.
        public static PointF GetScale(this Screen screen)
        {
            if (screen == null!)
                ThrowNull(nameof(screen));

            if (!IsThreadPerMonitorAware)
                return systemScale;

            // Unfortunately screen.Handle (HMONITOR) is not exposed publicly so we retrieve it by WinAPI.
            Debug.Assert(OSHelper.IsWindows, "Non-Windows platform per-monitor awareness");
            var rect = new RECT(screen.Bounds);
            IntPtr hMonitor = User32.MonitorFromRect(ref rect, Constants.MONITOR_DEFAULTTONEAREST);
            if (ShCore.TryGetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY))
                return new PointF(dpiX / DefaultDpi, dpiY / DefaultDpi);

            return systemScale;
        }

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the scale factor of the <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The control whose scale factor is used for scaling.</param>
        /// <param name="size">The <see cref="Size"/> to be scaled.</param>
        /// <returns>A <see cref="Size"/> representing the scaled size.</returns>
        public static Size ScaleSize(this Control control, Size size) => size.Scale(control.GetScale());

        /// <summary>
        /// Scales the specified <paramref name="width"/> by the horizontal scale factor of the <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The control whose horizontal scale factor is used for scaling.</param>
        /// <param name="width">The width to be scaled.</param>
        /// <returns>An integer value representing the scaled width.</returns>
        public static int ScaleWidth(this Control control, int width) => width.Scale(control.GetScale().X);

        /// <summary>
        /// Scales the specified <paramref name="height"/> by the vertical scale factor of the <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The control whose vertical scale factor is used for scaling.</param>
        /// <param name="height">The height to be scaled.</param>
        /// <returns>An integer value representing the scaled height.</returns>
        public static int ScaleHeight(this Control control, int height) => height.Scale(control.GetScale().Y);

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to be scaled.</param>
        /// <param name="scale">The scale factor to be applied, represented as a <see cref="PointF"/> where X is the horizontal scale and Y is the vertical scale.</param>
        /// <returns>A <see cref="SizeF"/> representing the scaled size.</returns>
        public static SizeF ScaleF(this Size size, PointF scale) => new SizeF(scale.X * size.Width, scale.Y * size.Height);

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to be scaled.</param>
        /// <param name="scale">The scale factor to be applied, represented as a <see cref="PointF"/> where X is the horizontal scale and Y is the vertical scale.</param>
        /// <returns>A rounded <see cref="Size"/> representing the scaled size.</returns>
        public static Size Scale(this Size size, PointF scale) => Size.Round(ScaleF(size, scale));

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="size">The integer value to be scaled.</param>
        /// <param name="scale">The scale factor to be applied.</param>
        /// <returns>A rounded integer value representing the scaled size.</returns>
        public static int Scale(this int size, float scale) => (int)Math.Round(size * scale);

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to be scaled.</param>
        /// <param name="scale">The scale factor to be applied.</param>
        /// <returns>A <see cref="Size"/> representing the scaled size.</returns>
        public static Size Scale(this Size size, float scale) => size.Scale(new PointF(scale, scale));

        /// <summary>
        /// Scales the specified <paramref name="size"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to be scaled.</param>
        /// <param name="scale">The scale factor to be applied.</param>
        /// <returns>A <see cref="SizeF"/> representing the scaled size.</returns>
        public static SizeF ScaleF(this Size size, float scale) => size.ScaleF(new PointF(scale, scale));

        /// <summary>
        /// Scales the specified <paramref name="padding"/> by the provided <paramref name="scale"/> factor.
        /// </summary>
        /// <param name="padding">The <see cref="Padding"/> to be scaled.</param>
        /// <param name="scale">The scale factor to be applied, represented as a <see cref="PointF"/> where X is the horizontal scale and Y is the vertical scale.</param>
        /// <returns>A <see cref="Padding"/> representing the scaled padding.</returns>
        public static Padding Scale(this Padding padding, PointF scale) => new Padding(
            padding.Left.Scale(scale.X),
            padding.Top.Scale(scale.Y),
            padding.Right.Scale(scale.X),
            padding.Bottom.Scale(scale.Y));

        /// <summary>
        /// Gets the specified <paramref name="font"/> if it is not null and is not equal to <see cref="Control.DefaultFont">Control.DefaultFont</see>; otherwise, returns the <see cref="DefaultFont"/>.
        /// </summary>
        /// <param name="font">The <see cref="Font"/> to check.</param>
        /// <returns>The specified <paramref name="font"/> if it is not null and not equal to <see cref="Control.DefaultFont">Control.DefaultFont</see>; otherwise, the value of the <see cref="DefaultFont"/> property.</returns>
        /// <remarks>
        /// <para>This method can be especially useful on .NET Framework, where the <see cref="Control.DefaultFont">Control.DefaultFont</see> property may return a non-scaled font, which is not suitable for high DPI scenarios.</para>
        /// </remarks>
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

        /// <summary>
        /// Gets the recommended width and height of scrollbars matching the current scaling of the specified <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The control for which the scrollbar size is requested.</param>
        /// <returns>A <see cref="Size"/> representing the recommended width and height of scrollbars.</returns>
        public static Size GetScrollBarSize(this Control control)
        {
            if (control == null!)
                ThrowNull(nameof(control));

            if (OSHelper.IsMono)
                return scrollbarFallbackReferenceSize.Scale(control.GetScale());

            int perMonitorDpiAwarenessVersion = PerMonitorDpiAwarenessVersion;
            if (perMonitorDpiAwarenessVersion == 0)
                return new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);

            return GetScrollbarSizeForDpi(GetDpi(control), perMonitorDpiAwarenessVersion);
        }

        /// <summary>
        /// Gets the recommended width and height of scrollbars matching the current scaling of the specified window handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window for which the scrollbar size is requested.</param>
        /// <returns>A <see cref="Size"/> representing the recommended width and height of scrollbars.</returns>
        public static Size GetScrollBarSize(IntPtr hWnd)
        {
            if (OSHelper.IsMono)
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
            if (!IsThreadPerMonitorAware)
                return;

            // No need to store a reference - the notifier will be disposed when the control is disposed.
            var _ = new FormDpiChangeNotifier(control);
        }

#if NET47_OR_GREATER || NETCOREAPP
        internal static bool IsParentScalingWhileCreated(this Control control)
        {
            // Skipping if the control is already created (not the handle), or when the handle of top-level control is not created yet.
            if (control.Created || control is Form)
                return false;
            Control? parentForm = control.FindForm();
            if (parentForm?.IsHandleCreated != true)
                return false;

            int deviceDpi = control.DeviceDpi;
            for (Control? c = control.Parent; c != null; c = c.Parent)
            {
                if (c.DeviceDpi != deviceDpi)
                    return true;

                // stopping at the first Form parent, because an already existing MDI parent would corrupt the result
                if (c == parentForm)
                    break;
            }

            return false;
        }
#endif

        /// <summary>
        /// Gets the scale of the top-level control or the parent control if there is no top-level control.
        /// Should be used to determine the effective scale of the parent font from OnParentChanged.
        /// </summary>
        internal static PointF GetScaleForParentChanged(this Control control)
        {
            if (!isProcessPerMonitorAware)
                return systemScale;

            // ISSUE: Typically in .NET 7+, the Parent.Font in OnParentChanged can be a scaled font with an unmatching DeviceDpi (and GetScale). Hence, using the top level control's scaling if possible.
            return (control.TopLevelControl ?? control.Parent ?? control).GetScale();
        }

        internal static PointF GetScaleForParentFontChanged(this Control control)
        {
            if (!isProcessPerMonitorAware)
                return systemScale;

            // ISSUE: We need to find the root control that triggered the font change and use the scale of that control.
            // The root cause can be either a font change or a parent change.
            Control? parent = control.Parent;
            if (parent == null)
                return control.GetScale();

            // For now, we only check if an IObservableParent is adding a control, and stop crawling up if a parent has a different font.
            Font parentFont = parent.Font;
            for (Control? c = parent; c != null; c = c.Parent)
            {
                if (c is IObservableParent op && (op.IsAddingControl || op.IsChangingFont))
                    return c.GetScale();

                // Important: we only know that we should stop the search when a different font is found, but cannot be sure
                // that we already passed the triggering control (e.g. if it resets default font), so not returning c.GetScale() here.
                if (!parentFont.Equals(c.Font))
                    break;
            }

            // Here we cannot be sure which parent triggered the font change, so we just return the scale of the direct parent.
            return parent.GetScale();
        }

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
            if (IsThreadPerMonitorAware)
            {
                // Windows 10 1607 or later
                if (OSHelper.IsWindows10Build1607OrLater)
                {
                    // NOTE: this always returns a single value, so we assume the same DPI in both dimensions.
                    var dpi = (int)User32.GetDpiForWindow(hwnd);
                    if (dpi != 0)
                        return new Point(dpi, dpi);
                }
                // Windows 8.1 or later
                else
                {
                    Debug.Assert(OSHelper.IsWindows81OrLater, "Supporting per-monitor awareness is expected on Windows only");
                    IntPtr hMonitor = User32.MonitorFromWindow(hwnd, Constants.MONITOR_DEFAULTTONEAREST);
                    if (ShCore.TryGetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY))
                        return new Point((int)dpiX, (int)dpiY);
                }
            }
            // Initializing system DPI on non-Windows platforms
            else if (!OSHelper.IsWindows && systemInitialDpi == default)
            {
                Debug.Assert(hwnd == IntPtr.Zero);
                using Graphics screen = Graphics.FromHwnd(hwnd);
                return new Point((int)screen.DpiX, (int)screen.DpiY);
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
