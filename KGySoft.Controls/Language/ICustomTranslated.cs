using System;
using System.Collections.Generic;
using System.Text;

namespace KGySoft.Libraries.Language
{
    /// <summary>
    /// Makes a control custom translatable. See <see cref="Language"/>.
    /// </summary>
    [Obsolete("TODO: Remove or refactor")]
    public interface ICustomTranslated
    {
        /// <summary>
        /// Tanslates the control.
        /// </summary>
        /// <param name="translationFinished">If an implementer returns true, no further translation will be performed on child elements.</param>
        /// <returns>Should return false is translation is disabled for the control for some internal reason, otherwise, true. </returns>
        bool TranslateControl(out bool translationFinished);
    }
}