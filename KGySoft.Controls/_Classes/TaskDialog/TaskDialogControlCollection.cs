#region Used namespaces

using System;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace KGySoft.Controls
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
        /// <returns>A <see cref="TaskDialogControl"/> instance with the searched name or <see langword="null"/>&#160;if no control found with such name.</returns>
        public T this[string name]
        {
            get
            {
                return Items.FirstOrDefault(x => x.Name == name);
            }
        }

        #endregion

        #region Construction and Destruction

        #region Constructors

        internal TaskDialogControlCollection(TaskDialog parent)
        {
            this.parent = parent;
        }

        #endregion

        #region Explicit Disposing

        void IDisposable.Dispose()
        {
            foreach (T control in this)
            {
                control.Dispose();
            }

            base.ClearItems();
        }

        #endregion

        #endregion

        #region Methods

        protected override void InsertItem(int index, T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            parent.CheckCanChangeProperty();

            item.AssignParent(parent);
            base.InsertItem(index, item);

            if (parent.IsDialogShowing)
            {
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Insert, index);
            }
        }

        protected override void RemoveItem(int index)
        {
            parent.CheckCanChangeProperty();
            this[index].AssignParent(null);
            base.RemoveItem(index);

            if (parent.IsDialogShowing)
            {
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Remove, index);
            }
        }

        protected override void SetItem(int index, T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            parent.CheckCanChangeProperty();
            this[index].AssignParent(null);
            item.AssignParent(parent);

            base.SetItem(index, item);

            if (parent.IsDialogShowing)
            {
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Replace, index);
            }
        }

        protected override void ClearItems()
        {
            if (Count == 0)
            {
                return;
            }

            parent.CheckCanChangeProperty();
            foreach (T control in this)
            {
                control.AssignParent(null);
            }

            base.ClearItems();

            if (parent.IsDialogShowing)
            {
                parent.ControlCollectionChanged(this, TaskDialogControlCollectionChangeTypes.Clear, 0);
            }
        }

        #endregion
    }
}
