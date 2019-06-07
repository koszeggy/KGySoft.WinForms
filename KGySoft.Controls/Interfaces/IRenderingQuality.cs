using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using KGySoft.Drawing;

namespace KGySoft.Controls
{
    internal interface IRenderingQuality
    {
        /// <summary>
        /// Gets or sets the rendering quality of the control.
        /// </summary>
        RenderingQuality RenderingQuality { get; set; }
    }
}
