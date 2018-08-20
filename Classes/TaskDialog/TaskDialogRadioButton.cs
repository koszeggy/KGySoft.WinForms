#region Used namespaces

using System;
using System.Globalization;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a custom button in a <see cref="TaskDialog"/> that can be added to <see cref="TaskDialog.RadioButtons"/> collection.
    /// </summary>
    public sealed class TaskDialogRadioButton: TaskDialogButtonBase
    {
        #region Constants

        /// <summary>
        /// Gets the name of the <see cref="Checked"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyChecked = "Checked";

        #endregion

        #region Fields

        private bool isChecked;
        private EventHandler selected;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when <see cref="TaskDialogRadioButton"/> is selected. That is, when <see cref="Checked"/> becomes <see langword="true"/>.
        /// If you need a notification even if <see cref="Checked"/> bacames <see langword="false"/>, use <see cref="TaskDialogControl.PropertyChanged"/> event instead.
        /// </summary>
        public event EventHandler Selected
        {
            add
            {
                CheckDisposed();
                selected += value;
            }
            remove { selected -= value; }
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the radio button is checked.
        /// </summary>
        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value)
                    return;

                CheckChangePropertyValue();
                isChecked = value;
                if (isChecked)
                    OnSelected();
                OnPropertyChanged(PropertyChecked);
            }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Sets checked internally without raising events.
        /// Can be used when correcting multiple checked buttons in the same collection.
        /// Can be accessed even in initializing state.
        /// </summary>
        internal bool CheckedInternal
        {
            set { isChecked = value; }
        }

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of a task dialog radio button.
        /// </summary>
        public TaskDialogRadioButton()
        {
        }

        /// <summary>
        /// Creates a new instance of a task dialog radio button with
        /// the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        public TaskDialogRadioButton(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Creates a new instance of a task dialog radio button with
        /// the specified name and text.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="text">The text of the button.</param>
        public TaskDialogRadioButton(string name, string text)
            : base(name, text)
        {
        }

        #endregion

        #region Explicit Disposing

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            selected = null;
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns the string representation of this button.
        /// </summary>
        /// <returns>A <see cref="System.String"/>.</returns>
        public override string ToString()
        {
            return base.ToString() + ", Checked: " + this.Checked.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Private Methods
        
        private void OnSelected()
        {
            if (selected != null)
                selected.Invoke(this, EventArgs.Empty);
        }

        #endregion
        
        #endregion
    }
}
