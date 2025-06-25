#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseForm.cs
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
#if !NET5_0_OR_GREATER
using System.Collections.Specialized;
#endif
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Drawing;
using KGySoft.Libraries.Language;
#if !NET5_0_OR_GREATER
using KGySoft.Reflection;
using KGySoft.WinForms.Reflection;
#endif
using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETFRAMEWORK && !NET47_OR_GREATER
#pragma warning disable CS1574 // the documentation contains types that are not available in every target
#endif

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// A base form with additional features and bug fixes.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseForm"/> class provides the following features and changes:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the form is disposed. To do that for the events of derived forms as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item><see cref="ToolTip"/> property to create tool tips for the controls on the form.</item>
    /// <item><see cref="CommandBindings"/> property. See the <a href="https://kgysoft.net/corelibraries#command-binding" target="_blank">online documentation</a> for details.</item>
    /// <item>Advanced MDI application support, see <see cref="ShowMdiChild"/> method and <see cref="CalledMdiChildClosed"/> and <see cref="PaintMdiClientArea"/> events.</item>
    /// <item>Fixes a <a href="https://github.com/dotnet/winforms/issues/1504" target="_blank">resizing bug</a> that exists in .NET Framework and .NET Core 3.x that can occur with multiple displays.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// <item>Fixes the small icon of the form if the application is per-monitor DPI aware and the DPI of the form is different from the DPI of the primary display.</item>
    /// </list>
    /// </remarks>
    public class BaseForm : Form
    {
        #region Fields

        #region Static Fields

#if !NET5_0_OR_GREATER
        private static readonly BitVector32.Section formStateRenderSizeGrip;
#endif

        #endregion

        #region Instance Fields

        #region Protected Fields
        
        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.ToolTip"/> of the <see cref="BaseForm"/>.
        /// Kept for compatibility, if a derived form uses it from the designer.
        /// From code, prefer using the <see cref="ToolTip"/> property instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected readonly ToolTip BaseToolTip;

        #endregion

        #region Private Fields
        
        private readonly CommandBindingsCollection commandBindings = new WinFormsCommandBindingsCollection();
        private readonly InvokeMarshaller invoker;

        private bool translateControls;
        private bool isLoaded;
        private bool suspended;
        private bool resumeCaller;
        private BaseForm? callerMdiForm;
        private MdiClient? mdiClient;
        private PointF deviceScale = ScaleHelper.SystemScale;
        private Icon? smallIcon;

        #endregion

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an MDI child showed by a <see cref="ShowMdiChild"/> call is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI child showed by a ShowMdiChild call is closed.")]
        public event FormClosedEventHandler? CalledMdiChildClosed
        {
            add => Events.AddHandler(nameof(CalledMdiChildClosed), value);
            remove => Events.RemoveHandler(nameof(CalledMdiChildClosed), value);
        }

        /// <summary>
        /// Occurs when MDI area of the form has to be repainted. <see cref="Form.IsMdiContainer"/> must be true to access this event.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when MDI area of the form has to be repainted. IsMdiContainer must be true to access this event.")]
        public event PaintEventHandler? PaintMdiClientArea
        {
            add
            {
                MdiClient client = GetMdiClient();
                client.Paint += value;
            }
            remove
            {
                MdiClient client = GetMdiClient();
                client.Paint -= value;
            }
        }

        /// <summary>
        /// Occurs when an MDI Child window called by <see cref="ShowMdiChild"/> suspends the caller instance.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI Child window called by ShowMdiChild suspends the caller instance.")]
        public event EventHandler? Suspended
        {
            add => Events.AddHandler(nameof(Suspended), value);
            remove => Events.RemoveHandler(nameof(Suspended), value);
        }

        /// <summary>
        /// Occurs when the MDI Child window called by <see cref="ShowMdiChild"/> that suspended the caller instance is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when the MDI Child window called by ShowMdiChild that suspended the caller instance is closed.")]
        public event EventHandler? Resumed
        {
            add => Events.AddHandler(nameof(Resumed), value);
            remove => Events.RemoveHandler(nameof(Resumed), value);
        }

        /// <summary>
        /// Occurs when the scale of the form's display device changes. Similar to the <see cref="Form.DpiChanged"/> event,
        /// but this is available for all .NET versions, and the event arguments contain the scale of the display rather than DPI values.
        /// </summary>
        /// <remarks>
        /// <para>This event is raised only on Windows 8.1 or later when the application has per-monitor DPI awareness.</para>
        /// <para>On platform targets where the <see cref="Form.DpiChanged"/> event is also available, this event is raised after <see cref="Form.DpiChanged"/>.
        /// If you want to prevent auto-scaling by <see cref="Form.DpiChanged"/>, subscribe <see cref="Form.DpiChanged"/> as well (or override <see cref="Form.OnDpiChanged">OnDpiChanged</see>),
        /// and set <see cref="CancelEventArgs.Cancel"/> in the event arguments to <see langword="true"/>.
        /// In contrast, the arguments of the <see cref="DeviceScaleChanged"/> event cannot be canceled, but this event does not do anything automatically if not subscribed.</para>
        /// <para>Unlike in the <see cref="Form.DpiChanged"/> event arguments, the <see cref="DeviceScaleChangedEventArgs.SuggestedBounds">DeviceScaleChangedEventArgs.SuggestedBounds</see> property
        /// contains a scaled size even if <see cref="ContainerControl.AutoScaleMode"/> is <see cref="AutoScaleMode.None"/>.
        /// The suggested bounds still can be ignored by the subscriber of the event.</para>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs when the scale of the form's display device changes. Similar to the DpiChanged event, "
            + "but this is available for all .NET versions, and the event arguments contain the scale of the display rather than DPI values.")]
        public event EventHandler<DeviceScaleChangedEventArgs>? DeviceScaleChanged
        {
            add => Events.AddHandler(nameof(DeviceScaleChanged), value);
            remove => Events.RemoveHandler(nameof(DeviceScaleChanged), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the form should translate its controls.
        /// </summary>
        [Category("BaseForm")]
        [DefaultValue(false)]
        [Description("[OBSOLETE]Gets or sets whether the form should translate its controls.")]
        [Obsolete("Old auto-translation does not work anymore, it just removes the possible translation postfixes.")]
        [Browsable(false)]
        public bool TranslateControls
        {
            get => translateControls;
            set => translateControls = value;
        }

        /// <summary>
        /// Gets whether the form is suspended by a called MDI child.
        /// </summary>
        [Browsable(false)]
        public bool IsSuspended => suspended;

        /// <summary>
        /// Gets the command bindings of this form. The <see cref="O:KGySoft.ComponentModel.CommandBindingsCollection.Add">Add</see> methods also add
        /// the <see cref="PropertyCommandStateUpdater"/> to the created bindings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        /// <summary>
        /// Gets the current scale of the form's display device. Before showing the form, or when per-monitor DPI awareness is not enabled,
        /// this property returns the system scale of the primary display, which is the same as the <see cref="ScaleHelper.SystemScale">ScaleHelper.SystemScale</see> property.
        /// </summary>
        /// <remarks>
        /// <para>This property is similar to the <see cref="Control.DeviceDpi"/> property, but it returns the scale factor as a <see cref="PointF"/> value,
        /// and it is available on all .NET versions, even on .NET Framework 3.5.</para>
        /// <note>Even on platforms where the <see cref="Control.DeviceDpi"/> is available, the <see cref="Control.DeviceDpi"/> property
        /// may return an incorrect value (e.g. the .NET Framework requires the DPI awareness settings in the <c>app.config</c> file, even
        /// if the awareness is set in the application manifest). In contrast, this property always returns the correct scale
        /// if there is an application manifest file or the DPI awareness is set for the application manually.</note>
        /// </remarks>
        [Browsable(false)]
        public PointF DeviceScale => deviceScale;

        /// <inheritdoc cref="Form.Icon" />
        public new Icon? Icon
        {
            get => base.Icon;
            set
            {
                base.Icon = value;
                smallIcon?.Dispose();
                if (value == null)
                {
                    smallIcon = null;
                    return;
                }

                if (!OSUtils.IsWindows || !ScaleHelper.IsThreadPerMonitorAware)
                    return;

                // Fixing the small icon if the DPI of the form is different from the system DPI
                smallIcon = value.Resize(this.ScaleSize(IconsHelper.SmallIconReferenceSize));
                if (IsHandleCreated)
                    User32.SendMessage(Handle, Constants.WM_SETICON, Constants.ICON_SMALL, smallIcon.Handle);
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets a <see cref="System.Windows.Forms.ToolTip"/> instance that can be used to show tooltips for controls of this form.
        /// </summary>
        protected ToolTip ToolTip => BaseToolTip;

        /// <summary>
        /// Gets whether the form is in design mode. Unlike the <see cref="Component.DesignMode"/> property,
        /// this property works even during initialization.
        /// </summary>
        [Browsable(false)]
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Gets whether the form has already been loaded. This property is <see langword="true"/> after the <see cref="Form.Load"/> event is raised for the first time,
        /// and remains <see langword="true"/> even if the form is shown as a dialog multiple times or the handle is recreated (e.g. because <see cref="Control.RightToLeft"/> changes).
        /// Can be useful of we overload the <see cref="Form.OnLoad"/> method and want to avoid executing some initialization more than once.
        /// </summary>
        [Browsable(false)]
        protected bool IsLoaded => isLoaded;

        #endregion

        #endregion

        #region Constructors

        #region Static Constructors

#if !NET5_0_OR_GREATER
        static BaseForm()
        {
            if (!OSUtils.IsWindows || OSUtils.IsMono)
                return;

            // Not using Accessors because it's obtained only once.
            formStateRenderSizeGrip = Reflector.TryGetField(typeof(Form), "FormStateRenderSizeGrip", out object? value) && value is BitVector32.Section section ? section : default;
        }
#endif

        #endregion

        #region Instance Constructors

        /// <summary>
        /// Creates a new instance of <see cref="BaseForm"/>
        /// </summary>
        public BaseForm()
        {
            invoker = new InvokeMarshaller(this);
            StartPosition = FormStartPosition.CenterScreen; // kept for compatibility, CenterParent would actually be better
            BaseToolTip = new ToolTip
            {
                InitialDelay = 500,
                ReshowDelay = 100
            };

#if !NET35
            if (!OSUtils.IsWindows11OrLater)
#endif
            {
                BaseToolTip.AutoPopDelay = Int16.MaxValue;
            }

        }

        #endregion

        #endregion

        #region Public methods

        /// <summary>
        /// Shows the form as an MDI child of the specified caller form.
        /// </summary>
        /// <param name="child">The child to show</param>
        /// <param name="suspendCaller">When true, suspends the caller form (dialog effect).
        /// Because shown form is not a dialog form, execution of caller will not be suspended.
        /// If user needs to react of closing the child form, then either subscribe to caller's
        /// <see cref="CalledMdiChildClosed"/> event or override its <see cref="OnCalledMdiChildClosed"/> method.</param>
        public void ShowMdiChild(Form child, bool suspendCaller)
        {
            child.MdiParent = this;
            if (child is BaseForm baseChild)
            {
                baseChild.callerMdiForm = this;
                baseChild.resumeCaller = suspendCaller;
                if (suspendCaller)
                    Suspend();
            }
            Show();
        }

        /// <summary>
        /// Invalidates the MDI client area. Applicable only if the <see cref="Form.IsMdiContainer"/> is <see langword="true"/> for this form.
        /// </summary>
        public void InvalidateMdiClientArea()
        {
            GetMdiClient().Invalidate(false);
        }

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            deviceScale = this.GetScale();
            base.OnHandleCreated(e);
            ResetSmallIcon();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            bool loaded = isLoaded;
            isLoaded = true;
            base.OnLoad(e);

#pragma warning disable CS0618 // Type or member is obsolete
            if (!loaded)
                PerformTranslate(this);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (callerMdiForm != null)
            {
                callerMdiForm.OnCalledMdiChildClosed(this, e);

                if (callerMdiForm.suspended && resumeCaller)
                    callerMdiForm.Resume();

                callerMdiForm = null;
            }
        }

        /// <summary>
        /// Disposes the form and its resources.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                BaseToolTip.Dispose();
                commandBindings.Dispose();
                Events.Dispose();
                smallIcon?.Dispose();
            }

            mdiClient = null;
        }

        /// <summary>
        /// Translates controls and tooltips of given control.
        /// </summary>
        /// <param name="control"></param>
        [Obsolete("Translation does not works anymore, it just removes the possible postfixes.")]
        protected void PerformTranslate(Control control)
        {
            if (translateControls)
            {
                bool finished;
                if (LanguageWinForms.TranslateControl(control, out finished))
                    TranslateToolTip(control);
                if (finished)
                    return;

                if (control.HasChildren)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    foreach (Control c in control.Controls)
                        PerformTranslate(c!);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
        }

        /// <summary>
        /// Raises the <see cref="CalledMdiChildClosed"/> event.
        /// </summary>
        /// <param name="sender">The closed form, which is the sender of the provided arguments.</param>
        /// <param name="e">Arguments of the closed form.</param>
        protected virtual void OnCalledMdiChildClosed(BaseForm sender, FormClosedEventArgs e)
            => Events.GetHandler<FormClosedEventHandler>(nameof(CalledMdiChildClosed))?.Invoke(sender, e);

        /// <summary>
        /// Raises the <see cref="Suspended"/> event.
        /// </summary>
        protected virtual void OnSuspended(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Suspended))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="Resumed"/> event.
        /// </summary>
        protected virtual void OnResumed(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Resumed))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceScaleChanged"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDeviceScaleChanged(DeviceScaleChangedEventArgs e)
            => Events.GetHandler<EventHandler<DeviceScaleChangedEventArgs>>(nameof(DeviceScaleChanged))?.Invoke(this, e);

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
#if !NET5_0_OR_GREATER
                case Constants.WM_NCHITTEST when OSUtils.IsWindows && !OSUtils.IsMono:
                    WmNCHitTest(ref m);
                    return;
#endif

                case Constants.WM_DPICHANGED:
                    PointF oldScale = deviceScale;
                    var scale = new PointF(m.WParam.LOWORD() / ScaleHelper.DefaultDpi, m.WParam.HIWORD() / ScaleHelper.DefaultDpi);
                    deviceScale = scale;
                    var scaleChange = new PointF(scale.X / oldScale.X, scale.Y / oldScale.Y);
                    Rectangle suggestedBounds;
                    unsafe { suggestedBounds = ((RECT*)m.LParam)->ToRectangle(); }
                    Screen newScreen = Screen.FromRectangle(suggestedBounds);

                    // Refining the originally suggested bounds as it sometimes can be weird, e.g. can make the form larger and larger on each DPI change
                    // (e.g. when border style is FixedSingle). Also, suggesting a scaled size even if AutoScaleMode is None, which still can be ignored.
                    suggestedBounds = new Rectangle(suggestedBounds.Location, Size.Scale(scaleChange)).EnsureScreen(newScreen, false);
                    
                    base.WndProc(ref m);
                    ResetSmallIcon();
                    OnDeviceScaleChanged(new DeviceScaleChangedEventArgs(suggestedBounds, scale, oldScale, scaleChange));
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> on the thread that the control was created on.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// <para>This method is similar as using <see cref="Control.InvokeRequired"/> and <see cref="Control.Invoke(Delegate)"/> together,
        /// but it works even when the handle is not created yet, in which case <see cref="Control.InvokeRequired"/> returns <see langword="false"/>.</para>
        /// <para>The callback is invoked only if <see cref="Control.Disposing"/> and <see cref="Control.IsDisposed"/> properties return <see langword="false"/>.</para>
        /// </remarks>
        protected void InvokeOnUIThread(Action callback) => invoker.Invoke(callback);

        #endregion

        #region Private methods

        /// <summary>
        /// Suspends the current form instance.
        /// </summary>
        private void Suspend()
        {
            if (!suspended)
            {
                suspended = true;
                if (!IsMdiContainer)
                    Enabled = false;
                OnSuspended(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Resumes the current form instance.
        /// </summary>
        private void Resume()
        {
            if (suspended)
            {
                suspended = false;
                if (!IsMdiContainer)
                    Enabled = true;
                OnResumed(EventArgs.Empty);
            }
        }

        [Obsolete]
        private void TranslateToolTip(Control control)
        {
            if (BaseToolTip.GetToolTip(control)?.Length > 0)
                BaseToolTip.SetToolTip(control, Language.Translate(BaseToolTip.GetToolTip(control)));
        }

        private MdiClient GetMdiClient()
        {
            if (mdiClient != null && !mdiClient.IsDisposed)
                return mdiClient;

            if (!IsMdiContainer)
                throw new InvalidOperationException(Res.BaseFormNotMdiContainer);
            MdiClient? result = null;
            foreach (Control? child in Controls)
            {
                result = child as MdiClient;
                if (result != null)
                    break;
            }

            mdiClient = result ?? throw new InvalidOperationException(Res.BaseFormMdiClientNotFound);
            return result;
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Bugfix: When size grip is visible, and form is above and left of the primary monitor, form cannot be dragged anymore due to forced diagonal resizing.
        /// In .NET 5 I already fixed this in WinForms: https://github.com/dotnet/winforms/pull/2032
        /// </summary>
        private void WmNCHitTest(ref Message m)
        {
            if (this.FormState()[formStateRenderSizeGrip] != 0)
            {
                // Here is the bug in original code: LParam contains two shorts. Without the cast negative values are positive ints
                int x = m.LParam.SignedLOWORD();
                int y = m.LParam.SignedHIWORD();
                POINT pt = new POINT(x, y);
                User32.ScreenToClient(Handle, ref pt);
                Size clientSize = ClientSize;
                if (pt.x >= clientSize.Width - 16 && pt.y >= clientSize.Height - 16 && clientSize.Height >= 16)
                {
                    m.Result = IsMirrored ? (IntPtr)16 : (IntPtr)17;
                    return;
                }
            }

            DefWndProc(ref m);
            if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
            {
                nint result = m.Result;
                if (result >= 10 && result <= 17)
                    m.Result = (IntPtr)18;
            }
        }
#endif

        private void ResetSmallIcon()
        {
            if (smallIcon == null || !OSUtils.IsWindows || !ScaleHelper.IsThreadPerMonitorAware)
                return;

            smallIcon.Dispose();
            smallIcon = base.Icon?.Resize(this.ScaleSize(IconsHelper.SmallIconReferenceSize));
            if (smallIcon != null && IsHandleCreated)
                User32.SendMessage(Handle, Constants.WM_SETICON, Constants.ICON_SMALL, smallIcon.Handle);
        }

        #endregion
    }
}