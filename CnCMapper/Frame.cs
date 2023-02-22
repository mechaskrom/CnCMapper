using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CnCMapper
{
    //Simple palette indexed 8-bit image. Essentially a 1D byte array with a 2D size.
    class Frame
    {
        private readonly Size mSize;
        private readonly byte[] mPixels;
        private readonly bool mIsEmpty; //Frame is empty/dummy?

        public Frame(int width, int height)
            : this(width, height, new byte[width * height], false)
        {
        }

        public Frame(int width, int height, byte[] pixels)
            : this(width, height, pixels, false)
        {
        }

        protected Frame(int width, int height, bool isEmpty)
            : this(width, height, new byte[width * height], isEmpty)
        {
        }

        protected Frame(int width, int height, byte[] pixels, bool isEmpty)
        {
            if ((width * height) != pixels.Length)
            {
                throw new ArgumentException("Width*height doesn't match array length!");
            }
            mSize = new Size(width, height);
            mPixels = pixels;
            mIsEmpty = isEmpty;
        }

        public Frame(Frame srcFrame) //Makes a deep copy.
            : this(srcFrame.Width, srcFrame.Height, srcFrame.IsEmpty)
        {
            Buffer.BlockCopy(srcFrame.Pixels, 0, Pixels, 0, srcFrame.Pixels.Length);
        }

        public Frame(Frame srcFrame, Rectangle srcRect)
            : this(srcRect.Width, srcRect.Height, srcFrame.IsEmpty)
        {
            write(srcFrame, srcRect, new Point(0, 0));
        }

        public static Frame createEmpty(int width, int height)
        {
            return new Frame(width, height, true);
        }

        public static Frame createPalette()
        {
            //Create a 16x16 frame with the full palette.
            Frame frame = new Frame(16, 16);
            for (int i = 0; i < frame.Length; i++)
            {
                frame[i] = (byte)i;
            }
            return frame;
        }

        public static Frame createSolid(byte paletteIndex, Size size)
        {
            Frame frame = new Frame(size.Width, size.Height);
            frame.clear(paletteIndex);
            return frame;
        }

        public static Frame createSolid(byte paletteIndex, byte edgePaletteIndex, Size size)
        {
            //Create a solid colored frame with an edge around it.
            Frame frame = createSolid(paletteIndex, size);
            frame.drawRect(frame.getRect(), edgePaletteIndex);
            return frame;
        }

        public Size Size
        {
            get { return mSize; }
        }

        public int Width
        {
            get { return mSize.Width; }
        }

        public int Height
        {
            get { return mSize.Height; }
        }

        public int Stride //Bytes/pixels per row. Usually same as width.
        {
            get { return Width; }
        }

        public byte[] Pixels
        {
            get { return mPixels; }
        }

        public bool IsEmpty
        {
            get { return mIsEmpty; }
        }

        public int Length
        {
            get { return mPixels.Length; }
        }

        public byte this[int index]
        {
            get { return mPixels[index]; }
            set { mPixels[index] = value; }
        }

        public byte this[Point pos]
        {
            get { return mPixels[getOffset(pos)]; }
            set { mPixels[getOffset(pos)] = value; }
        }

        public byte this[int x, int y]
        {
            get { return mPixels[getOffset(x, y)]; }
            set { mPixels[getOffset(x, y)] = value; }
        }

        public int getOffset(Point pos)
        {
            return getOffset(pos.X, pos.Y);
        }

        public int getOffset(int x, int y)
        {
            return x + (y * Stride);
        }

        public Rectangle getRect()
        {
            return new Rectangle(0, 0, Width, Height);
        }

        protected virtual Rectangle getClip() //Region to clip drawing inside.
        {
            return getRect();
        }

        public Rectangle getBoundingBox()
        {
            //Get rectangle that encloses all opaque pixels (not 0) in the frame.
            byte[] pixels = Pixels;
            int width = Width;
            int height = Height;
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int y = 0, k = 0; y < height; y++)
            {
                bool isOpaqueRow = false;
                for (int x = 0; x < width; x++, k++)
                {
                    if (pixels[k] != 0) //Not a transparent pixel?
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        isOpaqueRow = true;
                    }
                }
                if (isOpaqueRow) //Row has opaque pixel(s).
                {
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            if (minX < int.MaxValue) //At least one opaque pixel in the frame?
            {
                return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            return new Rectangle(0, 0, width, height); ; //All pixels in the frame are transparent.
        }

        public Frame flip(bool flipH, bool flipV) //Returns a copy with any flipping applied.
        {
            return flip(this, flipH, flipV);
        }

        private static Frame flip(Frame srcFrame, bool flipH, bool flipV)
        {
            if (!flipH && !flipV) //No flipping?
            {
                return new Frame(srcFrame);
            }

            int startY, addY;
            if (flipV) //Copy frame lines in reverse?
            {
                startY = srcFrame.Height - 1; //Go to end of frame.
                addY = -1;
            }
            else
            {
                startY = 0;
                addY = +1;
            }

            Frame dstFrame = new Frame(srcFrame.Width, srcFrame.Height);
            for (int srcY = 0, dstY = startY; srcY < srcFrame.Height; srcY++, dstY += addY)
            {
                int kSrc = srcY * srcFrame.Stride;
                int kDst = dstY * dstFrame.Stride;
                int kDstAdd = +1;
                if (flipH) //Copy line in reverse?
                {
                    kDst += dstFrame.Stride - 1; //Go to end of line.
                    kDstAdd = -1;
                }
                for (int srcX = 0; srcX < srcFrame.Width; srcX++, kSrc++, kDst += kDstAdd)
                {
                    dstFrame.mPixels[kDst] = srcFrame.mPixels[kSrc];
                }
            }
            return dstFrame;
        }

        public void clear(byte value)
        {
            mPixels.clearBytes(value);
        }

        public void write(Frame srcFrame) //Write pixels from frame (src) to this (dst).
        {
            write(srcFrame, new Point(0, 0));
        }

        public void write(Frame srcFrame, Point dstPos)
        {
            write(srcFrame, srcFrame.getRect(), dstPos);
        }

        public void write(Frame srcFrame, Rectangle srcRect, Point dstPos)
        {
            write(srcFrame, srcRect, this, dstPos);
        }

        private static void write(Frame srcFrame, Rectangle srcRect, Frame dstFrame, Point dstPos)
        {
            //Write will just copy pixels over to another frame.
            MiscExt.copyBytes(srcFrame.Pixels, srcFrame.Stride, srcRect, dstFrame.Pixels, dstFrame.Stride, dstPos);
        }

        public void draw(Frame srcFrame) //Draw pixels from frame (src) to this (dst).
        {
            draw(srcFrame, new Point(0, 0));
        }

        public void draw(Frame srcFrame, Point dstPos)
        {
            draw(srcFrame, dstPos, null);
        }

        public void draw(Frame srcFrame, Point dstPos, byte[] remap)
        {
            draw(srcFrame, dstPos, remap, null);
        }

        public void draw(Frame srcFrame, Point dstPos, byte[] remap, byte[] filter)
        {
            draw(srcFrame, srcFrame.getRect(), dstPos, remap, filter);
        }

        public void draw(Frame srcFrame, Rectangle srcRect, Point dstPos)
        {
            draw(srcFrame, srcRect, dstPos, null);
        }

        public void draw(Frame srcFrame, Rectangle srcRect, Point dstPos, byte[] remap)
        {
            draw(srcFrame, srcRect, dstPos, remap, null);
        }

        public void draw(Frame srcFrame, Rectangle srcRect, Point dstPos, byte[] remap, byte[] filter)
        {
            draw(srcFrame, srcRect, this, this.getClip(), dstPos, remap, filter);
        }

        private static void draw(Frame srcFrame, Rectangle srcRect, Frame dstFrame, Rectangle dstClip, Point dstPos, byte[] remap, byte[] filter)
        {
            //Draw is similar to write (see above), but will skip transparent pixels (index 0),
            //drawn region can be clipped and pixels can be remapped and filtered.
            MiscExt.drawPixels(srcFrame.Pixels, srcFrame.Stride, srcRect, dstFrame.Pixels, dstFrame.Stride, dstClip, dstPos, remap, filter);
        }

        public void drawRect(Rectangle dstRect, byte value)
        {
            Pixels.drawRect(Stride, getClip(), dstRect, value);
        }

        public void drawRectCorners(Rectangle dstRect, int cornerLength, byte value)
        {
            Pixels.drawRectCorners(Stride, getClip(), dstRect, cornerLength, value);
        }

        public void drawRectCorners(Rectangle dstRect, int cornerLength, byte value,
            bool doTop, bool doBot, bool doLef, bool doRig)
        {
            Pixels.drawRectCorners(Stride, getClip(), dstRect, cornerLength, value, doTop, doBot, doLef, doRig);
        }

        public void drawRectFilled(Rectangle dstRect, byte value)
        {
            Pixels.drawRectFilled(Stride, getClip(), dstRect, value);
        }

        public void drawText(TextDrawer td, string chars, Point dstPos)
        {
            td.getTextDrawInfo(chars).draw(dstPos, this);
        }

        public void save(ColorPalette palette, string filePath)
        {
            save(getRect(), palette, filePath);
        }

        public void save(Rectangle srcRect, ColorPalette palette, string filePath)
        {
            Pixels.savePixels(Stride, srcRect, palette, filePath);
        }

        public static void debugSaveFramesSheet(Frame[] frames, ColorPalette palette,
            int maxPerRow, byte backgroundIndex, string folderPath, string sheetName)
        {
            //This saves all frames into one image file which is faster to execute and then
            //browse through afterwards than saving one frame per image file.
            int frameCount = frames.Length;
            if (frameCount < 1)
            {
                throw new ArgumentException("Frame count is less than '1'!");
            }
            Directory.CreateDirectory(folderPath);

            //Figure out max size of frames.
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (Frame frame in frames)
            {
                maxWidth = Math.Max(maxWidth, frame.Width);
                maxHeight = Math.Max(maxHeight, frame.Height);
            }

            //Figure out layout of sheet.
            int colCount = Math.Min(maxPerRow, frameCount); //Max 8 per row.
            int rowCount = (frameCount + colCount - 1) / colCount;
            //int colCount = Math.Min(maxPerRow, (int)(Math.Sqrt(frameCount) + 0.5)); //Square. Keep row and column count close.
            //int rowCount = (frameCount + colCount - 1) / colCount;
            int spacing = 1;
            Frame image = new Frame(
                (colCount * (maxWidth + spacing)) + spacing,
                (rowCount * (maxHeight + spacing)) + spacing);

            //Set background color first.
            image.clear(backgroundIndex);

            //Write frames over background color.
            Point pos = new Point(spacing, spacing);
            for (int y = 0, i = 0; y < rowCount; y++, pos.Y += maxHeight + spacing)
            {
                pos.X = spacing;
                for (int x = 0; x < colCount && i < frameCount; x++, i++, pos.X += maxWidth + spacing)
                {
                    Frame frame = frames[i];
                    if (frame.Width * frame.Height > 0)
                    {
                        image.write(frame, pos);
                    }
                }
            }
            string filePath = folderPath + sheetName + string.Format(" ({0}x{1},#{2}).png", maxWidth, maxHeight, frameCount);
            image.save(palette, filePath);
        }
    }
}
