#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckBoxFlatAdapter.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    internal class CheckBoxFlatAdapter : CheckBoxBaseAdapter
    {
        #region Constructors

        internal CheckBoxFlatAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        protected static void DrawCheckFlat(PaintEventArgs e, LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors, ControlAppearanceState state)
        {
            Rectangle checkBounds = layout.CheckBounds;
            checkBounds.Width--;
            checkBounds.Height--;
            e.Graphics.DrawRectangle(checkBorder.GetPen(), checkBounds);

            checkBounds.Inflate(-1, -1);
            checkBounds.Width++;
            checkBounds.Height++;
            if (state.CheckState == CheckState.Indeterminate)
                DrawDitheredFill(e.Graphics, colors.ButtonFace, checkBackground, checkBounds);
            else
                e.Graphics.FillRectangle(checkBackground.GetBrush(), checkBounds);

            checkBounds = layout.CheckOnlyBounds;
            checkBounds.Width++;
            checkBounds.Height++;
            DrawCheckOnly(e.Graphics, checkBounds, colors, checkColor, true, state);
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintDown(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.Highlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText /*disabledForeColor*/, colors.ButtonFace /*disabledBackColor*/, colors.ButtonShadow, colors);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintOver(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.LowHighlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText /*disabledForeColor*/, colors.ButtonFace /*disabledBackColor*/, colors.WindowFrame /*disabledForeColor*/, colors);
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintUp(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                if (state.Enabled)
                    PaintFlatWorker(e, colors.WindowText, colors.Highlight, colors.WindowFrame, colors);
                else
                    PaintFlatWorker(e, colors.WindowText /*disabledForeColor*/, colors.ButtonFace /*disabledBackColor*/, colors.WindowFrame /*disabledForeColor*/, colors);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonFlatAdapter(ButtonInstance);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.CheckSize = FlatCheckSize.Scale(options.Scale.X);
            options.ShadowedText = false;
            return options;
        }

        #endregion

        #region Private Methods

        private void PaintFlatWorker(PaintStateEventArgs e, Color checkColor, Color checkBackground, Color checkBorder, ColorData colors)
        {
            Graphics graphics = e.Graphics;
            ControlAppearanceState state = e.State;
            LayoutData layout = Layout(graphics, state).Layout(graphics);
            PaintButtonBackground(e, ButtonInstance.ClientRectangle, null);
            PaintImage(e, layout);
            DrawCheckFlat(e, layout, checkColor, colors.HighContrast ? colors.ButtonFace : checkBackground, checkBorder, colors, state);
            AdjustFocusRectangle(state, layout);
            PaintField(e, layout, colors, true);
        }

        #endregion

        #endregion

        #endregion
    }
}
