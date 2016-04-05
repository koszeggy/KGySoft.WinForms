#region Used namespaces

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a custom button in a <see cref="TaskDialog"/> that can be added to <see cref="TaskDialog.Buttons"/> collection.
    /// The button can be either a reguler push button or a link command depending on <see cref="TaskDialog.Options"/> property flags.
    /// </summary>
    /// <seealso cref="TaskDialogControlCollection{T}"/>
    /// <seealso cref="TaskDialogOptions"/>
    public sealed class TaskDialogButton: TaskDialogButtonBase
    {
        #region Constants

        /// <summary>
        /// Gets the name of the <see cref="IsElevated"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyIsElevated = "IsElevated";

        /// <summary>
        /// Gets the name of the <see cref="CustomIcon"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyCustomIcon = "CustomIcon";

        /// <summary>
        /// Gets the name of the <see cref="IsDefault"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyIsDefault = "IsDefault";

        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<HandledEventArgs> click;
        private bool isElevated;
        private Icon customIcon;
        private bool isDefault;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the task dialog button is clicked.
        /// If not subscribed, <see cref="TaskDialog"/> will be closed when the button is clicked.
        /// Otherwise, <see cref="HandledEventArgs.Handled"/> property should be set to <c>false</c> to let the system close the window.
        /// </summary>
        public event EventHandler<HandledEventArgs> Click
        {
            add
            {
                CheckDisposed();
                click += value;
            }
            remove { click -= value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether an elevation icon is displayed on the button.
        /// </summary>
        public bool IsElevated
        {
            get { return isElevated; }
            set
            {
                if (isElevated == value)
                    return;

                CheckChangePropertyValue();
                isElevated = value;
                OnPropertyChanged(PropertyIsElevated);
            }
        }

        /// <summary>
        /// Gets ot sets a custom icon of the button. Has effect only when <see cref="TaskDialog"/> is used in compatibility mode.
        /// If <see cref="IsElevated"/> is also set, the elevated icon is displayed.
        /// </summary>
        /// <seealso cref="TaskDialog.ForceCompatibilityMode"/>
        public Icon CustomIcon
        {
            get { return customIcon; }
            set
            {
                if (customIcon == value)
                    return;

                CheckChangePropertyValue();
                TaskDialog.ReplaceIcon(ref customIcon, value, 16);
                OnPropertyChanged(PropertyCustomIcon);
            }
        }

        /// <summary>
        /// Gets or sets whether this button is the default button.
        /// If there are more default buttons in a collection first one will be the default one when the dialog appears.
        /// </summary>
        public bool IsDefault
        {
            get { return isDefault; }
            set
            {
                if (isDefault == value)
                    return;

                CheckChangePropertyValue();
                isDefault = value;
                OnPropertyChanged(PropertyIsDefault);
            }
        }

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of a task dialog button.
        /// </summary>
        public TaskDialogButton()
        {
        }

        /// <summary>
        /// Creates a new instance of a task dialog button with
        /// the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Creates a new instance of a task dialog button with
        /// the specified name and text.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string name, string text)
            : base(name, text)
        {
        }

        #endregion

        #region Explicit Disposing

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // always clearing event subscriptions to prevent memory leaks
            click = null;

            if (disposing)
            {
                if (customIcon != null)
                {
                    customIcon.Dispose();
                }

                customIcon = null;
            }
        }

        #endregion

        #endregion

        #region Methods

        internal void OnClick(HandledEventArgs e)
        {
            if (!Enabled)
            {
                e.Handled = true;
                return;
            }

            if (click != null)
            {
                click.Invoke(this, e);
            }
            else
            {
                e.Handled = false;
            }
        }

        #endregion
    }
}
