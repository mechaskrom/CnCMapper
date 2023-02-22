using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    class ColorSchemeD2
    {
        private readonly byte[] mRemapUnit;
        private readonly byte[] mRemapStructure;
        private readonly byte mRadarIndex;

        private ColorSchemeD2(int paletteRow)
        {
            if (!GameD2.Config.LimitRemapRange)
            {
                //What the game does:
                mRemapUnit = createRemap(paletteRow, 0x98); //Units 0x90-0x98. Yes!, 0x98 and not 0x96.
                mRemapStructure = createRemap(paletteRow, 0xA0); //Structures 0x90-0xA0. Yes!, 0xA0 and not 0x96 or 0x9F.
            }
            else
            {
                //What it probably should do instead:
                mRemapUnit = createRemap(paletteRow, 0x96);
                mRemapStructure = createRemap(paletteRow, 0x96);
                //This will fix the IX remap error without affecting any other units or structures.
            }

            mRadarIndex = (byte)(paletteRow * 16); //Always first index in row of color scheme?
        }

        public byte[] RemapUnit
        {
            get { return mRemapUnit; }
        }

        public byte[] RemapStructure
        {
            get { return mRemapStructure; }
        }

        public byte RadarIndex
        {
            get { return mRadarIndex; }
        }

        //Color remap tables. Palette indices 0x90-0x98 for units and 0x90-0xA0 for structures are
        //remapped to a color scheme depending on which house they belong to. Checked in game.

        //In the game palette, "IBM.PAL", it looks like there are only 6 colors (0x90-0x96) per scheme so
        //I'm not sure why remap tables are bigger than that or why unit and structure remap tables have
        //different length. Just 0x90-0x96 for both would have been much more sensible.

        //I checked all frames in "UNITS.SHP", "UNITS1.SHP" and "UNITS2.SHP" and none used a final
        //palette index in the excessive remap range (0x97-0x98). So no unit seems to be affected by it.
        //Changing the remap range to just 0x90-0x96 will therefore not affect anything.

        //I checked all tiles in the "ICON.ICN" file and only tiles 286-290 (which uses remap tables 105-107)
        //are affected by the excessive remap range (0x97-0xA0). All these tiles belong to the "IX" structure
        //which uses some palette indices (0x98, 0x99, 0x9A) in this excessive remap range and will look weird
        //with other color schemes than neutral (Red/Harkonnen). Changing the remap range to just 0x90-0x96
        //will therefore fix this problem without negatively affecting anything else.

        //https://forum.dune2k.com/topic/18875-exe-editing-programming-issues/page/13/#comment-356305
        //MrFlibble Posted September 9, 2010:
        //"
        //With the help of Segra's database I've been able to track down and fix the notorious Ix remap bug.
        //To fix, search for the bytes (identical in all versions): 90 7C 09 3C A0
        //And replace 3C A0 with 3C 96
        //An interesting side note, Dune 2 has separate code for unit remap, structure remap and
        //button/menu coloured text remap, although the colours used in each case are all the same.
        //"

        public static readonly ColorSchemeD2 Grey = new ColorSchemeD2(8);
        public static readonly ColorSchemeD2 Red = new ColorSchemeD2(9);
        public static readonly ColorSchemeD2 Blue = new ColorSchemeD2(10);
        public static readonly ColorSchemeD2 Green = new ColorSchemeD2(11);
        public static readonly ColorSchemeD2 Orange = new ColorSchemeD2(12);
        public static readonly ColorSchemeD2 Purple = new ColorSchemeD2(13);
        public static readonly ColorSchemeD2 Brown = new ColorSchemeD2(14);
        public static readonly ColorSchemeD2 Neutral = Red;

        private static byte[] createRemap(int paletteRow, int rangeEnd)
        {
            //Init remap table to neutral (no remap).
            byte[] remap = new byte[256]; //16 rows * 16 colors.
            for (int i = 0; i < remap.Length; i++)
            {
                remap[i] = (byte)i;
            }
            //Set remap range.
            for (int i = 0x90, r = paletteRow * 16; i <= rangeEnd; i++, r++)
            {
                remap[i] = (byte)r;
            }
            return remap;
        }
    }
}
