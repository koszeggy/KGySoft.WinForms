#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FadingPainter.cs
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
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Helper class for buffered fading animations. The host control must implement the <see cref="ISupportsFading{TState}"/> interface.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    public class FadingPainter<TState> : IDisposable
    {
        #region Fields

        #region Static Fields

        [ThreadStatic]
        private static int threadOperatingCount;

        #endregion

        #region Instance Fields

        private ISupportsFading<TState> host;
        private bool disposed;
        private bool operating;
        private bool isFailing;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the stored last state explicitly. Setting this property does not invalidate the host control.
        /// </summary>
        public TState? State { get; set; }

        #endregion

        #region Internal Properties

        internal virtual bool Enabled
        {
            get
            {
#if NETFRAMEWORK || NET10_0_OR_GREATER
                return operating && !disposed && host.FadingAnimationsEnabled && FadingPainterInternal.IsSupported;
#else
                return operating && !disposed && host.FadingAnimationsEnabled && FadingPainterInternal.IsSupported && CanUseSystemPaint();
#endif
            }
            private protected set
            {
                if (value == operating)
                    return;

                isFailing = false;
                if (value)
                {
                    operating = FadingPainterInternal.IsSupported;
                    if (!operating)
                        return;

                    if (threadOperatingCount == 0)
                    {
                        operating = UxTheme.BufferedPaintInit();
                        if (!operating)
                            return;
                    }

                    threadOperatingCount += 1;
                    return;
                }

                operating = false;
                Debug.Assert(threadOperatingCount > 0, "FadingPainter: More disabling than enabling detected in the current thread");
                threadOperatingCount -= 1;
                if (threadOperatingCount == 0)
                    UxTheme.BufferedPaintUnInit();
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the host control.
        /// </summary>
        protected Control Control => (Control)host;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="FadingPainter{TState}"/> class.
        /// </summary>
        /// <param name="host">The host control that implements <see cref="ISupportsFading{TState}"/>.</param>
        /// <param name="initialState">The initial state of the host control.
        /// If <see langword="null"/>, you must set the <see cref="State"/> property before the control is painted for the first time.</param>
        public FadingPainter(ISupportsFading<TState> host, TState? initialState)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host), PublicResources.ArgumentNull);

            Debug.Assert(host is Control);
            State = initialState;
            this.host = host;
            HookEvents();
            Enabled = true;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Invokes the <see cref="ISupportsFading{TState}.PaintState">PaintState</see> method of the host control if <see cref="State"/> has been changed.
        /// If buffered fading animations are not available, it acts a regular painting session.
        /// </summary>
        /// <param name="e">Paint event args from the host control <see cref="System.Windows.Forms.Control.OnPaint"/> method or <see cref="System.Windows.Forms.Control.Paint"/> event handler.</param>
        public void Paint(PaintEventArgs e)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name, PublicResources.ObjectDisposed);

            if (!Enabled)
            {
                State = host.State;

                // On Windows, Framework Mono throws an exception from BufferedGraphicsContext.Allocate, so leaving the paint without double buffering
                if (OSHelper.IsWindowsMono)
                {
                    host.PaintState(State, e);
                    return;
                }

                // the original control must use a disabled double buffer, so using a buffer here
                using var context = new BufferedGraphicsContext();
                using BufferedGraphics bg = context.Allocate(e.Graphics, new Rectangle(Point.Empty, Control.ClientSize));
                context.Invalidate();
                using (var be = new PaintEventArgs(bg.Graphics, e.ClipRectangle))
                    host.PaintState(State, be);
                bg.Render(e.Graphics);

                return;
            }

            Debug.Assert(OSHelper.IsWindowsVistaOrLater);
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                // exiting, if fading is in progress and OnPaint was invoked just because of the animation
                if (UxTheme.BufferedPaintRenderAnimation(host.Handle, hdc))
                    return; 
            }
            finally
            {
                e.Graphics.ReleaseHdc(hdc);
            }

            PaintCore(e, host.State);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Internal Methods

        internal virtual void PaintCore(PaintEventArgs e, TState newState)
        {
            Debug.Assert(OSHelper.IsWindowsVistaOrLater);
            int speed = !StateEquals(State ??= host.State, newState) ? GetSpeed(State, newState) : 0;
            if (speed < 0)
                speed = 0;

            // Not falling back if speed is 0, because in this case only new state is drawn, using buffer.
            // Previous animations must be stopped. When not stopped and current paint is a change witout state change,
            // accidentally fading transitions may occur (e.g. Elevated state of a (CommandLink)Button).
            if (speed == 0)
                StopAnimations();

            IntPtr hbpAnimation;
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                hbpAnimation = UxTheme.BeginBufferedAnimation(Control.Handle, hdc, Control.ClientRectangle, speed, out IntPtr hdcFrom, out IntPtr hdcTo);
                if (hbpAnimation != IntPtr.Zero)
                {
                    isFailing = false;
                    if (hdcFrom != IntPtr.Zero)
                    {
                        using Graphics g = Graphics.FromHdc(hdcFrom);
                        host.PaintState(State ?? host.State, new PaintEventArgs(g, e.ClipRectangle));
                    }
                    if (hdcTo != IntPtr.Zero)
                    {
                        using Graphics g = Graphics.FromHdc(hdcTo);
                        host.PaintState(newState, new PaintEventArgs(g, e.ClipRectangle));
                    }

                    State = newState;
                    UxTheme.EndBufferedAnimation(hbpAnimation);
                }
            }
            finally
            {
                e.Graphics.ReleaseHdc(hdc);
            }

            Size clientSize = Control.ClientSize;
            if (hbpAnimation != IntPtr.Zero || clientSize.Width <= 0 || clientSize.Height <= 0)
                return;

            // Fallback: for two consecutive failures we turn off the animations for this control. Could be reset on visual styles change.
            State = newState;
            if (isFailing)
                Enabled = false;
            else
                isFailing = true;

            // On Windows, Mono throws an exception from BufferedGraphicsContext.Allocate, so leaving the paint without double buffering
            if (OSHelper.IsWindowsMono)
            {
                host.PaintState(newState, e);
                return;
            }

            using var context = new BufferedGraphicsContext();
            using BufferedGraphics bg = context.Allocate(e.Graphics, new Rectangle(Point.Empty, clientSize));
            context.Invalidate();
            using (PaintEventArgs be = new PaintEventArgs(bg.Graphics, e.ClipRectangle))
                host.PaintState(newState, be);
            bg.Render(e.Graphics);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called when the Windows theme has changed.
        /// </summary>
        protected virtual void OnThemeChanged()
        {
            // suspending if changing to classic theme
            if (operating && !FadingPainterInternal.IsSupported)
            {
                Enabled = false;
                return;
            }

            // resuming if changing back to Vista theme
            if (!operating && FadingPainterInternal.IsSupported)
                Enabled = true;
        }

        /// <summary>
        /// Gets the speed of the fading animation between the two specified states.
        /// When not overridden, the host's <see cref="ISupportsFading{TState}.GetFadingAnimationSpeed">GetFadingAnimationSpeed</see>
        /// is requested. If that returns a negative value, <see cref="ISupportsFading{TState}.FadingAnimationDefaultSpeed"/> is used.
        /// </summary>
        /// <param name="prevState">Previous state.</param>
        /// <param name="newState">New state.</param>
        /// <returns>An integer value representing animation speed in milliseconds.</returns>
        protected virtual int GetSpeed(TState prevState, TState newState)
        {
            int speed = host.GetFadingAnimationSpeed(prevState, newState);
            if (speed < 0)
                speed = host.FadingAnimationDefaultSpeed;

            if (speed < 0)
                speed = 0;

            return speed;
        }

        /// <summary>
        /// Gets whether the specified states are equal.
        /// </summary>
        /// <param name="prevState">The previous state.</param>
        /// <param name="newState">The new state.</param>
        /// <returns><see langword="true"/>, if states are equal; otherwise, <see langword="false"/>.</returns>
        protected virtual bool StateEquals(TState prevState, TState newState) => Equals(prevState, newState);

        /// <summary>
        /// Stops all animations for the host control.
        /// </summary>
        protected virtual void StopAnimations()
        {
            if (operating && FadingPainterInternal.IsSupported && host.IsHandleCreated)
                UxTheme.BufferedPaintStopAllAnimations(host.Handle);
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="FadingPainter{TState}"/> class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            UnhookEvents();
            if (operating)
            {
                if (Control.IsHandleCreated)
                    UxTheme.BufferedPaintStopAllAnimations(Control.Handle);
                Enabled = false;
            }

            if (disposing)
                host = null!;
            disposed = true;
        }

        #endregion

        #region Private Methods

        private void HookEvents()
        {
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_SystemColorsChanged;
            Control.SizeChanged += Control_SizeChanged;
        }

        private void UnhookEvents()
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_SystemColorsChanged;
            Control.SizeChanged -= Control_SizeChanged;
        }

#if NETCOREAPP && !NET10_0_OR_GREATER
        private bool CanUseSystemPaint()
        {
            if (Control.BackColor.A == Byte.MaxValue)
                return true;

            // alpha background color: paint can be corrupted with no double buffering if a parent has a background image - see https://github.com/dotnet/winforms/issues/13784
            for (Control? parent = Control.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ISafePaintBackground)
                    return true;
                if (parent.BackgroundImage != null)
                    return false;
                if (parent.BackColor.A == Byte.MaxValue)
                    return true;
            }

            return true;
        }
#endif

        #endregion

        #region Event Handlers

        void VisualStyleHelper_SystemColorsChanged(object? sender, EventArgs e) => OnThemeChanged();

        void Control_SizeChanged(object? sender, EventArgs e)
        {
            StopAnimations();
            Control.Invalidate();
        }

        #endregion

        #endregion
    }
}
