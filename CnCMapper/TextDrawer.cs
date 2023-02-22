using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper
{
    class TextDrawer
    {
        private readonly FileFntFontWw mFileFnt;
        private readonly byte[] mRemap; //Palette index remap table (16 entries) for chars.
        private readonly int mSpacing; //Horizontal spacing between chars in pixels.

        //Cache text draw infos. Often the same string is drawn multiple times (e.g. sprite trigger or action).
        private readonly Dictionary<string, TextDrawInfo> mTextDrawInfos = new Dictionary<string, TextDrawInfo>();

        public TextDrawer(FileFntFontWw fileFnt, byte[] remap)
            : this(fileFnt, remap, 0)
        {
        }

        public TextDrawer(FileFntFontWw fileFnt, byte[] remap, int spacing)
        {
            mFileFnt = fileFnt;
            mRemap = remap;
            mSpacing = spacing;
        }

        public byte LineHeight
        {
            get { return mFileFnt.Height; }
        }

        public TextDrawInfo getTextDrawInfo(string chars)
        {
            TextDrawInfo textDi;
            if (!mTextDrawInfos.TryGetValue(chars, out textDi)) //Cached?
            {
                textDi = new TextDrawInfo(this, chars);
                mTextDrawInfos.Add(chars, textDi);
            }
            return textDi;
        }

        public void draw(string chars, Point dstPos, Frame dstFrame)
        {
            getTextDrawInfo(chars).draw(dstPos, dstFrame);
        }

        public class TextDrawInfo //Stores draw info for a string.
        {
            private readonly TextDrawer mTextDrawer;
            private readonly List<CharDrawInfo> mCharDrawInfos; //All chars in the string.
            private readonly Size mTextSize; //Total size in pixels of drawn text.

            public TextDrawInfo(TextDrawer textDrawer, string chars)
            {
                mTextDrawer = textDrawer;
                mCharDrawInfos = getCharDrawInfos(textDrawer.mFileFnt, textDrawer.mSpacing, chars, out mTextSize);
            }

            private static List<CharDrawInfo> getCharDrawInfos(FileFntFontWw fileFnt, int spacing, string chars, out Size textSize)
            {
                List<CharDrawInfo> charDrawInfos = new List<CharDrawInfo>();
                int maxLineWidth = 0; //Widest line in text.
                Point pos = new Point(0, 0);
                foreach (char c in chars)
                {
                    if (c == '\n') //New line?
                    {
                        maxLineWidth = Math.Max(maxLineWidth, pos.X - spacing); //Remove last spacing.
                        pos.X = 0;
                        pos.Y += fileFnt.Height;
                    }
                    else
                    {
                        CharDrawInfo charDi = new CharDrawInfo(fileFnt.getCharData((byte)c), pos);
                        charDrawInfos.Add(charDi);
                        pos.X += charDi.Frame.Width + spacing; //Variabel width.
                        //pos.X += fileFnt.Width + spacing; //Fixed width.
                    }
                }
                maxLineWidth = Math.Max(maxLineWidth, pos.X - spacing); //Remove last spacing.
                textSize = new Size(maxLineWidth, pos.Y + fileFnt.Height);
                return charDrawInfos;
            }

            public Size Size
            {
                get { return mTextSize; }
            }

            public int Width
            {
                get { return Size.Width; }
            }

            public int Height
            {
                get { return Size.Height; }
            }

            public void draw(Point dstPos, Frame dstFrame)
            {
                foreach (CharDrawInfo charDi in mCharDrawInfos)
                {
                    dstFrame.draw(charDi.Frame, charDi.Position.getOffset(dstPos), mTextDrawer.mRemap);
                }
            }

            private struct CharDrawInfo //Stores draw info for a char in a string.
            {
                private readonly Frame mFrame; //Char frame.
                private readonly Point mPosition; //Char draw position in pixels.

                public CharDrawInfo(FileFntFontWw.CharData charData, Point position)
                    : this(charData.Frame, position.getOffset(charData.Offset))
                {
                }

                public CharDrawInfo(Frame frame, Point position)
                {
                    mFrame = frame;
                    mPosition = position;
                }

                public Frame Frame
                {
                    get { return mFrame; }
                }

                public Point Position
                {
                    get { return mPosition; }
                }
            }
        }
    }
}
