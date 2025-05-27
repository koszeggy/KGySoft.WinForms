#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextFormatFlagsExtensions.cs
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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

using KGySoft.Collections;
#if NETFRAMEWORK
using KGySoft.CoreLibraries;
#endif

#endregion

namespace KGySoft.WinForms
{
    internal static class TextFormatFlagsExtensions
    {
        #region Fields

        private static readonly Cache<TextFormatFlags, StringFormat> formatsCache =
            new Cache<TextFormatFlags, StringFormat>(TextFormatFlagsToStringFormat, 8, Comparer)
            {
                Behavior = CacheBehavior.RemoveOldestElement,
                DisposeDroppedValues = true
            };

        #endregion

        #region Properties

#if NETFRAMEWORK
        private static IEqualityComparer<TextFormatFlags> Comparer => EnumComparer<TextFormatFlags>.Comparer;
#else
        private static IEqualityComparer<TextFormatFlags>? Comparer => null;
#endif

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Use from the UI thread only, and do not mutate or dispose the result.
        /// </summary>
        internal static StringFormat ToStringFormat(this TextFormatFlags tff) => formatsCache[tff];

        #endregion

        #region Private Methods

        private static StringFormat TextFormatFlagsToStringFormat(TextFormatFlags tff)
        {
            StringFormat result = new StringFormat();
            StringFormatFlags sff = StringFormatFlags.MeasureTrailingSpaces;

            bool isRtl = (tff & TextFormatFlags.RightToLeft) != 0;

            if ((tff & (TextFormatFlags.Bottom)) != 0)
                result.LineAlignment = StringAlignment.Far;
            else if ((tff & (TextFormatFlags.VerticalCenter)) != 0)
                result.LineAlignment = StringAlignment.Center;
            else
                result.LineAlignment = StringAlignment.Near;

            if ((tff & (TextFormatFlags.Right)) != 0)
                result.Alignment = isRtl ? StringAlignment.Near : StringAlignment.Far;
            else if ((tff & (TextFormatFlags.HorizontalCenter)) != 0)
                result.Alignment = StringAlignment.Center;
            else
                result.Alignment = isRtl ? StringAlignment.Far : StringAlignment.Near;

            if (isRtl)
                sff |= StringFormatFlags.DirectionRightToLeft;

            if ((tff & (TextFormatFlags.SingleLine)) != 0)
                sff |= StringFormatFlags.NoWrap;

            if ((tff & (TextFormatFlags.EndEllipsis)) != 0)
                result.Trimming = StringTrimming.EllipsisCharacter;

            if ((tff & (TextFormatFlags.NoPrefix)) != 0)
                result.HotkeyPrefix = HotkeyPrefix.None;
            else if ((tff & (TextFormatFlags.HidePrefix)) != 0)
                result.HotkeyPrefix = HotkeyPrefix.Hide;
            else
                result.HotkeyPrefix = HotkeyPrefix.Show;

            result.FormatFlags = sff;
            return result;
        }

        #endregion

        #endregion
    }
}
