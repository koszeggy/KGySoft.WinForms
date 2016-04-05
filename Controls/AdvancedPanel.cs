using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using KGySoft.Controls.Design;
using KGySoft.Controls.WinApi;

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents an advanced panel with much more flexible <see cref="BorderStyle"/> than original <see cref="Panel"/>
    /// </summary>
    [Designer(typeof(AdvancedPanelDesigner))]
    public class AdvancedPanel: Panel
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
        new public AdvancedBorderStyle BorderStyle
        {
            get { return borderStyle; }
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
                            throw new ArgumentOutOfRangeException("value");
                    }

                    NCHelper.InvalidateNC(Handle);
                }
            }
        }

        #endregion

        #region Methods

        #region Protected Methods

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

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            NCHelper.InvalidateNC(Handle);
        }

        #endregion

        #endregion
    }
}
