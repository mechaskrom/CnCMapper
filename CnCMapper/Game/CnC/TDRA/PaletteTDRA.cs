using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA
{
    abstract class PaletteTDRA : PaletteCnC
    {
        protected PaletteTDRA()
            : base()
        {
        }

        protected PaletteTDRA(Palette6Bit copyFrom)
            : base(copyFrom)
        {
        }

        public abstract PaletteTDRA getAdjusted();

        //"Build_Fading_Table()". Same in both Tiberian Dawn and Red Alert.
        public byte[] createRemapTable(byte targetIndex, byte fraction)
        {
            return createRemapTable(targetIndex, fraction, 1, 255, 1, 255, false, true, true, true);
        }

        //"Conquer_Build_Fading_Table()". Dynamic dispatch to Tiberian Dawn or Red Alert version.
        public abstract byte[] createRemapTableConquer(byte targetIndex, byte fraction);

        //"Conquer_Build_Fading_Table()". Tiberian Dawn version.
        protected byte[] createRemapTableConquerTD(byte targetIndex, byte fraction)
        {
            return createRemapTable(targetIndex, fraction, 1, 239, 240, 255, true, false, true, true);
        }

        //"Conquer_Build_Fading_Table()". Red Alert version.
        protected byte[] createRemapTableConquerRA(byte targetIndex, byte fraction)
        {
            //The index overlap (endIndex==startMatch==240) is probably unintentional (bug?), but that's what the source code does.
            return createRemapTable(targetIndex, fraction, 1, 240, 240, 254, true, false, false, false);
        }

        //"Make_Fading_Table()". Only in Red Alert.
        public byte[] createRemapTableFull(byte targetIndex, byte fraction)
        {
            //Probably need to set index 0 to 0 afterwards in returned table to make transparent black work.
            //Not sure why it's included in the remapped range, but that's what the source code does.
            return createRemapTable(targetIndex, fraction, 0, 255, 0, 255, true, false, false, false);
        }

        private byte[] createRemapTable(byte targetIndex, byte fraction, int startRemap, int endRemap, int startMatch, int endMatch,
            bool doSelf, bool doEqual, bool doEarly, bool doIdealTD)
        {
            return createRemapTable(targetIndex, fraction, startRemap, endRemap, startMatch, endMatch, doSelf, false, doEqual, doEarly, doIdealTD);
        }
    }
}
