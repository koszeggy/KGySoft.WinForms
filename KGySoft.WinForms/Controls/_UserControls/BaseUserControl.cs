#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseUserControl.cs
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
using System.Threading;
using System.Windows.Forms;

using KGySoft.ComponentModel;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class BaseUserControl : UserControl
    {
        #region Fields

        private readonly int threadId;
        private readonly ManualResetEventSlim handleCreated;

        #endregion

        #region Properties

        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        #endregion

        #region Constructors

        protected BaseUserControl()
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
            handleCreated = new ManualResetEventSlim();
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal void InvokeIfRequired(Action action)
        {
            if (Disposing || IsDisposed)
                return;

            try
            {
                // no invoke is required (not using InvokeRequired because that may return false if handle is not created yet)
                if (threadId == Thread.CurrentThread.ManagedThreadId)
                {
                    action.Invoke();
                    return;
                }

                if (!handleCreated.IsSet)
                    handleCreated.Wait();

                Invoke(action);
            }
            catch (ObjectDisposedException)
            {
                // it can happen that actual Invoke is started to execute only after querying isClosing and when Disposing and IsDisposed both return false
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsDesignMode)
                return;

            handleCreated.Set();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                handleCreated.Dispose();
                Events.Dispose();
            }
        }

        #endregion

        #endregion
    }
}