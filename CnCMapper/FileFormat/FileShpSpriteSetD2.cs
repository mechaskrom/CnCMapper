using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Sprite (shape) set frames. Used mainly in Dune 2, but still used for the
    //mouse cursor (mouse.shp) in Tiberian Dawn and Red Alert.
    //https://moddingwiki.shikadi.net/wiki/Westwood_SHP_Format_(Dune_II)
    class FileShpSpriteSetD2 : FileBase
    {
        //Layout: header,frame_entries.
        //header:
        //-frame count UInt16: Number of frames in file.
        //-frame entry offsets UInt16/UInt32[frame count + 1]: Offsets to frame entries. Last should match file length.
        //frame entry:
        //-flags UInt16: Flags controlling some header fields. See below.
        //-slice count UInt8: Rows/lines in the RLE-data. Should(?) match the frame height.
        //-width UInt16: Frame width.
        //-height UInt8: Frame height.
        //-length UInt16: Length of frame entry in file (header + compressed image data).
        //-rle length UInt16: Length of RLE-compressed image data (before any LCW-compression).
        //-remap count UInt8: Number of remap table entries. Only present if flag field says so. Not used in Dune 2?
        //-remap table UInt8[]: Palette index remap table. Only present if flag field says so.

        //Each frame entry is followed by actual image data which should
        //be LCW-decompressed (if LCW-flag=yes) and then RLE-decompressed.

        //Frame entry offsets (two versions exist):
        //-Old: All 16-bit and relative to beginning of file.
        //-New: All 32-bit and relative to beginning of frame entry offsets (i.e. beginning of file + 2).
        //https://forum.dune2k.com/topic/19114-dune-ii-editor-with-107-support/?do=findComment&comment=328482
        //Dune 2 version 1.00 uses 16-bit offsets? Version 1.07 uses 32-bit offsets?

        //Frame entry flags:
        //-bit 0: Remap table is present in header (0=no, 1=yes).
        //-bit 1: RLE data is not LCW-compressed (0=no, 1=yes).
        //-bit 2: Remap count is present in header (0=no, 1=yes). Not used in Dune 2?

        //It seems like Dune 2 doesn't use (or support?) the optional remap count field in the header.
        //Any remap tables in Dune 2 SHP-files:
        //-16 bytes in length. Zeroes are padded at the end if image data has less than 16 indices.
        //-start with a 0 byte (only if image data contains an index 0?).
        //-indices are in the order they are found in the image data (except index 0 which is always first).

        //This together with some testing I did seems to indicate that Dune 2 treats frames in SHP-files
        //as either 4-bit or 8-bit. The remap table is just for converting 4-bit frames to 8-bit (kind of like
        //how the remap tables in the RPAL block in the ICON.ICN-file works). Index 0 is always(?) treated as
        //transparent (remapped to 0?) regardless of what the first value in a remap table is.
        //I.e. a 4-bit index 0 is automatically remapped to a 8-bit index 0?

        private const int HeaderLength = 2;
        private const int FrameEntryLengthMin = 10; //Minimum length i.e. excluding optional fields and image data.
        private const byte RemapCountDef = 16;

        private UInt16 mFrameCount;
        private FrameEntry[] mFrameEntries;

        private struct FrameEntry
        {
            public const int FlagHasRemapTable = 0x0001;
            public const int FlagNotLcwCompressed = 0x0002;
            public const int FlagHasRemapCount = 0x0004;

            //Header.
            public UInt16 flags;
            public byte sliceCount;
            public UInt16 width;
            public byte height;
            public UInt16 length; //Frame entry length in file (header + compressed image data).
            public UInt16 rleLength; //RLE-compressed image data length.
            public byte remapCount; //Optional. Default = 16.
            public byte[] remapTable; //Optional.

            //Custom member that stores an offset in the file to this frame's image data.
            public long imageDataOffset;

            public bool hasRemapTable { get { return (flags & FlagHasRemapTable) != 0; } }
            public bool notLcwCompressed { get { return (flags & FlagNotLcwCompressed) != 0; } }
            public bool hasRemapCount { get { return (flags & FlagHasRemapCount) != 0; } }
        }

        private enum HeaderVersion
        {
            Unknown,
            FrameEntryOffsets16bit,
            FrameEntryOffsets32bit,
        }

        private HeaderVersion mHeaderVersion = HeaderVersion.Unknown;
        private Frame[] mDecodedFrames = null;
        private Frame[] mRemappedFrames = null;
        private Rectangle?[] mBoundingBoxes; //Cached bounding box that encloses all non-transparent pixels per frame.

        public FileShpSpriteSetD2()
        {
        }

        public FileShpSpriteSetD2(string filePath)
            : base(filePath)
        {
        }

        public FileShpSpriteSetD2(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "SHP";
        }

        protected override void parseInit(Stream stream)
        {
            //Header.
            checkFileLengthMin(HeaderLength + 4); //Header + two (one + ending) 16-bit frame entry offsets.
            mFrameCount = stream.readUInt16();
            if (mFrameCount < 1)
            {
                throwParseError("Frame count is less than '1'!");
            }

            //Try to detect header version by looking for file length in last FEO (frame entry offset).
            UInt32[] feos = new UInt32[mFrameCount];
            //Try 16-bit first.
            int feoLength = (mFrameCount + 1) * 2;
            UInt32 firstFeo = stream.readUInt32();
            if (firstFeo > 0xFFFF && Length >= (HeaderLength + feoLength))
            {
                //16-bit offsets are from start of file.
                feos[0] = firstFeo & 0xFFFF;
                stream.Seek(-2, SeekOrigin.Current); //Restart at second offset in case frame count is 1.
                for (int i = 1; i < feos.Length; i++)
                {
                    feos[i] = stream.readUInt16();
                }
                UInt16 lastFeo = stream.readUInt16();
                if (lastFeo == Length) //Last offset matches file length?
                {
                    mHeaderVersion = HeaderVersion.FrameEntryOffsets16bit; //Probably 16-bit frame entry offsets.
                }
            }
            //Try 32-bit instead if it wasn't 16-bit.
            if (mHeaderVersion == HeaderVersion.Unknown && Length >= (HeaderLength + (feoLength * 2)))
            {
                //32-bit offsets are from start of frame entry offsets i.e. add length of header to get actual offset.
                feos[0] = firstFeo + HeaderLength;
                for (int i = 1; i < feos.Length; i++)
                {
                    feos[i] = stream.readUInt32() + HeaderLength;
                }
                UInt32 lastFeo = stream.readUInt32() + HeaderLength;
                if (lastFeo == Length) //Last offset matches file length?
                {
                    mHeaderVersion = HeaderVersion.FrameEntryOffsets32bit; //Probably 32-bit frame entry offsets.
                    feoLength *= 2;
                }
            }

            if (mHeaderVersion == HeaderVersion.Unknown) //Couldn't detect header version.
            {
                throwParseError("Couldn't detect header!");
            }

            //Frame entries.
            checkFileLengthMin(HeaderLength + feoLength + (mFrameCount * FrameEntryLengthMin));
            mFrameEntries = new FrameEntry[mFrameCount];
            for (int i = 0; i < mFrameEntries.Length; i++)
            {
                //Frame entry header.
                stream.Seek(Start + feos[i], SeekOrigin.Begin);
                FrameEntry fe = new FrameEntry();
                fe.flags = stream.readUInt16();
                fe.sliceCount = stream.readUInt8();
                fe.width = stream.readUInt16();
                fe.height = stream.readUInt8();
                fe.length = stream.readUInt16();
                fe.rleLength = stream.readUInt16();

                if (fe.hasRemapCount) //Is remap count present in header?
                {
                    fe.remapCount = stream.readUInt8();
                    //This is never used in Dune 2?
                    Program.warn(string.Format("Remap count '{0}' present in '{1}'!", fe.remapCount, FullName));
                }
                else
                {
                    fe.remapCount = RemapCountDef;
                }

                if (fe.hasRemapTable) //Is remap table present in header?
                {
                    fe.remapTable = stream.readArray(fe.remapCount);
                }
                else
                {
                    fe.remapTable = null;
                }

                //Header is followed by image data. Store offset and decode it later when needed.
                fe.imageDataOffset = stream.Position - Start;

                mFrameEntries[i] = fe;
            }

            //Create actual frames later when requested.
            mDecodedFrames = new Frame[mFrameCount];
            mRemappedFrames = new Frame[mFrameCount];
            mBoundingBoxes = new Rectangle?[mFrameCount];
        }

        public int FrameCount
        {
            get { return mFrameCount; }
        }

        public bool isFrameRemapped(int frameIndex)
        {
            //Only frames with a remap table are house colored in Dune 2. Checked in game.
            //Use this to check if a frame is remapped.
            return mFrameEntries[frameIndex].remapTable != null;
        }

        public Frame getFrame(int frameIndex) //Frame without any remapping.
        {
            Frame frame = mDecodedFrames[frameIndex];
            if (frame == null)
            {
                frame = decodeFrame(frameIndex);
                mDecodedFrames[frameIndex] = frame;
            }
            return frame;
        }

        public Frame getFrameRemapped(int frameIndex) //Frame with remap table (if present) applied.
        {
            Frame frame = mRemappedFrames[frameIndex];
            if (frame == null)
            {
                frame = remapFrame(frameIndex);
                mRemappedFrames[frameIndex] = frame;
            }
            return frame;
        }

        private Frame decodeFrame(int frameIndex)
        {
            FrameEntry fe = mFrameEntries[frameIndex];
            byte[] rleData = new byte[fe.rleLength];
            Stream stream = getStream(fe.imageDataOffset);
            if (fe.notLcwCompressed) //Is RLE-data not LCW-compressed?
            {
                stream.Read(rleData, 0, rleData.Length);
            }
            else //RLE-data is LCW-compressed.
            {
                Crypt.Format80.decode(rleData, stream);
            }
            //RLE-decode before returning final frame.
            byte[] pixels = Crypt.RleZeroD2.decode(rleData, fe.width, fe.height);
            return new Frame(fe.width, fe.height, pixels);
        }

        private Frame remapFrame(int frameIndex)
        {
            Frame frame = getFrame(frameIndex);
            byte[] remapTable = mFrameEntries[frameIndex].remapTable;
            if (remapTable != null) //Frame entry has a remap table?
            {
                frame = new Frame(frame); //Make a copy and remap its pixels.
                byte[] pixels = frame.Pixels;
                for (int i = 0; i < pixels.Length; i++)
                {
                    //pixels[i] = remapTable[pixels[i]];

                    byte p = pixels[i];
                    if (p != 0 && //Index 0 is always(?) treated as transparent i.e. never remapped.
                        p < remapTable.Length) //Just in case. Table should hold all values in frame.
                    {
                        pixels[i] = remapTable[p];
                    }
                }
            }
            return frame;
        }

        public Rectangle getBoundingBox(int frameIndex)
        {
            //Get rectangle that encloses all opaque pixels in sprite frame.
            if (frameIndex < mFrameCount)
            {
                Rectangle? boundingBox = mBoundingBoxes[frameIndex];
                if (!boundingBox.HasValue)
                {
                    boundingBox = getFrame(frameIndex).getBoundingBox();
                    mBoundingBoxes[frameIndex] = boundingBox;
                }
                return boundingBox.Value;
            }
            return new Rectangle(0, 0, 16, 16);
        }

        public Frame[] copyFrames() //Frames without any remapping.
        {
            //Copy frames from sprite file.
            Frame[] frames = new Frame[mFrameCount];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(getFrame(i)); //Copy frame.
            }
            return frames;
        }

        public byte[][] copyRemaps()
        {
            //Copy remap tables from sprite file.
            byte[][] remaps = new byte[mFrameCount][];
            for (int i = 0; i < remaps.Length; i++)
            {
                byte[] remap = mFrameEntries[i].remapTable;
                if (remap != null)
                {
                    remaps[i] = remap.takeBytes(); //Copy remap.
                }
            }
            return remaps;
        }

        public static void writeShp(string path, Frame[] frames, bool is16bitOffsetsVersion)
        {
            writeShp(path, frames, null, is16bitOffsetsVersion);
        }

        public static void writeShp(string path, Frame[] frames, byte[][] remaps, bool is16bitOffsetsVersion)
        {
            using (FileStream fs = File.Create(path))
            {
                writeShp(fs, frames, remaps, is16bitOffsetsVersion);
            }
        }

        private static void writeShp(Stream stream, Frame[] frames, byte[][] remaps, bool is16bitOffsetsVersion)
        {
            //Simple method to save custom frames into a valid SHP-file.
            //Uses format80 compression on all frames to keep it simple.

            if (remaps != null && remaps.Length != frames.Length)
            {
                throw new ArgumentException("Remap and frame count must be equal!");
            }

            //Create frame entries and image data.
            FrameEntry[] frameEntries = new FrameEntry[frames.Length];
            byte[][] imageData = new byte[frames.Length][];
            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];
                if (frame.Width > UInt16.MaxValue)
                {
                    throw new ArgumentException("Max '" + UInt16.MaxValue + "' width allowed!");
                }
                if (frame.Height > byte.MaxValue)
                {
                    throw new ArgumentException("Max '" + byte.MaxValue + "' height allowed!");
                }

                int feFlags = 0;
                int feLength = FrameEntryLengthMin;
                int feRemapCount = RemapCountDef;
                byte[] feRemapTable = null;

                //Remap table.
                if (remaps != null)
                {
                    feRemapTable = remaps[i];
                    if (feRemapTable != null)
                    {
                        feFlags |= FrameEntry.FlagHasRemapTable; //Has remap table.
                        byte[] copy;
                        if (feRemapTable.Length <= RemapCountDef) //Default remap count?
                        {
                            copy = new byte[RemapCountDef];
                        }
                        else if (feRemapTable.Length <= 255) //Custom remap count?
                        {
                            feFlags |= FrameEntry.FlagHasRemapCount; //Has remap count.
                            feLength += 1;
                            copy = new byte[feRemapTable.Length];
                        }
                        else
                        {
                            throw new ArgumentException("Max '255' remap count allowed!");
                        }
                        Buffer.BlockCopy(feRemapTable, 0, copy, 0, feRemapTable.Length);
                        feLength += copy.Length;
                        feRemapCount = copy.Length;
                        feRemapTable = copy;
                    }
                }

                //Compress frame.
                byte[] rleData = Crypt.RleZeroD2.encode(frame.Pixels, frame.Width, frame.Height);
                byte[] lcwData = Crypt.Format80.encode(rleData);
                feLength += lcwData.Length;

                frameEntries[i].flags = (UInt16)feFlags;
                frameEntries[i].sliceCount = (byte)frame.Height;
                frameEntries[i].width = (UInt16)frame.Width;
                frameEntries[i].height = (byte)frame.Height;
                frameEntries[i].length = (UInt16)feLength;
                frameEntries[i].rleLength = (UInt16)rleData.Length;
                frameEntries[i].remapCount = (byte)feRemapCount;
                frameEntries[i].remapTable = feRemapTable;

                imageData[i] = lcwData;
            }

            //Header (frame count + frame entry offsets).
            //16-bit offsets are from start of file. 32-bit are from start of frame entry offsets.
            int offsetCount = frames.Length + 1; //One extra offset to the end of the file.
            int offset = is16bitOffsetsVersion ? HeaderLength + (offsetCount * 2) : offsetCount * 4;
            stream.writeUInt16((UInt16)frames.Length); //Frame count.
            for (int i = 0; i < offsetCount; i++)
            {
                if (is16bitOffsetsVersion)
                {
                    stream.writeUInt16((UInt16)offset);
                }
                else
                {
                    stream.writeUInt32((UInt32)offset);
                }

                if (i < frames.Length)
                {
                    offset += frameEntries[i].length;
                }
            }

            //Frame entries (frame entry header + image data).
            for (int i = 0; i < frames.Length; i++)
            {
                FrameEntry fe = frameEntries[i];

                //Frame entry header.
                stream.writeUInt16(fe.flags); //Flags.
                stream.writeUInt8(fe.sliceCount); //Slice count (same as height).
                stream.writeUInt16(fe.width); //Width.
                stream.writeUInt8(fe.height); //Height.
                stream.writeUInt16(fe.length); //Length (header + image data).
                stream.writeUInt16(fe.rleLength); //RLE length.
                if (fe.hasRemapCount)
                {
                    stream.writeUInt8(fe.remapCount); //Remap count.
                }
                if (fe.hasRemapTable)
                {
                    stream.writeArray(fe.remapTable); //Remap table.
                }

                //Frame image data.
                stream.writeArray(imageData[i]);
            }
        }

        public void debugSaveShpRemapTables(string folderName)
        {
            string folderPath = Program.DebugOutPath + "shpdune\\" + folderName + " remaps\\";
            Directory.CreateDirectory(folderPath);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mFrameEntries.Length; i++)
            {
                sb.Append(i.ToString("D3") + ":");
                byte[] remap = mFrameEntries[i].remapTable;
                if (remap != null)
                {
                    foreach (byte r in remap)
                    {
                        sb.Append("0x" + r.ToString("X2") + ",");
                    }
                }
                else
                {
                    sb.Append("(none)");
                }
                sb.AppendLine();
            }
            File.WriteAllText(folderPath + Id.ToLowerInvariant() + " remap.txt", sb.ToString());
        }

        public void debugSaveShpImages(string folderName, Palette6Bit palette, bool doRemap)
        {
            //This saves frames in separate files which is slow to execute and then browse through afterwards.
            //Easy to see what index frames have though.
            string folderPath = Program.DebugOutPath + "shpdune\\" + folderName + "\\" + Id.ToLowerInvariant() + "\\";
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < mDecodedFrames.Length; i++)
            {
                Frame frame = doRemap ? getFrameRemapped(i) : getFrame(i);
                frame.save(palette, folderPath + i + ".png");
            }
        }

        public void debugSaveShpSheet(string folderName, Palette6Bit palette, byte backgroundIndex, bool doRemap)
        {
            //This saves frames in the same file which is faster to execute and then browse through afterwards.
            string folderPath = Program.DebugOutPath + "shpdune\\" + folderName + " sheets\\";
            Frame[] frames = new Frame[mDecodedFrames.Length];
            for (int i = 0; i < frames.Length; i++) //Decode all frames.
            {
                Frame frame = doRemap ? getFrameRemapped(i) : getFrame(i);
                frames[i] = frame;
            }
            Frame.debugSaveFramesSheet(frames, palette, 8, backgroundIndex, folderPath, Name);
        }
    }
}
