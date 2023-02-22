using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteStructureTD : SpriteStructureTDRA
    {
        private StructureTD mStructure = null; //Constant data associated with structure type.

        private SpriteStructureTD(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
        }

        public StructureTD Structure
        {
            get { return mStructure; }
        }

        public override bool HasBib
        {
            get { return mStructure.HasBib; }
        }

        public bool IsWall //Wall structure to be converted to wall overlay.
        {
            get { return mStructure.IsWall; }
        }

        public override TilePos getBibPos()
        {
            //In Red Alert it seems like any draw offset will affect bib position. Not sure if
            //same in Tiberian Dawn because it has no structure with a draw offset to test with.

            //Calculate first bottom left tile not covered by drawn building.
            int tileX = TilePos.X;
            int tileY = TilePos.Y + ((Height + MapTD.TileHeight - 1) / MapTD.TileHeight);
            //A bib starts one tile up under covered tiles.
            return new TilePos(tileX, tileY - 1);
        }

        public static void endAdd(List<SpriteStructureTD> structures, MapTD map, List<SpriteTDRA> sprites)
        {
            //Sort structures into normal and base for checking rebuilt flag later.
            List<SpriteStructureTD> strNormals = new List<SpriteStructureTD>();
            List<SpriteStructureTD> strBases = new List<SpriteStructureTD>();

            foreach (SpriteStructureTD structure in structures)
            {
                //Don't add structure walls to final sprite list. Let the overlay adder deal with them.
                if (!structure.IsWall)
                {
                    if (structure.IsBase)
                    {
                        strBases.Add(structure);
                    }
                    else
                    {
                        strNormals.Add(structure);
                    }
                    sprites.Add(structure);
                }

                //Init rebuilt flag.
                structure.mIsRebuilt = false;
            }

            //Set actual value of rebuilt flag.
            setIsRebuilt(strNormals, strBases);

            //Fill silos with starting credits.
            //fillSilos(strNormals, map);
            //TODO: Do or don't fill silos? Add config option?
            //Silos are not filled at start so it's against my guideline to only draw things that's in the INI-file at start.
        }

        private static void fillSilos(List<SpriteStructureTD> strNormals, MapTD map)
        {
            //Ore silos (SILO) use a frame index depending on how much credit and storage capacity a house has.
            //At start they are empty and aren't updated until a harvester finishes unloading at a refinery.

            //Actual code to adjust silos should be something like the following. A bit hard to test if correct.
            Dictionary<HouseTDRA, int> houseCapacities = new Dictionary<HouseTDRA, int>();
            Dictionary<HouseTDRA, List<SpriteStructureTD>> houseSilos = new Dictionary<HouseTDRA, List<SpriteStructureTD>>();
            foreach (SpriteStructureTD structure in strNormals)
            {
                if (structure.mStructure == StructureTD.SILO)
                {
                    if (!houseSilos.ContainsKey(structure.mHouse))
                    {
                        houseSilos.Add(structure.mHouse, new List<SpriteStructureTD>());
                    }
                    houseSilos[structure.mHouse].Add(structure); //Add to list of silos owned by house.
                }

                int storage = structure.mStructure.Storage;
                if (storage > 0)
                {
                    if (!houseCapacities.ContainsKey(structure.mHouse))
                    {
                        houseCapacities.Add(structure.mHouse, 0);
                    }
                    houseCapacities[structure.mHouse] += storage; //Increase house's storage capacity.
                }
            }

            foreach (KeyValuePair<HouseTDRA, List<SpriteStructureTD>> house in houseSilos)
            {
                //Check that the house has storage capacity and silos.
                int houseCapacity = houseCapacities[house.Key];
                if (houseCapacity > 0 && house.Value.Count > 0)
                {
                    int startCredits = 0;
                    IniSection houseSection = map.FileIni.findSection(house.Key.Id);
                    if (houseSection != null)
                    {
                        IniKey creditsKey = houseSection.findKey("Credits");
                        if (creditsKey != null)
                        {
                            startCredits = creditsKey.valueAsInt32() * 100;
                        }
                    }

                    //Convert all credits to tiberium. Doesn't matter if more than capacity
                    //because value is clipped to 0-4 anyway.
                    int houseTiberium = startCredits;

                    //Formula same in both Tiberian Dawn and Red Alert (even still called tiberium).
                    //(HouseTiberium*5)/HouseCapacity clipped to 0-4. Checked in source.
                    int frameIndexOffset = ((houseTiberium * 5) / houseCapacity).clip(0, 4);
                    if (frameIndexOffset > 0) //Need to update silos?
                    {
                        foreach (SpriteStructureTD silo in house.Value)
                        {
                            if (silo.mFrameIndex != silo.FrameCount - 1) //Silo isn't destroyed (last frame)?
                            {
                                silo.mFrameIndex += frameIndexOffset;
                            }
                        }
                    }
                }
            }
        }

        public static void add(MapTD map, List<SpriteStructureTD> structures) //Add normal and base structures if configured so.
        {
            addNormal(map, structures);
            if (GameTD.Config.AddBaseStructures)
            {
                addBase(map, structures);
            }
        }

        public static void addNormal(MapTD map, List<SpriteStructureTD> structures)
        {
            //Format: number=house,id,health,tileNumber,direction,trigger
            //Example: 013=GoodGuy,PROC,256,1106,0,hunt
            //Direction only(?) works for GUN turret.
            IniSection iniSection = map.FileIni.findSection("STRUCTURES");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseTD house = HouseTD.create(values[0]);
                    string id = values[1];
                    int health = toHealth(values[2]);
                    TilePos tilePos = MapTD.toTilePos(values[3]);
                    string direction = values[4];
                    string trigger = values[5];

                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                    SpriteStructureTD spr = new SpriteStructureTD(id, tilePos, fileShp);
                    //Set values same for all structures.
                    //All structures, even civilian, are color remapped depending on house owner. Checked in game.
                    spr.mColorRemap = house.ColorRemapAlt;
                    spr.mHouse = house;
                    spr.mTrigger = trigger;
                    addInner(spr, health, direction, map, structures);
                }
            }
        }

        public static void addBase(MapTD map, List<SpriteStructureTD> structures)
        {
            //Format: number=id,tileNumberBase
            //Example: 008=HQ,234886400
            //Also always contain a count key e.g. Count=9.
            //Tile number base UInt32: bits 0-7 = tile position x, bits 24-31 = tile position y.
            //Structures that all AI players will try to build during game.
            IniSection iniSection = map.FileIni.findSection("Base");
            if (iniSection != null)
            {
                int count = iniSection.getKey("Count").valueAsInt32();
                if (count > 0)
                {
                    //Assume AI house is "BadGuy" unless human player is it.
                    HouseTD house = map.Player == HouseTD.BadGuy ? HouseTD.GoodGuy : HouseTD.BadGuy;
                    int health = 256; //Default full health.
                    string direction = "0"; //Default north direction.
                    byte[] colorRemap = house.ColorRemapAlt;
                    foreach (IniKey key in iniSection.Keys)
                    {
                        if (key.Id != "Count")
                        {
                            int baseNumber = key.idAsInt32();
                            if (baseNumber < count)
                            {
                                string[] values = key.Value.Split(',');
                                string id = values[0];
                                TilePos tilePos = toTilePosBase(values[1]);

                                FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                                SpriteStructureTD spr = new SpriteStructureTD(id, tilePos, fileShp);
                                //Set values same for all structures.
                                spr.mDrawMode = DrawMode.Dithered;
                                spr.mColorRemap = colorRemap;
                                spr.mHouse = house;
                                spr.mBaseNumber = baseNumber;
                                addInner(spr, health, direction, map, structures);

                                //Set priority for base structures against normal structures.
                                spr.mPriOffset.Offset(PriOffsetAddBase);
                            }
                        }
                    }
                }
            }
        }

        private static void addInner(SpriteStructureTD spr, int health, string direction, MapTD map, List<SpriteStructureTD> structures)
        {
            switch (spr.Id)
            {
                case "AFLD": //Airstrip.
                    spr.mStructure = StructureTD.AFLD; addFrameIndexDefault(spr, health, structures); break;
                case "ATWR": //Advanced guard tower.
                    spr.mStructure = StructureTD.ATWR; addFrameIndexDefault(spr, health, structures); break;
                case "BIO": //Bio-research laboratory.
                    spr.mStructure = StructureTD.BIO; addFrameIndexDefault(spr, health, structures); break;
                case "EYE": //Advanced communications center.
                    spr.mStructure = StructureTD.EYE; addFrameIndexDefault(spr, health, structures); break;
                case "FACT": //Construction yard.
                    spr.mStructure = StructureTD.FACT; addFrameIndexDefault(spr, health, structures); break;
                case "FIX": //Repair bay.
                    spr.mStructure = StructureTD.FIX; addFix(spr, health, structures); break;
                case "GTWR": //Guard tower.
                    spr.mStructure = StructureTD.GTWR; addGtwr(spr, health, structures); break;
                case "GUN": //Gun turret.
                    spr.mStructure = StructureTD.GUN; addGun(spr, health, direction, structures); break;
                case "HAND": //Hand of Nod.
                    spr.mStructure = StructureTD.HAND; addFrameIndexDefault(spr, health, structures); break;
                case "HOSP": //Hospital.
                    spr.mStructure = StructureTD.HOSP; addFrameIndexDefault(spr, health, structures); break;
                case "HPAD": //Helipad.
                    spr.mStructure = StructureTD.HPAD; addPriOffsetCenter(spr, health, structures); break;
                case "HQ": //Communications center.
                    spr.mStructure = StructureTD.HQ; addFrameIndexDefault(spr, health, structures); break;
                case "MISS": //Technology center.
                    spr.mStructure = StructureTD.MISS; addFrameIndexDefault(spr, health, structures); break;
                case "NUK2": //Advanced power plant.
                    spr.mStructure = StructureTD.NUK2; addFrameIndexDmgLast(spr, health, structures); break;
                case "NUKE": //Power plant.
                    spr.mStructure = StructureTD.NUKE; addFrameIndexDmgLast(spr, health, structures); break;
                case "OBLI": //Obelisk of light.
                    spr.mStructure = StructureTD.OBLI; addFrameIndexDefault(spr, health, structures); break;
                case "PROC": //Tiberium refinery.
                    spr.mStructure = StructureTD.PROC; addPriOffsetCenter(spr, health, structures); break;
                case "PYLE": //Barracks.
                    spr.mStructure = StructureTD.PYLE; addPriOffsetCenter(spr, health, structures); break;
                case "SAM": //SAM site.
                    spr.mStructure = StructureTD.SAM; addFrameIndexDefault(spr, health, structures); break;
                case "SILO": //Tiberium silo.
                    spr.mStructure = StructureTD.SILO; addSilo(spr, health, structures); break;
                case "TMPL": //Temple of Nod.
                    spr.mStructure = StructureTD.TMPL; addFrameIndexDmgLast(spr, health, structures); break;
                case "WEAP": //Weapons factory.
                    spr.mStructure = StructureTD.WEAP; addWeap(spr, health, map, structures); break;

                case "BARB": //Barbwire fence.
                    spr.mStructure = StructureTD.BARB; addWall(spr, structures); break;
                case "BRIK": //Concrete wall.
                    spr.mStructure = StructureTD.BRIK; addWall(spr, structures); break;
                case "CYCL": //Chain link fence.
                    spr.mStructure = StructureTD.CYCL; addWall(spr, structures); break;
                case "SBAG": //Sandbag wall.
                    spr.mStructure = StructureTD.SBAG; addWall(spr, structures); break;
                case "WOOD": //Wooden fence.
                    spr.mStructure = StructureTD.WOOD; addWall(spr, structures); break;

                case "ARCO": //Civilian oil tanker.
                    spr.mStructure = StructureTD.ARCO; addArco(spr, health, structures); break;
                case "V01": //Civilian structure.
                    spr.mStructure = StructureTD.V01; addFrameIndexDefault(spr, health, structures); break;
                case "V02": //Civilian structure.
                    spr.mStructure = StructureTD.V02; addFrameIndexDefault(spr, health, structures); break;
                case "V03": //Civilian structure.
                    spr.mStructure = StructureTD.V03; addFrameIndexDefault(spr, health, structures); break;
                case "V04": //Civilian structure.
                    spr.mStructure = StructureTD.V04; addFrameIndexDefault(spr, health, structures); break;
                case "V05": //Civilian structure.
                    spr.mStructure = StructureTD.V05; addFrameIndexDefault(spr, health, structures); break;
                case "V06": //Civilian structure.
                    spr.mStructure = StructureTD.V06; addFrameIndexDefault(spr, health, structures); break;
                case "V07": //Civilian structure.
                    spr.mStructure = StructureTD.V07; addFrameIndexDefault(spr, health, structures); break;
                case "V08": //Civilian structure.
                    spr.mStructure = StructureTD.V08; addFrameIndexDefault(spr, health, structures); break;
                case "V09": //Civilian structure.
                    spr.mStructure = StructureTD.V09; addFrameIndexDefault(spr, health, structures); break;
                case "V10": //Civilian structure.
                    spr.mStructure = StructureTD.V10; addFrameIndexDefault(spr, health, structures); break;
                case "V11": //Civilian structure.
                    spr.mStructure = StructureTD.V11; addFrameIndexDefault(spr, health, structures); break;
                case "V12": //Farmland.
                    spr.mStructure = StructureTD.V12; addFrameIndexDefault(spr, health, structures); break;
                case "V13": //Farmland.
                    spr.mStructure = StructureTD.V13; addFrameIndexDefault(spr, health, structures); break;
                case "V14": //Farmland.
                    spr.mStructure = StructureTD.V14; addFrameIndexDefault(spr, health, structures); break;
                case "V15": //Farmland.
                    spr.mStructure = StructureTD.V15; addFrameIndexDefault(spr, health, structures); break;
                case "V16": //Farmland.
                    spr.mStructure = StructureTD.V16; addFrameIndexDefault(spr, health, structures); break;
                case "V17": //Farmland.
                    spr.mStructure = StructureTD.V17; addFrameIndexDefault(spr, health, structures); break;
                case "V18": //Farmland.
                    spr.mStructure = StructureTD.V18; addFrameIndexDefault(spr, health, structures); break;
                case "V19": //Civilian oil derrick pump.
                    spr.mStructure = StructureTD.V19; addFrameIndexDefault(spr, health, structures); break;
                case "V20": //Civilian structure.
                    spr.mStructure = StructureTD.V20; addFrameIndexDefault(spr, health, structures); break;
                case "V21": //Civilian structure.
                    spr.mStructure = StructureTD.V21; addFrameIndexDefault(spr, health, structures); break;
                case "V22": //Civilian structure.
                    spr.mStructure = StructureTD.V22; addFrameIndexDefault(spr, health, structures); break;
                case "V23": //Civilian structure.
                    spr.mStructure = StructureTD.V23; addFrameIndexDefault(spr, health, structures); break;
                case "V24": //Civilian structure.
                    spr.mStructure = StructureTD.V24; addFrameIndexDefault(spr, health, structures); break;
                case "V25": //Civilian structure.
                    spr.mStructure = StructureTD.V25; addFrameIndexDefault(spr, health, structures); break;
                case "V26": //Civilian structure.
                    spr.mStructure = StructureTD.V26; addFrameIndexDefault(spr, health, structures); break;
                case "V27": //Civilian structure.
                    spr.mStructure = StructureTD.V27; addFrameIndexDefault(spr, health, structures); break;
                case "V28": //Civilian structure.
                    spr.mStructure = StructureTD.V28; addFrameIndexDefault(spr, health, structures); break;
                case "V29": //Civilian structure.
                    spr.mStructure = StructureTD.V29; addFrameIndexDefault(spr, health, structures); break;
                case "V30": //Civilian structure.
                    spr.mStructure = StructureTD.V30; addFrameIndexDefault(spr, health, structures); break;
                case "V31": //Civilian structure.
                    spr.mStructure = StructureTD.V31; addFrameIndexDefault(spr, health, structures); break;
                case "V32": //Civilian structure.
                    spr.mStructure = StructureTD.V32; addFrameIndexDefault(spr, health, structures); break;
                case "V33": //Civilian structure.
                    spr.mStructure = StructureTD.V33; addFrameIndexDefault(spr, health, structures); break;
                case "V34": //Civilian structure.
                    spr.mStructure = StructureTD.V34; addFrameIndexDefault(spr, health, structures); break;
                case "V35": //Civilian structure.
                    spr.mStructure = StructureTD.V35; addFrameIndexDefault(spr, health, structures); break;
                case "V36": //Civilian structure.
                    spr.mStructure = StructureTD.V36; addFrameIndexDefault(spr, health, structures); break;
                case "V37": //Civilian structure.
                    spr.mStructure = StructureTD.V37; addV37(spr, health, structures); break;

                default: //Undefined structure id.
                    spr.mStructure = StructureTD.Default; addUndefined(spr, health, structures); break;
            }
        }

        private static void addGun(SpriteStructureTD spr, int health, string direction, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexGun(spr, health, direction);
            addDefault(spr, structures);
            //Gun turrets are often hidden behind terrain.
            spr.mPriPlane = GameTD.Config.ExposeConcealed ? PriPlaneHigh : spr.mPriPlane;
        }

        private static void addFix(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            //FIX (repair bay) has no priority offset. Checked in source.
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            spr.mPriOffset = PriOffsetNone; //Priority offset isn't changed from default which should be none.
            structures.Add(spr);
        }

        private static void addGtwr(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            addDefault(spr, structures);
            //Guard towers are often hidden behind terrain.
            spr.mPriPlane = GameTD.Config.ExposeConcealed ? PriPlaneHigh : spr.mPriPlane;
        }

        private static void addSilo(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexSilo(spr, health);
            addDefault(spr, structures);
        }

        private static void addWeap(SpriteStructureTD spr, int health, MapTD map, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            addDefault(spr, structures);
            spr.mAddSprite = getAddSpriteWeap(spr, health, map); //Last so owner is properly set up first.
        }

        private static void addArco(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexArco(spr, health);
            addDefault(spr, structures);
        }

        private static void addV37(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            spr.mDrawOffset = toDrawOffset(1, 0);
            addDefault(spr, structures);
        }

        private static void addFrameIndexDefault(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            addDefault(spr, structures);
        }

        private static void addFrameIndexDmgLast(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDmgLast(spr, health);
            addDefault(spr, structures);
        }

        private static void addPriOffsetCenter(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, health);
            spr.mPriOffset = getPriOffsetCenter(spr);
            structures.Add(spr);
        }

        private static void addWall(SpriteStructureTD spr, List<SpriteStructureTD> structures)
        {
            //Walls are converted to and added as normal overlays.
            //Not affected by house (color remap), health or direction fields. Checked in game.
            structures.Add(spr);
        }

        private static void addDefault(SpriteStructureTD spr, List<SpriteStructureTD> structures)
        {
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addUndefined(SpriteStructureTD spr, int health, List<SpriteStructureTD> structures)
        {
            Program.warn(string.Format("Undefined structure sprite id '{0}'!", spr.Id));
            if (GameTD.Config.AddUndefinedSprites)
            {
                addFrameIndexDefault(spr, health, structures);
            }
        }

        private static TilePos toTilePosBase(string tileNumberBase)
        {
            UInt32 tn = UInt32.Parse(tileNumberBase);
            return new TilePos((int)((tn >> 8) & 0xFF), (int)(tn >> 24));
        }

        private static int getFrameIndexDefault(SpriteStructureTD spr, int health)
        {
            //Most structures have 2 sets of frames (normal, damaged) and 1 extra frame to show when destroyed.
            if (health >= 128) return 0; //Normal.
            if (health >= 1) return (spr.FrameCount - 1) / 2; //Damaged.
            return spr.FrameCount - 1; //Destroyed! Structure is removed in the game.
        }

        private static int getFrameIndexSilo(SpriteStructureTD spr, int health)
        {
            //SILO (Tiberium silo) is like default, but it also uses the destroyed frame when health is 1.
            if (health >= 128) return 0; //Normal.
            if (health >= 2) return (spr.FrameCount - 1) / 2; //Damaged.
            return spr.FrameCount - 1; //Destroyed! Structure is removed in the game if health is 0.
        }

        private static int getFrameIndexArco(SpriteStructureTD spr, int health)
        {
            //ARCO has a normal and a damaged frame. The damaged frame is used first when health is lower than 2.
            if (health >= 2) return 0; //Normal.
            return spr.FrameCount / 2; //Damaged. Structure is removed in the game if health is 0.
        }

        private static int getFrameIndexDmgLast(SpriteStructureTD spr, int health)
        {
            //Some structures always use the last frame in the damaged set for some reason.
            //They never animate when damaged even if they have frames for it. Seems like a bug?
            //TMPL (Temple of Nod), NUKE (power plant) and NUK2 (advanced power plant). Checked in game.
            if (health >= 128) return 0; //Normal.
            if (health >= 1) return spr.FrameCount - 2; //Damaged. Use last frame.
            return spr.FrameCount - 1; //Destroyed! Structure is removed in the game.
        }

        private static int getFrameIndexGun(SpriteStructureTD spr, int health, string direction)
        {
            //GUN (Gun turret) has 2 sets of frames (normal, damaged), but no extra frame to show when destroyed.
            int frameIndex = health >= 128 ? 0 : spr.FrameCount / 2; //Select set (starting index).
            return frameIndex + getFrameIndex32Dir(direction); //Add rotation to set.
        }

        private static SpriteStructureTD getAddSpriteWeap(SpriteStructureTD owner, int health, MapTD map)
        {
            //Weapons factory consists of two sprites, WEAP and WEAP2.
            if (health > 0)
            {
                FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet("WEAP2");
                SpriteStructureTD spr = new SpriteStructureTD(fileShp.Id, owner.TilePos, fileShp);
                owner.setAddSpriteDraw(spr);
                spr.mFrameIndex = health >= 128 ? 0 : fileShp.FrameCount / 2;
                return spr;
            }
            return null; //Destroyed! Structure is removed in the game.
        }

        private static Point getPriOffsetDefault(SpriteStructureTD spr)
        {
            return getPriOffsetDefault(spr.getSizeInTiles());
        }

        private static Point getPriOffsetCenter(SpriteStructureTD spr)
        {
            return getPriOffsetCenter(spr.getSizeInTiles());
        }

        public override void draw(TheaterTDRA theater, IndexedImage image)
        {
            //Don't draw rebuilt base (covered by normal structure).
            if (!(IsRebuilt && IsBase))
            {
                base.draw(theater, image);
            }
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            //"Plot_Radar_Pixel()" in "RADAR.CPP" does not check if ObjectTypeClass->IsStealthy.
            //V01-V18 are set (IsStealthy=true in "BDATA.CPP"), but still visible on the radar.
            //Checked in source and game.
            if (!IsBase) //&& mStructure.IsRadarVisible) //Game radar renderer ignores IsRadarVisible.
            {
                //Draw a filled rectangle at every tile the structure occupies.
                Rectangle rect = new Rectangle(0, 0, scale, scale);
                byte colorIndex = mHouse.RadarIndex;
                foreach (Point occupyOffset in mStructure.Occupies)
                {
                    rect.X = (TilePos.X + occupyOffset.X) * scale;
                    rect.Y = (TilePos.Y + occupyOffset.Y) * scale;
                    image.drawRectFilled(rect, colorIndex);
                }
            }
        }
    }
}
