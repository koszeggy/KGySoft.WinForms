#region Used namespaces

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class ButtonStandardAdapter : ButtonBaseAdapter
    {
        #region Constructors

        internal ButtonStandardAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static PUSHBUTTONSTATES DetermineState(bool up, ControlAppearanceState state)
        {
            if (!up)
                return PUSHBUTTONSTATES.PBS_PRESSED;

            PUSHBUTTONSTATES result = (PUSHBUTTONSTATES)state.SystemStateId;
            if (result == PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING && !WindowsUtils.IsVistaOrLater)
                result = PUSHBUTTONSTATES.PBS_DEFAULTED;

            return result;
        }

        private static void Draw3DBorderHighContrastRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            Pen penTopLeft = null;
            Pen penBottomRight = null;
            Pen insetPen = null;
            Pen bottomRightInsetPen = null;

            try
            {
                // top + left
                penTopLeft = stockColor ? /*SystemPens.ControlLightLight*/ new Pen(SystemColors.ControlLightLight) : new Pen(colors.highlight);
                g.DrawLine(penTopLeft, p1, p2); // top  (right-left)
                g.DrawLine(penTopLeft, p2, p3); // left (up-down)

                // bottom + right
                penBottomRight = stockColor ? new Pen(SystemColors.ControlDarkDark) : new Pen(colors.buttonShadowDark);

                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(penBottomRight, p3, p4);  // bottom (left-right)
                g.DrawLine(penBottomRight, p4, p1);  // right  (bottom-up )

                // Draw inset
                if (stockColor)
                {
                    insetPen = SystemInformation.HighContrast ? new Pen(SystemColors.ControlLight) : new Pen(SystemColors.Control);
                }
                else
                {
                    insetPen = SystemInformation.HighContrast ? new Pen(colors.highlight) : new Pen(colors.buttonFace);
                }

                p1.Offset(-1, 2);
                p2.Offset(1, 1);
                p3.Offset(1, -1);
                p4.Offset(-1, -1);

                // top + left inset
                g.DrawLine(insetPen, p1, p2); // top (right-left)
                g.DrawLine(insetPen, p2, p3); // left( up-down)

                // Bottom + right inset

                bottomRightInsetPen = stockColor ? new Pen(SystemColors.ControlDark) : new Pen(colors.buttonShadow);
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(bottomRightInsetPen, p3, p4); // bottom (left-right)
                g.DrawLine(bottomRightInsetPen, p4, p1); // right  (bottom-up)
            }
            finally
            {
                if (penTopLeft != null)
                {
                    penTopLeft.Dispose();
                }

                if (penBottomRight != null)
                {
                    penBottomRight.Dispose();
                }

                if (insetPen != null)
                {
                    insetPen.Dispose();
                }

                if (bottomRightInsetPen != null)
                {
                    bottomRightInsetPen.Dispose();
                }
            }
        }

        private static void Draw3DBorderNormal(Graphics g, ref Rectangle bounds, ColorData colors)
        {

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // top + left
            Pen pen = new Pen(colors.buttonShadowDark);
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right
            pen = new Pen(colors.highlight);
            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4); // bottom(left-right)
                g.DrawLine(pen, p4, p1); // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }

            // Draw inset

            pen = new Pen(colors.buttonFace);

            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            // top + left inset
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right inset
            if (colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor())
            {
                pen = new Pen(SystemColors.ControlLight);
            }
            else
            {
                pen = new Pen(colors.buttonFace);
            }

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4); // bottom(left-right)
                g.DrawLine(pen, p4, p1); // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }
        }

        private static void Draw3DBorderRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // Draw counter-clock-wise.

            // top + left
            Pen pen = stockColor ? new Pen(SystemColors.ControlLightLight) : new Pen(colors.highlight);

            try
            {
                g.DrawLine(pen, p1, p2);   // top (right-left)
                g.DrawLine(pen, p2, p3);   // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right
            pen = stockColor ? new Pen(SystemColors.ControlDarkDark) : new Pen(colors.buttonShadowDark);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4);    // bottom(left-right)
                g.DrawLine(pen, p4, p1);    // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }

            // Draw inset
            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            if (stockColor)
            {
                pen = SystemInformation.HighContrast ? new Pen(SystemColors.ControlLight) : new Pen(SystemColors.Control);
            }
            else
            {
                pen = new Pen(colors.buttonFace);
            }

            // top + left inset
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // Bottom + right inset
            pen = stockColor ? new Pen(SystemColors.ControlDark) : new Pen(colors.buttonShadow);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4);  // bottom(left-right)
                g.DrawLine(pen, p4, p1);  // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }
        }

        private static void Draw3DBorder(Graphics g, Rectangle bounds, ColorData colors, bool raised, ControlAppearanceState state)
        {
            if (state.BackColor != SystemColors.Control && SystemInformation.HighContrast)
            {
                if (raised)
                {
                    Draw3DBorderHighContrastRaised(g, ref bounds, colors);
                }
                else
                {
                    ControlPaint.DrawBorder(g, bounds, ControlPaint.Dark(state.BackColor), ButtonBorderStyle.Solid);
                }
            }
            else
            {
                if (raised)
                {
                    Draw3DBorderRaised(g, ref bounds, colors);
                }
                else
                {
                    Draw3DBorderNormal(g, ref bounds, colors);
                }
            }
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e)
        {
            PaintWorker(e, false);
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            PaintUp(e);
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            PaintWorker(e, true);
        }

        #endregion

        #region Protected Methods

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            return PaintLayout(state, false);
        }

        #endregion

        #region Private Methods

        private LayoutOptions PaintLayout(ControlAppearanceState state, bool up)
        {
            LayoutOptions options = CommonLayout(state);
            options.textOffset = !up;
            options.everettButtonCompat = !Application.RenderWithVisualStyles;
            return options;
        }

        private void PaintThemedButtonBackground(PaintStateEventArgs e, Rectangle bounds, bool up)
        {
            ControlAppearanceState state = e.State;
            PUSHBUTTONSTATES buttonState = DetermineState(up, state);
            if (ButtonRenderer.IsBackgroundPartiallyTransparent((PushButtonState)buttonState))
            {
                ButtonRenderer.DrawParentBackground(e.Graphics, bounds, ButtonInstance);
            }

            ButtonRenderer.DrawButton(e.Graphics, ButtonInstance.ClientRectangle, false, (PushButtonState)buttonState);
            bounds.Inflate(-ButtonBorderSize, -ButtonBorderSize);
            if (!ButtonInstance.UseVisualStyleBackColor)
            {
                Color backColor = state.BackColor;
                if (backColor.A > 0)
                {
                    if (backColor.A == 0xff)
                    {
                        backColor = e.Graphics.GetNearestColor(backColor);
                    }
                    using (Brush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                    }
                }
            }

            if (ButtonInstance.BackgroundImage != null && !SystemInformation.HighContrast)
                e.Graphics.DrawBackgroundImage(ButtonInstance.BackgroundImage, Color.Transparent, ButtonInstance.BackgroundImageLayout, ButtonInstance.ClientRectangle, bounds, ButtonInstance.DisplayRectangle.Location, ButtonInstance.RightToLeft);
        }

        private void PaintWorker(PaintStateEventArgs e, bool up)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            up = up && (state.CheckState == CheckState.Unchecked);
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = Application.RenderWithVisualStyles ? PaintLayout(state, true).Layout(g) : PaintLayout(state, up).Layout(g);
            if (Application.RenderWithVisualStyles)
            {
                PaintThemedButtonBackground(e, ButtonInstance.ClientRectangle, up);
            }
            else
            {
                Rectangle clientRectangle = ButtonInstance.ClientRectangle;
                if (up)
                    clientRectangle.Inflate(-2, -2);
                else
                    clientRectangle.Inflate(-1, -1);

                if (state.CheckState == CheckState.Indeterminate)
                {
                    using (Brush brush = CreateDitherBrush(colors.highlight, colors.buttonFace))
                    {
                        e.Graphics.FillRectangle(brush, clientRectangle);
                    }
                }
                else
                {
                    PaintButtonBackground(e, clientRectangle, colors.buttonFace);
                }
            }
            PaintImage(e, layout);
            if (Application.RenderWithVisualStyles)
            {
                layout.focus.Inflate(1, 1);
            }
            PaintField(e, layout, colors, true);
            if (!Application.RenderWithVisualStyles)
            {
                Rectangle r = ButtonInstance.ClientRectangle;
                if (state.IsDefault)
                {
                    r.Inflate(-1, -1);
                }
                DrawDefaultBorder(g, r, colors.windowFrame, state.IsDefault);
                if (up)
                {
                    Draw3DBorder(g, r, colors, true, state);
                }
                else
                {
                    ControlPaint.DrawBorder(g, r, colors.buttonShadow, ButtonBorderStyle.Solid);
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
