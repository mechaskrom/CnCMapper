using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    class PaletteD2 : PaletteCnC
    {
        public PaletteD2(Palette6Bit copyFrom)
            : base(copyFrom)
        {
        }

        public static PaletteD2 getCopyWithColorCycling(Palette6Bit copyFrom)
        {
            //Dune 2 uses palette index 223, 239 and 255 (0xDF,0xEF,0xFF) for some color cycling animations.
            //Index 223 is for the glowing effect on the windtrap structure.
            //Let's set it to one of the values used by the game to make windtraps look better in maps.
            PaletteD2 palette = new PaletteD2(copyFrom);
            palette.mRgbEntries[223] = RgbEntry.createFrom8Bit(8, 8, 180); //Windtrap blue glow.
            return palette;
        }

        public override PaletteCnC getCopy()
        {
            return new PaletteD2(this);
        }

        public byte[] createRemapTable(byte targetIndex, byte fraction)
        {
            return createRemapTable(targetIndex, fraction, 1, 255, 1, 255, false, true, true, false, true);
        }
    }
}
