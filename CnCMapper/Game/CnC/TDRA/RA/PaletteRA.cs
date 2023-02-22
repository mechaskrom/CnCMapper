using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class PaletteRA : PaletteTDRA
    {
        protected PaletteRA()
            : base()
        {
        }

        public PaletteRA(Palette6Bit copyFrom)
            : base(copyFrom)
        {
        }

        private static HsvEntry toHsv(RgbEntry rgbEntry)
        {
            return toHsv(rgbEntry.r << 2, rgbEntry.g << 2, rgbEntry.b << 2);
        }

        private static HsvEntry toHsv(int red, int green, int blue)
        {
            //Translation of "operator HSVClass()" in "RGB.CPP" from source.
            int value = Math.Max(red, Math.Max(green, blue));
            int white = Math.Min(red, Math.Min(green, blue));

            int saturation = value > 0 ? ((value - white) * 255) / value : 0;

            int hue = 0;
            if (saturation > 0)
            {
                int tmp = value - white;
                int r1 = ((value - red) * 255) / tmp;
                int g1 = ((value - green) * 255) / tmp;
                int b1 = ((value - blue) * 255) / tmp;
                if (value == red)
                {
                    if (white == green)
                    {
                        tmp = 5 * 256 + b1;
                    }
                    else
                    {
                        tmp = 1 * 256 - g1;
                    }
                }
                else
                {
                    if (value == green)
                    {
                        if (white == blue)
                        {
                            tmp = 1 * 256 + r1;
                        }
                        else
                        {
                            tmp = 3 * 256 - b1;
                        }
                    }
                    else
                    {
                        if (white == red)
                        {
                            tmp = 3 * 256 + g1;
                        }
                        else
                        {
                            tmp = 5 * 256 - r1;
                        }
                    }
                }
                hue = tmp / 6;
            }

            return new HsvEntry(hue, saturation, value);
        }

        private static RgbEntry toRgb(HsvEntry hsvEntry)
        {
            return toRgb(hsvEntry.h, hsvEntry.s, hsvEntry.v);
        }

        private static RgbEntry toRgb(int hue, int saturation, int value)
        {
            //Translation of "operator RGBClass()" in "HSV.CPP" from source.
            int[] values = new int[7];

            hue *= 6;
            int fraction = hue % 255;

            values[1] =
            values[2] = value;

            int tmp = (saturation * fraction) / 255;
            values[3] = (value * (255 - tmp)) / 255;

            values[4] =
            values[5] = (value * (255 - saturation)) / 255;

            tmp = 255 - (saturation * (255 - fraction)) / 255;
            values[6] = (value * tmp) / 255;

            int whole = hue / 255;

            whole += (whole > 4) ? -4 : 2;
            byte red = (byte)values[whole];

            whole += (whole > 4) ? -4 : 2;
            byte blue = (byte)values[whole];

            whole += (whole > 4) ? -4 : 2;
            byte green = (byte)values[whole];

            return RgbEntry.createFrom8Bit(red, green, blue);
        }

        public override PaletteCnC getCopy()
        {
            return new PaletteRA(this);
        }

        public override PaletteTDRA getAdjusted()
        {
            //Settings are fixed math values and defaults to FixedMath.Frac1_2.
            return getAdjusted(FixedMath.Frac1_2, FixedMath.Frac1_2, FixedMath.Frac1_2, FixedMath.Frac1_2);
        }

        public PaletteRA getAdjusted(FixedMath brightness, FixedMath color, FixedMath contrast, FixedMath tint)
        {
            //Settings are fixed math values and defaults to FixedMath.Fraction1_2.

            //The visual settings (visual controls) in the game where brightness, color, contrast and tint
            //can be adjusted is calculated in HSV space. The RGB entries in the palette is converted to
            //HSV and then back to RGB. The conversion between HSV and RGB isn't perfect and will introduce
            //small differences between the stored palette and what's displayed at default visual settings.
            //The purpose of this method is to make images of maps better match what the game displays.

            //Translation of "Adjust_Palette()" in "OPTIONS.CPP" from Red Alert's source.

            //TODO: Make my own conversion to and from HSV? Currently it is pretty much a straight copy
            //of the source code. The only piece of code in this whole project that I just copy-pasted
            //(feels bad). The conversion is a bit complicated though so I'm not sure if I'm able to
            //create my own version of it.

            PaletteRA result = new PaletteRA();
            for (int i = 0; i < result.mRgbEntries.Length; i++)
            {
                HsvEntry hsvEntry = toHsv(this.mRgbEntries[i]);

                //At default settings this shouldn't affect the HSV entry.
                int temp;
                temp = (int)((hsvEntry.v * brightness.mul(256).toUInt32()) / 0x80); //Brightness.
                temp = temp.clip(0, 0xFF);
                int v = temp;
                temp = (int)((((((int)v) - 0x80) * contrast.mul(256).toUInt32()) / 0x80) + 0x80); //Contrast.
                temp = temp.clip(0, 0xFF);
                v = temp;
                temp = (int)((hsvEntry.s * color.mul(256).toUInt32()) / 0x80); //Color.
                temp = temp.clip(0, 0xFF);
                int s = temp;
                temp = (int)((hsvEntry.h * tint.mul(256).toUInt32()) / 0x80); //Tint.
                temp = temp.clip(0, 0xFF);
                int h = temp;

                result.mRgbEntries[i] = toRgb(h, s, v);
            }
            result.mRgbEntries[16] = this.mRgbEntries[16]; //Index 16 isn't adjusted.
            return result;
        }

        public override byte[] createRemapTableConquer(byte targetIndex, byte fraction)
        {
            return createRemapTableConquerRA(targetIndex, fraction);
        }
    }
}
