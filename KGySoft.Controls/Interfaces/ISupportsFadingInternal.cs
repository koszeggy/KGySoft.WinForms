using System;

namespace KGySoft.Controls
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
