using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteShipRA : SpriteTDRA
    {
        //Ship/Vessel never overrides base class sorting AFAICT so its draw priority offset
        //should be the same as a centered infantry. Checked in source.
        private static readonly Point PriOffsetShip = toPriOffset(128, 128 + 48);

        private SpriteShipRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
            mPriOffset = PriOffsetShip;
        }

        private static SpriteShipRA createTurretSprite(SpriteShipRA owner, FileShpSpriteSetTDRA fileShp, Point drawOffset)
        {
            SpriteShipRA spr = new SpriteShipRA(fileShp.Id, owner.TilePos, fileShp);
            owner.setAddSpriteDraw(spr);
            //A turret sprite is drawn centered like other units. It is not affected
            //by any draw offset its owner has. Checked in game.
            spr.mDrawOffset = drawOffset.getOffset(getDrawOffsetCenter(fileShp));
            return spr;
        }

        public static void endAdd(List<SpriteShipRA> ships, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(ships);
        }

        public static void add(MapRA map, List<SpriteShipRA> ships)
        {
            add(map, map.FileIni, ships);
        }

        public static void add(MapRA map, FileIni fileIni, List<SpriteShipRA> ships)
        {
            //Format: number=house,id,health,tileNumber,direction,action,trigger
            //Example: 3=USSR,SS,256,10932,192,Area Guard,sea3
            IniSection iniSection = fileIni.findSection("SHIPS");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseRA house = HouseRA.create(values[0]);
                    string id = values[1];
                    TilePos tilePos = MapRA.toTilePos(values[3]);
                    Dir16Way direction = toDir16Way(values[4]);
                    string action = values[5];
                    string trigger = values[6];

                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                    SpriteShipRA spr = new SpriteShipRA(id, tilePos, fileShp);
                    //Set values same for all ships.
                    spr.mColorRemap = house.ColorRemap;
                    spr.mDrawOffset = getDrawOffsetCenter(spr);
                    spr.mHouse = house;
                    spr.mAction = action;
                    spr.mTrigger = trigger;
                    addInner(spr, direction, map, ships);
                }
            }
        }

        private static void addInner(SpriteShipRA spr, Dir16Way direction, MapRA map, List<SpriteShipRA> ships)
        {
            switch (spr.Id)
            {
                case "CA": //Cruiser.
                    addCa(spr, direction, map, ships); break;
                case "DD": //Destroyer.
                    addDd(spr, direction, map, ships); break;
                case "LST": //Transport hovercraft.
                    addTransport(spr, ships); break;
                case "PT": //Gunboat.
                    addPt(spr, direction, map, ships); break;
                case "SS": //Submarine.
                    addDefault(spr, direction, ships); break;

                case "CARR": //Helicarrier. Aftermath.
                    addTransport(spr, ships); break;
                case "MSUB": //Missile submarine. Aftermath.
                    addDefault(spr, direction, ships); break;

                default: //Undefined ship id.
                    addUndefined(spr, direction, ships); break;
            }
        }

        private static void addCa(SpriteShipRA spr, Dir16Way direction, MapRA map, List<SpriteShipRA> ships)
        {
            addDefault(spr, direction, ships);
            spr.mAddSprite = getTurretSpriteCa(spr, direction, map); //Last so owner is properly set up first.
        }

        private static void addDd(SpriteShipRA spr, Dir16Way direction, MapRA map, List<SpriteShipRA> ships)
        {
            addDefault(spr, direction, ships);
            spr.mAddSprite = getTurretSpriteDd(spr, direction, map);
        }

        private static void addPt(SpriteShipRA spr, Dir16Way direction, MapRA map, List<SpriteShipRA> ships)
        {
            addDefault(spr, direction, ships);
            spr.mAddSprite = getTurretSpritePt(spr, direction, map);
        }

        private static void addTransport(SpriteShipRA spr, List<SpriteShipRA> ships)
        {
            ships.Add(spr);
        }

        private static void addDefault(SpriteShipRA spr, Dir16Way direction, List<SpriteShipRA> ships)
        {
            spr.mFrameIndex = getFrameIndexDefault(direction);
            ships.Add(spr);
        }

        private static void addUndefined(SpriteShipRA spr, Dir16Way direction, List<SpriteShipRA> ships)
        {
            Program.warn(string.Format("Undefined ship sprite id '{0}'!", spr.Id));
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, direction, ships);
            }
        }

        private static Dir16Way toDir16Way(string direction)
        {
            byte dirVal = toDirection(direction);
            //Transform 256 directions to 16. Invert index also to match frame index.
            return (Dir16Way)((-(dirVal + 8) / 16) & 15); //Offset start and divide by 16 (256/16).
        }

        private static int getFrameIndexDefault(Dir16Way direction)
        {
            //Ships has 16 frames of rotation. Checked in game.
            return (int)direction;
        }

        private static SpriteShipRA getTurretSpriteDd(SpriteShipRA owner, Dir16Way direction, MapRA map)
        {
            //DD has a single turret. Frame index isn't affected by direction, but its draw offset is.
            Point drawOffset = getTurretDrawOffsetInverted(direction, 8); //Stern turret.
            drawOffset.Y -= 4; //Constant offset.
            return createTurretSprite(owner, map.Theater.getSpriteSet("SSAM"), drawOffset);
        }

        private static SpriteShipRA getTurretSpritePt(SpriteShipRA owner, Dir16Way direction, MapRA map)
        {
            //PT has a single turret. Frame index isn't affected by direction, but its draw offset is.
            Point drawOffset = getTurretDrawOffset(direction, 14); //Bow turret.
            drawOffset.Y += 1; //Constant offset.
            return createTurretSprite(owner, map.Theater.getSpriteSet("MGUN"), drawOffset);
        }

        private static SpriteShipRA getTurretSpriteCa(SpriteShipRA owner, Dir16Way direction, MapRA map)
        {
            //CA has two turrets. Frame index isn't affected by direction, but their draw offset is.
            Point offsFront = getTurretDrawOffset(direction, 22); //Bow turret.
            Point offsBack = getTurretDrawOffsetInverted(direction, 22); //Stern turret.
            offsFront.Y -= 4; //Constant offset.
            offsBack.Y -= 4;
            //Chain the two turret sprites together.
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet("TURR");
            SpriteShipRA spr = createTurretSprite(owner, fileShp, offsFront);
            spr.mAddSprite = createTurretSprite(owner, fileShp, offsBack);
            return spr;
        }

        private static Point getTurretDrawOffsetInverted(Dir16Way direction, int distance)
        {
            //For turrets located at the stern/back of the ship.
            return getTurretDrawOffset(toDir16WayInverted(direction), distance);
        }

        private static Point getTurretDrawOffset(Dir16Way direction, int distance)
        {
            //Adoption of source code to calculate turret position on ships.
            //See "Turret_Adjust()" in "VDATA.CPP".
            return movePoint(direction, distance, true);
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            Rectangle dstRect = new Rectangle(TilePos.X * scale, TilePos.Y * scale, scale, scale);
            image.drawRectFilled(dstRect, GameRA.Config.UseRadarBrightColor ? mHouse.RadarBrightIndex : mHouse.RadarIndex);
        }
    }
}
