#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RadioButtonFlatAdapter.cs
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
    internal class RadioButtonFlatAdapter : RadioButtonBaseAdapter
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
                ButtonAdapter.PaintDown(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.Highlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText, colors.ButtonFace, colors.ButtonShadow, colors);
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
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.LowHighlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText, colors.ButtonFace, colors.WindowFrame, colors);
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
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.Highlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText, colors.ButtonFace, colors.WindowFrame, colors);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonFlatAdapter(ButtonInstance);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.CheckSize = (int)(flatCheckSize * GetDpiScaleRatio());
            options.ShadowedText = false;
            return options;
        }

        #endregion

        #region Private Methods

        private void PaintFlatWorker(PaintStateEventArgs e, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors)
        {
            ControlAppearanceState state = e.State;
            LayoutData layout = Layout(e.Graphics, state).Layout(e.Graphics);
            PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.ButtonFace);
            PaintImage(e, layout);
            DrawCheckFlat(e, layout, checkColor, colors.HighContrast ? colors.ButtonFace : checkBackground, checkBorder);
            AdjustFocusRectangle(state, layout);
            PaintField(e, layout, colors, true);
        }

        private void DrawCheckFlat(PaintStateEventArgs e, LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder)
        {
            DrawCheckBackgroundFlat(e, layout.CheckBounds, checkBorder, checkBackground, true);
            DrawCheckOnly(e, layout, checkColor);
        }

        #endregion

        #endregion
    }
}
