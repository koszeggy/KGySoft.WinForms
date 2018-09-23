using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

using KGySoft.Controls.WinApi;
using KGySoft.Libraries.Language;
using KGySoft.Reflection;

namespace KGySoft.Controls
{
    // TODO: rename: AdvancedForm
    // TODO: Glass
    // TODO: enlist all of the fatures, including bugfixes
    /// <summary>
    /// A base form that provides tooltips for its controls and makes possible to
    /// translate form content. Supports showing forms as child forms of an MDI application.
    /// </summary>
    public partial class BaseForm: Form
    {
        #region Fields

        private bool translateControls;
        private bool suspended;
        private BaseForm callerMdiForm;
        private bool resumeCaller;
        private bool isTranslated;
        private MdiClient mdiClient;

        private static FieldAccessor fieldForm_formState;
        private static FieldAccessor fieldForm_FormStateRenderSizeGrip;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an MDI child whose caller was this form in a <see cref="ShowMdiChild"/> call is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI child whose caller was this form in a ShowMdiChild call is closed.")]
        public event FormClosedEventHandler CalledMdiChildClosed;

        /// <summary>
        /// Occurs when MDI area of the form has to be repainted. <see cref="Form.IsMdiContainer"/> must be true to access this event.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when MDI area of the form has to be repainted. IsMdiContainer must be true to access this event.")]
        public event PaintEventHandler PaintMdiClientArea
        {
            add
            {
                MdiClient mdiClient = GetMdiClient();
                mdiClient.Paint += value;
            }
            remove
            {
                MdiClient mdiClient = GetMdiClient();
                mdiClient.Paint -= value;
            }
        }

        /// <summary>
        /// Occurs when an MDI Child window called by <see cref="ShowMdiChild"/> suspends the caller instance.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI Child window called by ShowMdiChild suspends the caller instance.")]
        public event EventHandler Suspended;

        /// <summary>
        /// Occurs when the MDI Child window called by <see cref="ShowMdiChild"/> that suspended the caller instance is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when the MDI Child window called by ShowMdiChild that suspended the caller instance is closed.")]
        public event EventHandler Resumed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the form should translate its controls.
        /// </summary>
        [Category("BaseForm")]
        [DefaultValue(false),]
        [Description("Gets or sets whether the form should translate its controls.")]
        public bool TranslateControls
        {
            get { return translateControls; }
            set { translateControls = value; }
        }

        /// <summary>
        /// Gets or sets the main form in an MDI application.
        /// </summary>
        public static BaseForm MainMdiParent { get; set; }

        /// <summary>
        /// Gets whether the form is suspended by a called MDI child.
        /// </summary>
        public bool IsSuspended
        {
            get { return suspended; }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new instance of <see cref="BaseForm"/>
        /// </summary>
        public BaseForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Shows the form as an MDI child of the specified caller form.
        /// </summary>
        /// <param name="caller">Caller form</param>
        /// <param name="suspendCaller">When true, suspends the caller form (dialog effect).
        /// Because shown form is not a dialog form, execution of caller will not be suspended.
        /// If user needs to react of closing the child form, then either subscribe to caller's
        /// <see cref="CalledMdiChildClosed"/> event or override its <see cref="OnCalledMdiChildClosed"/> method.</param>
        public void ShowMdiChild(BaseForm caller, bool suspendCaller)
        {
            if (MainMdiParent == null && caller != null && caller.IsMdiContainer)
                MainMdiParent = caller;
            if (MainMdiParent == null)
                throw new InvalidOperationException("BaseForm.MainMdiParent property is not set.");
            this.MdiParent = MainMdiParent;
            callerMdiForm = caller;
            resumeCaller = suspendCaller;
            if (suspendCaller && caller != null)
                caller.Suspend();
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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            mdiClient = null;
        }

        /// <summary>
        /// Translates controls and tooltips of given control.
        /// </summary>
        /// <param name="control"></param>
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
                    foreach (Control c in control.Controls)
                        PerformTranslate(c);
            }
        }

        /// <summary>
        /// Triggers <see cref="CalledMdiChildClosed"/> event.
        /// </summary>
        /// <param name="sender">The sender closed form.</param>
        /// <param name="e">Arguments of closed form.</param>
        protected virtual void OnCalledMdiChildClosed(BaseForm sender, FormClosedEventArgs e)
        {
            if (CalledMdiChildClosed != null)
                CalledMdiChildClosed(sender, e);
        }

        /// <summary>
        /// Triggers <see cref="Suspended"/> event.
        /// </summary>
        protected virtual void OnSuspended(EventArgs e)
        {
            if (Suspended != null)
                Suspended.Invoke(this, e);
        }

        /// <summary>
        /// Triggers <see cref="Resumed"/> event.
        /// </summary>
        protected virtual void OnResumed(EventArgs e)
        {
            if (Resumed != null)
                Resumed.Invoke(this, e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_NCHITTEST:
                    WmNCHitTest(ref m);
                    return;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

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

        private void TranslateToolTip(Control control)
        {
            if (BaseToolTip.CanExtend(control) && BaseToolTip.GetToolTip(control).Length > 0)
                BaseToolTip.SetToolTip(control, Language.Translate(BaseToolTip.GetToolTip(control)));
        }

        private MdiClient GetMdiClient()
        {
            if (mdiClient != null && !mdiClient.IsDisposed)
                return mdiClient;

            if (!IsMdiContainer)
                throw new InvalidOperationException("Form must be an MDI container. Set IsMdiContainer before accessing this member!");
            MdiClient result = null;
            foreach (Control child in Controls)
            {
                result = child as MdiClient;
                if (result != null)
                    break;
            }
            if (result == null)
                throw new InvalidOperationException("MDI Client area not found");

            mdiClient = result;
            return result;
        }

        /// <summary>
        /// Bugfix: When size grip is visible, and form is above and left of the primary monitor, form cannot be dragged anymore due to forced diagonal resizing.
        /// </summary>
        private void WmNCHitTest(ref Message m)
        {
            if (IsGripVisible())
            {
                // Here is the bug in original code: LParam contains two shorts. Without the cast negative values are positive ints
                int x = (short)(m.LParam.ToInt32() & 0xffff);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xffff);
                POINT pt = new POINT(x, y);
                User32.ScreenToClient(Handle, ref pt);
                Size clientSize = ClientSize;
                if (((pt.x >= (clientSize.Width - 16)) && (pt.y >= (clientSize.Height - 16))) && (clientSize.Height >= 16))
                {
                    m.Result = IsMirrored ? ((IntPtr)16) : ((IntPtr)17);
                    return;
                }
            }

            DefWndProc(ref m);
            if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
            {
                int result = (int)m.Result;
                if ((result >= 10) && (result <= 17))
                {
                    m.Result = (IntPtr)18;
                }
            }
        }

        private bool IsGripVisible()
        {
            if (fieldForm_formState == null)
                fieldForm_formState = FieldAccessor.GetFieldAccessor(typeof(Form).GetField("formState", BindingFlags.Instance | BindingFlags.NonPublic));
            if (fieldForm_FormStateRenderSizeGrip == null)
                fieldForm_FormStateRenderSizeGrip = FieldAccessor.GetFieldAccessor(typeof(Form).GetField("FormStateRenderSizeGrip", BindingFlags.Static | BindingFlags.NonPublic));

            return ((BitVector32)fieldForm_formState.Get(this))[(BitVector32.Section)fieldForm_FormStateRenderSizeGrip.Get(null)] != 0;
        }

        #endregion

        #region Handled events

        private void BaseForm_Load(object sender, EventArgs e)
        {
            // translating only at first load (WinForms bug: despite of documentation, by ShowDialog Load occurs multiple times)
            if (!isTranslated)
                PerformTranslate(this);
            isTranslated = true;
        }

        void BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (callerMdiForm != null)
            {
                callerMdiForm.OnCalledMdiChildClosed(this, e);

                if (callerMdiForm.suspended && resumeCaller)
                    callerMdiForm.Resume();

                callerMdiForm = null;
            }
        }

        #endregion
    }
}