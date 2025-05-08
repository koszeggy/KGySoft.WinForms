#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RadioButtonStandardAdapter.cs
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

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class RadioButtonStandardAdapter : RadioButtonBaseAdapter
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
                ButtonAdapter.PaintDown(e);
            else
                PaintUp(e);
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintOver(e);
            else
                PaintUp(e);
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
                DrawCheckBox(e, layout);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonStandardAdapter(ButtonInstance);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.HintTextUp = false;
            options.DotNetOneButtonCompat = !VisualStyleHelper.RenderWithVisualStyles;
            options.CheckSize = VisualStyleHelper.RenderWithVisualStyles
                ? VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, ButtonInstance, graphics, state.SystemPartId, state.SystemStateId, false).Width
                : options.CheckSize.Scale(options.Scale.X);

            return options;
        }

        #endregion

        #region Private Methods

        private void DrawCheckBox(PaintStateEventArgs e, LayoutData layout)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            Rectangle checkBounds = layout.CheckBounds;
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                if (RadioButtonInstance.VisualsRenderingQuality == RenderingQuality.High
                    && layout.Options.Scale.X > 1f // just to omit querying part size at 100% DPI
                    && layout.Options.CheckSize != VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, true).Width)
                {
                    VisualStyleHelper.RenderScaled(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, layout.CheckBounds);
                }
                else
                    VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, ButtonInstance, g, state.SystemPartId, state.SystemStateId, checkBounds);
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
