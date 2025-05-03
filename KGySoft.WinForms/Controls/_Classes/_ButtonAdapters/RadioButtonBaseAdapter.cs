#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RadioButtonBaseAdapter.cs
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class RadioButtonBaseAdapter : CheckableControlBaseAdapter
    {
        #region Properties

        private RadioButton RadioButtonInstance => (RadioButton)ButtonInstance;

        #endregion

        #region Constructors

        internal RadioButtonBaseAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void DrawAndFillEllipse(Graphics graphics, Pen borderPen, Brush fieldBrush, Rectangle bounds)
        {
            graphics.FillRectangle(fieldBrush, new Rectangle(bounds.X + 2, bounds.Y + 2, 8, 8));
            graphics.FillRectangle(fieldBrush, new Rectangle(bounds.X + 4, bounds.Y + 1, 4, 10));
            graphics.FillRectangle(fieldBrush, new Rectangle(bounds.X + 1, bounds.Y + 4, 10, 4));

            // NOTE: values are different here because that uses lower level HDC drawing:
            // https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/System/Windows/Forms/Controls/Buttons/ButtonInternal/RadioButtonBaseAdapter.cs
            graphics.DrawLine(borderPen, new Point(bounds.X + 4, bounds.Y), new Point(bounds.X + 7, bounds.Y));
            graphics.DrawLine(borderPen, new Point(bounds.X + 4, bounds.Y + 11), new Point(bounds.X + 7, bounds.Y + 11));

            graphics.DrawLine(borderPen, new Point(bounds.X + 2, bounds.Y + 1), new Point(bounds.X + 3, bounds.Y + 1));
            graphics.DrawLine(borderPen, new Point(bounds.X + 8, bounds.Y + 1), new Point(bounds.X + 9, bounds.Y + 1));

            graphics.DrawLine(borderPen, new Point(bounds.X + 2, bounds.Y + 10), new Point(bounds.X + 3, bounds.Y + 10));
            graphics.DrawLine(borderPen, new Point(bounds.X + 8, bounds.Y + 10), new Point(bounds.X + 9, bounds.Y + 10));

            graphics.DrawLine(borderPen, new Point(bounds.X, bounds.Y + 4), new Point(bounds.X, bounds.Y + 7));
            graphics.DrawLine(borderPen, new Point(bounds.X + 11, bounds.Y + 4), new Point(bounds.X + 11, bounds.Y + 7));

            graphics.DrawLine(borderPen, new Point(bounds.X + 1, bounds.Y + 2), new Point(bounds.X + 1, bounds.Y + 3));
            graphics.DrawLine(borderPen, new Point(bounds.X + 1, bounds.Y + 8), new Point(bounds.X + 1, bounds.Y + 9));

            graphics.DrawLine(borderPen, new Point(bounds.X + 10, bounds.Y + 2), new Point(bounds.X + 10, bounds.Y + 3));
            graphics.DrawLine(borderPen, new Point(bounds.X + 10, bounds.Y + 8), new Point(bounds.X + 10, bounds.Y + 9));
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions options = base.CommonLayout(state);
            options.CheckAlign = RadioButtonInstance.CheckAlign;
            return options;
        }

        #endregion

        #region Protected Methods

        protected void DrawCheckBackgroundFlat(PaintStateEventArgs e, Rectangle bounds, Color borderColor, Color checkBackground, bool disabledColors)
        {
            ControlAppearanceState state = e.State;
            Color backColor = checkBackground;
            Color foreColor = borderColor;
            if (!state.Enabled && disabledColors)
            {
                foreColor = VisualStyleHelper.HighContrast ? SystemColors.WindowFrame : state.ForeColor;
                backColor = state.BackColor;
            }

            float scale = GetDpiScaleRatio();

            using Pen pen = new Pen(foreColor);
            using Brush brush = new SolidBrush(backColor);

            if (scale > 1.1f)
            {
                // In high DPI mode when we draw an ellipse as three rectangles, the quality of ellipse is poor. Draw
                // it directly as an ellipse.
                bounds.Width--;
                bounds.Height--;
                GraphicsState prevState = e.Graphics.Save();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, bounds);
                e.Graphics.DrawEllipse(pen, bounds);
                e.Graphics.Restore(prevState);
                bounds.Inflate(-1, -1);
            }
            else
                DrawAndFillEllipse(e.Graphics, pen, brush, bounds);
        }

        protected void DrawCheckOnly(PaintStateEventArgs e, LayoutData layout, Color checkColor)
        {
            ControlAppearanceState state = e.State;
            if (state.CheckState == CheckState.Unchecked)
                return;

            float scale = GetDpiScaleRatio();
            using Brush brush = new SolidBrush(checkColor);
            int padding = 5;

            // Original code
            if (scale < 1.1f)
            {
                Rectangle vCross = new Rectangle(layout.CheckBounds.X + padding, (layout.CheckBounds.Y + padding) - 1, 2, 4);
                e.Graphics.FillRectangle(brush, vCross);
                Rectangle hCross = new Rectangle((layout.CheckBounds.X + padding) - 1, layout.CheckBounds.Y + padding, 4, 2);
                e.Graphics.FillRectangle(brush, hCross);
                return;
            }

            // This scaled rendering differs from the original because that one is very ugly e.g. at 150%: https://github.com/dotnet/winforms/blob/1c324d074280ab5de6342d973069faa687f2c165/src/System.Windows.Forms/System/Windows/Forms/Controls/Buttons/ButtonInternal/RadioButtonBaseAdapter.cs#L149
            GraphicsState prevState = e.Graphics.Save();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle checkBounds = layout.CheckBounds;
            checkBounds.Width--;
            checkBounds.Height--;
            checkBounds.Inflate(-padding, -padding);
            e.Graphics.FillEllipse(brush, checkBounds);
            e.Graphics.Restore(prevState);
        }

        protected void AdjustFocusRectangle(ControlAppearanceState state, LayoutData layout)
        {
            if (String.IsNullOrEmpty(state.Text))
            {
                // When a RadioButton has no text, AutoSize sets the size to zero
                // and thus there's no place around which to draw the focus rectangle.
                // So, when AutoSize == true we want the focus rectangle to be rendered around the circle area.
                // Otherwise, it should encircle all the available space next to the box (like it's done in WPF and ComCtl32).
                layout.Focus = ButtonInstance.AutoSize ? layout.CheckBounds : layout.Field;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
