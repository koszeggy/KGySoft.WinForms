#region Used namespaces

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KGySoft.Controls.WinApi;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Helper class for fading animations. Host control must implement <see cref="ISupportsFading{TState}"/> interface.
    /// </summary>
    public class FadingPainter<TState> : IDisposable
    {
        #region Fields

        private ISupportsFading<TState> host;
        private bool disposed;
        private TState prevState;
        private bool operating;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the stored last state explicitly. Setting this property does not
        /// invalidate the host control.
        /// </summary>
        public TState State
        {
            get { return prevState; }
            set { prevState = value; }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the host control.
        /// </summary>
        protected Control Control
        {
            get { return (Control)host; }
        }

        /// <summary>
        /// Gets whether the fading painter is enabled.
        /// </summary>
        protected virtual bool Enabled
        {
            get { return operating && host.FadingAnimationsEnabled && FadingPainterInternal.IsSupported; }
        }

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="FadingPainter{TState}"/>.
        /// </summary>
        /// <param name="host">The host control that implements <see cref="ISupportsFading{TState}"/>.</param>
        /// <param name="initialState">Initial state of the host control.</param>
        public FadingPainter(ISupportsFading<TState> host, TState initialState)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            if (!(host is Control))
                throw new ArgumentException("Host should be a Control class.", "host");

            operating = FadingPainterInternal.IsSupported && UxTheme.BufferedPaintInit() == 0;
            State = initialState;
            this.host = host;
            HookEvents();
        }

        #endregion

        #region Destructor

        ~FadingPainter()
        {
            Dispose(false);
        }

        #endregion

        #region Explicit Disposing

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
            {
                host = null;
            }

            disposed = true;
        }

        #endregion

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

        #endregion

        #region Internal Methods

        internal virtual void PaintCore(PaintEventArgs e, TState newState)
        {
            int speed = !StateEquals(prevState, newState) ? GetSpeed(prevState, newState) : 0;
            if (speed < 0)
                speed = 0;

            // Not fallbacking if speed is 0 because in this case only new state is drawn, using buffer.
            BP_ANIMATIONPARAMS animParams = new BP_ANIMATIONPARAMS();
            animParams.cbSize = Marshal.SizeOf(animParams);
            animParams.style = BP_ANIMATIONSTYLE.BPAS_LINEAR;
            animParams.dwDuration = speed;

            // Previous animations must be stopped. When not stopped and current paint is a change witoug state change,
            // accidentally fading transitions may occur (eg. Elevated state of a (CommandLink)Button).
            if (speed == 0)
                StopAnimations();

            //// DEBUG: render to images
            //Size size = Control.ClientSize;
            //Bitmap prevStateImage = new Bitmap(size.Width, size.Height, e.Graphics);
            //Bitmap newStateImage = new Bitmap(size.Width, size.Height, e.Graphics);

            RECT rc = new RECT(Control.ClientRectangle);
            IntPtr hbpAnimation;
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                IntPtr hdcFrom, hdcTo;
                hbpAnimation = UxTheme.BeginBufferedAnimation(Control.Handle, hdc, ref rc, BP_BUFFERFORMAT.BPBF_COMPATIBLEBITMAP, IntPtr.Zero, ref animParams, out hdcFrom, out hdcTo);
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
                            host.PaintState(prevState, new PaintEventArgs(g, e.ClipRectangle));
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

                    prevState = newState;
                    UxTheme.EndBufferedAnimation(hbpAnimation, true);
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
                prevState = newState;
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

        protected virtual void HookEvents()
        {
            Control.SystemColorsChanged += new EventHandler(Control_SystemColorsChanged);
            Control.SizeChanged += new EventHandler(Control_SizeChanged);
        }

        protected virtual void UnhookEvents()
        {
            Control.SystemColorsChanged -= Control_SystemColorsChanged;
            Control.SizeChanged -= new EventHandler(Control_SizeChanged);
        }

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
            {
                operating = UxTheme.BufferedPaintInit() == 0;
            }
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
        protected virtual bool StateEquals(TState prevState, TState newState)
        {
            return Equals(prevState, newState);
        }

        /// <summary>
        /// Stops all animations for the host control.
        /// </summary>
        protected virtual void StopAnimations()
        {
            if (operating && FadingPainterInternal.IsSupported)
                UxTheme.BufferedPaintStopAllAnimations(host.Handle);
        }

        #endregion

        #region Private Methods

        void Control_SystemColorsChanged(object sender, EventArgs e)
        {
            OnThemeChanged();
        }

        void Control_SizeChanged(object sender, EventArgs e)
        {
            StopAnimations();
            Control.Invalidate();
        }

        #endregion

        #endregion
    }
}
