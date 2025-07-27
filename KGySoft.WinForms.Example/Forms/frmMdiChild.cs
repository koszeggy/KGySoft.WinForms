#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: frmMdiChild.cs
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
using System.Drawing;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmMdiChild : BaseForm
    {
        #region Fields

        private readonly Action<string> writeLog = null!;
        private readonly Size referenceSize = new Size(275, 100);

        #endregion

        #region Constructors

        #region Public Constructors

        public frmMdiChild()
        {
            InitializeComponent();
            msMenu.Visible = false; // to show only the merged menu in the MDI parent
            InitCommandBindings();
        }

        #endregion

        #region Internal Constructors

        internal frmMdiChild(Action<string> writeLog) : this() => this.writeLog = writeLog;

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            // just to fix the initial size on older platforms if the current display scale is different from the default system scale
            base.OnLoad(e);
            ClientSize = referenceSize.Scale(DeviceScale);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            miChildMenu.Text = Text;
            ResetStatus();
        }

        protected override void OnOwnedMdiChildClosed(OwnedMdiChildClosedEventArgs e)
        {
            base.OnOwnedMdiChildClosed(e);
            writeLog($"{Text}: Owned child {e.MdiChild.Text} has been closed");
        }

        protected override void OnSuspended(EventArgs e)
        {
            base.OnSuspended(e);
            writeLog($"{Text} has been suspended by {SuspendingMdiChild!.Text}.");
            ResetStatus();
        }

        protected override void OnResumed(EventArgs e)
        {
            base.OnResumed(e);
            writeLog($"{Text} has been resumed.");
            ResetStatus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (miCloseOwnedChildrenWhenClosed.Checked)
                CloseChildren();
        }

        #endregion

        #region Private Methods

        private void InitCommandBindings()
        {
            CommandBindings.Add(OnOpenChildNormallyCommand).AddSource(miOpenChildNormally, nameof(miOpenChildNormally.Click));
            CommandBindings.Add(OnOpenChildAsDialog).AddSource(miOpenChildWithSuspendingParent, nameof(miOpenChildWithSuspendingParent.Click));
            CommandBindings.Add(CloseChildren).AddSource(miCloseOwnedChildrenNow, nameof(miCloseOwnedChildrenNow.Click));
            CommandBindings.Add(ResetStatus).AddSource(miCloseOwnedChildrenWhenClosed, nameof(miCloseOwnedChildrenWhenClosed.CheckedChanged));
        }

        private void ResetStatus()
        {
            lblStatus.Text = (IsSuspended
                    ? $"This child is suspended by {SuspendingMdiChild!.Text}."
                    : $"This child is active. Use the '{Text}' menu.")
                + $@"{Environment.NewLine}Close with children: {miCloseOwnedChildrenWhenClosed.Checked}";
            Icon = IsSuspended ? Icons.SystemSecurityWarning : Icons.SystemSecuritySuccess;
        }

        private void CloseChildren() => OwnedMdiChildren.ForEach(c => c.Close());

        #endregion

        #region Command Handlers

        private void OnOpenChildNormallyCommand()
        {
            var child = new frmMdiChild(writeLog) { Text = @"Normal Child " + (MdiParent!.MdiChildren.Length + 1) };
            ShowMdiChild(child);
            writeLog($"{child.Text} is opened normally by {Text}");
        }

        private void OnOpenChildAsDialog()
        {
            var child = new frmMdiChild(writeLog) { Text = @"Dialog Child " + (MdiParent!.MdiChildren.Length + 1) };
            ShowMdiChild(child, true);
            writeLog($"{child.Text} is opened by {Text} with suspending the caller");
        }

        #endregion

        #endregion
    }
}
