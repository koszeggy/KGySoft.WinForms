#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.Controls
{
    internal class CheckBoxPopupAdapter: CheckBoxBaseAdapter
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
            {
                DrawDitheredFill(e.Graphics, colors.buttonFace, checkBackground, bounds);
            }
            else
            {
                DrawCheckBackground(state.Enabled, state.CheckState, e.Graphics, bounds, checkBackground, disabledColors);
            }
        }

        private static void DrawCheckBackground(bool controlEnabled, CheckState controlCheckState, Graphics g, Rectangle bounds, Color checkBackground, bool disabledColors)
        {
            Brush brush;
            bool toDispose = true;
            if (!controlEnabled && disabledColors)
            {
                brush = SystemBrushes.Control;
                toDispose = false;
            }
            else if (((controlCheckState == CheckState.Indeterminate) && (checkBackground == SystemColors.Window)) && disabledColors)
            {
                Color color = SystemInformation.HighContrast ? SystemColors.ControlDark : SystemColors.Control;
                byte red = (byte)((color.R + SystemColors.Window.R) / 2);
                byte green = (byte)((color.G + SystemColors.Window.G) / 2);
                byte blue = (byte)((color.B + SystemColors.Window.B) / 2);
                brush = new SolidBrush(Color.FromArgb(red, green, blue));
            }
            else
            {
                brush = new SolidBrush(checkBackground);
            }
            try
            {
                g.FillRectangle(brush, bounds);
            }
            finally
            {
                if (toDispose)
                    brush.Dispose();
            }
        }

        private static void DrawPopupBorder(Graphics g, Rectangle r, ColorData colors)
        {
            using (Pen pen = new Pen(colors.highlight))
            {
                using (Pen pen2 = new Pen(colors.buttonShadow))
                {
                    using (Pen pen3 = new Pen(colors.buttonFace))
                    {
                        g.DrawLine(pen, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                        g.DrawLine(pen, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
                        g.DrawLine(pen2, r.Left, r.Top, r.Left, r.Bottom - 1);
                        g.DrawLine(pen2, r.Left, r.Top, r.Right - 2, r.Top);
                        g.DrawLine(pen3, r.Right - 2, r.Top + 1, r.Right - 2, r.Bottom - 2);
                        g.DrawLine(pen3, r.Left + 1, r.Bottom - 2, r.Right - 2, r.Bottom - 2);
                    }
                }
            }
            r.Inflate(-1, -1);
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
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = PaintPopupLayout(state, true).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.checkBounds, colors.buttonFace, true, colors);
                DrawPopupBorder(g, layout.checkBounds, colors);
                DrawCheckOnly(e, layout, colors, colors.windowText, true, state);
                PaintField(e, layout, colors, true);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            Graphics g = e.Graphics;
            if (IsButton)
            {
                ButtonAdapter.PaintOver(e);
            }
            else
            {
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = PaintPopupLayout(state, true).Layout(g);
                Region clip = e.Graphics.Clip;
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.checkBounds, colors.highContrast ? colors.buttonFace : colors.highlight, true, colors);
                DrawPopupBorder(g, layout.checkBounds, colors);
                DrawCheckOnly(e, layout, colors, colors.windowText, true, state);
                e.Graphics.Clip = clip;
                e.Graphics.ExcludeClip(layout.checkArea);
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
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
                LayoutData layout = PaintPopupLayout(state, false).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                PaintImage(e, layout);
                DrawCheckBackground(e, layout.checkBounds, colors.highContrast ? colors.buttonFace : colors.highlight, true, colors);
                DrawFlatBorder(g, layout.checkBounds, colors.buttonShadow);
                DrawCheckOnly(e, layout, colors, colors.windowText, true, state);
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
            return PaintPopupLayout(state, true);
        }

        #endregion

        #region Private Methods

        private LayoutOptions PaintPopupLayout(ControlAppearanceState state, bool show3D)
        {
            LayoutOptions options = CommonLayout(state);
            options.shadowedText = false;
            if (show3D)
            {
                options.checkSize = 12;
                return options;
            }
            options.checkSize = 11;
            options.checkPaddingSize = 1;
            return options;
        }

        #endregion

        #endregion

        #endregion
    }
}
