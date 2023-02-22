using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteInfantryTD : SpriteInfantryTDRA
    {
        private SpriteInfantryTD(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp, GameTD.Config.ExposeConcealed)
        {
        }

        public static void endAdd(List<SpriteInfantryTD> infantries, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(infantries);
        }

        public static void add(MapTD map, List<SpriteInfantryTD> infantries)
        {
            //Format: number=house,id,health,tileNumber,subPos,action,direction,trigger
            //Example: 007=BadGuy,E3,256,3249,4,Guard,0,None
            IniSection iniSection = map.FileIni.findSection("INFANTRY");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseTD house = HouseTD.create(values[0]);
                    string id = values[1];
                    TilePos tilePos = MapTD.toTilePos(values[3]);
                    SubPos subPos = toSubPos(values[4]);
                    string action = values[5];
                    Dir8Way direction = toDir8Way(values[6]);
                    string trigger = values[7];

                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                    SpriteInfantryTD spr = new SpriteInfantryTD(id, tilePos, fileShp);
                    //Set values same for all infantries.
                    spr.mFrameIndex = getFrameIndex(direction);
                    spr.mColorRemap = house.ColorRemap;
                    spr.mDrawOffset = getDrawOffset(subPos);
                    spr.mPriOffset = getPriOffset(subPos);
                    spr.mHouse = house;
                    spr.mAction = action;
                    spr.mTrigger = trigger;
                    spr.mSubPos = subPos;
                    addInner(spr, infantries);
                }
            }
        }

        private static void addInner(SpriteInfantryTD spr, List<SpriteInfantryTD> infantries)
        {
            switch (spr.Id)
            {
                case "C1": //Civilian C1-C9.
                case "C2":
                case "C3":
                case "C4":
                case "C5":
                case "C6":
                case "C7":
                case "C8":
                case "C9":
                case "C10": //Nikoomba.
                case "CHAN": //Dr. Chan.
                case "DELPHI": //Agent Delphi.
                case "E1": //Minigunner.
                case "E2": //Grenadier.
                case "E3": //Bazooka.
                case "E4": //Flamethrower.
                case "E5": //Chemwarrior.
                case "E6": //Engineer. Called E7 in the source code, but uses INI name "E6".
                case "MOEBIUS": //Dr. Moebius.
                case "RMBO": //Commando.
                    addDefault(spr, infantries); break;
                default: //Undefined infantry id.
                    addUndefined(spr, infantries); break;
            }
        }

        private static void addDefault(SpriteInfantryTD spr, List<SpriteInfantryTD> infantries)
        {
            infantries.Add(spr);
        }

        private static void addUndefined(SpriteInfantryTD spr, List<SpriteInfantryTD> infantries)
        {
            Program.warn(string.Format("Undefined infantry sprite id '{0}'!", spr.Id));
            if (GameTD.Config.AddUndefinedSprites)
            {
                addDefault(spr, infantries);
            }
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            Point dstPos = TilePos.Location.scaleUp(scale);
            if (scale >= 3)
            {
                //From source, see "Render_Infantry()" in "RADAR.CPP".
                Point offset = toLeptons(mSubPos);
                offset.X /= 86;
                offset.Y /= 86;
                if (scale >= 6)
                {
                    offset.X <<= 1;
                    offset.Y <<= 1;
                }
                dstPos.Offset(offset);
            }
            image[dstPos] = mHouse.RadarBrightIndex; //Always one pixel big regardless of scale.
        }
    }
}
