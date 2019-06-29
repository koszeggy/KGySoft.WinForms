#region Used namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.Controls.WinApi;
using KGySoft.Drawing;
using KGySoft.Libraries;
using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a radio button with full Windows Vista features support. Fully compatible with for Windows XP, too.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedRadioButton"/> class offers the following features in addition to <see cref="RadioButton"/>:
    /// <list type="bullet">
    /// <item><description><see cref="ButtonBase.AutoSize"/> property works as expected when radio button is docked</description></item>
    /// <item><description>Different rendering qualities (see <see cref="RenderingQuality"/>) property.</description></item>
    /// <item><description>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</description></item>
    /// <item><description>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</description></item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(RadioButton))]
    [Description(@"A radio button that provides the following features in addition to regular RadioButton:
- AutoSize works as expected when radio button is docked
- Adjustable rendering qualities
- Adjustable colors in disabled state
- Fading animations")]
    public class AdvancedRadioButton : RadioButton, IDisabledColorCapable, ISupportButtonAdapter, ISupportsFadingInternal
    {
        #region Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle lastAdapterType;
        private Color disabledForeColor;
        private Color disabledBackColor;
        private ButtonBaseAdapter adapter;
        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingPainterInternal fadingPainter;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private bool left;
        private bool maskPaint;
        private bool entered;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedRadioButton")]
        public event EventHandler<PaintStateEventArgs> PaintState;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets disabled fore color.")]
        public Color DisabledForeColor
        {
            get { return disabledForeColor != Color.Empty ? disabledForeColor : ControlPaint.DarkDark(BackColor); }
            set
            {
                if (disabledForeColor == value)
                    return;

                disabledForeColor = value;
                if (!Enabled)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets disabled back color.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets disabled back color.")]
        public Color DisabledBackColor
        {
            get { return disabledBackColor != Color.Empty ? disabledBackColor : BackColor; }
            set
            {
                if (disabledBackColor == value)
                    return;

                disabledBackColor = value;
                if (disabledBackColor != Color.Empty)
                    UseVisualStyleBackColor = false;

                if (!Enabled)
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the control.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Drawing.Color"/> value representing the background color.
        /// </returns>
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                if (UseVisualStyleBackColor && disabledBackColor != Color.Empty)
                    UseVisualStyleBackColor = false;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether to use compatible text rendering engine (GDI+) or not (GDI).
        /// </summary>
        public new bool UseCompatibleTextRendering
        {
            get { return base.UseCompatibleTextRendering; }
            set
            {
                ResetSizeCache();
                base.UseCompatibleTextRendering = value;
            }
        }

        /// <summary>
        /// Gets or sets the flat style appearance of the button control.
        /// </summary>
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get { return base.FlatStyle; }
            set
            {
                if (base.FlatStyle == value && lastFlatStyle == value)
                    return;

                base.FlatStyle = value;
                lastFlatStyle = value;
                OnFlatStyleChanged();
            }
        }

        #endregion

        #region Explicitly Implemented Interface Properties

        ButtonBaseAdapter ISupportButtonAdapter.Adapter
        {
            get
            {
                if ((adapter == null) || (base.FlatStyle != lastAdapterType))
                {
                    switch (base.FlatStyle)
                    {
                        case FlatStyle.Flat:
                            adapter = new RadioButtonFlatAdapter(this);
                            break;

                        case FlatStyle.Popup:
                            adapter = new RadioButtonPopupAdapter(this);
                            break;

                        case FlatStyle.Standard:
                            adapter = new RadioButtonStandardAdapter(this);
                            break;
                    }
                    lastAdapterType = base.FlatStyle;
                }
                return adapter;
            }
        }

        bool ISupportButtonAdapter.ShowFocusCues
        {
            get { return ShowFocusCues; }
        }

        bool ISupportButtonAdapter.ShowKeyboardCues
        {
            get { return ShowKeyboardCues; }
        }

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedRadioButton"/>.
        /// </summary>
        public AdvancedRadioButton()
        {
            CheckStyles();
            fadingPainter = new FadingPainterInternal(this, "BUTTON");
        }

        #endregion

        #region Explicit Disposing

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (fadingPainter != null)
                {
                    fadingPainter.Dispose();
                    fadingPainter = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.
        /// </returns>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (FlatStyle == FlatStyle.System)
                return base.GetPreferredSize(proposedSize);

            Size preferredSize;
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | proposedSize.Width, out preferredSize))
            {
                return preferredSize;
            }

            if (proposedSize.Width == 1)
                proposedSize.Width = 0;
            if (proposedSize.Height == 1)
                proposedSize.Height = 0;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                g.SetQuality();
                preferredSize = ((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedSize, GetAppearance());
            }

            preferredSize = LayoutUtils.UnionSizes(preferredSize + Padding.Size, MinimumSize);
            preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
            return preferredSize;





            //Ez most annyiban jó, hogy tördel, de a base.GetPreferredSize nem veszi figyelembe a RenderingQuality-t, ezért rossz méretet adhat
            //RenderingQuality helyes figyelembe vételéhez lásd az AdvancedCheckBox-ot
            //if (!AutoSize)
            //    return base.GetPreferredSize(proposedSize);

            //Size preferredSize;
            //if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | proposedSize.Width, out preferredSize))
            //{
            //    return preferredSize;
            //}

            //Size bordersAndPadding = base.GetPreferredSize(new Size(Int32.MaxValue, Int32.MaxValue)) - SingleLineSize;
            //Size proposedTextSize = proposedSize - bordersAndPadding;

            //// 0 or 1 means unbounded
            //if (proposedTextSize.Width <= 1)
            //    proposedTextSize.Width = Int32.MaxValue;
            //if (proposedTextSize.Height <= 1)
            //    proposedTextSize.Height = Int32.MaxValue;

            //using (Graphics g = Graphics.FromHwnd(Handle))
            //{
            //    bool useGdi = base.FlatStyle == FlatStyle.System || !UseCompatibleTextRendering;
            //    g.SetQuality(renderingQuality, !useGdi);
            //    TextFormatFlags flags = this.GetFormatFlags();
            //    preferredSize =
            //        useGdi
            //        ? TextRenderer.MeasureText(base.Text, base.Font, proposedTextSize, this.GetFormatFlags())
            //        : g.MeasureString(base.Text, base.Font, proposedTextSize, flags.ToStringFormat()).Ceiling();
            //}

            //preferredSize += bordersAndPadding;
            //preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
            //return preferredSize;
        }

        #endregion

        #region Protected Methods

        protected override void OnTextChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnTextChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnFontChanged(e);
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            // Needed to react Theme changes (classic to non-classic and vice versa)
            base.OnSystemColorsChanged(e);
            CheckStyles();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting flatstyle if needed (in System mode this is in WndProc)
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = base.FlatStyle;
                OnFlatStyleChanged();
                return;
            }

            // when focus is changed with cursor multiple paints occur that may couse flickering
            // leave -> focused (masked) -> not focused
            // entered -> not focused unchecked (masked) -> not focused checked (masked) -> focused
            if (left || entered)
            {
                bool focused = Focused;
                if (left && focused || entered && !focused)
                    maskPaint = true;

                left = false;
                if (focused) // clearing entered only when focused because 2 paints have to be masked
                    entered = false;
            }

            if (maskPaint)
            {
                maskPaint = false;
                return;
            }

            if (fadingPainter.State == null)
                fadingPainter.State = GetAppearance();

            fadingPainter.Paint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void WndProc(ref Message m)
        {
            if (base.FlatStyle != FlatStyle.System)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    // FlatStyle is not overridable property so in case of native rendering reacting for its change here.
                    // (On custom rendering, this is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged();
                    }

                    base.WndProc(ref m);
                    return;
            }

            base.WndProc(ref m);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            isMouseDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if (isMouseDown)
                isPressed = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

            base.OnMouseMove(mevent);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && !isPressed)
            {
                isPressed = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;
            }

            base.OnKeyUp(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            base.OnVisibleChanged(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnPaddingChanged(e);
        }

        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            e.Graphics.SetQuality();
            e.Graphics.SmoothingMode = SmoothingMode.Default; // preventing 1 pixel width invalid area of ClientRectangle

            // ButtonBase.OnPaint:
            if (AutoEllipsis)
            {
                int preferredHeight = GetPreferredSize(new Size(Width, 0)).Height;
                this.ShowToolTip(Height < preferredHeight);
            }
            else
            {
                this.ShowToolTip(false);
            }

            if (GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                ((ISupportButtonAdapter)this).Adapter.Paint(e);
            }

            // Raising PaintState
            if (PaintState != null)
                PaintState.Invoke(this, e);

            // Control.OnPaint:
            PaintEventHandler handler = (PaintEventHandler)Events[ButtonBaseAccess.EventPaint];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            if (FadingAnimationsEnabled && FadingPainterInternal.IsSupported)
                entered = true;
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (FadingAnimationsEnabled && FadingPainterInternal.IsSupported)
                left = true;
            base.OnLeave(e);
        }

        #endregion

        #region Private Methods

        private void ResetSizeCache()
        {
            preferredSizeCache.Clear();
        }

        private void OnFlatStyleChanged()
        {
            ResetSizeCache();
            CheckStyles();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void CheckStyles()
        {
            if (fadingAnimationsEnabled && FadingPainterInternal.IsSupported)
            {
                // to enabling animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (base.FlatStyle != FlatStyle.System)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private ControlAppearanceState GetAppearance()
        {
            return new ControlAppearanceState(Appearance == Appearance.Normal ? (int)BUTTONPARTS.BP_RADIOBUTTON : (int)BUTTONPARTS.BP_PUSHBUTTON, GetSystemState())
            {
                BackColor = Enabled ? BackColor : DisabledBackColor,
                ForeColor = Enabled ? ForeColor : DisabledForeColor,
                Enabled = Enabled,
                Hovered = isHovered,
                Pressed = isPressed,
                IsDefault = IsDefault,
                CheckState = Checked ? CheckState.Checked : CheckState.Unchecked,
                Text = base.Text,
                Visible = Visible,
            };
        }

        private int GetSystemState()
        {
            if (Appearance == Appearance.Normal)
            {
                RadioButtonState result = RadioButtonState.UncheckedNormal;
                if (!Enabled)
                    result = RadioButtonState.UncheckedDisabled;
                else if (isPressed)
                    result = RadioButtonState.UncheckedPressed;
                else if (isHovered)
                    result = RadioButtonState.UncheckedHot;

                if (Checked)
                    result += (int)RadioButtonState.CheckedNormal - 1;

                return (int)result;
            }

            if (!Enabled)
                return (int)PUSHBUTTONSTATES.PBS_DISABLED;

            if (isPressed || Checked)
                return (int)PUSHBUTTONSTATES.PBS_PRESSED;

            if (isHovered)
                return (int)PUSHBUTTONSTATES.PBS_HOT;

            if (IsDefault)
                return (int)PUSHBUTTONSTATES.PBS_DEFAULTED;

            return (int)PUSHBUTTONSTATES.PBS_NORMAL;
        }

        private bool ShouldSerializeDisabledBackColor()
        {
            return disabledBackColor != Color.Empty;
        }

        private bool ShouldSerializeDisabledForeColor()
        {
            return disabledForeColor != Color.Empty;
        }

        #endregion

        #endregion

        #region ISupportsFading Members

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(true)]
        [Description("Gets or sets whether fading animations are enabled for the control. Animations work in Windows Vista and above, with non-classic themes.")]
        public bool FadingAnimationsEnabled
        {
            get { return fadingAnimationsEnabled; }
            set
            {
                if (fadingAnimationsEnabled == value)
                    return;

                fadingAnimationsEnabled = value;
                CheckStyles();
            }
        }

        /// <summary>
        /// Gets or sets fading options of the control.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(FadingOptions.StandardEffects)]
        [Description("Gets or sets fading options of the control.")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FadingOptions FadingAnimationOptions
        {
            get { return fadingOptions; }
            set
            {
                if (fadingOptions == value)
                    return;

                if (!Enum<FadingOptions>.AllFlagsDefined(value))
                    throw new ArgumentOutOfRangeException("value");

                fadingOptions = value;

                // storing invisible state so when control turns visible it will fading when enabled
                if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                    fadingPainter.State = GetAppearance();

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [DefaultValue(500)]
        [Description("Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.")]
        public int FadingAnimationDefaultSpeed
        {
            get { return fadingAnimationDefaultSpeed; }
            set
            {
                if (fadingAnimationDefaultSpeed == value)
                    return;

                if (fadingAnimationDefaultSpeed < 0)
                    throw new ArgumentOutOfRangeException("value");

                fadingAnimationDefaultSpeed = value;
            }
        }

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State
        {
            get { return GetAppearance(); }
        }

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
        {
            // system speeds are determined by the painter
            return FadingAnimationDefaultSpeed;
        }

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
        {
            OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));
        }

        #endregion
    }
}