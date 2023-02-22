using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Map background layer tiles (icons). Used both for terrain and structures.
    //In Dune 2 tiles (icons) are stored in the "ICON.ICN" file and tile sets in the "ICON.MAP" file.
    class FileIcnTilesD2 : FileBase
    {
        //These files use the Interchange File Format (IFF). See the IFF chunk class implementation
        //for a little more info about that format.

        //The "ICON.ICN" file has a FORM top level chunk. The following sub-chunks are stored in its data:
        //ICON: No size nor data. Just identifies that this FORM top level chunk contains icons?
        //SINF: 4 bytes of unknown data. Specifies size and bit-depth of tiles?
        //SSET: 8 bytes of unknown data followed by TileCount*128 bytes of tile data.
        //      Each tile is 16*16 of 4-bit values. Each 4-bit value is a remap table index.
        //RPAL: Remap tables (4-bit to 8-bit palette index). Each table is 16 bytes of 8-bit values.
        //      Each value is an 8-bit palette index.
        //RTBL: TileCount bytes of 8-bit values. Each value is an index to a remap table in the RPAL chunk.

        //To convert a 4-bit pixel (N) in a tile (X) to its final 8-bit palette index (P): P = RPAL[RTBL[X]*16 + N]

        //It's difficult to find information about the "ICON.ICN" file format.
        //All comments and code within here are my understanding of it from reading forum posts
        //and other projects' source code. It may be incorrect, but seems to work well enough
        //to handle Dune 2 at least.
        //https://forum.dune2k.com/topic/20620-icn-map-format-specifications/#comment-345607
        //https://forum.dune2k.com/topic/19099-format80-decompression/page/3/#comment-336606

        //The unknown data in the SINF chunk may specify size and bit-depth of tiles according to this post:
        //https://forum.dune2k.com/topic/20620-icn-map-format-specifications/#comment-345633
        //The 4 bytes are usually 2,2,3,4 (width, height, shift, bits).
        //Tile width = width byte << shift byte. 2<<3=16.
        //Tile height = height byte << shift byte. 2<<3=16.

        //The "ICON.ICN" file in Dune 2 version 1.00 contains 399 tiles while version 1.07 contains 389 tiles.
        //The 10 missing tiles in 1.07 are for the:
        //-High-tech facility dome opening animation.
        //-Repair Facility gate halfway opened animation.
        //The "ICON.MAP" file is changed to account for these missing tiles. Same number of animation frames
        //in both versions. I wonder why these animations were removed? Seems pretty pointless.

        //The 8 bytes of unknown data in the SSET chunk seems to be different in Dune 2 versions:
        //-1.00 = 0, 0, 128, 199, 0, 0, 0, 0
        //-1.07 = 0, 0, 128, 194, 0, 0, 0, 0
        //Byte 2 and 3 could maybe be size of all tiles stored as a 16-bit integer (little endian)?
        //399*128=51072=0xC780
        //389*128=49792=0xC280

        //Testing seems to indicate that Dune 2 does not automatically treat index 0 in a
        //remap table as transparent unlike remap tables in SHP-files.
        //I.e. a 4-bit index 0 is not automatically remapped to a 8-bit index 0?

        private const string IdSinf = "SINF";
        private const string IdSset = "SSET";
        private const string IdRpal = "RPAL";
        private const string IdRtbl = "RTBL";

        private const int SsetUnknownLength = 8;
        private const int TileLength = 128; //16*16*4/8.
        private const int RemapLength = 16; //Remap table length.
        public const int TileWidth = 16; //Probably should read tile size from the SINF chunk?
        public const int TileHeight = 16;

        private byte[] mSinfUnknown; //The 4 bytes of unknown data in the SINF chunk.
        private byte[] mSsetUnknown; //The 8 bytes of unknown data in the SSET chunk.
        private byte[] mTileData; //The TileCount*128 bytes in the SSET chunk.
        private byte[][] mRemaps; //The remap tables in the RPAL chunk.
        private byte[] mRemapIndices; //The remap table indices in the RTBL chunk.

        private Frame[] mDecodedTiles = null;
        private Frame[] mRemappedTiles = null;

        public FileIcnTilesD2()
        {
        }

        public FileIcnTilesD2(string filePath)
            : base(filePath)
        {
        }

        public FileIcnTilesD2(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "ICN";
        }

        protected override void parseInit(Stream stream)
        {
            IffFile iff = IffFile.read(this, stream, Length);
            //iff.debugSaveChunks(Program.DebugOutPath + Name + " chunks.txt");

            //RTBL chunk.
            IffFile.Chunk rtbl = iff.get(IdRtbl);
            mRemapIndices = rtbl.Data;

            int tileCount = mRemapIndices.Length; //399 tiles for v1.00 and 389 for v1.07?

            //RPAL chunk.
            IffFile.Chunk rpal = iff.get(IdRpal);
            if (rpal.Size % RemapLength != 0)
            {
                throwParseError(IdRpal + " chunk has an unexpected data length!");
            }
            mRemaps = new byte[rpal.Size / RemapLength][];
            for (int i = 0; i < mRemaps.Length; i++)
            {
                mRemaps[i] = rpal.Data.takeBytes(i * RemapLength, RemapLength);
            }

            //SSET chunk.
            IffFile.Chunk sset = iff.get(IdSset);
            if (sset.Size != (SsetUnknownLength + (tileCount * TileLength)))
            {
                throwParseError(IdSset + " chunk has an unexpected data length!");
            }
            mSsetUnknown = sset.Data.takeBytes(0, SsetUnknownLength);
            mTileData = sset.Data.takeBytes(SsetUnknownLength, tileCount * TileLength);

            //SINF chunk.
            IffFile.Chunk sinf = iff.find(IdSinf);
            if (sinf != null)
            {
                mSinfUnknown = sinf.Data;
                int shift = mSinfUnknown[2];
                int width = mSinfUnknown[0] << shift;
                int height = mSinfUnknown[1] << shift;
                int bits = mSinfUnknown[3];
                if (width != TileWidth || height != TileHeight || bits != 4) //Always 16*16*4 in Dune 2.
                {
                    throwParseError(IdSinf + " chunk has unexpected data!");
                }
            }

            //Create actual tiles later when requested.
            mDecodedTiles = new Frame[tileCount];
            mRemappedTiles = new Frame[tileCount];
        }

        public Frame getTile(int tileIndex) //Tile without any remapping.
        {
            if (tileIndex >= mDecodedTiles.Length) //Invalid index?
            {
                //I haven't really checked that much what the game does with invalid tiles.
                //It doesn't crash though. Seems to display garbage/glitch tile?
                //Let's return tile at index 0 for now.
                warn(string.Format("Tile index '{0}' is out of range '{1}'!", tileIndex, mDecodedTiles.Length));
                tileIndex = 0;
            }

            Frame tile = mDecodedTiles[tileIndex];
            if (tile == null)
            {
                tile = decodeTile(tileIndex);
                mDecodedTiles[tileIndex] = tile;
            }
            return tile;
        }

        public Frame getTileRemapped(int tileIndex) //Tile with remap table applied.
        {
            if (tileIndex >= mRemappedTiles.Length) //Invalid index?
            {
                warn(string.Format("Tile index '{0}' is out of range '{1}'!", tileIndex, mRemappedTiles.Length));
                tileIndex = 0;
            }

            Frame tile = mRemappedTiles[tileIndex];
            if (tile == null)
            {
                tile = remapTile(tileIndex);
                mRemappedTiles[tileIndex] = tile;
            }
            return tile;
        }

        private Frame decodeTile(int tileIndex)
        {
            //Convert 4-bit tile data to 8-bit pixels.
            Frame tile = new Frame(TileWidth, TileHeight);
            for (int dst = 0, src = tileIndex * TileLength; dst < (TileLength * 2); dst += 2, src++)
            {
                byte v = mTileData[src];
                int lo = (v >> 0) & 0x0F; //Low nibble;
                int hi = (v >> 4) & 0x0F; //High nibble;
                tile[dst + 0] = (byte)hi; //High nibble is first (left pixel).
                tile[dst + 1] = (byte)lo; //Low nibble is second (right pixel).
            }
            return tile;
        }

        private Frame remapTile(int tileIndex)
        {
            Frame frame = new Frame(getTile(tileIndex)); //Make a copy and remap its pixels.
            byte[] remapTable = mRemaps[mRemapIndices[tileIndex]];
            byte[] pixels = frame.Pixels;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = remapTable[pixels[i]];
            }
            return frame;
        }

        public Frame[] copyTiles() //Tiles without any remapping.
        {
            //Copy tiles from icon file.
            Frame[] tiles = new Frame[mDecodedTiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new Frame(getTile(i)); //Copy tile.
            }
            return tiles;
        }

        public byte[][] copyRemaps()
        {
            //Copy remap tables from icon file.
            byte[][] remaps = new byte[mRemaps.Length][];
            for (int i = 0; i < remaps.Length; i++)
            {
                remaps[i] = mRemaps[i].takeBytes(); //Copy remap.
            }
            return remaps;
        }

        public byte[] copyRemapIndices()
        {
            //Copy remap table indices from icon file.
            return mRemapIndices.takeBytes(); //Copy remap indices.
        }

        public static void writeIcn(string path, Frame[] tiles, byte[][] remaps, byte[] remapIndices)
        {
            using (FileStream fs = File.Create(path))
            {
                writeIcn(fs, tiles, remaps, remapIndices);
            }
        }

        private static void writeIcn(Stream stream, Frame[] tiles, byte[][] remaps, byte[] remapIndices)
        {
            //Simple method to save custom tiles into a valid ICN-file.
            //All tiles must be 16*16 big and use 4-bit pixels (index 0-15).
            //All remaps must be 16 bytes in length.

            if (tiles.Length != remapIndices.Length)
            {
                throw new ArgumentException("Frame and remap indices count must be equal!");
            }

            MemoryStream ms = new MemoryStream();

            //ICON.
            ms.writeString("ICON"); //Only id, no size nor data.

            //SINF.
            ms.writeString("SINF"); //Id.
            ms.writeInt32BE(4); //Size.
            ms.writeArray(new byte[4] { 2, 2, 3, 4 }); //Usual SINF unknown data in Dune 2.

            //SSET.
            ms.writeString("SSET");
            int ssetDataLength = TileLength * tiles.Length;
            ms.writeInt32BE(SsetUnknownLength + ssetDataLength);
            ms.writeArray(new byte[SsetUnknownLength]
            {
                0, 0, (byte)(ssetDataLength>>0), (byte)(ssetDataLength>>8), 0, 0, 0, 0 //Use data length?
                //0, 0, 128, 199, 0, 0, 0, 0 //Version 1.00.
                //0, 0, 128, 194, 0, 0, 0, 0 //Version 1.07.
            }); //SSET unknown data.
            for (int i = 0; i < tiles.Length; i++)
            {
                Frame tile = tiles[i];
                if (tile.Width != TileWidth || tile.Height != TileHeight)
                {
                    throw new ArgumentException(string.Format("Size of frame '{0}' is not '{1}*{2}'!", i, TileWidth, TileHeight));
                }
                //Convert 8-bit pixels to 4-bit tile data.
                byte[] pixels = tiles[i].Pixels;
                for (int j = 0; j < pixels.Length; j += 2)
                {
                    byte hi = pixels[j + 0]; //High nibble is first (left pixel).
                    byte lo = pixels[j + 1]; //Low nibble is second (right pixel).
                    if (lo >= 16 || hi >= 16)
                    {
                        throw new ArgumentException(string.Format("Frame '{0}' has a non 4-bit pixel!", i));
                    }
                    ms.writeUInt8((byte)((hi << 4) | lo));
                }
            }

            //RPAL.
            ms.writeString("RPAL");
            ms.writeInt32BE(RemapLength * remaps.Length);
            for (int i = 0; i < remaps.Length; i++)
            {
                byte[] indices = remaps[i];
                if (indices.Length != RemapLength)
                {
                    throw new ArgumentException(string.Format("Length of remap '{0}' is not '{1}'!", i, RemapLength));
                }
                ms.writeArray(indices);
            }

            //RTBL.
            ms.writeString("RTBL");
            ms.writeInt32BE(remapIndices.Length);
            for (int i = 0; i < remapIndices.Length; i++)
            {
                byte index = remapIndices[i];
                if (index >= remaps.Length)
                {
                    throw new ArgumentException(string.Format("Remap index '{0}' is outside length of remaps '{1}'!", i, remaps.Length));
                }
                ms.writeUInt8(index);
            }

            //FORM.
            //Write all chunks to the FORM chunk's data.
            //Only the RTBL chunk can have an uneven size, but as it's written last we
            //don't have to worry about inserting any 0 pad bytes between chunks.
            byte[] formData = ms.ToArray();
            stream.writeString("FORM");
            stream.writeInt32BE(formData.Length);
            stream.writeArray(formData);
        }

        public void debugSaveIcnImages(string folderName, Palette6Bit palette, bool doRemap)
        {
            //This saves tiles in separate files which is slow to execute and then browse through afterwards.
            //Easy to see what indices tiles have though.
            string folderPath = Program.DebugOutPath + "icn\\" + folderName + "\\";
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < mDecodedTiles.Length; i++)
            {
                Frame tile = doRemap ? getTileRemapped(i) : getTile(i);
                tile.save(palette, folderPath + i + ".png");
            }
        }

        public void debugSaveIcnSheet(string folderName, Palette6Bit palette, byte backgroundIndex, bool doRemap)
        {
            //This saves tiles in the same file which is faster to execute and then browse through afterwards.
            string folderPath = Program.DebugOutPath + "icn\\" + folderName + " sheets\\";
            Frame[] tiles = new Frame[mDecodedTiles.Length];
            for (int i = 0; i < tiles.Length; i++) //Decode all tiles.
            {
                Frame tile = doRemap ? getTileRemapped(i) : getTile(i);
                tiles[i] = tile;
            }
            Frame.debugSaveFramesSheet(tiles, palette, 8, backgroundIndex, folderPath, Name);
        }
    }

    //https://en.wikipedia.org/wiki/Interchange_File_Format
    //Not a complete IFF implementation. Just enough to read the Dune 2 "ICON.ICN" file.
    class IffFile
    {
        //Reserved chunk id:s with particular IFF meanings: "LIST", "FORM", "PROP", "CAT ", and "    ".
        private const string IdForm = "FORM"; //Has chunks in its data section.

        //Dune 2 uses a non-standard chunk in the "ICON.ICN" file.
        private const string IdIcon = "ICON"; //Only has an id, no size nor data.

        private readonly FileBase mParent; //File it was created from.
        private readonly List<Chunk> mChunks;

        private IffFile(FileBase parent, List<Chunk> chunks)
        {
            mParent = parent;
            mChunks = chunks;
        }

        public static IffFile read(FileBase parent, Stream stream, long fileLength)
        {
            List<Chunk> chunks = new List<Chunk>();
            read(stream, fileLength, chunks);
            return new IffFile(parent, chunks);
        }

        private static void read(Stream stream, long fileLength, List<Chunk> chunks)
        {
            long fileStart = stream.Position;
            long endPosition = fileStart + fileLength;
            while (stream.Position < endPosition)
            {
                Chunk chunk = Chunk.read(stream, fileStart);
                chunks.Add(chunk);
                if (chunk.Id == IdForm) //Data is also chunks?
                {
                    MemoryStream ms = new MemoryStream(chunk.Data);
                    read(ms, ms.Length, chunks);
                }
            }
        }

        public Chunk find(string id) //Returns null if file is not found.
        {
            return mChunks.Find((c) => c.Id == id);
        }

        public Chunk get(string id) //Throws if not found.
        {
            Chunk chunk = find(id);
            if (chunk != null)
            {
                return chunk;
            }
            throw mParent.newArgError(string.Format("Couldn't find chunk '{0}'!", id));
        }

        public void debugSaveChunks(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Chunk chunk in mChunks)
            {
                sb.AppendLine("{0},{1}", chunk.Id, chunk.Size);
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        public class Chunk
        {
            private readonly string mId; //4 chars.
            private readonly Int32 mSize; //Size of data (signed int in big endian byte order).
            private readonly byte[] mData; //Data.

            //Total size of a chunk is data_size+4+4 bytes.
            //Chunks must begin on even file offsets. A 0 pad byte is inserted before a chunk if needed.
            //This pad byte is not included in the previous chunk's size value.

            private Chunk(string id, Int32 size, byte[] data)
            {
                mId = id;
                mSize = size;
                mData = data;
            }

            public string Id
            {
                get { return mId; }
            }

            public Int32 Size
            {
                get { return mSize; }
            }

            public byte[] Data
            {
                get { return mData; }
            }

            public static Chunk read(Stream stream, long fileStart)
            {
                //Make sure stream is at an even position relative to start before reading chunk.
                if ((stream.Position - fileStart) % 2 != 0)
                {
                    stream.ReadByte(); //Read and skip pad byte.
                }

                string id = stream.readChars(4);
                if (id == IdIcon) //This Dune 2 non-standard chunk only has an id.
                {
                    return new Chunk(IdIcon, 0, new byte[0]);
                }

                Int32 size = stream.readInt32BE(); //4 bytes in big endian order.
                byte[] data = stream.readArray(size);
                return new Chunk(id, size, data);
            }
        }
    }
}
