#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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

using System.Drawing;
using KGySoft.Collections;

#endregion

namespace KGySoft.WinForms
{
    internal static class ColorExtensions
    {
        #region Fields

        // Need to use locking caches to be able to use DisposeDroppedValues, but it shouldn't be an issue as we don't expect many concurrent UI threads.
        private static readonly IThreadSafeCacheAccessor<int, Pen> penCache = new Cache<int, Pen>(c => new Pen(Color.FromArgb(c)), 16)
        {
            DisposeDroppedValues = true,
            EnsureCapacity = true,
        }.GetThreadSafeAccessor();

        private static readonly IThreadSafeCacheAccessor<int, Brush> brushCache = new Cache<int, Brush>(c => new SolidBrush(Color.FromArgb(c)), 16)
        {
            DisposeDroppedValues = true,
            EnsureCapacity = true,
        }.GetThreadSafeAccessor();

        #endregion

        #region Methods

        #region Internal Methods

        internal static Color Dark(this Color color, float percent)
        {
            float l;
            if (percent >= 1f || (l = (color.GetBrightness() * (1 - percent))) <= 0f)
                return Color.Black;

            return FromHLS((int)(color.GetHue() * 240f / 360f), (int)(l * 240f), (int)(color.GetSaturation() * 240f));
        }

        internal static Color Light(this Color color, float percent)
        {
            float l;
            if ((l = (color.GetBrightness() * (1 + percent))) >= 1f)
                return Color.White;

            return FromHLS((int)(color.GetHue() * 240f / 360f), (int)(l * 240f), (int)(color.GetSaturation() * 240f));
        }

        internal static Pen GetPen(this Color color) => color.IsSystemColor
            ? SystemPens.FromSystemColor(color)
            : penCache[color.ToArgb()];

        internal static Brush GetBrush(this Color color) => color.IsSystemColor
            ? SystemBrushes.FromSystemColor(color)
            : brushCache[color.ToArgb()];

        #endregion

        #region Private Methods

        private static Color FromHLS(int hue, int luminosity, int saturation)
        {
            byte r;
            byte g;
            byte b;
            if (saturation == 0)
                r = g = b = (byte)((luminosity * 255) / 240);
            else
            {
                int num5;
                if (luminosity <= 120)
                    num5 = ((luminosity * (240 + saturation)) + 120) / 240;
                else
                    num5 = (luminosity + saturation) - (((luminosity * saturation) + 120) / 240);
                int num4 = (2 * luminosity) - num5;
                r = (byte)(((HueToRGB(num4, num5, hue + 80) * 255) + 120) / 240);
                g = (byte)(((HueToRGB(num4, num5, hue) * 255) + 120) / 240);
                b = (byte)(((HueToRGB(num4, num5, hue - 80) * 255) + 120) / 240);
            }
            return Color.FromArgb(r, g, b);
        }

        private static int HueToRGB(int n1, int n2, int hue)
        {
            if (hue < 0)
                hue += 240;
            if (hue > 240)
                hue -= 240;
            if (hue < 40)
                return (n1 + ((((n2 - n1) * hue) + 20) / 40));
            if (hue < 120)
                return n2;
            if (hue < 160)
                return (n1 + ((((n2 - n1) * (160 - hue)) + 20) / 40));
            return n1;
        }

        #endregion

        #endregion
    }
}
