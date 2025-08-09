#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FadingPainter.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Helper class for fading animations. Host control must implement <see cref="ISupportsFading{TState}"/> interface.
    /// </summary>
    public class FadingPainter<TState> : IDisposable
    {
        #region Fields

        private ISupportsFading<TState> host;
        private bool disposed;
        private bool operating;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the stored last state explicitly. Setting this property does not
        /// invalidate the host control.
        /// </summary>
        public TState? State { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the host control.
        /// </summary>
        protected Control Control => (Control)host;

        /// <summary>
        /// Gets whether the fading painter is enabled.
        /// </summary>
        protected virtual bool Enabled
#if NETFRAMEWORK || NET10_0_OR_GREATER
            => operating && !disposed && host.FadingAnimationsEnabled && FadingPainterInternal.IsSupported;
#else
            => operating && !disposed && host.FadingAnimationsEnabled && FadingPainterInternal.IsSupported && CanUseSystemPaint();
#endif

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="FadingPainter{TState}"/>.
        /// </summary>
        /// <param name="host">The host control that implements <see cref="ISupportsFading{TState}"/>.</param>
        /// <param name="initialState">Initial state of the host control.</param>
        public FadingPainter(ISupportsFading<TState> host, TState? initialState)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            Debug.Assert(host is Control);
            operating = FadingPainterInternal.IsSupported && UxTheme.BufferedPaintInit();
            State = initialState;
            this.host = host;
            HookEvents();
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Invokes painting using fading animation if state has been changed and fading is supported and enabled.
        /// </summary>
        /// <param name="e">Paint event args from the host control <see cref="System.Windows.Forms.Control.OnPaint"/> method or <see cref="System.Windows.Forms.Control.Paint"/> event handler.</param>
        public void Paint(PaintEventArgs e)
        {
            if (disposed)
                throw new ObjectDisposedException(ToString());

            if (!Enabled)
            {
                // using a buffer because original control must use a disabled buffer
                State = host.State;
                using (BufferedGraphicsContext context = new BufferedGraphicsContext())
                {
                    using (BufferedGraphics bg = context.Allocate(e.Graphics, new Rectangle(Point.Empty, Control.ClientSize)))
                    {
                        context.Invalidate();
                        using (PaintEventArgs be = new PaintEventArgs(bg.Graphics, e.ClipRectangle))
                        {
                            host.PaintState(State, be);
                        }

                        bg.Render(e.Graphics);
                    }
                }

                return;
            }

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
            int speed = !StateEquals(State ??= host.State, newState) ? GetSpeed(State, newState) : 0;
            if (speed < 0)
                speed = 0;

            // Not fallbacking if speed is 0 because in this case only new state is drawn, using buffer.
            // Previous animations must be stopped. When not stopped and current paint is a change witoug state change,
            // accidentally fading transitions may occur (eg. Elevated state of a (CommandLink)Button).
            if (speed == 0)
                StopAnimations();

            //// DEBUG: render to images
            //Size size = Control.ClientSize;
            //Bitmap prevStateImage = new Bitmap(size.Width, size.Height, e.Graphics);
            //Bitmap newStateImage = new Bitmap(size.Width, size.Height, e.Graphics);

            IntPtr hbpAnimation;
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                IntPtr hdcFrom, hdcTo;
                hbpAnimation = UxTheme.BeginBufferedAnimation(Control.Handle, hdc, Control.ClientRectangle, speed, out hdcFrom, out hdcTo);
                if (hbpAnimation != IntPtr.Zero)
                {
                    //// DEBUG: render to images
                    //using (BufferedGraphicsContext context = new BufferedGraphicsContext())
                    //{
                    //    using (Graphics graphicsImage = Graphics.FromImage(prevStateImage))
                    //    {
                    //        using (BufferedGraphics bg = context.Allocate(graphicsImage, new Rectangle(Point.Empty, size)))
                    //        {
                    //            host.PaintState(prevState, new PaintEventArgs(bg.Graphics, Control.ClientRectangle));
                    //            bg.Render(graphicsImage);
                    //        }
                    //    }

                    //    using (Graphics graphicsImage = Graphics.FromImage(newStateImage))
                    //    {
                    //        using (BufferedGraphics bg = context.Allocate(graphicsImage, new Rectangle(Point.Empty, size)))
                    //        {
                    //            host.PaintState(newState, new PaintEventArgs(bg.Graphics, Control.ClientRectangle));
                    //            bg.Render(graphicsImage);
                    //        }
                    //    }
                    //}

                    if (hdcFrom != IntPtr.Zero)
                    {
                        using (Graphics g = Graphics.FromHdc(hdcFrom))
                        {
                            host.PaintState(State ?? host.State, new PaintEventArgs(g, e.ClipRectangle));
                            //g.DrawImage(prevStateImage, Control.ClientRectangle);
                            //prevStateImage.Save(@"d:\temp\"+DateTime.UtcNow.ToFileTime()+".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    if (hdcTo != IntPtr.Zero)
                    {
                        using (Graphics g = Graphics.FromHdc(hdcTo))
                        {
                            host.PaintState(newState, new PaintEventArgs(g, e.ClipRectangle));
                            //g.DrawImage(newStateImage, Control.ClientRectangle);
                            //newStateImage.Save(@"d:\temp\" + DateTime.UtcNow.ToFileTime() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }

                    State = newState;
                    UxTheme.EndBufferedAnimation(hbpAnimation);
                }
            }
            finally
            {
                e.Graphics.ReleaseHdc(hdc);
            }

            // fallbacking
            Size clientSize = Control.ClientSize;
            if (hbpAnimation == IntPtr.Zero && clientSize.Width > 0 && clientSize.Height > 0)
            {
                State = newState;
                using (BufferedGraphicsContext context = new BufferedGraphicsContext())
                {
                    using (BufferedGraphics bg = context.Allocate(e.Graphics, new Rectangle(Point.Empty, clientSize)))
                    {
                        context.Invalidate();
                        using (PaintEventArgs be = new PaintEventArgs(bg.Graphics, e.ClipRectangle))
                        {
                            host.PaintState(newState, be);
                        }

                        bg.Render(e.Graphics);
                    }
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executed when Windows theme has been changed.
        /// </summary>
        protected virtual void OnThemeChanged()
        {
            // suspending if changing to classic theme
            if (operating && !FadingPainterInternal.IsSupported)
            {
                UxTheme.BufferedPaintUnInit();
                operating = false;
                return;
            }

            // resuming if changing back to Vista theme
            if (!operating && FadingPainterInternal.IsSupported)
                operating = UxTheme.BufferedPaintInit();
        }

        /// <summary>
        /// Gets the speed of the fading animation between specified states.
        /// When not overridden, hosts <see cref="ISupportsFading{TState}.GetFadingAnimationSpeed"/>
        /// is requested. If that returns negative value, <see cref="ISupportsFading{TState}.FadingAnimationDefaultSpeed"/> is used.
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
        /// Gets whether the previous and new states are equal.
        /// </summary>
        /// <param name="prevState">Previous state.</param>
        /// <param name="newState">New state.</param>
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
                UxTheme.BufferedPaintUnInit();
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
