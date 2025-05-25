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

#if !NETCOREAPP
using Microsoft.Win32;
#endif

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class WindowsUtils
    {
        #region Fields

        private static bool? isXpOrLater;
        private static bool? isVistaOrLater;
        private static bool? isWin10OrLater;
        private static bool? isWin10_1607OrLater;
        private static bool? isWin81OrLater;
        private static Version? windowsVersion;

        #endregion

        #region Properties

        internal static bool IsWindowsXpOrLater
            => isXpOrLater ??= GetWindowsVersion() is Version version && version >= new Version(5, 1, 2600);

        internal static bool IsVistaOrLater
            => isVistaOrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 0, 5243);

        internal static bool IsWindows81OrLater
            => isWin81OrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 3, 9600);

        internal static bool IsWindows10OrLater
            => isWin10OrLater ??= GetWindowsVersion() is Version version && version >= new Version(10, 0, 10240);

        /// <summary>
        ///  Windows 10 Anniversary Update or later. (Redstone 1, build 14393, version 1607)
        /// </summary>
        internal static bool IsWindows10_1607OrLater
            => isWin10_1607OrLater ??= GetWindowsVersion() is Version version && version >= new Version(10, 0, 14393);

        #endregion

        #region Methods

        private static Version? GetWindowsVersion()
        {
            if (windowsVersion is not null)
                return windowsVersion;
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return null;

#if NETCOREAPP
            windowsVersion = osVer.Version;
#else
            if (osVer.Version != new Version(6, 2, 9200, 0))
                windowsVersion = osVer.Version;
            else
            {
                // .NET Framework never returns a higher version than Windows 8, so we need to access the Registry
                // NOTE: This can be fixed by an app.manifest file with supportedOS element, but we cannot guarantee that in a consumer application
                const string path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                const string keyLcuVer = "LCUVer";
                const string keyMajor = "CurrentMajorVersionNumber";
                const string keyMinor = "CurrentMinorVersionNumber";
                const string keyBuild = "CurrentBuild";
                const int defaultMajor = 10;
                const int defaultMinor = 0;
                try
                {
                    using RegistryKey? reg = Registry.LocalMachine.OpenSubKey(path);
                    if (reg == null)
                        windowsVersion = osVer.Version;
                    else if (reg.GetValue(keyLcuVer) is string versionString && VersionExtensions.TryParse(versionString, out Version? version))
                        windowsVersion = version;
                    else if (reg.GetValue(keyBuild) is string build && Int32.TryParse(build, out int buildNumber))
                        windowsVersion = new Version(reg.GetValue(keyMajor, defaultMajor) is int major ? major : defaultMajor,
                            reg.GetValue(keyMinor, defaultMinor) is int minor ? minor : defaultMinor,
                            buildNumber);
                    else
                        windowsVersion = osVer.Version;
                }
                catch (Exception e) when (!e.IsCritical())
                {
                    windowsVersion = osVer.Version;
                }
            }
#endif
            return windowsVersion;
        }

        #endregion
    }
}
