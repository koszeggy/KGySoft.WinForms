#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedProgressBar.cs
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
    // TODO: Blocks
    ///// <item><description>Block appearance with every non-system styles (see <see cref="DisplayBlocks"/>)</description></item>
    // TODO: Text
    // TODO: Taskbar progress/state
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
            get =>
                //return (ProgressBarState)(int)User32.SendMessage(Handle, Constants.PBM_GETSTATE, IntPtr.Zero, IntPtr.Zero) - 1;
                state;
            set
            {
                if (state == value)
                    return;

                if (!Enum<ProgressBarState>.IsDefined(value))
                    throw new ArgumentOutOfRangeException("value");

                if (IsHandleCreated && WindowsUtils.IsVistaOrLater && WindowsUtils.IsComCtlV6Available)
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
                    throw new ArgumentOutOfRangeException("value");

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
        public new int MarqueeAnimationSpeed
        {
            get => base.MarqueeAnimationSpeed;
            set
            {
                if (base.MarqueeAnimationSpeed == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                base.MarqueeAnimationSpeed = value;
                ResetAnimation(false);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current position of the progress bar.
        /// </summary>
        public new int Value
        {
            get => base.Value;
            set
            {
                base.Value = value;

                // in system mode paused/error state the stat must be reset, otherwise, the value may not change visually
                if (IsHandleCreated && state != ProgressBarState.Normal && !IsClassicAppearance && style == AdvancedProgressBarStyle.System && WindowsUtils.IsVistaOrLater && WindowsUtils.IsComCtlV6Available)
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

        protected override CreateParams CreateParams
        {
            get
            {
                // enabling marquee style even in design mode
                CreateParams createParams = base.CreateParams;
                if (isMarquee)
                    createParams.Style |= 8;

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            animationTimer.Tick -= animationTimer_Tick;

            if (disposing)
                animationTimer.Dispose();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (WindowsUtils.IsVistaOrLater && WindowsUtils.IsComCtlV6Available)
                User32.SendMessage(Handle, Constants.PBM_SETSTATE, (IntPtr)(state + 1), IntPtr.Zero);
            ResetAnimation(true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (!IsClassicAppearance && style == AdvancedProgressBarStyle.ThemedShiny)
                this.PaintTransparentBackground(pevent);
            else
                base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsClassicAppearance)
                PaintClassicAppearance(e);
            else if (style == AdvancedProgressBarStyle.ThemedShiny)
                PaintShinyAppearance(e);
            else
                PaintFlatAppearance(e);

            // TODO: PaintText() - ha lesz, figyelni LTR mód esetén a tükrözésre!
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (isMarquee && state != ProgressBarState.Normal)
            {
                ResetAnimation(true);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_TIMER:
                    // When built-in timer works, using that to avoid double invalidatings
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
                User32.SendMessage(Handle, Constants.PBM_SETMARQUEE, Convert.ToInt32(speed > 0), speed);
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

        private void PaintClassicAppearance(PaintEventArgs e)
        {
            // background
            PaintSimpleBackground(e);

            // frame: when visual styles are disabled, there is already a frame in NC area
            Rectangle rect = ClientRectangle;
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                ControlPaint.DrawBorder3D(e.Graphics, rect, Border3DStyle.SunkenOuter);
                rect.Inflate(-2, -2);
            }
            else
                rect.Inflate(-1, -1);

            DrawBar(e.Graphics, GetBarRect(rect));
        }

        private void PaintSimpleBackground(PaintEventArgs e)
        {
            if (BackColor == Color.Transparent)
                this.PaintTransparentBackground(e);
            else
            {
                e.Graphics.FillRectangle(BackColor.GetBrush(), e.ClipRectangle);
            }
        }

        private Rectangle GetBarRect(Rectangle rect)
        {
            // marquee style
            if (IsMarquee)
            {
                rect.Intersect(new Rectangle(animationOffset, rect.Top, MarqueeBlockWidth, rect.Height));
            }
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

        private void PaintFlatAppearance(PaintEventArgs e)
        {
            // background
            PaintSimpleBackground(e);

            Rectangle rect = ClientRectangle;
            rect.Width--;
            rect.Height--;
            if (BackColor != Color.Transparent)
                e.Graphics.DrawRectangle(BackColor.Dark(0.3f).GetPen(), rect);

            rect.Height++;
            rect.Width++;
            rect.Inflate(-1, -1);
            DrawBar(e.Graphics, GetBarRect(rect));

        }

        private void PaintShinyAppearance(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            DrawShinyBackground(e.Graphics);
            if (isMarquee)
                DrawShinyMarquee(e.Graphics);
            else
                DrawShinyBar(e.Graphics);

            DrawShinyFrame(e.Graphics);
        }

        private void DrawShinyFrame(Graphics graphics)
        {
            // inner stroke
            Rectangle rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            rect.Width--;
            rect.Height--;
            graphics.DrawRoundedRectangle(Color.FromArgb(100, Color.White).GetPen(), rect, 2);

            // frame
            rect = ClientRectangle;
            rect.Width--;
            rect.Height--;
            graphics.DrawRoundedRectangle(BackColor.Dark(0.3f).GetPen(), rect, 2);
        }

        private void DrawShinyBackground(Graphics g)
        {
            Rectangle rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            if (rect.Width >= 1 && rect.Height >= 1)
                g.FillRoundedRectangle(BackColor.Dark(0.1f).GetBrush(), rect, 2);

            rect.Inflate(0, -1);
            DrawShadows(g, rect, 10, 40);
            DrawHighlight(g, new Rectangle(1, 1, Width - 2, Height - 2), BackColor);
        }

        private void DrawShinyBar(Graphics g)
        {
            Rectangle rect = ClientRectangle;
            rect.Inflate(-1, -1);
            rect = GetBarRect(rect);
            DrawBar(g, rect);
            DrawShadows(g, rect, 20, 100);
            DrawHighlight(g, rect, GetActualForeColor());
            if (state == ProgressBarState.Normal)
                DrawGlow(g, rect);
        }

        private void DrawShinyMarquee(Graphics graphics)
        {
            graphics.SetClip(GetBarRect(ClientRectangle));
            try
            {
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
                    graphics.FillRectangle(brush, rect);
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
                    graphics.FillRectangle(brush, rect);
                }

            }
            finally
            {
                graphics.ResetClip();
            }
        }

        private void DrawHighlight(Graphics g, Rectangle clipRect, Color highlightColor)
        {
            int height = (Height - 2) / 2;
            if (!isMarquee)
                height = Math.Min(6, height);

            //Rectangle rect = new Rectangle(1, 1, Width - 1, height);
            Rectangle rect = new Rectangle(clipRect.Location, new Size(clipRect.Width, height));
            if (rect.Height <= 0 || rect.Width <= 0)
                return;

            highlightColor = Color.FromArgb((highlightColor.R + 255) / 2, (highlightColor.G + 255) / 2, (highlightColor.B + 255) / 2);
            //try
            //{
            //using (GraphicsPath path = DrawingHelper.RoundedRect(rect, 2, 2, 0, 0))
            //{
            //    //rect.Intersect(clipRect);
            //    //g.SetClip(path);
            //    g.SetClip(clipRect);
            //using (Brush brush = new LinearGradientBrush(rect, Color.FromArgb(220, Color.White), Color.FromArgb(92, Color.White), LinearGradientMode.Vertical))
            using (Brush brush = new LinearGradientBrush(rect, highlightColor, Color.FromArgb(92, highlightColor), LinearGradientMode.Vertical))
            {
                //g.FillPath(brush, path);
                g.FillRectangle(brush, rect);
            }
            //}

            height = Math.Min(4, (clipRect.Height - 2) / 3);
            if (height <= 0)
                return;

            rect = new Rectangle(clipRect.Left, clipRect.Height - height, clipRect.Width, height);
            //using (GraphicsPath path = DrawingHelper.RoundedRect(rect, 0, 0, 2, 2))
            //{
            //    g.SetClip(path);
            using (Brush brush = new LinearGradientBrush(rect, Color.Transparent, Color.FromArgb(64, /*this.HighlightColor*/Color.White), LinearGradientMode.Vertical))
            {
                //g.FillPath(brush, path);
                g.FillRectangle(brush, rect);
            }
            //}

            //}
            //finally
            //{
            //    g.ResetClip();
            //}
        }

        private void DrawGlow(Graphics g, Rectangle clipRect)
        {
            g.SetClip(clipRect);
            try
            {
                Rectangle rect = new Rectangle(animationOffset, 0, 60, Height);
                using LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Transparent, ControlPaint.LightLight(ForeColor), LinearGradientMode.Horizontal);
                Blend blend = new Blend(4)
                {
                    Factors = new float[] { 0f, 0.5f, 0.5f, 0f },
                    Positions = new float[] { 0f, 0.5f, 0.6f, 1f }
                };

                brush.Blend = blend;

                //Rectangle clip = new Rectangle(1, 2, this.Width - 3, this.Height - 3);
                //clip.Width = (int)(Value * 1.0F / (Maximum - Minimum) * this.Width);
                g.FillRectangle(brush, rect);
                //using (LinearGradientBrush lgb = new LinearGradientBrush(rect, Color.White, Color.White, LinearGradientMode.Horizontal))
                //{
                //    ColorBlend cb = new ColorBlend(4);
                //    cb.Colors = new Color[] { Color.Transparent, /*this.GlowColor*/Color.FromArgb(128, ControlPaint.LightLight(ForeColor)), /*this.GlowColor*/Color.FromArgb(128, ControlPaint.LightLight(ForeColor)), Color.Transparent };
                //    cb.Positions = new float[] { 0.0F, 0.5F, 0.6F, 1.0F };
                //    lgb.InterpolationColors = cb;

                //    //Rectangle clip = new Rectangle(1, 2, this.Width - 3, this.Height - 3);
                //    //clip.Width = (int)(Value * 1.0F / (Maximum - Minimum) * this.Width);
                //    g.FillRectangle(lgb, rect);
                //}

            }
            finally
            {
                g.ResetClip();
            }
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

        #endregion

        #region Event handlers

        void animationTimer_Tick(object? sender, EventArgs e)
        {
            AdvanceAnimation();
            Invalidate();
        }

        #endregion

        #endregion

        #endregion
    }
}
