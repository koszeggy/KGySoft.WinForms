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
    /// <summary>
    /// Provides extension methods for the <see cref="Rectangle"/> structure.
    /// </summary>
    public static class RectangleExtensions
    {
        #region Methods

        /// <summary>
        /// Gets the center point of the rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to get the center point of.</param>
        /// <returns>The center point of the rectangle.</returns>
        public static Point GetCenter(this Rectangle rect) => rect.Location + new Size(rect.Size.Width / 2, rect.Size.Height / 2);

        /// <summary>
        /// Creates a <see cref="Rectangle"/> with the specified center point and size.
        /// </summary>
        /// <param name="center">The <see cref="Point"/> representing the center of the rectangle.</param>
        /// <param name="size">The <see cref="Size"/> representing the width and height of the rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> with the specified center and size.</returns>
        public static Rectangle FromCenter(Point center, Size size) => new(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);

        /// <summary>
        /// Adjusts the specified rectangle to ensure it is positioned within the given screen's working area.
        /// </summary>
        /// <param name="suggestedBounds">The initial rectangle bounds to be adjusted.</param>
        /// <param name="screen">The screen within which the rectangle should be positioned.</param>
        /// <param name="forceSingleScreen"><see langword="true"/> to ensure the rectangle is entirely contained within the specified screen if possible;
        /// <see langword="false"/> to ensure that the largest part of the rectangle is on the specified screen.</param>
        /// <returns>The adjusted <see cref="Rectangle"/>.</returns>
        public static Rectangle EnsureScreen(this Rectangle suggestedBounds, Screen screen, bool forceSingleScreen)
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

        /// <summary>
        /// Gets whether the rectangle has zero Width OR Height.
        /// Not just faster than the IsEmpty property but also works better when Intersect returns a non-default practically zero rectangle.
        /// </summary>
        public static bool IsEmpty(this Rectangle rect) => rect.Width == 0 || rect.Height == 0;

        #endregion
    }
}