using System.Drawing;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    internal class CheckBoxFlatAdapter : CheckBoxBaseAdapter
    {
        // Methods
        internal CheckBoxFlatAdapter(ButtonBase control)
            : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonFlatAdapter(ButtonInstance);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.checkSize = 11;
            options.shadowedText = false;
            return options;
        }

        internal override void PaintDown(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintDown(e);
            }
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                if (state.Enabled)
                {
                    PaintFlatWorker(e, colors.windowText, colors.highlight, colors.windowFrame, colors);
                }
                else
                {
                    PaintFlatWorker(e, colors.windowText/*disabledForeColor*/, colors.buttonFace/*disabledBackColor*/, colors.buttonShadow, colors);
                }
            }
        }

        private void PaintFlatWorker(PaintStateEventArgs e, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors)
        {
            Graphics graphics = e.Graphics;
            ControlAppearanceState state = e.State;
            LayoutData layout = Layout(graphics, state).Layout(graphics);
            PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
            PaintImage(e, layout);
            DrawCheckFlat(e, layout, checkColor, colors.highContrast ? colors.buttonFace : checkBackground, checkBorder, colors, state);
            PaintField(e, layout, colors, true);
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintOver(e);
            }
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                if (state.Enabled)
                {
                    PaintFlatWorker(e, colors.windowText, colors.lowHighlight, colors.windowFrame, colors);
                }
                else
                {
                    PaintFlatWorker(e, colors.windowText/*disabledForeColor*/, colors.buttonFace/*disabledBackColor*/, colors.windowFrame/*disabledForeColor*/, colors);
                }
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintUp(e);
            }
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                if (state.Enabled)
                {
                    PaintFlatWorker(e, colors.windowText, colors.highlight, colors.windowFrame, colors);
                }
                else
                {
                    PaintFlatWorker(e, colors.windowText/*disabledForeColor*/, colors.buttonFace/*disabledBackColor*/, colors.windowFrame/*disabledForeColor*/, colors);
                }
            }
        }

        protected void DrawCheckFlat(PaintEventArgs e, LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors, ControlAppearanceState state)
        {
            Rectangle checkBounds = layout.checkBounds;
            checkBounds.Width--;
            checkBounds.Height--;
            using (Pen pen = new Pen(checkBorder))
            {
                e.Graphics.DrawRectangle(pen, checkBounds);
            }
            checkBounds.Inflate(-1, -1);
            if (state.CheckState == CheckState.Indeterminate)
            {
                checkBounds.Width++;
                checkBounds.Height++;
                DrawDitheredFill(e.Graphics, colors.buttonFace, checkBackground, checkBounds);
            }
            else
            {
                using (Brush brush = new SolidBrush(checkBackground))
                {
                    checkBounds.Width++;
                    checkBounds.Height++;
                    e.Graphics.FillRectangle(brush, checkBounds);
                }
            }

            DrawCheckOnly(e, layout, colors, checkColor, true, state);
        }
    }
}
