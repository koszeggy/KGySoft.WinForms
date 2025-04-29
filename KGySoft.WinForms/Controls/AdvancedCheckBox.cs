#region Used namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a checkbox with full Windows Vista features support. Fully compatible with for Windows XP, too.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedCheckBox"/> class offers the following features in addition to <see cref="CheckBox"/>:
    /// <list type="bullet">
    /// <item><description><see cref="ButtonBase.AutoSize"/> property works as expected when check box is docked</description></item>
    /// <item><description>Different rendering qualities (see <see cref="RenderingQuality"/>) property.</description></item>
    /// <item><description>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</description></item>
    /// <item><description>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</description></item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(CheckBox))]
    [Description(@"A check box that provides the following features in addition to regular CheckBox:
- AutoSize works as expected when check box is docked
- Adjustable rendering qualities
- Adjustable colors in disabled state
- Fading animations")]
    public class AdvancedCheckBox : CheckBox, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal
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
        private bool maskPaint;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedCheckBox")]
        public event EventHandler<PaintStateEventArgs> PaintState;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedCheckBox")]
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
        [Category("AdvancedCheckBox")]
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
                            adapter = new CheckBoxFlatAdapter(this);
                            break;

                        case FlatStyle.Popup:
                            adapter = new CheckBoxPopupAdapter(this);
                            break;

                        case FlatStyle.Standard:
                            adapter = new CheckBoxStandardAdapter(this);
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
        /// Creates a new instance of <see cref="AdvancedCheckBox"/>.
        /// </summary>
        public AdvancedCheckBox()
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

            if (fadingPainter.State == null)
                fadingPainter.State = GetAppearance();

            if (maskPaint)
            {
                maskPaint = false;
                return;
            }

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
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // masking next paint if check state will chage; otherwise, because of double paints,
            // no animation is performed due to wrong transitions (unchecked hot -> unchecked pressed -> unchecked hot (masked) -> checked hot)
            if (isPressed && isHovered && Appearance == Appearance.Normal)
                maskPaint = true;

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
                //Invalidate();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;

                // masking next paint if check state will chage; otherwise, because of double paints,
                // no animation is performed due to wrong transitions (unchecked hot -> unchecked pressed -> unchecked hot (masked) -> checked hot)
                if (Appearance == Appearance.Normal)
                    maskPaint = true;
                //Invalidate();
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
                this.SetShowToolTip(Height < preferredHeight);
            }
            else
            {
                this.SetShowToolTip(false);
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
            PaintEventHandler handler = (PaintEventHandler)Events[Accessors.PaintEvent];
            handler?.Invoke(this, e);
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
                // to enable animations, double buffering must be disabled
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
                return;
            }

            if (base.FlatStyle != FlatStyle.System)
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private ControlAppearanceState GetAppearance()
        {
            return new ControlAppearanceState((int)(Appearance == Appearance.Normal ? BUTTONPARTS.BP_CHECKBOX : BUTTONPARTS.BP_PUSHBUTTON), GetSystemState())
            {
                BackColor = Enabled ? BackColor : DisabledBackColor,
                ForeColor = Enabled ? ForeColor : DisabledForeColor,
                Enabled = Enabled,
                Hovered = isHovered,
                Pressed = isPressed,
                IsDefault = IsDefault,
                CheckState = CheckState,
                Text = base.Text,
                Visible = Visible,
            };
        }

        private int GetSystemState()
        {
            if (Appearance == Appearance.Normal)
            {
                CheckBoxState result = CheckBoxState.UncheckedNormal;
                if (!Enabled)
                    result = CheckBoxState.UncheckedDisabled;
                else if (isPressed)
                    result = CheckBoxState.UncheckedPressed;
                else if (isHovered)
                    result = CheckBoxState.UncheckedHot;

                if (CheckState == CheckState.Checked)
                    result += (int)CheckBoxState.CheckedNormal - 1;
                else if (CheckState == CheckState.Indeterminate)
                    result += (int)CheckBoxState.MixedNormal - 1;

                return (int)result;
            }

            if (!Enabled)
                return (int)PUSHBUTTONSTATES.PBS_DISABLED;

            if (isPressed || CheckState != CheckState.Unchecked)
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
        [Category("AdvancedCheckBox")]
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
        [Category("AdvancedCheckBox")]
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
        [Category("AdvancedCheckBox")]
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
