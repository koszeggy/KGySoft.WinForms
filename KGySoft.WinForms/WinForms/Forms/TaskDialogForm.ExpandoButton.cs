#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogForm.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

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

            // these must not be disposed, they are just references to statically cached images
            private Bitmap? cachedDefaultImageNormalDown;
            private Bitmap? cachedDefaultImageHoveredDown;
            private Bitmap? cachedDefaultImagePressedDown;
            private Bitmap? cachedDefaultImageNormalUp;
            private Bitmap? cachedDefaultImageHoveredUp;
            private Bitmap? cachedDefaultImagePressedUp;

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
                    ExpandedChanged?.Invoke(this, EventArgs.Empty);
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

            private Bitmap DefaultImageNormalDown => cachedDefaultImageNormalDown ??= ExtractBitmap(Resources.ExpandoNormalDown, nameof(Resources.ExpandoNormalDown));
            private Bitmap DefaultImageHoveredDown => cachedDefaultImageHoveredDown ??= ExtractBitmap(Resources.ExpandoHoveredDown, nameof(Resources.ExpandoHoveredDown));
            private Bitmap DefaultImagePressedDown => cachedDefaultImagePressedDown ??= ExtractBitmap(Resources.ExpandoPressedDown, nameof(Resources.ExpandoPressedDown));
            private Bitmap DefaultImageNormalUp => cachedDefaultImageNormalUp ??= ExtractBitmap(Resources.ExpandoNormalUp, nameof(Resources.ExpandoNormalUp));
            private Bitmap DefaultImageHoveredUp => cachedDefaultImageHoveredUp ??= ExtractBitmap(Resources.ExpandoHoveredUp, nameof(Resources.ExpandoHoveredUp));
            private Bitmap DefaultImagePressedUp => cachedDefaultImagePressedUp ??= ExtractBitmap(Resources.ExpandoPressedUp, nameof(Resources.ExpandoPressedUp));

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            public override Size GetPreferredSize(Size proposedSize)
            {
                var imageSize = GetImageSize();
                if (!IsHandleCreated && OSHelper.IsWindowsMono)
                    return LayoutUtils.UnionSizes(imageSize, proposedSize);
                using Graphics g = Graphics.FromHwnd(IsHandleCreated ? Handle : IntPtr.Zero);
                g.SetTextRenderingQuality(TextRenderingQuality, UseCompatibleTextRendering);
                return LayoutUtils.UnionSizes(imageSize, GetTextSize(g, imageSize, null, proposedSize) + new Size(0, 1)) // +1 for focus rectangle
                    + new Size(Margin.Left + imageSize.Width, Margin.Top);
            }

            #endregion

            #region Internal Methods

            internal void ResetTheme()
            {
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
                g.FillRectangle(BackColor.GetBrush(), ClientRectangle);

                Size imageSize;
                if (VisualStyleHelper.RenderWithVisualStyles)
                {
                    if (OSHelper.IsWindowsVistaOrLater)
                        PaintNativeButton(g, out imageSize);
                    else
                        PaintThemedButton(g, out imageSize);
                }
                else
                    PaintClassicButton(g, out imageSize);

                Size textSize = GetTextSize(g, imageSize, isExpanded, Size);
                TextFormatFlags formatFlags = this.GetFormatFlags();
                Rectangle textRect = new Rectangle(Margin.Left + imageSize.Width, Margin.Top, textSize.Width, textSize.Height);
                if ((formatFlags & TextFormatFlags.RightToLeft) != 0)
                    textRect.X = ClientRectangle.Right - textRect.Right;
                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, formatFlags);
                if (ShowFocusCues && Enabled && (IsDefault || Focused))
                {
                    textRect.Inflate(0, 1);
                    ControlPaint.DrawFocusRectangle(g, textRect, ForeColor, BackColor);
                }
            }

            #endregion

            #region Private Methods

            private Size GetImageSize() => referenceImageSize.Scale(this.GetScale());

            private Rectangle GetButtonBounds(Size imageSize)
            {
                var result = new Rectangle(Point.Empty, imageSize);
                if (RightToLeft == RightToLeft.Yes)
                    result.X = ClientRectangle.Right - imageSize.Width;
                return result;
            }

            private Bitmap ExtractBitmap(Icon icon, string name)
            {
                try
                {
                    Size desiredSize = this.ScaleSize(referenceImageSize);
                    return icon.GetCachedBitmap(name, desiredSize);
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

                imageSize = GetImageSize();
                IntPtr hwnd = Handle;
                if (imageSize == VisualStyleHelper.GetPartSize(Constants.ThemeClassTaskDialog, hwnd, g, Constants.TDLG_EXPANDOBUTTON, (int)EXPANDOBUTTONSTATES.TDLGEBS_NORMAL, true))
                    VisualStyleHelper.Render(Constants.ThemeClassTaskDialog, hwnd, g, Constants.TDLG_EXPANDOBUTTON, (int)state, GetButtonBounds(imageSize));
                else
                    VisualStyleHelper.RenderScaled(Constants.ThemeClassTaskDialog, hwnd, g, Constants.TDLG_EXPANDOBUTTON, (int)state, GetButtonBounds(imageSize));
            }

            private void PaintThemedButton(Graphics g, out Size imageSize)
            {
                Bitmap image;
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
                g.DrawImage(image, GetButtonBounds(imageSize));
            }

            private void PaintClassicButton(Graphics g, out Size imageSize)
            {
                imageSize = GetImageSize();
                Rectangle rect = GetButtonBounds(imageSize);

                g.DrawBorder(isPressed ? AdvancedBorderStyle.SunkenLow : AdvancedBorderStyle.RaisedHigh, rect);
                rect.Inflate(-2, -2);

                if (isPressed)
                    rect.Offset(1, 1);

                Color color = isHovered && !isPressed ? SystemColors.HotTrack : SystemColors.ControlText;
                g.DrawImageColorized(ControlPaintHelper.GetArrowImage(rect.Size, isExpanded), rect, color);
            }


            #endregion

            #endregion
        }
    }
}

