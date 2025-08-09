#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ApplicationHelper.cs
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms
{
    internal static class ApplicationHelper
    {
        #region Fields

        private static string? applicationName;

        #endregion

        #region Properties

        internal static string ApplicationName
        {
            get
            {
                if (applicationName == null)
                {
                    string?[] args = Environment.GetCommandLineArgs();
                    if (!String.IsNullOrEmpty(args[0]))
                        applicationName = Path.GetFileName(args[0])!;
                    else
                    {
                        ProcessModule? mainModule = Process.GetCurrentProcess().MainModule;
                        applicationName = mainModule?.ModuleName
                            ?? Application.ProductName
                            ?? Assembly.GetEntryAssembly()?.GetName().Name
                            ?? String.Empty;
                    }
                }

                return applicationName;
            }
        }

        #endregion
    }
}