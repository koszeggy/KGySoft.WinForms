#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScaleHelper.cs
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

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    ///  Helper class for scaling.
    /// </summary>
    internal static class ScaleHelper
    {
        #region Constants

        internal const float OneHundredPercentLogicalDpi = 96f;

        #endregion

        #region Properties

        /// <summary>
        ///  Returns a boolean to specify if we should enable processing of WM_DPICHANGED and related messages
        /// </summary>
        internal static bool IsThreadPerMonitorV2Aware => false; // TODO: see https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms.Primitives/src/System/Windows/Forms/Internals/ScaleHelper.cs

        internal static PointF SystemScale => GetScale(IntPtr.Zero);

        internal static PointF SystemDpi => GetDpiForHwnd(IntPtr.Zero);

        #endregion

        #region Methods

        #region Internal Methods

        internal static PointF GetScale(IntPtr handle)
        {
            var dpi = GetDpiForHwnd(handle);
            return new PointF(dpi.X / OneHundredPercentLogicalDpi, dpi.Y / OneHundredPercentLogicalDpi);
        }

        internal static PointF GetScale(this Control control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            return GetScale(control.Handle);
        }

        internal static int PerMonitorScale(this Control control, int value)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

#if NET47_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if (IsThreadPerMonitorV2Aware)
                return control.LogicalToDeviceUnits(value);
#endif

            return control.IsHandleCreated
                ? (int)(value * control.GetScale().X)
                : (int)(value * SystemScale.X);
        }

        internal static Size ScaleSize(this Control control, Size size) => size.Scale(control.GetScale());

        internal static SizeF ScaleF(this Size size, PointF scale) =>
            new SizeF(scale.X * size.Width, scale.Y * size.Height);

        internal static Size Scale(this Size size, PointF scale) =>
            Size.Round(ScaleF(size, scale));

        #endregion

        #region Private Methods

        private static PointF GetDpiForHwnd(IntPtr handle)
        {
            using Graphics screen = Graphics.FromHwnd(handle);
            return new PointF(screen.DpiX, screen.DpiY);
        }

        #endregion

        #endregion
    }
}
