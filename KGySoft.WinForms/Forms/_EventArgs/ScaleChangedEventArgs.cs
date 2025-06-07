#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScaleChangedEventArgs.cs
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
    /// Represents the event data for the <see cref="BaseForm.ScaleChanged"/> event.
    /// </summary>
    public sealed class ScaleChangedEventArgs : EventArgs
    {
        #region Properties
        /// <summary>
        /// Gets the new scale factor.
        /// </summary>
        public PointF NewScale { get; }

        /// <summary>
        /// Gets the suggested bounds of the form after scaling.
        /// </summary>
        public Rectangle SuggestedBounds { get; }

        #endregion

        #region Constructors
        
        internal ScaleChangedEventArgs(Rectangle suggestedBounds, PointF newScale)
        {
            SuggestedBounds = suggestedBounds;
            NewScale = newScale;
        }
        
        #endregion
    }
}
