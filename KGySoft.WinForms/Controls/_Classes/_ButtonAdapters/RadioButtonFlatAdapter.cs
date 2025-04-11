#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class RadioButtonFlatAdapter: RadioButtonBaseAdapter
    {
        #region Constants

        private const int flatCheckSize = 12;

        #endregion

        #region Constructors

        internal RadioButtonFlatAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

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
                    PaintFlatWorker(e, colors.windowText, colors.buttonFace, colors.buttonShadow, colors);
                }
            }
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
                    PaintFlatWorker(e, colors.windowText, colors.buttonFace, colors.windowFrame, colors);
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
                    PaintFlatWorker(e, colors.windowText, colors.buttonFace, colors.windowFrame, colors);
                }
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonFlatAdapter(ButtonInstance);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.checkSize = (int)(flatCheckSize * GetDpiScaleRatio());
            options.shadowedText = false;
            return options;
        }

        #endregion

        #region Private Methods

        private void PaintFlatWorker(PaintStateEventArgs e, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors)
        {
            ControlAppearanceState state = e.State;
            LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
            PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
            PaintImage(e, layout);
            DrawCheckFlat(e, layout, checkColor, colors.highContrast ? colors.buttonFace : checkBackground, checkBorder);
            PaintField(e, layout, colors, true);
        }

        private void DrawCheckFlat(PaintStateEventArgs e, LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder)
        {
            DrawCheckBackgroundFlat(e, layout.checkBounds, checkBorder, checkBackground, true);
            DrawCheckOnly(e, layout, checkColor);
        }

        #endregion

        #endregion
    }
}
