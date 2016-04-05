using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace KGySoft.Controls.Design
{
    sealed class AdvancedPanelDesigner: ScrollableControlDesigner
    {
        // Methods
        public AdvancedPanelDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private void DrawBorder(Graphics graphics)
        {
            AdvancedPanel component = (AdvancedPanel)base.Component;
            if ((component != null) && component.Visible)
            {
                Pen borderPen = this.BorderPen;
                Rectangle clientRectangle = this.Control.ClientRectangle;
                clientRectangle.Width--;
                clientRectangle.Height--;
                graphics.DrawRectangle(borderPen, clientRectangle);
                borderPen.Dispose();
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            AdvancedPanel component = (AdvancedPanel)base.Component;
            if (component.BorderStyle == AdvancedBorderStyle.None)
            {
                this.DrawBorder(pe.Graphics);
            }
            base.OnPaintAdornments(pe);
        }

        // Properties
        private Pen BorderPen
        {
            get
            {
                return new Pen((this.Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(this.Control.BackColor) : ControlPaint.Dark(this.Control.BackColor)) { DashStyle = DashStyle.Dash };
            }
        }

    }
}
