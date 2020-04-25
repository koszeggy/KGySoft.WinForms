using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides a class for capturing screenshots.
    /// </summary>
    public static class Screenshot
    {
        /// <summary>
        /// Takes a screenshot of the given bounds.
        /// </summary>
        /// <returns>The <see cref="Image"/> of the screenshot</returns>
        public static Image CaptureScreenshot(Rectangle bounds)
        {
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return screenshot;
        }

        /// <summary>
        /// Takes a screenshot of all of the screens.
        /// </summary>
        /// <returns>The <see cref="Image"/> of the screenshot</returns>
        public static Image CaptureScreenshot()
        {
            return CaptureScreenshot(Screen.AllScreens.Aggregate(Rectangle.Empty, (current, s) => Rectangle.Union(current, s.Bounds)));
        }
    }
}
