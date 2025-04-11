#region Used namespaces

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class RadioButtonPopupAdapter: RadioButtonFlatAdapter
    {
        #region Constructors

        internal RadioButtonPopupAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void DrawCheckBackground3DLite(PaintStateEventArgs e, Rectangle bounds, Color checkBackground, ColorData colors, bool disabledColors)
        {
            Graphics graphics = e.Graphics;
            GraphicsState? prevState = graphics.DpiX / ScaleHelper.OneHundredPercentLogicalDpi > 1.1f ? graphics.Save() : null;
            if (prevState != null)
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

            ControlAppearanceState state = e.State;
            Color backColor = checkBackground;
            if (!state.Enabled && disabledColors)
                backColor = state.BackColor;

            using Brush brush = new SolidBrush(backColor);
            using Pen pen = new Pen(colors.buttonShadow);
            using Pen pen2 = new Pen(colors.buttonFace);
            using Pen pen3 = new Pen(colors.highlight);
            bounds.Width--;
            bounds.Height--;
            graphics.DrawPie(pen, bounds, 136f, 88f);
            graphics.DrawPie(pen, bounds, 226f, 88f);
            graphics.DrawPie(pen3, bounds, 316f, 88f);
            graphics.DrawPie(pen3, bounds, 46f, 88f);
            bounds.Inflate(-1, -1);
            graphics.FillEllipse(brush, bounds);
            graphics.DrawEllipse(pen2, bounds);

            if (prevState != null)
                graphics.Restore(prevState);
        }

        #endregion

        #region Instance Methods

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
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBackground3DLite(e, layout.checkBounds, colors.highlight, colors, true);
                DrawCheckOnly(e, layout, colors.buttonShadow);
                PaintField(e, layout, colors, true);
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
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBackground3DLite(e, layout.checkBounds, colors.highContrast ? colors.buttonFace : colors.highlight, colors, true);
                DrawCheckOnly(e, layout, colors.windowText);
                PaintField(e, layout, colors, true);
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
                DrawCheckBackgroundFlat(e, layout.checkBounds, colors.buttonShadow, colors.highContrast ? colors.buttonFace : colors.highlight, true);
                DrawCheckOnly(e, layout, colors.windowText);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonPopupAdapter(ButtonInstance);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = base.Layout(graphics, state);
            if (!state.Pressed && !state.Hovered)
            {
                options.shadowedText = true;
            }
            return options;
        }

        #endregion

        #endregion

        #endregion
    }
}
