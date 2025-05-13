#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DecimalValueOnBlank.cs
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
    /// Controls <see cref="DecimalTextBox.Value"/> in <see cref="DecimalTextBox.Blank"/> state.
    /// </summary>
    public enum DecimalValueOnBlank
    {
        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return zero in <see cref="DecimalTextBox.Blank"/> state
        /// </summary>
        Zero,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return the internally stored value in <see cref="DecimalTextBox.Blank"/> state
        /// </summary>
        Value,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return lower limit minus one or <see cref="decimal.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        LowerLimitMinusOne,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return upper limit plus one or <see cref="decimal.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        UpperLimitPlusOne,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="int.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MinInt,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="int.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MaxInt,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="decimal.MinValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MinDecimal,

        /// <summary>
        /// Indicates that <see cref="DecimalTextBox.Value"/> should return <see cref="decimal.MaxValue"/> in <see cref="DecimalTextBox.Blank"/> state.
        /// </summary>
        MaxDecimal
    }
}
