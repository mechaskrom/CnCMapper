using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;
using System.Drawing;

namespace CnCMapper.Game.CnC.D2
{
    partial class SpriteUnitD2 : SpriteD2
    {
        private readonly Dir8Way mDirection;
        private DirFrameSet mFrameSet;
        private DirFrameSet mTurretFrameSet; //Null if unit has no turret.
        private bool mIsAircraft;

        private SpriteUnitD2(string id, TilePos tilePos, HouseD2 house, Dir8Way direction, string action)
            : base(id, tilePos, PriPlaneDefault, house, action)
        {
            mDirection = direction;
            mFrameSet = DirFrameSet.Unit.None;
            mTurretFrameSet = DirFrameSet.Turret.None;
            mIsAircraft = false;
        }

        public static void endAdd(MapD2 map, List<SpriteUnitD2> units)
        {
            //Nothing to do.
        }

        public static void add(MapD2 map, List<SpriteUnitD2> units)
        {
            //Format: ID<number>=house,id,health,tileNumber,direction,action
            //Example: ID036=Ordos,Tank,256,1492,64,Area Guard

            //Units with the same key id are handled a bit weird by the game. It seems to add
            //the first (lowest line number in the INI-file) entry normally. Any repeated id
            //afterwards is just ignored and the first entry (with its values) is added again
            //i.e. the first entry is added every time its id is found.

            //This seems to be the same behavior as in Tiberian Dawn i.e. when a key's values
            //are to be fetched the game searches (from top to bottom in INI-section) for the
            //key's id. So use the Tiberian Dawn key finder method to mimic this behavior.

            //Units can have the same tile number. They are all added (stacked) to the same tile. Checked in game.

            //Dune 2 has really low max unit counts. I don't know the exact numbers, but it's around:
            //-80 units.
            //-11 aircraft (Carryall + Ornithopter).
            //-1 Frigate.

            //Sandworms max is a bit hard to test, but it's just a few, like 2-5?

            //IniSection iniSection = map.FileIni.findSection("UNITS");
            IniKeyFinderD2 keyFinder = IniKeyFinderD2.create(map.FileIni.findSection("UNITS"));
            //if (iniSection != null)
            if (keyFinder != null)
            {
                //foreach (IniKey key in iniSection.Keys)
                foreach (IniKey key in keyFinder.findKeys())
                {
                    string[] values = key.Value.Split(',');
                    HouseD2 house = HouseD2.create(values[0]);
                    string id = values[1];
                    //int health = toHealth(values[2]); //Health field not used.
                    TilePos tilePos = MapD2.toTilePos(values[3]);
                    Dir8Way direction = toDir8Way(values[4]);
                    string action = values[5];
                    addInner(id, tilePos, house, direction, action, units);
                }
            }
        }

        private static void addInner(string id, TilePos tilePos, HouseD2 house, Dir8Way direction, string action, List<SpriteUnitD2> units)
        {
            SpriteUnitD2 spr = new SpriteUnitD2(id, tilePos, house, direction, action);
            addInner(spr, units);
        }

        private static void addInner(SpriteUnitD2 spr, List<SpriteUnitD2> units)
        {
            switch (spr.Id)
            {
                case "Carryall": //Carryall.
                    addAircraft(spr, DirFrameSet.Unit.Carryall, units); break;
                case "'Thopter": //Ornithopter.
                    addAircraft(spr, DirFrameSet.Unit.Ornithopter, units); break;
                case "Frigate": //Frigate.
                    addAircraft(spr, DirFrameSet.Unit.Frigate, units); break;

                case "Soldier": //Soldier.
                    addDefault(spr, DirFrameSet.Unit.Soldier, units); break;
                case "Infantry": //Infantry.
                    addDefault(spr, DirFrameSet.Unit.Infantry, units); break;
                case "Trooper": //Trooper.
                    addDefault(spr, DirFrameSet.Unit.Trooper, units); break;
                case "Troopers": //Troopers.
                    addDefault(spr, DirFrameSet.Unit.Troopers, units); break;
                case "Saboteur": //Saboteur (Ordos).
                    addDefault(spr, DirFrameSet.Unit.Saboteur, units); break;

                case "Tank": //Combat tank.
                    addTank(spr, DirFrameSet.Unit.Tank, DirFrameSet.Turret.Single, units); break;
                case "Siege Tank": //Siege tank.
                    addTank(spr, DirFrameSet.Unit.SiegeTank, DirFrameSet.Turret.Howitzer, units); break;
                case "Devastator": //Devastator (Harkonnen nuclear tank).
                    addTank(spr, DirFrameSet.Unit.Devastator, DirFrameSet.Turret.Dual, units); break;
                case "Sonic Tank": //Sonic tank (Atreides).
                    addTank(spr, DirFrameSet.Unit.SonicTank, DirFrameSet.Turret.Dish, units); break;
                case "Launcher": //Missile tank.
                    addTank(spr, DirFrameSet.Unit.Launcher, DirFrameSet.Turret.Ramp, units); break;
                case "Deviator": //Deviator (Ordos nerve gas missile launcher).
                    addTank(spr, DirFrameSet.Unit.Deviator, DirFrameSet.Turret.Ramp, units); break;

                case "Trike": //Trike.
                    addDefault(spr, DirFrameSet.Unit.Trike, units); break;
                case "Raider Trike": //Ordos raider.
                    addDefault(spr, DirFrameSet.Unit.RaiderTrike, units); break;
                case "Quad": //Quad.
                    addDefault(spr, DirFrameSet.Unit.Quad, units); break;
                case "Harvester": //Harvester.
                    addDefault(spr, DirFrameSet.Unit.Harvester, units); break;
                case "MCV": //MCV (Mobile Construction Vehicle).
                    addDefault(spr, DirFrameSet.Unit.MCV, units); break;

                case "Sandworm": //Sand worm.
                    addDefault(spr, DirFrameSet.Unit.Sandworm, units); break;

                default:
                    Program.warn(string.Format("Undefined unit sprite id '{0}'!", spr.Id)); break;
            }
        }

        private static void addAircraft(SpriteUnitD2 spr, DirFrameSet frameSet, List<SpriteUnitD2> units)
        {
            spr.mPriPlane = PriPlaneAircraft;
            spr.mFrameSet = frameSet;
            spr.mIsAircraft = true;
            units.Add(spr);
        }

        private static void addTank(SpriteUnitD2 spr, DirFrameSet frameSet, DirFrameSet turretFrameSet, List<SpriteUnitD2> units)
        {
            spr.mFrameSet = frameSet;
            spr.mTurretFrameSet = turretFrameSet;
            units.Add(spr);
        }

        private static void addDefault(SpriteUnitD2 spr, DirFrameSet frameSet, List<SpriteUnitD2> units)
        {
            spr.mFrameSet = frameSet;
            units.Add(spr);
        }

        private static Point getDrawPosCenter(TilePos tilePos, Frame frame)
        {
            Point drawPos = getDrawPos(tilePos);
            drawPos.Offset((MapD2.TileWidth / 2) - (frame.Width / 2), (MapD2.TileHeight / 2) - (frame.Height / 2));
            return drawPos;
        }

        private void drawTurret(TheaterD2 theater, IndexedImage image)
        {
            FrameRemap frame = mTurretFrameSet.getFrame(mDirection, theater);
            Point drawPos = getDrawPosCenter(TilePos, frame);
            byte[] remap = getHouseRemap(frame);
            drawPos.Offset(getDrawOffsetTurret(mTurretFrameSet, mDirection));
            image.draw(frame, drawPos, remap);
        }

        private static Point getDrawOffsetTurret(DirFrameSet turretFrameSet, Dir8Way direction)
        {
            if (turretFrameSet == DirFrameSet.Turret.Single) return getDrawOffsetTurretSingle();
            if (turretFrameSet == DirFrameSet.Turret.Howitzer) return getDrawOffsetTurretHowitzer(direction);
            if (turretFrameSet == DirFrameSet.Turret.Dual) return getDrawOffsetTurretDual(direction);
            if (turretFrameSet == DirFrameSet.Turret.Dish) return getDrawOffsetTurretDish();
            if (turretFrameSet == DirFrameSet.Turret.Ramp) return getDrawOffsetTurretRamp();
            return new Point();
        }

        private static Point getDrawOffsetTurretSingle()
        {
            //Offset the same regardless of direction. Checked in game.
            return toDrawOffset(0, 0);
        }

        private static Point getDrawOffsetTurretHowitzer(Dir8Way direction)
        {
            switch (direction)
            {
                case Dir8Way.North: return toDrawOffset(0, -5);
                case Dir8Way.Northwest: return toDrawOffset(-1, -5);
                case Dir8Way.West: return toDrawOffset(-2, -3);
                case Dir8Way.Southwest: return toDrawOffset(-2, -1);
                case Dir8Way.South: return toDrawOffset(-1, -3);
                case Dir8Way.Southeast: return toDrawOffset(2, -1);
                case Dir8Way.East: return toDrawOffset(2, -3);
                case Dir8Way.Northeast: return toDrawOffset(0, -5);
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        private static Point getDrawOffsetTurretDual(Dir8Way direction)
        {
            switch (direction)
            {
                case Dir8Way.North: return toDrawOffset(0, -4);
                case Dir8Way.Northwest: return toDrawOffset(1, -3);
                case Dir8Way.West: return toDrawOffset(-2, -4);
                case Dir8Way.Southwest: return toDrawOffset(0, -3);
                case Dir8Way.South: return toDrawOffset(-1, -3);
                case Dir8Way.Southeast: return toDrawOffset(0, -3);
                case Dir8Way.East: return toDrawOffset(2, -4);
                case Dir8Way.Northeast: return toDrawOffset(-1, -3);
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        private static Point getDrawOffsetTurretDish()
        {
            //Offset the same regardless of direction. Checked in game.
            return toDrawOffset(0, -2);
        }

        private static Point getDrawOffsetTurretRamp()
        {
            //Offset the same regardless of direction. Checked in game.
            return toDrawOffset(0, -3);
        }

        private byte[] getHouseRemap(FrameRemap frame)
        {
            //Only frames with a remap table are house colored in Dune 2. Checked in game.
            return frame.IsRemapped ? House.RemapUnit : null;
        }

        public override Rectangle getDrawBox(TheaterD2 theater)
        {
            Rectangle drawBox = getDrawBox(mFrameSet, mDirection, TilePos, theater);
            if (mTurretFrameSet != DirFrameSet.Turret.None) //Unit has a turret?
            {
                Rectangle drawBoxTurret = getDrawBox(mTurretFrameSet, mDirection, TilePos, theater);
                drawBoxTurret.Offset(getDrawOffsetTurret(mTurretFrameSet, mDirection));
                drawBox = Rectangle.Union(drawBox, drawBoxTurret);
            }
            return drawBox;
        }

        private static Rectangle getDrawBox(DirFrameSet frameSet, Dir8Way direction, TilePos tilePos, TheaterD2 theater)
        {
            FrameRemap frame = frameSet.getFrame(direction, theater);
            Point drawPos = getDrawPosCenter(tilePos, frame);
            Rectangle drawBox = frame.getBoundingBox();
            drawBox.Offset(drawPos);
            return drawBox;
        }

        public override void draw(TheaterD2 theater, IndexedImage image)
        {
            FrameRemap frame = mFrameSet.getFrame(mDirection, theater);
            Point drawPos = getDrawPosCenter(TilePos, frame);
            byte[] remap = getHouseRemap(frame);

            //Draw any aircraft shadow first.
            if (mIsAircraft)
            {
                //Always offset (1,3) and apply a darkening filter to underlying pixels.
                image.draw(frame, drawPos.getOffset(1, 3), remap, theater.getAircraftShadowFilter());

                //Aircraft shadows have the same draw priority as their owner i.e. they
                //are usually drawn over other sprites (including aircrafts) with a lower
                //draw priority. Checked in game.

                //Drawing can be a bit glitchy in the game though and sometimes (at take off?)
                //an aircraft will cast two shadows (stacked) on the ground making the shadow
                //extra dark. One shadow will also have lower priority than other units?
            }

            //Draw unit sprite frame.
            image.draw(frame, drawPos, remap);

            //Draw any turret last.
            if (mTurretFrameSet != DirFrameSet.Turret.None) //Unit has a turret?
            {
                drawTurret(theater, image);
            }
        }

        public override void drawRadar(int scale, MapD2 map, IndexedImage image)
        {
            //Aircrafts are not visible on radar.
            if (!mIsAircraft)
            {
                Rectangle dstRect = new Rectangle(TilePos.X * scale, TilePos.Y * scale, scale, scale);
                byte radarIndex = House.RadarIndex;
                if (mFrameSet == DirFrameSet.Unit.Sandworm)
                {
                    //Sandworms use index 255 (cycles between grey and white) regardless of owner. Checked in game.
                    radarIndex = 255;
                }
                image.drawRectFilled(dstRect, radarIndex);
            }
        }
    }
}
