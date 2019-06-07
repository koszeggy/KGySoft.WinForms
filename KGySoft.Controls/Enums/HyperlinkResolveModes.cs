using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents hyperlink resolve modes
    /// </summary>
    public enum HyperlinkResolveModes
    {
        /// <summary>
        /// Hyperlinks are not resolved.
        /// </summary>
        None,

        /// <summary>
        /// Only explicit hyperlinks are resolved, such as <c><example>&gt;a href="http://kgysoft.try.hu"&lt;link&gt;/a&lt;</example></c>
        /// </summary>
        ResolveHrefsOnly,

        /// <summary>
        /// Every URLs are resolved in text.
        /// </summary>
        ResolveAll
    }
}
