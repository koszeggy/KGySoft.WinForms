#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedPanel.cs
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
using System.ComponentModel;
using System.Windows.Forms;

using KGySoft.WinForms.Controls.Design;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents an advanced panel with much more flexible <see cref="BorderStyle"/> than original <see cref="Panel"/>
    /// </summary>
    [Designer(typeof(AdvancedPanelDesigner))]
    public class AdvancedPanel : Panel, ISafePaintBackground
    {
        #region Fields

        private AdvancedBorderStyle borderStyle = AdvancedBorderStyle.Raised;
        private int borderWidth = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the border style of the <see cref="AdvancedPanel"/> panel.
        /// </summary>
        [Category("AdvancedPanel")]
        [Description("Gets or sets the border style of the AdvancedPanel.")]
        [DefaultValue(typeof(AdvancedBorderStyle), "Raised")]
        public new AdvancedBorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                if (borderStyle != value)
                {
                    borderStyle = value;
                    switch (value)
                    {
                        case AdvancedBorderStyle.None:
                            borderWidth = 0;
                            break;
                        case AdvancedBorderStyle.FixedSingle:
                        case AdvancedBorderStyle.Raised:
                        case AdvancedBorderStyle.Sunken:
                            borderWidth = 1;
                            break;
                        case AdvancedBorderStyle.Flat:
                        case AdvancedBorderStyle.RaisedHigh:
                        case AdvancedBorderStyle.SunkenLow:
                        case AdvancedBorderStyle.RaisedFrame:
                        case AdvancedBorderStyle.SunkenFrame:
                            borderWidth = 2;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    NCHelper.InvalidateNC(Handle);
                }
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_NCCALCSIZE:
                    if (m.WParam == IntPtr.Zero || m.WParam == new IntPtr(1))
                    {
                        NCHelper.CalcSizeNC(m.LParam, borderWidth);
                    }
                    break;

                case Constants.WM_NCPAINT:
                    NCHelper.DrawBorderNC(m.HWnd, Size, borderStyle);
                    break;
            }
            base.WndProc(ref m);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            NCHelper.InvalidateNC(Handle);
        }

#if NETCOREAPP && !NET10_0_OR_GREATER
        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // workaround for https://github.com/dotnet/winforms/issues/13784
            base.OnPaintBackground(e);
            e.Graphics.GetHdc();
            e.Graphics.ReleaseHdc();
        }
#endif

        #endregion
    }
}
