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
        /// It may return an empty rectangle if the event is raised before the form is shown.
        /// This typically happens when the form is shown on a display with a different scale factor from the primary display.
        /// </summary>
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
