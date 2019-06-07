using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents possible fading options
    /// </summary>
    [Flags]
    public enum FadingOptions
    {
        /// <summary>
        /// Represents disabled fading options
        /// </summary>
        None,

        /// <summary>
        /// Indicates that standard fading animations (enabling, hovering, clicking, etc.) are enabled for the control.
        /// Speed of these effects are retrieved from the system.
        /// </summary>
        [Description("Indicates that standard fading animations (enabling, hovering, clicking, etc.) are enabled for the control. Speed of these effects are retrieved from the system.")]
        StandardEffects = 1,

        ///// <summary>
        ///// Indicates that enabling/disabling the control should be performed by a fading effect.
        ///// </summary>
        //[Description("Indicates that enabling/disabling the control should be performed by a fading effect.")]
        //Enabling = 1,

        ///// <summary>
        ///// Indicates that hovering the control should be performed by a fading effect.
        ///// </summary>
        //[Description("Indicates that hovering the control should be performed by a fading effect.")]
        //Hovering = 1 << 1,

        ///// <summary>
        ///// Indicates pressing the left mouse button should be performed by a fading effect.
        ///// </summary>
        //[Description("Indicates pressing the left mouse button should be performed by a fading effect.")]
        //MouseDown = 1 << 2,

        /// <summary>
        /// Indicates that text change should be performed by a fading effect.
        /// </summary>
        [Description("Indicates that text change should be performed by a fading effect.")]
        TextChange = 1 << 3,

        /// <summary>
        /// Indicates that a fading effect should be performed when Visibility of the control is turned on.
        /// <note>When the cotrols turns invisible, it is performed without fading to avoid security problems.</note>
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

        ///// <summary>
        ///// Represents standard effects (<see cref="Enabling"/>, <see cref="Hovering"/>, <see cref="MouseDown"/>)
        ///// </summary>
        //StandardEffects = Enabling | Hovering | MouseDown
    }
}
