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
        #region Properties

        /// <summary>
        /// Gets the new scale factor.
        /// </summary>
        public PointF NewScale { get; }

        /// <summary>
        /// Gets the previous scale factor.
        /// </summary>
        public PointF PreviousScale { get; }

        /// <summary>
        /// Gets the suggested bounds of the form after scaling.
        /// </summary>
        public Rectangle SuggestedBounds { get; }

        #endregion

        #region Constructors
        
        internal DeviceScaleChangeEventArgs(Rectangle suggestedBounds, PointF newScale, PointF previousScale)
        {
            SuggestedBounds = suggestedBounds;
            NewScale = newScale;
            PreviousScale = previousScale;
        }
        
        #endregion
    }
}
