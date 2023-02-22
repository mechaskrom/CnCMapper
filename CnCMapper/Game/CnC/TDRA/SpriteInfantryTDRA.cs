using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class SpriteInfantryTDRA : SpriteTDRA
    {
        //Infantry in Tiberian Dawn and Red Alert is very similar and can share many properties/methods.

        protected enum SubPos
        {
            Center = 0,
            UpperLeft = 1,
            UpperRight = 2,
            LowerLeft = 3,
            LowerRight = 4,
        }

        protected SubPos mSubPos = SubPos.Center;

        protected SpriteInfantryTDRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp, bool exposeConcealed)
            : base(id, tilePos, fileShp)
        {
            //Infantries are often hidden behind terrain.
            mPriPlane = exposeConcealed ? PriPlaneHigh : PriPlaneDefault;
        }

        protected static Dir8Way toDir8Way(string direction)
        {
            byte dirVal = toDirection(direction);
            //Infantry has 8 frames of rotation. Transform 256 directions to 8. Checked in game (TD and RA).
            if (dirVal <= 13) return Dir8Way.North;
            if (dirVal <= 45) return Dir8Way.Northeast;
            if (dirVal <= 73) return Dir8Way.East;
            if (dirVal <= 112) return Dir8Way.Southeast;
            if (dirVal <= 141) return Dir8Way.South;
            if (dirVal <= 173) return Dir8Way.Southwest;
            if (dirVal <= 201) return Dir8Way.West;
            if (dirVal <= 240) return Dir8Way.Northwest;
            return Dir8Way.North; //241-255.
        }

        protected static SubPos toSubPos(string subPos)
        {
            return (SubPos)int.Parse(subPos);
        }

        protected static Point toLeptons(SubPos subPos)
        {
            switch (subPos)
            {
                case SubPos.Center: return new Point(128, 128);
                case SubPos.UpperLeft: return new Point(64, 64);
                case SubPos.UpperRight: return new Point(192, 64);
                case SubPos.LowerLeft: return new Point(64, 192);
                case SubPos.LowerRight: return new Point(192, 192);
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        protected static int getFrameIndex(Dir8Way direction)
        {
            //Infantry has 8 frames of rotation.
            return (int)direction;
        }

        protected static Point getDrawOffset(SubPos subPos)
        {
            switch (subPos)
            {
                case SubPos.Center: return toDrawOffset(-15, -3);
                case SubPos.UpperLeft: return toDrawOffset(-21, -9);
                case SubPos.UpperRight: return toDrawOffset(-9, -9);
                case SubPos.LowerLeft: return toDrawOffset(-21, 3);
                case SubPos.LowerRight: return toDrawOffset(-9, 3);
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        protected static Point getPriOffset(SubPos subPos)
        {
            //Infantry draw priority is the five subpositions with 48 added to Y. Checked in source (TD and RA).
            Point leptons = toLeptons(subPos);
            return toPriOffset(leptons.X, leptons.Y + 48);
        }
    }
}
