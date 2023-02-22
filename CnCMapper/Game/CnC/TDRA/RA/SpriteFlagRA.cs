using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Flags for the random starting points in multiplayer games.
    class SpriteFlagRA : SpriteTDRA
    {
        private SpriteFlagRA(TilePos tilePos, FileShpSpriteSetTDRA fileShp, byte[] colorRemap)
            : base(fileShp.Id, tilePos, fileShp)
        {
            mColorRemap = colorRemap;
            //Can't check draw offset (assume centered) in the game, but same as in the editor at least.
            mDrawOffset = getDrawOffsetCenter(fileShp);
            mPriPlane = PriPlaneFlag;
        }

        public static void endAdd(List<SpriteFlagRA> flags, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(flags);
        }

        public static void add(MapRA map, List<SpriteFlagRA> flags)
        {
            //Format: number=tileNumber
            //Example: 98=11234
            //Max 100 (0-99) waypoints. 0-7, 98 and 99(?) are special.
            //98 is the initial (x+9, y+7 top left) location of the player's view in a singleplayer mission.
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
                                flags.Add(new SpriteFlagRA(MapRA.toTilePos(tileNum), fileShp, toColorRemap(number)));
                            }
                        }
                    }
                }
            }
            //Players are randomly assigned one of the 8 starting points (waypoint 0-7)?

            //Looking at the source code it seems like max player count in Red Alert is 8.
        }

        private static byte[] toColorRemap(int number)
        {
            //The Red Alert editor seems to use the multiplayer house colors on flags.
            //Maybe just use neutral color on all starting point flags?
            switch (number % 8) //Wrap around if too high.
            {
                case 0: return HouseRA.Multi1.ColorRemap;
                case 1: return HouseRA.Multi2.ColorRemap;
                case 2: return HouseRA.Multi3.ColorRemap;
                case 3: return HouseRA.Multi4.ColorRemap;
                case 4: return HouseRA.Multi5.ColorRemap;
                case 5: return HouseRA.Multi6.ColorRemap;
                case 6: return HouseRA.Multi7.ColorRemap;
                case 7: return HouseRA.Multi8.ColorRemap;
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}
