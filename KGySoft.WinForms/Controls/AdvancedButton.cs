#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedButton.cs
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
using System.Drawing.Imaging;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a pushbutton with full Windows Vista features support. Fully compatible with for Windows XP, too.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedButton"/> class offers the following features in addition to <see cref="Button"/>:
    /// <list type="bullet">
    /// <item><description>Images are displayed also when <see cref="ButtonBase.FlatStyle"/> property is <see cref="System.Windows.Forms.FlatStyle.System"/>. On a pre-Vista Windows <c>FlatStyle</c> is automatically switched to <see cref="System.Windows.Forms.FlatStyle.Standard"/> at runtime.</description></item>
    /// <item><description>Elevated mode (see <see cref="IsElevated"/> property). The shield icon is rendered also on a pre-Vista Windows.</description></item>
    /// <item><description>Different rendering qualities (see <see cref="TextRenderingQuality"/>) property.</description></item>
    /// <item><description>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</description></item>
    /// <item><description>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</description></item>
    /// <item><description>Slightly different appearance in some cases (e. g. focus rectangle size and width, image shifts along with text in classic or popup appearance,
    /// fixed highlight fore color in high contrast mode with visual styles enabled, etc.).</description></item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(Button))]
    [Description(@"A button that provides the following features in addition to regular Button:
- Allows using images even if FlatStyle is System
- IsElevated property (shield icon)
- Different rendering qualities
- Adjustable colors in disabled state
- Fading animations
- Fixed appearance in several cases")]
    public class AdvancedButton : Button, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal
    {
        #region Fields

        #region Static Fields

        private static readonly string nbsp = '\u00A0'.ToString(null);
        private static readonly Size referenceIconSize = new Size(16, 16);

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);

        private bool isElevated;
        private bool isImageUpToDate = true;
        private Image? currentImage; // the actual displayed image, including the shield icon when base.Image is null
        private FlatStyle lastFlatStyle = FlatStyle.Standard; // the explicitly set or the detected flat style changed in base
        private FlatStyle reportedFlatStyle = FlatStyle.Standard; // the flat style that is reported by the control (can be different when base does not support System)
        private FlatStyle lastAdapterType;
        private RenderingQuality textRenderingQuality;
        private Color disabledForeColor;
        private Color disabledBackColor;
        private ButtonBaseAdapter? adapter;
        private bool isHovered;
        private bool isMouseDown;
        private bool isPressed;
        private bool fadingAnimationsEnabled = true;
        private int fadingAnimationDefaultSpeed = 500;
        private FadingPainterInternal fadingPainter;
        private FadingOptions fadingOptions = FadingOptions.StandardEffects;
        private Timer? defaultAnimationTimer;
        private bool isAlternativeDefaultImage;
        private Bitmap? cachedSecurityShieldImage;
        private Size cachedSecurityShieldImageSize;
        private PointF lastScale;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedButton")]
        public event EventHandler<PaintStateEventArgs>? PaintState;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether an elevated shield icon should be displayed.
        /// </summary>
        [Category("AdvancedButton")]
        [Description("Gets or sets whether an elevated shield icon should be displayed.")]
        [DefaultValue(false)]
        public bool IsElevated
        {
            get => isElevated;
            set
            {
                if (isElevated == value)
                    return;

                isElevated = value;
                if (!isElevated && ReferenceEquals(currentImage, cachedSecurityShieldImage))
                    base.Image = null;

                isImageUpToDate = false;
                CheckImage();

                Invalidate();
                if (AutoSize)
                    PerformLayout();
            }
        }

        /// <returns>
        /// The text associated with this control.
        /// </returns>
        [AllowNull]
        public override string Text
        {
            get
            {
                string result = base.Text;
                return result == nbsp ? String.Empty : base.Text;
            }
            set
            {
                // this fixes the issue that in System mode there can be no image without text
                if (String.IsNullOrEmpty(value))
                    value = nbsp;

                ResetSizeCache();
                base.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode by which the <see cref="AdvancedButton"/> automatically resizes itself.
        /// </summary>
        [DefaultValue(AutoSizeMode.GrowOnly)]
        public new AutoSizeMode AutoSizeMode
        {
            get => base.AutoSizeMode;
            set
            {
                ResetSizeCache();
                base.AutoSizeMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of text and image relative to each other.
        /// </summary>
        [DefaultValue(TextImageRelation.ImageBeforeText)]
        public new TextImageRelation TextImageRelation
        {
            get => base.TextImageRelation;
            set
            {
                ResetSizeCache();
                base.TextImageRelation = value;
            }
        }

        /// <summary>
        /// Gets or sets the flat style appearance of the button control.
        /// </summary>
        public new FlatStyle FlatStyle // it is also detected when base.FlatStyle changes but reacting onto that in OnPaint has a performance cost
        {
            get => reportedFlatStyle;
            set
            {
                if (reportedFlatStyle == value && base.FlatStyle == value && lastFlatStyle == value)
                    return;

                base.FlatStyle = lastFlatStyle = reportedFlatStyle = value;
                OnFlatStyleChanged(false);
            }
        }

        /// <summary>
        /// Gets or sets the image that is displayed on the button control.
        /// </summary>
        public new Image? Image // it is also detected when base.Image changes but reacting onto that in OnPaint has a performance cost
        {
            get => base.Image;
            set
            {
                base.Image = value;
                isImageUpToDate = false;
                CheckImage();
            }
        }

        /// <summary>
        /// Gets or sets the text rendering quality of the <see cref="AdvancedButton"/>.
        /// </summary>
        [Category("AdvancedButton")]
        [Description("Gets or sets the text rendering quality of the button control. Has effect only when FlatStyle is not System.")]
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
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
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

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override Size DefaultSize => new(100, base.DefaultSize.Height);

        #endregion

        #region Private Properties

        private Image SecurityShieldImage
        {
            get
            {
                Size currentSize = this.ScaleSize(referenceIconSize);
                if (currentSize != cachedSecurityShieldImageSize || cachedSecurityShieldImage == null)
                {
                    if (cachedSecurityShieldImage != null && ReferenceEquals(cachedSecurityShieldImage, currentImage))
                    {
                        isImageUpToDate = false;
                        if (ReferenceEquals(cachedSecurityShieldImage, base.Image))
                            base.Image = null;
                    }

                    cachedSecurityShieldImage?.Dispose();
                    using var icon = Icons.SystemShield;
                    cachedSecurityShieldImage = icon.ExtractNearestBitmap(currentSize, PixelFormat.Format32bppArgb);
                    cachedSecurityShieldImageSize = currentSize;
                    if (!isImageUpToDate)
                        Invalidate();
                }

                return cachedSecurityShieldImage;
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
                        FlatStyle.Flat => new ButtonFlatAdapter(this),
                        FlatStyle.Popup => new ButtonPopupAdapter(this),
                        FlatStyle.Standard => new ButtonStandardAdapter(this),
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
        /// Creates a new instance of <see cref="AdvancedButton"/>.
        /// </summary>
        public AdvancedButton()
        {
            base.TextImageRelation = TextImageRelation.ImageBeforeText;
            CheckStyles();
            fadingPainter = new FadingPainterInternal(this, "BUTTON");
        }

        #endregion

        #region Explicit Disposing

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadingPainter.Dispose();
                defaultAnimationTimer?.Dispose();
                defaultAnimationTimer = null;
                cachedSecurityShieldImage?.Dispose();
                cachedSecurityShieldImage = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <inheritdoc />
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | (uint)proposedSize.Width, out var preferredSize))
                return preferredSize;

            // System mode
            if (base.FlatStyle == FlatStyle.System)
            {
                if (base.Image == null && !isElevated)
                    preferredSize = base.GetPreferredSize(proposedSize);
                else
                {
                    // in system mode we must calculate with the image so hacking base.systemSize field
                    Size systemSize = this.GetSystemSize();
                    if (systemSize.Width == Int32.MinValue)
                    {
                        systemSize = SizeFromClientSize(TextRenderer.MeasureText(base.Text, base.Font));
                        systemSize.Width += 14;
                        systemSize.Height += 9;
                        Size imageSize = base.Image != null ? base.Image.Size : SecurityShieldImage.Size;
                        if (imageSize.Height + 7 > systemSize.Height)
                            systemSize.Height = imageSize.Height + 7;
                        this.SetSystemSize(systemSize);
                    }

                    // now base.GetPreferresSize will return correct result
                    preferredSize = base.GetPreferredSize(proposedSize);
                }

                preferredSizeCache[((long)proposedSize.Height << 32) | (uint)proposedSize.Width] = preferredSize;
                return preferredSize;
            }

            // Non-System mode: we must calculate with the current rendering quality so reimplementing base logic
            Size proposedConstraints = proposedSize;
            if (proposedConstraints.Width == 1)
                proposedConstraints.Width = 0;
            if (proposedConstraints.Height == 1)
                proposedConstraints.Height = 0;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                g.SetTextRenderingQuality(textRenderingQuality, UseCompatibleTextRendering);
                preferredSize = LayoutUtils.UnionSizes(((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedConstraints, GetAppearance()) + Padding.Size, MinimumSize);
            }

            if (AutoSizeMode != AutoSizeMode.GrowAndShrink)
            {
                preferredSize = LayoutUtils.UnionSizes(preferredSize, Size);
            }

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
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            // Needed to react Theme changes (classic to non-classic and vice versa)
            base.OnSystemColorsChanged(e);
            CheckStyles();
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_PAINT when base.FlatStyle == FlatStyle.System:
                    // Image and FlatStyle are not overridable properties so in case of native rendering reacting their change here.
                    // (On custom rendering, image change is handled in OnPaint)
                    if (base.FlatStyle != lastFlatStyle)
                    {
                        lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                        OnFlatStyleChanged(true);
                    }

                    if (CheckImage() && AutoSize)
                        PerformLayout();

                    base.WndProc(ref m);
                    return;

                //// Non-System FlatStyle with elevated icon: invalidating the icon
                //case Constants.WM_DPICHANGED_BEFOREPARENT when isElevated && base.FlatStyle != FlatStyle.System && ReferenceEquals(base.Image, cachedSecurityShieldImage):
                //    base.WndProc(ref m);
                //    isImageUpToDate = false;
                //    base.Image = null; // it will be updated in CheckImage
                //    Invalidate();
                //    return;

                // System FlatStyle: the WM_DPICHANGED_AFTERPARENT resets the elevated icon, but we want to prevent if an image is set
                case Constants.WM_DPICHANGED_AFTERPARENT when isElevated && base.FlatStyle == FlatStyle.System && base.Image != null:
                    base.WndProc(ref m);
                    isImageUpToDate = false;
                    Invalidate();
                    return;
            }

            base.WndProc(ref m);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            // adjusting flatstyle if needed (in System mode this is in WndProc)
            bool invalidated = false;
            if (base.FlatStyle != lastFlatStyle)
            {
                lastFlatStyle = reportedFlatStyle = base.FlatStyle;
                OnFlatStyleChanged(true);
                invalidated = true;
            }

            if (CheckImage() && AutoSize)
            {
                PerformLayout();
                invalidated = true;
            }

            CheckDefaultAnimation();

            // in this case new paint will be triggered
            if (invalidated)
                return;

            fadingPainter.State ??= GetAppearance();
            fadingPainter.Paint(e);
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            CheckDpiChange();
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
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            isMouseDown = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            Invalidate();
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
        protected override void OnFontChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            CheckDefaultAnimation();
            base.OnVisibleChanged(e);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnSizeChanged(e);
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

        #endregion

        #region Private Methods

        private ControlAppearanceState GetAppearance()
        {
            int partId = (int)BUTTONPARTS.BP_PUSHBUTTON;
            int stateId = (int)GetSystemState();
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
                Text = base.Text,
                Visible = Visible,
            };
        }

        private PUSHBUTTONSTATES GetSystemState()
        {
            if (!Enabled)
                return PUSHBUTTONSTATES.PBS_DISABLED;

            if (isPressed)
                return PUSHBUTTONSTATES.PBS_PRESSED;

            if (isHovered)
                return PUSHBUTTONSTATES.PBS_HOT;

            if (IsDefault)
                return fadingAnimationsEnabled && (fadingOptions & FadingOptions.StandardEffects) != FadingOptions.None && isAlternativeDefaultImage
                ? PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING
                : PUSHBUTTONSTATES.PBS_DEFAULTED;

            return PUSHBUTTONSTATES.PBS_NORMAL;
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

        private void CheckDefaultAnimation()
        {
            if (!WindowsUtils.IsVistaOrLater || !VisualStyleHelper.HasDefaultAnimation((int)BUTTONPARTS.BP_PUSHBUTTON, (int)PUSHBUTTONSTATES.PBS_DEFAULTED, (int)PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING))
                return;

            bool enabled = base.FlatStyle == FlatStyle.Standard && !isPressed && !isHovered && IsDefault && VisualStyleHelper.RenderWithVisualStyles && !VisualStyleHelper.HighContrast;
            if (enabled && (defaultAnimationTimer == null || !defaultAnimationTimer.Enabled))
            {
                if (defaultAnimationTimer == null)
                {
                    defaultAnimationTimer = new Timer();
                    defaultAnimationTimer.Interval = UxTheme.TryGetThemeTransitionDuration(VisualStyleHelper.ButtonTheme, (int)BUTTONPARTS.BP_PUSHBUTTON,
                        (int)PUSHBUTTONSTATES.PBS_DEFAULTED,
                        (int)PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING,
                        Constants.TMT_TRANSITIONDURATIONS, out int duration) && duration != 0
                        ? duration
                        : 1000;
                    defaultAnimationTimer.Tick += defaultAnimationTimer_Tick;
                }

                isAlternativeDefaultImage = false;
                defaultAnimationTimer.Enabled = true;
            }
            else if (!enabled && defaultAnimationTimer != null && defaultAnimationTimer.Enabled)
            {
                defaultAnimationTimer.Enabled = false;
                isAlternativeDefaultImage = false;
            }
        }

        /// <summary>
        /// Checks image consistency. Returns true if image update has been performed.
        /// </summary>
        private bool CheckImage()
        {
            if (!IsHandleCreated)
                return true;

            // if image is up-to-date checking consistency only (to handle setting base.Image)
            if (isImageUpToDate)
            {
                if (!isElevated && currentImage == base.Image
                    || currentImage == null && base.Image == null
                    || isElevated && (base.FlatStyle == FlatStyle.System ^ base.Image != null) && ReferenceEquals(currentImage, SecurityShieldImage))
                    return false;
            }

            // Resetting System FlatStyle if it was faked and there is no image anymore
            if (reportedFlatStyle == FlatStyle.System && base.FlatStyle != reportedFlatStyle && base.Image == null && !isElevated)
                base.FlatStyle = lastFlatStyle = FlatStyle.System;

            // Image > Elevated > no image
            if (FlatStyle == FlatStyle.System && WindowsUtils.IsVistaOrLater)
            {
                this.SetSystemSize(new Size(Int32.MinValue, Int32.MinValue));
            }

            Invalidate();
            ResetSizeCache();
            isImageUpToDate = true;
            if (base.Image != null)
            {
                currentImage = base.Image;
                if (base.FlatStyle == FlatStyle.System)
                {
                    if (!WindowsUtils.IsVistaOrLater || !WindowsUtils.IsComCtlV6Available)
                    {
                        base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
                        return true;
                    }

                    Bitmap bmp = base.Image as Bitmap ?? new Bitmap(base.Image);
                    User32.SendMessage(Handle, Constants.BM_SETIMAGE, new IntPtr(1), bmp.GetHicon());
                }

                return true;
            }

            currentImage = null;

            if (isElevated)
            {
                currentImage = SecurityShieldImage;

                if (base.FlatStyle != FlatStyle.System || !WindowsUtils.IsVistaOrLater || !WindowsUtils.IsComCtlV6Available)
                {
                    base.Image = currentImage;

                    if (!WindowsUtils.IsVistaOrLater || !WindowsUtils.IsComCtlV6Available)
                    {
                        base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
                        return true;
                    }

                    return true;
                }

                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
            }
            else if (base.FlatStyle == FlatStyle.System && WindowsUtils.IsVistaOrLater)
            {
                User32.SendMessage(Handle, Constants.BCM_SETSHIELD, IntPtr.Zero, IntPtr.Zero);
            }

            return true;
        }

        private void OnFlatStyleChanged(bool ignoreCheckImage)
        {
            CheckDefaultAnimation();

            // Images are supported only in Vista and above in System mode when Application.EnableVisualStyles was called
            if (base.FlatStyle == FlatStyle.System && (base.Image != null || isElevated) && (!WindowsUtils.IsVistaOrLater || !WindowsUtils.IsComCtlV6Available))
            {
                // note: this will not change the reported FlatStyle in designer
                base.FlatStyle = lastFlatStyle = FlatStyle.Standard;
                ImageAlign = ContentAlignment.MiddleRight;
            }

            isImageUpToDate = false;
            if (!ignoreCheckImage)
                CheckImage();

            if (base.FlatStyle == FlatStyle.System && isElevated && base.Image.EqualsByContent(SecurityShieldImage))
                base.Image = null;

            CheckStyles();
            ResetSizeCache();
            Invalidate();
            if (AutoSize)
                PerformLayout();
        }

        private void ResetSizeCache() => preferredSizeCache.Clear();
        private bool ShouldSerializeDisabledBackColor() => disabledBackColor != Color.Empty;
        private bool ShouldSerializeDisabledForeColor() => disabledForeColor != Color.Empty;

        private bool ShouldSerializeImage()
        {
            if (currentImage == null)
                return false;
            return !isElevated && ReferenceEquals(currentImage, cachedSecurityShieldImage);
        }

        private void CheckDpiChange()
        {
            PointF scale = this.GetScale();
            if (scale == lastScale)
                return;

            lastScale = scale;
            if (isElevated)
            {
                base.Image = null;
                isImageUpToDate = false;
                Invalidate();
            }

            ResetSizeCache();
        }

        #endregion

        #region Event Handlers
        // ReSharper disable InconsistentNaming

        void defaultAnimationTimer_Tick(object? sender, EventArgs e)
        {
            isAlternativeDefaultImage = !isAlternativeDefaultImage;
            Invalidate();
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #endregion

        #region ISupportsFading Members

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
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

                Invalidate(); // delete if ResetOptions is uncommented
            }
        }

        /// <summary>
        /// Gets or sets default fading animation speed for non-standard animations in milliseconds. Zero value means immediate change.
        /// </summary>
        [Category("AdvancedButton")]
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
