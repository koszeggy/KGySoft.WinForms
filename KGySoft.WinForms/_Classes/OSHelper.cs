#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OSHelper.cs
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

#if !NET5_0_OR_GREATER
using Microsoft.Win32;
#endif

#endregion

namespace KGySoft.WinForms
{
    public static class OSHelper
    {
        #region Fields

        private static bool? isXpOrLater;
        private static bool? isVistaOrLater;
        private static bool? isWin10OrLater;
        private static bool? isWin10Build1607OrLater;
        private static bool? isWin7OrLater;
        private static bool? isWin8OrLater;
        private static bool? isWin81OrLater;
        private static bool? isWin11OrLater;
        private static bool? isWindows;
        private static bool? isMono;
        private static Version? windowsVersion;

        #endregion

        #region Properties

        public static bool IsWindows => isWindows ??= Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Win32Windows;
        public static bool IsMono => isMono ??= Type.GetType("Mono.Runtime") != null;

        public static bool IsWindowsXpOrLater
            => isXpOrLater ??= GetWindowsVersion() is Version version && version >= new Version(5, 1, 2600);

        public static bool IsWindowsVistaOrLater
            => isVistaOrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 0, 5243);

        public static bool IsWindows7OrLater
            => isWin7OrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 1, 7600);

        public static bool IsWindows8OrLater
            => isWin8OrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 2, 9200);

        public static bool IsWindows81OrLater
            => isWin81OrLater ??= GetWindowsVersion() is Version version && version >= new Version(6, 3, 9600);

        public static bool IsWindows10OrLater
            => isWin10OrLater ??= GetWindowsVersion() is Version version && version >= new Version(10, 0, 10240);

        /// <summary>
        ///  Windows 10 Anniversary Update or later. (Redstone 1, build 14393, version 1607)
        /// </summary>
        internal static bool IsWindows10Build1607OrLater
            => isWin10Build1607OrLater ??= GetWindowsVersion() is Version version && version >= new Version(10, 0, 14393);

        public static bool IsWindows11OrLater
            => isWin11OrLater ??= GetWindowsVersion() is Version version && version >= new Version(10, 0, 22000);

        #endregion

        #region Methods

        public static Version? GetWindowsVersion()
        {
            if (windowsVersion is not null)
                return windowsVersion;
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return null;

#if NET5_0_OR_GREATER
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
