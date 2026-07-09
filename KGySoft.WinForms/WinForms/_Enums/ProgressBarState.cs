#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ProgressBarState.cs
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
    /// <summary>
    /// Represents possible progress bar states.
    /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm">online help</a> of the <see cref="AdvancedProgressBar"/> class for an animated image example.</div>
    /// </summary>
    /// <example>
    /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="AdvancedProgressBar"/> class for an animated image example.</note>
    /// </example>
    public enum ProgressBarState
    {
        /// <summary>
        /// Indicates the normal progress bar state.
        /// </summary>
        Normal,

        /// <summary>
        /// Indicates the error progress bar state.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates the paused progress bar state.
        /// </summary>
        Paused
    }
}