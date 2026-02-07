#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogButtonBase.cs
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

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Base class of task dialog buttons property.
    /// </summary>
    public abstract class TaskDialogButtonBase: TaskDialogControl
    {
        #region Constants

        /// <summary>
        /// Gets the name of the <see cref="Text"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyText = "Text";

        /// <summary>
        /// Gets the name of the <see cref="Description"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyDescription = "Description";

        /// <summary>
        /// Gets the name of the <see cref="Enabled"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        public const string PropertyEnabled = "Enabled";

        #endregion

        #region Fields

        private string? text;
        private bool enabled = true;
        private string? description;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        public string? Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;

                CheckChangePropertyValue();
                text = value;
                OnPropertyChanged(PropertyText);
            }
        }

        /// <summary>
        /// Gets or sets description of the button. If the button is displayed as a command link, description is displayed
        /// under the main text. Otherwise, the description might be displayed as a tooltip (only when <see cref="TaskDialog"/> is used in compatibility mode).
        /// </summary>
        /// <seealso cref="TaskDialog.ForceCompatibilityMode"/>
        public string? Description
        {
            get => description;
            set
            {
                if (description == value)
                    return;

                CheckChangePropertyValue();
                description = value;
                OnPropertyChanged(PropertyDescription);
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether the button is enabled.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;

                CheckChangePropertyValue();
                enabled = value;
                OnPropertyChanged(PropertyEnabled);
            }
        }

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of a task dialog button.
        /// </summary>
        protected TaskDialogButtonBase()
        {
        }

        /// <summary>
        /// Creates a new instance of a task dialog button with
        /// the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        protected TaskDialogButtonBase(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Creates a new instance of a task dialog button with
        /// the specified name and text.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="text">The text of the button.</param>
        protected TaskDialogButtonBase(string name, string text)
            : base(name)
        {
            this.text = text;
        }

        #endregion

        #region Explicit Disposing

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // on explicit disposing nullifying other references (except text because that is displayed in ToString)
            if (disposing)
            {
                description = null;
            }
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
            return base.ToString() + " {" + (text ?? String.Empty) + "}";
        }

        #endregion

        #endregion
    }
}
