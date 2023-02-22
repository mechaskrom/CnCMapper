using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game
{
    interface ITilePos
    {
        TilePos TilePos { get; }
    }

    //Helper class for stuff that is stored per tile in maps.
    class TileTracker<T> where T : class, ITilePos
    {
        private readonly Size mSizeInTiles;
        private readonly T[] mTilesWithItem; //One item per tile.
        //Using something like a dictionary instead of an array to track items isn't faster in my tests.
        //To small arrays (<=16KB usually) to really affect speed?

        public TileTracker(Size sizeInTiles)
        {
            mSizeInTiles = sizeInTiles;
            mTilesWithItem = new T[sizeInTiles.Height * sizeInTiles.Width];
        }

        public Size SizeInTiles
        {
            get { return mSizeInTiles; }
        }

        public T this[TilePos tilePos]
        {
            get { return this[tilePos.X, tilePos.Y]; }
        }

        public T this[int tilePosX, int tilePosY]
        {
            get { return this[toTileNumber(tilePosX, tilePosY)]; }
        }

        private T this[int tileNumber]
        {
            get { return mTilesWithItem[tileNumber]; }
        }

        public T getAdjacentN(TilePos tilePos) //Get item directly north of tile position.
        {
            return getAdjacent(tilePos.X + 0, tilePos.Y - 1);
        }

        public T getAdjacentNE(TilePos tilePos) //Get item directly north-east of tile position.
        {
            return getAdjacent(tilePos.X + 1, tilePos.Y - 1);
        }

        public T getAdjacentE(TilePos tilePos) //Get item directly east of tile position.
        {
            return getAdjacent(tilePos.X + 1, tilePos.Y + 0);
        }

        public T getAdjacentSE(TilePos tilePos) //Get item directly south-east of tile position.
        {
            return getAdjacent(tilePos.X + 1, tilePos.Y + 1);
        }

        public T getAdjacentS(TilePos tilePos) //Get item directly south of tile position.
        {
            return getAdjacent(tilePos.X + 0, tilePos.Y + 1);
        }

        public T getAdjacentSW(TilePos tilePos) //Get item directly south-west of tile position.
        {
            return getAdjacent(tilePos.X - 1, tilePos.Y + 1);
        }

        public T getAdjacentW(TilePos tilePos) //Get item directly west of tile position.
        {
            return getAdjacent(tilePos.X - 1, tilePos.Y + 0);
        }

        public T getAdjacentNW(TilePos tilePos) //Get item directly north-west of tile position.
        {
            return getAdjacent(tilePos.X - 1, tilePos.Y - 1);
        }

        private T getAdjacent(int tilePosX, int tilePosY) //Returns null if out of bounds.
        {
            return isInside(tilePosX, tilePosY) ? this[tilePosX, tilePosY] : null;
        }

        public bool isInside(TilePos tilePos)
        {
            return isInside(tilePos.X, tilePos.Y);
        }

        public bool isInside(int tilePosX, int tilePosY)
        {
            return tilePosX >= 0 && tilePosX < mSizeInTiles.Width && tilePosY >= 0 && tilePosY < mSizeInTiles.Height;
        }

        public TilePos toTilePos(string tileNumber)
        {
            return toTilePos(int.Parse(tileNumber));
        }

        public TilePos toTilePos(int tileNumber)
        {
            return new TilePos(tileNumber % mSizeInTiles.Width, tileNumber / mSizeInTiles.Width);
        }

        private int toTileNumber(TilePos tilePos)
        {
            return toTileNumber(tilePos.X, tilePos.Y);
        }

        private int toTileNumber(int tilePosX, int tilePosY)
        {
            return tilePosX + (tilePosY * mSizeInTiles.Width);
        }

        public void addItem(T item) //Add item only if tile is empty.
        {
            int tileNumber = toTileNumber(item.TilePos);
            if (this[tileNumber] == null)
            {
                mTilesWithItem[tileNumber] = item;
            }
        }

        public void replaceItem(T item) //Overwrite whatever is in tile.
        {
            mTilesWithItem[toTileNumber(item.TilePos)] = item;
        }

        public IEnumerable<T> getTileItems()
        {
            for (int i = 0; i < mTilesWithItem.Length; i++)
            {
                T itemInTile = mTilesWithItem[i];
                if (itemInTile != null)
                {
                    yield return itemInTile;
                }
            }
        }
    }
}
