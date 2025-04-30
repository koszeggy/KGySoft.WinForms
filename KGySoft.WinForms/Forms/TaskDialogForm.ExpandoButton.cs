#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogForm.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Forms
{
    #region Usings

    using Resources = Properties.Resources;
    
    #endregion

    partial class TaskDialogForm
    {
        private sealed class ExpandoButton : AdvancedButton
        {
            #region Fields

            #region Static Fields

            static readonly Size imageSize = new Size(19, 21);

            #endregion

            #region Instance Fields

            private bool isHovered;
            private bool isMouseDown;
            private bool isPressed;
            private string? textExpanded;
            private string? textCollapsed;
            private bool isExpanded;

            #endregion

            #endregion

            #region Events

            internal event EventHandler? ExpandedChanged;

            #endregion

            #region Properties

            #region Public Properties

            [AllowNull]
            public override string Text
            {
                get => base.Text;
                set
                {
                    if (isExpanded)
                        TextExpanded = value;
                    else
                        TextCollapsed = value;
                }
            }

            #endregion

            #region Internal Properties

            internal bool IsExpanded
            {
                get => isExpanded;
                set
                {
                    if (isExpanded == value)
                        return;

                    isExpanded = value;
                    base.Text = isExpanded ? textExpanded : textCollapsed;
                    if (ExpandedChanged != null)
                        ExpandedChanged.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }

            internal string? TextExpanded
            {
                get => textExpanded;
                set
                {
                    if (textExpanded == value)
                        return;

                    textExpanded = value;
                    if (isExpanded)
                        base.Text = value;

                    PerformLayout();
                    //AdjustHeight();
                }
            }

            internal string? TextCollapsed
            {
                get => textCollapsed;
                set
                {
                    if (textCollapsed == value)
                        return;

                    textCollapsed = value;
                    if (!isExpanded)
                        base.Text = value;

                    PerformLayout();
                    //AdjustHeight();
                }
            }

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            public override Size GetPreferredSize(Size proposedSize)
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SetQuality();
                    return LayoutUtils.UnionSizes(imageSize, GetTextSize(g, null, proposedSize) + new Size(0, 1)) // +1 for focus rectangle
                        + new Size(Margin.Left + imageSize.Width, Margin.Top);
                }
            }

            #endregion

            #region Protected Methods

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                IsExpanded = !isExpanded;
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                isHovered = false;
                Invalidate();
                base.OnMouseLeave(e);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                isHovered = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                isPressed = false;
                isMouseDown = false;
                Invalidate();
                base.OnMouseUp(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                isPressed = e.Button == MouseButtons.Left;
                isMouseDown = isPressed;
                Invalidate();
                base.OnMouseDown(e);
            }

            protected override void OnMouseMove(MouseEventArgs mevent)
            {
                if (isMouseDown)
                    isPressed = mevent.X >= 0 && mevent.X < Width && mevent.Y >= 0 && mevent.Y < Height;

                base.OnMouseMove(mevent);
            }

            protected override void OnPaintState(PaintStateEventArgs e)
            {
                using (Brush b = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                if (Application.RenderWithVisualStyles)
                {
                    if (WindowsUtils.IsVistaOrLater)
                        PaintNativeButton(e.Graphics);
                    else
                        PaintThemedButton(e.Graphics);
                }
                else
                {
                    PaintClassicButton(e.Graphics);
                }

                Size textSize = GetTextSize(e.Graphics, isExpanded, Size);
                TextFormatFlags formatFlags = this.GetFormatFlags(); //TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;
                Rectangle textRect = new Rectangle(Margin.Left + imageSize.Width, Margin.Top, textSize.Width, textSize.Height);
                TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, formatFlags);
                if (ShowFocusCues && Enabled && (IsDefault || Focused))
                {
                    textRect.Inflate(0, 1);
                    ControlPaint.DrawFocusRectangle(e.Graphics, textRect, ForeColor, BackColor);
                }
            }

            #endregion

            #region Private Methods

            //private void AdjustHeight()
            //{
            //    using (Graphics g = Graphics.FromHwnd(Handle))
            //    {
            //        Height = Math.Max(imageSize.Height, GetTextSize(g, null).Height + Margin.Vertical);
            //    }
            //}

            private Size GetTextSize(Graphics g, bool? expanded, Size proposedSize)
            {
                if (proposedSize.Width <= 1)
                    proposedSize.Width = Int32.MaxValue;
                if (proposedSize.Height <= 1)
                    proposedSize.Height = Int32.MaxValue;

                proposedSize -= new Size(Margin.Left + imageSize.Width, Margin.Top);
                TextFormatFlags flags = this.GetFormatFlags();
                return LayoutUtils.UnionSizes(!expanded.HasValue || expanded.Value ? TextRenderer.MeasureText(g, TextExpanded, Font, proposedSize, flags) : Size.Empty,
                    !expanded.HasValue || !expanded.Value ? TextRenderer.MeasureText(g, TextCollapsed, Font, proposedSize, flags) : Size.Empty);
            }

            private void PaintNativeButton(Graphics g)
            {
                EXPANDOBUTTONSTATES state;
                if (!isExpanded)
                {
                    if (isPressed)
                        state = EXPANDOBUTTONSTATES.TDLGEBS_PRESSED;
                    else if (isHovered)
                        state = EXPANDOBUTTONSTATES.TDLGEBS_HOVER;
                    else
                        state = EXPANDOBUTTONSTATES.TDLGEBS_NORMAL;
                }
                else
                {
                    if (isPressed)
                        state = EXPANDOBUTTONSTATES.TDLGEBS_EXPANDEDPRESSED;
                    else if (isHovered)
                        state = EXPANDOBUTTONSTATES.TDLGEBS_EXPANDEDHOVER;
                    else
                        state = EXPANDOBUTTONSTATES.TDLGEBS_EXPANDEDNORMAL;
                }

                VisualStyleRenderer renderer = new VisualStyleRenderer("TASKDIALOG", Constants.TDLG_EXPANDOBUTTON, (int)state);
                renderer.DrawBackground(g, new Rectangle(Point.Empty, renderer.GetPartSize(g, ThemeSizeType.True)));
            }

            private void PaintThemedButton(Graphics g)
            {
                Image image;
                if (!isExpanded)
                {
                    if (isPressed)
                        image = Resources.ExpandoPressedDown;
                    else if (isHovered)
                        image = Resources.ExpandoHoveredDown;
                    else
                        image = Resources.ExpandoNormalDown;
                }
                else
                {
                    if (isPressed)
                        image = Resources.ExpandoPressedUp;
                    else if (isHovered)
                        image = Resources.ExpandoHoveredUp;
                    else
                        image = Resources.ExpandoNormalUp;
                }

                g.DrawImage(image, new Point(0, 1));
            }

            private void PaintClassicButton(Graphics g)
            {
                Rectangle rect = new Rectangle(Point.Empty, imageSize);
                ButtonState state = ButtonState.Normal;
                if (isPressed)
                    state = ButtonState.Pushed;

                ControlPaint.DrawComboButton(g, rect, state);

                if (isExpanded || isHovered)
                {
                    Bitmap image = new Bitmap(imageSize.Width, imageSize.Height, g);
                    using (Graphics imageGraphics = Graphics.FromImage(image))
                    {
                        ControlPaint.DrawComboButton(imageGraphics, rect, state);
                    }

                    int offset = 0;
                    if (isExpanded)
                    {
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        if (isPressed)
                            offset = 2;
                    }

                    ImageAttributes? attr = null;
                    if (isHovered && !isPressed)
                    {
                        attr = new ImageAttributes();
                        ColorMap map = new ColorMap { OldColor = SystemColors.ControlText, NewColor = SystemColors.HotTrack };
                        attr.SetRemapTable(new ColorMap[] { map }, ColorAdjustType.Bitmap);
                    }

                    rect.Inflate(-4, -4);
                    g.DrawImage(image, new Rectangle(rect.Left + offset, rect.Top + offset, rect.Width, rect.Height),
                        rect.Left, rect.Top, rect.Width, rect.Height, GraphicsUnit.Pixel, attr);
                }
            }

            #endregion

            #endregion
        }
    }
}

