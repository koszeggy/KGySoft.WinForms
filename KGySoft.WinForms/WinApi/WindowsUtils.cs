#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WindowsUtils.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class WindowsUtils
    {
        #region Fields

        private static bool? isWin8OrLater;
        private static bool? isVistaOrLater;
        private static bool? isXpOrLater;
        private static bool? isComCtlV6Available;
        private static Version? windowsVersion;

        #endregion

        #region Properties

        internal static bool IsVistaOrLater
            => isVistaOrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 0, 5243);

        internal static bool IsWindows8OrLater
            => isWin8OrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 2, 9200);

        internal static bool IsWindowsXpOrLater
            => isWin8OrLater ??= GetWindowsVersion() is Version version && version >= new Version(5, 1, 2600);

        /// <summary>
        /// Gets whether comctl32.dll V6 is available, without loading it explicitly.
        /// After all tells, whether <see cref="Application.EnableVisualStyles"/> was already called in this current application.
        /// </summary>
        internal static bool IsComCtlV6Available
        {
            get
            {
                if (isComCtlV6Available.HasValue)
                    return isComCtlV6Available.Value;

                // pre-XP: no visual styles
                if (!IsWindowsXpOrLater)
                {
                    isComCtlV6Available = false;
                    return false;
                }

                // visual styles are actually used
                if (Application.RenderWithVisualStyles)
                {
                    isComCtlV6Available = true;
                    return true;
                }

                // Here EnableVisualStyles was either called but classic theme is used (true result) or visual styles were not enabled at all (false result)
                // We could use the Comctl32ActivationContext and get the dll version of comctl32, but then V6 would be loaded accidentaly, causing that controls
                // begin to use visual styles in non-System mode.
                isComCtlV6Available = Accessors.ComCtlSupportsVisualStyles;
                return isComCtlV6Available.Value;
            }
        }

        #endregion

        #region Methods

        private static Version? GetWindowsVersion()
        {
            if (windowsVersion is not null)
                return windowsVersion;
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return null;

            windowsVersion = osVer.Version;
            return windowsVersion;
        }

        #endregion
    }
}
