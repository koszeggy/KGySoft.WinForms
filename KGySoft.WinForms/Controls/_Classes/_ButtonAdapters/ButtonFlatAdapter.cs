#region Used namespaces

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class ButtonFlatAdapter: ButtonBaseAdapter
    {
        #region Properties

        protected override int ButtonBorderSize
        {
            get { return 1; }
        }

        #endregion

        #region Constructors

        internal ButtonFlatAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static Color MixedColor(Color color1, Color color2)
        {
            byte a1 = color1.A;
            byte r1 = color1.R;
            byte g1 = color1.G;
            byte b1 = color1.B;

            byte a2 = color2.A;
            byte r2 = color2.R;
            byte g2 = color2.G;
            byte b2 = color2.B;

            int a3 = (a1 + a2) / 2;
            int r3 = (r1 + r2) / 2;
            int g3 = (g1 + g2) / 2;
            int b3 = (b1 + b2) / 2;

            return Color.FromArgb(a3, r3, g3, b3);
        }

        /// <summary>
        /// Draws the flat border with specified bordersize.
        /// This function gets called only for Flatstyle == Flatstyle.Flat.
        /// </summary>
        private static void DrawFlatBorderWithSize(Graphics g, Rectangle r, Color c, int size)
        {
            bool stockBorder = c.IsSystemColor;
            SolidBrush brush;

            if (size > 1)
            {
                brush = new SolidBrush(c);
            }
            else
            {
                if (stockBorder)
                {
                    brush = (SolidBrush)SystemBrushes.FromSystemColor(c);
                }
                else
                {
                    brush = new SolidBrush(c);
                }
            }

            try
            {
                size = Math.Min(size, Math.Min(r.Width, r.Height));
                // ...truncate pen width to button size, to avoid overflow if border size is huge!

                //Left Border
                g.FillRectangle(brush, r.X, r.Y, size, r.Height);

                //Right Border
                g.FillRectangle(brush, (r.X + r.Width - size), r.Y, size, r.Height);

                //Top Border
                g.FillRectangle(brush, (r.X + size), r.Y, (r.Width - size * 2), size);

                //Bottom Border
                g.FillRectangle(brush, (r.X + size), (r.Y + r.Height - size), (r.Width - size * 2), size);
            }
            finally
            {
                if (!stockBorder)
                {
                    brush.Dispose();
                }
            }
        }

        private static void DrawFlatFocus(Graphics g, Rectangle r, Color c)
        {
            using (Pen focus = new Pen(c))
            {
                g.DrawRectangle(focus, r);
            }
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            bool flag = (ButtonInstance.FlatAppearance.BorderSize != 1) || !ButtonInstance.FlatAppearance.BorderColor.IsEmpty;
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = PaintFlatLayout(state, !ButtonInstance.FlatAppearance.CheckedBackColor.IsEmpty || (SystemInformation.HighContrast ? (state.CheckState != CheckState.Indeterminate) : (state.CheckState == CheckState.Unchecked)),
                (!flag && SystemInformation.HighContrast) && (state.CheckState == CheckState.Checked), ButtonInstance.FlatAppearance.BorderSize).Layout(g);
            if (!ButtonInstance.FlatAppearance.BorderColor.IsEmpty)
            {
                colors.windowFrame = ButtonInstance.FlatAppearance.BorderColor;
            }
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            Color backColor = state.BackColor;
            if (!ButtonInstance.FlatAppearance.MouseDownBackColor.IsEmpty)
            {
                backColor = ButtonInstance.FlatAppearance.MouseDownBackColor;
            }
            else
            {
                switch (state.CheckState)
                {
                    case CheckState.Unchecked:
                    case CheckState.Checked:
                        backColor = colors.highContrast ? colors.buttonShadow : colors.lowHighlight;
                        break;

                    case CheckState.Indeterminate:
                        backColor = MixedColor(colors.highContrast ? colors.buttonShadow : colors.lowHighlight, colors.buttonFace);
                        break;
                }
            }

            PaintBackground(e, clientRectangle, backColor);
            ISupportButtonAdapter host = (ISupportButtonAdapter)ButtonInstance;
            if (state.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            PaintImage(e, layout);
            PaintField(e, layout, colors, false);
            if (ButtonInstance.Focused && host.ShowFocusCues)
            {
                DrawFlatFocus(g, layout.focus, colors.highContrast ? colors.windowText : colors.constrastButtonShadow);
            }
            if ((!state.IsDefault || !ButtonInstance.Focused) || (ButtonInstance.FlatAppearance.BorderSize != 0))
            {
                DrawDefaultBorder(g, clientRectangle, colors.windowFrame, state.IsDefault);
            }
            if (flag)
            {
                if (ButtonInstance.FlatAppearance.BorderSize != 1)
                {
                    DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, ButtonInstance.FlatAppearance.BorderSize);
                }
                else
                {
                    DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
            }
            else if ((state.CheckState == CheckState.Checked) && SystemInformation.HighContrast)
            {
                DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                DrawFlatBorder(g, clientRectangle, colors.buttonShadow);
            }
            else if (state.CheckState == CheckState.Indeterminate)
            {
                Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
            else
            {
                DrawFlatBorder(g, clientRectangle, colors.windowFrame);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            ControlAppearanceState state = e.State;
            if (SystemInformation.HighContrast)
            {
                PaintUp(e);
            }
            else
            {
                Graphics g = e.Graphics;
                bool hasBorder = (ButtonInstance.FlatAppearance.BorderSize != 1) || !ButtonInstance.FlatAppearance.BorderColor.IsEmpty;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = PaintFlatLayout(state, !ButtonInstance.FlatAppearance.CheckedBackColor.IsEmpty || (state.CheckState == CheckState.Unchecked),
                    false, ButtonInstance.FlatAppearance.BorderSize).Layout(g);
                if (!ButtonInstance.FlatAppearance.BorderColor.IsEmpty)
                {
                    colors.windowFrame = ButtonInstance.FlatAppearance.BorderColor;
                }
                Rectangle clientRectangle = ButtonInstance.ClientRectangle;
                Color backColor;
                if (!ButtonInstance.FlatAppearance.MouseOverBackColor.IsEmpty)
                {
                    backColor = ButtonInstance.FlatAppearance.MouseOverBackColor;
                }
                else if (!ButtonInstance.FlatAppearance.CheckedBackColor.IsEmpty)
                {
                    if ((state.CheckState == CheckState.Checked) || (state.CheckState == CheckState.Indeterminate))
                    {
                        backColor = MixedColor(ButtonInstance.FlatAppearance.CheckedBackColor, colors.lowButtonFace);
                    }
                    else
                    {
                        backColor = colors.lowButtonFace;
                    }
                }
                else if (state.CheckState == CheckState.Indeterminate)
                {
                    backColor = MixedColor(colors.buttonFace, colors.lowButtonFace);
                }
                else
                {
                    backColor = colors.lowButtonFace;
                }
                PaintBackground(e, clientRectangle, backColor);
                ISupportButtonAdapter host = (ISupportButtonAdapter)ButtonInstance;
                if (state.IsDefault)
                {
                    clientRectangle.Inflate(-1, -1);
                }
                PaintImage(e, layout);
                PaintField(e, layout, colors, false);
                if (ButtonInstance.Focused && host.ShowFocusCues)
                {
                    DrawFlatFocus(g, layout.focus, colors.constrastButtonShadow);
                }
                if ((!state.IsDefault || !ButtonInstance.Focused) || (ButtonInstance.FlatAppearance.BorderSize != 0))
                {
                    DrawDefaultBorder(g, clientRectangle, colors.windowFrame, state.IsDefault);
                }
                if (hasBorder)
                {
                    if (ButtonInstance.FlatAppearance.BorderSize != 1)
                    {
                        DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, ButtonInstance.FlatAppearance.BorderSize);
                    }
                    else
                    {
                        DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                    }
                }
                else if (state.CheckState == CheckState.Unchecked)
                {
                    DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
                else
                {
                    Draw3DLiteBorder(g, clientRectangle, colors, false);
                }
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            bool hasBorder = (ButtonInstance.FlatAppearance.BorderSize != 1) || !ButtonInstance.FlatAppearance.BorderColor.IsEmpty;
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            LayoutData layout = PaintFlatLayout(state,
                !ButtonInstance.FlatAppearance.CheckedBackColor.IsEmpty || (SystemInformation.HighContrast ? (state.CheckState != CheckState.Indeterminate) : (state.CheckState == CheckState.Unchecked)),
                (!hasBorder && SystemInformation.HighContrast) && (state.CheckState == CheckState.Checked), ButtonInstance.FlatAppearance.BorderSize).Layout(g);
            if (!ButtonInstance.FlatAppearance.BorderColor.IsEmpty)
            {
                colors.windowFrame = ButtonInstance.FlatAppearance.BorderColor;
            }
            Rectangle clientRectangle = ButtonInstance.ClientRectangle;
            Color backColor = state.BackColor;
            if (!ButtonInstance.FlatAppearance.CheckedBackColor.IsEmpty)
            {
                switch (state.CheckState)
                {
                    case CheckState.Checked:
                        backColor = ButtonInstance.FlatAppearance.CheckedBackColor;
                        break;

                    case CheckState.Indeterminate:
                        backColor = MixedColor(ButtonInstance.FlatAppearance.CheckedBackColor, colors.buttonFace);
                        break;
                }
            }
            else
            {
                switch (state.CheckState)
                {
                    case CheckState.Checked:
                        backColor = colors.highlight;
                        break;

                    case CheckState.Indeterminate:
                        backColor = MixedColor(colors.highlight, colors.buttonFace);
                        break;
                }
            }

            PaintBackground(e, clientRectangle, backColor);
            ISupportButtonAdapter host = (ISupportButtonAdapter)ButtonInstance;
            if (state.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            PaintImage(e, layout);
            PaintField(e, layout, colors, false);
            if (ButtonInstance.Focused && host.ShowFocusCues)
            {
                DrawFlatFocus(g, layout.focus, colors.highContrast ? colors.windowText : colors.constrastButtonShadow);
            }
            if ((!state.IsDefault || !ButtonInstance.Focused) || (ButtonInstance.FlatAppearance.BorderSize != 0))
            {
                DrawDefaultBorder(g, clientRectangle, colors.windowFrame, state.IsDefault);
            }
            if (hasBorder)
            {
                if (ButtonInstance.FlatAppearance.BorderSize != 1)
                {
                    DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, ButtonInstance.FlatAppearance.BorderSize);
                }
                else
                {
                    DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
            }
            else if ((state.CheckState == CheckState.Checked) && SystemInformation.HighContrast)
            {
                DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                DrawFlatBorder(g, clientRectangle, colors.buttonShadow);
            }
            else if (state.CheckState == CheckState.Indeterminate)
            {
                Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
            else
            {
                DrawFlatBorder(g, clientRectangle, colors.windowFrame);
            }
        }

        #endregion

        #region Protected Methods

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            return PaintFlatLayout(state, false, true, ButtonInstance.FlatAppearance.BorderSize);
        }

        #endregion

        #region Private Methods

        private void PaintBackground(PaintEventArgs e, Rectangle r, Color backColor)
        {
            Rectangle rectangle = r;
            rectangle.Inflate(-ButtonInstance.FlatAppearance.BorderSize, -ButtonInstance.FlatAppearance.BorderSize);
            ButtonInstance.PaintBackground(e, rectangle, backColor, rectangle.Location);
        }

        private LayoutOptions PaintFlatLayout(ControlAppearanceState state, bool up, bool check, int borderSize)
        {
            LayoutOptions options = CommonLayout(state);
            options.borderSize = borderSize + (check ? 1 : 0);
            options.paddingSize = check ? 1 : 2;
            options.focusOddEvenFixup = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        #endregion

        #endregion

        #endregion
    }
}
