using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Map background layer tiles (icons). Usually a graphics file inside theater MIX-files.
    //A template (meta-tile) is an image made of multiple tiles.
    abstract class FileIcnTileSetTDRA : FileBase
    {
        //The tile set file format used by Tiberian Dawn and Red Alert are similar. The Red Alert format
        //added some data (template size and radar color) that were stored internally in Tiberian Dawn.
        //-The Tiberian Dawn header is 32 bytes long and has 10 fields.
        //-The Red Alert header is 40 bytes long and has 13 fields.

        //{?} = unknown purpose field with name taken from the CnC Remastered Collection source code.
        //# = New header data added in the Red Alert format.
        //Layout: header,tile_entries,index_entries,unknown_entries. General order, but depends on offsets in header.
        //header:
        //-tile width UInt16: Width of tiles. Should always be 24?
        //-tile height UInt16: Height of tiles. Should always be 24?
        //-index entry count UInt16: Number of index entries. Not always same as number of tiles in file.
        //-{Allocated?} UInt16: Always 0?
        //#template width UInt16: Width of template in tiles.
        //#template height UInt16: Height of template in tiles. If 1*1 (width*height) then all tiles are individual.
        //-length UInt32: Length of file.
        //-tile entries offset UInt32: Offset of tile data [tile entry count] within file.
        //-{Palettes?} UInt32: Offset of palette data? Always 0? 
        //-{Remaps?} UInt32: Offset of remap index data? Always 0x0D1AFFFF in TD and >=0x2C730000 in RA? Always outside file?
        //-{TransFlag?} UInt32: Offset of transparency flag table [tile entry count] within file?
        //#{ColorMap?} UInt32: Offset of color control value table [template width*height] within file?
        //-index entries offset UInt32: Offset of index table [index entry count] within file.

        //tile entry:
        //-image UInt8[tile width*height]: Represents a simple 8-bit indexed tile width*height image.
        //{Palettes?} entry:
        //-???
        //{Remaps?} entry:
        //-???
        //{TransFlag?} entry:
        //-unknown UInt8: Controls drawing of palette index 0 for corresponding tile? Rarely used? Usually 0?
        //{ColorMap?} entry:
        //-unknown UInt8: Controls land type/color of tile in template (for corresponding index)? Should be 0-15?
        //index entry:
        //-tile index UInt8: Index of tile in template. 0xFF indicates an empty tile.

        //https://forums.cncnet.org/topic/1338-understanding-ra-format-tmp-terrain-files/
        //Quoting Blade: "It looks like the TransFlags array in the footer is an array of
        //boolean values that flags if the blitter should draw index 0 pixels or just skip
        //them and leave the pixel value that was there already for the cells they refer to."

        //Testing in game seems to corroborate Blade's finding above. TransFlag entry:
        //-0: Index 0 is drawn. Pixel is black.
        //-1: Index 0 is skipped. Pixel will cause a Hall-of-Mirror effect (because nothing is drawn?) in the map tile layer.
        //This flag doesn't really matter for the current map tile layer draw method because it must draw
        //something for all pixels so it can just as well draw a black pixel in both cases.

        protected const int ExpectedTileWidth = 24; //Tiles should be this big, but don't have to?
        protected const int ExpectedTileHeight = 24;
        protected const int ExpectedTileLength = ExpectedTileWidth * ExpectedTileHeight;
        protected const byte EmptyTileIndex = 0xFF;

        protected UInt16 mTileWidth;
        protected UInt16 mTileHeight;
        protected UInt16 mIndexEntryCount;
        protected UInt16 mAllocated;
        protected UInt32 mFileLength;
        protected UInt32 mTileEntriesOffset;
        protected UInt32 mPaletteEntriesOffset;
        protected UInt32 mRemapEntriesOffset;
        protected UInt32 mTransFlagEntriesOffset;
        protected UInt32 mIndexEntriesOffset;

        protected int mTileEntryCount; //Actual number of tiles in file.
        protected Frame[] mTileEntries;
        protected byte[] mTransFlagEntries;
        protected byte[] mIndexEntries;
        protected Frame mTileEntryEmpty = null; //Used for invalid tiles.

        protected FileIcnTileSetTDRA()
        {
        }

        protected FileIcnTileSetTDRA(string filePath)
            : base(filePath)
        {
        }

        protected FileIcnTileSetTDRA(FileProto fileProto)
            : base(fileProto)
        {
        }

        protected static void writeDummy(MemoryStream ms, bool isRA, UInt32 headerLength)
        {
            //For tile set id:s not found in theater. Easier than checking for null everywhere.
            //Tiberian Dawn and Red Alert seems to ignore tile sets not found and just not draw it.
            //This will write a dummy with a size of 24x24 and one frame of transparent pixels.

            //Calculate length of a file with one tile frame.
            UInt32 fileLength = 2; //TransFlag&Index entries.
            if (isRA)
            {
                fileLength += 1; //+ ColorMap entries.
            }
            fileLength += headerLength + ExpectedTileLength; //+ Header + Tile.

            //Header.
            ms.writeUInt16(ExpectedTileWidth); //Width.
            ms.writeUInt16(ExpectedTileHeight); //Height.
            ms.writeUInt16(1); //Index entry count.
            ms.writeUInt16(0); //Allocated?
            if (isRA)
            {
                ms.writeUInt16(1); //Template width.
                ms.writeUInt16(1); //Template height.
            }
            ms.writeUInt32(fileLength); //File length.
            ms.writeUInt32(headerLength); //Tile entries offset (after header).
            ms.writeUInt32(0); //Palettes offset?
            if (isRA)
            {
                ms.writeUInt32(0x2C730000); //Remaps offset?
                ms.writeUInt32(fileLength - 3); //TransFlag entries offset.
                ms.writeUInt32(fileLength - 2); //ColorMap entries offset.
            }
            else
            {
                ms.writeUInt32(0x0D1AFFFF); //Remaps offset?
                ms.writeUInt32(fileLength - 2); //TransFlag entries offset.
            }
            ms.writeUInt32(fileLength - 1); //Index entries offset.
            //Tile data.
            ms.writeArray(new byte[ExpectedTileLength]); //Empty tile data.
            //TransFlag entries.
            ms.writeUInt8(0);
            //ColorMap entries.
            if (isRA)
            {
                ms.writeUInt8(0); //0 == clear land type color.
            }
            //Index entries.
            ms.writeUInt8(0);
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "ICN" || ext == gfxFileExt;
        }

        protected void parse(Stream stream, bool isRA,
            out UInt16 templateWidth, out UInt16 templateHeight, out UInt32 colorMapEntriesOffset)
        {
            //Header.
            mTileWidth = stream.readUInt16();
            mTileHeight = stream.readUInt16();
            mIndexEntryCount = stream.readUInt16();
            mAllocated = stream.readUInt16();
            templateWidth = isRA ? stream.readUInt16() : UInt16.MinValue;
            templateHeight = isRA ? stream.readUInt16() : UInt16.MinValue;
            mFileLength = stream.readUInt32();
            mTileEntriesOffset = stream.readUInt32();
            mPaletteEntriesOffset = stream.readUInt32();
            mRemapEntriesOffset = stream.readUInt32();
            mTransFlagEntriesOffset = stream.readUInt32();
            colorMapEntriesOffset = isRA ? stream.readUInt32() : UInt32.MinValue;
            mIndexEntriesOffset = stream.readUInt32();
            checkHeaderTileSize();
            checkHeaderFileLength(mFileLength);
            if (mIndexEntryCount < 1)
            {
                throwParseError("Index entry count is less than '1'!");
            }

            //Length of a tile. Should be 24*24.
            int tileLength = mTileWidth * mTileHeight;

            //Calculate number of tiles that will fit within the file from the tile entries offset.
            //This could be higher than the actual number of tiles (tiles at the end will contain garbage) in some rare cases,
            //but as long as the index entries array is correct (only points to actual tiles) this shouldn't matter.
            //Actual number of tiles could be determined from the values in the index entries array.
            //But this relies on that the array values are valid (e.g. not too high) and would need a within file check.
            //This method is simpler and in most cases just as good.
            mTileEntryCount = (int)((Length - mTileEntriesOffset) / tileLength);
            if (mTileEntryCount < 1)
            {
                throwParseError("Tile entry count is less than '1'!");
            }
            //"LWAL0002.INT" is the only ICN-file in Tiberian Dawn and Red Alert which has room for more tiles (5)
            //than what is used in the index entries array (0-3). From other values in the file it looks like it
            //actually should have a tile count of 5, but trying to use the 5th tile in the game (with a modified file)
            //just causes the Hall-Of-Mirrors effect. Not sure how the game manages to figure out that only index 0-3
            //is valid. It's a very special case though (not in any normal maps) so not really worth bothering with.

            //Index entries.
            checkOffset(mIndexEntriesOffset, mIndexEntryCount);
            stream.Seek(Start + mIndexEntriesOffset, SeekOrigin.Begin);
            mIndexEntries = stream.readArray(mIndexEntryCount);

            //TransFlag entries.
            checkOffset(mTransFlagEntriesOffset, mTileEntryCount);
            stream.Seek(Start + mTransFlagEntriesOffset, SeekOrigin.Begin);
            mTransFlagEntries = stream.readArray(mTileEntryCount);

            //Tile entries.
            checkOffset(mTileEntriesOffset, mTileEntryCount * tileLength);
            stream.Seek(Start + mTileEntriesOffset, SeekOrigin.Begin);
            mTileEntries = new Frame[mTileEntryCount];
            for (int i = 0; i < mTileEntryCount; i++) //Parse all. Defer not worth it [*1].
            {
                mTileEntries[i] = new Frame(mTileWidth, mTileHeight, stream.readArray(tileLength));
            }
            //[*1] Defer parsing of tile entries to when they are requested doesn't make a noticeable
            //difference in speed. Probably because only a few entries in most tile sets?
        }

        private void checkHeaderTileSize()
        {
            if (mTileWidth != ExpectedTileWidth || mTileHeight != ExpectedTileHeight)
            {
                throwParseError(string.Format("Tile size in header '{0}*{1}' should always be '{2}*{3}'!",
                    mTileWidth, mTileHeight, ExpectedTileWidth, ExpectedTileHeight));
            }
        }

        public UInt16 IndexEntryCount
        {
            get { return mIndexEntryCount; }
        }

        public UInt32 TileEntriesOffset
        {
            get { return mTileEntriesOffset; }
        }

        public UInt32 TransFlagEntriesOffset
        {
            get { return mTransFlagEntriesOffset; }
        }

        public UInt32 IndexEntriesOffset
        {
            get { return mIndexEntriesOffset; }
        }

        public bool isEmptyTileIndex(byte tileIndex)
        {
            //Tiberian Dawn handles empty tile index as a clear tile template.
            //In Red Alert they just cause the Hall-Of-Mirrors effect.
            return mIndexEntries[getTileIndex(tileIndex)] == EmptyTileIndex;
        }

        public Frame getTile(byte tileIndex)
        {
            //Tiberian Dawn code should check if tile index is empty before calling this method. See above method.
            byte tileEntryIndex = mIndexEntries[getTileIndex(tileIndex)]; //Look up tile entry index in index entries.
            if (tileEntryIndex < mTileEntries.Length) //Will cover both empty (0xFF) and a too high entry.
            {
                return mTileEntries[tileEntryIndex];
            }
            return getTileEmpty();
        }

        private byte getTileIndex(byte tileIndex)
        {
            //Tile index seems to be set to 0 if too high in Tiberian Dawn and Red Alert. Checked in both games.
            if (tileIndex < mIndexEntries.Length)
            {
                return tileIndex;
            }
            return 0; //Zero index if too high.
        }

        private Frame getTileEmpty() //Returns an empty tile frame.
        {
            if (mTileEntryEmpty == null)
            {
                mTileEntryEmpty = Frame.createEmpty(mTileWidth, mTileHeight);
            }
            return mTileEntryEmpty;
        }

        public abstract byte getColorMap(byte tileIndex);

        public void debugSaveIcnImages(string folderName, Palette6Bit palette)
        {
            //This saves tiles in separate files which is slow to execute and then browse through afterwards.
            //Easy to see what indices tiles have though.
            string folderPath = Program.DebugOutPath + "icn\\" + folderName + "\\" + Id.ToLowerInvariant() + "\\";
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < mTileEntryCount; i++)
            {
                mTileEntries[i].save(palette, folderPath + i + ".png");
            }
        }

        public void debugSaveIcnSheet(string folderName, Palette6Bit palette, byte backgroundIndex)
        {
            //This saves tiles in the same file which is faster to execute and then browse through afterwards.
            string folderPath = Program.DebugOutPath + "icn\\" + folderName + " sheets\\";
            Frame.debugSaveFramesSheet(mTileEntries, palette, 8, backgroundIndex, folderPath, Name);
        }

        protected void debugSaveIcnTemplateInner(string folderName, Palette6Bit palette, byte backgroundIndex, int width, int height)
        {
            //This saves the tiles as a template. Width and height is the size of the template in tiles.
            if ((width * height) > mIndexEntries.Length)
            {
                throw new ArgumentException(string.Format("Template length '{0}*{1}={2}' should be max '{3}'!",
                    width, height, width * height, mIndexEntries.Length));
            }

            string folderPath = Program.DebugOutPath + "icn\\" + folderName + " templates\\";
            Directory.CreateDirectory(folderPath);

            Frame image = new Frame(width * mTileWidth, height * mTileHeight);

            //Set background color first.
            image.clear(backgroundIndex);

            string filePath = folderPath + Name;
            int tileInd = 0;
            //Save the template in the first file.
            for (int tileY = 0; tileY < height; tileY++)
            {
                for (int tileX = 0; tileX < width; tileX++, tileInd++)
                {
                    byte indexEntry = mIndexEntries[tileInd];
                    if (indexEntry != EmptyTileIndex)
                    {
                        Point dstPos = new Point(tileX * mTileWidth, tileY * mTileHeight);
                        image.write(mTileEntries[indexEntry], dstPos);
                    }
                }
            }
            image.save(palette, filePath + string.Format(" ({0}x{1},#1).png", width, height));

            //Save any remaining tiles in their own files.
            for (int nFile = 2; tileInd < mTileEntryCount; tileInd++, nFile++)
            {
                mTileEntries[tileInd].save(palette, filePath + string.Format(" (1x1,#{0}).png", nFile));
            }
        }
    }
}
