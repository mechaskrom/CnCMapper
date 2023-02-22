using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CnCMapper.FileFormat
{
    //Westwood font V3 (V4 support is not complete nor tested).
    //http://www.shikadi.net/moddingwiki/Westwood_Font_Format_v3
    //http://www.shikadi.net/moddingwiki/Westwood_Font_Format_v4
    class FileFntFontWw : FileBase
    {
        //Layout: header,offset_entries,width_entries,char_entries,height_entries. General order, but depends on offsets in header (always first).
        //header:
        //-file length UInt16: Length of file.
        //-signature UInt32: 0x000E0500=V3, 0x000E0002=V4.
        //-offset entries offset UInt16: Offset of offset data within file. Always 0x0014?
        //-width entries offset UInt16: Offset of width data within file.
        //-char entries offset UInt16: Offset of char data within file. Unused in V3.
        //-height entries offset UInt16: Offset of height data within file.
        //-unknown UInt24: Always 0x001012 in V3 and 0x000000 in V4?
        //-last char UInt8: Value of last char (actual char count is one more). Unused in V4.
        //-height UInt8: Overall max height of chars in pixels.
        //-width UInt8: Overall max width of chars in pixels.
        //offset entry:
        //-offset UInt16: Offset of char data within file. Absolute in V3, relative to char entries offset in V4.
        //width entry:
        //-width UInt8: Width of char in pixels.
        //char entry:
        //-char image data UInt4: 4-bit pixel image data (palette index). Rounded up to whole bytes per line.
        //height entry:
        //-y offset UInt8: Y offset for drawned char.
        //-height UInt8: Height of char in pixels.

        private const int HeaderLength = 20;
        private const UInt32 SignatureV3 = 0x000E0500;
        private const UInt32 SignatureV4 = 0x000E0002;
        private const int OffsetEntryLength = 2;
        private const int WidthEntryLength = 1;
        private const int HeightEntryLength = 2;

        private struct HeightEntry
        {
            public byte yOffset;
            public byte height;
        };

        private UInt16 mFileLength;
        private UInt32 mSignature;
        private UInt16 mOffsetEntriesOffset;
        private UInt16 mWidthEntriesOffset;
        private UInt16 mCharEntriesOffset;
        private UInt16 mHeightEntriesOffset;
        private UInt32 mUnknown; //UInt24.
        private byte mLastChar;
        private byte mHeight;
        private byte mWidth;

        private UInt16[] mOffsetEntries; //Offsets from start of file in V3, from CharEntriesOffset in V4.
        private byte[] mWidthEntries;
        private HeightEntry[] mHeightEntries;

        private int mCharCount;

        public struct CharData //Stores draw info for a char.
        {
            private readonly Frame mFrame; //Char frame.
            private readonly Point mOffset; //Char draw offset in pixels.

            public CharData(int width, int height, byte[] pixels, int yOffset)
            {
                mFrame = new Frame(width, height, pixels);
                mOffset = new Point(0, yOffset);
            }

            public Frame Frame
            {
                get { return mFrame; }
            }

            public Point Offset
            {
                get { return mOffset; }
            }
        }
        private CharData?[] mCharData;
        private CharData? mCharDataEmpty; //Used for invalid chars.

        public FileFntFontWw()
        {
        }

        public FileFntFontWw(string filePath)
            : base(filePath)
        {
        }

        public FileFntFontWw(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "FNT";
        }

        protected override void parseInit(Stream stream)
        {
            //Header.
            checkFileLengthMin(HeaderLength);
            mFileLength = stream.readUInt16();
            mSignature = stream.readUInt32();
            mOffsetEntriesOffset = stream.readUInt16();
            mWidthEntriesOffset = stream.readUInt16();
            mCharEntriesOffset = stream.readUInt16();
            mHeightEntriesOffset = stream.readUInt16();
            mUnknown = stream.readUInt24();
            mLastChar = stream.readUInt8();
            mHeight = stream.readUInt8();
            mWidth = stream.readUInt8();

            //Check values in header.
            checkHeaderFileLength(mFileLength);
            if (!IsV3) //Only V3 FNT-files are supported for now.
            {
                throwParseError(string.Format("Unsupported signature '0x{0:X}'!", mSignature));
                //TODO: Maybe support/test other FNT-file versions too?
            }

            mCharCount = mLastChar + 1; //Must be calculated from entry array lengths in V4?

            //Offset entries.
            checkOffset(mOffsetEntriesOffset, mCharCount * OffsetEntryLength);
            stream.Seek(Start + mOffsetEntriesOffset, SeekOrigin.Begin);
            mOffsetEntries = new ushort[mCharCount];
            for (int i = 0, offset = IsV4 ? mCharEntriesOffset : 0; i < mOffsetEntries.Length; i++)
            {
                mOffsetEntries[i] = (UInt16)(stream.readUInt16() + offset);
            }

            //Width entries.
            checkOffset(mWidthEntriesOffset, mCharCount * WidthEntryLength);
            stream.Seek(Start + mWidthEntriesOffset, SeekOrigin.Begin);
            mWidthEntries = stream.readArray(mCharCount);

            //Height entries.
            checkOffset(mHeightEntriesOffset, mCharCount * HeightEntryLength);
            stream.Seek(Start + mHeightEntriesOffset, SeekOrigin.Begin);
            mHeightEntries = new HeightEntry[mCharCount];
            for (int i = 0; i < mHeightEntries.Length; i++)
            {
                mHeightEntries[i].yOffset = stream.readUInt8();
                mHeightEntries[i].height = stream.readUInt8();
            }

            mCharData = new CharData?[mCharCount];
        }

        public bool IsV3
        {
            get { return mSignature == SignatureV3; }
        }

        public bool IsV4
        {
            get { return mSignature == SignatureV4; }
        }

        public byte Width
        {
            get { return mWidth; }
        }

        public byte Height
        {
            get { return mHeight; }
        }

        public CharData getCharData(byte charIndex)
        {
            CharData? charData;
            if (charIndex < mCharCount)
            {
                charData = mCharData[charIndex];
                if (!charData.HasValue) //Char is not cached?
                {
                    //Chars can share file offset, but have different sizes.
                    byte charWidth = mWidthEntries[charIndex];
                    byte charHeight = mHeightEntries[charIndex].height;
                    byte charOffsetY = mHeightEntries[charIndex].yOffset;
                    byte[] charPixels = null;
                    if (charWidth * charHeight > 0)
                    {
                        charPixels = new byte[charWidth * charHeight];
                        Stream stream = getStream(mOffsetEntries[charIndex]);
                        for (int y = 0, k = 0; y < charHeight; y++, k += charWidth)
                        {
                            for (int x = 0, b = 0; x < charWidth; x++)
                            {
                                //Convert 4-bit pixels to 8-bit. Split every byte (x is even) into 2 separate pixels.
                                b = (x & 1) == 0 ? stream.readUInt8() : b >> 4;
                                charPixels[k + x] = (byte)(b & 0x0F);
                            }
                        }
                        charData = new CharData(charWidth, charHeight, charPixels, charOffsetY);
                    }
                    else //Weird size.
                    {
                        warn(string.Format("Char '{0}'=[{1}] has weird size '{2}*{3}'!",
                            (char)charIndex, charIndex, charWidth, charHeight));

                        //Set weird sized chars to empty for now.
                        charData = getCharDataEmpty();
                        //TODO: Maybe figure out a better way to handle weird font chars?
                    }
                    mCharData[charIndex] = charData;
                }
            }
            else
            {
                warn(string.Format("Char '{0}' is over char count '{1}'!", (char)charIndex, mCharCount));
                charData = getCharDataEmpty();
            }
            return charData.Value;
        }

        private CharData getCharDataEmpty() //Return an empty char frame.
        {
            if (!mCharDataEmpty.HasValue)
            {
                mCharDataEmpty = new CharData(mWidth, mHeight, new byte[mWidth * mHeight], 0);
            }
            return mCharDataEmpty.Value;
        }

        #region Write FNT-file
        public static void writeFnt(string fontSheetPath, string savePath)
        {
            //Simple method to save a font sheet into a valid FNT-file.
            //Font sheet must be 16*16 chars big and 3 colors have special meanings (r,g,b):
            //-white (255,255,255) is converted to palette index 0 i.e. transparent.
            //-magenta (255,0,255) is not drawn, but included when calculating size of char.
            //-cyan (0,255,255) is converted to palette index 2. Usually used as drop shadow color.
            //-all other colors are converted to palette index 1.
            using (FileStream fs = File.Create(savePath))
            {
                byte[][,] chars = new byte[256][,];
                Size fontSize;
                using (Bitmap bmp = new Bitmap(fontSheetPath))
                {
                    if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                    {
                        throw new ArgumentException("Only 24-bit RGB font sheet supported!");
                    }
                    if (bmp.Width % 16 != 0 || bmp.Height % 16 != 0)
                    {
                        throw new ArgumentException("Font sheet's width and height must be a multiple of 16!");
                    }
                    byte[] bmpPixels = bmp.getPixels();
                    Size bmpSize = bmp.Size;
                    int bmpStride = bmpPixels.Length / bmp.Height;
                    Rectangle rc = new Rectangle(0, 0, bmp.Width / 16, bmp.Height / 16);
                    for (int i = 0; rc.Y < bmpSize.Height; rc.Y += rc.Height)
                    {
                        for (rc.X = 0; rc.X < bmpSize.Width; rc.X += rc.Width, i++)
                        {
                            chars[i] = toCharPixels(bmpPixels, bmpStride, rc);
                            addCharShadow(chars[i]);
                        }
                    }
                    fontSize = rc.Size;
                }
                writeFnt(fs, chars, fontSize);
            }
        }

        private static void writeFnt(Stream stream, byte[][,] chars, Size fontSize)
        {
            //Simple method to save 8-bit chars into a valid FNT-file.
            if (chars.Length < 1)
            {
                throw new ArgumentException("At least one char needed!");
            }
            if (fontSize.Width < 1 || fontSize.Width > 255 || fontSize.Height < 1 || fontSize.Height > 255)
            {
                throw new ArgumentException("Font size must be 1*1 to 255*255!");
            }
            foreach (byte[,] chr in chars) //Check char sizes.
            {
                if (chr.GetLength(1) > fontSize.Width || chr.GetLength(0) > fontSize.Height)
                {
                    throw new ArgumentException("Char can't be bigger than font size!");
                }
            }

            long streamStart = stream.Position; //Used later for debug asserting some conditions.
            int charCount = chars.Length;
            int offsetEntriesOffset = HeaderLength; //After header.
            int widthEntriesOffset = offsetEntriesOffset + (charCount * 2); //After offset entries.
            int heightEntriesOffset = widthEntriesOffset + (charCount * 1); //After width entries.
            int charEntriesOffset = heightEntriesOffset + (charCount * 2); //After height entries.

            UInt16[] offsetEntries = new UInt16[charCount];
            byte[] widthEntries = new byte[charCount];
            HeightEntry[] heightEntries = new HeightEntry[charCount];
            byte[][] charEntries = new byte[charCount][];
            int charsLength;
            toEntries(chars, offsetEntries, widthEntries, heightEntries, charEntries, out charsLength);

            //Header.
            stream.writeUInt16((UInt16)(charEntriesOffset + charsLength)); //File length.
            stream.writeUInt32(SignatureV3); //Signature V3.
            stream.writeUInt16((UInt16)offsetEntriesOffset); //Offset entries offset.
            stream.writeUInt16((UInt16)widthEntriesOffset); //Width entries offset.
            stream.writeUInt16((UInt16)charEntriesOffset); //Char entries offset.
            stream.writeUInt16((UInt16)heightEntriesOffset); //Height entries offset.
            stream.writeUInt24(0x001012); //Unknown. Always 0x001012 in V3?
            stream.writeUInt8((byte)(charCount - 1)); //Last char.
            stream.writeUInt8((byte)fontSize.Height); //Height.
            stream.writeUInt8((byte)fontSize.Width); //Width.

            //Offset entries.
            System.Diagnostics.Debug.Assert(stream.Position - streamStart == offsetEntriesOffset);
            for (int i = 0, offset = charEntriesOffset; i < offsetEntries.Length; i++)
            {
                //offset = char entries offset in V3, 0 in V4.
                stream.writeUInt16((UInt16)(offsetEntries[i] + offset));
            }

            //Width entries.
            System.Diagnostics.Debug.Assert(stream.Position - streamStart == widthEntriesOffset);
            stream.writeArray(widthEntries);

            //Height entries.
            System.Diagnostics.Debug.Assert(stream.Position - streamStart == heightEntriesOffset);
            for (int i = 0; i < heightEntries.Length; i++)
            {
                stream.writeUInt8(heightEntries[i].yOffset);
                stream.writeUInt8(heightEntries[i].height);
            }

            //Char entries.
            System.Diagnostics.Debug.Assert(stream.Position - streamStart == charEntriesOffset);
            for (int i = 0; i < charEntries.Length; i++)
            {
                stream.writeArray(charEntries[i]);
            }
        }

        private static void toEntries(byte[][,] charsSrc, UInt16[] offsets, byte[] widths, HeightEntry[] heights, byte[][] chars, out int charsLength)
        {
            //Convert 8-bit chars to 4-bit and figure out data for all entry lists (offsets, widths, heights and chars).
            int offset = 0;
            for (int i = 0; i < charsSrc.Length; i++)
            {
                byte[,] charSrcPixels = charsSrc[i]; //8-bit pixels.
                Rectangle charRect = getCharRect(charSrcPixels);
                int charStride = ((charRect.Width * 4) + 7) / 8;
                byte[] charPixels = new byte[charStride * charRect.Height]; //4-bit pixels.
                for (int y = charRect.Y, k = 0; y < charRect.Bottom; y++)
                {
                    for (int x = charRect.X; x < charRect.Right; x++, k++)
                    {
                        //Convert two 8-bit pixels into two 4-bit pixels stored in a byte.
                        //Set second 8-bit pixel to 0 if not available i.e. last pixel if uneven char width.
                        byte lb = charSrcPixels[y, x]; //Left pixel, low nibble.
                        x++;
                        byte hb = x < charRect.Right ? charSrcPixels[y, x] : (byte)0; //Right pixel, high nibble
                        charPixels[k] = (byte)(((hb << 4) & 0xF0) | (lb & 0x0F));
                    }
                }
                offsets[i] = (UInt16)offset;
                widths[i] = (byte)charRect.Width;
                chars[i] = charPixels;
                heights[i].yOffset = (byte)charRect.Y;
                heights[i].height = (byte)charRect.Height;

                offset += charPixels.Length;
            }
            charsLength = offset;
        }

        private static byte[,] toCharPixels(byte[] bmpPixels, int bmpStride, Rectangle rc)
        {
            //Read pixels specified by rectangle from 24-bit rgb source and convert to 8-bit indexed.
            byte[,] charPixels = new byte[rc.Height, rc.Width];
            int bmpInd = (rc.X * 3) + (rc.Y * bmpStride);
            for (int y = 0; y < rc.Height; y++, bmpInd += bmpStride)
            {
                for (int x = 0, k = bmpInd; x < rc.Width; x++, k += 3)
                {
                    int b = bmpPixels[k + 0];
                    byte p = 1;
                    if (b == 255)
                    {
                        int g = bmpPixels[k + 1];
                        int r = bmpPixels[k + 2];
                        if (g == 255 && r == 255) //White?
                        {
                            p = 0; //Transparent.
                        }
                        else if (g == 0 && r == 255) //Magenta?
                        {
                            p = 0xF0; //Char size marker in upper nibble. Only lower nibble is drawn.
                        }
                        else if (g == 255 && r == 0) //Cyan?
                        {
                            p = 2; //Drop shadow.
                        }
                    }
                    charPixels[y, x] = p;
                }
            }
            return charPixels;
        }

        private static void addCharShadow(byte[,] charPixels)
        {
            //Add drop shadow to pixels with index 1. Only pixels with index 0 are overwritten.
            int charHeight = charPixels.GetLength(0);
            int charWidth = charPixels.GetLength(1);
            for (int y = 0; y < charHeight; y++)
            {
                for (int x = 0; x < charWidth; x++)
                {
                    if (charPixels[y, x] == 1)
                    {
                        //Draw shadow pixel right, down and right-down i.e. scale pixel by 2.
                        for (int yy = y; yy <= y + 1 && yy < charHeight; yy++)
                        {
                            for (int xx = x; xx <= x + 1 && xx < charWidth; xx++)
                            {
                                if (charPixels[yy, xx] == 0)
                                {
                                    charPixels[yy, xx] = 2;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Rectangle getCharRect(byte[,] charPixels)
        {
            //Calculate rectangle that encloses all opaque pixels in char.
            int charHeight = charPixels.GetLength(0);
            int charWidth = charPixels.GetLength(1);
            int minX = charWidth;
            int maxX = -1;
            int minY = charHeight;
            int maxY = -1;
            for (int y = 0; y < charHeight; y++)
            {
                for (int x = 0; x < charWidth; x++)
                {
                    if (charPixels[y, x] != 0) //Not a transparent pixel?
                    {
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
            if (maxX >= 0) //Char has at least one opaque pixel.
            {
                return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            return new Rectangle(0, 0, charWidth, charHeight); //All pixels are transparent.
        }
        #endregion Write FNT-file

        public void debugSaveContent()
        {
            debugSaveContent(Program.DebugOutPath + "fnt\\");
        }

        public void debugSaveFontSheet(Palette6Bit palette)
        {
            string folderPath = Program.DebugOutPath + "fnt\\";
            Directory.CreateDirectory(folderPath);
            Frame image = new Frame(16 * mWidth, ((mCharCount + 15) / 16) * mHeight);
            Point pos = new Point(0, 0);
            int endX = 15 * mWidth;
            for (int i = 0; i < mCharCount; i++)
            {
                CharData charData = getCharData((byte)i);
                image.write(charData.Frame, pos.getOffset(charData.Offset));

                if (pos.X < endX)
                {
                    pos.X += mWidth;
                }
                else
                {
                    pos.X = 0;
                    pos.Y += mHeight;
                }
            }
            image.save(palette, folderPath + Name + ".png");
        }
    }
}
