#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogButton.cs
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
using System.Diagnostics;
using System.Drawing;

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents a custom button in a <see cref="TaskDialog"/> that can be added to the <see cref="TaskDialog.Buttons"/> collection.
    /// The button can be either a regular push button or a command link, depending on the <see cref="TaskDialog.Options"/> property flags.
    /// </summary>
    /// <seealso cref="TaskDialogControlCollection{T}"/>
    /// <seealso cref="TaskDialogOptions"/>
    public sealed class TaskDialogButton : TaskDialogButtonBase
    {
        #region Constants

        #region Public Constants

        /// <summary>
        /// Gets the name of the <see cref="IsElevated"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyIsElevated = nameof(IsElevated);

        /// <summary>
        /// Gets the name of the <see cref="CustomIcon"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyCustomIcon = nameof(CustomIcon);

        /// <summary>
        /// Gets the name of the <see cref="IsDefault"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyIsDefault = nameof(IsDefault);

        #endregion

        #region Private Constants

        // See more flags in the base classes
        private const int isElevated = 1 << 16;
        private const int isDefault = isElevated << 1;

        #endregion
        
        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<HandledEventArgs>? click;
        private Icon? customIcon;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when this <see cref="TaskDialogButton"/> is clicked.
        /// If not subscribed, the <see cref="TaskDialog"/> will be closed when the button is clicked.
        /// Otherwise, the <see cref="HandledEventArgs.Handled"/> property can be set to <see langword="false"/> to let the invoker side close the <see cref="TaskDialog"/>.
        /// </summary>
        public event EventHandler<HandledEventArgs> Click
        {
            add
            {
                CheckDisposed();
                click += value;
            }
            remove => click -= value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether an elevated shield icon is displayed on the button.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        public bool IsElevated
        {
            get => flags[isElevated];
            set
            {
                if (flags[isElevated] == value)
                    return;

                CheckChangePropertyValue();
                flags[isElevated] = value;
                OnPropertyChanged(PropertyIsElevated);
            }
        }

        /// <summary>
        /// Gets ot sets a custom icon for this <see cref="TaskDialogButton"/>. Has effect only when <see cref="TaskDialog"/> is used in compatibility mode.
        /// If <see cref="IsElevated"/> is also set, the elevated icon is displayed.
        /// </summary>
        /// <seealso cref="TaskDialog.ForceCompatibilityMode"/>
        public Icon? CustomIcon
        {
            get => customIcon;
            set
            {
                if (customIcon == value)
                    return;

                CheckChangePropertyValue();
                customIcon = value;
                OnPropertyChanged(PropertyCustomIcon);
            }
        }

        /// <summary>
        /// Gets or sets whether this button is the default button.
        /// If there are more default buttons in a collection, the first one will be the default button when the dialog appears.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        public bool IsDefault
        {
            get => flags[isDefault];
            set
            {
                if (flags[isDefault] == value)
                    return;

                CheckChangePropertyValue();
                flags[isDefault] = value;
                OnPropertyChanged(PropertyIsDefault);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogButton"/> class.
        /// </summary>
        public TaskDialogButton()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogButton"/> class with the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogButton"/> with the specified name and text.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string name, string text)
            : base(name, text)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal void OnClick(HandledEventArgs e)
        {
            if (!Enabled)
            {
                e.Handled = true;
                return;
            }

            if (click is EventHandler<HandledEventArgs> handler)
                handler.Invoke(this, e);
            else
                e.Handled = false;
        }

        #endregion

        #region Private Protected Methods

        private protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // always clearing event subscriptions to prevent memory leaks
            click = null;

            if (disposing)
                customIcon = null;
        }

        #endregion

        #endregion
    }
}
