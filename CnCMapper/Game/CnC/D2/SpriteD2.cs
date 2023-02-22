using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.D2
{
    abstract class SpriteD2 : ITilePos
    {
        protected const string ActionNull = null;

        private const long PriOffsetPerPlane = MapD2.WidthInTiles * 256L * MapD2.HeightInTiles * 256L; //Leptons per map.
        //Leptons are C&C's more precise positioning value. 256 per tile width and height e.g. 128,128=center of a tile.
        //I wouldn't be surprised if Dune 2 also used leptons.

        //Different draw priority planes for sprite types.
        //Units are always drawn over structures. Checked in game.
        protected const int PriPlaneAircraft = 1; //Aircrafts higher than normal.
        protected const int PriPlaneDefault = 0; //Normal plane/layer were most sprites are drawn.
        protected const int PriPlaneBuilding = -1; //Buildings lower than normal.
        protected const int PriPlaneConcrete = -2; //Concrete (walls and slabs) lower than buildings?

        private readonly string mId; //Id field in INI-key.
        private readonly TilePos mTilePos; //Position (in tiles) in map.
        protected int mPriPlane; //Draw priority plane (sprite layer). Used for coarse setting of draw priority.
        private readonly HouseD2 mHouse; //House field in INI-key.
        private readonly string mAction; //Action field in INI-key if present.

        protected SpriteD2(string id, TilePos tilePos, int priPlane, HouseD2 house, string action)
        {
            mId = id;
            mTilePos = tilePos;
            mPriPlane = priPlane;
            mHouse = house;
            mAction = action;
        }

        public string Id
        {
            get { return mId; }
        }

        public TilePos TilePos
        {
            get { return mTilePos; }
        }

        public HouseD2 House
        {
            get { return mHouse; }
        }

        public string Action
        {
            get { return mAction; }
        }

        protected static int toHealth(string health)
        {
            int h = int.Parse(health);
            return Math.Max(0, Math.Min(h, 256)); //Health is always 1-256?
            //Values higher than 256 doesn't seem to affect anything. Clamped to 256 by the game?
            //Health field for structures does nothing? Only works for units?
            //Structures always seem to have full health regardless of value.
        }

        protected static Point toDrawOffset(int x, int y) //Mostly to distinguish draw offset uses.
        {
            return new Point(x, y);
        }

        protected Point getDrawPos()
        {
            return getDrawPos(mTilePos);
        }

        protected static Point getDrawPos(TilePos tilePos)
        {
            return new Point(
                tilePos.X * MapD2.TileWidth,
                tilePos.Y * MapD2.TileHeight);
        }

        public static int compareDrawOrder(SpriteD2 x, SpriteD2 y)
        {
            return x.getDrawPriority().CompareTo(y.getDrawPriority());
        }

        private long getDrawPriority()
        {
            //Draw priority value can get pretty large so 64-bit long math is needed.
            Point priPos = new Point(mTilePos.X * 256, mTilePos.Y * 256);
            return priPos.X + (priPos.Y * MapD2.WidthInTiles * 256L) + (mPriPlane * PriOffsetPerPlane);
        }

        public abstract Rectangle getDrawBox(TheaterD2 theater); //Calculate bounding box of opaque sprite pixels for sprite.

        public abstract void draw(TheaterD2 theater, IndexedImage image);

        public virtual void drawRadar(int scale, MapD2 map, IndexedImage image)
        {
            //Draw nothing as default.
        }
    }
}
