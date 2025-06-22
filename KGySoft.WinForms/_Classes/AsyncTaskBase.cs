#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AsyncTaskBase.cs
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
using System.Threading;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a cancellable and completable asynchronous task that can be used even in frameworks that do not support
    /// the <see langword="async"/> and <see langword="await"/> keywords.
    /// </summary>
    public abstract class AsyncTaskBase : IDisposable
    {
        #region Fields

        private readonly ManualResetEventSlim completedEvent = new();
        
        private volatile bool isCanceled;
        private volatile bool isDisposed;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets whether the task has been canceled.
        /// </summary>
        public bool IsCanceled => isCanceled;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets whether the task has been disposed.
        /// </summary>
        protected bool IsDisposed => isDisposed;

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the task as completed, allowing any waiting threads to proceed.
        /// </summary>
        public virtual void SetCompleted() => completedEvent.Set();

        /// <summary>
        /// Cancels the task, setting the <see cref="IsCanceled"/> property to <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <note type="caller">Note that this method does not complete the task, it only sets the cancellation state.
        /// An override implementation may call the <see cref="SetCompleted">SetCompleted</see> method though, when the task is actually completed.</note>
        /// </remarks>
        public virtual void Cancel() => isCanceled = true;

        /// <summary>
        /// Waits for the task to complete. This method blocks the calling thread until the task is completed.
        /// </summary>
        public void WaitForCompletion()
        {
            if (IsDisposed)
                return;

            try
            {
                completedEvent.Wait();
            }
            catch (ObjectDisposedException)
            {
                // it can happen that the task has just been completed after querying IsCompleted but this part
                // must not be in a lock because that may cause deadlocks
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Disposes the resources used by the <see cref="AsyncTaskBase"/> instance.
        /// </summary>
        /// <param name="disposing">If <see langword="true"/>, the method has been called directly or indirectly by a user's code. If <see langword="false"/>, the method has been called by the runtime from inside the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                completedEvent.Set();
                completedEvent.Dispose();
            }

            isDisposed = true;
        }

        #endregion

        #endregion
    }
}
