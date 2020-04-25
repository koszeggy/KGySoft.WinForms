#region Used namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Reflection;
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
    /// <item><description>Different rendering qualities (see <see cref="RenderingQuality"/>) property.</description></item>
    /// <item><description>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</description></item>
    /// <item><description>Fading animations (only with enabled theming, on Vista and above, see <see cref="FadingAnimationsEnabled"/> and <see cref="FadingAnimationOptions"/> properties).</description></item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(Button))]
    [Description(@"A button that provides the following features in addition to regular Button:
- Allows using images even if FlatStyle is System
- IsElevated property (shield icon)
- Different rendering qualities
- Adjustable colors in disabled state
- Fading animations")]
    public class AdvancedButton : Button, ISupportsDisabledColor, ISupportButtonAdapter, ISupportsFadingInternal
    {
        #region Fields

        #region Static Fields

        private static Image securityShieldImage;
        private static readonly string nbsp = '\u00A0'.ToString(null);
        private static FieldAccessor systemSizeField;

        #endregion

        #region Instance Fields

        private readonly Dictionary<long, Size> preferredSizeCache = new Dictionary<long, Size>(4);

        private bool isElevated;
        private bool isImageUpToDate = true;
        private Image currentImage;
        private FlatStyle lastFlatStyle = FlatStyle.Standard;
        private FlatStyle reportedFlatStyle = FlatStyle.Standard;
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
        private Timer defaultAnimationTimer;
        private bool isAlternativeDefaultImage;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the control is painted in a specific state.
        /// </summary>
        [Description("Occurs when the control is painted in a specific state.")]
        [Category("AdvancedButton")]
        public event EventHandler<PaintStateEventArgs> PaintState;

        #endregion

        #region Properties

        #region Static Properties

        private static Image SecurityShieldImage
        {
            get
            {
                if (securityShieldImage != null)
                    return securityShieldImage;

                return securityShieldImage = Icons.SecurityShield.ExtractNearestBitmap(new Size(16, 16), PixelFormat.Format32bppArgb); // TODO: ToMultiResBitmap, and handle GetPreferredSize correctly
            }
        }

        /// <summary>
        /// Gets Button.systemSize field.
        /// </summary>
        private static FieldAccessor SystemSizeField
        {
            get
            {
                if (systemSizeField != null)
                    return systemSizeField;

                return systemSizeField = FieldAccessor.GetAccessor(typeof(Button).GetField("systemSize", BindingFlags.Instance | BindingFlags.NonPublic));
            }
        }

        #endregion

        #region Instance Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether an elevated shield icon should be displayed.
        /// </summary>
        [Category("AdvancedButton")]
        [Description("Gets or sets whether an elevated shield icon should be displayed.")]
        [DefaultValue(false)]
        public bool IsElevated
        {
            get { return isElevated; }
            set
            {
                if (isElevated == value)
                    return;

                isElevated = value;
                if (!isElevated && currentImage.EqualsByContent(SecurityShieldImage))
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
            get { return base.AutoSizeMode; }
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
            get { return base.TextImageRelation; }
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
            get { return reportedFlatStyle; }
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
        public new Image Image // it is also detected when base.Image changes but reacting onto that in OnPaint has a performance cost
        {
            get { return base.Image; }
            set
            {
                base.Image = value;
                isImageUpToDate = false;
                CheckImage();
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
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedButton")]
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
        [Category("AdvancedButton")]
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

        #endregion

        #region Protected Properties

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, base.DefaultSize.Height);
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
                            adapter = new ButtonFlatAdapter(this);
                            break;

                        case FlatStyle.Popup:
                            adapter = new ButtonPopupAdapter(this);
                            break;

                        case FlatStyle.Standard:
                            adapter = new ButtonStandardAdapter(this);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (fadingPainter != null)
                {
                    fadingPainter.Dispose();
                    fadingPainter = null;
                }

                if (defaultAnimationTimer != null)
                {
                    defaultAnimationTimer.Dispose();
                    defaultAnimationTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            if (preferredSizeCache.TryGetValue(((long)proposedSize.Height << 32) | proposedSize.Width, out preferredSize))
            {
                return preferredSize;
            }

            // System mode
            if (base.FlatStyle == FlatStyle.System)
            {
                if (base.Image == null && !isElevated)
                    preferredSize = base.GetPreferredSize(proposedSize);
                else
                {
                    // in system mode we must calculate with the image so hacking base.systemSize field
                    Size systemSize = (Size)SystemSizeField.Get(this);
                    if (systemSize.Width == Int32.MinValue)
                    {
                        systemSize = SizeFromClientSize(TextRenderer.MeasureText(base.Text, base.Font));
                        systemSize.Width += 14;
                        systemSize.Height += 9;
                        Size imageSize = base.Image != null ? base.Image.Size : SecurityShieldImage.Size;
                        if (imageSize.Height + 7 > systemSize.Height)
                            systemSize.Height = imageSize.Height + 7;
                        systemSizeField.Set(this, systemSize);
                    }

                    // now base.GetPreferresSize will return correct result
                    preferredSize = base.GetPreferredSize(proposedSize);
                }

                preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
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
                g.SetQuality();
                preferredSize = LayoutUtils.UnionSizes(((ISupportButtonAdapter)this).Adapter.GetPreferredSizeCore(g, proposedConstraints, GetAppearance()) + Padding.Size, MinimumSize);
            }

            if (AutoSizeMode != AutoSizeMode.GrowAndShrink)
            {
                preferredSize = LayoutUtils.UnionSizes(preferredSize, base.Size);
            }

            preferredSizeCache[((long)proposedSize.Height << 32) | proposedSize.Width] = preferredSize;
            return preferredSize;
        }

        #endregion

        #region Protected Methods

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            // Needed to react Theme changes (classic to non-classic and vice versa)
            base.OnSystemColorsChanged(e);
            CheckStyles();
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
            }

            base.WndProc(ref m);
        }

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

            if (fadingPainter.State == null)
                fadingPainter.State = GetAppearance();

            fadingPainter.Paint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
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
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = e.Button == MouseButtons.Left;
            isMouseDown = isPressed;
            Invalidate();
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

        protected override void OnFontChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnFontChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // storing invisible state so when control turns visible it will fading when enabled
            if (!Visible && (fadingOptions & (FadingOptions.Appearing | FadingOptions.AnyChange)) != FadingOptions.None)
                fadingPainter.State = GetAppearance();

            CheckDefaultAnimation();
            base.OnVisibleChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            ResetSizeCache();
            base.OnSizeChanged(e);
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

        #endregion

        #region Private Methods

        private ControlAppearanceState GetAppearance()
        {
            return new ControlAppearanceState((int)BUTTONPARTS.BP_PUSHBUTTON, (int)GetSystemState())
            {
                BackColor = Enabled ? BackColor : DisabledBackColor,
                ForeColor = Enabled ? ForeColor : DisabledForeColor,
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
            if (!WindowsUtils.IsVistaOrLater)
                return;

            bool enabled = base.FlatStyle == FlatStyle.Standard && !isPressed && !isHovered && IsDefault && Application.RenderWithVisualStyles;

            if (enabled && (defaultAnimationTimer == null || !defaultAnimationTimer.Enabled))
            {
                if (defaultAnimationTimer == null)
                {
                    defaultAnimationTimer = new Timer();
                    IntPtr hTheme = UxTheme.OpenThemeData(Handle, "BUTTON");
                    int duration;
                    if (UxTheme.GetThemeTransitionDuration(hTheme, (int)BUTTONPARTS.BP_PUSHBUTTON, (int)PUSHBUTTONSTATES.PBS_DEFAULTED, (int)PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING, Constants.TMT_TRANSITIONDURATIONS, out duration) == 0)
                        defaultAnimationTimer.Interval = duration;
                    else
                        defaultAnimationTimer.Interval = 1000;
                    defaultAnimationTimer.Tick += new EventHandler(defaultAnimationTimer_Tick);
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
            // if image is up-to-date checking consistency only (to handle setting base.Image)
            if (isImageUpToDate)
            {
                if (currentImage == base.Image
                    || currentImage == null && base.Image == null
                    || isElevated && (base.FlatStyle == FlatStyle.System ^ base.Image != null) && currentImage.EqualsByContent(SecurityShieldImage))
                    return false;
            }

            // Image > Elevated > no image
            if (FlatStyle == FlatStyle.System && WindowsUtils.IsVistaOrLater)
            {
                SystemSizeField.Set(this, new Size(Int32.MinValue, Int32.MinValue));
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

                if (base.FlatStyle != FlatStyle.System || !WindowsUtils.IsVistaOrLater)
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

        private void ResetSizeCache()
        {
            preferredSizeCache.Clear();
        }

        private bool ShouldSerializeDisabledBackColor()
        {
            return disabledBackColor != Color.Empty;
        }

        private bool ShouldSerializeDisabledForeColor()
        {
            return disabledForeColor != Color.Empty;
        }

        private bool ShouldSerializeImage()
        {
            if (currentImage == null)
                return false;
            return !isElevated && !currentImage.EqualsByContent(SecurityShieldImage);
        }

        #endregion

        #region Event Handlers
        // ReSharper disable InconsistentNaming

        void defaultAnimationTimer_Tick(object sender, EventArgs e)
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
        [Category("AdvancedButton")]
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
