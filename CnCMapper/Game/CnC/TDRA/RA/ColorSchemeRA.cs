using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class ColorSchemeRA
    {
        private readonly byte[] mRemap;
        private readonly byte mRadarIndex;
        private readonly byte mRadarBrightIndex; //Not used by the game?

        private ColorSchemeRA(byte[] remap, byte radarIndex, byte radarBrightIndex)
        {
            mRemap = remap;
            mRadarIndex = radarIndex;
            mRadarBrightIndex = radarBrightIndex;
        }

        public byte[] Remap
        {
            get { return mRemap; }
        }

        public byte RadarIndex
        {
            get { return mRadarIndex; }
        }

        public byte RadarBrightIndex
        {
            get { return mRadarBrightIndex; }
        }

        private static ColorSchemeRA[] getColorSchemes()
        {
            //"REDALERT.MIX\LOCAL.MIX\PALETTE.CPS" has a picture of the master palette.
            //Color remaps are "stored" in the top left corner of this picture.
            //Data in this corner is used by the game to create 11 color remaps among other things.
            //First remap line (neutral/gold) selects which palette indices to remap.
            //Checked in source. See "Init_Color_Remaps()" in "INIT.CPP". And also:
            //https://forums.cncnet.org/topic/1234-replacing-colors-ra1-and-tiberian-dawn/

            //Blue and grey schemes are stored in the wrong order (not same as "PlayerColorType" enum)
            //and are swapped afterwards in the code. Lets just use the stored order to avoid this.

            //We only need the first 8 color remaps (house color schemes). The last 3 are for special uses.
            ColorSchemeRA[] colorSchemes = new ColorSchemeRA[8];
            Frame frame = new FileCpsImageWw(GameRA.FileMixRedAlertLocal.get().getFile("PALETTE.CPS")).Frame;
            for (int y = 0; y < colorSchemes.Length; y++)
            {
                byte[] remap = new byte[256];
                for (int x = 0; x < remap.Length; x++) //Init color remap table.
                {
                    remap[x] = (byte)x; //No color remap.
                }
                for (int x = 0; x < 16; x++)
                {
                    int indexToRemap = frame[x, 0]; //Get index to remap from first line (neutral/gold).
                    remap[indexToRemap] = frame[x, y]; //Remap to index in image.
                }
                byte radarIndex = frame[6, y]; //"Bar" in source code.
                byte radarBrightIndex = frame[1, y]; //"BrightColor" in source code.
                colorSchemes[y] = new ColorSchemeRA(remap, radarIndex, radarBrightIndex);

                //"BrightColor" is always set to 15 (white) instead of read from "PALETTE.CPS" in the source code?
            }
            return colorSchemes;
        }

        private static readonly ColorSchemeRA[] mColorSchemes = getColorSchemes();
        //Order as stored in the master palette picture.
        public static readonly ColorSchemeRA Gold = mColorSchemes[0];
        public static readonly ColorSchemeRA LightBlue = mColorSchemes[1];
        public static readonly ColorSchemeRA Red = mColorSchemes[2];
        public static readonly ColorSchemeRA Green = mColorSchemes[3];
        public static readonly ColorSchemeRA Orange = mColorSchemes[4];
        public static readonly ColorSchemeRA Grey = mColorSchemes[5];
        public static readonly ColorSchemeRA Blue = mColorSchemes[6]; //More like teal.
        public static readonly ColorSchemeRA Brown = mColorSchemes[7];
        public static readonly ColorSchemeRA Neutral = Gold;
    }
}
