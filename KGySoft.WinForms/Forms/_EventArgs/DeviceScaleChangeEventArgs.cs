#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DeviceScaleChangeEventArgs.cs
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

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Represents the event data for the <see cref="BaseForm.DeviceScaleChanging"/> and <see cref="BaseForm.DeviceScaleChanged"/> events.
    /// </summary>
    public sealed class DeviceScaleChangeEventArgs : EventArgs
    {
        #region Fields

        private PointF newScale;
        private PointF previousScale;
        private Rectangle suggestedBounds;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the new scale factor.
        /// </summary>
        public PointF NewScale => newScale;

        /// <summary>
        /// Gets the previous scale factor.
        /// </summary>
        public PointF PreviousScale => previousScale;

        /// <summary>
        /// Gets the suggested bounds of the form after scaling.
        /// It may return an empty rectangle if the event is raised before the form is shown, or when the form is an MDI child.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="SuggestedBounds"/> are not applied, and no custom size is set either, Windows may apply the suggested bounds automatically.</para>
        /// <note type="caution">If you want to apply custom bounds, make sure that you don't change the screen; otherwise you may end up in an infinite loop
        /// of DPI change due to different scaling of displays. To apply custom bounds safely, make sure you query the <see cref="Screen"/>
        /// by the <see cref="Screen.FromRectangle">FromRectangle</see> method using the original <see cref="SuggestedBounds"/>, and then
        /// call the <see cref="RectangleExtensions.EnsureScreen">EnsureScreen</see> extension method using the custom bounds and the screen of the original bounds.</note>
        /// <para><see cref="SuggestedBounds"/> is empty if the form is an MDI child form. It can be empty also for top-level forms, when a DPI change is detected
        /// during the form creation, out of a DPI change Windows event. This typically happens when the form is shown on a display that has a different scale factor than the primary display.
        /// In such cases there is no <see cref="BaseForm.DeviceScaleGetNewSize"/> event before the <see cref="BaseForm.DeviceScaleChanging"/> event,
        /// and there is no <see cref="BaseForm.DeviceScaleAutoResized"/> event after the <see cref="BaseForm.DeviceScaleChanged"/> event.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>If your application uses per-monitor DPI awareness V2, you can use the <see cref="BaseForm.DeviceScaleGetNewSize"/> event to set
        /// a custom desired size in advance, so the <see cref="SuggestedBounds"/> will be calculated based on that desired size.</item>
        /// <item>If your application uses per-monitor DPI awareness V1, Windows may forcibly reapply the suggested bounds even if you set a custom size
        /// in the <see cref="BaseForm.DeviceScaleChanged"/> event. In that case you can use the <see cref="BaseForm.DeviceScaleAutoResized"/> event to set the custom size safely.</item>
        /// </list></note>
        /// </remarks>
        public Rectangle SuggestedBounds => suggestedBounds;

        #endregion

        #region Constructors

        internal DeviceScaleChangeEventArgs(Rectangle suggestedBounds, PointF newScale, PointF previousScale)
        {
            this.suggestedBounds = suggestedBounds;
            this.newScale = newScale;
            this.previousScale = previousScale;
        }

        #endregion

        #region Methods

        internal DeviceScaleChangeEventArgs Reset(Rectangle bounds, PointF deviceScale, PointF oldScale)
        {
            suggestedBounds = bounds;
            newScale = deviceScale;
            previousScale = oldScale;
            return this;
        }

        #endregion
    }
}
