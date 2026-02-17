#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportsFadingInternal.cs
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

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    internal interface ISupportsFadingInternal : ISupportsFading<ControlAppearanceState>
    {
        #region Properties

        FadingOptions FadingAnimationOptions { get; }

        #endregion

        #region Methods
        
        int GetStandardAnimationSpeed(ControlAppearanceState stateFrom, ControlAppearanceState stateTo, int defaultSpeed);

        #endregion
    }
}