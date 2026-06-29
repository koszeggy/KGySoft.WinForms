#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogRadioButton.cs
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
    /// Represents a radio button in a <see cref="TaskDialog"/> that can be added to the <see cref="TaskDialog.RadioButtons"/> collection.
    /// </summary>
    public sealed class TaskDialogRadioButton : TaskDialogButtonBase
    {
        #region Constants

        #region Public Constants

        /// <summary>
        /// Gets the name of the <see cref="Checked"/> property.
        /// Can be used to identify the property in <see cref="TaskDialogControl.PropertyChanged"/> event.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Since we have nameof(), this is not really needed anymore
        public const string PropertyChecked = nameof(Checked);
        
        #endregion

        #region Private Constants

        // See more flags in the base classes
        private const int isChecked = 1 << 16;

        #endregion
        
        #endregion

        #region Fields

        private EventHandler? selected;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when this <see cref="TaskDialogRadioButton"/> is selected. That is, when <see cref="Checked"/> becomes <see langword="true"/>.
        /// If you need a notification even when <see cref="Checked"/> bacomes <see langword="false"/>, use the <see cref="TaskDialogControl.PropertyChanged"/> event instead.
        /// </summary>
        public event EventHandler Selected
        {
            add
            {
                CheckDisposed();
                selected += value;
            }
            remove => selected -= value;
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the radio button is checked.
        /// </summary>
        public bool Checked
        {
            get => flags[isChecked];
            set
            {
                if (flags[isChecked] == value)
                    return;

                CheckChangePropertyValue();
                flags[isChecked] = value;
                if (value)
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
            set => flags[isChecked] = value;
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogRadioButton"/> class.
        /// </summary>
        public TaskDialogRadioButton()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogRadioButton"/> class with the specified text.
        /// </summary>
        /// <param name="text">The text of the radio button.</param>
        public TaskDialogRadioButton(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TaskDialogRadioButton"/> class with the specified name and text.
        /// </summary>
        /// <param name="name">The name of the radio button.</param>
        /// <param name="text">The text of the radio button.</param>
        public TaskDialogRadioButton(string name, string text)
            : base(name, text)
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns the string representation of this <see cref="TaskDialogRadioButton"/>.
        /// </summary>
        /// <returns>The string representation of this radio button.</returns>
        public override string ToString() => $"{base.ToString()}, Checked: {Checked}";

        #endregion

        #region Private Protected Methods

        private protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            selected = null;
        }

        #endregion

        #region Private Methods

        private void OnSelected() => selected?.Invoke(this, EventArgs.Empty);

        #endregion
        
        #endregion
    }
}
