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
using System.Collections.Specialized;
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
    /// Represents a control hosted by a <see cref="TaskDialog"/> instance.
    /// </summary>
    public abstract class TaskDialogControl : IDisposable, INotifyPropertyChanged
    {
        #region Constants

        // There are further flags in the derived classes
        private const int isDisposed = 1;

        #endregion

        #region Fields

        #region Private Protected Fields

        private protected BitVector32 flags;

        #endregion
        
        #region Private Fields
        
        private PropertyChangedEventHandler? propertyChanged;
        private TaskDialog? parent;
        private string? name;
        private object? tag;

        #endregion
        
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
        /// Gets or sets a tag for this <see cref="TaskDialogControl"/>.
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

        #region Constructors

        private protected TaskDialogControl(string? name) => Name = name;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns the string representation of this control.
        /// </summary>
        /// <returns>The string representation of this control.</returns>
        public override string ToString() => !String.IsNullOrEmpty(Name) ? Name : base.ToString()!;

        /// <summary>
        /// Disposes this <see cref="TaskDialogControl"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        #region Private Protected Methods

        private protected void CheckDisposed()
        {
            if (flags[isDisposed])
                throw new ObjectDisposedException(ToString(), PublicResources.ObjectDisposed);
        }

        private protected void CheckChangePropertyValue()
        {
            CheckDisposed();
            parent?.CheckCanChangeProperty();
        }

        private protected void OnPropertyChanged(string propName)
        {
            Debug.Assert(!String.IsNullOrEmpty(propName), "Changed property name is empty");
            parent?.ControlPropertyChanged(this, propName);
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private protected virtual void Dispose(bool disposing)
        {
            if (flags[isDisposed])
                return;

            flags[isDisposed] = true;

            // always clearing event subscriptions to prevent memory leaks
            propertyChanged = null;

            // on explicit disposing nullifying other references
            if (disposing)
                parent = null;
        }

        #endregion

        #endregion
    }
}
