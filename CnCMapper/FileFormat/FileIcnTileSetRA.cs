using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    class FileIcnTileSetRA : FileIcnTileSetTDRA
    {
        private const int HeaderLength = 40;

        //Header data that were added in Red Alert to the original Tiberian Dawn format.
        private UInt16 mTemplateWidth;
        private UInt16 mTemplateHeight;
        private UInt32 mColorMapEntriesOffset;

        private byte[] mColorMapEntries;

        public FileIcnTileSetRA()
        {
        }

        public FileIcnTileSetRA(string filePath)
            : base(filePath)
        {
        }

        public FileIcnTileSetRA(FileProto fileProto)
            : base(fileProto)
        {
        }

        private static MemoryStream mDummyStream = null;
        private static Frame mDummyFrame = null;
        public static FileIcnTileSetRA createDummy(string fileName, Origin origin)
        {
            if (mDummyStream == null)
            {
                mDummyStream = new MemoryStream();
                mDummyFrame = Frame.createEmpty(ExpectedTileWidth, ExpectedTileHeight);
                writeDummy(mDummyStream, true, HeaderLength);
            }
            FileIcnTileSetRA fileIcn = new FileIcnTileSetRA(new FileProto(fileName, mDummyStream, "InternalDummy"));
            fileIcn.mTileEntries[0] = mDummyFrame; //Set the empty/dummy frame.
            fileIcn.mOrigin = origin;
            return fileIcn;
        }

        protected override void parseInit(Stream stream)
        {
            checkFileLengthMin(HeaderLength);
            parse(stream, true, out mTemplateWidth, out mTemplateHeight, out mColorMapEntriesOffset);

            int templateLength = mTemplateWidth * mTemplateHeight;

            if (templateLength < 1 || templateLength > mIndexEntries.Length)
            {
                throwParseError(string.Format("Template length '{0}*{1}={2}' should be 1 to '{3}'!",
                    mTemplateWidth, mTemplateHeight, templateLength, mIndexEntries.Length));
            }

            //ColorMap entries. If 1*1 template then one color map entry is used for all the tiles.
            checkOffset(mColorMapEntriesOffset, templateLength);
            stream.Seek(Start + mColorMapEntriesOffset, SeekOrigin.Begin);
            mColorMapEntries = stream.readArray(templateLength);
        }

        public UInt16 TemplateWidth
        {
            get { return mTemplateWidth; }
        }

        public UInt16 TemplateHeight
        {
            get { return mTemplateHeight; }
        }

        public override byte getColorMap(byte tileIndex)
        {
            //Looks like index wraps around if too high when getting color map and not set to 0 like in getTile().
            //Meaning that a too high tile index can have the wrong color map (i.e. not color map entry 0) in
            //the radar at scale 1. See "Land_Type()" in "CDATA.CPP". Also checked in game.
            return mColorMapEntries[tileIndex % (mTemplateWidth * mTemplateHeight)];
        }

        public void debugSaveIcnTemplate(string folderName, Palette6Bit palette, byte backgroundIndex)
        {
            debugSaveIcnTemplateInner(folderName, palette, backgroundIndex, mTemplateWidth, mTemplateHeight);
        }
    }
}
