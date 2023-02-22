using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteUnitRA : SpriteUnitTDRA
    {
        private SpriteUnitRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp, GameRA.Config.ExposeConcealed)
        {
        }

        private static SpriteUnitRA create(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
        {
            return new SpriteUnitRA(id, tilePos, fileShp);
        }

        private static SpriteUnitRA createAddSprite(SpriteUnitRA owner, FileShpSpriteSetTDRA fileShp, int frameIndex, Point drawOffset)
        {
            return createAddSprite(owner, fileShp, frameIndex, drawOffset, create);
        }

        private static SpriteUnitRA createTurretSprite(SpriteUnitRA owner, int frameIndex, Point drawOffset)
        {
            return createAddSprite(owner, owner.mFileShp, frameIndex, drawOffset);
        }

        private static SpriteUnitRA createAircraftRotorSprite(SpriteUnitRA owner, Dir8Way direction, AircraftRotors rotors, MapRA map)
        {
            return createAircraftRotorSprite(owner, direction, rotors, map, create);
        }

        private static SpriteUnitRA createAircraftShadowSprite(SpriteUnitRA owner)
        {
            return createAircraftShadowSprite(owner, create);
        }

        public static void endAdd(List<SpriteUnitRA> units, List<SpriteStructureRA> structures, MapRA map, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(units);
            if (GameRA.Config.AddHelipadAircraft) //Add aircraft to helipads?
            {
                addHelipadAircraft(structures, map, sprites);
            }

            //Refineries (PROC) get a harvester (HARV) when built during a game. They shouldn't(?) get one at start
            //(refinery added from INI-file), but I think I've seen that happen sometimes (maybe it was only in Red Alert?).
            //Bug? See "Grand_Opening()" in "BUILDING.CPP".
        }

        private static void addHelipadAircraft(List<SpriteStructureRA> structures, MapRA map, List<SpriteTDRA> sprites)
        {
            //Helipads seem to start with an aircraft (HELI or HIND?).
            mIsAddingHelipadAircrafts = true;
            List<SpriteUnitRA> aircrafts = new List<SpriteUnitRA>();
            foreach (SpriteStructureRA structure in structures)
            {
                if (structure.Structure == StructureRA.HPAD && !structure.IsBase) //Add only to normal (not base) helipads.
                {
                    HouseRA house = (HouseRA)structure.House;
                    string id = house == HouseRA.BadGuy || house == HouseRA.Ukraine || house == HouseRA.USSR ? "HIND" : "HELI";
                    addInner(house, id, structure.TilePos, Dir8Way.Northeast, ActionNull, TriggerNull, map, aircrafts);
                }
            }
            //Shift aircrafts to helipad position and add them to sprite list.
            Point drawOffsetHelipad = toDrawOffset(12, 6); //At start. Can change a bit after every landing.
            foreach (SpriteUnitRA aircraft in aircrafts)
            {
                aircraft.addDrawOffset(drawOffsetHelipad);
                sprites.Add(aircraft);
            }
            mIsAddingHelipadAircrafts = false;
            //TODO: Helipad aircraft type depends on if owner acts like good or bad side?
            //Good side get HELI and bad side get HIND, but I'm not sure how acts like is decided.
            //Assume it's same as owner house?
            //Bad side = HouseUSSR, HouseUkraine and HouseBadGuy?
            //See "Grand_Opening()" in "BUILDING.CPP".
        }

        public static void add(MapRA map, List<SpriteUnitRA> units)
        {
            add(map, map.FileIni, units);
        }

        public static void add(MapRA map, FileIni fileIni, List<SpriteUnitRA> units)
        {
            //Format: number=house,id,health,tileNumber,direction,action,trigger
            //Example: 21=USSR,V2RL,256,10532,0,Guard,None
            IniSection iniSection = fileIni.findSection("UNITS");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseRA house = HouseRA.create(values[0]);
                    string id = values[1];
                    //string health = values[2];
                    TilePos tilePos = MapRA.toTilePos(values[3]);
                    Dir8Way direction = toDir8Way(values[4]);
                    string action = values[5];
                    string trigger = values[6];

                    addInner(house, id, tilePos, direction, action, trigger, map, units);
                }
            }
        }

        private static void addInner(HouseRA house, string id, TilePos tilePos, Dir8Way direction,
            string action, string trigger, MapRA map, List<SpriteUnitRA> units)
        {
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
            SpriteUnitRA spr = new SpriteUnitRA(id, tilePos, fileShp);
            //Set values same for all units.
            spr.mColorRemap = house.ColorRemap;
            spr.mDrawOffset = getDrawOffsetCenter(spr);
            spr.mHouse = house;
            spr.mAction = action;
            spr.mTrigger = trigger;
            addInner(spr, direction, map, units);
        }

        private static void addInner(SpriteUnitRA spr, Dir8Way direction, MapRA map, List<SpriteUnitRA> units)
        {
            switch (spr.Id)
            {
                case "1TNK": //Light tank.
                    addTank(spr, direction, units); break;
                case "2TNK": //Medium tank.
                    addTank(spr, direction, units); break;
                case "3TNK": //Heavy tank.
                    addTank(spr, direction, units); break;
                case "4TNK": //Mammoth tank.
                    addTank(spr, direction, units); break;
                case "APC": //Armored personnel carrier.
                    addDefault(spr, direction, units); break;
                case "ARTY": //Artillery.
                    addDefault(spr, direction, units); break;
                case "HARV": //Ore truck.
                    addDefault(spr, direction, units); break;
                case "JEEP": //Ranger.
                    addJeep(spr, direction, units); break;
                case "MCV": //Mobile construction vehicle.
                    addDefault(spr, direction, units); break;
                case "MGG": //Mobile gap generator.
                    addMgg(spr, direction, units); break;
                case "MNLY": //Mine laying truck.
                    addDefault(spr, direction, units); break;
                case "MRJ": //Mobile radar jammer. Replaces MHQ in original (same sprites)?
                    addMrj(spr, direction, units); break;
                case "TRUK": //Convoy truck.
                    addDefault(spr, direction, units); break;
                case "V2RL": //V2 rocket launcher.
                    addDefault(spr, direction, units); break;

                case "ANT1": //Warrior ant. Counterstrike.
                case "ANT2":
                case "ANT3":
                    addAnt(spr, direction, units); break;

                case "CTNK": //Chrono tank. Aftermath.
                    addDefault(spr, direction, units); break;
                case "DTRK": //Demolition truck. Aftermath.
                    addDefault(spr, direction, units); break;
                case "QTNK": //MAD tank. Aftermath.
                    addDefault(spr, direction, units); break;
                case "STNK": //Phase transport. Aftermath. Replaces STNK (stealth tank) in original?
                    addStnk(spr, direction, units); break;
                case "TTNK": //Tesla tank. Aftermath.
                    addTtnk(spr, direction, units); break;

                case "FTNK": //Flame tank. Has SHP, but not defined? Tiberian Dawn remnant?
                    addFtnk(spr, direction, units); break;
                case "MHQ": //Mobile HQ. Has SHP, but not defined? Tiberian Dawn remnant? Replaced by MRJ?
                    addMhq(spr, direction, units); break;
                case "MLRS": //Mobile SAM launcher (SSM Launcher). Has SHP, but not defined? Tiberian Dawn remnant?
                    addMlrs(spr, direction, units); break;

                case "BADR": //Badger bomber.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "HELI": //Longbow attack helicopter.
                    addAircraft(spr, direction, AircraftRotors.Single, map, units); break;
                case "HIND": //Hind attack helicopter.
                    addAircraft(spr, direction, AircraftRotors.Single, map, units); break;
                case "MIG": //Mig attack plane.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "TRAN": //Chinook transport helicopter.
                    addAircraft(spr, direction, AircraftRotors.Dual, map, units); break;
                case "U2": //Spy plane.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;
                case "YAK": //Yak attack plane.
                    addAircraft(spr, direction, AircraftRotors.None, map, units); break;

                case "ORCA": //Orca attack helicopter. Has SHP, but not defined? Tiberian Dawn remnant? "HIND" written over it.
                    addOrca(spr, direction, map, units); break;
                case "SMIG": //Spy plane. Has SHP, but not defined? U2 used instead?
                    addSmig(spr, direction, map, units); break;

                default: //Undefined unit id.
                    addUndefined(spr, direction, units); break;
            }
        }

        private static void addTank(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteCannon(spr); //Last so owner is properly set up first.
        }

        private static void addJeep(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteGun(spr);
        }

        private static void addMgg(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteMgg(spr, direction);
        }

        private static void addMrj(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteMrj(spr);
        }

        private static void addStnk(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteStnk(spr);
        }

        private static void addTtnk(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            addDefault(spr, direction, units);
            spr.mAddSprite = getTurretSpriteTtnk(spr);
        }

        private static void addFtnk(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            //FTNK isn't defined and will not be added in the game even though it has an SHP-file. Tiberian Dawn remnant?
            warnTiberianDawnRemnant(spr.Id);
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, direction, units);
            }
        }

        private static void addMhq(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            //MHQ isn't defined and will not be added in the game even though it has an SHP-file. Tiberian Dawn remnant?
            warnTiberianDawnRemnant(spr.Id);
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, direction, units);
                spr.mAddSprite = getTurretSpriteMrj(spr); //Assume MHQ has the same turret as MRJ.
            }
        }

        private static void addMlrs(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            //MLRS isn't defined and will not be added in the game even though it has an SHP-file. Tiberian Dawn remnant?
            warnTiberianDawnRemnant(spr.Id);
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, direction, units);
                spr.mAddSprite = getTurretSpriteRamp(spr, direction);
            }
        }

        private static void addAnt(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            spr.mFrameIndex = getFrameIndexAnt(direction);
            units.Add(spr);
        }

        private static void addOrca(SpriteUnitRA spr, Dir8Way direction, MapRA map, List<SpriteUnitRA> units)
        {
            //ORCA isn't defined and will not be added in the game even though it has an SHP-file. Tiberian Dawn remnant?
            //The frames in its SHP-file have "HIND" written over them.
            warnTiberianDawnRemnant(spr.Id);
            if (GameRA.Config.AddUndefinedSprites)
            {
                addAircraft(spr, direction, AircraftRotors.None, map, units);
            }
        }

        private static void addSmig(SpriteUnitRA spr, Dir8Way direction, MapRA map, List<SpriteUnitRA> units)
        {
            //SMIG isn't defined and will not be added in the game even though it has an SHP-file.
            if (GameRA.Config.AddUndefinedSprites)
            {
                addAircraft(spr, direction, AircraftRotors.None, map, units);
            }
        }

        private static void addAircraft(SpriteUnitRA spr, Dir8Way direction, AircraftRotors rotors, MapRA map, List<SpriteUnitRA> units)
        {
            //Aircrafts aren't added to a map in the game. Only airstrips/helipads can summon them?
            //TODO: Test/check aircrafts in game. How? They aren't added to maps.
            if (GameRA.Config.AddAircraftSprites || mIsAddingHelipadAircrafts)
            {
                spr.mFrameIndex = getFrameIndexAir(spr.FrameCount, direction);
                spr.mAddSprite = createAircraftRotorSprite(spr, direction, rotors, map);
                units.Add(createAircraftShadowSprite(spr));
                units.Add(spr);
            }
        }

        private static void addDefault(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            spr.mFrameIndex = getFrameIndexDefault(direction);
            units.Add(spr);
        }

        private static void addUndefined(SpriteUnitRA spr, Dir8Way direction, List<SpriteUnitRA> units)
        {
            Program.warn(string.Format("Undefined unit sprite id '{0}'!", spr.Id));
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, direction, units);
            }
        }

        private static void warnTiberianDawnRemnant(string id)
        {
            //FTNK, MHQ, MLRS and ORCA aren't defined units and will not be added in the game
            //even though they have SHP-files. Probably Tiberian Dawn remnants?
            //Same(?) with STNK, but it is defined and used (replaced?) by phase transport in aftermath.
            Program.warn(string.Format("Tiberian Dawn unit sprite id '{0}' is not defined in Red Alert!", id));
        }

        private static int getFrameIndexAir(int sprFrameCount, Dir8Way direction)
        {
            //Some aircrafts have only 16 frames of rotation compared to the usual 32.
            int frameIndex = getFrameIndexDefault(direction);
            return sprFrameCount < 32 ? frameIndex / 2 : frameIndex; //16 or 32 frames of rotation?
        }

        private static int getFrameIndexAnt(Dir8Way direction)
        {
            //Ants have 8 frames of rotation.
            return (int)direction;
        }

        private static SpriteUnitRA getTurretSpriteCannon(SpriteUnitRA owner)
        {
            //Turret equipped units with two frame sets: 32 unit rotation and then
            //32 turret rotation (starts at index 32). 
            return createTurretSprite(owner, owner.mFrameIndex + 32, owner.mDrawOffset);
        }

        private static SpriteUnitRA getTurretSpriteGun(SpriteUnitRA owner)
        {
            //JEEP has a gun turret that is always draw offset by x=0,y=-4 regardless of direction.
            Point drawOffset = owner.mDrawOffset.getOffset(0, -4);
            return createTurretSprite(owner, owner.mFrameIndex + 32, drawOffset);
        }

        private static SpriteUnitRA getTurretSpriteRamp(SpriteUnitRA owner, Dir8Way direction)
        {
            //MLRS and MSAM have a rocket ramp that is offset depending on direction.
            //Can't check in game because these units are not defined in Red Alert.
            //MGG uses same offsets as these did in Tiberian Dawn so this is probably correct though.
            Point drawOffset = owner.mDrawOffset.getOffset(getDrawOffsetRamp(direction));
            return createTurretSprite(owner, owner.mFrameIndex + 32, drawOffset);
        }

        private static SpriteUnitRA getTurretSpriteMgg(SpriteUnitRA owner, Dir8Way direction)
        {
            //MGG "turret" is a constantly rotating radar. Frame index isn't affected by
            //direction, but its draw offset is.
            Point drawOffset = owner.mDrawOffset.getOffset(getDrawOffsetRamp(direction));
            return createTurretSprite(owner, 32, drawOffset);
        }

        private static SpriteUnitRA getTurretSpriteMrj(SpriteUnitRA owner)
        {
            //MRJ "turret" is a constantly rotating radar that isn't affected by direction.
            //Radar is always offset by x=0,y=-5. Checked in game.

            //MHQ should be the same because MRJ uses same sprites as it did.
            //Can't check in game because MHQ is not defined in Red Alert.
            return createTurretSprite(owner, 32, owner.mDrawOffset.getOffset(0, -5));
        }

        private static SpriteUnitRA getTurretSpriteStnk(SpriteUnitRA owner)
        {
            //STNK has three frame sets: 32 unit rotation, 6 loading/unloading and then
            //32 turret rotation (starts at index 38).
            return createTurretSprite(owner, owner.mFrameIndex + 38, owner.mDrawOffset);
        }

        private static SpriteUnitRA getTurretSpriteTtnk(SpriteUnitRA owner)
        {
            //TTNK "turret" is a constantly flashing dome that isn't affected by direction.
            return createTurretSprite(owner, 32, owner.mDrawOffset);
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            //"Render_Infantry()" in "RADAR.CPP" does check if "TechnoClass->Is_Visible_On_Radar()".
            //MRJ and TTNK are set ("IsStealthy=true" in "UDATA.CPP"), but this isn't what "Is_Visible_On_Radar()"
            //checks? It seems to instead check if the unit is visible and not cloaked or an ally.
            //So TTNK is visible on the radar and MRJ has a special check that makes it invisible unless
            //it's owned by the player or an ally. Checked in game.
            if (!IsSpecialEffect && //Ignore aircraft shadows.
                !(Id == "MRJ" && !(GameRA.Config.ShowInvisibleEnemies || map.isPlayerOrAlly(mHouse)))) //Ignore invisible MRJ?
            {
                Rectangle dstRect = new Rectangle(TilePos.X * scale, TilePos.Y * scale, scale, scale);
                image.drawRectFilled(dstRect, GameRA.Config.UseRadarBrightColor ? mHouse.RadarBrightIndex : mHouse.RadarIndex);
            }
        }
    }
}
