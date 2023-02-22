using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.D2
{
    class TileSetD2
    {
        private readonly byte mId; //Id of set in the "ICON.MAP" file.
        private readonly Size mTemplateSize; //Size of template in tiles. If 1*1 then all tiles are individual.

        private TileSetD2(byte id, int templateWidth, int templateHeight)
        {
            mId = id;
            mTemplateSize = new Size(templateWidth, templateHeight);
        }

        public byte Id
        {
            get { return mId; }
        }

        public Size TemplateSize
        {
            get { return mTemplateSize; }
        }

        //Not sure how constant the following values really are in Dune 2,
        //but they seem to be the same in the versions I've checked.
        public const byte IdUnknown = 0;
        public const byte IdRockCraters = 1;
        public const byte IdSandCraters = 2;
        public const byte IdAirCrash = 3;
        public const byte IdDeadInfantry = 4;
        public const byte IdVehicleTracks = 5;
        public const byte IdWall = 6;
        public const byte IdShroud = 7;
        public const byte IdConcreteSlab = 8; //Also used by Concrete4. 1*1 and 2*2 slabs.
        public const byte IdTerrain = 9;
        public const byte IdSpiceBloom = 10;
        public const byte IdPalace = 11;
        public const byte IdLightFactory = 12;
        public const byte IdHeavyFactory = 13;
        public const byte IdHighTechFactory = 14;
        public const byte IdIX = 15;
        public const byte IdWOR = 16;
        public const byte IdConstructionYard = 17;
        public const byte IdBarracks = 18;
        public const byte IdWindtrap = 19;
        public const byte IdStarport = 20;
        public const byte IdRefinery = 21;
        public const byte IdRepairFacility = 22;
        public const byte IdTurret = 23;
        public const byte IdRocketTurret = 24;
        public const byte IdSpiceSilos = 25;
        public const byte IdOutpost = 26;

        public static readonly TileSetD2 Unknown = new TileSetD2(IdUnknown, 1, 1); //Unknown.
        public static readonly TileSetD2 RockCraters = new TileSetD2(IdRockCraters, 1, 1);
        public static readonly TileSetD2 SandCraters = new TileSetD2(IdSandCraters, 1, 1);
        public static readonly TileSetD2 AirCrash = new TileSetD2(IdAirCrash, 1, 1);
        public static readonly TileSetD2 DeadInfantry = new TileSetD2(IdDeadInfantry, 1, 1);
        public static readonly TileSetD2 VehicleTracks = new TileSetD2(IdVehicleTracks, 1, 1);
        public static readonly TileSetD2 Wall = new TileSetD2(IdWall, 1, 1);
        public static readonly TileSetD2 Shroud = new TileSetD2(IdShroud, 1, 1); //Unexplored map.
        public static readonly TileSetD2 ConcreteSlab = new TileSetD2(IdConcreteSlab, 1, 1);
        public static readonly TileSetD2 Terrain = new TileSetD2(IdTerrain, 1, 1);
        public static readonly TileSetD2 SpiceBloom = new TileSetD2(IdSpiceBloom, 1, 1);
        public static readonly TileSetD2 Palace = new TileSetD2(IdPalace, 3, 3);
        public static readonly TileSetD2 LightFactory = new TileSetD2(IdLightFactory, 2, 2);
        public static readonly TileSetD2 HeavyFactory = new TileSetD2(IdHeavyFactory, 3, 2);
        public static readonly TileSetD2 HighTechFactory = new TileSetD2(IdHighTechFactory, 3, 2);
        public static readonly TileSetD2 IX = new TileSetD2(IdIX, 2, 2); //Research centre.
        public static readonly TileSetD2 WOR = new TileSetD2(IdWOR, 2, 2); //Trooper training facility.
        public static readonly TileSetD2 ConstructionYard = new TileSetD2(IdConstructionYard, 2, 2);
        public static readonly TileSetD2 Barracks = new TileSetD2(IdBarracks, 2, 2);
        public static readonly TileSetD2 Windtrap = new TileSetD2(IdWindtrap, 2, 2);
        public static readonly TileSetD2 Starport = new TileSetD2(IdStarport, 3, 3);
        public static readonly TileSetD2 Refinery = new TileSetD2(IdRefinery, 3, 2);
        public static readonly TileSetD2 RepairFacility = new TileSetD2(IdRepairFacility, 3, 2);
        public static readonly TileSetD2 Turret = new TileSetD2(IdTurret, 1, 1);
        public static readonly TileSetD2 RocketTurret = new TileSetD2(IdRocketTurret, 1, 1);
        public static readonly TileSetD2 SpiceSilos = new TileSetD2(IdSpiceSilos, 2, 2);
        public static readonly TileSetD2 Outpost = new TileSetD2(IdOutpost, 2, 2); //Radar.

        //Special for Concrete4. Same id as concrete, just a different size.
        public static readonly TileSetD2 Concrete4Slab = new TileSetD2(IdConcreteSlab, 2, 2);

        public static TileSetD2 get(int id)
        {
            switch (id)
            {
                case IdUnknown: return Unknown;
                case IdRockCraters: return RockCraters;
                case IdSandCraters: return SandCraters;
                case IdAirCrash: return AirCrash;
                case IdDeadInfantry: return DeadInfantry;
                case IdVehicleTracks: return VehicleTracks;
                case IdWall: return Wall;
                case IdShroud: return Shroud;
                case IdConcreteSlab: return ConcreteSlab;
                case IdTerrain: return Terrain;
                case IdSpiceBloom: return SpiceBloom;
                case IdPalace: return Palace;
                case IdLightFactory: return LightFactory;
                case IdHeavyFactory: return HeavyFactory;
                case IdHighTechFactory: return HighTechFactory;
                case IdIX: return IX;
                case IdWOR: return WOR;
                case IdConstructionYard: return ConstructionYard;
                case IdBarracks: return Barracks;
                case IdWindtrap: return Windtrap;
                case IdStarport: return Starport;
                case IdRefinery: return Refinery;
                case IdRepairFacility: return RepairFacility;
                case IdTurret: return Turret;
                case IdRocketTurret: return RocketTurret;
                case IdSpiceSilos: return SpiceSilos;
                case IdOutpost: return Outpost;
                default:
                    Program.warn(string.Format("Unknown tile set id value '{0}'!", id));
                    return Unknown;
            }
        }
    }
}
