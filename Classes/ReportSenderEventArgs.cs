using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    ///<summary>
    /// Arguments for <see cref="AdvancedMessageDialog.ReportSender"/> event.
    ///</summary>
    public sealed class ReportSenderEventArgs: EventArgs
    {
        /// <summary>
        /// Summary message from the sender <see cref="AdvancedMessageDialog"/>.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Details content of the sender <see cref="AdvancedMessageDialog"/>.
        /// </summary>
        public string Details { get; private set; }

        /// <summary>
        /// Path of screenshot or null when screenshot has not been saved.
        /// </summary>
        public string ScreenshotPath { get; private set; }

        /// <summary>
        /// Gets or sets whether message dialog can be closed after returning from
        /// handler of <see cref="AdvancedMessageDialog.ReportSender"/>.
        /// </summary>
        public bool CloseMessageDialog { get; set; }

        internal ReportSenderEventArgs(string message, string details, string path)
        {
            Message = message;
            Details = details;
            ScreenshotPath = path;
            CloseMessageDialog = true;
        }
    }
}
