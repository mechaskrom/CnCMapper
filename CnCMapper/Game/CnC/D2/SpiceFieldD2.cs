using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    //Helper class for adding spice fields to a map's ground layer.
    //This is based on the "mapRetile" function in "map.cpp" in the DuneMaps project:
    //https://sourceforge.net/p/dunemaps/code/HEAD/tree/head/src/dune/map.cpp#l37
    //I compared the output from this function with a few maps in the game and it seemed to match.
    //Edges of the map may not match the game though because spice fields tend
    //to read/write from/to weird locations around them.
    //
    //I also tested the "Map_FillCircleWithSpice" function in the OpenDUNE project:
    //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L687
    //But it created spice fields that didn't quite match the game for some reason.
    class SpiceFieldD2
    {
        private readonly byte[][] mTileSetTable;
        private readonly TheaterD2 mTheater;
        private readonly RandomD2 mRandom; //This should be the one that the map terrain generator used first.
        private readonly bool[] mTilesWithStructure;

        public SpiceFieldD2(byte[][] tileSetTable, MapD2 map, RandomD2 random, List<SpriteStructureD2> structures)
        {
            mTileSetTable = tileSetTable;
            mTheater = map.Theater;
            mRandom = random;
            mTilesWithStructure = SpriteStructureD2.getTilesWithStructure(structures);
        }

        public void add(int tileNum, int radius)
        {
            if (radius == 0) return;

            int orgX = toTilePosX(tileNum);
            int orgY = toTilePosY(tileNum);

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int curX = (orgX + x) & 0xFFFF; //Wrap around like a UInt16 to match the game.
                    int curY = (orgY + y) & 0xFFFF;
                    int curTileNum = curX | (curY << 6);

                    int distance = getDistance(tileNum, curTileNum);

                    if (distance > radius) continue;

                    if (distance == radius && (mRandom.get() & 0x01) == 0) continue;

                    if (getLandType(curTileNum).Id == LandTypeD2.IdSpice) continue;

                    increaseSpiceAmount(curTileNum);
                }
            }

            increaseSpiceAmount(tileNum);
        }

        private void increaseSpiceAmount(int tileNum)
        {
            if (isTileOutOfMap(tileNum)) return;

            int type = getLandType(tileNum).Id;

            if (type == LandTypeD2.IdThickSpice) return;
            if (type != LandTypeD2.IdNormalSand && type != LandTypeD2.IdEntirelyDune && type != LandTypeD2.IdSpice) return;

            //Change to spice or thick spice?
            int tileId = type != LandTypeD2.IdSpice ? 49 : 65;
            setTable(tileNum, tileId);

            fixupSpiceEdges(tileNum);
            fixupSpiceEdges(tileNum + 1);
            fixupSpiceEdges(tileNum - 1);
            fixupSpiceEdges(tileNum - 64);
            fixupSpiceEdges(tileNum + 64);
        }

        private static readonly int[] AdjOffsets = new int[4] { -64, +1, +64, -1 }; //N, E, S, W.
        private void fixupSpiceEdges(int tileNum)
        {
            tileNum &= 0xFFF;
            int type = getLandType(tileNum).Id;
            if (type == LandTypeD2.IdSpice || type == LandTypeD2.IdThickSpice)
            {
                int tileId = 0;
                for (int i = 0; i < AdjOffsets.Length; i++)
                {
                    int adjTileNum = tileNum + AdjOffsets[i]; //Adjacent tile.
                    int adjX = toTilePosX(adjTileNum);
                    int adjY = toTilePosY(adjTileNum);
                    if (adjX < 0 || adjX > 0x40 || adjY < 0 || adjY > 0x40)
                    {
                        if (type == LandTypeD2.IdSpice || type == LandTypeD2.IdThickSpice)
                        {
                            tileId |= 1 << i;
                        }
                        continue;
                    }

                    int adjType = getLandType(adjTileNum).Id;
                    if (type == LandTypeD2.IdSpice)
                    {
                        if (adjType == LandTypeD2.IdSpice || adjType == LandTypeD2.IdThickSpice)
                        {
                            tileId |= 1 << i;
                        }
                    }
                    else if (adjType == LandTypeD2.IdThickSpice)
                    {
                        tileId |= 1 << i;
                    }
                }

                //Change to spice or thick spice?
                tileId += (type == LandTypeD2.IdSpice) ? 49 : 65;
                setTable(tileNum, tileId);
            }
        }

        private LandTypeD2 getLandType(int tileNum)
        {
            if (isTileOutOfMap(tileNum)) return LandTypeD2.NormalSand;

            byte tileSetId = mTileSetTable[0][tileNum];
            byte tileSetIndex = mTileSetTable[1][tileNum];
            bool hasStructure = mTilesWithStructure[tileNum];
            return mTheater.getLandType(tileSetId, tileSetIndex, hasStructure);
        }

        private void setTable(int tileNum, int tileSetIndex)
        {
            mTileSetTable[0][tileNum] = TileSetD2.IdTerrain;
            mTileSetTable[1][tileNum] = (byte)tileSetIndex;
        }

        private static int toTilePosX(int tileNum)
        {
            return tileNum & 0x3F;
        }

        private static int toTilePosY(int tileNum)
        {
            return (tileNum >> 6) & 0x3F;
        }

        private static bool isTileOutOfMap(int tileNum)
        {
            return (tileNum & 0xF000) != 0;
        }

        private static int getDistance(int tileNum1, int tileNum2)
        {
            //Y pos should not be masked by 0x3F in this function to match the game.
            int x1 = tileNum1 & 0x3F;
            int y1 = tileNum1 >> 6;
            int x2 = tileNum2 & 0x3F;
            int y2 = tileNum2 >> 6;

            //The longest distance between the X or Y pos, plus half the shortest.
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            return Math.Max(dx, dy) + (Math.Min(dx, dy) / 2);
        }
    }
}
