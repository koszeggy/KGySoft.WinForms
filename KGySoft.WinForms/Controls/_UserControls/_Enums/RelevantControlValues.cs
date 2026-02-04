#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RelevantControlValues.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents the possible values of the <see cref="ucCustomSelector.RelevantControlValue">ucCustomSelector.RelevantControlValue</see> property.
    /// </summary>
    [Obsolete("This type is used by the obsoleted ucCustomSelector and is not recommended to use it anymore.")]
    public enum RelevantControlValues
    {
        /// <summary>
        /// Indicates that the <see cref="ucCustomSelector.Value">ucCustomSelector.Value</see> property reflects the value of the inner control.
        /// </summary>
        Value,
        
        /// <summary>
        /// Indicates that the <see cref="ucCustomSelector.Text">ucCustomSelector.Text</see> property reflects the text of the inner control.
        /// </summary>
        Text,

        /// <summary>
        /// Indicates that the <see cref="ucCustomSelector.State">ucCustomSelector.State</see> property reflects the state of the inner control.
        /// </summary>
        State
    }
}