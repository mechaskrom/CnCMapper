using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Color palette with 18-bit RGB entries i.e. 6-bit color channels.
    //https://moddingwiki.shikadi.net/wiki/VGA_Palette#The_.22Classic.22_format
    class FilePalPalette6Bit : FileBase
    {
        //Layout: colors.
        //colors rgb[256]:
        //-r UInt8: Red intensity. Only the 6 LSBs are used.
        //-g UInt8: Green intensity. Only the 6 LSBs are used.
        //-b UInt8: Blue intensity. Only the 6 LSBs are used.

        private const int PaletteLength = Palette6Bit.PaletteLength;

        private Palette6Bit mPalette;

        public FilePalPalette6Bit()
        {
        }

        public FilePalPalette6Bit(string filePath)
            : base(filePath)
        {
        }

        public FilePalPalette6Bit(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "PAL";
        }

        protected override void parseInit(Stream stream)
        {
            checkFileLengthExpected(PaletteLength);

            mPalette = new Palette6Bit(stream);
        }

        public Palette6Bit Palette
        {
            get { return mPalette; }
        }

        public void debugSavePalette()
        {
            mPalette.debugSavePalette(Name);
        }
    }
}
