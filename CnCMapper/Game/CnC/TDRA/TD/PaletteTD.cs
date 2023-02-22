using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class PaletteTD : PaletteTDRA
    {
        protected PaletteTD()
            : base()
        {
        }

        public PaletteTD(Palette6Bit copyFrom)
            : base(copyFrom)
        {
        }

        public static PaletteTD getCopyWithoutColorCycling(Palette6Bit copyFrom)
        {
            //Tiberian Dawn theaters build their fading/remap tables from their file palette, but with the water
            //cycling colors (index 32-38) set to white. See "Init_Theater()" in "DISPLAY.CPP". Checked in source.
            PaletteTD palette = new PaletteTD(copyFrom);
            RgbEntry rgbWhite = new RgbEntry(0x3F, 0x3F, 0x3F);
            for (int i = 32; i < 39; i++) //Set all cycling colors to white.
            {
                palette.mRgbEntries[i] = rgbWhite;
            }
            return palette;
        }

        public override PaletteCnC getCopy()
        {
            return new PaletteTD(this);
        }

        public override PaletteTDRA getAdjusted()
        {
            //The visual settings (visual controls) in the game where brightness, color, contrast and tint
            //can be adjusted is calculated in HSV space. The RGB entries in the palette is converted to
            //HSV and then back to RGB. The conversion between HSV and RGB isn't perfect and will introduce
            //small differences between the stored palette and what's displayed at default visual settings.
            //The purpose of this method is to make images of maps better match what the game displays.

            //Tiberian Dawn uses a slightly different "Adjust_Palette()" than Red Alert and the
            //source code for converting to and from HSV ("Convert_RGB_To_HSV()" and
            //"Convert_HSV_To_RGB()") isn't included(?) in the project unfortunately.

            //TODO: Find complete source for Tiberian Dawn's "Adjust_Palette()" and translate it.

            PaletteTD result = new PaletteTD();
            for (int i = 0; i < result.mRgbEntries.Length; i++)
            {
                RgbEntry rgbEntry = this.mRgbEntries[i];

                //Because lack of source code lets just manually change the six entries
                //known to be affected by the default settings adjustment in the three
                //standard theater palettes. This will not work with custom palettes however.
                if (rgbEntry.r == 57 && rgbEntry.g == 37 && rgbEntry.b == 12)
                {
                    rgbEntry.g = 38;
                }
                else if (rgbEntry.r == 53 && rgbEntry.g == 30 && rgbEntry.b == 4)
                {
                    rgbEntry.g = 31;
                }
                else if (rgbEntry.r == 41 && rgbEntry.g == 14 && rgbEntry.b == 0)
                {
                    rgbEntry.g = 15;
                }
                else if (rgbEntry.r == 55 && rgbEntry.g == 5 && rgbEntry.b == 2)
                {
                    rgbEntry.g = 4;
                }
                else if (rgbEntry.r == 51 && rgbEntry.g == 52 && rgbEntry.b == 9)
                {
                    rgbEntry.r = 50;
                }
                else if (rgbEntry.r == 40 && rgbEntry.g == 56 && rgbEntry.b == 7)
                {
                    rgbEntry.r = 41;
                }

                result.mRgbEntries[i] = rgbEntry;
            }
            result.mRgbEntries[255] = this.mRgbEntries[255]; //Index 255 isn't adjusted.
            return result;
        }

        public override byte[] createRemapTableConquer(byte targetIndex, byte fraction)
        {
            return createRemapTableConquerTD(targetIndex, fraction);
        }
    }
}
