using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    //Values from OpenDUNE: https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.h#L6
    class LandTypeD2
    {
        public const int IdNormalSand = 0; //Flat sand.
        public const int IdPartialRock = 1; //Edge of a rocky area (mostly sand).
        public const int IdEntirelyDune = 2; //Entirely sand dunes.
        public const int IdPartialDune = 3; //Partial sand dunes.
        public const int IdEntirelyRock = 4; //Center part of rocky area.
        public const int IdMostlyRock = 5; //Edge of a rocky area (mostly rocky).
        public const int IdEntirelyMountain = 6; //Center part of the mountain.
        public const int IdPartialMountain = 7; //Edge of a mountain.
        public const int IdSpice = 8; //Sand with spice.
        public const int IdThickSpice = 9; //Sand with thick spice.
        public const int IdConcreteSlab = 10; //Concrete slab.
        public const int IdWall = 11; //Wall.
        public const int IdStructure = 12; //Structure.
        public const int IdDestroyedWall = 13; //Destroyed wall.
        public const int IdBloomField = 14; //Bloom field.

        private static readonly byte OwnerRadarColor = ColorSchemeD2.Neutral.RadarIndex; //No owner so use neutral radar color.

        public static readonly LandTypeD2 NormalSand = new LandTypeD2(IdNormalSand, true, 88);
        public static readonly LandTypeD2 PartialRock = new LandTypeD2(IdPartialRock, false, 28);
        public static readonly LandTypeD2 EntirelyDune = new LandTypeD2(IdEntirelyDune, true, 92);
        public static readonly LandTypeD2 PartialDune = new LandTypeD2(IdPartialDune, true, 89);
        public static readonly LandTypeD2 EntirelyRock = new LandTypeD2(IdEntirelyRock, false, 30);
        public static readonly LandTypeD2 MostlyRock = new LandTypeD2(IdMostlyRock, false, 29);
        public static readonly LandTypeD2 EntirelyMountain = new LandTypeD2(IdEntirelyMountain, false, 12);
        public static readonly LandTypeD2 PartialMountain = new LandTypeD2(IdPartialMountain, false, 133);
        public static readonly LandTypeD2 Spice = new LandTypeD2(IdSpice, true, 215);
        public static readonly LandTypeD2 ThickSpice = new LandTypeD2(IdThickSpice, true, 216);
        public static readonly LandTypeD2 ConcreteSlab = new LandTypeD2(IdConcreteSlab, false, 133);
        public static readonly LandTypeD2 Wall = new LandTypeD2(IdWall, false, OwnerRadarColor);
        public static readonly LandTypeD2 Structure = new LandTypeD2(IdStructure, false, OwnerRadarColor);
        public static readonly LandTypeD2 DestroyedWall = new LandTypeD2(IdDestroyedWall, false, 29);
        public static readonly LandTypeD2 BloomField = new LandTypeD2(IdBloomField, true, 50);

        private readonly int mId;
        private readonly bool mCanBecomeSpice;
        private readonly byte mRadarIndex; //Radar color palette index.

        private LandTypeD2(int id, bool canBecomeSpice, byte radarIndex)
        {
            mId = id;
            mCanBecomeSpice = canBecomeSpice;
            mRadarIndex = radarIndex;
        }

        public int Id { get { return mId; } }
        public bool CanBecomeSpice { get { return mCanBecomeSpice; } }
        public byte RadarIndex { get { return mRadarIndex; } }

        public static LandTypeD2 get(int id)
        {
            switch (id)
            {
                case IdNormalSand: return NormalSand;
                case IdPartialRock: return PartialRock;
                case IdEntirelyDune: return EntirelyDune;
                case IdPartialDune: return PartialDune;
                case IdEntirelyRock: return EntirelyRock;
                case IdMostlyRock: return MostlyRock;
                case IdEntirelyMountain: return EntirelyMountain;
                case IdPartialMountain: return PartialMountain;
                case IdSpice: return Spice;
                case IdThickSpice: return ThickSpice;
                case IdConcreteSlab: return ConcreteSlab;
                case IdWall: return Wall;
                case IdStructure: return Structure;
                case IdDestroyedWall: return DestroyedWall;
                case IdBloomField: return BloomField;
                default: throw new ArgumentException("Value is not a valid land type id!");
            }
        }

        public static LandTypeD2 get(int tileSetId, int tileSetIndex, FileMapTileSetsD2 fileMap, bool hasStructure)
        {
            int ti = fileMap.getTileIndex(tileSetId, tileSetIndex);
            int tiSlab = fileMap.getTileIndex(TileSetD2.IdConcreteSlab, 2);
            int tiBloom = fileMap.getTileIndex(TileSetD2.IdSpiceBloom, 0);
            int tiWall = fileMap.getTileIndex(TileSetD2.IdWall, 0);
            int tiTerrain = fileMap.getTileIndex(TileSetD2.IdTerrain, 0);
            return get(ti, tiSlab, tiBloom, tiWall, tiTerrain, hasStructure);
        }

        private static LandTypeD2 get(int ti, int tiSlab, int tiBloom, int tiWall, int tiTerrain, bool hasStructure)
        {
            //Determine land type of tile index based on values in a MAP-file.
            //ti = MAP[tileSetId][tileSetIndex]; //Tile to determine land type for.
            //tiSlab = MAP[8][2];
            //tiBloom = MAP[10][0];
            //tiWall = MAP[6][0];
            //tiTerrain = MAP[9][0];

            //Testing indicate that the game does something rather complicated to determine land type.
            //In the game the land type seems to be affected by tile index values in the terrain tile set.
            //So it's not simply one land type per tile in the ICN-file. The first value in the set
            //seems to affect the land type of the other tiles in it? It's really weird.

            //This code is the result from testing and the C version in the OpenDUNE project.
            //See the "Map_GetLandscapeType" function in "map.c".
            //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/map.c#L541
            //We only care about tiles in the ground layer here so this conversion ignores overlays,
            //structures, etc.

            //I'm not sure that this matches the game in all (weird) cases, but in the normal case it
            //should be the same.

            //Concrete slab.
            if (ti == tiSlab) return LandTypeD2.ConcreteSlab;

            //Spice bloom.
            if (ti == tiBloom || (ti == tiBloom + 1)) return LandTypeD2.BloomField;

            //Wall.
            if (ti > tiWall && (ti < tiWall + 75)) return LandTypeD2.Wall;

            //Wall destroyed. In the overlay layer which we don't need to worry about.

            //Structure.
            if (hasStructure) return LandTypeD2.Structure; //Tile has a structure?

            //Terrain. Return as rock if index is out of range. Calculate the range from the first index in the terrain set.
            ti -= tiTerrain;

            if (ti < 0) return LandTypeD2.EntirelyRock; //Default out of range (rock).

            if (ti == 0) return LandTypeD2.NormalSand; //Sand.
            if (ti <= 3 || ti == 5) return LandTypeD2.PartialRock; //Sand-rock edge. Mostly sand.

            if (ti <= 15) return LandTypeD2.MostlyRock; //Sand-rock edge. Mostly rock.
            if (ti == 16) return LandTypeD2.EntirelyRock; //Rock.

            if (ti <= 31) return LandTypeD2.PartialDune; //Sand-dune edge.
            if (ti == 32) return LandTypeD2.EntirelyDune; //Dune.

            if (ti <= 47) return LandTypeD2.PartialMountain; //Rock-mountain edge.
            if (ti == 48) return LandTypeD2.EntirelyMountain; //Mountain.

            if (ti <= 64) return LandTypeD2.Spice; //Spice.
            if (ti <= 80) return LandTypeD2.ThickSpice; //Thick spice.

            return LandTypeD2.EntirelyRock; //Default out of range (rock).
        }
    }
}
