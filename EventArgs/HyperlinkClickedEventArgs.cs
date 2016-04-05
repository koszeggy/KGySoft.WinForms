using System.ComponentModel;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contains arguments of <see cref="TaskDialog.HyperlinkClicked"/> and <see cref="AdvancedLabel.HyperlinkClicked"/> events.
    /// </summary>
    public sealed class HyperlinkClickedEventArgs: HandledEventArgs
    {
        /// <summary>
        /// Gets the hyperlink that was clicked. If resolving was not handled,
        /// the <see cref="HandledEventArgs.Handled"/> property can be set to <c>false</c>
        /// to make the system resolve the hyperlink.
        /// </summary>
        public string Hyperlink { get; private set; }

        internal HyperlinkClickedEventArgs(string link)
            :base(true)
        {
            Hyperlink = link;
        }
    }
}
