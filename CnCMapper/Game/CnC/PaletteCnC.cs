using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC
{
    abstract class PaletteCnC : Palette6Bit
    {
        protected PaletteCnC()
            : base()
        {
        }

        protected PaletteCnC(Palette6Bit copyFrom)
            : base(copyFrom)
        {
        }

        public abstract PaletteCnC getCopy();

        protected byte[] createRemapTable(byte targetIndex, byte fraction, int startRemap, int endRemap, int startMatch, int endMatch,
            bool doSelf, bool doSelfTarget, bool doEqual, bool doEarly, bool doIdealTD)
        {
            //targetIndex = palette index of color to fade remap table towards.
            //fraction = level of fade to target color. 0=no change, 255=completely changed to target color.

            //startRemap & endRemap = range of indices to remap.
            //startMatch & endMatch = range of indices to find best match in.

            //doSelf true/false = Do or don't match index against itself.
            //doSelfTarget true/false = Do or don't match index against itself unless it's the target index [*1]. Ignored if doSelf==true.
            //doEqual true/false = Use <= or < when finding closest match.
            //doEarly true/false = Do or don't quit match early if diff is 0.
            //doIdealTD true/false = Use Tiberian Dawn or Red Alert ideal color calculation.

            //Creates special remap/fading tables. Adaptation of assembler/C++ versions in
            //the CnC Remastered Collection source code.
            //Tiberian Dawn and Red Alert have a few different, but very similar, functions to
            //create remap/fading tables. This method tries to consolidate them all.

            //Build_Fading_Table (TD): "DrawMisc.cpp".
            //Build_Fading_Table (RA): "DrawMisc.cpp". Same as in TD.
            //Conquer_Build_Fading_Table (TD): "MiscAsm.cpp".
            //Conquer_Build_Fading_Table (RA): "JSHELL.CPP".
            //Make_Fading_Table (RA): "JSHELL.CPP". Only in RA.

            //Build_Translucent_Table (TD & RA): "JSHELL.CPP". Uses Build_Fading_Table.
            //Conquer_Build_Translucent_Table (TD & RA): "JSHELL.CPP". Uses Conquer_Build_Fading_Table.

            //Calculations are in 6-bit color format.

            //[*1] This setting was added afterwards for Dune 2 to match the OpenDUNE version
            //of this function. See "GUI_Palette_CreateMapping" in "gui.c".
            //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/gui/gui.c#L1929
            //Dune 2's version is pretty much same as Tiberian Dawn's except it does matches
            //same as the target index a bit different.


            byte[] remap = new byte[PaletteEntryCount];
            RgbEntry rgbTarget = mRgbEntries[targetIndex]; //Target color to fade remap table towards.
            for (int i = 0; i < PaletteEntryCount; i++)
            {
                if (i >= startRemap && i <= endRemap) //Remap this index?
                {
                    //Calculate ideal faded color.
                    RgbEntry rgbIdeal;
                    if (doIdealTD) //Tiberian Dawn.
                    {
                        rgbIdeal = getRgbIdealTD(mRgbEntries[i], rgbTarget, fraction);
                    }
                    else //Red Alert.
                    {
                        rgbIdeal = getRgbIdealRA(mRgbEntries[i], rgbTarget, fraction);
                    }

                    //Check for closest match i.e. lowest sum of squared diffs.
                    int closestMatch = 0;
                    int closestDiff = int.MaxValue;
                    for (int j = startMatch; j <= endMatch; j++)
                    {
                        if (j != i || //Match not same as index?
                            doSelf || //Match can be same?
                            (doSelfTarget && j == targetIndex)) //Match can be same if it's the target index?
                        {
                            int diff = mRgbEntries[j].difference(rgbIdeal); //Sum of squared diffs.
                            if (diff < closestDiff || //Match is better (<)?
                                (doEqual && diff <= closestDiff)) //Match can be equal (<=)?
                            {
                                closestDiff = diff;
                                closestMatch = j;
                                if (doEarly && diff == 0) //Quit early if perfect match?
                                {
                                    break;
                                }
                            }
                        }
                    }
                    remap[i] = (byte)(closestMatch & 0xFF);
                }
                else //Don't remap this index.
                {
                    remap[i] = (byte)i;
                }
            }
            return remap;
        }

        private static RgbEntry getRgbIdealTD(RgbEntry source, RgbEntry target, int fraction) //Used by Tiberian Dawn (and Dune 2).
        {
            fraction >>= 1;
            RgbEntry ideal;
            ideal.r = (byte)(source.r - (((source.r - target.r) * fraction) >> 7));
            ideal.g = (byte)(source.g - (((source.g - target.g) * fraction) >> 7));
            ideal.b = (byte)(source.b - (((source.b - target.b) * fraction) >> 7));
            return ideal;
        }

        private static RgbEntry getRgbIdealRA(RgbEntry source, RgbEntry target, int fraction) //Used by Red Alert.
        {
            RgbEntry ideal;
            ideal.r = (byte)(source.r + (((target.r - source.r) * fraction) / 256));
            ideal.g = (byte)(source.g + (((target.g - source.g) * fraction) / 256));
            ideal.b = (byte)(source.b + (((target.b - source.b) * fraction) / 256));
            return ideal;
        }
    }
}
