using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteUnitTD : SpriteUnitTDRA
    {
        private SpriteUnitTD(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp, GameTD.Config.ExposeConcealed)
        {
        }

        private static SpriteUnitTD create(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
        {
            return new SpriteUnitTD(id, tilePos, fileShp);
        }

        private static SpriteUnitTD createAddSprite(SpriteUnitTD owner, FileShpSpriteSetTDRA fileShp, int frameIndex, Point drawOffset)
        {
            return createAddSprite(owner, fileShp, frameIndex, drawOffset, create);
        }

        private static SpriteUnitTD createTurretSprite(SpriteUnitTD owner, int frameIndex, Point drawOffset)
        {
            return createAddSprite(owner, owner.mFileShp, frameIndex, drawOffset);
        }

        private static SpriteUnitTD createAircraftRotorSprite(SpriteUnitTD owner, Dir8Way direction, AircraftRotors rotors, MapTD map)
        {
            return createAircraftRotorSprite(owner, direction, rotors, map, create);
        }

        private static SpriteUnitTD createAircraftShadowSprite(SpriteUnitTD owner)
        {
            return createAircraftShadowSprite(owner, create);
        }

        private static SpriteUnitTD createBoatWakeSprite(SpriteUnitTD owner, MapTD map)
        {
            //BOAT (and WAKE) only has west or east rotation and it seems that it always starts going west.
            //Wake is always offset by x=-14,y=8. Checked in game.
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet("WAKE");
            int frameIndex = 6; //0 if gunboat is going east.
            SpriteUnitTD spr = createAddSprite(owner, fileShp, frameIndex, owner.mDrawOffset.getOffset(-14, 8));
            spr.setPriorityJustUnder(owner);
            spr.mIsSpecialEffect = true;
            return spr;
        }

        public static void endAdd(List<SpriteUnitTD> units, List<SpriteStructureTD> structures, MapTD map, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(units);
            if (GameTD.Config.AddHelipadAircraft) //Add aircraft to helipads?
            {
                addHelipadAircraft(structures, map, sprites);
            }

            //Refineries (PROC) get a harvester (HARV) when built during a game. They shouldn't(?) get one at start
            //(refinery added from INI-file), but I think I've seen that happen sometimes (maybe it was only in Red Alert?).
            //Bug? See "Grand_Opening()" in "BUILDING.CPP".
        }

        private static void addHelipadAircraft(List<SpriteStructureTD> structures, MapTD map, List<SpriteTDRA> sprites)
        {
            //Helipads seem to start with an aircraft (ORCA or HELI?).
            mIsAddingHelipadAircrafts = true;
            List<SpriteUnitTD> aircrafts = new List<SpriteUnitTD>();
            string health = "256"; //Default full health.
            foreach (SpriteStructureTD structure in structures)
            {
                if (structure.Structure == StructureTD.HPAD && !structure.IsBase) //Add only to normal (not base) helipads.
                {
                    HouseTD house = (HouseTD)structure.House;
                    string id = house == HouseTD.GoodGuy ? "ORCA" : "HELI";
                    addInner(house, id, health, structure.TilePos, Dir8Way.Northeast, ActionNull, TriggerNull, map, aircrafts);
                }
            }
            //Shift aircrafts to helipad position and add them to sprite list.
            Point drawOffsetHelipad = toDrawOffset(12, 6); //At start. Can change a bit after every landing.
            foreach (SpriteUnitTD aircraft in aircrafts)
            {
                aircraft.addDrawOffset(drawOffsetHelipad);
                sprites.Add(aircraft);
            }
            mIsAddingHelipadAircrafts = false;
            //TODO: Helipad aircraft type depends on if owner acts like good or bad side?
            //Good side get ORCA and bad side get HELI, but I'm not sure how acts like is decided.
            //Assume it's same as owner house?
            //Good side = HouseGoodGuy?
            //See "Grand_Opening()" in "BUILDING.CPP".
        }

        public static void add(MapTD map, List<SpriteUnitTD> units)
        {
            //Format: number=house,id,health,tileNumber,direction,action,trigger
            //Example: 004=GoodGuy,HARV,256,2309,128,Harvest,None
            IniSection iniSection = map.FileIni.findSection("UNITS");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseTD house = HouseTD.create(values[0]);
                    string id = values[1];
                    string health = values[2];
                    TilePos tilePos = MapTD.toTilePos(values[3]);
                    Dir8Way direction = toDir8Way(values[4]);
                    string action = values[5];
                    string trigger = values[6];

                    addInner(house, id, health, tilePos, direction, action, trigger, map, units);
                }
            }
        }

        private static void addInner(HouseTD house, string id, string health, TilePos tilePos, Dir8Way direction,
            string action, string trigger, MapTD map, List<SpriteUnitTD> units)
        {
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
            SpriteUnitTD spr = new SpriteUnitTD(id, tilePos, fileShp);
            //Set values same for all units.
            spr.mDrawOffset = getDrawOffsetCenter(spr);
            spr.mHouse = house;
            spr.mAction = action;
            spr.mTrigger = trigger;
            addInner(spr, health, direction, map, units);
        }

        private static void addInner(SpriteUnitTD spr, string health, Dir8Way direction, MapTD map, List<SpriteUnitTD> units)
        {
            switch (spr.Id)
            {
                case "APC": //Armored personnel carrier.
                    addFrameIndexDefault(spr, direction, units); break;
                case "ARTY": //Artillery.
                    addFrameIndexDefault(spr, direction, units); break;
                case "BGGY": //Buggy.
                    addGun(spr, direction, units); break;
                case "BIKE": //Recon bike.
                    addFrameIndexDefault(spr, direction, units); break;
                case "BOAT": //Gunboat.
                    addBoat(spr, health, direction, map, units); break;
                case "FTNK": //Flame tank.
                    addFrameIndexDefault(spr, direction, units); break;
                case "HARV": //Harvester.
                    addColorRemapAlt(spr, direction, units); break;
                case "HTNK": //Mammoth tank.
                    addTank(spr, direction, units); break;
                case "JEEP": //Humvee.
                    addGun(spr, direction, units); break;
                case "LST": //Landing hovercraft.
                    addFrameIndex0(spr, units); break;
                case "LTNK": //Light tank.
                    addTank(spr, direction, units); break;
                case "MCV": //Mobile construction vehicle.
                    addColorRemapAlt(spr, direction, units); break;
                case "MHQ": //Mobile HQ.
                    addMhq(spr, direction, units); break;
                case "MLRS": //Mobile SAM launcher (SSM Launcher). UNIT_MSAM, source code got INI name mixed up with UNIT_MLRS?
                    addRamp(spr, direction, units); break;
                case "MSAM": //Rocket launcher. UNIT_MLRS, source code got INI name mixed up with UNIT_MSAM?
                    addRamp(spr, direction, units); break;
                case "MTNK": //Medium tank.
                    addTank(spr, direction, units); break;
                case "STNK": //Stealth tank.
                    addFrameIndexDefault(spr, direction, units); break;
                case "VICE": //Visceroid.
                    addFrameIndex0(spr, units); break;

                case "RAPT": //Velociraptor. Dinosaurs.
                case "STEG": //Stegosaurus.
                case "TREX": //Tyrannosaurus rex.
                case "TRIC": //Triceratops.
                    addDinosaur(spr, direction, units); break;

                case "A10": //A-10 bomber plane.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "C17": //C-17 cargo plane.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "HELI": //Apache attack helicopter.
                    addAircraft(spr, direction, AircraftRotors.Single, map, units); break;
                case "ORCA": //Orca attack helicopter.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "TRAN": //Chinook transport helicopter.
                    addAircraft(spr, direction, AircraftRotors.Dual, map, units); break;

                default: //Undefined unit id.
                    addUndefined(spr, direction, units); break;
            }
        }

        private static void addBoat(SpriteUnitTD spr, string health, Dir8Way direction, MapTD map, List<SpriteUnitTD> units)
        {
            spr.mFrameIndex = getFrameIndexBoat(direction, toHealth(health));
            addDefault(spr, units);
            if (GameTD.Config.AddBoatWakes)
            {
                units.Add(createBoatWakeSprite(spr, map));
            }
        }

        private static void addGun(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            addFrameIndexDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteGun(spr);
        }

        private static void addMhq(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            addFrameIndexDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteMhq(spr);
        }

        private static void addRamp(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            addFrameIndexDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteRamp(spr, direction);
        }

        private static void addTank(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            addFrameIndexDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteCannon(spr);
        }

        private static void addFrameIndex0(SpriteUnitTD spr, List<SpriteUnitTD> units)
        {
            //Seems like LST and VICE isn't affected by direction. Checked in game.
            addDefault(spr, units);
        }

        private static void addFrameIndexDefault(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            spr.mFrameIndex = getFrameIndexDefault(direction);
            addDefault(spr, units);
        }

        private static void addColorRemapAlt(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            spr.mFrameIndex = getFrameIndexDefault(direction);
            spr.mColorRemap = spr.mHouse.ColorRemapAlt;
            units.Add(spr);
        }

        private static void addDinosaur(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            spr.mFrameIndex = getFrameIndexDinosaur(direction);
            addDefault(spr, units);
        }

        private static void addAircraft(SpriteUnitTD spr, Dir8Way direction, AircraftRotors rotors, MapTD map, List<SpriteUnitTD> units)
        {
            //Aircrafts aren't added to a map in the game. Only commands/helipads can summon them?
            //TODO: Test/check aircrafts in game. How? They aren't added to maps.
            if (GameTD.Config.AddAircraftSprites || mIsAddingHelipadAircrafts)
            {
                addFrameIndexDefault(spr, direction, units);
                spr.mAddSprite = createAircraftRotorSprite(spr, direction, rotors, map);
                units.Add(createAircraftShadowSprite(spr));
            }
        }

        private static void addDefault(SpriteUnitTD spr, List<SpriteUnitTD> units)
        {
            spr.mColorRemap = spr.mHouse.ColorRemap;
            units.Add(spr);
        }

        private static void addUndefined(SpriteUnitTD spr, Dir8Way direction, List<SpriteUnitTD> units)
        {
            Program.warn(string.Format("Undefined unit sprite id '{0}'!", spr.Id));
            if (GameTD.Config.AddUndefinedSprites)
            {
                addFrameIndexDefault(spr, direction, units);
            }
        }

        private static int getFrameIndexBoat(Dir8Way direction, int health)
        {
            //BOAT (and WAKE) only has west or east rotation and it seems that it always starts going west.
            //The direction controls its rocket ramp turret instead which seems to be like other units.
            //Rocket ramp has 32 frames of rotation, but direction value only selects 8 ways. Checked in game.
            //Default 8 way frame indices should be close (max 1 frame off). Hard to check though because the
            //gunboat and its ramp are constantly moving. These are the indices I got right after start:
            //North=0
            //Northwest=4 //Default is 3!
            //West=8
            //Southwest=12 //Default is 13!
            //South=16
            //Southeast=19
            //East=24
            //Northeast=29
            int frameIndex = getFrameIndexDefault(direction); //Add 96 if gunboat is going east.
            if (health < 64) //Heavily damaged?
            {
                frameIndex += 64;
            }
            else if (health < 128) //Lightly damaged.
            {
                frameIndex += 32;
            }
            return frameIndex;
        }

        private static int getFrameIndexDinosaur(Dir8Way direction)
        {
            //Dinosaurs only have 8 frames of rotation.
            return (int)direction;
        }

        private static SpriteUnitTD getTurretSpriteCannon(SpriteUnitTD owner)
        {
            //Use first set of 32 turret sprites.
            return createTurretSprite(owner, owner.mFrameIndex + 32, owner.mDrawOffset);
        }

        private static SpriteUnitTD getTurretSpriteGun(SpriteUnitTD owner)
        {
            //BGGY and JEEP have a gun turret that is offset the same regardless of direction.
            //Gun turret is always offset by x=0,y=-4. Checked in game.
            return createTurretSprite(owner, owner.mFrameIndex + 32, owner.mDrawOffset.getOffset(0, -4));
        }

        private static SpriteUnitTD getTurretSpriteRamp(SpriteUnitTD owner, Dir8Way direction)
        {
            //MLRS and MSAM have a rocket ramp that is offset depending on direction. Checked in game.
            return createTurretSprite(owner, owner.mFrameIndex + 32, owner.mDrawOffset.getOffset(getDrawOffsetRamp(direction)));
        }

        private static SpriteUnitTD getTurretSpriteMhq(SpriteUnitTD owner)
        {
            //MHQ "turret" is a constantly rotating radar that isn't affected by direction.
            //Radar is always offset by x=0,y=-5. Checked in game.
            return createTurretSprite(owner, 32, owner.mDrawOffset.getOffset(0, -5));
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            //"Render_Infantry()" in "RADAR.CPP" does not check if ObjectTypeClass->IsStealthy.
            //VICE, STNK, RAPT, STEG, TREX, TRIC are set (IsStealthy=true in "UDATA.CPP"),
            //but still visible on the radar. Checked in source and game.
            if (!IsSpecialEffect) //Ignore aircraft shadows and boat wakes.
            {
                Rectangle dstRect = new Rectangle(TilePos.X * scale, TilePos.Y * scale, scale, scale);
                if (Id == "BOAT") //Gunboat is 3 tiles wide.
                {
                    dstRect.X -= scale; //Center position.
                    dstRect.Width *= 3;
                }
                image.drawRectFilled(dstRect, mHouse.RadarBrightIndex);
            }
        }
    }
}
