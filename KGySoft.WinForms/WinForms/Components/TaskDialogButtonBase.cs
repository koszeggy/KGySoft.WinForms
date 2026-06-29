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
using System.ComponentModel;

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents the base class for <see cref="TaskDialog"/> buttons.
    /// </summary>
    public abstract class TaskDialogButtonBase : TaskDialogControl
    {
        #region Constants

        #region Public Constants

        /// <summary>
        /// Gets the name of the <see cref="Text"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyText = nameof(Text);

        /// <summary>
        /// Gets the name of the <see cref="Description"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyDescription = nameof(Description);

        /// <summary>
        /// Gets the name of the <see cref="Enabled"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyEnabled = nameof(Enabled);

        #endregion
        
        #region Private Constants

        // See more flags in the base and derived classes
        private const int isEnabled = 1 << 8;
        
        #endregion

        #endregion

        #region Fields

        private string? text;
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
        /// Gets or sets the description of the button. If the button is displayed as a command link, description is displayed
        /// under the main text. Otherwise, the description can be displayed as a tooltip (when the <see cref="TaskDialog"/> is used in compatibility mode).
        /// </summary>
        /// <remarks>
        /// <para>You can set the <see cref="TaskDialog.ForceCompatibilityMode"/> property to <see langword="true"/> to make sure that the description
        /// is displayed as a tooltip when the button is not displayed as command link.</para>
        /// </remarks>
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
        /// Gets or sets whether the button is enabled.
        /// </summary>
        public bool Enabled
        {
            get => flags[isEnabled];
            set
            {
                if (flags[isEnabled] == value)
                    return;

                CheckChangePropertyValue();
                flags[isEnabled] = value;
                OnPropertyChanged(PropertyEnabled);
            }
        }

        #endregion

        #region Constructors

        private protected TaskDialogButtonBase()
            : this(null, null)
        {
        }

        private protected TaskDialogButtonBase(string? text)
            : this(null, text)
        {
        }

        private protected TaskDialogButtonBase(string? name, string? text)
            : base(name)
        {
            this.text = text;
            flags[isEnabled] = true;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns the string representation of this button.
        /// </summary>
        /// <returns>The string representation of this button.</returns>
        public override string ToString() => $"{base.ToString()} {{{(text ?? String.Empty)}}}";

        #endregion

        #region Private Protected Methods

        private protected override void Dispose(bool disposing)
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
    }
}
