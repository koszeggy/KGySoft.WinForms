using System.Windows.Forms;

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Represents the event data for the <see cref="BaseForm.OwnedMdiChildClosed"/> event.
    /// </summary>
    public class OwnedMdiChildClosedEventArgs : FormClosedEventArgs
    {
        /// <summary>
        /// Gets the close MDI child form.
        /// </summary>
        public Form MdiChild { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedMdiChildClosedEventArgs"/> class.
        /// </summary>
        /// <param name="mdiChild">The MDI child form that was closed.</param>
        /// <param name="closeReason">The reason why the MDI child was closed.</param>
        public OwnedMdiChildClosedEventArgs(Form mdiChild, CloseReason closeReason) : base(closeReason) => MdiChild = mdiChild;
    }
}
