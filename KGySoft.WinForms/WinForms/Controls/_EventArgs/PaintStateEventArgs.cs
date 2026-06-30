#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PaintStateEventArgs.cs
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

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents arguments of for the <see cref="ISupportsFading{TState}.PaintState"/> event.
    /// </summary>
    public class PaintStateEventArgs : PaintEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the appearance state of the control for the painting.
        /// </summary>
        public ControlAppearanceState State { get; }

        #endregion

        #region Constructors

        internal PaintStateEventArgs(Graphics g, Rectangle clipRect, ControlAppearanceState state)
            : base(g, clipRect)
        {
            State = state;
        }

        #endregion
    }
}
