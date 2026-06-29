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
    /// Extension methods for the <see cref="ContentAlignment"/> enumeration.
    /// </summary>
    public static class ContentAlignmentExtensions
    {
        #region Constants

        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        private const ContentAlignment anyLeft = ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft | ContentAlignment.TopLeft;
        private const ContentAlignment anyTop = ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft;
        private const ContentAlignment anyBottom = ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft;
        private const ContentAlignment anyMiddle = ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft;
        private const ContentAlignment anyRight = ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight;
        private const ContentAlignment anyCenter = ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter;
        // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

        #endregion

        #region Properties

        internal static ContentAlignment[][] RtlMapping => field ??= new ContentAlignment[3][]
        {
            [ContentAlignment.TopLeft, ContentAlignment.TopRight],
            [ContentAlignment.MiddleLeft, ContentAlignment.MiddleRight],
            [ContentAlignment.BottomLeft, ContentAlignment.BottomRight]
        };

        #endregion

        #region Methods

        /// <summary>
        /// Calls the protected <see cref="Control.RtlTranslateContent">Control.RtlTranslateContent</see> method as if it was a public method.
        /// It also applies a workaround for a Mono bug when the internal field value is <see cref="RightToLeft.Inherit"/>.
        /// </summary>
        /// <param name="alignment">The alignment to translate if needed.</param>
        /// <param name="instance">The <see cref="Control"/> instance.</param>
        /// <returns>The translated alignment.</returns>
        public static ContentAlignment RtlTranslateContent(this ContentAlignment alignment, Control instance)
        {
            if (OSHelper.IsMono && instance.RightToLeft != RightToLeft.Yes)
                return alignment;
            return instance.RtlTranslateContent(alignment);
        }

        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        /// <summary>
        /// Gets if any left alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any left alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyLeft(this ContentAlignment contentAlignment) => (contentAlignment & anyLeft) != 0;

        /// <summary>
        /// Gets if any top alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any top alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyTop(this ContentAlignment contentAlignment) => (contentAlignment & anyTop) != 0;

        /// <summary>
        /// Gets if any bottom alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any bottom alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyBottom(this ContentAlignment contentAlignment) => (contentAlignment & anyBottom) != 0;

        /// <summary>
        /// Gets if any middle alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any middle alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyMiddle(this ContentAlignment contentAlignment) => (contentAlignment & anyMiddle) != 0;

        /// <summary>
        /// Gets if any right alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any right alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyRight(this ContentAlignment contentAlignment) => (contentAlignment & anyRight) != 0;

        /// <summary>
        /// Gets if any center alignment is set in <paramref name="contentAlignment"/>.
        /// </summary>
        /// <param name="contentAlignment">The content alignment to check.</param>
        /// <returns><see langword="true"/> if any center alignment is set; otherwise, <see langword="false"/>.</returns>
        public static bool AnyCenter(this ContentAlignment contentAlignment) => (contentAlignment & anyCenter) != 0;
        // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

        #endregion
    }
}
