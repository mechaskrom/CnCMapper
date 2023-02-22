using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CnCMapper.FileFormat
{
    //Images that are 320*200*8-bit in the PC version. Amiga version is a bit different and not implemented here.
    //http://www.shikadi.net/moddingwiki/Westwood_CPS_Format
    class FileCpsImageWw : FileBase
    {
        //Layout: header,[palette],image_data
        //header:
        //-file length UInt16: Length of file. If compression 0 or 4 this value is 2 bytes less than actual length.
        //-compression method UInt16: Compression method used on image data.
        //-uncompressed length UInt32: Length of image data after it is decrompessed. 320*200=64000 if PC version.
        //-palette length UInt16: Length of palette. Bytes between header and image data. 0 or 768 if PC version.
        //image_data:
        //-image UInt8[320*200]: Represents a simple 8-bit indexed 320*200 image.

        //Compression methods:
        //-0x0000: uncompressed
        //-0x0001: Westwood LZW-12
        //-0x0002: Westwood LZW-14
        //-0x0003: Westwood RLE
        //-0x0004: Westwood LCW (Format80)
        //Seems like Tiberian Dawn and Red Alert always uses method 4.

        private const int Width = 320;
        private const int Height = 200;
        private const int ImageDataLength = Width * Height;
        private const int HeaderLength = 10;

        private UInt16 mFileLength;
        private UInt16 mCompressionMethod;
        private UInt32 mUncompressedLength;
        private UInt16 mPaletteLength;

        private Palette6Bit mPalette; //Null if no palette is present in file.
        private Frame mFrame;

        public FileCpsImageWw()
        {
        }

        public FileCpsImageWw(string filePath)
            : base(filePath)
        {
        }

        public FileCpsImageWw(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "CPS";
        }

        protected override void parseInit(Stream stream)
        {
            //Header.
            checkFileLengthMin(HeaderLength);
            mFileLength = stream.readUInt16();
            mCompressionMethod = stream.readUInt16();
            mUncompressedLength = stream.readUInt32();
            mPaletteLength = stream.readUInt16();
            if (mCompressionMethod == 0 || mCompressionMethod == 4)
            {
                mFileLength += 2; //File length doesn't include first 2 bytes in header if method 0 or 4.
            }

            //Check values in header.
            checkHeaderFileLength(mFileLength);
            if (!(mCompressionMethod == 0 || mCompressionMethod == 4))
            {
                throwParseError(string.Format("Unsupported compression '{0}'!", mCompressionMethod));
            }
            if (mUncompressedLength != ImageDataLength)
            {
                throwParseError(string.Format("Unsupported uncompressed length '{0}'!", mUncompressedLength));
            }
            if (!(mPaletteLength == 0 || mPaletteLength == Palette6Bit.PaletteLength))
            {
                throwParseError(string.Format("Unsupported palette length '{0}'!", mPaletteLength));
            }

            //Palette.
            checkFileLengthMin(HeaderLength + mPaletteLength);
            if (mPaletteLength == Palette6Bit.PaletteLength)
            {
                mPalette = new Palette6Bit(stream);
            }
            else if (mPaletteLength > 0)
            {
                stream.Seek(mPaletteLength, SeekOrigin.Current);
            }

            //Image data.
            byte[] imageData = null;
            if (mCompressionMethod == 0)
            {
                imageData = stream.readArray(mUncompressedLength);
            }
            else if (mCompressionMethod == 4)
            {
                imageData = new byte[mUncompressedLength];
                Crypt.Format80.decode(imageData, stream);
            }

            mFrame = new Frame(Width, Height, imageData);
        }

        public Palette6Bit Palette
        {
            get { return mPalette; }
        }

        public Frame Frame
        {
            get { return mFrame; }
        }

        public void debugSaveImage()
        {
            debugSaveImage(mPalette);
        }

        public void debugSaveImage(Palette6Bit palette)
        {
            string folderPath = Program.DebugOutPath + "cps\\";
            Directory.CreateDirectory(folderPath);
            mFrame.save(palette, folderPath + Name + ".png");
        }
    }
}
