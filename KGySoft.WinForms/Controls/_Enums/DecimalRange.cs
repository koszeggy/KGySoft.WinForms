#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DecimalRange.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents possible ranges of <see cref="DecimalTextBox"/> control.
    /// </summary>
    public enum DecimalRange
    {
        /// <summary>
        /// Any value is accepted.
        /// </summary>
        Any,

        /// <summary>
        /// Positive values are accepted, excluding zero value.
        /// </summary>
        Positive,

        /// <summary>
        /// Negative values are accepted, excluding zero value.
        /// </summary>
        Negative,

        /// <summary>
        /// Positive values are accepted, including zero value.
        /// </summary>
        PositiveNull,

        /// <summary>
        /// Negative values are accepted, including zero value.
        /// </summary>
        NegativeNull,

        /// <summary>
        /// Accepted values are controlled by <see cref="DecimalTextBox.RangeMinValue"/> and <see cref="DecimalTextBox.RangeMaxValue"/> properties.
        /// </summary>
        MinMax
    }
}