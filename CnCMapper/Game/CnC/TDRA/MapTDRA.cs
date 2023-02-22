using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    abstract class MapTDRA : MapCnC
    {
        public const int TileWidth = 24; //Pixels.
        public const int TileHeight = 24;

        protected readonly TheaterTDRA mTheater;
        protected HouseTDRA mPlayer;
        protected readonly Dictionary<HouseTDRA, List<HouseTDRA>> mAllies;

        protected MapTDRA(FileIni fileIni, int namesakeCount, Rectangle bordersInTiles, TheaterTDRA theater, HouseTDRA playerDefault)
            : base(fileIni, namesakeCount, bordersInTiles)
        {
            mTheater = theater;
            mPlayer = playerDefault; //Changed later on to "Player" key in [Basic] section if it exists.
            mAllies = new Dictionary<HouseTDRA, List<HouseTDRA>>();
        }

        public TheaterTDRA Theater
        {
            get { return mTheater; }
        }

        public HouseTDRA Player
        {
            get { return mPlayer; }
        }

        public bool isPlayerOrAlly(HouseTDRA house)
        {
            //Returns true if house is the player or allied with them.
            return house == mPlayer || isAlly(house, mPlayer);
        }

        private bool isAlly(HouseTDRA house1, HouseTDRA house2)
        {
            //Returns true if house1 is allied with house2. The reverse does not have to be true.
            List<HouseTDRA> houseAllies;
            if (!mAllies.TryGetValue(house2, out houseAllies)) //House allies not yet parsed?
            {
                houseAllies = new List<HouseTDRA>();
                mAllies.Add(house2, houseAllies);
                IniSection section = mFileIni.findSection(house2.Id);
                if (section != null)
                {
                    IniKey key = section.findKey("Allies");
                    if (key != null)
                    {
                        foreach (string houseId in key.Value.Split(','))
                        {
                            houseAllies.Add(createHouse(houseId));
                        }
                    }
                }
            }
            return houseAllies.Contains(house1);
        }

        protected abstract HouseTDRA createHouse(string id);

        protected static bool isMapTypeIncluded(FileIni fileIni, ConfigTDRA.MapType mapType)
        {
            //Check config if singleplayer, multiplayer or all map types are included when rendering.
            return mapType == ConfigTDRA.MapType.all ||
                (mapType == ConfigTDRA.MapType.single && !fileIni.Id.StartsWith("SCM")) ||
                (mapType == ConfigTDRA.MapType.multi && fileIni.Id.StartsWith("SCM"));
        }

        protected static Rectangle getBordersInTiles(IniSection mapSection)
        {
            int mapWidth = mapSection.getKey("Width").valueAsInt32();
            int mapHeight = mapSection.getKey("Height").valueAsInt32();
            int mapX = mapSection.getKey("X").valueAsInt32();
            int mapY = mapSection.getKey("Y").valueAsInt32();
            return new Rectangle(mapX, mapY, mapWidth, mapHeight);
        }

        protected static Rectangle toInPixels(Rectangle rectInTiles)
        {
            return toInPixels(rectInTiles, TileWidth, TileHeight);
        }
    }

    static class FileIniExt
    {
        public static bool isMapTDRA(this FileIni fileIni) //Determine if this INI-file may be a map.
        {
            //Tiberian Dawn and Red Alert maps should at least have a [MAP] or [Map] section.
            return fileIni.findSection((s) => s.Id == "Map" || s.Id == "MAP") != null;
        }
    }
}
