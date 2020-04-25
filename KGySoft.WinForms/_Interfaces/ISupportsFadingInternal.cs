using System;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms
{
    internal interface ISupportsFadingInternal : ISupportsFading<ControlAppearanceState>
    {
        #region Properties

        /// <summary>
        /// Gets or sets fading options of the control.
        /// </summary>
        FadingOptions FadingAnimationOptions { get; set; }

        #endregion
    }
}
