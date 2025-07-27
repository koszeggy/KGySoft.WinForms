#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedCheckBox.cs
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
using System.Diagnostics.CodeAnalysis;
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
    /// Represents a checkbox with additional features such as disabled colors, fixed auto size, buffered animations and more.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedCheckBox"/> class offers the following features in addition to <see cref="CheckBox"/>:
    /// <list type="bullet">
    /// <item><see cref="ButtonBase.AutoSize"/> property works as expected when check box is docked</item>
    /// <item>Different rendering qualities (see <see cref="TextRenderingQuality"/> and <see cref="VisualsRenderingQuality"/>) properties.</item>
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(CheckBox))]
    [Description(@"A check box that provides the following features in addition to regular CheckBox:
- AutoSize works as expected when check box is docked
- Adjustable rendering qualities
- Adjustable colors in disabled state
- Fading animations
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedCheckBox : CheckBox, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledForeColor = SystemColors.ControlText;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);
        private readonly FadingPainterInternal fadingPainter;
        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        private RenderingQuality textRenderingQuality;
        private RenderingQuality visualsRenderingQuality = RenderingQuality.High;
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle lastAdapterType;

        // NOTE: Unlike in AdvancedTextBox and AdvancedComboBox, we never set the base colors, because we handle all non-System drawings in the reimplemented adapters.
        // We only need to invoke OnBackColorChanged and OnForeColorChanged when the overriding colors are changed.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        private ButtonBaseAdapter? adapter;
        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private bool maskPaint;
        private bool hasPaintError;

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private int dpiChangingCount;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedCheckBox")]
        public event EventHandler<PaintStateEventArgs>? PaintState;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedCheckBox"/>.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Gets or sets the text rendering quality of the advanced check box. Has effect only when FlatStyle is not System.")]
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
        /// Gets or sets the rendering quality of the <see cref="AdvancedCheckBox"/> visuals.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Gets or sets the rendering quality of the advanced check box visuals. Has effect only in high DPI mode when FlatStyle is Standard.")]
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
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> state.
        /// </summary>
        [Description("The background color in the current Enabled state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => Enabled ? EnabledBackColor : DisabledBackColor;
            set
            {
                if (Enabled)
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current <see cref="Control.Enabled"/> state.
        /// </summary>
        [Description("The text color in the current Enabled state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => Enabled ? EnabledForeColor : DisabledForeColor;
            set
            {
                if (Enabled)
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Determines the background color when the control is Enabled.")]
        public Color EnabledBackColor
        {
            get => !enabledBackColor.IsEmpty ? enabledBackColor : base.BackColor;
            set
            {
                if (enabledBackColor == value)
                    return;
                enabledBackColor = value;
                if (!enabledBackColor.IsEmpty)
                    UseVisualStyleBackColor = false; // because Appearance can be Button
                if (Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor : base.ForeColor;
            set
            {
                if (enabledForeColor == value)
                    return;
                enabledForeColor = value;
                if (Enabled)
                    OnForeColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Determines the disabled background color.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : base.BackColor;
            set
            {
                if (disabledBackColor == value)
                    return;
                disabledBackColor = value;
                if (!disabledBackColor.IsEmpty)
                    UseVisualStyleBackColor = false; // because Appearance can be Button
                if (!Enabled)
                    OnBackColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedCheckBox")]
        [Description("Determines the disabled text color.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
                if (!Enabled)
                    OnForeColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("AdvancedCheckBox")]
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
        [Category("AdvancedCheckBox")]
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
                    throw new ArgumentOutOfRangeException(nameof(value));

                fadingOptions = value;

                // storing invisible state so when control turns visible it will fade when enabled
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
            get => fadingAnimationDefaultSpeed;
            set
            {
                if (fadingAnimationDefaultSpeed == value)
                    return;

                if (fadingAnimationDefaultSpeed < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                fadingAnimationDefaultSpeed = value;
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
        /// Gets or sets whether <see cref="Font"/> should be automatically scaled when DPI changes and the current thread has per-monitor DPI awareness.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see langword="true"/>, the <see cref="Font"/> is automatically scaled to the current DPI of the corresponding display on every executing platform.
        /// It also ensures that without an explicitly set font it is inherited from <see cref="Control.Parent"/>, which would be the normal behavior, but is broken in .NET 6+ and above.</para>
        /// <para>When <see langword="false"/>, the <see cref="Font"/> may or may not be scaled, and the font of the parent control may or may not be applied correctly, depending on the default behavior of the executing platform.</para>
        /// <note>Please note that this property affects the font only. Scaling the size and location always depends on the executing platform behavior.</note>
        /// </remarks>
        [Category("AdvancedCheckBox")]
        [DefaultValue(true)]
        [Description("True to auto scale Font when DPI changes and inherit the font when it's not explicitly set; False to rely on the default behavior of the current executing platform.")]
        public bool AutoScaleFont
        {
            get => autoScaleFont;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (autoScaleFont == value)
                    return;

                autoScaleFont = value;
                font?.ResetFrom(font.Font, value ? this.GetScale() : ScaleHelper.SystemScale);
                if (value)
                {
                    Control? parent = Parent;
                    defaultFont = new ScalingFont(ScaleHelper.GetFontOrDefault(parent?.Font), parent?.GetScale() ?? ScaleHelper.SystemScale);

                    // theoretically this would not be needed, but in .NET 6+ the default font handling gets broken after the first DPI change
                    SetFont(font ?? defaultFont);
                    return;
                }

                defaultFont?.Dispose();
                defaultFont = null;
                if (font == null)
                    base.Font = null!;
            }
        }

        /// <inheritdoc />
        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (dpiChangingCount > 0 && AutoScaleFont)
                    return;

                if (!ReferenceEquals(base.Font, value))
                    ResetSizeCache();

                // resetting the default font; or null, when AutoScaleFont is false
                if (value is null)
                {
                    font?.Dispose();
                    font = null;
                    Control? parent = Parent;
                    PointF parentScale = parent?.GetScale() ?? ScaleHelper.SystemScale;
                    defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent?.Font), parentScale);
                    SetFont(defaultFont);
                    return;
                }

                // setting a font explicitly - always setting base.Font, even if it is the same as value
                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;
                if (font == null)
                    font = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                else
                    font.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                SetFont(font);
            }
        }

        /// <summary>
        /// Gets or sets the flat style appearance of the checkbox.
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
                        FlatStyle.Flat => new CheckBoxFlatAdapter(this),
                        FlatStyle.Popup => new CheckBoxPopupAdapter(this),
                        FlatStyle.Standard => new CheckBoxStandardAdapter(this),
                        _ => throw new InvalidOperationException()
                    };
                    lastAdapterType = base.FlatStyle;
                }

                return adapter;
            }
        }

        bool ISupportButtonAdapter.ShowFocusCues => ShowFocusCues;
        bool ISupportButtonAdapter.ShowKeyboardCues => ShowKeyboardCues;
        ControlAppearanceState ISupportsFading<ControlAppearanceState>.State => GetAppearance();

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedCheckBox"/>.
        /// </summary>
        public AdvancedCheckBox()
        {
            CheckStyles();
            fadingPainter = new FadingPainterInternal(this, Constants.ThemeClassButton);
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            this.RegisterPerMonitorAwarenessNotifications();
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
        }

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

            using (Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero))
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
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnTextChanged(e);
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;

            ResetSizeCache();
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (EnabledBackColor != DisabledBackColor)
                OnBackColorChanged(EventArgs.Empty);
            if (EnabledForeColor != DisabledForeColor)
                OnForeColorChanged(EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting FlatStyle if needed (in System mode this is in WndProc)
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = base.FlatStyle;
                OnFlatStyleChanged();
                return;
            }

            if (maskPaint)
            {
                maskPaint = false;
                return;
            }

            CheckDpiChange();

            try
            {
                fadingPainter.State ??= GetAppearance();
                fadingPainter.Paint(e);
                hasPaintError = false;
            }
            catch (Exception ex) when (!ex.IsCritical())
            {
                // We tolerate one exception if we can recover from it in the next paint.
                // But if exceptions are thrown in two consecutive paints, we let the second one propagate.
                if (hasPaintError)
                    throw;
                hasPaintError = true;
                lastScale = PointF.Empty;
                CheckDpiChange();
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /// <inheritdoc />
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Control? parent = Parent;
            if (parent == null)
                return;

            // Setting default font from new parent font without scaling
            if (font == null)
            {
                PointF scale = this.GetScaleForParentChanged();
                defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent.Font), scale);
                if (this.GetScale() != scale)
                    lastScale = PointF.Empty;
            }

            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // if the parent control is rescaling its font due to DPI change, then ignoring the event (we do our scaling in CheckDpiChange)
            if (dpiChangingCount > 0 || !AutoScaleFont)
                return;

#if NET47_OR_GREATER || NETCOREAPP
            // The parent is rescaling its font out of a WM_DPICHANGED event (occurs typically in .NET 7+ during form handle creation)
            if (this.IsParentScalingWhileCreated())
                return;
#endif

            // but if the parent font is changing not because of scaling, then we reset our default font as well
            PointF scale = this.GetScaleForParentFontChanged();
            defaultFont!.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), scale);

            if (font != null)
                return;

            // setting default font from new parent font without scaling
            SetFont(defaultFont);

            // the parent has different scale: invalidating lastScale, so CheckDpiChange will adjust the scale if needed
            if (this.GetScale() != scale)
                lastScale = PointF.Empty;
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT:
                    // FlatStyle is not overridable property so in case of native rendering reacting for its change here.
                    // (On custom rendering, this is handled in OnPaint)
                    if (base.FlatStyle == FlatStyle.System && base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged();
                    }

                    CheckDpiChange();
                    base.WndProc(ref m);
                    return;

                case Constants.WM_DPICHANGED_BEFOREPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }

                    CheckDpiChange();
                    return;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }

                    if (AutoSize)
                        PerformLayout();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // masking next paint if check state will change; otherwise, because of double paints,
            // no animation is performed due to wrong transitions (unchecked hot -> unchecked pressed -> unchecked hot (masked) -> checked hot)
            if (isPressed && isHovered && Appearance == Appearance.Normal)
                maskPaint = true;

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
                //Invalidate();
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space && isPressed)
            {
                isPressed = false;

                // masking next paint if check state will change; otherwise, because of double paints,
                // no animation is performed due to wrong transitions (unchecked hot -> unchecked pressed -> unchecked hot (masked) -> checked hot)
                if (Appearance == Appearance.Normal)
                    maskPaint = true;
                //Invalidate();
            }

            base.OnKeyUp(e);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fade when enabled
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
                this.SetShowToolTip(false);

            if (GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                ((ISupportButtonAdapter)this).Adapter.Paint(e);
            }

            // Raising PaintState
            PaintState?.Invoke(this, e);

            // Control.OnPaint:
            PaintEventHandler? handler = (PaintEventHandler?)Events[Accessors.PaintEvent];
            handler?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            if (disposing)
            {
                font?.Dispose();
                defaultFont?.Dispose();
                font = null;
                defaultFont = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void OnFlatStyleChanged()
        {
            ResetSizeCache();
            CheckStyles();
            Invalidate();
            if (AutoScaleFont && base.FlatStyle == FlatStyle.System)
                SetFont(font ?? defaultFont);
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
            int partId = (int)(Appearance == Appearance.Normal ? BUTTONPARTS.BP_CHECKBOX : BUTTONPARTS.BP_PUSHBUTTON);
            int stateId = GetSystemState();
            bool isEnabled = Enabled;
            Color foreColor = ForeColor;
            if (lastFlatStyle == FlatStyle.Standard && VisualStyleHelper.RenderWithVisualStyles
                && (isEnabled && foreColor == defaultEnabledForeColor || !isEnabled && foreColor == defaultDisabledForeColor))
            {
                foreColor = VisualStyleHelper.GetTextColor(VisualStyleHelper.ButtonTheme, partId, stateId, foreColor);
            }

            return new ControlAppearanceState(partId, stateId)
            {
                BackColor = BackColor,
                ForeColor = foreColor,
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

        private void ResetSizeCache() => preferredSizeCache.Clear();

        private bool ShouldSerializeFont() => font != null;
        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();

            // The Font check is needed for .NET 6, where WinForms' (bad) auto font scaling may occur without notification
            if ((scale == lastScale && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            lastScale = scale;
            if (!AutoScaleFont)
                return;

            if (font is ScalingFont explicitFont)
                explicitFont.Scale(scale);
            else
                defaultFont!.Scale(scale);
            SetFont(font ?? defaultFont);
        }

        private void SetFont(ScalingFont? value)
        {
            if (value == null)
            {
                base.Font = null!;
                return;
            }

            // explicitly set fonts must be forcibly set in base.Font
            bool force = ReferenceEquals(font, value) || base.FlatStyle == FlatStyle.System;
            Font oldFont = base.Font;
            Font newFont = value.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            if (Equals(oldFont, newFont))
            {
                if (!force)
                {
                    if (ReferenceEquals(oldFont, newFont))
                        return;

                    // Non-reference equality: we are alright if the old font is not disposed...
                    // ...except in .NET Core 3.0 - .NET 5.0 when FlatStyle is System and using v1 per-monitor DPI awareness, in which case the font gets corrupted
#if NETCOREAPP && !NET6_0_OR_GREATER
                    if (!oldFont.IsDisposed() && !(isPerMonitorDpiAwarenessV1 && base.FlatStyle == FlatStyle.System && OSHelper.IsWindows && !OSHelper.IsMono))
                    {
                        ResetSizeCache();
                        if (AutoSize)
                            PerformLayout();
                        return;
                    }
#else
                    if (!oldFont.IsDisposed())
                        return;
#endif 
                }

                suppressFontChanged = true;
                try
                {
                    base.Font = null!;
                }
                finally
                {
                    suppressFontChanged = false;
                }
            }

            base.Font = newFont;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        int ISupportsFading<ControlAppearanceState>.GetFadingAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo)
            // system speeds are determined by the painter
            => FadingAnimationDefaultSpeed;

        void ISupportsFading<ControlAppearanceState>.PaintState(ControlAppearanceState state, PaintEventArgs e)
            => OnPaintState(new PaintStateEventArgs(e.Graphics, e.ClipRectangle, state));

        void IPerMonitorDpiAware.ParentFormDpiChanging()
        {
            dpiChangingCount += 1;
            if (isPerMonitorDpiAwarenessV1)
                CheckDpiChange();
        }

        void IPerMonitorDpiAware.ParentFormDpiChanged()
        {
            Debug.Assert(dpiChangingCount > 0);
            dpiChangingCount -= 1;
            if (isPerMonitorDpiAwarenessV1 && AutoSize)
                PerformLayout();
        }

        #endregion

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => CheckStyles();

        #endregion

        #endregion
    }
}
