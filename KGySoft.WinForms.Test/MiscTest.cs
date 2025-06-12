using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
#if !NET35
using System.Runtime.Versioning;
#endif
using KGySoft.WinForms.Test.Forms;
using Microsoft.Win32;

#nullable enable
namespace KGySoft.WinForms.Test
{
    internal partial class MiscTest : ControlsTestBaseForm
    {
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

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            AddLine($"{e.Category}");
            AddLine($"Current Scale: {this.GetScale()}");
#if NET47_OR_GREATER || NETCOREAPP
            AddLine($"DeviceDpi: {DeviceDpi}");
#endif
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

        private void AddLine(string s, [CallerMemberName]string? caller = null)
        {
            textBox1.AppendText($"{caller}: {s}{Environment.NewLine}");
        }
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
