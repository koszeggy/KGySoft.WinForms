#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonStandardAdapter.cs
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
using System.Windows.Forms.VisualStyles;

using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal class ButtonStandardAdapter : ButtonBaseAdapter
    {
        #region Constructors

        internal ButtonStandardAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static PUSHBUTTONSTATES DetermineState(bool up, ControlAppearanceState state)
        {
            if (!up)
                return PUSHBUTTONSTATES.PBS_PRESSED;

            PUSHBUTTONSTATES result = (PUSHBUTTONSTATES)state.SystemStateId;
            if (result == PUSHBUTTONSTATES.PBS_DEFAULTED_ANIMATING && !WindowsUtils.IsVistaOrLater)
                result = PUSHBUTTONSTATES.PBS_DEFAULTED;

            return result;
        }

        private static void Draw3DBorderHighContrastRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.ButtonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            Pen? penTopLeft = null;
            Pen? penBottomRight = null;
            Pen? insetPen = null;
            Pen? bottomRightInsetPen = null;

            try
            {
                // top + left
                penTopLeft = stockColor ? /*SystemPens.ControlLightLight*/ new Pen(SystemColors.ControlLightLight) : new Pen(colors.Highlight);
                g.DrawLine(penTopLeft, p1, p2); // top  (right-left)
                g.DrawLine(penTopLeft, p2, p3); // left (up-down)

                // bottom + right
                penBottomRight = stockColor ? new Pen(SystemColors.ControlDarkDark) : new Pen(colors.ButtonShadowDark);

                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(penBottomRight, p3, p4);  // bottom (left-right)
                g.DrawLine(penBottomRight, p4, p1);  // right  (bottom-up )

                // Draw inset
                if (stockColor)
                    insetPen = VisualStyleHelper.HighContrast ? new Pen(SystemColors.ControlLight) : new Pen(SystemColors.Control);
                else
                    insetPen = VisualStyleHelper.HighContrast ? new Pen(colors.Highlight) : new Pen(colors.ButtonFace);

                p1.Offset(-1, 2);
                p2.Offset(1, 1);
                p3.Offset(1, -1);
                p4.Offset(-1, -1);

                // top + left inset
                g.DrawLine(insetPen, p1, p2); // top (right-left)
                g.DrawLine(insetPen, p2, p3); // left( up-down)

                // Bottom + right inset

                bottomRightInsetPen = stockColor ? new Pen(SystemColors.ControlDark) : new Pen(colors.ButtonShadow);
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(bottomRightInsetPen, p3, p4); // bottom (left-right)
                g.DrawLine(bottomRightInsetPen, p4, p1); // right  (bottom-up)
            }
            finally
            {
                if (penTopLeft != null)
                    penTopLeft.Dispose();

                if (penBottomRight != null)
                    penBottomRight.Dispose();

                if (insetPen != null)
                    insetPen.Dispose();

                if (bottomRightInsetPen != null)
                    bottomRightInsetPen.Dispose();
            }
        }

        private static void Draw3DBorderNormal(Graphics g, ref Rectangle bounds, ColorData colors)
        {

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // top + left
            Pen pen = new Pen(colors.ButtonShadowDark);
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right
            pen = new Pen(colors.Highlight);
            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4); // bottom(left-right)
                g.DrawLine(pen, p4, p1); // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }

            // Draw inset
            pen = new Pen(colors.ButtonFace);

            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            // top + left inset
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right inset
            pen = colors.ButtonFace.ToKnownColor() == SystemColors.Control.ToKnownColor()
                ? new Pen(SystemColors.ControlLight)
                : new Pen(colors.ButtonFace);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4); // bottom(left-right)
                g.DrawLine(pen, p4, p1); // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }
        }

        private static void Draw3DBorderRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.ButtonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // Draw counter-clock-wise.

            // top + left
            Pen pen = stockColor ? new Pen(SystemColors.ControlLightLight) : new Pen(colors.Highlight);

            try
            {
                g.DrawLine(pen, p1, p2);   // top (right-left)
                g.DrawLine(pen, p2, p3);   // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // bottom + right
            pen = stockColor ? new Pen(SystemColors.ControlDarkDark) : new Pen(colors.ButtonShadowDark);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4);    // bottom(left-right)
                g.DrawLine(pen, p4, p1);    // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }

            // Draw inset
            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            pen = stockColor
                ? VisualStyleHelper.HighContrast ? new Pen(SystemColors.ControlLight) : new Pen(SystemColors.Control)
                : new Pen(colors.ButtonFace);

            // top + left inset
            try
            {
                g.DrawLine(pen, p1, p2); // top (right-left)
                g.DrawLine(pen, p2, p3); // left(up-down)
            }
            finally
            {
                pen.Dispose();
            }

            // Bottom + right inset
            pen = stockColor ? new Pen(SystemColors.ControlDark) : new Pen(colors.ButtonShadow);

            try
            {
                p1.Offset(0, -1); // need to paint last pixel too.
                g.DrawLine(pen, p3, p4);  // bottom(left-right)
                g.DrawLine(pen, p4, p1);  // right (bottom-up)
            }
            finally
            {
                pen.Dispose();
            }
        }

        private static void Draw3DBorder(Graphics g, Rectangle bounds, ColorData colors, bool raised, ControlAppearanceState state)
        {
            if (state.BackColor != SystemColors.Control && VisualStyleHelper.HighContrast)
            {
                if (raised)
                    Draw3DBorderHighContrastRaised(g, ref bounds, colors);
                else
                    ControlPaint.DrawBorder(g, bounds, ControlPaint.Dark(state.BackColor), ButtonBorderStyle.Solid);
            }
            else
            {
                if (raised)
                    Draw3DBorderRaised(g, ref bounds, colors);
                else
                    Draw3DBorderNormal(g, ref bounds, colors);
            }
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override void PaintDown(PaintStateEventArgs e) => PaintWorker(e, false);
        internal override void PaintOver(PaintStateEventArgs e) => PaintUp(e);
        internal override void PaintUp(PaintStateEventArgs e) => PaintWorker(e, true);

        #endregion

        #region Protected Methods

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state) => PaintLayout(state, false);

        #endregion

        #region Private Methods

        private LayoutOptions PaintLayout(ControlAppearanceState state, bool up)
        {
            LayoutOptions options = CommonLayout(state);
            options.ForceDoubleFocusWidth = WindowsUtils.IsWindows10OrLater;
            options.TextOffset = !up;
            options.GrowBorderBy1PxWhenDefault = options.DotNetOneButtonCompat = !VisualStyleHelper.RenderWithVisualStyles;
            return options;
        }

        private void PaintThemedButtonBackground(PaintStateEventArgs e, Rectangle bounds, bool up)
        {
            ControlAppearanceState state = e.State;
            PUSHBUTTONSTATES buttonState = DetermineState(up, state);
            if (ButtonRenderer.IsBackgroundPartiallyTransparent((PushButtonState)buttonState))
                ButtonRenderer.DrawParentBackground(e.Graphics, bounds, ButtonInstance);

            VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, (int)buttonState, ButtonInstance.ClientRectangle);
            bounds.Inflate(-ButtonBorderSize, -ButtonBorderSize);
            if (!ButtonInstance.UseVisualStyleBackColor)
            {
                Color backColor = state.BackColor;
                if (backColor.A > 0)
                {
                    if (backColor.A == 0xff)
                        backColor = e.Graphics.GetNearestColor(backColor);

                    using Brush brush = new SolidBrush(backColor);
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }

            if (ButtonInstance.BackgroundImage != null && !VisualStyleHelper.HighContrast)
                e.Graphics.DrawBackgroundImage(ButtonInstance.BackgroundImage, Color.Transparent, ButtonInstance.BackgroundImageLayout, ButtonInstance.ClientRectangle, bounds, ButtonInstance.DisplayRectangle.Location, ButtonInstance.RightToLeft);
        }

        private void PaintWorker(PaintStateEventArgs e, bool up)
        {
            Graphics g = e.Graphics;
            ControlAppearanceState state = e.State;
            up = up && (state.CheckState == CheckState.Unchecked);
            ColorData colors = ColorData.Calculate(e.Graphics, state.BackColor, state.ForeColor);
            bool renderWithVisualStyles = VisualStyleHelper.RenderWithVisualStyles;
            LayoutData layout = renderWithVisualStyles
                ? PaintLayout(state, true).Layout(g)
                : PaintLayout(state, up).Layout(g);

            if (renderWithVisualStyles)
                PaintThemedButtonBackground(e, ButtonInstance.ClientRectangle, up);
            else
            {
                Rectangle clientRectangle = ButtonInstance.ClientRectangle;
                if (up)
                    clientRectangle.Inflate(-2, -2);
                else
                    clientRectangle.Inflate(-1, -1);

                if (state.CheckState == CheckState.Indeterminate)
                {
                    using Brush brush = CreateDitherBrush(colors.Highlight, colors.ButtonFace);
                    e.Graphics.FillRectangle(brush, clientRectangle);
                }
                else
                    PaintButtonBackground(e, clientRectangle, colors.ButtonFace);
            }
            
            PaintImage(e, layout);
            PaintField(e, layout, colors, true);
            
            if (!renderWithVisualStyles)
            {
                Rectangle r = ButtonInstance.ClientRectangle;
                if (state.IsDefault)
                    r.Inflate(-1, -1);
                
                DrawDefaultBorder(g, r, colors.WindowFrame, state.IsDefault);
                if (up)
                    Draw3DBorder(g, r, colors, true, state);
                else
                    ControlPaint.DrawBorder(g, r, colors.ButtonShadow, ButtonBorderStyle.Solid);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
