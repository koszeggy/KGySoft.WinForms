#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FadingPainterInternal.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A special fading painter implementation which gets duration speeds of standard animations from the OS,
    /// and supports several fading options, including the support of turning on visibility and any visual changes.
    /// </summary>
    internal sealed class FadingPainterInternal : FadingPainter<ControlAppearanceState>
    {
        #region Fields

        #region Static Fields

        private static readonly TimeSpan disablingMaskingTime = new TimeSpan(0, 0, 0, 0, 50);

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, int> speedCache = new Dictionary<long, int>();
        private readonly string className;

        private Bitmap? prevStateImage;
        private DateTime lastEnableToggled;

        #endregion

        #endregion

        #region Properties

        #region Static Properties

        internal static bool IsSupported => OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.RenderWithVisualStyles;

        #endregion

        #region Instance Properties

        #region Protected Properties

        /// <summary>
        /// Gets whether the fading painter is enabled.
        /// </summary>
        protected override bool Enabled => base.Enabled && Host.FadingAnimationOptions != FadingOptions.None;

        #endregion

        #region Private Properties

        private ISupportsFadingInternal Host => (ISupportsFadingInternal)Control;

        #endregion

        #endregion

        #endregion

        #region Constructors

        internal FadingPainterInternal(ISupportsFading<ControlAppearanceState> host, string className)
            : base(host, null)
        {
            this.className = className;
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override void PaintCore(PaintEventArgs e, ControlAppearanceState newState)
        {
            bool isStandardChangeOnly = (Host.FadingAnimationOptions & FadingOptions.StandardEffects) != FadingOptions.None
                    && !newState.EqualsWithOptions(State, FadingOptions.StandardEffects)
                    && newState.EqualsWithOptions(State, ControlAppearanceState.NonStandardChanges);

            // performing base paint if regular changes are required
            if ((Host.FadingAnimationOptions & FadingOptions.AnyChange) == FadingOptions.None)
            {
                prevStateImage?.Dispose();
                prevStateImage = null;

                // Bug workaround: When disabling a button, command link or enabling/disabling a label, a further paint is immediately triggered and UxTheme.BufferedPaintRenderAnimation
                // fails to report that animating is in progress. Therefore, here masking double triggered enabling/disabling to avoid a flickering effect
                if (Equals(newState, State) && lastEnableToggled != default(DateTime) && DateTime.UtcNow - lastEnableToggled < disablingMaskingTime)
                {
                    lastEnableToggled = default(DateTime);
                    return;
                }

                if (isStandardChangeOnly && State!.Enabled != newState.Enabled)
                    lastEnableToggled = DateTime.UtcNow;
                else
                    lastEnableToggled = default(DateTime);

                base.PaintCore(e, newState);
                return;
            }

            // AnyChange is handled here: creating and comparing snapshots
            Size size = Control.ClientSize;
            Bitmap newStateImage;
            //ControlAppearanceState newState = Host.State;

            //    /////////////////////// Rendering into a bitmap - Native solution:
            //    IntPtr hdc = e.Graphics.GetHdc();
            //    IntPtr comapitbleDc = Gdi32.CreateCompatibleDC(hdc);
            //    IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdc, size.Width, size.Height);
            //    Gdi32.SelectObject(comapitbleDc, hBitmap);
            //    e.Graphics.ReleaseHdc(hdc);

            //    using (Graphics g = Graphics.FromHdc(comapitbleDc))
            //    {
            //        Host.PaintState(Host.State, new PaintEventArgs(g, e.ClipRectangle));
            //        newStateImage = Image.FromHbitmap(hBitmap);
            //    }

            //    Gdi32.DeleteObject(hBitmap);
            //    Gdi32.DeleteObject(comapitbleDc);


            /////////////////////// Managed solution (this is better because BufferedGraphics.Render uses Gdi.Bitblt, which is faster than Graphics.DrawImage):

            // Using a BufferedGraphics instead of using the Graphics of a newly created Bitmap
            // because that would cause ugly text rendering when using TextRenderer.DrawText
            using (BufferedGraphicsContext context = new BufferedGraphicsContext())
            {
                using (BufferedGraphics bg = context.Allocate(e.Graphics, new Rectangle(Point.Empty, size)))
                {
                    Host.PaintState(newState, new PaintEventArgs(bg.Graphics, Control.ClientRectangle));

                    newStateImage = new Bitmap(size.Width, size.Height, e.Graphics);
                    using (Graphics graphicsImage = Graphics.FromImage(newStateImage))
                    {
                        bg.Render(graphicsImage);
                    }

                    // if no actual change or no speed is set rendering result quickly (maybe just Invalidate was called)
                    bool equal = false;
                    if (State == null || (State.Visible == newState.Visible && (prevStateImage == null || (equal = prevStateImage.EqualsByContent(newStateImage)))
                            || prevStateImage?.Size != newStateImage.Size || (Host.FadingAnimationDefaultSpeed <= 0 && !isStandardChangeOnly)))
                    {
                        // Bug workaround: When disabling a button or enabling/disabling a label, a further paint is immediately triggered and UxTheme.BufferedPaintRenderAnimation
                        // fails to report that animating is in progress. Therefore, here masking double triggered enabling/disabling to avoid a flickering effect
                        if (equal && lastEnableToggled != default(DateTime) && DateTime.UtcNow - lastEnableToggled < disablingMaskingTime)
                        {
                            lastEnableToggled = default(DateTime);
                            return;
                        }

                        // this copies newState into e.Graphics
                        bg.Render();

                        if (equal)
                            newStateImage.Dispose();
                        else
                        {
                            prevStateImage?.Dispose();
                            prevStateImage = newStateImage;
                        }

                        State = newState;
                        lastEnableToggled = default(DateTime);
                        return;
                    }
                }
            }

            if (isStandardChangeOnly && State.Enabled != newState.Enabled)
                lastEnableToggled = DateTime.UtcNow;
            else
                lastEnableToggled = default(DateTime);

            IntPtr hbpAnimation;
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                int speed = isStandardChangeOnly ? GetSpeed(State, newState) : base.GetSpeed(State, newState);
                hbpAnimation = UxTheme.BeginBufferedAnimation(Control.Handle, hdc, Control.ClientRectangle, speed, out IntPtr hdcFrom, out IntPtr hdcTo);
                if (hbpAnimation != IntPtr.Zero)
                {
                    if (hdcFrom != IntPtr.Zero)
                    {
                        using Graphics g = Graphics.FromHdc(hdcFrom);

                        // if previous state was invisible, letting the control paint
                        if (State?.Visible == false)
                            Host.PaintState(State, new PaintEventArgs(g, Control.ClientRectangle));
                        else
                            g.DrawImage(prevStateImage, Control.ClientRectangle);
                    }
                    if (hdcTo != IntPtr.Zero)
                    {
                        using Graphics g = Graphics.FromHdc(hdcTo);
                        g.DrawImage(newStateImage, Control.ClientRectangle);
                    }

                    prevStateImage?.Dispose();
                    prevStateImage = newStateImage;
                    State = newState;
                    UxTheme.EndBufferedAnimation(hbpAnimation);
                    return;
                }
            }
            finally
            {
                e.Graphics.ReleaseHdc(hdc);
            }

            // fallback
            if (hbpAnimation == IntPtr.Zero)
            {
                prevStateImage?.Dispose();
                prevStateImage = newStateImage;
                State = newState;
                e.Graphics.DrawImage(newStateImage, Control.ClientRectangle);
            }
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                prevStateImage?.Dispose();
                prevStateImage = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Stops all animations for the host control.
        /// </summary>
        protected override void StopAnimations()
        {
            base.StopAnimations();
            prevStateImage?.Dispose();
            prevStateImage = null;
        }

        /// <summary>
        /// Executed when Windows theme has been changed.
        /// </summary>
        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            speedCache.Clear();
        }

        /// <summary>
        /// Getting standard effect speeds from the OS.
        /// </summary>
        protected override int GetSpeed(ControlAppearanceState prevState, ControlAppearanceState newState)
        {
            if (!OSHelper.IsWindowsVistaOrLater)
                return base.GetSpeed(prevState, newState);

            // not considering color change because color may change with these events (enabled-disabled)
            bool isStandardChangeOnly = !newState.EqualsWithOptions(State, FadingOptions.StandardEffects)
                    && newState.EqualsWithOptions(State, ControlAppearanceState.NonStandardChanges);

            if (!isStandardChangeOnly)
            {
                bool isEnabledChangeOnly = newState.EqualsWithOptions(State, ControlAppearanceState.NonStandardChanges & ~Host.FadingAnimationOptions);
                return isEnabledChangeOnly ? base.GetSpeed(prevState, newState) : 0;
            }

            if (!speedCache.TryGetValue(((long)newState.SystemPartId << 32) | (uint)(prevState.SystemStateId << 16) | (uint)newState.SystemStateId, out int speed))
            {
                IntPtr hTheme = UxTheme.OpenThemeDataGlobal(className);
                if (!UxTheme.TryGetThemeTransitionDuration(hTheme, newState.SystemPartId, prevState.SystemStateId, newState.SystemStateId, Constants.TMT_TRANSITIONDURATIONS, out speed))
                    return base.GetSpeed(prevState, newState);

                // if speed is 0, trying other direction (eg. default and default_animating states)
                if (speed == 0)
                    UxTheme.TryGetThemeTransitionDuration(hTheme, newState.SystemPartId, newState.SystemStateId, prevState.SystemStateId, Constants.TMT_TRANSITIONDURATIONS, out speed);

                speedCache[((long)newState.SystemPartId << 32) | (uint)(prevState.SystemStateId << 16) | (uint)newState.SystemStateId] = speed;
            }

            return Host.GetStandardAnimationSpeed(prevState, newState, speed);
        }

        /// <summary>
        /// Gets whether the previous and new states are equal.
        /// </summary>
        /// <param name="prevState">Previous state.</param>
        /// <param name="newState">New state.</param>
        /// <returns><see langword="true"/>, if states are equal; otherwise, <see langword="false"/>.</returns>
        protected override bool StateEquals(ControlAppearanceState prevState, ControlAppearanceState newState)
            => prevState.EqualsWithOptions(newState, Host.FadingAnimationOptions);

        #endregion

        #endregion
    }
}
