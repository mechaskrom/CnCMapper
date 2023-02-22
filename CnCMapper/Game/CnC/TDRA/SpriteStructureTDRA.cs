using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    abstract class SpriteStructureTDRA : SpriteTDRA
    {
        //Extra priority offset to add to all base structures to control draw order against normal structures.
        public static readonly Point PriOffsetAddBase = toPriOffset(1, 0); //A bit higher priority -> draw base over normal.
        //public static readonly Point PriOffsetAddBase = toPriOffset(-1, 0); //A bit lower priority -> draw base under normal.

        private const int BaseNumberNone = -1; //Not a base structure if < 0.

        protected int mBaseNumber = BaseNumberNone;

        //Rebuilt = base and normal structure with the same id at the same tile position.
        //Used mostly to speed up drawing by skipping some base structures.
        //Nullable because value is set late and shouldn't be used before.
        protected bool? mIsRebuilt = null;

        protected SpriteStructureTDRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
        }

        public int BaseNumber
        {
            get { return mBaseNumber; }
        }

        public bool IsBase
        {
            get { return mBaseNumber > BaseNumberNone; }
        }

        public bool IsRebuilt
        {
            get
            {
                System.Diagnostics.Debug.Assert(mIsRebuilt.HasValue, "Must call 'endAdd()' before 'IsRebuilt' is used!");
                return mIsRebuilt.Value;
            }
        }

        public abstract bool HasBib
        {
            get;
        }

        public abstract TilePos getBibPos();

        protected static void setIsRebuilt<T>(List<T> strNormals, List<T> strBases)
            where T : SpriteStructureTDRA
        {
            //Adjust structures where normal and base overlap. If overlap:
            //-set same frame index so base can be drawn over/under normal.
            //-set is rebuilt flag for both.
            foreach (T strBase in strBases)
            {
                foreach (T strNormal in strNormals)
                {
                    if (strBase.TilePos == strNormal.TilePos && strBase.Id == strNormal.Id)
                    {
                        //Base overlaps with normal at tile position.
                        strBase.mFrameIndex = strNormal.mFrameIndex; //Set same frame index as normal.
                        strBase.mIsRebuilt = true;
                        strNormal.mIsRebuilt = true;
                    }
                }
            }
        }

        protected static int getFrameIndex32Dir(string direction)
        {
            //Game uses a lookup table (see "CONST.CPP"). Checked in source.
            //Turrets have 32 frames of rotation and all can be selected. Checked in game.
            //Same in both Tiberian Dawn and Red Alert.
            byte dirVal = toDirection(direction);
            if (dirVal <= 4) return 0; //North.
            if (dirVal <= 13) return 31;
            if (dirVal <= 21) return 30;
            if (dirVal <= 32) return 29;
            if (dirVal <= 38) return 28; //North-east.
            if (dirVal <= 45) return 27;
            if (dirVal <= 52) return 26;
            if (dirVal <= 59) return 25;
            if (dirVal <= 66) return 24; //East.
            if (dirVal <= 73) return 23;
            if (dirVal <= 80) return 22;
            if (dirVal <= 87) return 21;
            if (dirVal <= 95) return 20; //South-east.
            if (dirVal <= 103) return 19;
            if (dirVal <= 112) return 18;
            if (dirVal <= 121) return 17;
            if (dirVal <= 132) return 16; //South.
            if (dirVal <= 141) return 15;
            if (dirVal <= 150) return 14;
            if (dirVal <= 160) return 13;
            if (dirVal <= 166) return 12; //South-west.
            if (dirVal <= 173) return 11;
            if (dirVal <= 180) return 10;
            if (dirVal <= 187) return 9;
            if (dirVal <= 194) return 8; //West.
            if (dirVal <= 201) return 7;
            if (dirVal <= 208) return 6;
            if (dirVal <= 215) return 5;
            if (dirVal <= 223) return 4; //North-west.
            if (dirVal <= 231) return 3;
            if (dirVal <= 240) return 2;
            if (dirVal <= 249) return 1;
            return 0; //North (250-255).
        }

        protected static Point getPriOffsetDefault(Size sprSizeInTiles)
        {
            //Same in both Tiberian Dawn and Red Alert. Checked in source.
            return getPriOffsetCenter(sprSizeInTiles).getOffset(0, (sprSizeInTiles.Height * 256) / 3);
        }

        protected static Point getPriOffsetCenter(Size sprSizeInTiles)
        {
            //Same in both Tiberian Dawn and Red Alert. Checked in source.
            return toPriOffset(getPriOffsetLength(sprSizeInTiles.Width), getPriOffsetLength(sprSizeInTiles.Height));
        }

        private static int getPriOffsetLength(int lengthInTiles)
        {
            //Center of length in tiles. Two tiles result in 255 and not 256 leptons for some reason.
            //Same in both Tiberian Dawn and Red Alert. Checked in source.
            return lengthInTiles == 2 ? 255 : lengthInTiles * 128; //Leptons.
        }
    }
}
