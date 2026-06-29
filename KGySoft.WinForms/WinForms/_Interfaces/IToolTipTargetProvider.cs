#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IToolTipTargetProvider.cs
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

using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a target control provider for tooltips.
    /// It is used by <see cref="LocalizationHelper"/> for <c>ToolTipText</c> properties.
    /// </summary>
    public interface IToolTipTargetProvider
    {
        #region Methods

        /// <summary>
        /// Gets the control that the tooltip should be associated with.
        /// </summary>
        /// <returns>The control that the tooltip should be associated with.</returns>
        Control GetToolTipTarget();

        #endregion
    }
}