#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SelectorStates.cs
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

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents the possible values for the <see cref="ucCustomSelector.State"/> property.
    /// </summary>
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public enum SelectorStates
    {
        /// <summary>
        /// Represents the 'Not Selected' state.
        /// </summary>
        NotSelected = ControlExtensions.NotSelectedValue,

        /// <summary>
        /// Represents the 'All Selected' state.
        /// </summary>
        All = ControlExtensions.AllSelectedValue,
        
        /// <summary>
        /// Represents the 'None Selected' state.
        /// </summary>
        None = ControlExtensions.NoneSelectedValue,

        /// <summary>
        /// Represents the 'Value Set' state.
        /// </summary>
        ValueSet = ControlExtensions.UndefinedValue
    }
}