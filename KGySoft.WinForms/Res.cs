#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Globalization;
using KGySoft.CoreLibraries;
using KGySoft.Resources;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Contains the string resources of the project.
    /// </summary>
    internal static class Res
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.WinForms.Messages", typeof(Res).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        #region BaseForm

        /// <summary>The form must be an MDI container. Set IsMdiContainer before accessing this member.</summary>
        internal static string BaseFormNotMdiContainer => Get("BaseForm_NotMdiContainer");

        /// <summary>This form must be either an MDI parent or an MDI child.</summary>
        internal static string BaseFormMdiContainerNotFound => Get("BaseForm_MdiContainerNotFound");

        /// <summary>Cannot suspend an MDI parent.</summary>
        internal static string BaseFormCannotSuspendMdiParent => Get("BaseForm_CannotSuspendMdiParent");

        /// <summary>This form is already suspended by another MDI child.</summary>
        internal static string BaseFormAlreadySuspended => Get("BaseForm_AlreadySuspended");

        #endregion

        #region Dialogs

        /// <summary>Information</summary>
        internal static string DialogsInfoCaption => Get("Dialogs_InfoCaption");

        /// <summary>Warning</summary>
        internal static string DialogsWarningCaption => Get("Dialogs_WarningCaption");

        /// <summary>Error</summary>
        internal static string DialogsErrorCaption => Get("Dialogs_ErrorCaption");

        /// <summary>Confirmation</summary>
        internal static string DialogsConfirmationCaption => Get("Dialogs_ConfirmationCaption");

        #endregion

        #region TaskDialog

        /// <summary>See &amp;details</summary>
        internal static string TaskDialogShowDetails => Get("TaskDialog_ShowDetails");

        /// <summary>Hide &amp;details</summary>
        internal static string TaskDialogHideDetails => Get("TaskDialog_HideDetails");

        /// <summary>[Window Title]</summary>
        internal static string TaskDialogCaption => Get("TaskDialog_Caption");

        /// <summary>[Main Instruction]</summary>
        internal static string TaskDialogMainInstruction => Get("TaskDialog_MainInstruction");

        /// <summary>[Content]</summary>
        internal static string TaskDialogMessage => Get("TaskDialog_Message");

        /// <summary>[Expanded Information]</summary>
        internal static string TaskDialogDetails => Get("TaskDialog_Details");

        /// <summary>[Footer]</summary>
        internal static string TaskDialogFooter => Get("TaskDialog_Footer");

        /// <summary>Cannot change this property while the TaskDialog is being initialized.</summary>
        internal static string TaskDialogInitializing => Get("TaskDialog_Initializing");

        #endregion

        #region Visual Styles

        /// <summary>Visual styles are not available.</summary>
        internal static string NoVisualStyles => Get("VisualStyles_NoVisualStyles");

        #endregion

        #region InvokeMarshaller

        /// <summary>Invoke is required but no SynchronizationContext was captured and the handle of the owner is not created yet.</summary>
        internal static string InvokeMarshallerNoSynchronizationContext => Get("InvokeMarshaller_NoSynchronizationContext");

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        #region General

        /// <summary>
        /// Just an empty method to be able to trigger the static constructor without running any code other than field initializations.
        /// </summary>
        internal static void EnsureInitialized()
        {
        }

        internal static string Get<TEnum>(TEnum value) where TEnum : struct, Enum => Get($"{value.GetType().Name}.{Enum<TEnum>.ToString(value)}");

        /// <summary>Internal Error: {0}</summary>
        internal static string InternalError(string msg) => Get("General_InternalErrorFormat", msg);

        #endregion

        #region Accessors

        /// <summary>Instance property "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstancePropertyDoesNotExist(string? propertyName, Type type) => Get("Accessors_InstancePropertyDoesNotExistFormat", propertyName, type);

        /// <summary>Static property "{0}" not found on type "{1}".</summary>
        internal static string AccessorsStaticPropertyDoesNotExist(string? propertyName, Type type) => Get("Accessors_StaticPropertyDoesNotExistFormat", propertyName, type);

        /// <summary>Instance field "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstanceFieldDoesNotExist(string? fieldName, Type type) => Get("Accessors_InstanceFieldDoesNotExistFormat", fieldName, type);

        /// <summary>Static field "{0}" not found on type "{1}".</summary>
        internal static string AccessorsStaticFieldDoesNotExist(string? fieldName, Type type) => Get("Accessors_StaticFieldDoesNotExistFormat", fieldName, type);

        /// <summary>Method "{0}" was not found on type "{1}".</summary>
        internal static string AccessorsMethodDoesNotExist(string? methodName, Type type) => Get("Accessors_InstanceDoesNotExistFormat", methodName, type);

        #endregion

        #region TaskDialog

        /// <summary>(O) {0}</summary>
        internal static string TaskDialogRadioButtonChecked(string? text) => Get("TaskDialog_RadioButtonCheckedFormat", text);

        /// <summary>( ) {0}</summary>
        internal static string TaskDialogRadioButtonUnchecked(string? text) => Get("TaskDialog_RadioButtonUncheckedFormat", text);

        /// <summary>[{0}]</summary>
        internal static string TaskDialogButton(string? text) => Get("TaskDialog_ButtonFormat", text);

        /// <summary>[U {0}]</summary>
        internal static string TaskDialogButtonElevated(string? text) => Get("TaskDialog_ButtonElevatedFormat", text);

        /// <summary>[-> {0}]</summary>
        internal static string TaskDialogButtonCommandLink(string? text) => Get("TaskDialog_ButtonCommandLinkFormat", text);

        /// <summary>[# {0}]</summary>
        internal static string TaskDialogButtonCustomIcon(string? text) => Get("TaskDialog_ButtonCustomIconFormat", text);

        /// <summary>[^] {0}</summary>
        internal static string TaskDialogExpandoButtonExpanded(string? text) => Get("TaskDialog_ExpandoButtonExpandedFormat", text);

        /// <summary>[V] {0}</summary>
        internal static string TaskDialogExpandoButtonCollapsed(string? text) => Get("TaskDialog_ExpandoButtonCollapsedFormat", text);

        /// <summary>[X] {0}</summary>
        internal static string TaskDialogCheckBoxChecked(string? text) => Get("TaskDialog_CheckBoxCheckedFormat", text);

        /// <summary>[ ] {0}</summary>
        internal static string TaskDialogCheckBoxUnchecked(string? text) => Get("TaskDialog_CheckBoxUncheckedFormat", text);

        /// <summary>Task dialog control '{0}' already has a parent.</summary>
        internal static string TaskDialogHasParent(string name) => Get("TaskDialog_HasParentFormat", name);

        /// <summary>Changing property '{0}' is not allowed while the dialog is displayed.</summary>
        internal static string TaskDialogPropertyChange(string name) => Get("TaskDialog_PropertyChangeFormat", name);

        #endregion

        #region Localization

        /// <summary>Localization of key {0} failed because the format string {1} arguments: {2}</summary>
        internal static string LocalizationInvalidResource(string key, int count, string format) => Get("Localization_InvalidResourceFormat", key, count, format);

        #endregion

        #endregion

        #region Private Methods

        private static string Get(string id) => resourceManager.GetString(id, LanguageSettings.DisplayLanguage) ?? String.Format(CultureInfo.InvariantCulture, unavailableResource, id);

        private static string Get(string id, params object?[]? args)
        {
            string format = Get(id);
            return args == null ? format : SafeFormat(format, args);
        }

        private static string SafeFormat(string format, object?[] args)
        {
            try
            {
                int i = Array.IndexOf(args, null);
                if (i >= 0)
                {
                    string nullRef = PublicResources.Null;
                    for (; i < args.Length; i++)
                        args[i] ??= nullRef;
                }

                return String.Format(LanguageSettings.FormattingLanguage, format, args);
            }
            catch (FormatException)
            {
                return String.Format(CultureInfo.InvariantCulture, invalidResource, args.Length, format);
            }
        }

        #endregion

        #endregion
    }
}
