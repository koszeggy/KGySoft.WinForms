#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DeviceScaleGetNewSizeEventArgs.cs
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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

#region Suppressions

#if NETFRAMEWORK && !NET47_OR_GREATER
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - The documentation references members that are not available on all platforms
#endif

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Represents the event data for the <see cref="BaseForm.DeviceScaleGetNewSize"/> event.
    /// To apply a custom size, change <see cref="DesiredSize"/>, and set the <see cref="HandledEventArgs.Handled"/> property to <see langword="true"/>.
    /// </summary>
    public sealed class DeviceScaleGetNewSizeEventArgs : HandledEventArgs
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
        /// Gets or sets the desired size of the form after scaling.
        /// To apply a custom size, set the <see cref="HandledEventArgs.Handled"/> property to <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>On platforms where the <see cref="Form.OnGetDpiScaledSize">Form.OnGetDpiScaledSize</see> method is available, an overridden implementation
        /// may already have set a desired size. In such case the <see cref="HandledEventArgs.Handled"/> property is already set to <see langword="true"/>.
        /// To revoke such custom resizing and to apply the default scaling behavior instead, set the <see cref="HandledEventArgs.Handled"/> property to <see langword="false"/>.</para>
        /// </remarks>
        public Size DesiredSize { get; set; }
        
        #endregion

        #region Constructors

        internal DeviceScaleGetNewSizeEventArgs(Size desiredSize, PointF newScale, PointF previousScale, bool handled)
            : base(handled)
        {
            DesiredSize = desiredSize;
            NewScale = newScale;
            PreviousScale = previousScale;
        }

        #endregion
    }
}
