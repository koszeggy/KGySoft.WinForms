#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RectangleExtensions.cs
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
using System.Runtime.CompilerServices;
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
            // Framework Mono: AllScreens may always return a single screen, even in multi-display environment.
            if (OSHelper.IsFrameworkMono && Screen.AllScreens.Length <= 1)
            {
                if (!forceSingleScreen)
                    return suggestedBounds;

                // If single screen is forced, and suggested bounds already mainly cover the specified screen (which may or may not be the only screen),
                // then we assume that screen is really the one that we want to use. Cannot use Screen.From... here, as it always would return the primary screen.
                Rectangle overlap = suggestedBounds.IntersectSafe(screen.Bounds);
                if (overlap.Width < suggestedBounds.Width / 2 || overlap.Height < suggestedBounds.Height / 2)
                    return suggestedBounds; // too small overlap, assuming multiple screens that we cannot query
            }

            Rectangle screenBounds;
            if (!forceSingleScreen)
            {
                if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                    return suggestedBounds;

                // If the suggested rectangle not on the given screen, then we perform some adjustment.
                // Due to typical mouse dragging scenario we prefer horizontal adjustment in the first place,
                // so we check if ensuring half, 2/3, 3/4 or the whole the rectangle is on the given screen horizontally solves the problem.
                screenBounds = screen.WorkingArea;
                foreach (int minimumWidth in new[] { suggestedBounds.Width / 2 + 1, suggestedBounds.Width * 2 / 3, suggestedBounds.Width * 3 / 4 })
                {
                    if (screenBounds.IntersectSafe(suggestedBounds).Width < minimumWidth)
                    {
                        if (suggestedBounds.Left < screenBounds.Left)
                            suggestedBounds.X = screenBounds.Left + minimumWidth - suggestedBounds.Width;
                        else if (suggestedBounds.Right > screenBounds.Right)
                            suggestedBounds.X = screenBounds.Right - minimumWidth;

                        if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                            return suggestedBounds;
                    }
                }

                // After the adjustments above at least the 3/4 of the rectangle should be on the given screen horizontally,
                // so vertically we ensure only the half of the rectangle.
                int minimumHeight = suggestedBounds.Height / 2 + 1;
                if (screenBounds.IntersectSafe(suggestedBounds).Height < minimumHeight)
                {
                    if (suggestedBounds.Top < screenBounds.Top)
                        suggestedBounds.Y = screenBounds.Top + minimumHeight - suggestedBounds.Height;
                    else if (suggestedBounds.Bottom > screenBounds.Bottom)
                        suggestedBounds.Y = screenBounds.Bottom - minimumHeight;
                }

                if (Screen.FromRectangle(suggestedBounds).Equals(screen))
                    return suggestedBounds;
            }

            // here ensuring that the suggested bounds are entirely within the given screen - we may reach this point even when it is not forced
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

            // If the rectangle is still not on the given screen, then it must be (much) bigger than the screen.
            // In this case we center it on the screen - this time using the screen bounds instead of the working area.
            // NOTE: this may mean that the top is not visible, because if we adjust the top, Screen.FromRectangle returns another screen.
            return FromCenter(screen.Bounds.GetCenter(), suggestedBounds.Size);
        }

        /// <summary>
        /// Gets whether the rectangle has zero Width OR Height.
        /// Not just faster than the IsEmpty property but also works better when Intersect returns a non-default practically zero rectangle.
        /// </summary>
        public static bool IsEmpty(this Rectangle rect) => rect.Width == 0 || rect.Height == 0;

        /// <summary>
        /// Like Rectangle.Intersect, but works with big ranges, and returns Rectangle.Empty if the result would be a practically zero rectangle.
        /// </summary>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Rectangle IntersectSafe(this Rectangle a, Rectangle b)
        {
            int x1 = Math.Max(a.X, b.X);
            long x2 = Math.Min((long)a.X + a.Width, (long)b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            long y2 = Math.Min((long)a.Y + a.Height, (long)b.Y + b.Height);

            // The original Rectangle.Intersect method has >= checks, which can return non-default zero height or width rectangles.
            if (x2 > x1 && y2 > y1)
                // The (int) cast is safe because the result is guaranteed to be in the int range as intersection can only reduce height and width.
                return new Rectangle(x1, y1, (int)(x2 - x1), (int)(y2 - y1));

            return Rectangle.Empty;
        }

        #endregion
    }
}