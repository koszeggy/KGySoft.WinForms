#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: InvokeMarshaller.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms
{
    internal sealed class InvokeMarshaller
    {
        #region Fields

        private readonly Control owner;
        private readonly int threadId;
        private readonly SynchronizationContext? synchronizationContext;

        #endregion

        #region Constructors

        internal InvokeMarshaller(Control owner)
        {
            this.owner = owner;
            threadId = ThreadHelper.ManagedThreadId;
            synchronizationContext = SynchronizationContext.Current;
        }

        #endregion

        #region Methods

        internal void Invoke(Action action)
        {
            if (owner.Disposing || owner.IsDisposed)
                return;

            try
            {
                // no invoke is required (not using owner.InvokeRequired because that may return false if handle is not created yet)
                if (threadId == ThreadHelper.ManagedThreadId)
                {
                    action.Invoke();
                    return;
                }

                // invoking from a foreign thread
                // NOTE: NOT using owner.Invoke, because in very extreme cases it may block the caller thread forever in a Wait call, never invoking the callback, while the UI remains responsive.
                //       Example: owner is a modal dialog, RTL changes, and during handle recreation a callback is requested.
                //if (owner.IsHandleCreated)
                //    owner.Invoke(action);
                //else 
                if (synchronizationContext != null)
                    synchronizationContext.Send(_ => action.Invoke(), null);
                else
                    throw new InvalidOperationException(Res.InvokeMarshallerNoSynchronizationContext);
            }
            catch (ObjectDisposedException)
            {
                // it can happen that both Disposing and IsDisposed returned false, but actual Invoke is started to execute only after disposing has started
            }
            catch (InvalidOperationException) when (!owner.IsHandleCreated || owner.IsDisposed)
            {
                // "Invoke or BeginInvoke cannot be called on a control until the window handle has been created."
                // Similar to the ObjectDisposedException catch, but in some cases even the Invoke call succeeds to marshal the delegate,
                // but by the time the actual execution starts, the control is already disposed.
                // NOTE: maybe this is not even relevant anymore, as not using owner.Invoke.
            }
        }

        #endregion
    }
}
