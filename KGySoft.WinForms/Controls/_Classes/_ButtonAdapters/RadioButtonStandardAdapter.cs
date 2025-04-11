#region Used namespaces

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class RadioButtonStandardAdapter: RadioButtonBaseAdapter
    {
        #region Constructors

        internal RadioButtonStandardAdapter(ButtonBase control)
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
                PaintUp(e);
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
                PaintUp(e);
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
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBox(e, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonStandardAdapter(ButtonInstance);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.hintTextUp = false;
            options.dotNetOneButtonCompat = !Application.RenderWithVisualStyles;
            if (Application.RenderWithVisualStyles)
            {
                //using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                //{
                    options.checkSize = RadioButtonRenderer.GetGlyphSize(graphics, (RadioButtonState)state.SystemStateId).Width;
                //}
            }
            else
            {
                options.checkSize = (int)(options.checkSize * GetDpiScaleRatio());
            }
            return options;
        }

        #endregion

        #region Private Methods

        private void DrawCheckBox(PaintStateEventArgs e, LayoutData layout)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            Rectangle checkBounds = layout.checkBounds;
            if (Application.RenderWithVisualStyles)
            {
                RadioButtonRenderer.DrawRadioButton(g, new Point(checkBounds.Left, checkBounds.Top), (RadioButtonState)state.SystemStateId);
            }
            else
            {
                checkBounds.X--;
                ControlPaint.DrawRadioButton(g, checkBounds, GetButtonState(state));
            }
        }

        #endregion

        #endregion
    }
}
