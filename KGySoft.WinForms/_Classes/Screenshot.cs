#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Screenshot.cs
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
using System.Linq;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides a class for capturing screenshots.
    /// </summary>
    public static class Screenshot
    {
        #region Methods

        /// <summary>
        /// Takes a screenshot of the given bounds.
        /// </summary>
        /// <returns>The <see cref="Image"/> of the screenshot</returns>
        public static Image CaptureScreenshot(Rectangle bounds)
        {
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using Graphics g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);

            return screenshot;
        }

        /// <summary>
        /// Takes a screenshot of every screen.
        /// </summary>
        /// <returns>The <see cref="Image"/> of the screenshot</returns>
        public static Image CaptureScreenshot()
            => CaptureScreenshot(Screen.AllScreens.Aggregate(Rectangle.Empty, (current, s) => Rectangle.Union(current, s.Bounds)));

        #endregion
    }
}