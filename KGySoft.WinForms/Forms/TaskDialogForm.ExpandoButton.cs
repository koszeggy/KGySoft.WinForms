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

using KGySoft.Drawing;
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

            static readonly Size referenceImageSize = new Size(19, 21);

            #endregion

            #region Instance Fields

            private bool isHovered;
            private bool isMouseDown;
            private bool isPressed;
            private bool isExpanded;
            private string? textExpanded;
            private string? textCollapsed;
            private Image? cachedDefaultImageNormalDown;
            private Image? cachedDefaultImageHoveredDown;
            private Image? cachedDefaultImagePressedDown;
            private Image? cachedDefaultImageNormalUp;
            private Image? cachedDefaultImageHoveredUp;
            private Image? cachedDefaultImagePressedUp;

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

            #region Private Properties

            private Image DefaultImageNormalDown => cachedDefaultImageNormalDown ??= ExtractBitmap(Resources.ExpandoNormalDown);
            private Image DefaultImageHoveredDown => cachedDefaultImageHoveredDown ??= ExtractBitmap(Resources.ExpandoHoveredDown);
            private Image DefaultImagePressedDown => cachedDefaultImagePressedDown ??= ExtractBitmap(Resources.ExpandoPressedDown);
            private Image DefaultImageNormalUp => cachedDefaultImageNormalUp ??= ExtractBitmap(Resources.ExpandoNormalUp);
            private Image DefaultImageHoveredUp => cachedDefaultImageHoveredUp ??= ExtractBitmap(Resources.ExpandoHoveredUp);
            private Image DefaultImagePressedUp => cachedDefaultImagePressedUp ??= ExtractBitmap(Resources.ExpandoPressedUp);

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            public override Size GetPreferredSize(Size proposedSize)
            {
                using Graphics g = Graphics.FromHwnd(Handle);
                g.SetTextRenderingQuality(TextRenderingQuality, UseCompatibleTextRendering);
                var imageSize = GetImageSize(g);
                return LayoutUtils.UnionSizes(imageSize, GetTextSize(g, imageSize, null, proposedSize) + new Size(0, 1)) // +1 for focus rectangle
                    + new Size(Margin.Left + imageSize.Width, Margin.Top);
            }

            #endregion

            #region Internal Methods

            internal void ResetTheme()
            {
                cachedDefaultImageNormalDown?.Dispose();
                cachedDefaultImageHoveredDown?.Dispose();
                cachedDefaultImagePressedDown?.Dispose();
                cachedDefaultImageNormalUp?.Dispose();
                cachedDefaultImageHoveredUp?.Dispose();
                cachedDefaultImagePressedUp?.Dispose();
                cachedDefaultImageNormalDown = null;
                cachedDefaultImageHoveredDown = null;
                cachedDefaultImagePressedDown = null;
                cachedDefaultImageNormalUp = null;
                cachedDefaultImageHoveredUp = null;
                cachedDefaultImagePressedUp = null;
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
                Graphics g = e.Graphics;
                using (Brush b = new SolidBrush(BackColor))
                {
                    g.FillRectangle(b, ClientRectangle);
                }

                Size imageSize;
                if (VisualStyleHelper.RenderWithVisualStyles)
                {
                    if (WindowsUtils.IsVistaOrLater)
                        PaintNativeButton(g, out imageSize);
                    else
                        PaintThemedButton(g, out imageSize);
                }
                else
                    PaintClassicButton(g, out imageSize);

                Size textSize = GetTextSize(g, imageSize, isExpanded, Size);
                TextFormatFlags formatFlags = this.GetFormatFlags(); //TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;
                Rectangle textRect = new Rectangle(Margin.Left + imageSize.Width, Margin.Top, textSize.Width, textSize.Height);
                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, formatFlags);
                if (ShowFocusCues && Enabled && (IsDefault || Focused))
                {
                    textRect.Inflate(0, 1);
                    ControlPaint.DrawFocusRectangle(g, textRect, ForeColor, BackColor);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    cachedDefaultImageNormalDown?.Dispose();
                    cachedDefaultImageHoveredDown?.Dispose();
                    cachedDefaultImagePressedDown?.Dispose();
                    cachedDefaultImageNormalUp?.Dispose();
                    cachedDefaultImageHoveredUp?.Dispose();
                    cachedDefaultImagePressedUp?.Dispose();
                }
                base.Dispose(disposing);
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

            private Size GetImageSize(Graphics g)
            {
                if (VisualStyleHelper.RenderWithVisualStyles)
                {
                    if (WindowsUtils.IsVistaOrLater)
                        return VisualStyleHelper.GetPartSize(VisualStyleHelper.TaskDialogTheme, this, g, Constants.TDLG_EXPANDOBUTTON, (int)EXPANDOBUTTONSTATES.TDLGEBS_NORMAL, true);
                    return DefaultImageNormalDown.Size;
                }

                return referenceImageSize.Scale(g.GetScale());
            }

            private Image ExtractBitmap(Icon icon)
            {
                try
                {
                    Size desiredSize = this.ScaleSize(referenceImageSize);
                    Bitmap scaledDefaultGlyph = icon.ExtractNearestBitmap(desiredSize, PixelFormat.Format32bppArgb);
                    if (scaledDefaultGlyph.Width >= desiredSize.Width || desiredSize.Width < scaledDefaultGlyph.Width * 1.25f)
                        return scaledDefaultGlyph;

                    var resizedDefaultGlyph = scaledDefaultGlyph.Resize(desiredSize);
                    scaledDefaultGlyph.Dispose();
                    return resizedDefaultGlyph;
                }
                finally
                {
                    icon.Dispose();
                }
            }

            private Size GetTextSize(Graphics g, Size imageSize, bool? expanded, Size proposedSize)
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

            private void PaintNativeButton(Graphics g, out Size imageSize)
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

                imageSize = GetImageSize(g);
                VisualStyleHelper.Render(VisualStyleHelper.TaskDialogTheme, this, g, Constants.TDLG_EXPANDOBUTTON, (int)state, new Rectangle(Point.Empty, imageSize));
            }

            private void PaintThemedButton(Graphics g, out Size imageSize)
            {
                Image image;
                if (!isExpanded)
                {
                    image = isPressed ? DefaultImagePressedDown
                        : isHovered ? DefaultImageHoveredDown
                        : DefaultImageNormalDown;
                }
                else
                {
                    image = isPressed ? DefaultImagePressedUp
                        : isHovered ? DefaultImageHoveredUp
                        : DefaultImageNormalUp;
                }

                imageSize = image.Size;
                g.DrawImage(image, new Rectangle(Point.Empty, imageSize));
            }

            private void PaintClassicButton(Graphics g, out Size imageSize)
            {
                imageSize = referenceImageSize.Scale(g.GetScale());
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

