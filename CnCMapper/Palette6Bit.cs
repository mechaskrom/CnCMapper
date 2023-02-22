using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CnCMapper
{
    //Color palette with 18-bit RGB entries i.e. 6-bit color channels.
    class Palette6Bit
    {
        protected const int PaletteEntryCount = 256;
        public const int PaletteLength = PaletteEntryCount * 3;

        protected struct RgbEntry
        {
            public byte r; //6-bit.
            public byte g;
            public byte b;

            public RgbEntry(byte red, byte green, byte blue) //6-bit color channels.
            {
                r = red;
                g = green;
                b = blue;
            }

            public static RgbEntry createFrom8Bit(byte red, byte green, byte blue) //8-bit color channels.
            {
                return new RgbEntry((byte)to6bit(red), (byte)to6bit(green), (byte)to6bit(blue));
            }

            private static int to6bit(byte col8bit)
            {
                return (col8bit >> 2) & 0x3F;
            }

            public Color toColor()
            {
                //The 6 LSBs are used as the 6 MSBs i.e. value is left shifted 2 times.
                //Color stored as --VVVVVV is displayed as VVVVVV00. Checked in Tiberian Dawn and Red Alert.
                return Color.FromArgb(to8bit(r), to8bit(g), to8bit(b));
            }

            private static int to8bit(int col6bit)
            {
                return col6bit < 64 ? col6bit << 2 : 255;
            }

            public int difference(RgbEntry entry)
            {
                //Sum of squared diffs.
                int diffR = entry.r - r;
                int diffG = entry.g - g;
                int diffB = entry.b - b;
                return (diffR * diffR) + (diffG * diffG) + (diffB * diffB);
            }
        }

        protected struct HsvEntry
        {
            public byte h;
            public byte s;
            public byte v;

            public HsvEntry(int hue, int saturation, int value)
            {
                h = (byte)hue;
                s = (byte)saturation;
                v = (byte)value;
            }
        }

        protected readonly RgbEntry[] mRgbEntries = new RgbEntry[PaletteEntryCount];
        protected ColorPalette mColorPalette = null;

        protected Palette6Bit()
        {
        }

        public Palette6Bit(Stream stream)
        {
            //Colors.
            for (int i = 0; i < mRgbEntries.Length; i++)
            {
                mRgbEntries[i].r = stream.readUInt8();
                mRgbEntries[i].g = stream.readUInt8();
                mRgbEntries[i].b = stream.readUInt8();
            }
        }

        public Palette6Bit(Palette6Bit copyFrom)
        {
            copyFrom.mRgbEntries.CopyTo(mRgbEntries, 0);
        }

        public static implicit operator ColorPalette(Palette6Bit convFrom) //Implicit conversion to 8-bit color palette.
        {
            return convFrom.getColorPalette();
        }

        public ColorPalette getColorPalette()
        {
            if (mColorPalette == null)
            {
                //Can only(?) construct a palette from a bitmap.
                using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed)) //Temp bitmap.
                {
                    mColorPalette = bmp.Palette; //Creates a copy.
                }
                Color[] entries = mColorPalette.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i] = mRgbEntries[i].toColor();
                }
            }
            return mColorPalette;
        }

        public byte getHalfBlend(byte ind1, byte ind2)
        {
            //Returns palette index to closest match of a half color blend between parameters.
            RgbEntry entry1 = mRgbEntries[ind1];
            RgbEntry entry2 = mRgbEntries[ind2];
            entry1.r = (byte)((entry1.r + entry2.r) / 2);
            entry1.g = (byte)((entry1.g + entry2.g) / 2);
            entry1.b = (byte)((entry1.b + entry2.b) / 2);
            return findClosestMatch(entry1);
        }

        public byte findClosestMatch(Color color)
        {
            return findClosestMatch(color.R, color.G, color.B);
        }

        public byte findClosestMatch(byte r, byte g, byte b) //8-bit color channels.
        {
            return findClosestMatch(RgbEntry.createFrom8Bit(r, g, b));
        }

        private byte findClosestMatch(RgbEntry rgbIdeal)
        {
            //Check for closest match i.e. lowest sum of squared diffs. Don't include transparent index 0.
            int closestMatch = 0;
            int closestDiff = int.MaxValue;
            for (int i = 1; i < PaletteEntryCount; i++)
            {
                //Sum of squared diffs.
                int totDiff = mRgbEntries[i].difference(rgbIdeal);
                if (totDiff < closestDiff) //Better match?
                {
                    closestMatch = i;
                    closestDiff = totDiff;
                    if (totDiff == 0) //Perfect match?
                    {
                        break;
                    }
                }
            }
            return (byte)closestMatch;
        }

        public void debugSavePalette(string name)
        {
            string folderPath = Program.DebugOutPath + "pal\\";
            Directory.CreateDirectory(folderPath);
            Size tileSize = new Size(16, 16);
            using (Bitmap bmp = new Bitmap(tileSize.Width * 16, tileSize.Height * 16))
            using (Graphics gr = Graphics.FromImage(bmp))
            using (SolidBrush br = new SolidBrush(Color.White))
            {
                for (int y = 0, index = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++, index++)
                    {
                        br.Color = mRgbEntries[index].toColor();
                        gr.FillRectangle(br, x * tileSize.Width, y * tileSize.Height, tileSize.Width, tileSize.Height);
                    }
                }
                bmp.Save(folderPath + name + ".png");
            }
        }
    }
}
