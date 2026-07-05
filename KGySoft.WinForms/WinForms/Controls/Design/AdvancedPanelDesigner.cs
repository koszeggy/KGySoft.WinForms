#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedPanelDesigner.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

#endregion

namespace KGySoft.WinForms.Controls.Design
{
    internal sealed class AdvancedPanelDesigner : ScrollableControlDesigner
    {
        #region Properties

        private Pen BorderPen => new((Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(Control.BackColor) : ControlPaint.Dark(Control.BackColor)) { DashStyle = DashStyle.Dash };

        #endregion

        #region Constructors

        public AdvancedPanelDesigner() => AutoResizeHandles = true;

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            AdvancedPanel component = (AdvancedPanel)Component;
            if (component.BorderStyle == AdvancedBorderStyle.None)
                DrawBorder(pe.Graphics);
            base.OnPaintAdornments(pe);
        }

        #endregion

        #region Private Methods

        private void DrawBorder(Graphics graphics)
        {
            if (Component is AdvancedPanel { Visible: true })
            {
                Pen borderPen = BorderPen;
                Rectangle clientRectangle = Control.ClientRectangle;
                clientRectangle.Width--;
                clientRectangle.Height--;
                graphics.DrawRectangle(borderPen, clientRectangle);
                borderPen.Dispose();
            }
        }

        #endregion

        #endregion
    }
}
