#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedRadioButton.cs
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
using System.ComponentModel;
using System.Drawing;
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
    /// Represents a radio button with full Windows Vista features support. Fully compatible with for Windows XP, too.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedRadioButton"/> class offers the following features in addition to <see cref="RadioButton"/>:
    /// <list type="bullet">
    /// <item><description><see cref="ButtonBase.AutoSize"/> property works as expected when radio button is docked</description></item>
    /// <item><description>Different rendering qualities (see <see cref="TextRenderingQuality"/> and <see cref="VisualsRenderingQuality"/>) properties.</description></item>
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
    public class AdvancedRadioButton : RadioButton, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal
    {
        #region Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;

        private RenderingQuality textRenderingQuality;
        private RenderingQuality visualsRenderingQuality = RenderingQuality.High;
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle lastAdapterType;
        private Color disabledForeColor;
        private Color disabledBackColor;
        private ButtonBaseAdapter? adapter;
        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
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
        public event EventHandler<PaintStateEventArgs>? PaintState;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedRadioButton"/>.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets the text rendering quality of the advanced radio button. Has effect only when FlatStyle is not System.")]
        [DefaultValue(RenderingQuality.SystemDefault)]
        public RenderingQuality TextRenderingQuality
        {
            get => textRenderingQuality;
            set
            {
                if (textRenderingQuality == value)
                    return;

                if (!Enum<RenderingQuality>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                textRenderingQuality = value;
                Invalidate();
                if (AutoSize)
                {
                    ResetSizeCache();
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the rendering quality of the <see cref="AdvancedRadioButton"/> visuals.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets the rendering quality of the advanced radio button visuals. Has effect only in high DPI mode.")]
        [DefaultValue(RenderingQuality.High)]
        public RenderingQuality VisualsRenderingQuality
        {
            get => visualsRenderingQuality;
            set
            {
                if (visualsRenderingQuality == value)
                    return;

                if (!Enum<RenderingQuality>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                visualsRenderingQuality = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedRadioButton")]
        [Description("Gets or sets disabled fore color.")]
        public Color DisabledForeColor
        {
            get => disabledForeColor != Color.Empty ? disabledForeColor : ControlPaint.DarkDark(BackColor);
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
            get => disabledBackColor != Color.Empty ? disabledBackColor : BackColor;
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
            get => base.BackColor;
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
            get => base.UseCompatibleTextRendering;
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
            get => base.FlatStyle;
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
                    adapter = base.FlatStyle switch
                    {
                        FlatStyle.Flat => new RadioButtonFlatAdapter(this),
                        FlatStyle.Popup => new RadioButtonPopupAdapter(this),
                        FlatStyle.Standard => new RadioButtonStandardAdapter(this),
                        _ => throw new InvalidOperationException()
                    };
                    lastAdapterType = base.FlatStyle;
                }
                return adapter;
            }
        }

        bool ISupportButtonAdapter.ShowFocusCues => ShowFocusCues;
        bool ISupportButtonAdapter.ShowKeyboardCues => ShowKeyboardCues;

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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                fadingPainter.Dispose();

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

            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            if (proposedSize.Width == 1)
                proposedSize.Width = 0;
            if (proposedSize.Height == 1)
                proposedSize.Height = 0;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                g.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);
                preferredSize = ((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedSize, GetAppearance());
            }

            preferredSize = LayoutUtils.UnionSizes(preferredSize + Padding.Size, MinimumSize);
            preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
            return preferredSize;
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnTextChanged(e);
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            // Needed to react Theme changes (classic to non-classic and vice versa)
            base.OnSystemColorsChanged(e);
            CheckStyles();
        }

        /// <inheritdoc />
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

            fadingPainter.State ??= GetAppearance();
            fadingPainter.Paint(e);
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            isMouseDown = false;
            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if (isMouseDown)
                isPressed = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

            base.OnMouseMove(mevent);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && !isPressed)
            {
                isPressed = true;
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;
            }

            base.OnKeyUp(e);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            base.OnVisibleChanged(e);
        }

        /// <inheritdoc />
        protected override void OnPaddingChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnPaddingChanged(e);
        }

        /// <summary>
        /// Paints the specified state of this control, and raises the <see cref="PaintState"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintStateEventArgs"/> that contains the event data.</param>
        protected virtual void OnPaintState(PaintStateEventArgs e)
        {
            e.Graphics.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);

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
            PaintEventHandler? handler = (PaintEventHandler?)Events[Accessors.PaintEvent];
            handler?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void OnEnter(EventArgs e)
        {
            if (FadingAnimationsEnabled && FadingPainterInternal.IsSupported)
                entered = true;
            base.OnEnter(e);
        }

        /// <inheritdoc />
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
            int partId = (int)(Appearance == Appearance.Normal ? BUTTONPARTS.BP_RADIOBUTTON : BUTTONPARTS.BP_PUSHBUTTON);
            int stateId = GetSystemState();
            bool isEnabled = Enabled;
            Color foreColor = !isEnabled ? DisabledForeColor : base.ForeColor;
            if (lastFlatStyle == FlatStyle.Standard && isEnabled && VisualStyleHelper.RenderWithVisualStyles && foreColor == SystemColors.ControlText)
                foreColor = VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, partId, stateId, foreColor);
            return new ControlAppearanceState(partId, stateId)
            {
                BackColor = isEnabled ? BackColor : DisabledBackColor,
                ForeColor = foreColor,
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
            get => fadingAnimationsEnabled;
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
            get => fadingOptions;
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
            get => fadingAnimationDefaultSpeed;
            set
            {
                if (fadingAnimationDefaultSpeed == value)
                    return;

                if (fadingAnimationDefaultSpeed < 0)
                    throw new ArgumentOutOfRangeException("value");

                fadingAnimationDefaultSpeed = value;
            }
        }

        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

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