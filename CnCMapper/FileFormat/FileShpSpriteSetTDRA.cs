using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Sprite (shape) set frames. Usually a graphic file inside theater MIX-files.
    //https://moddingwiki.shikadi.net/wiki/Westwood_SHP_Format_(TD)
    //http://xhp.xwis.net/documents/ccfiles4.txt
    class FileShpSpriteSetTDRA : FileBase
    {
        //Layout: header,frame_entries,ending_frame_entries,[palette],data.
        //header:
        //-frame count UInt16: Number of frames in file.
        //-x offset UInt16: Ignored.
        //-y offset UInt16: Ignored.
        //-width UInt16: Width of frames.
        //-height UInt16: Height of frames.
        //-delta length UInt16: Largest buffer length needed to decompress the frames.
        //-flags UInt16: Extra options. Palette present (unused?) among other things.
        //frame entry:
        //-data offset UInt24: Offset of compressed data. Offsets are from start of file i.e. start of header.
        //-data format UInt8: Format of compressed data. 0x80=LCW, 0x40=XOR base, 0x20=XOR chain.
        //-reference offset UInt24: Depends on format. 0x80=empty, 0x40=start of LCW data, 0x20=chain start frame number.
        //-reference format UInt8: Depends on format. 0x80=empty, 0x40=0x80 (LCW), 0x20=0x48.
        //ending frame entries:
        //-two extra frame entries with file length info and sometimes a loop frame (ignore?).

        private const int HeaderLength = 14;
        private const int FrameEntryLength = 8;
        private const int PalettePresentMask = 0x0002;

        private UInt16 mFrameCount;
        private UInt16 mOffsetX; //Ignored.
        private UInt16 mOffsetY; //Ignored.
        private UInt16 mWidth;
        private UInt16 mHeight;
        private UInt16 mDeltaLength;
        private UInt16 mFlags;
        private FrameEntry[] mFrameEntries;
        private Palette6Bit mPalette; //Unused?

        private struct FrameEntry
        {
            public UInt32 dataOffset; //UInt24.
            public byte dataFormat;
            public UInt32 refOffset; //UInt24.
            public byte refFormat;
        }

        private Frame[] mDecodedFrames;
        private Dictionary<UInt32, Frame> mLcwFrames; //Lookup table for decoded unique LCW frames.
        private Rectangle?[] mBoundingBoxes; //Cached bounding box that encloses all non-transparent pixels per frame.
        private Frame mFrameEmpty = null; //Used for invalid frames.

        public FileShpSpriteSetTDRA()
        {
        }

        public FileShpSpriteSetTDRA(string filePath)
            : base(filePath)
        {
        }

        public FileShpSpriteSetTDRA(FileProto fileProto)
            : base(fileProto)
        {
        }

        private static MemoryStream mDummyStream = null;
        private static Frame mDummyFrame = null;
        public static FileShpSpriteSetTDRA createDummy(string fileName, Origin origin)
        {
            //For sprite id:s not found. Easier than checking for null everywhere.
            //Tiberian Dawn and Red Alert seems to ignore sprites not found and just not draw them.
            //Maybe a problem though that the games still know what sprite it should be i.e. its properties like size.
            //This will return a dummy with a size of 24x24 and one frame of transparent pixels.
            if (mDummyStream == null)
            {
                Size size = new Size(24, 24);
                mDummyStream = new MemoryStream();
                mDummyFrame = Frame.createEmpty(size.Width, size.Height);
                writeShp(mDummyStream, new Frame[] { mDummyFrame }, size);
            }
            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(new FileProto(fileName, mDummyStream, "InternalDummy"));
            fileShp.mDecodedFrames[0] = mDummyFrame; //Set the empty/dummy frame.
            fileShp.mOrigin = origin;
            return fileShp;
        }

        public static FileShpSpriteSetTDRA create(string fileName, Frame[] frames, Size size)
        {
            MemoryStream stream = new MemoryStream();
            writeShp(stream, frames, size);
            return new FileShpSpriteSetTDRA(new FileProto(fileName, stream, "InternalCustom"));
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "SHP" || ext == gfxFileExt;
        }

        protected override void parseInit(Stream stream)
        {
            //Header.
            checkFileLengthMin(HeaderLength);
            mFrameCount = stream.readUInt16();
            mOffsetX = stream.readUInt16();
            mOffsetY = stream.readUInt16();
            mWidth = stream.readUInt16();
            mHeight = stream.readUInt16();
            mDeltaLength = stream.readUInt16();
            mFlags = stream.readUInt16();
            if (mFrameCount < 1)
            {
                throwParseError("Frame count is less than '1'!");
            }

            //Frame entries.
            long frameEntriesEndOffset = HeaderLength + ((mFrameCount + 2) * FrameEntryLength);
            checkFileLengthMin(frameEntriesEndOffset);
            mFrameEntries = new FrameEntry[mFrameCount];
            for (int i = 0; i < mFrameEntries.Length; i++)
            {
                mFrameEntries[i].dataOffset = stream.readUInt24();
                mFrameEntries[i].dataFormat = stream.readUInt8();
                mFrameEntries[i].refOffset = stream.readUInt24();
                mFrameEntries[i].refFormat = stream.readUInt8();
            }

            //Ending two frame entries. One should contain file length in a valid SHP-file.
            if (stream.readUInt24() != Length) //Check first ending entry.
            {
                stream.Seek(FrameEntryLength - 3, SeekOrigin.Current); //Already read 3 bytes from first frame entry.
                if (stream.readUInt24() != Length) //Check second ending entry.
                {
                    throwParseError(string.Format("Ending frame entries don't contain file length '{0}'!", Length));
                }
            }

            //Palette.
            if (IsPalettePresent)
            {
                checkFileLengthMin(frameEntriesEndOffset + Palette6Bit.PaletteLength);
                stream.Seek(Start + frameEntriesEndOffset, SeekOrigin.Begin);
                mPalette = new Palette6Bit(stream);
            }

            mDecodedFrames = new Frame[mFrameCount];
            mLcwFrames = new Dictionary<UInt32, Frame>();
            mBoundingBoxes = new Rectangle?[mFrameCount];

            //Do warnings last after parse didn't throw any errors.
            if ((mWidth * mHeight) == 0)
            {
                warn(string.Format("Zero size '{0}*{1}' (width*height)!", mWidth, mHeight));
            }
        }

        protected bool IsPalettePresent
        {
            get { return (mFlags & PalettePresentMask) != 0; }
        }

        public int Width
        {
            get { return mWidth; }
        }

        public int Height
        {
            get { return mHeight; }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        public int FrameCount
        {
            get { return mFrameCount; }
        }

        public Frame getFrame(int frameIndex)
        {
            //Return empty (draw nothing) if index is to high. Checked in game.
            if (frameIndex < mFrameCount)
            {
                if (mDecodedFrames[frameIndex] == null)
                {
                    decodeFramesUntil(frameIndex);
                }
                return mDecodedFrames[frameIndex];
            }
            return getFrameEmpty();
        }

        private Frame getFrameEmpty() //Return an empty frame.
        {
            if (mFrameEmpty == null)
            {
                mFrameEmpty = Frame.createEmpty(mWidth, mHeight);
            }
            return mFrameEmpty;
        }

        private void decodeFramesUntil(int index) //Decode frames until index.
        {
            //Need to decode preceding frames because of chain linking.
            //All frames can be pre-decoded by calling decodeFramesUntil(mFrameCount - 1) in parse init.
            //Sprites usually use frame 0 in maps so not decoding all frames in the file will save some time.

            for (int i = 0; i <= index; i++)
            {
                if (mDecodedFrames[i] != null) continue; //Already decoded?

                FrameEntry fe = mFrameEntries[i];
                Frame frame;
                Stream stream = getStream(fe.dataOffset);
                if (fe.dataFormat == 0x80) //LCW frame.
                {
                    if (!mLcwFrames.TryGetValue(fe.dataOffset, out frame)) //Not decoded yet?
                    {
                        frame = new Frame(mWidth, mHeight);
                        Crypt.Format80.decode(frame.Pixels, stream);
                        mLcwFrames.Add(fe.dataOffset, frame);
                    }
                }
                else if (fe.dataFormat == 0x40) //XOR with a LCW frame.
                {
                    if (fe.refFormat != 0x80)
                    {
                        warn(string.Format("Frame reference format should be 0x80 and not '{0}' in a 0x40 frame entry!", fe.refFormat));
                        System.Diagnostics.Debug.Assert(false);
                    }
                    frame = new Frame(mLcwFrames[fe.refOffset]); //Copy frame.
                    Crypt.Format40.decode(frame.Pixels, stream);
                }
                else if (fe.dataFormat == 0x20) //XOR with preceding frame.
                {
                    if (fe.refFormat != 0x48)
                    {
                        warn(string.Format("Frame reference format should be 0x48 and not '{0}' in a 0x20 frame entry!", fe.refFormat));
                        System.Diagnostics.Debug.Assert(false);
                    }
                    frame = new Frame(mDecodedFrames[i - 1]); //Copy frame.
                    Crypt.Format40.decode(frame.Pixels, stream);
                }
                else //Unknown format.
                {
                    warn(string.Format("Unknown frame data format '{0}'!", fe.dataFormat));

                    //Set undecoded frames as transparent dummies and exit.
                    for (; i <= index; i++)
                    {
                        if (mDecodedFrames[i] != null) continue; //Already decoded?
                        mDecodedFrames[i] = getFrameEmpty(); //Transparent dummy frame.
                    }
                    return;
                }
                mDecodedFrames[i] = frame;
            }
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
            return new Rectangle(0, 0, Width, Height);
        }

        public Frame[] copyFrames()
        {
            //Copy frames from sprite.
            Frame[] frames = new Frame[mFrameCount];
            for (int i = mFrameCount - 1; i >= 0; i--) //Reverse is a bit faster because of decodeFramesUntil().
            {
                frames[i] = new Frame(getFrame(i)); //Copy frame.
            }
            return frames;
        }

        public static void writeShp(string path, Frame[] frames, Size size)
        {
            using (FileStream fs = File.Create(path))
            {
                writeShp(fs, frames, size);
            }
        }

        private static void writeShp(Stream stream, Frame[] frames, Size size)
        {
            //Simple method to save custom frames into a valid SHP-file.
            //Uses format80 compression on all frames to keep it simple.
            //Pretty bad compression rate though.

            if (frames.Length > UInt16.MaxValue)
            {
                throw new ArgumentException("Max '" + UInt16.MaxValue + "' frames allowed!");
            }
            if (size.Width > UInt16.MaxValue)
            {
                throw new ArgumentException("Max '" + UInt16.MaxValue + "' width allowed!");
            }
            if (size.Height > UInt16.MaxValue)
            {
                throw new ArgumentException("Max '" + UInt16.MaxValue + "' height allowed!");
            }

            int frameCount = frames.Length;
            //Header.
            stream.writeUInt16((UInt16)frameCount); //Frame count.
            stream.writeUInt16(0); //X offset.
            stream.writeUInt16(0); //Y offset.
            stream.writeUInt16((UInt16)size.Width); //Width.
            stream.writeUInt16((UInt16)size.Height); //Height.
            stream.writeUInt16((UInt16)Math.Min(frames[0].Length, UInt16.MaxValue)); //Delta length.
            stream.writeUInt16(0); //Flags.

            //Skip frame entries and do sprite data first.
            int frameEntriesLength = (frameCount + 2) * FrameEntryLength;
            stream.Seek(frameEntriesLength, SeekOrigin.Current); //Skip frame entries.
            UInt32[] dataOffsets = new UInt32[frameCount];
            int dataLength = 0;
            for (int i = 0; i < frameCount; i++) //Write frames.
            {
                if (frames[i].Size != size)
                {
                    throw new ArgumentException("All frames are not equal to specified size!");
                }
                dataOffsets[i] = (UInt32)(HeaderLength + frameEntriesLength + dataLength);
                byte[] encodedData = Crypt.Format80.encode(frames[i].Pixels);
                stream.writeArray(encodedData);
                dataLength += encodedData.Length;
            }

            //Go back and do frame entries last.
            stream.Seek(-(frameEntriesLength + dataLength), SeekOrigin.Current);
            for (int i = 0; i < frameCount; i++) //Write frame entries.
            {
                stream.writeUInt24(dataOffsets[i]); //Data offset.
                stream.writeUInt8(0x80); //Data format.
                stream.writeUInt24(0); //Reference offset.
                stream.writeUInt8(0); //Reference format.
            }
            //First ending frame entry with file length.
            stream.writeUInt32((UInt32)(HeaderLength + frameEntriesLength + dataLength)); //File length.
            stream.writeUInt32(0);
            //Last ending frame entry with zeroes.
            stream.writeUInt32(0);
            stream.writeUInt32(0);
        }

        public void debugSaveShpImages(string folderName, Palette6Bit palette)
        {
            //This saves frames in separate files which is slow to execute and then browse through afterwards.
            //Easy to see what index frames have though.
            string folderPath = Program.DebugOutPath + "shp\\" + folderName + "\\" + Id.ToLowerInvariant() + "\\";
            Directory.CreateDirectory(folderPath);
            for (int i = mFrameCount - 1; i >= 0; i--) //Reverse is a bit faster because of decodeFramesUntil().
            {
                getFrame(i).save(palette, folderPath + i + ".png");
            }
        }

        public void debugSaveShpSheet(string folderName, Palette6Bit palette, byte backgroundIndex)
        {
            //This saves frames in the same file which is faster to execute and then browse through afterwards.
            string folderPath = Program.DebugOutPath + "shp\\" + folderName + " sheets\\";
            decodeFramesUntil(mFrameCount - 1); //Pre-decode all frames.
            Frame.debugSaveFramesSheet(mDecodedFrames, palette, 8, backgroundIndex, folderPath, Name);
        }
    }
}
