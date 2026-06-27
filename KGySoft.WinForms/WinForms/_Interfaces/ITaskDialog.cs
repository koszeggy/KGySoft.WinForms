#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ITaskDialog.cs
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
using System.Runtime.CompilerServices;

using KGySoft.WinForms.Components;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a task dialog implementation.
    /// </summary>
    internal interface ITaskDialog : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the state of the dialog. When it is <see cref="TaskDialogStatus.Initializing"/>, property changing is not allowed in host <see cref="TaskDialog"/>.
        /// Changing notifications will be forwarded to the implementation in <see cref="TaskDialogStatus.Showing"/> and <see cref="TaskDialogStatus.Closing"/> states.
        /// </summary>
        TaskDialogStatus ShowState { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the dialog (blocking call is expected).
        /// </summary>
        /// <param name="taskDialog">The host <see cref="TaskDialog"/> instance.</param>
        /// <param name="owner">Owner window handle (if any)</param>
        /// <param name="selectedButtonIndex">Zero based index of the custom button that closed the dialog, or -1 if the dialog was not closed by a custom button.</param>
        /// <param name="selectedRadioButtonIndex">Zero based index of the selected radio button, or -1 if there was no selected radio button.</param>
        /// <param name="checkBoxChecked">A value that indicated whether the verification checkbox was checked when the dialog was closed.</param>
        /// <returns>A <see cref="TaskDialogResult"/> value that identifies the standard button that caused the closing of the dialog. If <see cref="TaskDialogResult.Custom"/>, then refer <paramref name="selectedButtonIndex"/>.</returns>
        TaskDialogResult Execute(TaskDialog taskDialog, IntPtr owner, out int selectedButtonIndex, out int selectedRadioButtonIndex, out bool checkBoxChecked);

        /// <summary>
        /// Closes the dialog with the preferred result.
        /// </summary>
        void Close(TaskDialogResult result);

        /// <summary>
        /// Indicates that a <see cref="TaskDialog"/> property has been changed.
        /// </summary>
        void PropertyChanged([CallerMemberName]string propName = null!);

        /// <summary>
        /// Indicates that a <see cref="TaskDialogControl"/> property has been changed.
        /// </summary>
        void ControlPropertyChanged(TaskDialogControl control, string propName);

        /// <summary>
        /// Indicates that <see cref="TaskDialog.Buttons"/> collection has been changed.
        /// </summary>
        void CustomButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index);

        /// <summary>
        /// Indicates that <see cref="TaskDialog.RadioButtons"/> collection has been changed.
        /// </summary>
        void RadioButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index);

        /// <summary>
        /// Indicates that the timer should be started or stopped.
        /// </summary>
        void TimerChanged(bool enabled);

        #endregion
    }
}
