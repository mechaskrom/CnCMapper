using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    //Dune 2 uses an algorithm that takes a seed value and generates a map terrain/landscape from it.
    //I've no idea how this works, but luckily much smarter people have figured this out.
    //This implementation of the algorithm should generate the same maps as the game.
    //Edges of the map may not match the game though because the generator tends
    //to read/write from/to weird locations around them.


    //This generator is a C# version based mostly on the OpenDUNE version (OpenDUNE-master snapshot 2023-01-11).
    //See the "Map_CreateLandscape()" function in:
    //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L1441
    //I compared it with a few seeds in the game and it matched except for generated spice tiles.
    //
    //I tracked the cause to the "Tile_MoveByRandom()" function and the "Tile_PackTile()" function
    //on the line after it:
    //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L1593
    //"Tile_MoveByRandom()": https://github.com/OpenDUNE/OpenDUNE/blob/master/src/tile.c#L306
    //I changed these lines in it:
    //    x += ((_stepX[orientation] * distance) / 128) * 16;
    //    y -= ((_stepY[orientation] * distance) / 128) * 16;
    //to:
    //    x += (UInt16)(((_stepX[orientation] * distance) >> 7) << 4);
    //    y += (UInt16)(((-_stepY[orientation] * distance) >> 7) << 4);
    //
    //And on the line afterwards I called "Tile_PackXY()" instead.
    //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L1594
    //I changed this line:
    //    packed = Tile_PackTile(unpacked);
    //to:
    //    packed = Tile_PackXY(unpacked.x>>8, unpacked.y>>8);
    //This to avoid masking x and y with 0x3F that "Tile_PackTile()" does.
    //And then generated spice tiles also matched the game.


    //I also looked at other map generators and made some changes based on them. I made sure though
    //that despite the changes the generator still matched the output from the OpenDUNE version with
    //the two fixes described above.
    //
    //"XCC Utilities" project by Olaf van der Spek.
    //https://sourceforge.net/p/xccu/code/HEAD/tree/trunk/xd2/misc/seed_decoder.cpp
    //https://github.com/OlafvdSpek/xcc/blob/master/xd2/misc/seed_decoder.cpp
    //JPEXS added spice generation (and more?) to this version(?) in his SEED2MAP program.
    //https://forum.dune2k.com/topic/20344-map-generator/
    //JPEXS solution can also be found in the "Dune II - The Maker" project by Stefan Hendriks.
    //https://github.com/stefanhendriks/Dune-II---The-Maker/tree/master/tools/seedgen
    //
    //"Dune Legacy" project.
    //https://sourceforge.net/p/dunelegacy/code/ci/master/tree/src/MapSeed.cpp
    //
    //"DuneMaps" project by Segra.
    //https://sourceforge.net/p/dunemaps/code/HEAD/tree/head/src/dune/engine/mapGenerator.cpp
    //DuneMaps seems to very closely match the game so I wanted to compare its map generator with this.
    //I made a C# conversion of DuneMaps map generator function:
    //https://sourceforge.net/p/dunemaps/code/HEAD/tree/head/src/dune/engine/mapGenerator.cpp#l12
    //It needed one small fix affecting dune tiles to match the game. After this line:
    //https://sourceforge.net/p/dunemaps/code/HEAD/tree/head/src/dune/engine/mapGenerator.cpp#l195
    //I added this condition: if (minDune > minRock - 3) minDune = (UInt16)(minRock - 3);
    //Condition taken from OpenDUNE: https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L1557
    //With this small fix it seems to match the game (and this generator).

    class MapGeneratorD2
    {
        //Sine lookup table for calculating spice positions.
        private static readonly sbyte[] sineTable = new sbyte[256] //SineTable[index] = 127 * sin(pi * index/128).
        {
               0,    3,    6,    9,   12,   15,   18,   21,   24,   27,   30,   33,   36,   39,   42,   45,
              48,   51,   54,   57,   59,   62,   65,   67,   70,   73,   75,   78,   80,   82,   85,   87,
              89,   91,   94,   96,   98,  100,  101,  103,  105,  107,  108,  110,  111,  113,  114,  116,
             117,  118,  119,  120,  121,  122,  123,  123,  124,  125,  125,  126,  126,  126,  126,  126,
             127,  126,  126,  126,  126,  126,  125,  125,  124,  123,  123,  122,  121,  120,  119,  118,
             117,  116,  114,  113,  112,  110,  108,  107,  105,  103,  102,  100,   98,   96,   94,   91,
              89,   87,   85,   82,   80,   78,   75,   73,   70,   67,   65,   62,   59,   57,   54,   51,
              48,   45,   42,   39,   36,   33,   30,   27,   24,   21,   18,   15,   12,    9,    6,    3,
               0,   -3,   -6,   -9,  -12,  -15,  -18,  -21,  -24,  -27,  -30,  -33,  -36,  -39,  -42,  -45,
             -48,  -51,  -54,  -57,  -59,  -62,  -65,  -67,  -70,  -73,  -75,  -78,  -80,  -82,  -85,  -87,
             -89,  -91,  -94,  -96,  -98, -100, -102, -103, -105, -107, -108, -110, -111, -113, -114, -116,
            -117, -118, -119, -120, -121, -122, -123, -123, -124, -125, -125, -126, -126, -126, -126, -126,
            -126, -126, -126, -126, -126, -126, -125, -125, -124, -123, -123, -122, -121, -120, -119, -118,
            -117, -116, -114, -113, -112, -110, -108, -107, -105, -103, -102, -100,  -98,  -96,  -94,  -91,
             -89,  -87,  -85,  -82,  -80,  -78,  -75,  -73,  -70,  -67,  -65,  -62,  -59,  -57,  -54,  -51,
             -48,  -45,  -42,  -39,  -36,  -33,  -30,  -27,  -24,  -21,  -18,  -15,  -12,   -9,   -6,   -3
        };

        //Offsets used by matrix.
        private static readonly sbyte[] offsetTable1 = new sbyte[21]
        {
            0, -1, 1, -16, 16, -17, 17, -15, 15, -2, 2, -32, 32, -4, 4, -64, 64, -30, 30, -34, 34
        };

        private static readonly byte[][][] offsetTable2 = new byte[2][][] //2*21*4.
        {
	        new byte[21][]
            {
		        new byte[4]{0, 0, 4, 0}, new byte[4]{4, 0, 4, 4}, new byte[4]{0, 0, 0, 4}, new byte[4]{0, 4, 4, 4},
                new byte[4]{0, 0, 0, 2}, new byte[4]{0, 2, 0, 4}, new byte[4]{0, 0, 2, 0}, new byte[4]{2, 0, 4, 0},
                new byte[4]{4, 0, 4, 2}, new byte[4]{4, 2, 4, 4}, new byte[4]{0, 4, 2, 4}, new byte[4]{2, 4, 4, 4},
                new byte[4]{0, 0, 4, 4}, new byte[4]{2, 0, 2, 2}, new byte[4]{0, 0, 2, 2}, new byte[4]{4, 0, 2, 2},
                new byte[4]{0, 2, 2, 2}, new byte[4]{2, 2, 4, 2}, new byte[4]{2, 2, 0, 4}, new byte[4]{2, 2, 4, 4},
		        new byte[4]{2, 2, 2, 4},
	        },
	        new byte[21][]
            {
		        new byte[4]{0, 0, 4, 0}, new byte[4]{4, 0, 4, 4}, new byte[4]{0, 0, 0, 4}, new byte[4]{0, 4, 4, 4},
                new byte[4]{0, 0, 0, 2}, new byte[4]{0, 2, 0, 4}, new byte[4]{0, 0, 2, 0}, new byte[4]{2, 0, 4, 0},
                new byte[4]{4, 0, 4, 2}, new byte[4]{4, 2, 4, 4}, new byte[4]{0, 4, 2, 4}, new byte[4]{2, 4, 4, 4},
                new byte[4]{4, 0, 0, 4}, new byte[4]{2, 0, 2, 2}, new byte[4]{0, 0, 2, 2}, new byte[4]{4, 0, 2, 2},
                new byte[4]{0, 2, 2, 2}, new byte[4]{2, 2, 4, 2}, new byte[4]{2, 2, 0, 4}, new byte[4]{2, 2, 4, 4},
		        new byte[4]{2, 2, 2, 4},
	        },
        };

        private readonly byte[] mTiles;
        private readonly RandomD2 mRandom;

        private MapGeneratorD2(RandomD2 random)
        {
            mTiles = new byte[64 * 64];
            mRandom = random;
        }

        //Creates the terrain using the given seed.
        public static byte[] createTerrain(int seed)
        {
            return createTerrain(new RandomD2(seed));
        }

        public static byte[] createTerrain(RandomD2 random)
        {
            MapGeneratorD2 mapGen = new MapGeneratorD2(random);

            //Place random data on a 4x4 grid.
            mapGen.initMatrix();

            //Average around the 4x4 grid.
            mapGen.spreadMatrix();

            //Average each tile with its neighbors.
            mapGen.filterMap();

            //Filter each tile to determine its final type.
            mapGen.createRegions();

            //Add some spice.
            mapGen.addSpice();

            //Make everything smoother and use the right tile indices.
            mapGen.finalize();

            return mapGen.mTiles;
        }

        private void initMatrix()
        {
            byte[] matrix = new byte[273];
            for (int i = 0; i < 272; i++)
            {
                matrix[i] = (byte)Math.Min(getRandom() & 0xF, 10);
            }

            for (int i = getRandom() & 0xF; i >= 0; i--)
            {
                int vbase = getRandom();
                for (int j = 0; j < offsetTable1.Length; j++)
                {
                    int index = Math.Min(Math.Max(0, vbase + offsetTable1[j]), 272);
                    matrix[index] = (byte)((matrix[index] + getRandom()) & 0xF);
                }
            }

            for (int i = getRandom() & 0x3; i >= 0; i--)
            {
                int vbase = getRandom();
                for (int j = 0; j < offsetTable1.Length; j++)
                {
                    int index = Math.Min(Math.Max(0, vbase + offsetTable1[j]), 272);
                    matrix[index] = (byte)(getRandom() & 0x3);
                }
            }

            //Copy matrix to tiles.
            for (int y = 0, i = 0; y < 64; y += 4)
            {
                for (int x = 0, k = y * 64; x < 64; x += 4, k += 4, i++)
                {
                    mTiles[k] = matrix[i];
                }
            }
        }

        private void spreadMatrix()
        {
            for (int y = 0; y < 64; y += 4)
            {
                for (int x = 0; x < 64; x += 4)
                {
                    int ot = ((x / 4) + 1) % 2;
                    for (int k = 0; k < 21; k++)
                    {
                        byte[] offsets = offsetTable2[ot][k];
                        int x1 = x + offsets[0];
                        int y1 = y + offsets[1];
                        int x2 = x + offsets[2];
                        int y2 = y + offsets[3];

                        int tileNumMed = (toTileNum(x1, y1) + toTileNum(x2, y2)) / 2;
                        if (isTileOutOfMap(tileNumMed)) continue;

                        int tileNum1 = toTileNum(x1 & 0x3F, y1);
                        int tileNum2 = toTileNum(x2 & 0x3F, y2);
                        System.Diagnostics.Debug.Assert(tileNum1 < 64 * 64);

                        /* ENHANCEMENT -- use groundTileID=0 when out-of-bounds to generate the original maps. */
                        int tileId = 0;
                        if (tileNum2 < 64 * 64)
                        {
                            tileId = mTiles[tileNum2];
                        }

                        mTiles[tileNumMed] = (byte)((mTiles[tileNum1] + tileId + 1) / 2);
                    }
                }
            }
        }

        private void filterMap()
        {
            //Copy current tiles. We are going to change them based on their current values.
            byte[] oldTiles = mTiles.takeBytes();
            for (int y = 0, k = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++, k++)
                {
                    int c, nw, n, ne, e, se, s, sw, w; //Center with neighbors.
                    nw = n = ne = e = se = s = sw = w = c = oldTiles[k]; //Initialize all as center.

                    if (x != 0 && y != 0) nw = oldTiles[k - 64 - 1]; //NW.
                    if (y != 0) n = oldTiles[k - 64]; //N.
                    if (x != 63 && y != 0) ne = oldTiles[k - 64 + 1]; //NE.
                    if (x != 0) w = oldTiles[k - 1]; //W.
                    if (x != 63) e = oldTiles[k + 1]; //E.
                    if (x != 0 && y != 63) sw = oldTiles[k + 64 - 1]; //SW.
                    if (y != 63) s = oldTiles[k + 64]; //S.
                    if (x != 63 && y != 63) se = oldTiles[k + 64 + 1]; //SE.

                    mTiles[k] = (byte)((c + nw + n + ne + e + se + s + sw + w) / 9);
                }
            }
        }

        private void createRegions()
        {
            //Variables named like terrains are range limits.
            int rock = Math.Min(Math.Max(getRandom() & 0xF, 8), 12); //8 to 12.
            int mountains = rock + 4;
            int dunes = (getRandom() & 0x3) - 1; //-1 to 2.
            if (dunes < 0)
            {
                dunes = rock - 3;
                //This causes some seeds (e.g. 355 and 666) to generate a lot of dunes.
                //Seems more like a bug than intended behavior, but it matches the game.
            }

            for (int i = 0; i < 4096; i++)
            {
                int tileId = mTiles[i];

                if (tileId > mountains) tileId = LandTypeD2.IdEntirelyMountain;
                else if (tileId >= rock) tileId = LandTypeD2.IdEntirelyRock;
                else if (tileId <= dunes) tileId = LandTypeD2.IdEntirelyDune;
                else tileId = LandTypeD2.IdNormalSand;

                mTiles[i] = (byte)tileId;
            }
        }

        private void addSpice()
        {
            for (int i = getRandom() & 0x2F; i > 0; i--)
            {
                int tileNum;
                do
                {
                    int y = getRandom() & 0x3F;
                    int x = getRandom() & 0x3F;
                    tileNum = toTileNum(x, y);
                } while (!canBecomeSpice(mTiles[tileNum]));

                for (int j = getRandom() & 0x1F; j > 0; j--)
                {
                    int movedTileNum;
                    do
                    {
                        int distance = getRandom() & 0x3F;
                        movedTileNum = moveTileRandom(tileNum, distance);
                    } while (isTileOutOfMap(movedTileNum));

                    addSpiceOnTile(movedTileNum);
                }
            }
        }

        private void addSpiceOnTile(int tileNum)
        {
            int tileId = mTiles[tileNum];
            if (tileId == LandTypeD2.IdSpice)
            {
                mTiles[tileNum] = LandTypeD2.IdThickSpice;
                addSpiceOnTile(tileNum);
            }
            else if (tileId == LandTypeD2.IdThickSpice)
            {
                int tileX = (tileNum >> 0) & 0x3F;
                int tileY = (tileNum >> 6) & 0x3F;
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        int newTileNum = toTileNum(tileX + i, tileY + j);

                        if (isTileOutOfMap(newTileNum)) continue;

                        if (!canBecomeSpice(mTiles[newTileNum]))
                        {
                            mTiles[tileNum] = LandTypeD2.IdSpice;
                        }
                        else if (mTiles[newTileNum] != LandTypeD2.IdThickSpice)
                        {
                            mTiles[newTileNum] = LandTypeD2.IdSpice;
                        }
                    }
                }
            }
            else if (canBecomeSpice(mTiles[tileNum]))
            {
                mTiles[tileNum] = LandTypeD2.IdSpice;
            }
        }

        private void finalize()
        {
            //Copy current tiles. We are going to change them based on their current values.
            byte[] oldTiles = mTiles.takeBytes();
            for (int y = 0, k = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++, k++)
                {
                    int c, n, e, s, w; //Center with neighbors.
                    n = e = s = w = c = oldTiles[k]; //Initialize all as center.

                    if (y != 0) n = oldTiles[k - 64]; //N.
                    if (x != 63) e = oldTiles[k + 1]; //E.
                    if (y != 63) s = oldTiles[k + 64]; //S.
                    if (x != 0) w = oldTiles[k - 1]; //W.

                    int tileId = 0;
                    if (n == c) tileId |= 1;
                    if (e == c) tileId |= 2;
                    if (s == c) tileId |= 4;
                    if (w == c) tileId |= 8;

                    switch (c)
                    {
                        case LandTypeD2.IdNormalSand:
                            tileId = 0;
                            break;
                        case LandTypeD2.IdEntirelyRock:
                            if (n == LandTypeD2.IdEntirelyMountain) tileId |= 1;
                            if (e == LandTypeD2.IdEntirelyMountain) tileId |= 2;
                            if (s == LandTypeD2.IdEntirelyMountain) tileId |= 4;
                            if (w == LandTypeD2.IdEntirelyMountain) tileId |= 8;
                            tileId += 1;
                            break;
                        case LandTypeD2.IdEntirelyDune:
                            tileId += 17;
                            break;
                        case LandTypeD2.IdEntirelyMountain:
                            tileId += 33;
                            break;
                        case LandTypeD2.IdSpice:
                            if (n == LandTypeD2.IdThickSpice) tileId |= 1;
                            if (e == LandTypeD2.IdThickSpice) tileId |= 2;
                            if (s == LandTypeD2.IdThickSpice) tileId |= 4;
                            if (w == LandTypeD2.IdThickSpice) tileId |= 8;
                            tileId += 49;
                            break;
                        case LandTypeD2.IdThickSpice:
                            tileId += 65;
                            break;
                        default: break;
                    }

                    mTiles[k] = (byte)tileId;
                }
            }
        }

        private int getRandom()
        {
            return mRandom.get();
        }

        private int moveTileRandom(int tileNum, int distance) //Move tile in a random direction.
        {
            if (distance == 0) return tileNum;

            //Do calculation in leptons. Lower 8 bits in tile position. 128 (0x80) is center of tile.
            int x = (((tileNum >> 0) & 0x3F) << 8) | 0x80;
            int y = (((tileNum >> 6) & 0x3F) << 8) | 0x80;

            int newDistance = getRandom();
            while (newDistance > distance) newDistance >>= 1;

            int orientation = getRandom();
            x += ((sineTable[orientation] * newDistance) >> 7) << 4;
            y += ((-sineTable[(orientation + 64) & 0xFF] * newDistance) >> 7) << 4;

            x &= 0xFFFF; //Wrap around like a UInt16 to match the game.
            y &= 0xFFFF;

            if (x > (64 << 8) || y > (64 << 8)) return tileNum;

            return toTileNum(x >> 8, y >> 8);
        }

        private static int toTileNum(int x, int y)
        {
            //Can't do y*64+x. Will not match the game because x can be >= 64.
            //UInt16 masking (0xFFFF) probably not needed, but just in case.
            return ((y << 6) | x) & 0xFFFF;
        }

        private static bool isTileOutOfMap(int tileNum)
        {
            return (tileNum & 0xF000) != 0;
        }

        private static bool canBecomeSpice(int tileId) //Land type can become spice?
        {
            return LandTypeD2.get(tileId).CanBecomeSpice;
        }
    }
}
