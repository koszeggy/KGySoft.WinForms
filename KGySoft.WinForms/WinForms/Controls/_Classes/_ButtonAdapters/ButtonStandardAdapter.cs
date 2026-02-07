#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonStandardAdapter.cs
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
using System.Windows.Forms.VisualStyles;

using KGySoft.WinForms.Reflection;

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

        private static void Draw3DBorderHighContrastRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.BackColor.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            Pen penTopLeft = (stockColor ? SystemColors.ControlLightLight : colors.Highlight).GetPen();
            Pen penBottomRight = (stockColor ? SystemColors.ControlDarkDark : colors.ButtonShadowDark).GetPen();
            Pen insetPen = stockColor
                ? (VisualStyleHelper.HighContrast ? SystemColors.ControlLight : SystemColors.Control).GetPen()
                : (VisualStyleHelper.HighContrast ? colors.Highlight : colors.BackColor).GetPen();
            Pen bottomRightInsetPen = (stockColor ? SystemColors.ControlDark : colors.ButtonShadow).GetPen();

            // top + left
            g.DrawLine(penTopLeft, p1, p2); // top  (right-left)
            g.DrawLine(penTopLeft, p2, p3); // left (up-down)

            // bottom + right
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(penBottomRight, p3, p4);  // bottom (left-right)
            g.DrawLine(penBottomRight, p4, p1);  // right  (bottom-up )

            // Draw inset
            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            // top + left inset
            g.DrawLine(insetPen, p1, p2); // top (right-left)
            g.DrawLine(insetPen, p2, p3); // left( up-down)

            // Bottom + right inset
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(bottomRightInsetPen, p3, p4); // bottom (left-right)
            g.DrawLine(bottomRightInsetPen, p4, p1); // right  (bottom-up)
        }

        private static void Draw3DBorderNormal(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // top + left
            Pen pen = colors.ButtonShadowDark.GetPen();
            g.DrawLine(pen, p1, p2); // top (right-left)
            g.DrawLine(pen, p2, p3); // left(up-down)

            // bottom + right
            pen = colors.Highlight.GetPen();
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(pen, p3, p4); // bottom(left-right)
            g.DrawLine(pen, p4, p1); // right (bottom-up)

            // Draw inset
            pen = colors.BackColor.GetPen();
            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            // top + left inset
            g.DrawLine(pen, p1, p2); // top (right-left)
            g.DrawLine(pen, p2, p3); // left(up-down)

            // bottom + right inset
            pen = colors.BackColor.ToKnownColor() == SystemColors.Control.ToKnownColor()
                ? SystemPens.ControlLight
                : colors.BackColor.GetPen();

            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(pen, p3, p4); // bottom(left-right)
            g.DrawLine(pen, p4, p1); // right (bottom-up)
        }

        private static void Draw3DBorderRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool stockColor = colors.BackColor.ToKnownColor() == SystemColors.Control.ToKnownColor();

            // Draw counter-clock-wise.
            Point p1 = new Point(bounds.X + bounds.Width - 1, bounds.Y);  // upper inner right.
            Point p2 = new Point(bounds.X, bounds.Y);  // upper left.
            Point p3 = new Point(bounds.X, bounds.Y + bounds.Height - 1);  // bottom inner left.
            Point p4 = new Point(bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);  // inner bottom right.

            // Draw counter-clock-wise.

            // top + left
            Pen pen = (stockColor ? SystemColors.ControlLightLight : colors.Highlight).GetPen();
            g.DrawLine(pen, p1, p2);   // top (right-left)
            g.DrawLine(pen, p2, p3);   // left(up-down)

            // bottom + right
            pen = (stockColor ? SystemColors.ControlDarkDark : colors.ButtonShadowDark).GetPen();
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(pen, p3, p4);    // bottom(left-right)
            g.DrawLine(pen, p4, p1);    // right (bottom-up)

            // Draw inset
            p1.Offset(-1, 2);
            p2.Offset(1, 1);
            p3.Offset(1, -1);
            p4.Offset(-1, -1);

            pen = (stockColor
                ? VisualStyleHelper.HighContrast ? SystemColors.ControlLight : SystemColors.Control
                : colors.BackColor).GetPen();

            // top + left inset
            g.DrawLine(pen, p1, p2); // top (right-left)
            g.DrawLine(pen, p2, p3); // left(up-down)

            // Bottom + right inset
            pen = (stockColor ? SystemColors.ControlDark : colors.ButtonShadow).GetPen();
            p1.Offset(0, -1); // need to paint last pixel too.
            g.DrawLine(pen, p3, p4);  // bottom(left-right)
            g.DrawLine(pen, p4, p1);  // right (bottom-up)
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

        protected override bool IsHighContrastHighlighted(ControlAppearanceState state)
            => state.CheckState == CheckState.Unchecked && base.IsHighContrastHighlighted(state);

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state) => PaintLayout(state, false);

        #endregion

        #region Private Methods

        private LayoutOptions PaintLayout(ControlAppearanceState state, bool up)
        {
            LayoutOptions options = CommonLayout(state);
            options.ForceDoubleFocusWidth = OSHelper.IsWindows10OrLater;
            options.TextOffset = !up;
            options.GrowBorderBy1PxWhenDefault = options.DotNetOneButtonCompat = !VisualStyleHelper.RenderWithVisualStyles;
            return options;
        }

        private void PaintThemedButtonBackground(PaintStateEventArgs e, Rectangle bounds, bool up)
        {
            ControlAppearanceState state = e.State;
            if (ButtonRenderer.IsBackgroundPartiallyTransparent((PushButtonState)state.SystemStateId))
                ButtonRenderer.DrawParentBackground(e.Graphics, bounds, ButtonInstance);

            VisualStyleHelper.Render(VisualStyleHelper.ButtonTheme, ButtonInstance, e.Graphics, state.SystemPartId, state.SystemStateId, ButtonInstance.ClientRectangle);
            bounds.Inflate(-ButtonBorderSize, -ButtonBorderSize);
            if (!ButtonInstance.UseVisualStyleBackColor)
            {
                bool isHighContrastHighlighted = up && IsHighContrastHighlighted(state);
                Color backColor = isHighContrastHighlighted ? SystemColors.Highlight : state.BackColor;
                if (backColor.A > 0)
                {
                    if (backColor.A == Byte.MaxValue)
                        backColor = e.Graphics.GetNearestColor(backColor);
                    e.Graphics.FillRectangle(backColor.GetBrush(), bounds);
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
            ColorData colors = ColorData.Calculate(this, e.Graphics, state);
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
                    using Brush brush = CreateDitherBrush(colors.Highlight, colors.BackColor);
                    PaintButtonBackground(e, clientRectangle, brush);
                }
                else
                    PaintButtonBackground(e, clientRectangle, null);
            }

            PaintImage(e, layout);

            // The original code may call ControlPaint.DrawHighContrastFocusRectangle here, which is handled in DrawFocus called from PaintField.
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
