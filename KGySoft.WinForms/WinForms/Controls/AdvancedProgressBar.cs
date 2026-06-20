#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedProgressBar.cs
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a progress bar with advanced capabilities.
    /// <remarks>
    /// The <see cref="AdvancedProgressBar"/> class offers the following features in addition to <see cref="ProgressBar"/>:
    /// <list type="bullet">
    /// <item><description>Paused/error state (see <see cref="State"/> property).</description></item>
    /// <item><description>New <see cref="Style"/> property that affect rendering mode.</description></item>
    /// <item><description>Custom colors (when <see cref="Style"/> is not <see cref="AdvancedProgressBarStyle.System"/>)</description></item>
    /// </list>
    /// </remarks>
    /// </summary>
    [ToolboxBitmap(typeof(ProgressBar))]
    [Description(@"A progress bar that provides the following features in addition to regular ProgressBar:
- Provides Warning/Error states
- Custom rendering styles
- Custom colors for non-system rendering styles
- Optional block appearance for non-system styles")]
    public class AdvancedProgressBar : ProgressBar
    {
        #region Constants

        private const int glowSpeed = 40;
        private const int glowPositionDefault = -160;

        #endregion

        #region Fields

        private readonly Timer animationTimer;
        private readonly bool initialized;

        private AdvancedProgressBarStyle style;
        private ProgressBarState state;
        private int animationOffset = -160;
        private bool isMarquee;
        private Color foreColor = Color.Empty;
        private Color pausedStateColor = Color.Yellow;
        private Color errorStateColor = Color.Red;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the state of the progress bar. On pre-Vista Windows versions, or when <see cref="Application.EnableVisualStyles"/>
        /// was not called in the executing application, applicable only for non-System <see cref="Style"/>s.
        /// The progress bar stops any animation when state is not <see cref="ProgressBarState.Normal"/>.
        /// </summary>
        [Category("AdvancedProgressBar")]
        [Description("Gets or sets the state of the progress bar. On pre-Vista Windows versions applicable only when Style is not System. The progress bar stops any animation when state is not Normal.")]
        [DefaultValue(ProgressBarState.Normal)]
        public ProgressBarState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                if (!Enum<ProgressBarState>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (IsHandleCreated && OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.InitializedWithVisualStyles)
                {
                    // changing state while progress bar is animating may prevent changing color in system mode
                    // workaround: adjusting value forward and back fixes the problem
                    if (!IsClassicAppearance && style == AdvancedProgressBarStyle.System && state == ProgressBarState.Normal && base.Value < Maximum)
                    {
                        base.Value++;
                        base.Value--;
                    }

                    User32.SendMessage(Handle, Constants.PBM_SETSTATE, (IntPtr)(value + 1), IntPtr.Zero);
                }

                state = value;
                ResetAnimation(value != ProgressBarState.Normal);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the rendering style of the <see cref="AdvancedProgressBar"/>.
        /// When visual styles are not available, <see cref="AdvancedProgressBarStyle.ThemedShiny"/> and <see cref="AdvancedProgressBarStyle.ThemedFlat"/> styles
        /// are defaulting to <see cref="AdvancedProgressBarStyle.Classic"/> style.
        /// </summary>
        [Category("AdvancedProgressBar")]
        [Description("Gets or sets the rendering style of the progress bar. When visual styles are not available, ThemedShiny and ThemedFlat styles are defaulting to Classic style.")]
        [DefaultValue(AdvancedProgressBarStyle.System)]
        public new AdvancedProgressBarStyle Style
        {
            get => style;
            set
            {
                if (style == value)
                    return;

                if (!Enum<AdvancedProgressBarStyle>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                style = value;
                SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, style != AdvancedProgressBarStyle.System);
                ResetSystemStyle();
                ResetAnimation(true);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether the progress bar should operate in marquee mode.
        /// </summary>
        [Category("AdvancedProgressBar")]
        [Description("Gets or sets whether the progress bar should operate in marquee mode.")]
        [DefaultValue(false)]
        public bool IsMarquee
        {
            get => isMarquee;
            set
            {
                if (isMarquee == value)
                    return;

                isMarquee = value;
                ResetSystemStyle();
                ResetAnimation(true);
            }
        }

        /// <summary>
        /// Gets or sets the interval in milliseconds between two frames of the marquee animation.
        /// </summary>
        [Description("Gets or sets the interval in milliseconds between two frames of the marquee animation.")]
        [DefaultValue(100)]
        public new int MarqueeAnimationSpeed
        {
            get => base.MarqueeAnimationSpeed;
            set
            {
                if (base.MarqueeAnimationSpeed == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

                base.MarqueeAnimationSpeed = value;
                ResetAnimation(false);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current position of the progress bar.
        /// </summary>
        [DefaultValue(0)]
        public new int Value
        {
            get => base.Value;
            set
            {
                base.Value = value;

                // in system mode paused/error state the stat must be reset, otherwise, the value may not change visually
                if (IsHandleCreated && state != ProgressBarState.Normal && !IsClassicAppearance && style == AdvancedProgressBarStyle.System && OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.InitializedWithVisualStyles)
                {
                    ProgressBarState currentState = state;
                    State = ProgressBarState.Normal;
                    State = currentState;
                }
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control.
        /// </summary>
        public override Color ForeColor
        {
            get
            {
                if (foreColor == Color.Empty)
                    return GetDefaultForeColor();

                return foreColor;
            }
            set
            {
                // workaround: base ctor sets highlight as fore color
                if (!initialized || foreColor == value)
                    return;

                foreColor = base.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the fore color of the paused <see cref="State"/>.
        /// Applicable only when <see cref="Style"/> is not <see cref="AdvancedProgressBarStyle.System"/>.
        /// </summary>
        [Category("AdvancedProgressBar")]
        [Description("Gets or sets the fore color of the paused state. Applicable only when Style is not System.")]
        [DefaultValue(typeof(Color), "Yellow")]
        public Color PausedStateColor
        {
            get => pausedStateColor;
            set
            {
                if (pausedStateColor == value)
                    return;

                pausedStateColor = value == Color.Empty ? Color.Yellow : value;
                if (state == ProgressBarState.Paused)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the fore color of the error <see cref="State"/>.
        /// Applicable only when <see cref="Style"/> is not <see cref="AdvancedProgressBarStyle.System"/>.
        /// </summary>
        [Category("AdvancedProgressBar")]
        [Description("Gets or sets the fore color of the error state. Applicable only when Style is not System.")]
        [DefaultValue(typeof(Color), "Red")]
        public Color ErrorStateColor
        {
            get => errorStateColor;
            set
            {
                if (errorStateColor == value)
                    return;

                errorStateColor = value == Color.Empty ? Color.Red : value;
                if (state == ProgressBarState.Error)
                    Invalidate();
            }
        }

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                // enabling marquee style even in design mode
                CreateParams createParams = base.CreateParams;
                if (isMarquee)
                    createParams.Style |= Constants.PBS_MARQUEE;

                return createParams;
            }
        }

        #endregion

        #region Private Properties

        private bool IsClassicAppearance => style == AdvancedProgressBarStyle.Classic || !VisualStyleHelper.RenderWithVisualStyles || VisualStyleHelper.HighContrast;

        private int MarqueeBlockWidth
        {
            get
            {
                if (IsClassicAppearance)
                    return 33;

                return style == AdvancedProgressBarStyle.Classic ? 50 : 120;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedProgressBar"/>.
        /// </summary>
        public AdvancedProgressBar()
        {
            animationTimer = new Timer();
            animationTimer.Tick += animationTimer_Tick;
            initialized = true;
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void DrawShadows(Graphics g, Rectangle rect, int shadowWidth, int alpha)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            using LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Transparent, Color.FromArgb(alpha, Color.Black), LinearGradientMode.Horizontal);
            float p1, p2;
            if (rect.Width / 2 <= shadowWidth)
                p1 = p2 = 0.5f;
            else
            {
                p1 = 100f / rect.Width * ((float)shadowWidth / 100);
                p2 = 1f - p1;
            }

            Blend blend = new Blend(4)
            {
                Factors = new float[] { 1f, 0.5f, 0.5f, 1f },
                Positions = new float[] { 0f, p1, p2, 1f }
            };

            brush.Blend = blend;
            g.FillRectangle(brush, rect);
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            animationTimer.Tick -= animationTimer_Tick;

            if (disposing)
            {
                animationTimer.Dispose();
                Events.Dispose();
            }
        }

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.InitializedWithVisualStyles)
                User32.SendMessage(Handle, Constants.PBM_SETSTATE, (IntPtr)(state + 1), IntPtr.Zero);
            ResetAnimation(true);
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Framework Mono paints the whole progress bar before calling OnPaint, so deferring out custom paint until then.
            if (OSHelper.IsFrameworkMono)
                return;

            if (IsMirrored)
                FixRtlVisibleClip(pevent.Graphics);
            PaintBackground(pevent);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsMirrored)
                FixRtlVisibleClip(e.Graphics);

            // On Framework Mono we already have a complete default paint at this point
            if (OSHelper.IsFrameworkMono)
            {
                if (style == AdvancedProgressBarStyle.System)
                    return;
                PaintBackground(e);
            }

            e.Graphics.EnsureCrossPlatformCorrectness(out float drawOffset);

            //// Reference paint for debugging LTR/RTL correctness on various platforms
            //e.Graphics.Clear(Color.Cyan);
            //RectangleF rect = ClientRectangle;
            //e.Graphics.DrawLine(Pens.Red, drawOffset, drawOffset, drawOffset, Height + drawOffset);
            //e.Graphics.DrawLine(Pens.Green, Width - 1 + drawOffset, drawOffset, Width - 1 + drawOffset, Height + drawOffset);
            //rect.Inflate(-1, -1);
            //e.Graphics.FillRectangle(Brushes.Blue, rect);
            //rect.Inflate(-1, -1);
            //rect.Offset(drawOffset, drawOffset);
            //e.Graphics.DrawRectangle(Pens.Yellow, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            //return;

            if (IsClassicAppearance)
                PaintClassicAppearance(e, drawOffset);
            else if (style == AdvancedProgressBarStyle.ThemedShiny)
                PaintShinyAppearance(e, drawOffset);
            else
                PaintFlatAppearance(e, drawOffset);

            // To raise the Paint event. Painting the System style has already occurred in WM_PAINT.
            base.OnPaint(e);

            // TODO: PaintText() - with mirroring if LTR
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (isMarquee && state != ProgressBarState.Normal)
                ResetAnimation(true);
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_TIMER:
                    // When built-in timer works, using that to avoid double invalidations
                    // Unfortunately in custom drawn marquee mode it runs only once so it does not work.
                    if (isMarquee || style == AdvancedProgressBarStyle.System)
                    {
                        base.WndProc(ref m);
                        return;
                    }

                    if (animationTimer.Enabled)
                        animationTimer.Enabled = false;

                    AdvanceAnimation();
                    break;

                case Constants.WM_NCPAINT when OSHelper.IsWindows && Style != AdvancedProgressBarStyle.System && Size != ClientSize:
                    NCHelper.DrawBorderNC(m.HWnd, Size, AdvancedBorderStyle.Sunken, IsMirrored);
                    return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Private Methods

        private Color GetDefaultForeColor()
        {
            if (!VisualStyleHelper.RenderWithVisualStyles)
                return SystemColors.Highlight;

            switch (style)
            {
                case AdvancedProgressBarStyle.ThemedShiny:
                    return Color.Lime;
                case AdvancedProgressBarStyle.ThemedFlat:
                    return Color.LimeGreen;
                default:
                    return SystemColors.Highlight;
            }
        }

        private bool ShouldSerializeForeColor()
        {
            return foreColor != Color.Empty;
        }

        private void ResetAnimation(bool resetPosition)
        {
            if (!IsHandleCreated)
                return;

            if (isMarquee)
            {
                int speed = base.MarqueeAnimationSpeed;
                if (OSHelper.IsWindows)
                    User32.SendMessage(Handle, Constants.PBM_SETMARQUEE, new IntPtr(speed > 0 ? 1 : 0), new IntPtr(speed));
                if (style == AdvancedProgressBarStyle.System)
                    animationTimer.Enabled = false;
                else
                {
                    if (speed > 0)
                        animationTimer.Interval = speed;
                    animationTimer.Enabled = state == ProgressBarState.Normal && speed > 0;
                    if (resetPosition || speed == 0)
                    {
                        animationOffset = state == ProgressBarState.Normal
                            ? -MarqueeBlockWidth
                            : Width / 2 - MarqueeBlockWidth / 2;
                    }
                }
            }
            else
            {
                if (style == AdvancedProgressBarStyle.System)
                    animationTimer.Enabled = false;
                else
                {
                    animationTimer.Interval = glowSpeed;
                    animationTimer.Enabled = !IsClassicAppearance && style == AdvancedProgressBarStyle.ThemedShiny && state == ProgressBarState.Normal;
                    if (resetPosition)
                        animationOffset = glowPositionDefault;
                }
            }
        }

        /// <summary>
        /// Resetting the underlying system style. Executed even in custom modes to reset
        /// internal params and timers.
        /// </summary>
        private void ResetSystemStyle()
        {
            if (isMarquee)
                base.Style = ProgressBarStyle.Marquee;
            //else if (displayBlocks)
            //    base.Style = ProgressBarStyle.Blocks;
            else
                base.Style = ProgressBarStyle.Continuous;
        }

        private void PaintBackground(PaintEventArgs e)
        {
            if (!IsClassicAppearance && style == AdvancedProgressBarStyle.ThemedShiny)
                this.PaintTransparentBackground(e);
            else
                this.PaintBackground(e, e.ClipRectangle, BackColor);
        }

        private void PaintClassicAppearance(PaintEventArgs e, float offset)
        {
            // frame: when visual styles are disabled, there is already a frame in NC area, except in Framework Mono, where the frame is in the client area
            Rectangle rect = ClientRectangle;
            if (VisualStyleHelper.RenderWithVisualStyles || OSHelper.IsFrameworkMono)
            {
                e.Graphics.DrawBorder(AdvancedBorderStyle.Sunken, rect, IsMirrored && !OSHelper.IsFrameworkMono ? Width : 0);
                rect.Inflate(-2, -2);
            }
            else
                rect.Inflate(-1, -1);

            DrawBar(e.Graphics, GetBarRect(rect));
        }

        private Rectangle GetBarRect(Rectangle rect)
        {
            // marquee style
            if (IsMarquee)
                rect = rect.IntersectSafe(new Rectangle(animationOffset, rect.Top, MarqueeBlockWidth, rect.Height));
            // regular style
            else
            {
                if (Maximum <= Minimum || Value <= Minimum)
                    return Rectangle.Empty;

                int range = Maximum - Minimum;
                double value = ((double)Value - Minimum) / range * rect.Width;
                rect.Width = value < (double)range / 2 ? (int)Math.Ceiling(value) : (int)Math.Floor(value);
            }

            return rect;
        }

        private void DrawBar(Graphics graphics, Rectangle rect)
        {
            if (rect.Height <= 0 || rect.Width <= 0)
                return;

            graphics.FillRectangle(GetActualForeColor().GetBrush(), rect);
        }

        private Color GetActualForeColor() => state switch
        {
            ProgressBarState.Normal => ForeColor,
            ProgressBarState.Error => ErrorStateColor,
            ProgressBarState.Paused => PausedStateColor,
            _ => throw new ArgumentOutOfRangeException()
        };

        private void PaintFlatAppearance(PaintEventArgs e, float offset)
        {
            Rectangle rect = ClientRectangle;
            if (BackColor != Color.Transparent)
                e.Graphics.DrawRectangle(BackColor.Dark(0.3f).GetPen(), rect.X + offset, rect.Y + offset, rect.Width - 1, rect.Height - 1);

            rect.Inflate(-1, -1);
            DrawBar(e.Graphics, GetBarRect(rect));
        }

        private void PaintShinyAppearance(PaintEventArgs e, float offset)
        {
            #region Local Methods

            void DrawShinyBackground()
            {
                Graphics g = e.Graphics;
                Rectangle rect = ClientRectangle;
                rect.Inflate(-1, -1);
                if (rect.Width >= 1 && rect.Height >= 1)
                    g.FillRoundedRectangle(BackColor.Dark(0.1f).GetBrush(), rect, 2);

                rect.Inflate(0, -1);
                DrawShadows(g, rect, 10, 40);
                DrawHighlight(new Rectangle(1, 1, Width - 2, Height - 2), BackColor);
            }

            void DrawShinyBar()
            {
                Graphics g = e.Graphics;
                Rectangle rect = ClientRectangle;
                rect.Inflate(-1, -1);
                rect = GetBarRect(rect);
                DrawBar(g, rect);
                DrawShadows(g, rect, 20, 100);
                DrawHighlight(rect, GetActualForeColor());
                if (state != ProgressBarState.Normal)
                    return;

                // glow: only when not in paused/error state
                var savedState = g.Save();
                g.IntersectClip(rect);
                rect = new Rectangle(animationOffset, 0, 60, Height);
                using var brush = new LinearGradientBrush(rect, Color.Transparent, ControlPaint.LightLight(ForeColor), LinearGradientMode.Horizontal);
                var blend = new Blend(4)
                {
                    Factors = [0f, 0.5f, 0.5f, 0f],
                    Positions = [0f, 0.5f, 0.6f, 1f]
                };

                brush.Blend = blend;
                g.FillRectangle(brush, rect);
                g.Restore(savedState);
            }

            void DrawShinyMarquee()
            {
                Graphics g = e.Graphics;
                var savedState = g.Save();
                g.IntersectClip(GetBarRect(Rectangle.Inflate(ClientRectangle, -1, -1)));
                Rectangle rect = new Rectangle(animationOffset, 1, MarqueeBlockWidth, (Height - 2) / 2);
                if (rect.Width <= 0 || rect.Height <= 0)
                    return;

                Color color = GetActualForeColor();
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Transparent, color, LinearGradientMode.Horizontal))
                {
                    Blend blend = new Blend(3)
                    {
                        Factors = new float[] { 0f, 1f, 0f },
                        Positions = new float[] { 0f, 0.5f, 1f }
                    };

                    brush.SetSigmaBellShape(0.5f);
                    brush.Blend = blend;
                    g.FillRectangle(brush, rect);
                }

                rect.Y = rect.Bottom;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Transparent, color.Dark(0.2f), LinearGradientMode.Horizontal))
                {
                    Blend blend = new Blend(3)
                    {
                        Factors = new float[] { 0f, 1f, 0f },
                        Positions = new float[] { 0f, 0.5f, 1f }
                    };

                    brush.SetSigmaBellShape(0.5f);
                    brush.Blend = blend;
                    g.FillRectangle(brush, rect);
                }

                g.Restore(savedState);
            }

            void DrawHighlight(Rectangle clipRect, Color highlightColor)
            {
                Graphics g = e.Graphics;
                int height = (Height - 2) / 2;
                if (!isMarquee)
                    height = Math.Min(6, height);

                //Rectangle rect = new Rectangle(1, 1, Width - 1, height);
                Rectangle rect = new Rectangle(clipRect.Location, new Size(clipRect.Width, height));
                if (rect.Height <= 0 || rect.Width <= 0)
                    return;

                highlightColor = Color.FromArgb((highlightColor.R + 255) / 2, (highlightColor.G + 255) / 2, (highlightColor.B + 255) / 2);
                using (Brush brush = new LinearGradientBrush(rect, highlightColor, Color.FromArgb(92, highlightColor), LinearGradientMode.Vertical))
                    g.FillRectangle(brush, rect);

                height = Math.Min(4, (clipRect.Height - 2) / 3);
                if (height <= 0)
                    return;

                rect = new Rectangle(clipRect.Left, clipRect.Height - height, clipRect.Width, height);
                using (Brush brush = new LinearGradientBrush(rect, Color.Transparent, Color.FromArgb(64, /*this.HighlightColor*/Color.White), LinearGradientMode.Vertical))
                    g.FillRectangle(brush, rect);
            }

            void DrawShinyFrame()
            {
                Graphics g = e.Graphics;

                // inner stroke
                RectangleF rect = ClientRectangle;
                rect.Inflate(-1, -1);
                rect.Width--;
                rect.Height--;
                rect.Offset(offset, offset);
                g.DrawRoundedRectangle(Color.FromArgb(100, Color.White).GetPen(), rect, 2);

                // frame
                rect = ClientRectangle;
                rect.Width--;
                rect.Height--;
                rect.Offset(offset, offset);
                g.DrawRoundedRectangle(BackColor.Dark(0.3f).GetPen(), rect, 2);
            }

            #endregion

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawShinyBackground();
            if (isMarquee)
                DrawShinyMarquee();
            else
                DrawShinyBar();

            DrawShinyFrame();
        }

        private void AdvanceAnimation()
        {
            if (state != ProgressBarState.Normal || style == AdvancedProgressBarStyle.System)
            {
                animationTimer.Enabled = false;
                return;
            }

            if (animationOffset == glowPositionDefault && Value == Maximum)
                return;

            if (isMarquee)
            {
                animationOffset += style == AdvancedProgressBarStyle.ThemedShiny && VisualStyleHelper.RenderWithVisualStyles ? 6 : 10;
                if (animationOffset > Width)
                    animationOffset = -MarqueeBlockWidth;
                return;
            }

            animationOffset += 10;
            if (animationOffset > Width - glowPositionDefault)
                animationOffset = glowPositionDefault;
        }

        private void FixRtlVisibleClip(Graphics g)
        {
            // On real windows with RTL layout the actual visible clip bounds are off by 1 pixel.
            // With no double buffering (like in case of DateTimePicker) we wouldn't need to do anything, just draw in ClintRectangle, even though VisibleClipBounds.X is -1.
            // With double buffering though, VisibleClipBounds is "fixed" initially (covers ClientRectangle), but in practice, the right side (X = 0 with mirroring) will be clipped,
            // unless we restore the clip bounds with offset. To detect this case, we need to reset the clip bounds to reveal the whole size of the internal buffer
            // and the initial clip for it. If it has a negative horizontal offset, we reset the original clip, but with applying the offset to the visible clip bounds.
            if (!OSHelper.IsRealWindows || !DoubleBuffered)
                return;

            GraphicsState savedState = g.Save();
            g.ResetClip();
            PointF offset = g.VisibleClipBounds.Location;
            g.Restore(savedState);
            if (offset.X < 0)
                g.TranslateClip(offset.X, offset.Y);
        }

        #endregion

        #region Event handlers
#pragma warning disable IDE1006 // Naming Styles

        void animationTimer_Tick(object? sender, EventArgs e)
        {
            AdvanceAnimation();
            Invalidate();
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion

        #endregion
    }
}
