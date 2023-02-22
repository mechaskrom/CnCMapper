using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CnCMapper.FileFormat
{
    class FileIcnTileSetTD : FileIcnTileSetTDRA
    {
        private const int HeaderLength = 32;

        public FileIcnTileSetTD()
        {
        }

        public FileIcnTileSetTD(string filePath)
            : base(filePath)
        {
        }

        public FileIcnTileSetTD(FileProto fileProto)
            : base(fileProto)
        {
        }

        private static MemoryStream mDummyStream = null;
        private static Frame mDummyFrame = null;
        public static FileIcnTileSetTD createDummy(string fileName, Origin origin)
        {
            if (mDummyStream == null)
            {
                mDummyStream = new MemoryStream();
                mDummyFrame = Frame.createEmpty(ExpectedTileWidth, ExpectedTileHeight);
                writeDummy(mDummyStream, false, HeaderLength);
            }
            FileIcnTileSetTD fileIcn = new FileIcnTileSetTD(new FileProto(fileName, mDummyStream, "InternalDummy"));
            fileIcn.mTileEntries[0] = mDummyFrame; //Set the empty/dummy frame.
            fileIcn.mOrigin = origin;
            return fileIcn;
        }

        protected override void parseInit(Stream stream)
        {
            checkFileLengthMin(HeaderLength);
            UInt16 dummyUInt16;
            UInt32 dummyUInt32;
            parse(stream, false, out dummyUInt16, out dummyUInt16, out dummyUInt32);
        }

        public override byte getColorMap(byte tileIndex)
        {
            throw new NotSupportedException("Template tile index color isn't stored in Tiberian Dawn ICN-files!");
        }

        public void debugSaveIcnTemplate(string folderName, Palette6Bit palette, byte backgroundIndex, Size templateSize)
        {
            debugSaveIcnTemplateInner(folderName, palette, backgroundIndex, templateSize.Width, templateSize.Height);
        }
    }
}
