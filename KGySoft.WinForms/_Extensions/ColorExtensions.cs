using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace KGySoft.WinForms
{
    internal static class ColorExtensions
    {
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

        private static Color FromHLS(int hue, int luminosity, int saturation)
        {
            byte r;
            byte g;
            byte b;
            if (saturation == 0)
            {
                r = g = b = (byte)((luminosity * 255) / 240);
                if (hue == 160)
                {
                }
            }
            else
            {
                int num5;
                if (luminosity <= 120)
                {
                    num5 = ((luminosity * (240 + saturation)) + 120) / 240;
                }
                else
                {
                    num5 = (luminosity + saturation) - (((luminosity * saturation) + 120) / 240);
                }
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
            {
                hue += 240;
            }
            if (hue > 240)
            {
                hue -= 240;
            }
            if (hue < 40)
            {
                return (n1 + ((((n2 - n1) * hue) + 20) / 40));
            }
            if (hue < 120)
            {
                return n2;
            }
            if (hue < 160)
            {
                return (n1 + ((((n2 - n1) * (160 - hue)) + 20) / 40));
            }
            return n1;
        }
    }
}
