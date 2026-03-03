#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RadioButtonBaseAdapter.cs
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

#region Used Namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.Drawing.Shapes;

#endregion

#region Used Aliases

using Brush = System.Drawing.Brush;
using Pen = System.Drawing.Pen;

#endregion

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class RadioButtonBaseAdapter : CheckableControlBaseAdapter
    {
        #region Properties

        protected AdvancedRadioButton RadioButtonInstance => (AdvancedRadioButton)ButtonInstance;

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

            if (OSHelper.IsWine)
            {
                // NOTE: These are the values of the original code, but that uses a lower level HDC drawing, which ends up in a different result when drawing by Graphics:
                // On Wine (both with and without Mono) it works as the original HDC drawing though.
                // https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/System/Windows/Forms/Controls/Buttons/ButtonInternal/RadioButtonBaseAdapter.cs

                graphics.DrawLine(borderPen, new(bounds.X + 4, bounds.Y), new(bounds.X + 8, bounds.Y));
                graphics.DrawLine(borderPen, new(bounds.X + 4, bounds.Y + 11), new(bounds.X + 8, bounds.Y + 11));
                graphics.DrawLine(borderPen, new(bounds.X + 2, bounds.Y + 1), new(bounds.X + 4, bounds.Y + 1));
                graphics.DrawLine(borderPen, new(bounds.X + 8, bounds.Y + 1), new(bounds.X + 10, bounds.Y + 1));
                graphics.DrawLine(borderPen, new(bounds.X + 2, bounds.Y + 10), new(bounds.X + 4, bounds.Y + 10));
                graphics.DrawLine(borderPen, new(bounds.X + 8, bounds.Y + 10), new(bounds.X + 10, bounds.Y + 10));
                graphics.DrawLine(borderPen, new(bounds.X + 0, bounds.Y + 4), new(bounds.X + 0, bounds.Y + 8));
                graphics.DrawLine(borderPen, new(bounds.X + 11, bounds.Y + 4), new(bounds.X + 11, bounds.Y + 8));
                graphics.DrawLine(borderPen, new(bounds.X + 1, bounds.Y + 2), new(bounds.X + 1, bounds.Y + 4));
                graphics.DrawLine(borderPen, new(bounds.X + 1, bounds.Y + 8), new(bounds.X + 1, bounds.Y + 10));
                graphics.DrawLine(borderPen, new(bounds.X + 10, bounds.Y + 2), new(bounds.X + 10, bounds.Y + 4));
                graphics.DrawLine(borderPen, new(bounds.X + 10, bounds.Y + 8), new(bounds.X + 10, bounds.Y + 10));
                return;
            }

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

        protected void DrawCheckBackgroundFlat(PaintStateEventArgs e, Rectangle bounds, PointF scale, Color borderColor, Color checkBackground, bool disabledColors)
        {
            ControlAppearanceState state = e.State;
            Color backColor = checkBackground;
            Color foreColor = borderColor;
            if (!state.Enabled && disabledColors)
            {
                foreColor = VisualStyleHelper.HighContrast ? SystemColors.WindowFrame : state.ForeColor;
                backColor = state.BackColor;
            }

            if (scale.X > 1.1f || RadioButtonInstance.VisualsRenderingQuality == RenderingQuality.High)
            {
                // Wine issue: the anti-aliased ellipse has a poor quality under Wine, so drawing it into a bitmap
                if (OSHelper.IsWine)
                {
                    using var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
                    using (var bmpData = bmp.GetReadWriteBitmapData())
                    {
                        var options = new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half };
                        bmpData.FillEllipse(backColor, 0, 0, bounds.Width, bounds.Height, options);
                        bmpData.DrawEllipse(foreColor, 0, 0, bounds.Width - 1, bounds.Height - 1, options);
                    }

                    e.Graphics.DrawImage(bmp, bounds);
                    return;
                }

                bounds.Width--;
                bounds.Height--;
                GraphicsState prevState = e.Graphics.Save();
                if (RadioButtonInstance.VisualsRenderingQuality == RenderingQuality.High)
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                e.Graphics.FillEllipse(backColor.GetBrush(), bounds);
                e.Graphics.DrawEllipse(foreColor.GetPen(), bounds);
                e.Graphics.Restore(prevState);

                return;
            }

            DrawAndFillEllipse(e.Graphics, foreColor.GetPen(), backColor.GetBrush(), bounds);
        }

        protected void DrawCheckOnly(PaintStateEventArgs e, LayoutData layout, Color checkColor)
        {
            ControlAppearanceState state = e.State;
            if (state.CheckState == CheckState.Unchecked)
                return;
            Brush brush = checkColor.GetBrush();
            PointF scale = layout.Options.Scale;

            // Original code
            if (RadioButtonInstance.VisualsRenderingQuality != RenderingQuality.High || scale.X <= 1.1f)
            {
                Size scaledSize = new Size(2, 4).Scale(scale);
                Point middle = new Point(layout.CheckBounds.X + layout.CheckBounds.Width / 2, layout.CheckBounds.Y + layout.CheckBounds.Height / 2);
                Rectangle vCross = new Rectangle(middle.X - scaledSize.Width / 2, middle.Y - scaledSize.Height / 2, scaledSize.Width, scaledSize.Height);
                e.Graphics.FillRectangle(brush, vCross);
                Rectangle hCross = new Rectangle(middle.X - scaledSize.Height / 2, middle.Y - scaledSize.Width / 2, scaledSize.Height, scaledSize.Width);
                e.Graphics.FillRectangle(brush, hCross);
                return;
            }

            // This scaled rendering differs from the original because that one is very ugly e.g. at 150%: https://github.com/dotnet/winforms/blob/1c324d074280ab5de6342d973069faa687f2c165/src/System.Windows.Forms/System/Windows/Forms/Controls/Buttons/ButtonInternal/RadioButtonBaseAdapter.cs#L149
            int padding = 5;
            GraphicsState prevState = e.Graphics.Save();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle checkBounds = layout.CheckBounds;
            if (!OSHelper.IsFrameworkMono)
            {
                checkBounds.Width--;
                checkBounds.Height--;
            }

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
