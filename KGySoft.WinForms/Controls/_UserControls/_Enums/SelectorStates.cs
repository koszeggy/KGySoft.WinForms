#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SelectorStates.cs
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

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// <see cref="ucCustomSelector.State"/>
    /// </summary>
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public enum SelectorStates
    {
        NotSelected = ControlExtensions.NotSelectedValue,
        All = ControlExtensions.AllSelectedValue,
        None = ControlExtensions.NoneSelectedValue,
        ValueSet = ControlExtensions.UndefinedValue
    }
}