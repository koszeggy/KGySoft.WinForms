#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Dialogs.cs
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
using System.Globalization;
using System.Windows.Forms;

using KGySoft.Libraries.Language;

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Message dialogs
    /// </summary>
    public static class Dialogs
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether the <see cref="AdvancedMessageDialog"/> is used for showing messages.
        /// </summary>
        [Obsolete("AdvancedMessageDialog has been obsoleted, it's not recommended to use it anymore.")]
        public static bool UseAdvancedDialogs { get; set; }

        // TODO: remove this and apply overloads
        /// <summary>
        /// Gets or sets the owner of message dialogs
        /// </summary>
        public static IWin32Window DialogsOwner { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Information message
        /// </summary>
        /// <param name="msg">Message in invariant language (will be translated to <see cref="Libraries.Language.Language.ActiveLanguage"/>)</param>
        static public void InfoMessage(string msg)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(msg), Language.Translate("Information" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(msg), AdvancedDialogTypes.Information);
                }
            }
        }

        /// <summary>
        /// Information message
        /// </summary>
        /// <param name="msg">Message with placeholders in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="args">Arguments for placeholders</param>
        static public void InfoMessage(string msg, params object[] args)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(CultureInfo.CurrentCulture, msg, args), Language.Translate("Information" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), AdvancedDialogTypes.Information);
                }
            }
        }

        /// <summary>
        /// Error message
        /// </summary>
        /// <param name="msg">Message in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        static public void ErrorMessage(string msg)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(msg), Language.Translate("Error" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(msg), AdvancedDialogTypes.Error);
                }
            }
        }

        /// <summary>
        /// Error message
        /// </summary>
        /// <param name="msg">Message with placeholders in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="args">Arguments for placeholders</param>
        static public void ErrorMessage(string msg, params object[] args)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(CultureInfo.CurrentCulture, msg, args), Language.Translate("Error" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), AdvancedDialogTypes.Error);
                }
            }
        }

        /// <summary>
        /// Warning message
        /// </summary>
        /// <param name="msg">Message in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        static public void WarningMessage(string msg)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(msg), Language.Translate("Warning" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(msg), AdvancedDialogTypes.Warning);
                }
            }
        }

        /// <summary>
        /// Warning message
        /// </summary>
        /// <param name="msg">Message with placeholders in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="args">Arguments for placeholders</param>
        static public void WarningMessage(string msg, params object[] args)
        {
            if (!UseAdvancedDialogs)
                MessageBox.Show(DialogsOwner, Language.Translate(CultureInfo.CurrentCulture, msg, args), Language.Translate("Warning" + Language.DistinctionSeparator + "Dialogs"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), AdvancedDialogTypes.Warning);
                }
            }
        }

        /// <summary>
        /// Confirmation message with Yes/No[/Cancel] buttons and <see cref="DialogResult"/> return value.
        /// </summary>
        /// <param name="msg">Message in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="cancelButton">Yes if cancel button is needed</param>
        /// <returns>DialogResult</returns>
        static public DialogResult ConfirmMessage(bool cancelButton, string msg)
        {
            if (!UseAdvancedDialogs)
            {
                return MessageBox.Show(DialogsOwner, Language.Translate(msg), Language.Translate("Confirmation" + Language.DistinctionSeparator + "Dialogs"),
                    cancelButton ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    if (cancelButton)
                        return frm.Execute(Language.Translate(msg), String.Empty, Language.Translate("Confirmation" + Language.DistinctionSeparator + "Dialogs"), AdvancedDialogTypes.Confirmation, ButtonTypes.YesNoCancel, false, false, String.Empty);
                    else
                        return frm.Execute(Language.Translate(msg), AdvancedDialogTypes.Confirmation);
                }
            }
        }

        /// <summary>
        /// Confirmation message with Yes/No[/Cancel] buttons and <see cref="DialogResult"/> return value.
        /// </summary>
        /// <param name="msg">Message with placeholders in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <param name="cancelButton">Yes if cancel button is needed</param>
        /// <returns>DialogResult</returns>
        static public DialogResult ConfirmMessage(bool cancelButton, string msg, params object[] args)
        {
            if (!UseAdvancedDialogs)
            {
                return MessageBox.Show(DialogsOwner, Language.Translate(CultureInfo.CurrentCulture, msg, args), Language.Translate("Confirmation" + Language.DistinctionSeparator + "Dialogs"),
                    cancelButton ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    if (cancelButton)
                        return frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), String.Empty, Language.Translate("Confirmation" + Language.DistinctionSeparator + "Dialogs"), AdvancedDialogTypes.Confirmation, ButtonTypes.YesNoCancel, false, false, String.Empty);
                    else
                        return frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), AdvancedDialogTypes.Confirmation);
                }
            }
        }

        /// <summary>
        /// Confirmation message with Yes/No buttons and <see cref="bool"/> return value.
        /// </summary>
        /// <param name="msg">Message in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <returns>bool</returns>
        static public bool ConfirmMessage(string msg)
        {
            if (!UseAdvancedDialogs)
                return ConfirmMessage(false, msg) == DialogResult.Yes;
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    return frm.Execute(Language.Translate(msg), AdvancedDialogTypes.Confirmation) == DialogResult.Yes;
                }
            }
        }

        /// <summary>
        /// Confirmation message with Yes/No buttons and <see cref="bool"/> return value.
        /// </summary>
        /// <param name="msg">Message with placeholders in invariant language (will be translated to <see cref="Language.ActiveLanguage"/>)</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <returns>bool</returns>
        static public bool ConfirmMessage(string msg, params object[] args)
        {
            if (!UseAdvancedDialogs)
                return ConfirmMessage(false, msg, args) == DialogResult.Yes;
            else
            {
                using (AdvancedMessageDialog frm = new AdvancedMessageDialog())
                {
                    return frm.Execute(Language.Translate(CultureInfo.CurrentCulture, msg, args), AdvancedDialogTypes.Confirmation) == DialogResult.Yes;
                }
            }
        }

        /// <summary>
        /// Displays an input dialog.
        /// </summary>
        /// <param name="caption">Window caption</param>
        /// <param name="prompt">Text of input label</param>
        /// <param name="value">The value that initially may contain a default value</param>
        /// <param name="x">Horizontal position</param>
        /// <param name="y">Vertical position</param>
        /// <returns>Returns true if the OK button was pressed, otherwise, false.</returns>
        public static bool InputDialog(string caption, string prompt, ref string value, int x, int y)
        {
            return InputBox.Show(Language.Translate(caption), Language.Translate(prompt), ref value, x, y);
        }

        /// <summary>
        /// Displays an input dialog.
        /// </summary>
        /// <param name="caption">Window caption</param>
        /// <param name="prompt">Text of input label</param>
        /// <param name="value">The value that initially may contain a default value</param>
        /// <returns>Returns true if the OK button was pressed, otherwise, false.</returns>
        public static bool InputDialog(string caption, string prompt, ref string value)
        {
            return InputBox.Show(Language.Translate(caption), Language.Translate(prompt), ref value);
        }

        /// <summary>
        /// Displays an input dialog.
        /// </summary>
        /// <param name="prompt">Text of input label</param>
        /// <param name="value">The value that initially may contain a default value</param>
        /// <returns>Returns true if the OK button was pressed, otherwise, false.</returns>
        public static bool InputDialog(string prompt, ref string value)
        {
            return InputBox.Show(Application.ProductName, Language.Translate(prompt), ref value);
        }

        /// <summary>
        /// Displays an input dialog.
        /// </summary>
        /// <param name="value">The value that initially may contain a default value</param>
        /// <returns>Returns true if the OK button was pressed, otherwise, false.</returns>
        public static bool InputDialog(ref string value)
        {
            return InputBox.Show(Application.ProductName, Language.Translate("Value:"), ref value);
        }

        #endregion
    }
}
