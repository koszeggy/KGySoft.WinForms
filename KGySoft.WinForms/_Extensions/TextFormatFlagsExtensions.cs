using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace KGySoft.WinForms
{
    internal static class TextFormatFlagsExtensions
    {
        internal static StringFormat ToStringFormat(this TextFormatFlags tff)
        {
            StringFormat result = new StringFormat();
            StringFormatFlags sff = StringFormatFlags.MeasureTrailingSpaces;

            bool isRtl = (tff & TextFormatFlags.RightToLeft) != 0;

            if ((tff & (TextFormatFlags.Bottom)) != 0)
                result.LineAlignment = StringAlignment.Far;
            else if ((tff & (TextFormatFlags.VerticalCenter)) != 0)
                result.LineAlignment = StringAlignment.Center;
            else if ((tff & (TextFormatFlags.Top)) != 0)
                result.LineAlignment = StringAlignment.Near;

            if ((tff & (TextFormatFlags.Right)) != 0)
                result.Alignment = isRtl ? StringAlignment.Near : StringAlignment.Far;
            else if ((tff & (TextFormatFlags.HorizontalCenter)) != 0)
                result.Alignment = StringAlignment.Center;
            else if ((tff & (TextFormatFlags.Left)) != 0)
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
    }
}
