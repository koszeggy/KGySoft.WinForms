#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportsFadingInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    internal interface ISupportsFadingInternal : ISupportsFading<ControlAppearanceState>
    {
        #region Properties

        /// <summary>
        /// Gets or sets fading options of the control.
        /// </summary>
        FadingOptions FadingAnimationOptions { get; }

        #endregion
    }
}