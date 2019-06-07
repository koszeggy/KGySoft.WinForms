using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contains arguments of <see cref="AdvancedSaveFileDialog.SelectedFileChanged"/> event.
    /// </summary>
    public class SelectedFileChangedEventArgs: EventArgs
    {
        private readonly string fileName;

        /// <summary>
        /// Gets selected file name
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        internal SelectedFileChangedEventArgs(string fileName)
        {
            this.fileName = fileName;
        }
    }
}
