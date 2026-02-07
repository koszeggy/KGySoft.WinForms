#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: HyperlinkClickedEventArgs.cs
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

using System.ComponentModel;

using KGySoft.WinForms.Components;
using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Contains arguments of <see cref="TaskDialog.HyperlinkClicked"/> and <see cref="AdvancedLabel.HyperlinkClicked"/> events.
    /// </summary>
    public sealed class HyperlinkClickedEventArgs : HandledEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the hyperlink that was clicked. If resolving was not handled,
        /// the <see cref="HandledEventArgs.Handled"/> property can be set to <see langword="false"/>
        /// to make the system resolve the hyperlink.
        /// </summary>
        public string Hyperlink { get; private set; }

        #endregion

        #region Constructors

        internal HyperlinkClickedEventArgs(string link)
            : base(true)
        {
            Hyperlink = link;
        }

        #endregion
    }
}