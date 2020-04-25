#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class ButtonPopupAdapter: ButtonBaseAdapter
    {
        #region Constructors

        internal ButtonPopupAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = PaintPopupLayout(state, false, SystemInformation.HighContrast ? 2 : 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            PaintButtonBackground(e, clientRectangle, colors.buttonFace);
            if (state.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            clientRectangle.Inflate(-1, -1);
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            clientRectangle.Inflate(1, 1);
            DrawDefaultBorder(g, clientRectangle, colors.highContrast ? colors.windowText : colors.windowFrame, state.IsDefault);
            ControlPaint.DrawBorder(g, clientRectangle, colors.highContrast ? colors.windowText : colors.buttonShadow, ButtonBorderStyle.Solid);
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = PaintPopupLayout(state, state.CheckState == CheckState.Unchecked, SystemInformation.HighContrast ? 2 : 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
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
            if (state.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            DrawDefaultBorder(g, clientRectangle, colors.highContrast ? colors.windowText : colors.buttonShadow, state.IsDefault);
            if (SystemInformation.HighContrast)
            {
                using (Pen pen = new Pen(colors.windowFrame))
                {
                    using (Pen pen2 = new Pen(colors.highlight))
                    {
                        using (Pen pen3 = new Pen(colors.buttonShadow))
                        {
                            g.DrawLine(pen, (clientRectangle.Left + 1), (clientRectangle.Top + 1), (clientRectangle.Right - 2), (clientRectangle.Top + 1));
                            g.DrawLine(pen, (clientRectangle.Left + 1), (clientRectangle.Top + 1), (clientRectangle.Left + 1), (clientRectangle.Bottom - 2));
                            g.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Right, clientRectangle.Bottom - 1);
                            g.DrawLine(pen, clientRectangle.Right - 1, clientRectangle.Top, clientRectangle.Right - 1, clientRectangle.Bottom);
                            g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Top);
                            g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Left, clientRectangle.Bottom);
                            g.DrawLine(pen3, (clientRectangle.Left + 1), (clientRectangle.Bottom - 2), (clientRectangle.Right - 2), (clientRectangle.Bottom - 2));
                            g.DrawLine(pen3, (clientRectangle.Right - 2), (clientRectangle.Top + 1), (clientRectangle.Right - 2), (clientRectangle.Bottom - 2));
                        }
                    }
                }
                clientRectangle.Inflate(-2, -2);
            }
            else
            {
                Draw3DLiteBorder(g, clientRectangle, colors, true);
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = PaintPopupLayout(state, state.CheckState == CheckState.Unchecked, 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
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

            if (state.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            DrawDefaultBorder(g, clientRectangle, colors.highContrast ? colors.windowText : colors.buttonShadow, state.IsDefault);
            if (state.CheckState == CheckState.Unchecked)
            {
                DrawFlatBorder(g, clientRectangle, colors.highContrast ? colors.windowText : colors.buttonShadow);
            }
            else
            {
                Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
        }

        #endregion

        #region Protected Methods

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            return PaintPopupLayout(state, false, 0);
        }

        #endregion

        #region Private Methods

        private LayoutOptions PaintPopupLayout(ControlAppearanceState state, bool up, int paintedBorder)
        {
            LayoutOptions options = CommonLayout(state);
            options.borderSize = paintedBorder;
            options.paddingSize = 2 - paintedBorder;
            options.hintTextUp = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        #endregion

        #endregion
    }
}
