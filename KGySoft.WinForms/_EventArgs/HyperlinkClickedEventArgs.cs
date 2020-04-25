using System.ComponentModel;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms
{
    /// <summary>
    /// Contains arguments of <see cref="TaskDialog.HyperlinkClicked"/> and <see cref="AdvancedLabel.HyperlinkClicked"/> events.
    /// </summary>
    public sealed class HyperlinkClickedEventArgs: HandledEventArgs
    {
        /// <summary>
        /// Gets the hyperlink that was clicked. If resolving was not handled,
        /// the <see cref="HandledEventArgs.Handled"/> property can be set to <see langword="false"/>
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
