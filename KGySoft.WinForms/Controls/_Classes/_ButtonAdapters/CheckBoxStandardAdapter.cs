#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckBoxStandardAdapter.cs
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

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal sealed class CheckBoxStandardAdapter : CheckBoxBaseAdapter
    {
        #region Constructors

        internal CheckBoxStandardAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
            => IsButton
                ? ButtonAdapter.GetPreferredSizeCore(g, proposedSize, state)
                : Layout(g, state).GetPreferredSizeCore(g, proposedSize);

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
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(g, state.BackColor, state.ForeColor);
                LayoutData layout = Layout(g, state).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.ButtonFace);

                if (!layout.Options.DotNetOneButtonCompat)
                    layout.TextBounds.Offset(-1, -1);

                layout.ImageBounds.Offset(-1, -1);
                AdjustFocusRectangle(state, layout);

                if (!String.IsNullOrEmpty(state.Text))
                {
                    // Minor adjustment to make sure the appearance is exactly the same as Win32 app.
                    int focusRectFixup = layout.Focus.X & 0x1; // if it's odd, subtract one pixel for fixup.
                    if (!VisualStyleHelper.RenderWithVisualStyles)
                        focusRectFixup = 1 - focusRectFixup;

                    layout.Focus.Offset(-(focusRectFixup + 1), -2);
                    layout.Focus.Width = layout.TextBounds.Width + layout.ImageBounds.Width - 1;
                    layout.Focus.Intersect(layout.TextBounds);

                    if (!layout.Options.TextAlign.AnyLeft()
                        && layout.Options.UseCompatibleTextRendering
                        && layout.Options.Font.Italic)
                    {
                        // Fixup for GDI+ text rendering.
                        layout.Focus.Width += 2;
                    }
                }

                PaintImage(e, layout);
                DrawCheckBox(e, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonStandardAdapter(ButtonInstance);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.CheckPaddingSize = 1;
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
            ControlAppearanceState state = e.State;
            if (VisualStyleHelper.RenderWithVisualStyles)
            {
                if (CheckBoxInstance.VisualsRenderingQuality == RenderingQuality.High
                    && layout.Options.Scale.X > 1f // just to omit querying part size at 100% DPI
                    && layout.Options.CheckSize != VisualStyleHelper.GetPartSize(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, true).Width)
                {
                    VisualStyleHelper.RenderScaled(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, layout.CheckBounds);
                }
                else
                    VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, layout.CheckBounds);
            }
            else if (state.CheckState == CheckState.Indeterminate)
                ControlPaint.DrawMixedCheckBox(e.Graphics, layout.CheckBounds, GetButtonState(state));
            else
                ControlPaint.DrawCheckBox(e.Graphics, layout.CheckBounds, GetButtonState(state));
        }

        #endregion

        #endregion
    }
}
