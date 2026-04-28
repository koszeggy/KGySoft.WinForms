#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Dialogs.cs
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

using System.Diagnostics.CodeAnalysis;
using System.Drawing;

#region Used Namespaces

using System;
using System.Globalization;
using System.Windows.Forms;

using KGySoft.Libraries.Language;
using KGySoft.Resources;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

#endregion

#region Used Aliases

using TaskDialog = KGySoft.WinForms.Components.TaskDialog;

#endregion

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Provides static methods for showing common dialogs.
    /// </summary>
    /// <remarks>
    /// <note type="tip">If your application uses per-monitor DPI awareness, it is recommended to set the <see cref="UseTaskDialogs"/> property
    /// to <see langword="true"/> in the startup code of your application.</note>
    /// </remarks>
    public static class Dialogs
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether an <see cref="AdvancedMessageDialog"/> instance is used for message dialogs.
        /// This option does not support right-to-left layout, default button selection, and high DPI scaling.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        [Obsolete("AdvancedMessageDialog has been obsoleted, it is recommended to use UseTaskDialogs instead.")]
        public static bool UseAdvancedDialogs { get; set; }

        /// <summary>
        /// Gets or sets whether a <see cref="TaskDialog"/> instance should be used when calling the message dialog methods of this class.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <para>The default value is <see langword="false"/> to maintain backward compatibility with previous versions,
        /// but it is recommended to set this property to <see langword="true"/> in new applications, especially if your application uses per-monitor DPI awareness,
        /// or you want to be able to localize the standard button texts of the dialogs.</para>
        /// <para>It is safe to set this property to <see langword="true"/> even if visual styles are not enabled or the application is not running on Windows Vista or later.</para>
        /// </remarks>
        public static bool UseTaskDialogs { get; set; }

        /// <summary>
        /// Gets or sets a common owner for the message dialogs when the owner is not specified in the dialog showing methods.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public static IWin32Window? DialogsOwner { get; set; }

        /// <summary>
        /// Gets whether right-to-left layout is automatically applied to the dialogs when the current UI culture is right-to-left.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        public static bool AutoRightToLeftLayout { get; set; }

        #endregion

        #region Methods

        #region Public Methods

        #region Information

        /// <summary>
        /// Shows an information message dialog with an OK button.
        /// </summary>
        /// <param name="message">The message to display in the information dialog.</param>
        /// <remarks>
        /// <note>In versions prior to 5.0.0, the <paramref name="message"/> was translated by the obsolete <see cref="Language"/> class.
        /// Since version 5.0.0, the <paramref name="message"/> is expected to be already localized. To use the same dynamic localization
        /// as <see cref="BaseForm"/> or <see cref="BaseUserControl"/> when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/> or <see cref="DynamicStringLocalization.LocalScope"/>, you can use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method.
        /// To localize the window caption, you can use the <see cref="InfoMessage(IWin32Window,string,string)"/> overload and pass a custom localized string to the <c>caption</c> parameter.
        /// To localize both the window caption and buttons, opt-in to use task dialogs by setting the <see cref="UseTaskDialogs"/> property to <see langword="true"/>,
        /// and set the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// property to <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Resources_ResourceManagerSources.htm">CompiledAndResX</a> in the startup code of your application, and translate the
        /// auto-generated <c>KGySoft.WinForms.Messages.&lt;LanguageId&gt;.resx</c> files in the <c>Resources</c> folder of the executable application.</note>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// </remarks>
        public static void InfoMessage(string message) => InfoMessage(null, message);

        /// <summary>
        /// Shows an information message dialog with an OK button.
        /// </summary>
        /// <param name="message">Message with placeholders in invariant language.</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <remarks>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix,
        /// and simply formats it with the <paramref name="args"/> parameters. See the <strong>Remarks</strong> section of the <see cref="InfoMessage(string)"/> overload for more details.</note>
        /// </remarks>
        [Obsolete("Use InfoMessage(string) with an already localized message instead.")]
        public static void InfoMessage(string message, params object[] args) => InfoMessage(Language.Translate(CultureInfo.CurrentCulture, message, args));

        /// <summary>
        /// Shows an information message dialog with an OK button.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InfoMessage(string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal message dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="message">The message to display in the information dialog.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, a localized string similar to <c>Information</c> will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void InfoMessage(IWin32Window? owner, string message, string? caption = null)
            => ShowMessage(owner, message, caption ?? Res.DialogsInfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

        #endregion

        #region Error

        /// <summary>
        /// Shows an error message dialog with an OK button.
        /// </summary>
        /// <param name="message">The message to display in the error dialog.</param>
        /// <remarks>
        /// <note>In versions prior to 5.0.0, the <paramref name="message"/> was translated by the obsolete <see cref="Language"/> class.
        /// Since version 5.0.0, the <paramref name="message"/> is expected to be already localized. To use the same dynamic localization
        /// as <see cref="BaseForm"/> or <see cref="BaseUserControl"/> when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/> or <see cref="DynamicStringLocalization.LocalScope"/>, you can use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method.
        /// To localize the window caption, you can use the <see cref="ErrorMessage(IWin32Window,string,string)"/> overload and pass a custom localized string to the <c>caption</c> parameter.
        /// To localize both the window caption and buttons, opt-in to use task dialogs by setting the <see cref="UseTaskDialogs"/> property to <see langword="true"/>,
        /// and set the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// auto-generated <c>KGySoft.WinForms.Messages.&lt;LanguageId&gt;.resx</c> files in the <c>Resources</c> folder of the executable application.</note>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// </remarks>
        public static void ErrorMessage(string message) => ErrorMessage(null, message);

        /// <summary>
        /// Shows an error message dialog with an OK button.
        /// </summary>
        /// <param name="message">Message with placeholders in invariant language.</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <remarks>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix,
        /// and simply formats it with the <paramref name="args"/> parameters. See the <strong>Remarks</strong> section of the <see cref="ErrorMessage(string)"/> overload for more details.</note>
        /// </remarks>
        [Obsolete("Use ErrorMessage(string) with an already localized message instead.")]
        public static void ErrorMessage(string message, params object[] args) => ErrorMessage(Language.Translate(CultureInfo.CurrentCulture, message, args));

        /// <summary>
        /// Shows an error message dialog with an OK button.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ErrorMessage(string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal message dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="message">The message to display in the error dialog.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, a localized string similar to <c>Error</c> will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void ErrorMessage(IWin32Window? owner, string message, string? caption = null)
            => ShowMessage(owner, message, caption ?? Res.DialogsErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

        #endregion

        #region Warning

        /// <summary>
        /// Shows a warning message dialog with an OK button.
        /// </summary>
        /// <param name="message">The message to display in the warning dialog.</param>
        /// <remarks>
        /// <note>In versions prior to 5.0.0, the <paramref name="message"/> was translated by the obsolete <see cref="Language"/> class.
        /// Since version 5.0.0, the <paramref name="message"/> is expected to be already localized. To use the same dynamic localization
        /// as <see cref="BaseForm"/> or <see cref="BaseUserControl"/> when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/> or <see cref="DynamicStringLocalization.LocalScope"/>, you can use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method.
        /// To localize the window caption, you can use the <see cref="WarningMessage(IWin32Window,string,string)"/> overload and pass a custom localized string to the <c>caption</c> parameter.
        /// To localize both the window caption and buttons, opt-in to use task dialogs by setting the <see cref="UseTaskDialogs"/> property to <see langword="true"/>,
        /// and set the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// auto-generated <c>KGySoft.WinForms.Messages.&lt;LanguageId&gt;.resx</c> files in the <c>Resources</c> folder of the executable application.</note>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// </remarks>
        public static void WarningMessage(string message) => WarningMessage(null, message);

        /// <summary>
        /// Shows a warning message dialog with an OK button.
        /// </summary>
        /// <param name="message">Message with placeholders in invariant language.</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <remarks>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix,
        /// and simply formats it with the <paramref name="args"/> parameters. See the <strong>Remarks</strong> section of the <see cref="WarningMessage(string)"/> overload for more details.</note>
        /// </remarks>
        [Obsolete("Use WarningMessage(string) with an already localized message instead.")]
        public static void WarningMessage(string message, params object[] args) => WarningMessage(Language.Translate(CultureInfo.CurrentCulture, message, args));

        /// <summary>
        /// Shows a warning message dialog with an OK button.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WarningMessage(string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal message dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="message">The message to display in the warning dialog.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, a localized string similar to <c>Warning</c> will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void WarningMessage(IWin32Window? owner, string message, string? caption = null)
            => ShowMessage(owner, message, caption ?? Res.DialogsWarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        #endregion

        #region Confirmation

        /// <summary>
        /// Shows a confirmation message dialog with Yes and No buttons.
        /// </summary>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked or the dialog was closed.</returns>
        /// <remarks>
        /// <note>In versions prior to 5.0.0, the <paramref name="message"/> was translated by the obsolete <see cref="Language"/> class.
        /// Since version 5.0.0, the <paramref name="message"/> is expected to be already localized. To use the same dynamic localization
        /// as <see cref="BaseForm"/> or <see cref="BaseUserControl"/> when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/> or <see cref="DynamicStringLocalization.LocalScope"/>, you can use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method.
        /// To localize the window caption, you can use the <see cref="ConfirmMessage(IWin32Window,string,string,bool)"/> overload and pass a custom localized string to the <c>caption</c> parameter.
        /// To localize both the window caption and buttons, opt-in to use task dialogs by setting the <see cref="UseTaskDialogs"/> property to <see langword="true"/>,
        /// and set the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// auto-generated <c>KGySoft.WinForms.Messages.&lt;LanguageId&gt;.resx</c> files in the <c>Resources</c> folder of the executable application.</note>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// <para>To show also a Cancel button, use the <see cref="O:KGySoft.WinForms.Forms.Dialogs.CancellableConfirmMessage">CancellableConfirmMessage</see> methods instead.</para>
        /// </remarks>
        public static bool ConfirmMessage(string message) => ConfirmMessage(null, message);

        // NOTE: isYesDefault could be an optional parameter in the other overload, but it would break the binary compatibility with previous versions.
        /// <summary>
        /// Shows a confirmation message dialog with Yes and No buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfirmMessage(string)"/> overload for more details.
        /// </summary>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <param name="isYesDefault"><see langword="true"/> if the Yes button should be the default button, <see langword="false"/> if No should be the default.</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked or the dialog was closed.</returns>
        public static bool ConfirmMessage(string message, bool isYesDefault) => ConfirmMessage(null, message, null, isYesDefault);

        /// <summary>
        /// Shows a confirmation message dialog with Yes and No buttons.
        /// </summary>
        /// <param name="message">Message with placeholders in invariant language.</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked or the dialog was closed.</returns>
        /// <remarks>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix,
        /// and simply formats it with the <paramref name="args"/> parameters. See the <strong>Remarks</strong> section of the <see cref="ConfirmMessage(string)"/> overload for more details.</note>
        /// </remarks>
        [Obsolete("Use ConfirmMessage(string) with an already localized message instead.")]
        public static bool ConfirmMessage(string message, params object[] args) => ConfirmMessage(Language.Translate(CultureInfo.CurrentCulture, message, args));

        /// <summary>
        /// Shows a confirmation message dialog with Yes and No buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfirmMessage(string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal message dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, a localized string similar to <c>Confirmation</c> will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="isYesDefault"><see langword="true"/> if the Yes button should be the default button, <see langword="false"/> if No should be the default. This parameter is optional.
        /// <br/>Default value: <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked or the dialog was closed.</returns>
        public static bool ConfirmMessage(IWin32Window? owner, string message, string? caption = null, bool isYesDefault = true)
            => ShowMessage(owner, message, caption ?? Res.DialogsConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                isYesDefault ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2) == true;

        /// <summary>
        /// Shows a confirmation message dialog with Yes, No, and an optional Cancel button.
        /// <br/>This method is obsolete. To show a confirmation dialog with Yes, No and Cancel buttons, use the <see cref="CancellableConfirmMessage(string,MessageBoxDefaultButton)"/> method instead.
        /// </summary>
        /// <param name="message">Message in invariant language</param>
        /// <param name="cancelButton"><see langword="true"/> to show also a Cancel button; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DialogResult"/> value indicating the user's choice.</returns>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix.
        /// See the <strong>Remarks</strong> section of the <see cref="ConfirmMessage(string)"/> overload for more details.</note>
        [Obsolete("To show also a Cancel button, use the CancellableConfirmMessage methods instead.")]
        public static DialogResult ConfirmMessage(bool cancelButton, string message) => ConfirmMessage(cancelButton, message, null);

        /// <summary>
        /// Shows a confirmation message dialog with Yes, No, and an optional Cancel button.
        /// <br/>This method is obsolete. To show a confirmation dialog with Yes, No and Cancel buttons, use the <see cref="CancellableConfirmMessage(string,MessageBoxDefaultButton)"/> method instead.
        /// </summary>
        /// <param name="message">Message with placeholders in invariant language.</param>
        /// <param name="args">Arguments for placeholders</param>
        /// <param name="cancelButton"><see langword="true"/> to show also a Cancel button; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DialogResult"/> value indicating the user's choice.</returns>
        /// <remarks>
        /// <note type="warning">This overload does not translate the <paramref name="message"/> parameter anymore, just removes the possibly existing distinction postfix,
        /// and simply formats it with the <paramref name="args"/> parameters. See the <strong>Remarks</strong> section of the <see cref="ConfirmMessage(string)"/> overload for more details.</note>
        /// </remarks>
        [Obsolete("To show also a Cancel button, use the CancellableConfirmMessage methods instead.")]
        public static DialogResult ConfirmMessage(bool cancelButton, string message, params object[]? args) => cancelButton
            ? CancellableConfirmMessage(Language.Translate(CultureInfo.CurrentCulture, message, args)) switch
            {
                true => DialogResult.Yes,
                false => DialogResult.No,
                null => DialogResult.Cancel
            }
            : ConfirmMessage(Language.Translate(message)) ? DialogResult.Yes : DialogResult.No;

        /// <summary>
        /// Shows a confirmation message dialog with Yes, No and Cancel buttons.
        /// </summary>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <param name="defaultButton">The default button to select when the dialog is shown. This parameter is optional.
        /// <br/>Default value: <see cref="MessageBoxDefaultButton.Button1"/> (i.e. Yes is the default button).</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked, or <see langword="null"/> if Cancel was clicked or the dialog was closed.</returns>
        /// <remarks>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// <para>To show also only Yes and No buttons, use the <see cref="O:KGySoft.WinForms.Forms.Dialogs.ConfirmMessage">ConfirmMessage</see> methods instead.</para>
        /// </remarks>
        public static bool? CancellableConfirmMessage(string message, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
            => CancellableConfirmMessage(null, message, null, defaultButton);

        /// <summary>
        /// Shows a confirmation message dialog with Yes, No and Cancel buttons.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal message dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, a localized string similar to <c>Confirmation</c> will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="defaultButton">The default button to select when the dialog is shown. This parameter is optional.
        /// <br/>Default value: <see cref="MessageBoxDefaultButton.Button1"/> (i.e. Yes is the default button).</param>
        /// <returns><see langword="true"/> if the user clicked Yes, <see langword="false"/> if No was clicked, or <see langword="null"/> if Cancel was clicked or the dialog was closed.</returns>
        /// <remarks>
        /// <para>This overload sets the <see cref="DialogsOwner"/> as the owner of the dialog; or, if it is <see langword="null"/>, the currently active window will be the owner.</para>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// <para>To show also only Yes and No buttons, use the <see cref="O:KGySoft.WinForms.Forms.Dialogs.ConfirmMessage">ConfirmMessage</see> methods instead.</para>
        /// </remarks>
        public static bool? CancellableConfirmMessage(IWin32Window? owner, string message, string? caption = null, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
            => ShowMessage(owner, message, caption ?? Res.DialogsConfirmationCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, defaultButton);

        #endregion

        #region Input Dialog

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, the name of the application is used.</param>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <param name="x">The initial horizontal position of the dialog.</param>
        /// <param name="y">The initial vertical position of the dialog.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(string? caption, string? prompt, ref string value, int x, int y) => InputDialog(null, caption, prompt, ref value, x, y);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal input dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, the name of the application is used.</param>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <param name="x">The initial horizontal position of the dialog.</param>
        /// <param name="y">The initial vertical position of the dialog.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(IWin32Window? owner, string? caption, string? prompt, ref string value, int x, int y)
            => InputBox.Show(owner ?? DialogsOwner, caption ?? ApplicationHelper.ApplicationName, prompt ?? Res.DialogsDefaultPrompt, ref value, new Point(x, y));

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal input dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, the name of the application is used.</param>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        /// <remarks>
        /// <note>In versions prior to 5.0.0, the <paramref name="caption"/> and the <paramref name="prompt"/> were translated by the obsolete <see cref="Language"/> class.
        /// Since version 5.0.0, the parameters are expected to be already localized. To use the same dynamic localization
        /// as <see cref="BaseForm"/> or <see cref="BaseUserControl"/> when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/> or <see cref="DynamicStringLocalization.LocalScope"/>, you can use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method.
        /// To localize default prompt text and buttons, set the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// property to <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Resources_ResourceManagerSources.htm">CompiledAndResX</a> in the startup code of your application, and translate the
        /// auto-generated <c>KGySoft.WinForms.Messages.&lt;LanguageId&gt;.resx</c> files in the <c>Resources</c> folder of the executable application.</note>
        /// <para>To use a right-to-left layout when the UI culture of the current thread is a right-to-left language, set the <see cref="AutoRightToLeftLayout"/> property to <see langword="true"/> before calling this method.</para>
        /// </remarks>
        public static bool InputDialog(IWin32Window? owner, string? caption, string? prompt, ref string value)
            => InputBox.Show(owner ?? DialogsOwner, caption ?? ApplicationHelper.ApplicationName, prompt ?? Res.DialogsDefaultPrompt, ref value);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="caption">The caption of the dialog. If <see langword="null"/>, the name of the application is used.</param>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(string? caption, string? prompt, ref string value) => InputDialog(null, caption, prompt, ref value);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(string? prompt, ref string value) => InputDialog(null, null, prompt, ref value);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal input dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="prompt">A prompt text, explaining the purpose of the input dialog. If <see langword="null"/>, a localized string of <c>Value:</c> is used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(IWin32Window? owner, string? prompt, ref string value) => InputDialog(owner, null, prompt, ref value);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(ref string value) => InputDialog(null, null, null, ref value);

        /// <summary>
        /// Displays an input dialog with an editable value, and OK and Cancel buttons.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="InputDialog(IWin32Window,string,string,ref string)"/> overload for more details.
        /// </summary>
        /// <param name="owner">An optional window that will own the modal input dialog. If <see langword="null"/>, the <see cref="DialogsOwner"/> property will be taken.
        /// If <see cref="DialogsOwner"/> is also <see langword="null"/>, the currently active window will be used.</param>
        /// <param name="value">A reference to a string that contains the initial value of the input field. When this method returns <see langword="true"/>, this parameter will contain the value entered by the user.</param>
        /// <returns><see langword="true"/> if the user clicked OK or pressed Enter, <see langword="false"/> if the user clicked Cancel, pressed Esc or closed the dialog.</returns>
        public static bool InputDialog(IWin32Window? owner, ref string value) => InputDialog(owner, null, null, ref value);

        #endregion

        #endregion

        #region Private Methods

        [SuppressMessage("ReSharper", "UsingStatementResourceInitialization", Justification = "False alarm, these properties do not throw exceptions")]
        private static bool? ShowMessage(IWin32Window? owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton = default)
        {
            owner ??= DialogsOwner;
            if (UseTaskDialogs)
            {
                using var taskDialog = new TaskDialog
                {
                    Caption = caption,
                    Message = message,
                    StandardButtons = buttons switch
                    {
                        MessageBoxButtons.YesNo => TaskDialogStandardButtonFlags.Yes | TaskDialogStandardButtonFlags.No,
                        MessageBoxButtons.YesNoCancel => TaskDialogStandardButtonFlags.Yes | TaskDialogStandardButtonFlags.No | TaskDialogStandardButtonFlags.Cancel,
                        _ => TaskDialogStandardButtonFlags.OK
                    },
                    Icon = icon switch
                    {
                        MessageBoxIcon.Information => TaskDialogStandardIcons.Information,
                        MessageBoxIcon.Error => TaskDialogStandardIcons.Error,
                        MessageBoxIcon.Warning => TaskDialogStandardIcons.Warning,
                        MessageBoxIcon.Question => TaskDialogStandardIcons.Question,
                        _ => TaskDialogStandardIcons.None
                    },
                    Options = TaskDialogOptions.ForceShowSysMenu,
                    DefaultStandardButton = defaultButton switch
                    {
                        MessageBoxDefaultButton.Button2 => TaskDialogStandardButtons.No,
                        MessageBoxDefaultButton.Button3 => TaskDialogStandardButtons.Cancel,
                        _ => buttons == MessageBoxButtons.OK ? TaskDialogStandardButtons.OK : TaskDialogStandardButtons.Yes
                    },
                };

                if (buttons != MessageBoxButtons.YesNo)
                    taskDialog.Options |= TaskDialogOptions.AllowCancel;
                if (AutoRightToLeftLayout && LanguageSettings.DisplayLanguage.TextInfo.IsRightToLeft)
                    taskDialog.Options |= TaskDialogOptions.RightToLeftLayout;
                if (LanguageSettings.DynamicResourceManagersSource is ResourceManagerSources.CompiledAndResX or ResourceManagerSources.ResXOnly)
                    taskDialog.Options |= TaskDialogOptions.TranslateStandardButtons;
                
                TaskDialogResult result = owner != null || !OSHelper.IsWindows
                    ? taskDialog.Show(owner)
                    : taskDialog.Show(User32.GetActiveWindow()); // without an owner the task dialog would be non-modal, so getting the active window explicitly
                
                return result switch
                {
                    TaskDialogResult.Yes => true,
                    TaskDialogResult.No => false,
                    _ => null
                };
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (UseAdvancedDialogs)
            {
                using var frm = new AdvancedMessageDialog();
                var result = frm.Execute(message, null, caption,
                    icon switch
                    {
                        MessageBoxIcon.Information => AdvancedDialogTypes.Information,
                        MessageBoxIcon.Error => AdvancedDialogTypes.Error,
                        MessageBoxIcon.Warning => AdvancedDialogTypes.Warning,
                        _ => AdvancedDialogTypes.Confirmation,
                    },
                    buttons switch
                    {
                        MessageBoxButtons.YesNo => ButtonTypes.YesNo,
                        MessageBoxButtons.YesNoCancel => ButtonTypes.YesNoCancel,
                        _ => ButtonTypes.OK,
                    });
                return result switch
                {
                    DialogResult.Yes => true,
                    DialogResult.No => false,
                    _ => null
                };
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return MessageBox.Show(owner, message, caption, buttons, icon, defaultButton,
                    AutoRightToLeftLayout && LanguageSettings.DisplayLanguage.TextInfo.IsRightToLeft ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : default) switch
                {
                    DialogResult.Yes => true,
                    DialogResult.No => false,
                    _ => null
                };
        }

        #endregion

        #endregion
    }
}
