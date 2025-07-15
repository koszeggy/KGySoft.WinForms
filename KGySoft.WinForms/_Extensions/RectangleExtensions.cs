#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RectangleExtensions.cs
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

#endregion

namespace KGySoft.WinForms
{
    internal static class RectangleExtensions
    {
        #region Methods

        internal static Point GetCenter(this Rectangle rect) => rect.Location + new Size(rect.Size.Width / 2, rect.Size.Height / 2);

        internal static Rectangle FromCenter(Point center, Size size) => new(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);

        internal static Rectangle EnsureScreen(this Rectangle suggestedBounds, Screen screen, bool forceSingleScreen)
        {
            Rectangle screenBounds;
            if (!forceSingleScreen)
            {
                if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                    return suggestedBounds;

                // if the suggested bounds is not on the given screen, then we perform the minimum adjustment to ensure that it is on the given screen
                screenBounds = screen.WorkingArea;
                int minimumWidth = suggestedBounds.Width / 2 + 1;
                if (Rectangle.Intersect(screenBounds, suggestedBounds).Width < minimumWidth)
                {
                    if (suggestedBounds.Left < screenBounds.Left)
                        suggestedBounds.X = screenBounds.Left + minimumWidth - suggestedBounds.Width;
                    else if (suggestedBounds.Right > screenBounds.Right)
                        suggestedBounds.X = screenBounds.Right - minimumWidth;
                }

                int minimumHeight = suggestedBounds.Height / 2 + 1;
                if (Rectangle.Intersect(screenBounds, suggestedBounds).Height < minimumHeight)
                {
                    if (suggestedBounds.Top < screenBounds.Top)
                        suggestedBounds.Y = screenBounds.Top + minimumHeight - suggestedBounds.Height;
                    else if (suggestedBounds.Bottom > screenBounds.Bottom)
                        suggestedBounds.Y = screenBounds.Bottom - minimumHeight;
                }

                if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                    return suggestedBounds;
            }

            // the suggested bounds is not on the given screen, then adjusting it, ensuring that it is entirely on the given screen
            screenBounds = screen.WorkingArea;
            if (suggestedBounds.Left < screenBounds.Left)
                suggestedBounds.X = screenBounds.Left;
            else if (suggestedBounds.Right > screenBounds.Right)
                suggestedBounds.X = screenBounds.Right - suggestedBounds.Width;

            if (suggestedBounds.Top < screenBounds.Top)
                suggestedBounds.Y = screenBounds.Top;
            else if (suggestedBounds.Bottom > screenBounds.Bottom)
                suggestedBounds.Y = screenBounds.Bottom - suggestedBounds.Height;

            if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                return suggestedBounds;

            // if the bounds is still not on the given screen, then we need to center it on the screen - this time using the screen bounds instead of the working area
            return FromCenter(screen.Bounds.GetCenter(), suggestedBounds.Size);
        }

        #endregion

    }
}