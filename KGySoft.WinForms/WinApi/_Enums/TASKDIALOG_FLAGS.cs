#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TASKDIALOG_FLAGS.cs
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

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Specifies the behavior of the task dialog.
    /// </summary>
    [Flags]
    internal enum TASKDIALOG_FLAGS
    {
        /// <summary>
        /// Enables hyperlink processing for the strings specified in the pszContent, pszExpandedInformation and pszFooter members. When enabled, these members may point to strings that contain hyperlinks in the following form:
        /// <A HREF="executablestring">Hyperlink Text</A>
        /// Warning: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
        /// Note  Task Dialogs will not actually execute any hyperlinks. Hyperlink execution must be handled in the callback function specified by pfCallback. For more details, see TaskDialogCallbackProc.
        /// </summary>
        TDF_ENABLE_HYPERLINKS = 0x0001,

        /// <summary>
        /// Indicates that the dialog should use the icon referenced by the handle in the hMainIcon member as the primary icon in the task dialog.
        /// </summary>
        TDF_USE_HICON_MAIN = 0x0002,

        /// <summary>
        /// Indicates that the dialog should use the icon referenced by the handle in the hFooterIcon member as the footer icon in the task dialog.
        /// </summary>
        TDF_USE_HICON_FOOTER = 0x0004,

        /// <summary>
        /// Indicates that the dialog should be able to be closed using Alt-F4, Escape, and the title bar's close button even if no cancel button is specified in either the dwCommonButtons or pButtons members.
        /// </summary>
        TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,

        /// <summary>
        /// Indicates that the buttons specified in the pButtons member are to be displayed as command links (using a standard task dialog glyph) instead of push buttons.
        /// When using command links, all characters up to the first new line character in the pszButtonText member will be treated as the command link's main text,
        /// and the remainder will be treated as the command link's note. This flag is ignored if the cButtons member is zero.
        /// </summary>
        TDF_USE_COMMAND_LINKS = 0x0010,

        /// <summary>
        /// Indicates that the buttons specified in the pButtons member are to be displayed as command links (without a glyph) instead of push buttons. When using command links,
        /// all characters up to the first new line character in the pszButtonText member will be treated as the command link's main text,
        /// and the remainder will be treated as the command link's note. This flag is ignored if the cButtons member is zero.
        /// </summary>
        TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,

        /// <summary>
        /// Indicates that the string specified by the pszExpandedInformation member is displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content.
        /// This flag is ignored if the pszExpandedInformation member is NULL.
        /// </summary>
        TDF_EXPAND_FOOTER_AREA = 0x0040,

        /// <summary>
        /// Indicates that the string specified by the pszExpandedInformation member is displayed when the dialog is initially displayed.
        /// This flag is ignored if the pszExpandedInformation member is NULL.
        /// </summary>
        TDF_EXPANDED_BY_DEFAULT = 0x0080,

        /// <summary>
        /// Indicates that the verification checkbox in the dialog is checked when the dialog is initially displayed.
        /// This flag is ignored if the pszVerificationText parameter is NULL.
        /// </summary>
        TDF_VERIFICATION_FLAG_CHECKED = 0x0100,

        /// <summary>
        /// Indicates that a Progress Bar is to be displayed.
        /// </summary>
        TDF_SHOW_PROGRESS_BAR = 0x0200,

        /// <summary>
        /// Indicates that an Marquee Progress Bar is to be displayed.
        /// </summary>
        TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,

        /// <summary>
        /// Indicates that the task dialog's callback is to be called approximately every 200 milliseconds.
        /// </summary>
        TDF_CALLBACK_TIMER = 0x0800,

        /// <summary>
        /// Indicates that the task dialog is positioned (centered) relative to the window specified by hwndParent.
        /// If the flag is not supplied (or no hwndParent member is specified), the task dialog is positioned (centered) relative to the monitor.
        /// </summary>
        TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,

        /// <summary>
        /// Indicates that text is displayed reading right to left.
        /// </summary>
        TDF_RTL_LAYOUT = 0x2000,

        /// <summary>
        /// Indicates that no default item will be selected.
        /// </summary>
        TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,

        /// <summary>
        /// Indicates that the task dialog can be minimized.
        /// </summary>
        TDF_CAN_BE_MINIMIZED = 0x8000,

        /// <summary>
        /// Indicates that the width of the task dialog is determined by the width of its content area. This flag is ignored if cxWidth is not set to 0.
        /// </summary>
        TDF_SIZE_TO_CONTENT = 0x1000000
    }
}
