#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ContentAlignmentExtensions.cs
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

using KGySoft.WinForms.Reflection;

#endregion


namespace KGySoft.WinForms
{
    /// <summary>
    /// Extension methods for <see cref="ContentAlignment"/> enumeration.
    /// </summary>
    public static class ContentAlignmentExtensions
    {
        #region Private Constants

        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        private const ContentAlignment anyLeft = ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft | ContentAlignment.TopLeft;
        private const ContentAlignment anyTop = ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft;
        private const ContentAlignment anyBottom = ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft;
        private const ContentAlignment anyMiddle = ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft;
        private const ContentAlignment anyRight = ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight;
        private const ContentAlignment anyCenter = ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter;
        // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

        #endregion

        #region Methods

        /// <summary>
        /// Calls the protected <see cref="Control.RtlTranslateContent"/> method as it was a public method.
        /// </summary>
        /// <param name="alignment">The alignment to translate if needed.</param>
        /// <param name="instance">The <see cref="Control"/> instance.</param>
        /// <returns></returns>
        public static ContentAlignment RtlTranslateContent(this ContentAlignment alignment, Control instance)
            => instance.RtlTranslateContent(alignment);

        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        /// <summary>
        /// Gets if any left alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyLeft(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyLeft) != 0;
        }

        /// <summary>
        /// Gets if any top alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyTop(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyTop) != 0;
        }

        /// <summary>
        /// Gets if any bottom alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyBottom(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyBottom) != 0;
        }

        /// <summary>
        /// Gets if any middle alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyMiddle(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyMiddle) != 0;
        }

        /// <summary>
        /// Gets if any right alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyRight(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyRight) != 0;
        }

        /// <summary>
        /// Gets if any center alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        public static bool AnyCenter(this ContentAlignment contentAlignment)
        {
            return (contentAlignment & anyCenter) != 0;
        }
        // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

        #endregion
    }
}
