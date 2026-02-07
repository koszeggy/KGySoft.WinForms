#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckBoxPopupAdapter.cs
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

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class CheckBoxPopupAdapter : CheckBoxBaseAdapter
    {
        #region Constructors

        internal CheckBoxPopupAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void DrawCheckBackground(PaintStateEventArgs e, Rectangle bounds, Color checkBackground, bool disabledColors, ColorData colors)
        {
            ControlAppearanceState state = e.State;
            if (state.CheckState == CheckState.Indeterminate)
                DrawDitheredFill(e.Graphics, colors.ButtonFace, checkBackground, bounds);
            else
                DrawCheckBackground(state.Enabled, state.CheckState, e.Graphics, bounds, checkBackground, disabledColors);
        }

        private static void DrawCheckBackground(bool controlEnabled, CheckState controlCheckState, Graphics g, Rectangle bounds, Color checkBackground, bool disabledColors)
        {
            Brush brush;
            if (!controlEnabled && disabledColors)
                brush = SystemBrushes.Control;
            else if (((controlCheckState == CheckState.Indeterminate) && (checkBackground == SystemColors.Window)) && disabledColors)
            {
                Color color = VisualStyleHelper.HighContrast ? SystemColors.ControlDark : SystemColors.Control;
                byte red = (byte)((color.R + SystemColors.Window.R) / 2);
                byte green = (byte)((color.G + SystemColors.Window.G) / 2);
                byte blue = (byte)((color.B + SystemColors.Window.B) / 2);
                brush = Color.FromArgb(red, green, blue).GetBrush();
            }
            else
                brush = checkBackground.GetBrush();

            g.FillRectangle(brush, bounds);
        }

        private static void DrawPopupBorder(Graphics g, Rectangle r, ColorData colors)
        {
            Pen pen = colors.Highlight.GetPen();
            Pen pen2 = colors.ButtonShadow.GetPen();
            Pen pen3 = colors.ButtonFace.GetPen();
            g.DrawLine(pen, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
            g.DrawLine(pen, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
            g.DrawLine(pen2, r.Left, r.Top, r.Left, r.Bottom - 1);
            g.DrawLine(pen2, r.Left, r.Top, r.Right - 2, r.Top);
            g.DrawLine(pen3, r.Right - 2, r.Top + 1, r.Right - 2, r.Bottom - 2);
            g.DrawLine(pen3, r.Left + 1, r.Bottom - 2, r.Right - 2, r.Bottom - 2);

            r.Inflate(-1, -1);
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
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                LayoutData layout = PaintPopupLayout(state, true).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, null);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.CheckBounds, colors.ButtonFace, true, colors);
                DrawPopupBorder(g, layout.CheckBounds, colors);
                DrawCheckOnly(g, layout, colors, colors.WindowText, true, state);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            if (IsButton)
                ButtonAdapter.PaintOver(e);
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                LayoutData layout = PaintPopupLayout(state, true).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, null);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.CheckBounds, colors.HighContrast ? colors.ButtonFace : colors.Highlight, true, colors);
                DrawPopupBorder(g, layout.CheckBounds, colors);
                DrawCheckOnly(g, layout, colors, colors.WindowText, true, state);

                Region? originalClip = null;
                if (!String.IsNullOrEmpty(state.Text))
                {
                    originalClip = e.Graphics.Clip;
                    e.Graphics.ExcludeClip(layout.CheckArea);
                }

                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);

                if (originalClip is not null)
                    e.Graphics.Clip = originalClip;
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            if (IsButton)
                ButtonAdapter.PaintUp(e);
            else
            {
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(this, e.Graphics, state);
                LayoutData layout = PaintPopupLayout(state, false).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, null);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.CheckBounds, colors.HighContrast ? colors.ButtonFace : colors.Highlight, true, colors);
                DrawFlatBorder(g, layout.CheckBounds, colors.ButtonShadow);
                DrawCheckOnly(g, layout, colors, colors.WindowText, true, state);
                AdjustFocusRectangle(state, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter() => new ButtonPopupAdapter(ButtonInstance);
        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state) => PaintPopupLayout(state, true);

        #endregion

        #region Private Methods

        private LayoutOptions PaintPopupLayout(ControlAppearanceState state, bool show3D)
        {
            LayoutOptions options = CommonLayout(state);
            options.ShadowedText = false;
            int checkSize = FlatCheckSize.Scale(options.Scale.X);

            if (show3D)
            {
                options.CheckSize = checkSize + 1;
                return options;
            }

            options.CheckSize = checkSize;
            options.CheckPaddingSize = 1;
            return options;
        }

        #endregion

        #endregion

        #endregion
    }
}
