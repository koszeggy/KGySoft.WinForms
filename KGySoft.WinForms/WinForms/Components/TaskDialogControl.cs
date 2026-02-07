#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogControl.cs
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
using System.ComponentModel;

#endregion

#region Suppressions

#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable CS8603 // Possible null reference return - false alarm for older frameworks
#endif

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents a dialog control hosted by a <see cref="TaskDialog"/> instance.
    /// </summary>
    public abstract class TaskDialogControl: IDisposable, INotifyPropertyChanged
    {
        #region Fields

        private bool disposed;
        private PropertyChangedEventHandler? propertyChanged;
        private TaskDialog? parent;
        private string? name;
        private object? tag;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                CheckDisposed();
                propertyChanged += value;
            }

            remove => propertyChanged -= value;
        }

        #endregion

        #region Properties

        #region Public Properties
        
        /// <summary>
        /// Gets the parent dialog that is hosting the control.
        /// </summary>
        public TaskDialog? Parent => parent;

        /// <summary>
        /// Gets or sets the name of this control. Name is not required to be set, but it can be used
        /// to identify controls in a <see cref="TaskDialogControlCollection{T}"/> by name.
        /// This property can be changed without restriction.
        /// </summary>
        public string? Name
        {
            get => name;
            set
            {
                CheckDisposed();
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets a tag to the <see cref="TaskDialogControl"/>.
        /// A tag can be any object for custom purposes.
        /// </summary>
        public object? Tag
        {
            get => tag;
            set
            {
                CheckDisposed();
                tag = value;
            }
        }

        #endregion

        #region Internal Properties

        internal int Id { get; set; }

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of a dialog control without name
        /// </summary>
        protected TaskDialogControl()
        {
        }

        /// <summary>
        /// Creates a new instance of a dialog control with the specified name.
        /// </summary>
        /// <param name="name">The name of the control.</param>
        protected TaskDialogControl(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Explicit Disposing

        /// <summary>
        /// Disposes the <see cref="TaskDialogControl"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources of the current <see cref="TaskDialogControl"/> instance.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            // always clearing event subscriptions to prevent memory leaks
            propertyChanged = null;

            // on explicit disposing nullifying other references
            if (disposing)
            {
                parent = null;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString() => !String.IsNullOrEmpty(Name) ? Name : base.ToString()!;

        #endregion

        #region Internal Methods

        internal void AssignParent(TaskDialog? parentDialog)
        {
            if (parentDialog != null)
            {
                CheckDisposed();
                if (parent != null)
                    throw new InvalidOperationException(Res.TaskDialogHasParent(ToString()));
            }

            parent = parentDialog;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Checks whether the current <see cref="TaskDialogControl"/> instance is disposed and if so, throws an <see cref="ObjectDisposedException"/>.
        /// </summary>
        protected void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(ToString(), PublicResources.ObjectDisposed);
        }

        ///<summary>
        /// Checks whether property changing is allowed.
        /// If not, throws a <see cref="NotSupportedException"/>.
        /// </summary>
        protected void CheckChangePropertyValue()
        {
            CheckDisposed();
            parent?.CheckCanChangeProperty();
        }

        ///<summary>
        /// Invokes refreshing property in host as well as <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propName">The name of the property that is changing.</param>
        protected void OnPropertyChanged(string propName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propName), "Changed property name is empty");
            parent?.ControlPropertyChanged(this, propName);
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #endregion
    }
}
