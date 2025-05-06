#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VisualStyleHelper.cs
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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

using Microsoft.Win32;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides helper methods for working with visual styles. We could use the VisualStyleRenderer class,
    /// but it always re-validates the class name and part combinations, and does not allow some values, such as font properties.
    /// </summary>
    internal static class VisualStyleHelper
    {
        #region Fields

        private static IntPtr buttonThemeHandle;
        private static IntPtr taskDialogThemeHandle;
        private static bool? visualStylesAvailable;
        private static bool? highContrast;

        #endregion

        #region Properties

        internal static bool RenderWithVisualStyles => visualStylesAvailable ??= Application.RenderWithVisualStyles;

        internal static bool HighContrast => highContrast ??= SystemInformation.HighContrast;

        internal static IntPtr ButtonTheme => buttonThemeHandle != IntPtr.Zero
            ? buttonThemeHandle
            : buttonThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassButton);

        internal static IntPtr TaskDialogTheme => taskDialogThemeHandle != IntPtr.Zero
            ? taskDialogThemeHandle
            : taskDialogThemeHandle = UxTheme.OpenThemeDataGlobal(Constants.ThemeClassTaskDialog);

        #endregion

        #region Constructors

        static VisualStyleHelper()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal static Size GetPartSize(IntPtr hTheme, Control control, Graphics g, int part, int state, bool actualSize)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control.HasNonDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                return UxTheme.GetThemePartSize(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, hdc, part, state,
                    (int)(actualSize ? ThemeSizeType.True : ThemeSizeType.Draw));
            }
            finally
            {
                g.ReleaseHdc(hdc);
                if (hThemeWindow != IntPtr.Zero && hTheme != hThemeWindow)
                    UxTheme.CloseThemeData(hThemeWindow);
            }
        }

        internal static void Render(IntPtr hTheme, Control control, Graphics g, int part, int state, Rectangle bounds)
        {
            IntPtr hThemeWindow = IntPtr.Zero;
            IntPtr hdc = g.GetHdc();
            try
            {
                if (control.HasNonDefaultScaling())
                    hThemeWindow = UxTheme.OpenThemeDataForWindow(control.Handle, GetClassName(hTheme));

                UxTheme.DrawThemeBackground(hThemeWindow == IntPtr.Zero ? hTheme : hThemeWindow, hdc, part, state, bounds);
            }
            finally
            {
                g.ReleaseHdc(hdc);
                if (hThemeWindow != IntPtr.Zero && hTheme != hThemeWindow)
                    UxTheme.CloseThemeData(hThemeWindow);
            }
        }

        internal static Color GetTextColor(IntPtr hTheme, int part, int state) => UxTheme.GetThemeColor(hTheme, part, state, Constants.TMT_COLOR);

        internal static Font GetFont(IntPtr hTheme, Graphics g, int part)
        {
            IntPtr hdc = g.GetHdc();
            try
            {
                return UxTheme.GetThemeFont(hTheme, hdc, part, 0, Constants.TMT_FONT);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        internal static void ClearCaches()
        {
            buttonThemeHandle = IntPtr.Zero;
            taskDialogThemeHandle = IntPtr.Zero;
            visualStylesAvailable = null;
            highContrast = null;
        }

        #endregion

        #region Private Methods

        private static string GetClassName(IntPtr hTheme) => hTheme == buttonThemeHandle ? Constants.ThemeClassButton
            : hTheme == taskDialogThemeHandle ? Constants.ThemeClassTaskDialog
            : String.Empty; // Not throwing here for performance reasons so the method can be inlined. The exception will be thrown by UxTheme

        #endregion

        #region Event handlers

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category is UserPreferenceCategory.VisualStyle or UserPreferenceCategory.General) // General: Light/Dark mode or DPI
                ClearCaches();
        }

        #endregion

        #endregion
    }
}
