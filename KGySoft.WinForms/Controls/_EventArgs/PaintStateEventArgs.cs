#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents arguments of a PaintState event.
    /// </summary>
    public class PaintStateEventArgs : PaintEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the appearance state of the control for the painting.
        /// </summary>
        public ControlAppearanceState State { get; private set; }

        #endregion

        #region Constructors

        internal PaintStateEventArgs(Graphics g, Rectangle clipRect, ControlAppearanceState state)
            : base(g, clipRect)
        {
            State = state;
        }

        #endregion
    }
}
