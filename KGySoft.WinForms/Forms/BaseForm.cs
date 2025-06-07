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
#if !NET5_0_OR_GREATER
using System.Drawing;
#endif
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Libraries.Language;
#if !NET5_0_OR_GREATER
using KGySoft.Reflection;
using KGySoft.WinForms.Reflection;
#endif
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// A base form with additional features and bug fixes.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseForm"/> class provides the following features and changes:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the form is disposed. To do that for the events of derived controls as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item><see cref="ToolTip"/> property to create tool tips for the controls on the form.</item>
    /// <item><see cref="CommandBindings"/> property. See the <a href="https://kgysoft.net/corelibraries#command-binding" target="_blank">online documentation</a> for details.</item>
    /// <item>Advanced MDI application support, see <see cref="ShowMdiChild"/> method and <see cref="CalledMdiChildClosed"/> and <see cref="PaintMdiClientArea"/> events.</item>
    /// <item>Fixes a <a href="https://github.com/dotnet/winforms/issues/1504" target="_blank">resizing bug</a> that exists in .NET Framework and .NET Core 3.x that can occur with multiple displays.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// </list>
    /// </remarks>
    public class BaseForm: Form
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
        private bool isTranslated;
        private bool suspended;
        private bool resumeCaller;
        private BaseForm? callerMdiForm;
        private MdiClient? mdiClient;

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
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

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
            this.Show();
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
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

#pragma warning disable CS0618 // Type or member is obsolete
            // translating only at first load (WinForms bug: despite the documentation, by ShowDialog Load occurs multiple times)
            if (!isTranslated)
                PerformTranslate(this);
            isTranslated = true;
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
        /// Triggers <see cref="CalledMdiChildClosed"/> event.
        /// </summary>
        /// <param name="sender">The sender closed form.</param>
        /// <param name="e">Arguments of closed form.</param>
        protected virtual void OnCalledMdiChildClosed(BaseForm sender, FormClosedEventArgs e)
            => Events.GetHandler<FormClosedEventHandler>(nameof(CalledMdiChildClosed))?.Invoke(this, e);

        /// <summary>
        /// Triggers <see cref="Suspended"/> event.
        /// </summary>
        protected virtual void OnSuspended(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Suspended))?.Invoke(this, e);

        /// <summary>
        /// Triggers <see cref="Resumed"/> event.
        /// </summary>
        protected virtual void OnResumed(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Resumed))?.Invoke(this, e);

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
                    this.Enabled = false;
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
                    this.Enabled = true;
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

        #endregion
    }
}