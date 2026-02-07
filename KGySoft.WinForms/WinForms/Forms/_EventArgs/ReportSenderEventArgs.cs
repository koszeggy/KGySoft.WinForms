#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReportSenderEventArgs.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.WinForms.Forms
{
    ///<summary>
    /// Arguments for <see cref="AdvancedMessageDialog.ReportSender"/> event.
    ///</summary>
    [Obsolete("This type is used by the obsoleted AdvancedMessageDialog")]
    public sealed class ReportSenderEventArgs : EventArgs
    {
        #region Properties

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

        #endregion

        #region Constructors

        internal ReportSenderEventArgs(string message, string details, string path)
        {
            Message = message;
            Details = details;
            ScreenshotPath = path;
            CloseMessageDialog = true;
        }

        #endregion
    }
}
