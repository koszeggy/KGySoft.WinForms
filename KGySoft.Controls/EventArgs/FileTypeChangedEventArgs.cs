using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contains arguments of <see cref="AdvancedSaveFileDialog.FileTypeChanged"/> event.
    /// </summary>
    public sealed class FileTypeChangedEventArgs: EventArgs
    {
        private readonly int selectedIndex;
        private readonly string extension;

        /// <summary>
        /// Gets selected index of the file type combo box in <see cref="AdvancedSaveFileDialog"/>.
        /// Unlike as <see cref="AdvancedSaveFileDialog.FilterIndex"/>, this is a zero-based index.
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
        }

        /// <summary>
        /// Gets selected file type filter in <see cref="AdvancedSaveFileDialog"/> or null when
        /// <see cref="AdvancedSaveFileDialog.Filter"/> was not correctly defined.
        /// </summary>
        public string Filter
        {
            get { return extension; }
        }

        internal FileTypeChangedEventArgs(int index, string filter)
        {
            selectedIndex = index;
            extension = filter;
        }
    }
}
