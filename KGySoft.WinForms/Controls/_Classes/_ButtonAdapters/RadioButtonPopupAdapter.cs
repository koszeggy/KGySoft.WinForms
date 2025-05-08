#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RadioButtonPopupAdapter.cs
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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class RadioButtonPopupAdapter : RadioButtonFlatAdapter
    {
        #region Constructors

        internal RadioButtonPopupAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintDown(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.ButtonFace);
                PaintImage(e, layout);
                DrawCheckBackground3DLite(e, layout.CheckBounds, layout.Options.Scale, colors.Highlight, colors, true);
                DrawCheckOnly(e, layout, colors.ButtonShadow);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintOver(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.ButtonFace);
                PaintImage(e, layout);
                DrawCheckBackground3DLite(e, layout.CheckBounds, layout.Options.Scale, colors.HighContrast ? colors.ButtonFace : colors.Highlight, colors, true);
                DrawCheckOnly(e, layout, colors.WindowText);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintUp(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.ButtonFace);
                PaintImage(e, layout);
                DrawCheckBackgroundFlat(e, layout.CheckBounds, layout.Options.Scale, colors.ButtonShadow, colors.HighContrast ? colors.ButtonFace : colors.Highlight, true);
                DrawCheckOnly(e, layout, colors.WindowText);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonPopupAdapter(ButtonInstance);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = base.Layout(graphics, state);
            if (!state.Pressed && !state.Hovered)
                options.ShadowedText = true;
            return options;
        }

        #endregion

        #region Private Methods

        private void DrawCheckBackground3DLite(PaintStateEventArgs e, Rectangle bounds, PointF scale, Color checkBackground, ColorData colors, bool disabledColors)
        {
            Graphics graphics = e.Graphics;
            GraphicsState? prevState = null;
            if (RadioButtonInstance.VisualsRenderingQuality == RenderingQuality.High)
            {
                prevState = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            ControlAppearanceState state = e.State;
            Color backColor = checkBackground;
            if (!state.Enabled && disabledColors)
                backColor = state.BackColor;

            using Brush brush = new SolidBrush(backColor);
            using Pen pen = new Pen(colors.ButtonShadow);
            using Pen pen2 = new Pen(colors.ButtonFace);
            using Pen pen3 = new Pen(colors.Highlight);
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

        #endregion
    }
}
