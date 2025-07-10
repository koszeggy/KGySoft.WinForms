#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MiscTest.cs
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
using System.Runtime.CompilerServices;
#if !NET35
using System.Runtime.Versioning;
#endif
using System.Windows.Forms;

using Microsoft.Win32;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class MiscTest : ControlsTestBaseForm
    {
        #region Constructors

        public MiscTest()
        {
            InitializeComponent();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
#if NET35
            const string frameworkName = ".NET Framework 3.5";
#else
            TargetFrameworkAttribute attr = (TargetFrameworkAttribute)Attribute.GetCustomAttribute(GetType().Assembly, typeof(TargetFrameworkAttribute))!;
            string frameworkName = attr.FrameworkDisplayName is { Length: > 0 } name ? name : attr.FrameworkName;
#endif

            AddLine(frameworkName);
#if NET47_OR_GREATER || NETCOREAPP
            AddLine($"DeviceDpi: {DeviceDpi}");
#endif

            AddLine($"SystemScale: {ScaleHelper.SystemScale}");
            AddLine($"Per-monitor awareness version: {ScaleHelper.PerMonitorDpiAwarenessVersion}");
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DPICHANGED = 0x02E0;
            base.WndProc(ref m);
            if (m.Msg == WM_DPICHANGED)
            {
                int dpi = m.WParam.ToInt32() & 0xFFFF;
#if NET47_OR_GREATER || NETCOREAPP
                AddLine($"DPI by DeviceDpi: {DeviceDpi}");
#endif

                AddLine($"DPI by WM_DPICHANGED: {dpi}");
                AddLine($"Current Scale: {this.GetScale()}");
            }
        }

        #endregion

        #region Private Methods

        private void AddLine(string s, [CallerMemberName]string? caller = null)
        {
            textBox1.AppendText($"{caller}: {s}{Environment.NewLine}");
        }

        #endregion

        #region Event handlers

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            AddLine($"{e.Category}");
            AddLine($"Current Scale: {this.GetScale()}");
#if NET47_OR_GREATER || NETCOREAPP
            AddLine($"DeviceDpi: {DeviceDpi}");
#endif

        }

        #endregion

        #endregion
    }
}

#if NET35 || NET40
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class CallerMemberNameAttribute : Attribute
    {
    }
}
#endif
