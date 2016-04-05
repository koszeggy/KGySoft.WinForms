using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls.WinApi
{
    /// <summary>
    /// TASKDIALOG_MESSAGES taken from CommCtrl.h.
    /// </summary>
    internal enum TASKDIALOG_MESSAGES
    {
        /// <summary>
        /// Navigate to a newly allocated page
        /// lParam = ptr to new page
        /// </summary>
        TDM_NAVIGATE_PAGE = Constants.WM_USER + 101,

        /// <summary>
        /// Click button.
        /// wParam = Button ID
        /// </summary>
        TDM_CLICK_BUTTON = Constants.WM_USER + 102,

        /// <summary>
        /// Set Progress bar to be marquee mode.
        /// wParam = 0 (nonMarque) wParam != 0 (Marquee)
        /// </summary>
        TDM_SET_MARQUEE_PROGRESS_BAR = Constants.WM_USER + 103,

        /// <summary>
        /// Set Progress bar state.
        /// wParam = new progress state
        /// </summary>
        TDM_SET_PROGRESS_BAR_STATE = Constants.WM_USER + 104,

        /// <summary>
        /// Set progress bar range.
        /// lParam = MAKELPARAM(nMinRange, nMaxRange)
        /// </summary>
        TDM_SET_PROGRESS_BAR_RANGE = Constants.WM_USER + 105,

        /// <summary>
        /// Set progress bar position.
        /// wParam = new position
        /// </summary>
        TDM_SET_PROGRESS_BAR_POS = Constants.WM_USER + 106,

        /// <summary>
        /// Set progress bar marquee (animation).
        /// wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
        /// </summary>
        TDM_SET_PROGRESS_BAR_MARQUEE = Constants.WM_USER + 107,

        /// <summary>
        /// Set a text element of the Task Dialog.
        /// wParam = element (<see cref="TASKDIALOG_ELEMENTS"/>), lParam = new element text (LPCWSTR)
        /// </summary>
        TDM_SET_ELEMENT_TEXT = Constants.WM_USER + 108,

        /// <summary>
        /// Click a radio button.
        /// wParam = Radio Button ID
        /// </summary>
        TDM_CLICK_RADIO_BUTTON = Constants.WM_USER + 110,

        /// <summary>
        /// Enable or disable a button.
        /// lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
        /// </summary>
        TDM_ENABLE_BUTTON = Constants.WM_USER + 111,

        /// <summary>
        /// Enable or disable a radio button.
        /// lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
        /// </summary>
        TDM_ENABLE_RADIO_BUTTON = Constants.WM_USER + 112,

        /// <summary>
        /// Check or uncheck the verfication checkbox.
        /// wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
        /// </summary>
        TDM_CLICK_VERIFICATION = Constants.WM_USER + 113,

        /// <summary>
        /// Update the text of an element (no effect if origially set as null).
        /// wParam = element (<see cref="TASKDIALOG_ELEMENTS"/>), lParam = new element text (LPCWSTR)
        /// </summary>
        TDM_UPDATE_ELEMENT_TEXT = Constants.WM_USER + 114,

        /// <summary>
        /// Designate whether a given Task Dialog button or command link should have a User Account Control (UAC) shield icon.
        /// wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
        /// </summary>
        TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = Constants.WM_USER + 115,

        /// <summary>
        /// Refreshes the icon of the task dialog.
        /// wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
        /// </summary>
        TDM_UPDATE_ICON = Constants.WM_USER + 116
    }
}
