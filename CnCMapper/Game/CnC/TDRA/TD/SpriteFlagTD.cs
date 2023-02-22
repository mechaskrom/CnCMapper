using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Flags for the random starting points in multiplayer games.
    class SpriteFlagTD : SpriteTDRA
    {
        private SpriteFlagTD(TilePos tilePos, FileShpSpriteSetTDRA fileShp, byte[] colorRemap)
            : base(fileShp.Id, tilePos, fileShp)
        {
            mColorRemap = colorRemap;
            mDrawOffset = getDrawOffsetCenter(fileShp); //Assume centered. Can't(?) check in the game.
            mPriPlane = PriPlaneFlag;
        }

        public static void endAdd(List<SpriteFlagTD> flags, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(flags);
        }

        public static void add(MapTD map, List<SpriteFlagTD> flags)
        {
            //Format: number=tileNumber
            //Example: 27=2087
            //Max 32 (0-31) waypoints and tileNumber=-1 means that it isn't used. 0-7 and 25-27 are special.
            //26 is the initial (x+0, y+0 top left) location of the player's view in a singleplayer mission.
            //0-7 are used as starting points in multiplayer games.

            //TODO: Better way to detect a multiplayer map? MPR=Multiplayer INI-file, but only used in Red Alert?
            FileIni fileIni = map.FileIni;
            if (fileIni.Id.StartsWith("SCM") || fileIni.Ext == "MPR")
            {
                IniSection iniSection = fileIni.findSection("Waypoints");
                if (iniSection != null)
                {
                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet("FLAGFLY");
                    foreach (IniKey key in iniSection.Keys)
                    {
                        int number = key.idAsInt32();
                        if (number <= 7) //Waypoint 0-7 are starting points?
                        {
                            int tileNum = key.valueAsInt32();
                            if (tileNum >= 0)
                            {
                                flags.Add(new SpriteFlagTD(MapTD.toTilePos(tileNum), fileShp, toColorRemap(number)));
                            }
                        }
                    }
                }
            }
            //Looking at the source code it seems like max player count in Tiberian Dawn is 6.
            //And flags use the color of the house owning it (capture the flag).
            //Players are randomly assigned one of the 8 starting points (waypoint 0-7)?
            //Actually it randomly picks it from a sorted list of assigned waypoints (i.e. the first 6 valid)?
        }

        private static byte[] toColorRemap(int number)
        {
            //The Red Alert editor seems to use the multiplayer house colors on flags.
            //Let's do the same in Tiberian Dawn.
            //Maybe just use neutral color on all starting point flags?
            switch (number % 6) //Wrap around if too high.
            {
                case 0: return HouseTD.Multi1.ColorRemap;
                case 1: return HouseTD.Multi2.ColorRemap;
                case 2: return HouseTD.Multi3.ColorRemap;
                case 3: return HouseTD.Multi4.ColorRemap;
                case 4: return HouseTD.Multi5.ColorRemap;
                case 5: return HouseTD.Multi6.ColorRemap;
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}
