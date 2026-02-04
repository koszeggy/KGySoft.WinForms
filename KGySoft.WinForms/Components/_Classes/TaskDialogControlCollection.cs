#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogControlCollection.cs
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
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents a collection of <see cref="TaskDialogControl"/> instances.
    /// </summary>
    public sealed class TaskDialogControlCollection<T>: Collection<T>, IDisposable
        where T: TaskDialogControl
    {
        #region Fields

        private readonly TaskDialog parent;

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an item of the <see cref="TaskDialogControlCollection{T}"/> by name.
        /// </summary>
        /// <param name="name">Name of the control</param>
        /// <returns>A <see cref="TaskDialogControl"/> instance with the searched name or <see langword="null"/> if no control found with such name.</returns>
        public T? this[string name] => Items.FirstOrDefault(x => x.Name == name);

        #endregion

        #region Constructors

        internal TaskDialogControlCollection(TaskDialog parent) => this.parent = parent;

        #endregion

        #region Methods

        #region Protected Methods

        /// <summary>
        /// Inserts a <see cref="TaskDialogControl"/> into the <see cref="TaskDialogControlCollection{T}" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be <see langword="null" /> for reference types.</param>
        protected override void InsertItem(int index, T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), PublicResources.ArgumentNull);
            }

            parent.CheckCanChangeProperty();

            item.AssignParent(parent);
            base.InsertItem(index, item);

            if (parent.IsDialogShowing)
            {
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Insert, index);
            }
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="TaskDialogControlCollection{T}" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            parent.CheckCanChangeProperty();
            this[index].AssignParent(null);
            base.RemoveItem(index);

            if (parent.IsDialogShowing)
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Remove, index);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be <see langword="null" /> for reference types.</param>
        protected override void SetItem(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), PublicResources.ArgumentNull);

            parent.CheckCanChangeProperty();
            this[index].AssignParent(null);
            item.AssignParent(parent);

            base.SetItem(index, item);

            if (parent.IsDialogShowing)
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Replace, index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="TaskDialogControlCollection{T}" />.
        /// </summary>
        protected override void ClearItems()
        {
            if (Count == 0)
                return;

            parent.CheckCanChangeProperty();
            foreach (T control in this)
                control.AssignParent(null);

            base.ClearItems();
            if (parent.IsDialogShowing)
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Clear, 0);
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IDisposable.Dispose()
        {
            foreach (T control in this)
                control.Dispose();

            base.ClearItems();
        }

        #endregion

        #endregion
    }
}
