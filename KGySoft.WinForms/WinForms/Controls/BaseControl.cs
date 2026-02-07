#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseControl.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A base class for custom controls that provides some additional functionality.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseControl"/> class provides the following additional features:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the user control is disposed. To do that for the events of derived controls as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item><see cref="MouseHWheel"/> event for horizontal mouse wheel scrolling.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// </list>
    /// </remarks>
    public class BaseControl : Control
    {
        #region Fields

        #region Static Fields

        /// <summary>
        /// Gets the amount of the delta value of a single mouse wheel rotation increment.
        /// </summary>
        protected static readonly int MouseWheelScrollDelta = OSHelper.IsMono && OSHelper.IsWindows ? 120 : SystemInformation.MouseWheelScrollDelta;

        #endregion

        #region Instance Fields

        private readonly InvokeMarshaller invoker;

        private Exception? lastPaintError;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the horizontal mouse wheel is scrolled while the control has focus.
        /// </summary>
        [Category("BaseControl")]
        [Description("Occurs when the horizontal mouse wheel is scrolled while the control has focus.")]
        internal event EventHandler<HandledMouseEventArgs> MouseHWheel
        {
            add => Events.AddHandler(nameof(MouseHWheel), value);
            remove => Events.RemoveHandler(nameof(MouseHWheel), value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the control is in design mode. Unlike the <see cref="Component.DesignMode"/> property,
        /// this property works even during initialization.
        /// </summary>
        [Browsable(false)]
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseControl"/> class.
        /// </summary>
        protected BaseControl()
        {
            invoker = new InvokeMarshaller(this);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    try
                    {
                        base.WndProc(ref m);
                        lastPaintError = null; // resetting the last paint error if the paint was successful
                    }
                    catch (Exception e) when (!e.IsCritical())
                    {
                        if (lastPaintError == e)
                            throw;

                        lastPaintError = e;

                        // In Mono sometimes an internal GDI+ exception happens here
                        Invalidate();
                    }

                    break;

                // Horizontal scroll
                case Constants.WM_MOUSEHWHEEL:
                    HandledMouseEventArgs args = new HandledMouseEventArgs(MouseButtons.None, 0,
                            m.LParam.SignedLOWORD(), m.LParam.SignedHIWORD(), m.WParam.SignedHIWORD());
                    OnMouseHWheel(args);
                    m.Result = new IntPtr(args.Handled ? 0 : 1);
                    if (args.Handled)
                        return;
                    DefWndProc(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseHWheel"/> event with the specified <paramref name="e"/> argument.
        /// </summary>
        /// <param name="e">The event data to pass to the event handlers.</param>
        protected virtual void OnMouseHWheel(HandledMouseEventArgs e) => Events.GetHandler<EventHandler<HandledMouseEventArgs>>(nameof(MouseHWheel))?.Invoke(this, e);

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> on the thread that the control was created on.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// <para>This method is similar as using <see cref="Control.InvokeRequired"/> and <see cref="Control.Invoke(Delegate)"/> together,
        /// but it works even when the handle is not created yet, in which case <see cref="Control.InvokeRequired"/> returns <see langword="false"/>.</para>
        /// <para>The callback is invoked only if <see cref="Control.Disposing"/> and <see cref="Control.IsDisposed"/> properties return <see langword="false"/>.</para>
        /// </remarks>
        protected void InvokeOnUIThread(Action callback) => invoker.Invoke(callback);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                Events.Dispose();
        }

        #endregion
    }
}
