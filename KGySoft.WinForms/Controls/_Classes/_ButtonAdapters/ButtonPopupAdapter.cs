#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonPopupAdapter.cs
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
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class ButtonPopupAdapter : ButtonBaseAdapter
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
            ColorData colors = ColorData.Calculate(this, g, state);
            LayoutData layout = PaintPopupLayout(state, false, colors.HighContrast ? 2 : 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            PaintButtonBackground(e, clientRectangle, null);

            if (state.IsDefault)
                clientRectangle.Inflate(-1, -1);
            clientRectangle.Inflate(-1, -1);
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            clientRectangle.Inflate(1, 1);
            DrawDefaultBorder(g, clientRectangle, colors.HighContrast ? colors.WindowText : colors.WindowFrame, state.IsDefault);
            ControlPaint.DrawBorder(g, clientRectangle, colors.HighContrast ? colors.WindowText : colors.ButtonShadow, ButtonBorderStyle.Solid);
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            ColorData colors = ColorData.Calculate(this, e.Graphics, state);
            LayoutData layout = PaintPopupLayout(state, state.CheckState == CheckState.Unchecked, colors.HighContrast ? 2 : 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            if (state.CheckState == CheckState.Indeterminate)
            {
                using Brush brush = CreateDitherBrush(colors.Highlight, state.BackColor);
                PaintButtonBackground(e, clientRectangle, brush);
            }
            else
                ButtonInstance.PaintBackground(e, clientRectangle, colors.ButtonFace, clientRectangle.Location);

            if (state.IsDefault)
                clientRectangle.Inflate(-1, -1);
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            DrawDefaultBorder(g, clientRectangle, colors.HighContrast ? colors.WindowText : colors.ButtonShadow, state.IsDefault);
            if (VisualStyleHelper.HighContrast)
            {
                Pen pen = colors.WindowFrame.GetPen();
                Pen pen2 = colors.Highlight.GetPen();
                Pen pen3 = colors.ButtonShadow.GetPen();
                g.DrawLine(pen, (clientRectangle.Left + 1), (clientRectangle.Top + 1), (clientRectangle.Right - 2), (clientRectangle.Top + 1));
                g.DrawLine(pen, (clientRectangle.Left + 1), (clientRectangle.Top + 1), (clientRectangle.Left + 1), (clientRectangle.Bottom - 2));
                g.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Right, clientRectangle.Bottom - 1);
                g.DrawLine(pen, clientRectangle.Right - 1, clientRectangle.Top, clientRectangle.Right - 1, clientRectangle.Bottom);
                g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Top);
                g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Left, clientRectangle.Bottom);
                g.DrawLine(pen3, (clientRectangle.Left + 1), (clientRectangle.Bottom - 2), (clientRectangle.Right - 2), (clientRectangle.Bottom - 2));
                g.DrawLine(pen3, (clientRectangle.Right - 2), (clientRectangle.Top + 1), (clientRectangle.Right - 2), (clientRectangle.Bottom - 2));
                clientRectangle.Inflate(-2, -2);
            }
            else
                Draw3DLiteBorder(g, clientRectangle, colors, true);
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            ColorData colors = ColorData.Calculate(this, g, state);
            LayoutData layout = PaintPopupLayout(state, state.CheckState == CheckState.Unchecked, 1).Layout(g);
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            if (state.CheckState == CheckState.Indeterminate)
            {
                using Brush brush = CreateDitherBrush(colors.Highlight, state.BackColor);
                PaintButtonBackground(e, clientRectangle, brush);
            }
            else
                ButtonInstance.PaintBackground(e, clientRectangle, colors.ButtonFace, clientRectangle.Location);

            if (state.IsDefault)
                clientRectangle.Inflate(-1, -1);
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            DrawDefaultBorder(g, clientRectangle, colors.HighContrast ? colors.WindowText : colors.ButtonShadow, state.IsDefault);
            if (state.CheckState == CheckState.Unchecked)
                DrawFlatBorder(g, clientRectangle, colors.HighContrast ? colors.WindowText : colors.ButtonShadow);
            else
                Draw3DLiteBorder(g, clientRectangle, colors, false);
        }

        #endregion

        #region Protected Methods

        protected override bool IsHighContrastHighlighted(ControlAppearanceState state)
        {
            bool isUp = !state.Pressed && !state.Hovered;
            return (!isUp || state.CheckState != CheckState.Indeterminate) && base.IsHighContrastHighlighted(state);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state) => PaintPopupLayout(state, false, 0);

        #endregion

        #region Private Methods

        private LayoutOptions PaintPopupLayout(ControlAppearanceState state, bool up, int paintedBorder)
        {
            LayoutOptions options = CommonLayout(state);
            options.BorderSize = paintedBorder;
            options.PaddingSize = 2 - paintedBorder;
            options.HintTextUp = false;
            options.TextOffset = !up;
            options.ShadowedText = VisualStyleHelper.HighContrast;
            return options;
        }

        #endregion

        #endregion
    }
}
