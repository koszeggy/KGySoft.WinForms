#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageViewer.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using KGySoft.WinForms.Forms;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a high-performance image display control with zooming and panning by the keyboard and the mouse.
    /// Does not support auto-rendering animated images.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="ImageViewer"/> control supports both <see cref="Bitmap"/> and <see cref="Metafile"/> instances,
    /// including the ones with a <see cref="Image.PixelFormat"/> that is not supported by <see cref="PictureBox"/> or the GDI+ renderer, even on Linux/Mono.</para>
    /// <para>The control can use optimizations for very fast rendering even if the image is zoomed. It can use multiple CPU cores to generate the displayed image.
    /// The generation happens asynchronously, so the control may display a low-quality preview image while the high-quality one is being generated.
    /// Very huge images may consume much memory, but if too high memory pressure is detected, the optimizations are automatically turned off.</para>
    /// <para>The <see cref="SmoothingEnabled"/> property allows turning on and off interpolation when zooming. It can be used also for <see cref="Metafile"/> images,
    /// in which case the metafile is rendered with antialiasing. As a contrast, the <see cref="PictureBox"/> control always uses interpolation
    /// when displaying a resized <see cref="Bitmap"/>, and never uses antialiasing when displaying a <see cref="Metafile"/> image.</para>
    /// <para>The <see cref="Zoom"/> property allows you to set an arbitrary zoom. The zoom can also be adjusted by the mouse (Ctrl+Mouse wheel).
    /// When the displayed image is larger than the control, the scrollbars are automatically shown. Pan the image by dragging it with the mouse or by using the arrow keys.
    /// You can also use the <see cref="AutoZoom"/> property to <see langword="true"/> to automatically adjust the zoom to fit the image to the control.</para>
    /// <note type="important">Never assign the same <see cref="Bitmap"/> instance to a <see cref="PictureBox"/> and an <see cref="ImageViewer"/>.
    /// Doing so may lead to an <see cref="InvalidOperationException"/> ("bitmap region is already locked"), because <see cref="ImageViewer"/> may access the <see cref="BitmapData"/>
    /// of the <see cref="Bitmap"/> asynchronously while the <see cref="PictureBox"/> control may try to paint the image. Assigning the same image to multiple <see cref="ImageViewer"/> controls is safe.
    /// Similarly, if you access the <see cref="BitmapData"/> of a <see cref="Bitmap"/> that is assigned to the <see cref="Image"/>
    /// property, make sure you lock on the image until you access <see cref="BitmapData"/> instance. The <see cref="ImageViewer"/> control may process the image
    /// on different threads asynchronously to generate the displayed image, and during this time it locks the <see cref="Image"/> instance.
    /// To avoid the <see cref="InvalidOperationException"/>, you should also lock on the same <see cref="System.Drawing.Image"/> instance.
    /// Though the latest coding standards don't recommend using publicly exposed synchronization root objects, careful locking as described above will not cause deadlocks.
    /// Another option is if you clone the image before assigning it to the <see cref="Image"/> property, though this may cause a significant memory overhead for large images.</note>
    /// </remarks>
    public partial class ImageViewer : BaseControl
    {
        #region InvalidateFlags enum

        [Flags]
        private enum InvalidateFlags
        {
            None,
            Sizes = 1,
            DisplayImage = 1 << 1,
            Image = 1 << 2,
            All = Sizes | DisplayImage | Image
        }

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Size referenceScrollSize = new Size(32, 32);

        #endregion

        #region Instance Fields

        private readonly DisplayImageGenerator displayImageGenerator;

        private Image? image;
        private Rectangle targetRectangle;
        private Rectangle clientRectangle;
        private float zoom = 1f;
        private Size scrollbarSize;
        private Size imageSize;
        private PixelFormat pixelFormat;

        private bool isMetafile;
        private bool smoothingEnabled;
        private bool autoZoom;
        private bool sbHorizontalVisible;
        private bool sbVerticalVisible;
        private bool isApplyingZoom;
        private bool isDragging;
        private bool allowUnsafeCooperativeLocking;

        private int scrollFractionVertical;
        private int scrollFractionHorizontal;
        private Size draggingOrigin;
        private Point scrollingOrigin;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="AutoZoom"/> property is changed.
        /// </summary>
        [Category("ImageViewer")]
        [Description("Occurs when the AutoZoom property is changed.")]
        public event EventHandler? AutoZoomChanged
        {
            add => Events.AddHandler(nameof(AutoZoomChanged), value);
            remove => Events.RemoveHandler(nameof(AutoZoomChanged), value);
        }

        /// <summary>
        /// Occurs when the <see cref="Zoom"/> property is changed.
        /// </summary>
        [Category("ImageViewer")]
        [Description("Occurs when the Zoom property is changed.")]
        public event EventHandler? ZoomChanged
        {
            add => Events.AddHandler(nameof(ZoomChanged), value);
            remove => Events.RemoveHandler(nameof(ZoomChanged), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the image to be displayed by the <see cref="ImageViewer"/>.
        /// </summary>
        [Bindable(true)]
        [Category("ImageViewer")]
        [Description("Gets or sets the image to be displayed by this control.")]
        [DefaultValue(null)]
        public Image? Image
        {
            get => image;
            set
            {
                if (image == value)
                    return;

                SetImage(value);
            }
        }

        /// <summary>
        /// Gets or sets whether the control automatically adjusts the zoom to fit the image to the control.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        [Category("ImageViewer")]
        [Description("Determines whether the control automatically adjusts the zoom to fit the image to the control.")]
        [DefaultValue(false)]
        public bool AutoZoom
        {
            get => autoZoom;
            set => SetAutoZoom(value, true);
        }

        /// <summary>
        /// Gets or sets the zoom factor of the displayed image.
        /// <br/>Default value: 1.
        /// </summary>
        /// <remarks>
        /// <para>This property can be set only if the <see cref="AutoZoom"/> property is <see langword="false"/>.</para>
        /// <para>Setting this property to <see cref="Single.NaN"/> is equivalent to setting it to 1.
        /// Also, this property never throws an exception if the value is not a valid zoom factor. Instead, it is automatically adjusted to a valid value.</para>
        /// <para>The minimum zoom factor dynamically depends on the image size, so that the minimum zoomed image is at least 1 pixel in width and height.</para>
        /// <para>The maximum zoom factor is also dynamically determined based on the image size and the screen size.
        /// For <see cref="Metafile"/> images, the maximum zoom is between 1x and 2x screen size. 2x screen size is allowed if that is below 10,000 pixels.
        /// For <see cref="Bitmap"/> images, the default maximum zoom is image size x 10 (adjusted with DPI) but at least screen size x 2.</para>
        /// </remarks>
        [Category("ImageViewer")]
        [Description("When AutoZoom is False, determines the zoom factor of the displayed image. The value may be automatically adjusted to a valid zoom factor.")]
        public float Zoom
        {
            get => zoom;
            set => SetZoom(value);
        }

        /// <summary>
        /// When a <see cref="Bitmap"/> is assigned to <see cref="Image"/>, gets or sets whether rendering with resize uses interpolation (that is when <see cref="Zoom"/> is not 1).
        /// When a <see cref="Metafile"/> is assigned to <see cref="Image"/>, gets or sets whether the metafile is rendered with antialiasing.
        /// Default value: <see langword="false"/>.
        /// </summary>
        [Category("ImageViewer")]
        [Description("When a Bitmap is assigned to Image, determines whether rendering with resize uses interpolation (that is when Zoom is not 1). "
            + "When a Metafile is assigned to Image, determines whether the metafile is rendered with antialiasing.")]
        [DefaultValue(false)]
        public bool SmoothingEnabled
        {
            get => smoothingEnabled;
            set
            {
                if (smoothingEnabled == value)
                    return;
                smoothingEnabled = value;
                Invalidate(InvalidateFlags.DisplayImage);
            }
        }

        // When true, the control may use less memory, but the Image data may be accessed on other threads asynchronously, which may lead to exceptions if Image is accessed concurrently.
        // In this case Image is locked while it is being processed, so if you also want to access the Image on other threads, you need to cooperatively lock on the Image instance.
        // Must be set to false if the Image is used in another Control (e.g. Button, PictureBox, PropertyGrid, etc.) as they do not support unsafe cooperative locking.
        // In this case the Image is cloned internally before generating a good quality resized image asynchronously.
        [Category("ImageViewer")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DefaultValue(false)]
        public bool AllowUnsafeCooperativeLocking
        {
            get => allowUnsafeCooperativeLocking;
            set
            {
                if (allowUnsafeCooperativeLocking == value)
                    return;

                allowUnsafeCooperativeLocking = value;

                // When going to safe processing, we cancel the current tasks to make sure that Image can be used safely immediately.
                // If there is some image processing in progress, we invalidate the control so the pending repaint restarts in safe mode.
                if (!value && displayImageGenerator.CancelPendingTasks())
                    Invalidate();

                // NOTE: when going to unsafe mode, the possibly created image clone can be freed.
                // As we don't have strict exit conditions for that case, we let the possibly pending tasks finish. The clone will be discarded when the tasks finish.
            }
        }

        /// <inheritdoc />
        [Browsable(false)] // Hiding Cursor property because it is automatically changed. Still allowing to set it at run-time though.
        [AllowNull]
        public override Cursor Cursor
        {
            get => base.Cursor;
            set
            {
                if (IsDesignMode)
                    return;
                base.Cursor = value;
            }
        }

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                // Fixed single border
                cp.Style |= (int)Constants.WS_BORDER;
                return cp;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageViewer"/> class.
        /// </summary>
        public ImageViewer()
        {
            InitializeComponent();

            SetStyle(ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            scrollbarSize = OSUtils.IsMono ? new Size(16, 16).Scale(this.GetScale()) : new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
            sbVertical.Width = scrollbarSize.Width;
            sbHorizontal.Height = scrollbarSize.Height;

            sbVertical.ValueChanged += ScrollbarValueChanged;
            sbHorizontal.ValueChanged += ScrollbarValueChanged;

            displayImageGenerator = new DisplayImageGenerator(this);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Updates the displayed image.
        /// Call this method when <see cref="Image"/> content is mutated while the reference did not change
        /// (e.g. rotation, palette change are such mutating operations).
        /// </summary>
        public void UpdateImage()
        {
            if (image == null)
                return;

            var flags = InvalidateFlags.Image | InvalidateFlags.DisplayImage;
            Size newImageSize;
            lock (image)
            {
                newImageSize = image.Size;
                pixelFormat = image.PixelFormat;
            }

            if (newImageSize != imageSize)
            {
                imageSize = newImageSize;
                flags |= InvalidateFlags.Sizes;
            }

            Invalidate(flags);
        }

        /// <summary>
        /// Increases the zoom of the displayed image by 25%.
        /// </summary>
        public void IncreaseZoom()
        {
            SetAutoZoom(false, false);
            ApplyZoomChange(0.25f);
        }

        /// <summary>
        /// Decreases the zoom of the displayed image by 25%.
        /// </summary>
        public void DecreaseZoom()
        {
            SetAutoZoom(false, false);
            ApplyZoomChange(-0.25f);
        }

        /// <summary>
        /// Resets the zoom of the displayed image to 1 (100%), and disables the <see cref="AutoZoom"/> property.
        /// </summary>
        public void ResetZoom()
        {
            if (zoom.Equals(1f))
                return;
            AutoZoom = false;
            Zoom = 1f;
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate(InvalidateFlags.Sizes | (autoZoom ? InvalidateFlags.DisplayImage : InvalidateFlags.None));
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (image == null || e.ClipRectangle.Width <= 0 || e.ClipRectangle.Height <= 0)
                return;

            if (targetRectangle.IsEmpty)
                AdjustSizes();
            if (!targetRectangle.IsEmpty)
                PaintImage(e.Graphics);
        }

        /// <inheritdoc />
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    VerticalScroll(MouseWheelScrollDelta);
                    return true;
                case Keys.Down:
                    VerticalScroll(-MouseWheelScrollDelta);
                    return true;
                case Keys.Left:
                    HorizontalScroll(MouseWheelScrollDelta);
                    return true;
                case Keys.Right:
                    HorizontalScroll(-MouseWheelScrollDelta);
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (!(sbHorizontalVisible || sbVerticalVisible) || (e.Button & MouseButtons.Left) == MouseButtons.None)
                return;
            isDragging = true;
            draggingOrigin = new Size(e.Location);
            scrollingOrigin = new Point(sbHorizontal.Value, sbVertical.Value);
            Cursor = CursorsCache.HandGrab;
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if ((e.Button & MouseButtons.Left) == MouseButtons.None)
                return;
            isDragging = false;
            Cursor = sbHorizontalVisible || sbVerticalVisible ? CursorsCache.HandOpen : null;
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!isDragging)
                return;
            Point distance = e.Location - draggingOrigin;
            if (sbHorizontalVisible && distance.X != 0)
                sbHorizontal.SetValueSafe(scrollingOrigin.X - distance.X);
            if (sbVerticalVisible && distance.Y != 0)
                sbVertical.SetValueSafe(scrollingOrigin.Y - distance.Y);
        }

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            switch (ModifierKeys)
            {
                // zoom
                case Keys.Control:
                    if (autoZoom)
                        SetAutoZoom(false, false);
                    float delta = (float)e.Delta / MouseWheelScrollDelta / 5;
                    ApplyZoomChange(delta);
                    break;

                // vertical scroll
                case Keys.None:
                    VerticalScroll(e.Delta);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void OnMouseHWheel(HandledMouseEventArgs e)
        {
            base.OnMouseHWheel(e);

            // horizontal scroll
            if (ModifierKeys == Keys.None)
                HorizontalScroll(-e.Delta);
        }

        /// <inheritdoc />
        protected override void OnRightToLeftChanged(EventArgs e) => AdjustSizes();

        /// <summary>
        /// Raises the <see cref="AutoZoomChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAutoZoomChanged(EventArgs e) => Events.GetHandler<EventHandler>(nameof(AutoZoomChanged))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="ZoomChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnZoomChanged(EventArgs e) => Events.GetHandler<EventHandler>(nameof(ZoomChanged))?.Invoke(this, e);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            sbVertical.ValueChanged -= ScrollbarValueChanged;
            sbHorizontal.ValueChanged -= ScrollbarValueChanged;

            if (disposing)
                displayImageGenerator.Dispose();

            base.Dispose(disposing);
            if (disposing)
                Events.Dispose();
        }

        #endregion

        #region Private Methods

        private void SetImage(Image? value)
        {
            image = value;
            isMetafile = image is Metafile;
            imageSize = image?.Size ?? default;
            pixelFormat = image?.PixelFormat ?? default;
            Invalidate(InvalidateFlags.All);

            // making sure image is not under or over-zoomed
            if (!autoZoom && !isMetafile)
                SetZoom(zoom);
        }

        private void VerticalScroll(int delta)
        {
            // When scrolling by mouse, delta is always +-120 so this will be a small change on the scrollbar.
            // But we collect the fractional changes caused by the touchpad scrolling so it will not be lost either.
            int totalDelta = scrollFractionVertical + delta * sbVertical.SmallChange;
            scrollFractionVertical = totalDelta % MouseWheelScrollDelta;
            int newValue = sbVertical.Value - totalDelta / MouseWheelScrollDelta;
            sbVertical.SetValueSafe(newValue);
        }

        private void HorizontalScroll(int delta)
        {
            // When scrolling by mouse, delta is always +-120 so this will be a small change on the scrollbar.
            // But we collect the fractional changes caused by the touchpad scrolling so it will not be lost either.
            int totalDelta = scrollFractionHorizontal + delta * sbVertical.SmallChange;
            scrollFractionHorizontal = totalDelta % MouseWheelScrollDelta;
            int newValue = sbHorizontal.Value - totalDelta / MouseWheelScrollDelta;
            sbHorizontal.SetValueSafe(newValue);
        }

        private void Invalidate(InvalidateFlags flags)
        {
            if ((flags & InvalidateFlags.Sizes) != InvalidateFlags.None)
                AdjustSizes();

            if ((flags & InvalidateFlags.Image) != InvalidateFlags.None)
                displayImageGenerator.InvalidateImages();
            else if ((flags & InvalidateFlags.DisplayImage) != InvalidateFlags.None)
                displayImageGenerator.InvalidateDisplayImage();

            Invalidate();
        }

        private void AdjustSizes()
        {
            if (imageSize.IsEmpty)
            {
                sbHorizontal.Visible = sbVertical.Visible = sbHorizontalVisible = sbVerticalVisible = false;
                targetRectangle = Rectangle.Empty;
                Cursor = null;
                return;
            }

            Size clientSize = ClientSize;
            if (clientSize.Width < 1 || clientSize.Height < 1)
            {
                targetRectangle = Rectangle.Empty;
                return;
            }

            Point targetLocation;
            Size scaledSize;
            if (autoZoom)
            {
                zoom = Math.Min((float)clientSize.Width / imageSize.Width, (float)clientSize.Height / imageSize.Height);
                scaledSize = imageSize.Scale(zoom);
                targetLocation = new Point(Math.Max(0, (clientSize.Width >> 1) - (scaledSize.Width >> 1)),
                    Math.Max(0, (clientSize.Height >> 1) - (scaledSize.Height >> 1)));

                targetRectangle = new Rectangle(targetLocation, scaledSize);
                clientRectangle = new Rectangle(Point.Empty, clientSize);
                sbHorizontal.Visible = sbVertical.Visible = sbHorizontalVisible = sbVerticalVisible = false;
                Cursor = null;
                return;
            }

            scaledSize = imageSize.Scale(zoom);

            // scrollbars visibility
            sbHorizontalVisible = scaledSize.Width > clientSize.Width
                || scaledSize.Width > clientSize.Width - scrollbarSize.Width && scaledSize.Height > clientSize.Height;
            sbVerticalVisible = scaledSize.Height > clientSize.Height
                || scaledSize.Height > clientSize.Height - scrollbarSize.Height && scaledSize.Width > clientSize.Width;

            if (sbHorizontalVisible)
                clientSize.Height -= scrollbarSize.Height;
            if (sbVerticalVisible)
                clientSize.Width -= scrollbarSize.Width;
            if (clientSize.Width < 1 || clientSize.Height < 1)
            {
                targetRectangle = Rectangle.Empty;
                return;
            }

            Point clientLocation = Point.Empty;
            targetLocation = new Point((clientSize.Width >> 1) - (scaledSize.Width >> 1),
                (clientSize.Height >> 1) - (scaledSize.Height >> 1));

            bool isRtl = RightToLeft == RightToLeft.Yes;

            // both scrollbars
            if (sbHorizontalVisible && sbVerticalVisible)
            {
                sbHorizontal.Dock = sbVertical.Dock = DockStyle.None;
                sbHorizontal.Width = clientSize.Width;
                sbHorizontal.Top = clientSize.Height;
                sbHorizontal.Left = isRtl ? scrollbarSize.Width : 0;
                sbVertical.Height = clientSize.Height;
                sbVertical.Left = isRtl ? 0 : clientSize.Width;
            }
            // horizontal scrollbar
            else if (sbHorizontalVisible)
            {
                sbHorizontal.Dock = DockStyle.Bottom;
            }
            // vertical scrollbar
            else if (sbVerticalVisible)
            {
                sbVertical.Dock = isRtl ? DockStyle.Left : DockStyle.Right;
            }

            // adjust scrollbar values
            if (sbHorizontalVisible)
            {
                float origCenter = sbHorizontal.Visible
                    ? (sbHorizontal.Value - sbHorizontal.Minimum + sbHorizontal.LargeChange / 2f) / (sbHorizontal.Maximum - sbHorizontal.Minimum)
                    : 0.5f;
                sbHorizontal.Minimum = targetLocation.X;
                sbHorizontal.Maximum = targetLocation.X + scaledSize.Width;
                sbHorizontal.LargeChange = clientSize.Width;
                sbHorizontal.SmallChange = this.ScaleSize(referenceScrollSize).Width;
                int newValue = (int)(scaledSize.Width * origCenter - clientSize.Width / 2f) + targetLocation.X;
                sbHorizontal.Value = Math.Min(Math.Max(newValue, sbHorizontal.Minimum), sbHorizontal.Maximum - sbHorizontal.LargeChange);
            }

            if (sbVerticalVisible)
            {
                if (isRtl)
                {
                    targetLocation.X += scrollbarSize.Width;
                    clientLocation.X = scrollbarSize.Width;
                }

                float origCenter = sbVertical.Visible
                    ? (sbVertical.Value - sbVertical.Minimum + sbVertical.LargeChange / 2f) / (sbVertical.Maximum - sbVertical.Minimum)
                    : 0.5f;
                sbVertical.Minimum = targetLocation.Y;
                sbVertical.Maximum = targetLocation.Y + scaledSize.Height;
                sbVertical.LargeChange = clientSize.Height;
                sbVertical.SmallChange = this.ScaleSize(referenceScrollSize).Height;
                int newValue = (int)(scaledSize.Height * origCenter - clientSize.Height / 2f) + targetLocation.Y;
                sbVertical.Value = Math.Min(Math.Max(newValue, sbVertical.Minimum), sbVertical.Maximum - sbVertical.LargeChange);
            }

            sbHorizontal.Visible = sbHorizontalVisible;
            sbVertical.Visible = sbVerticalVisible;
            Cursor = sbHorizontalVisible || sbVerticalVisible ? CursorsCache.HandOpen : null;
            isDragging = false;

            clientRectangle = new Rectangle(clientLocation, clientSize);
            targetRectangle = new Rectangle(targetLocation, scaledSize);
            if (!isRtl || !sbVerticalVisible)
                return;

            clientRectangle.X = scrollbarSize.Width;
        }

        private void PaintImage(Graphics g)
        {
            g.IntersectClip(clientRectangle);
            Rectangle dest = targetRectangle;
            if (sbHorizontalVisible)
                dest.X -= sbHorizontal.Value;
            if (sbVerticalVisible)
                dest.Y -= sbVertical.Value;

            // This lock ensures that no disposed image is painted. The generator also locks on it when frees the cached preview.
            lock (displayImageGenerator.SyncRoot)
            {
                (Image? toDraw, InterpolationMode interpolationMode) = displayImageGenerator.GetDisplayImage();

                // happens if image format is not supported and generating compatible display images is disabled due to low memory
                if (toDraw == null)
                    return;

                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = interpolationMode;

                // Locking on display image so if it is the same as the original image, which is also locked when accessing its bitmap data
                // so the "bitmap region is already locked" can be avoided. Important: this cannot be ensured without locking here internally because
                // OnPaint can occur any time after invalidating.
                // NOTE: Of course, to avoid the exception every participant must cooperate and lock on the image when accessing its bitmap data.
                //       This not happens if the image used by 3rd party code (e.g. PictureBox, PropertyGrid) without locking on it.
                bool useLock = image == toDraw && allowUnsafeCooperativeLocking;
                if (useLock)
                    Monitor.Enter(toDraw);
                try
                {
                    g.DrawImage(toDraw, dest);
                }
                catch (Exception e) when (!e.IsCriticalGdi())
                {
                    // it is still possible that image is in use without lock,
                    // in which case we simply re-invalidate the control and waiting for another chance to paint
                    Invalidate();
                }
                finally
                {
                    if (useLock)
                        Monitor.Exit(toDraw);
                }
            }
        }

        private void ApplyZoomChange(float delta)
        {
            if (delta.Equals(0f))
                return;
            delta += 1;
            SetZoom(zoom * delta);
        }

        private void SetAutoZoom(bool value, bool resetIfBitmap)
        {
            if (autoZoom == value)
                return;
            autoZoom = value;
            if (resetIfBitmap && !autoZoom && !isMetafile)
                SetZoom(1f);

            Invalidate(InvalidateFlags.Sizes | (autoZoom ? InvalidateFlags.DisplayImage : InvalidateFlags.None));
            OnAutoZoomChanged(EventArgs.Empty);
        }

        private void SetZoom(float value)
        {
            if (autoZoom || isApplyingZoom)
                return;

            if (Single.IsNaN(value))
                value = 1f;
            float minZoom = image == null ? 1f : 1f / Math.Min(imageSize.Width, imageSize.Height);
            if (value < minZoom)
                value = minZoom;

            Size screenSize = Screen.GetBounds(this).Size;
            float maxZoom;

            if (isMetafile)
            {
                // For metafiles the max zoom is between 1x and 2x screen size. 2x screen size is allowed if that is below 10,000 pixels
                const int maxMetafileSize = 10_000;
                maxZoom = Math.Max(
                    Math.Min(Math.Max(screenSize.Width, maxMetafileSize), screenSize.Width << 1),
                    Math.Min(Math.Max(screenSize.Height, maxMetafileSize), screenSize.Height << 1))
                    / (float)Math.Max(imageSize.Width, imageSize.Height);
            }
            else
            {
                // For bitmaps the default maximum size is image size * 10 (adjusted with DPI) but at least screen size x 2
                PointF scale = this.GetScale();
                maxZoom = image == null ? 1f : Math.Max(
                    Math.Max(scale.X * 10, (screenSize.Width << 1) / (float)imageSize.Width),
                    Math.Max(scale.Y * 10, (screenSize.Height << 1) / (float)imageSize.Height));
            }

            if (value > maxZoom)
                value = maxZoom;

            if (zoom.Equals(value))
                return;

            zoom = value;
            Invalidate(InvalidateFlags.Sizes | InvalidateFlags.DisplayImage);
            isApplyingZoom = true;
            try
            {
                OnZoomChanged(EventArgs.Empty);
            }
            finally
            {
                isApplyingZoom = false;
            }
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
        private bool ShouldSerializeCursor() => false;
        private bool ShouldSerializeZoom() => !autoZoom && !zoom.Equals(1f);

        #endregion

        #region Event handlers

        private void ScrollbarValueChanged(object? sender, EventArgs e) => Invalidate();

        #endregion

        #endregion
    }
}
