#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FadingOptions.cs
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
using System.ComponentModel;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents possible fading animation options.
    /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm">online help</a> of the <see cref="AdvancedButton"/> class for an animated image example.</div>
    /// </summary>
    /// <example>
    /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="AdvancedButton"/> class for an animated image example.</note>
    /// </example>
    [Flags]
    public enum FadingOptions
    {
        /// <summary>
        /// Represents no enabled fading animation.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the standard fading animations (toggling the enabled, hovered, pushed and checked states where applicable) are enabled for the control.
        /// The speed of these effects are retrieved from the operating system.
        /// </summary>
        [Description("the standard fading animations (toggling the enabled, hovered, pushed and checked states where applicable) are enabled for the control. "
            + "The speed of these effects are retrieved from the operating system.")]
        StandardEffects = 1,

        /// <summary>
        /// Indicates that text change should be performed by a fading effect.
        /// </summary>
        [Description("Indicates that text change should be performed by a fading effect.")]
        TextChange = 1 << 3,

        /// <summary>
        /// Indicates that a fading effect should be performed when Visibility of the control is turned on.
        /// <note>When the control turns invisible, it is performed without fading to avoid security problems.</note>
        /// </summary>
        [Description("Indicates that a fading effect should be performed when the inivisible control appears. When the cotrols turns invisible, it is performed without fading to avoid security problems.")]
        Appearing = 1 << 4,

        /// <summary>
        /// Indicates that color changes should be performed by a fading effect.
        /// Does not affect colors of the flat appearance. If fading is required also for such colors, use the <see cref="AnyChange"/> flag instead.
        /// </summary>
        [Description("Indicates that color changes should be performed by a fading effect. Does not affect colors of the flat appearance. If fading is required also for such colors, use the AnyChange flag instead.")]
        ColorChange = 1 << 5,

        /// <summary>
        /// Indicates that any kind of visual change should be performed by a fading effect, except size changes.
        /// </summary>
        [Description("Indicates that any kind of visual change should be performed by a fading effect, except size changes.")]
        AnyChange = 1 << 31,
    }
}
