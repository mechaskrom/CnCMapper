using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class SpriteTDRA : ITilePos
    {
        protected enum Dir8Way //Values correspond with frame index used by units and infantries in both Tiberian Dawn and Red Alert.
        {
            North = 0,
            Northwest = 1,
            West = 2,
            Southwest = 3,
            South = 4,
            Southeast = 5,
            East = 6,
            Northeast = 7,
        }

        protected enum Dir16Way //Values correspond with frame index used by ships in Red Alert.
        {
            North = 0,
            NorthNorthWest = 1,
            NorthWest = 2,
            WestNorthWest = 3,
            West = 4,
            WestSouthWest = 5,
            SouthWest = 6,
            SouthSouthWest = 7,
            South = 8,
            SouthSouthEast = 9,
            SouthEast = 10,
            EastSouthEast = 11,
            East = 12,
            EastNorthEast = 13,
            NorthEast = 14,
            NorthNorthEast = 15,
        }

        //Some commonly used values.
        protected const int FrameIndex0 = 0;
        protected const DrawMode DrawModeNormal = DrawMode.Normal;
        protected static readonly Point DrawOffsetNone = toDrawOffset(0, 0);
        protected static readonly Point PriOffsetNone = toPriOffset(0, 0);
        protected static readonly HouseTDRA HouseNull = null;
        protected const string ActionNull = null;
        protected const string TriggerNull = null;
        protected static readonly SpriteTDRA AddSpriteNull = null;
        private static readonly byte[] ColorRemapNone = new byte[]
        {
            0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F,
            0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,
            0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2A,0x2B,0x2C,0x2D,0x2E,0x2F,
            0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x3B,0x3C,0x3D,0x3E,0x3F,
            0x40,0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x4A,0x4B,0x4C,0x4D,0x4E,0x4F,
            0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x5B,0x5C,0x5D,0x5E,0x5F,
            0x60,0x61,0x62,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6A,0x6B,0x6C,0x6D,0x6E,0x6F,
            0x70,0x71,0x72,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7A,0x7B,0x7C,0x7D,0x7E,0x7F,
            0x80,0x81,0x82,0x83,0x84,0x85,0x86,0x87,0x88,0x89,0x8A,0x8B,0x8C,0x8D,0x8E,0x8F,
            0x90,0x91,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9A,0x9B,0x9C,0x9D,0x9E,0x9F,
            0xA0,0xA1,0xA2,0xA3,0xA4,0xA5,0xA6,0xA7,0xA8,0xA9,0xAA,0xAB,0xAC,0xAD,0xAE,0xAF,
            0xB0,0xB1,0xB2,0xB3,0xB4,0xB5,0xB6,0xB7,0xB8,0xB9,0xBA,0xBB,0xBC,0xBD,0xBE,0xBF,
            0xC0,0xC1,0xC2,0xC3,0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xCB,0xCC,0xCD,0xCE,0xCF,
            0xD0,0xD1,0xD2,0xD3,0xD4,0xD5,0xD6,0xD7,0xD8,0xD9,0xDA,0xDB,0xDC,0xDD,0xDE,0xDF,
            0xE0,0xE1,0xE2,0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,0xE9,0xEA,0xEB,0xEC,0xED,0xEE,0xEF,
            0xF0,0xF1,0xF2,0xF3,0xF4,0xF5,0xF6,0xF7,0xF8,0xF9,0xFA,0xFB,0xFC,0xFD,0xFE,0xFF,
        };

        protected enum DrawMode //Sprite drawing modes.
        {
            Normal,
            Dithered,
            AircraftShadow,
            Invisible,
        }

        //4x4 (16 bits) dithered draw patterns. 1 = don't draw pixel.
        private const UInt16 DitherPattern30 = 0x5E5B; //0101,1110,0101,1011
        private const UInt16 DitherPattern40 = 0x5A5B; //0101,1010,0101,1011
        private const UInt16 DitherPattern50 = 0x5A5A; //0101,1010,0101,1010
        private const UInt16 DitherPattern60 = 0x4A1A; //0100,1010,0001,1010
        private const UInt16 DitherPattern70 = 0x2828; //0010,1000,0010,1000
        private const UInt16 DitherPatternSq = 0x5050; //0101,0000,0101,0000 //Square dotted.
        private const UInt16 DitherPatternDi = 0x04E4; //0000,0100,1110,0100 //Diamonds.
        private const UInt16 DitherPatternPl = 0x318C; //0011,0001,1000,1100 //Plates.
        private const UInt16 DitherPattern = DitherPatternPl;

        private const long PriOffsetPerPlaneTD = TD.MapTD.WidthInTiles * 256L * TD.MapTD.HeightInTiles * 256L; //Leptons per map.
        private const long PriOffsetPerPlaneRA = RA.MapRA.WidthInTiles * 256L * RA.MapRA.HeightInTiles * 256L; //Leptons per map.

        //Different draw priority planes for sprite types.
        protected const int PriPlaneHighCrate = 2; //Special to expose crate sprites if configured so.
        protected const int PriPlaneHigh = 1; //Special to expose often hidden sprites if configured so.
        protected const int PriPlaneDefault = 0; //Normal plane/layer were most sprites are drawn.
        protected const int PriPlaneFlag = -1; //Flags lower than normal.
        protected const int PriPlaneOverlay = -2; //Overlays lower than flags. It's possible walls have higher prio?
        protected const int PriPlaneSmudge = -3; //Smudges lower than overlays.

        private readonly string mId; //Usually, but not always, same as SHP-file id.
        private readonly TilePos mTilePos; //Position (in tiles) in map.
        protected FileShpSpriteSetTDRA mFileShp; //SHP-file.
        protected int mFrameIndex; //Frame index in SHP-file.
        protected DrawMode mDrawMode; //Mode to draw sprite in.
        protected byte[] mColorRemap; //Color remap table. 256 entries, one for every 8-bit palette index.
        protected Point mDrawOffset; //Offset (in pixels) from tile position to where to draw sprite.
        protected Point mPriOffset; //Offset (in leptons) from tile position to decide sprite draw order.
        protected int mPriPlane; //Draw priority plane (sprite layer). Used for coarse setting of draw priority.
        protected HouseTDRA mHouse; //House field in INI-key if present.
        protected string mAction; //Action field in INI-key if present.
        protected string mTrigger; //Trigger field in INI-key if present.
        protected SpriteTDRA mAddSprite; //Additional sprite to draw over this. Mostly used for turrets.

        //Leptons are C&C's more precise positioning value. 256 per tile width and height e.g. 128,128=center of a tile.
        //Used for fine setting of draw priority. All priority/lepton information is taken from the CnC Remastered Collection source code.

        protected SpriteTDRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
        {
            mId = id;
            mTilePos = tilePos;
            mFileShp = fileShp;
            mFrameIndex = FrameIndex0;
            mDrawMode = DrawModeNormal;
            mColorRemap = ColorRemapNone;
            mDrawOffset = DrawOffsetNone;
            mPriOffset = PriOffsetNone;
            mPriPlane = PriPlaneDefault;
            mHouse = HouseNull;
            mAction = ActionNull;
            mTrigger = TriggerNull;
            mAddSprite = AddSpriteNull;
        }

        public string Id
        {
            get { return mId; }
        }

        public TilePos TilePos
        {
            get { return mTilePos; }
        }

        public FileShpSpriteSetTDRA FileShp
        {
            get { return mFileShp; }
        }

        public int FrameIndex
        {
            get { return mFrameIndex; }
        }

        public byte[] ColorRemap
        {
            get { return mColorRemap; }
        }

        public HouseTDRA House
        {
            get { return mHouse; }
        }

        public string Action
        {
            get { return mAction; }
        }

        public string Trigger
        {
            get { return mTrigger; }
        }

        protected int FrameCount
        {
            get { return mFileShp.FrameCount; }
        }

        protected int Width
        {
            get { return mFileShp.Width; }
        }

        protected int Height
        {
            get { return mFileShp.Height; }
        }

        protected static int toHealth(string health)
        {
            int h = int.Parse(health);
            return Math.Max(0, Math.Min(h, 256)); //Health is always 1-256? 0 = dead/removed!
            //Values higher than 256 doesn't seem to affect anything. Clamped to 256 by the game?
        }

        protected static byte toDirection(string direction)
        {
            return (byte)(int.Parse(direction) & 0xFF); //Direction is always a byte (0-255).
        }

        protected static Dir16Way toDir16Way(Dir8Way dir8Way)
        {
            return (Dir16Way)((int)dir8Way * 2);
        }

        protected static Dir8Way toDir8WayInverted(Dir8Way dir8Way)
        {
            return (Dir8Way)(((int)dir8Way + (int)Dir8Way.South) & 0x07); //"Add" south direction to invert it.
        }

        protected static Dir16Way toDir16WayInverted(Dir16Way dir16Way)
        {
            return (Dir16Way)(((int)dir16Way + (int)Dir16Way.South) & 0x0F); //"Add" south direction to invert it.
        }

        protected static Point toDrawOffset(int x, int y) //Mostly to distinguish draw offset uses.
        {
            return new Point(x, y);
        }

        protected static Point toPriOffset(int x, int y) //Mostly to distinguish priority offset uses.
        {
            return new Point(x, y);
        }

        protected static Point getDrawOffsetCenter(SpriteTDRA spr)
        {
            return getDrawOffsetCenter(spr.mFileShp);
        }

        protected static Point getDrawOffsetCenter(FileShpSpriteSetTDRA fileShp)
        {
            //Get draw offset for a sprite positioned in the center of a tile.
            //This formula is correct for all sprites checked in game so far.
            //Same in both Tiberian Dawn and Red Alert.
            return toDrawOffset((MapTDRA.TileWidth / 2) - (fileShp.Width / 2), (MapTDRA.TileHeight / 2) - (fileShp.Height / 2));
        }

        protected void addDrawOffset(Point drawOffset)
        {
            //Add draw offset to all sprites in the chain.
            mDrawOffset.Offset(drawOffset);
            if (mAddSprite != null)
            {
                mAddSprite.addDrawOffset(drawOffset);
            }
        }

        protected void increaseFrameIndex()
        {
            mFrameIndex = Math.Min(mFrameIndex + 1, FrameCount - 1);
        }

        protected Point getPriorityPos()
        {
            return new Point((mTilePos.X * 256) + mPriOffset.X, (mTilePos.Y * 256) + mPriOffset.Y);
        }

        protected void setPriorityJustOver(SpriteTDRA target)
        {
            setPriorityJustAs(target, +1);
        }

        protected void setPriorityJustUnder(SpriteTDRA target)
        {
            setPriorityJustAs(target, -1);
        }

        private void setPriorityJustAs(SpriteTDRA target, int dx)
        {
            //Set draw priority same as target or just under/over it.
            Point locTarget = target.getPriorityPos();
            Point locThis = getPriorityPos();
            mPriOffset.Offset(locTarget.X - locThis.X + dx, locTarget.Y - locThis.Y);
            mPriPlane = target.mPriPlane;
        }

        public static int compareDrawOrderTD(SpriteTDRA x, SpriteTDRA y)
        {
            return x.getDrawPriorityTD().CompareTo(y.getDrawPriorityTD());
            //TODO: Check order of different sprite id with same prio? Ordered by INI-file listing?
            //Must be very rare in normal conditions though so probably not worth checking.
        }

        private long getDrawPriorityTD()
        {
            //Draw priority value can get pretty large so 64-bit long math is needed.
            Point priPos = getPriorityPos();
            return priPos.X + (priPos.Y * TD.MapTD.WidthInTiles * 256L) + (mPriPlane * PriOffsetPerPlaneTD);
        }

        public static int compareDrawOrderRA(SpriteTDRA x, SpriteTDRA y)
        {
            return x.getDrawPriorityRA().CompareTo(y.getDrawPriorityRA());
            //TODO: Check order of different sprite id with same prio? Ordered by INI-file listing?
            //Must be very rare in normal conditions though so probably not worth checking.
        }

        private long getDrawPriorityRA()
        {
            //Draw priority value can get pretty large so 64-bit long math is needed.
            Point priPos = getPriorityPos();
            return priPos.X + (priPos.Y * RA.MapRA.WidthInTiles * 256L) + (mPriPlane * PriOffsetPerPlaneRA);
        }

        protected void setAddSpriteDraw(SpriteTDRA addSpr)
        {
            //Set some values in an additional sprite so it's drawn same as the owner.
            addSpr.mDrawMode = mDrawMode;
            addSpr.mColorRemap = mColorRemap;
            addSpr.mHouse = mHouse; //Not needed?
        }

        protected void setAddSpriteLast(SpriteTDRA addSpr)
        {
            //Set an additional sprite at the end of the chain (after all additional sprites).
            if (mAddSprite == null)
            {
                mAddSprite = addSpr;
            }
            else
            {
                mAddSprite.setAddSpriteLast(addSpr);
            }
        }

        protected static Point movePoint(Dir8Way direction, int distance)
        {
            return movePoint(toDir16Way(direction), distance, false);
        }

        protected static Point movePoint(Dir16Way direction, int distance, bool isNormal)
        {
            //Adoption of source code to calculate moving point.
            //Values are from cosine and sine look-up-tables in the source code.
            //Normal move only used by ship turrets in Red Alert?
            //See "Move_Point()" and "Normal_Move_Point()" (Red Alert only) in "COORD.CPP".
            switch (direction)
            {
                case Dir16Way.North: /***********/ return movePoint(0, 127, distance, isNormal);
                case Dir16Way.NorthNorthWest: /**/ return movePoint(-48, 117, distance, isNormal);
                case Dir16Way.NorthWest: /*******/ return movePoint(-89, 89, distance, isNormal);
                case Dir16Way.WestNorthWest: /***/ return movePoint(-117, 48, distance, isNormal);
                case Dir16Way.West: /************/ return movePoint(-126, 0, distance, isNormal);
                case Dir16Way.WestSouthWest: /***/ return movePoint(-117, -48, distance, isNormal);
                case Dir16Way.SouthWest: /*******/ return movePoint(-89, -89, distance, isNormal);
                case Dir16Way.SouthSouthWest: /**/ return movePoint(-48, -117, distance, isNormal);
                case Dir16Way.South: /***********/ return movePoint(0, -126, distance, isNormal);
                case Dir16Way.SouthSouthEast: /**/ return movePoint(48, -117, distance, isNormal);
                case Dir16Way.SouthEast: /*******/ return movePoint(89, -89, distance, isNormal);
                case Dir16Way.EastSouthEast: /***/ return movePoint(117, -48, distance, isNormal);
                case Dir16Way.East: /************/ return movePoint(127, 0, distance, isNormal);
                case Dir16Way.EastNorthEast: /***/ return movePoint(117, 48, distance, isNormal);
                case Dir16Way.NorthEast: /*******/ return movePoint(89, 89, distance, isNormal);
                case Dir16Way.NorthNorthEast: /**/ return movePoint(48, 117, distance, isNormal);
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        private static Point movePoint(int cosVal, int sinVal, int distance, bool isNormal)
        {
            System.Diagnostics.Debug.Assert(
                (cosVal >= -128 && cosVal <= 127) &&
                (sinVal >= -128 && sinVal <= 127) &&
                (distance >= 0 && distance <= 32767), "Argument outside supported range of values!");
            //Source code uses some assembler code to calculate offset and it's pretty hard to
            //understand fully. I guess it does some special handling of signed values?. I tested this
            //simpler method and it produces the same values for the asserted ranges above at least.
            //Should be enough for what it's currently used for.
            return new Point(
                movePointCoord(cosVal, distance),
                -movePointCoord(isNormal ? sinVal / 2 : sinVal, distance));
        }

        private static int movePointCoord(int val, int distance)
        {
            return (short)((val * distance) >> 7);

            //Assembler version does something like this I think.
            //16-bit signed multiply of value and distance and then convert the 32-bit result to 16-bit?
            //int mul = val * distance;
            //ushort mulLo = (ushort)(mul & 0x0000FFFF);
            //ushort mulHi = (ushort)((mul >> 16) & 0x0000FFFF);
            //mulHi = (ushort)((mulHi << 1) | (mulLo >> 15));
            //mulLo >>= 7;
            //return (short)(((mulHi << 8) & 0xFF00) | (mulLo & 0x00FF));
        }

        public Size getSizeInTiles()
        {
            //Get sprite's size in tiles (rounded up to whole tiles).
            return new Size(
                (Width + MapTDRA.TileWidth - 1) / MapTDRA.TileWidth,
                (Height + MapTDRA.TileHeight - 1) / MapTDRA.TileHeight);
        }

        protected Point getDrawPos()
        {
            return new Point(
                mTilePos.X * MapTDRA.TileWidth + mDrawOffset.X,
                mTilePos.Y * MapTDRA.TileHeight + mDrawOffset.Y);
        }

        protected Rectangle getBoundingBox()
        {
            if (mAddSprite == null)
            {
                return mFileShp.getBoundingBox(mFrameIndex);
            }
            //If sprite has additional sprites we need to include draw offsets
            //when calculating bounding box for all sprites.
            Rectangle box = getDrawBox();
            Point pos = getDrawPos();
            box.Offset(-pos.X, -pos.Y); //Remove draw offset.
            return box;
        }

        public Rectangle getDrawBox()
        {
            //Calculate bounding box of opaque sprite pixels for all sprites in the chain.
            Rectangle drawBox = mFileShp.getBoundingBox(mFrameIndex);
            drawBox.Offset(getDrawPos()); //Add draw offset.
            if (mAddSprite != null)
            {
                drawBox = Rectangle.Union(drawBox, mAddSprite.getDrawBox());
            }
            return drawBox;
        }

        public virtual void draw(TheaterTDRA theater, IndexedImage image)
        {
            drawInner(theater, image);
        }

        private void drawInner(TheaterTDRA theater, IndexedImage image)
        {
            Frame sprFrame = mFileShp.getFrame(mFrameIndex);
            if (mDrawMode != DrawMode.Invisible && !sprFrame.IsEmpty) //Not an invisible sprite?
            {
                bool doDithering = mDrawMode == DrawMode.Dithered;
                byte[] sprPixels = sprFrame.Pixels;
                byte[] imgPixels = image.Pixels;
                byte[] colorRemap = mColorRemap;
                byte[][] colorFilter = mDrawMode != DrawMode.AircraftShadow
                    ? theater.UnitShadowFilter
                    : theater.getAircraftShadowFilter();

                //Often sprites have many transparent pixels so it's generally slightly faster to calculate
                //opaque pixels bounding box (cache it) and only loop y/x inside it when drawing.
                Rectangle box = mFileShp.getBoundingBox(mFrameIndex);
                Point drawOffset = getDrawPos();
                box.Offset(drawOffset); //Add draw offset.
                box.Intersect(image.Clip); //Make sure sprite's draw box is inside image.
                Point pos = box.Location; //Get position to draw sprite at in image.
                box.Offset(-drawOffset.X, -drawOffset.Y); //Remove draw offset.
                //Sprites are clipped inside image and don't wrap around if drawn near the edge. Checked in game.

                //Draw part of sprite specified by "box" at "pos" in image.
                for (int y = box.Y; y < box.Bottom; y++, pos.Y++)
                {
                    int kSpr = y * Width;
                    int kImg = image.getOffset(pos);
                    for (int x = box.X; x < box.Right; x++, kImg++)
                    {
                        //if (doDithering && (y % 2 != x % 2)) //50% is simple unlike other patterns.
                        if (doDithering && ((DitherPattern & (0x8000 >> ((x & 3) + ((y & 3) * 4)))) != 0))
                        {
                            continue;
                        }

                        byte p = sprPixels[kSpr + x];
                        if (p == 0) //Transparent pixel?
                        {
                            continue;
                        }

                        p = colorRemap[p]; //Do color remap.

                        byte f = colorFilter[0][p]; //Apply a filter (i.e. unit or aircraft shadow)?
                        if (f != 0xFF) //Pixel index is filtered? See MRF-file implementation for how this works.
                        {
                            p = colorFilter[f + 1][imgPixels[kImg]]; //Remap underlying pixel index.
                        }

                        imgPixels[kImg] = p; //Write final pixel index to image.
                    }
                }
            }

            if (mAddSprite != null)
            {
                mAddSprite.drawInner(theater, image);
            }
        }

        public virtual void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            //Draw nothing as default.
        }
    }
}
