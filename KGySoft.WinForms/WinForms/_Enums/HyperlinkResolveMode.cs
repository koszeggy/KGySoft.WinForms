#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: HyperlinkResolveMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents hyperlink resolve modes.
    /// </summary>
    public enum HyperlinkResolveMode
    {
        /// <summary>
        /// Hyperlinks are not resolved.
        /// </summary>
        None,

        /// <summary>
        /// Only explicit hyperlinks are resolved like <c><![CDATA[<a href="https://github.com/koszeggy">link</a>]]></c>
        /// </summary>
        ResolveHrefsOnly,

        /// <summary>
        /// Every URLs are resolved in text.
        /// </summary>
        ResolveAll
    }
}