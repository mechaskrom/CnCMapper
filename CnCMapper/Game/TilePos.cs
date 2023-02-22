using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game
{
    struct TilePos
    {
        private Point mLocation;

        public TilePos(int x, int y)
        {
            mLocation = new Point(x, y);
        }

        public int X
        {
            get { return mLocation.X; }
            set { mLocation.X = value; }
        }

        public int Y
        {
            get { return mLocation.Y; }
            set { mLocation.Y = value; }
        }

        public Point Location
        {
            get { return mLocation; }
        }

        public static bool operator ==(TilePos lhs, TilePos rhs)
        {
            return lhs.mLocation == rhs.mLocation;
        }

        public static bool operator !=(TilePos lhs, TilePos rhs)
        {
            return lhs.mLocation != rhs.mLocation;
        }

        public void offset(TilePos tilePos)
        {
            offset(tilePos.X, tilePos.Y);
        }

        public void offset(int dx, int dy)
        {
            mLocation.Offset(dx, dy);
        }

        public TilePos getOffset(int dx, int dy)
        {
            return new TilePos(mLocation.X + dx, mLocation.Y + dy);
        }

        public override int GetHashCode()
        {
            return mLocation.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return mLocation.Equals(obj);
        }
    }
}
