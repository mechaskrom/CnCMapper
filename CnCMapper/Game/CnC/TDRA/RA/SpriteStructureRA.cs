using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteStructureRA : SpriteStructureTDRA
    {
        private StructureRA mStructure = null; //Constant data associated with structure type.
        private bool mHasBib = false;
        private bool mIsRepaired = false; //Repair field in INI-key is set.

        private enum Condition //Strength ratio.
        {
            Black = 0, //Dead.
            //Red,     //<=25% (default rule). Not used for anything right now so no need to include it.
            Yellow,    //<=50% (default rule).
            Green
        }

        private SpriteStructureRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
        }

        public StructureRA Structure
        {
            get { return mStructure; }
        }

        public override bool HasBib
        {
            get { return mHasBib; }
        }

        public bool IsWall //Wall structure to be converted to wall overlay.
        {
            get { return mStructure.IsWall; }
        }

        public bool IsRepaired
        {
            get { return mIsRepaired; }
        }

        public bool IsFake
        {
            get { return mStructure.IsFake; }
        }

        public override TilePos getBibPos()
        {
            //Seems like any draw offset will affect bib also. It will be placed one tile up under
            //tiles covered by drawn building. STEK (Soviet tech center) for example has a drawing
            //offset which will move its bib one tile lower than normal.

            //Calculate first bottom left tile not covered by drawn building.
            int tileX = TilePos.X + ((mDrawOffset.X + MapRA.TileWidth - 1) / MapRA.TileWidth);
            int tileY = TilePos.Y + ((Height + mDrawOffset.Y + MapRA.TileHeight - 1) / MapRA.TileHeight);
            //A bib starts one tile up under covered tiles.
            return new TilePos(tileX, tileY - 1);
        }

        public static void endAdd(List<SpriteStructureRA> structures, MapRA map, List<SpriteTDRA> sprites)
        {
            //Sort structures into normal and base for checking rebuilt flag later.
            List<SpriteStructureRA> strNormals = new List<SpriteStructureRA>();
            List<SpriteStructureRA> strBases = new List<SpriteStructureRA>();

            foreach (SpriteStructureRA structure in structures)
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
            //The "FillSilos" field is weird and seems to just fill all silos to the max.
            //Not sure if a bug or intended so I'm not really keen to implement it.
            //Silos are normally not filled at start either so it's also a bit against
            //my guideline to only draw things that's in the INI-file at start.
        }

        private static void fillSilos(List<SpriteStructureRA> strNormals, MapRA map)
        {
            //Ore silos (SILO) use a frame index depending on how much credit and storage capacity a house has.
            //At start they are empty and aren't updated until a harvester finishes unloading at a refinery.
            //However if "FillSilos=yes" in [Basic] section in a map's INI-file they are updated at the start.

            //Early return if "FillSilos=yes" isn't present.
            IniSection basicSection = map.FileIni.findSection("Basic");
            if (basicSection == null) return;
            IniKey fillSilosKey = basicSection.findKey("FillSilos");
            if (fillSilosKey == null || fillSilosKey.Value != "yes") return;

            //Because of a bug(?) all silos are filled to the max if "FillSilos=yes" so this would suffice:
            //foreach (SpriteStructureRA structure in strNormals)
            //{
            //    if (structure.mType == StructureRA.SILO)
            //    {
            //        structure.mFrameIndex += 4;
            //    }
            //}
            //return;

            //Actual code to adjust silos should be something like the following. A bit hard to test if correct.
            Dictionary<HouseTDRA, int> houseCapacities = new Dictionary<HouseTDRA, int>();
            Dictionary<HouseTDRA, List<SpriteStructureRA>> houseSilos = new Dictionary<HouseTDRA, List<SpriteStructureRA>>();
            foreach (SpriteStructureRA structure in strNormals)
            {
                int storage = map.getRulesBuilding(structure.Id).getStorage();
                if (storage > 0)
                {
                    if (!houseCapacities.ContainsKey(structure.mHouse))
                    {
                        houseCapacities.Add(structure.mHouse, 0);
                    }
                    houseCapacities[structure.mHouse] += storage; //Increase house's storage capacity.
                }

                if (structure.mStructure == StructureRA.SILO)
                {
                    if (!houseSilos.ContainsKey(structure.mHouse))
                    {
                        houseSilos.Add(structure.mHouse, new List<SpriteStructureRA>());
                    }
                    houseSilos[structure.mHouse].Add(structure); //Add to list of silos owned by house.
                }
            }

            foreach (KeyValuePair<HouseTDRA, List<SpriteStructureRA>> house in houseSilos)
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

                    //"FillSilos" (assigned to "IsMoneyTiberium" in source code) is weird.
                    //How tiberium (stored ore?) and credits (ore converted to money?) are two
                    //separate concepts is confusing and maybe I'm stupid, but it looks
                    //like the code to move available money to silos just results in a house's
                    //tiberium amount being the same as its capacity.
                    //Intentional, a bug, or am I misunderstanding something?
                    //See use of the "IsMoneyTiberium" member in "SCENARIO.CPP".
                    //From source:
                    //int toMove = houseCapacity - houseTiberium; //But house tiberium is 0 at start?
                    //houseCredits -= toMove;
                    //houseTiberium += toMove;

                    //int houseTiberium = houseCapacity; //What's happening now.
                    int houseTiberium = Math.Min(startCredits, houseCapacity); //Probably what was intended?

                    //This means that the formula below will always return 5 clipped to 4 i.e.
                    //silos are always filled to the max at start if "FillSilos=yes" which
                    //is what I'm seeing in tests. Checked in game.

                    //Formula same in both Tiberian Dawn and Red Alert (even still called tiberium).
                    //(HouseTiberium*5)/HouseCapacity clipped to 0-4. Checked in source.
                    int frameIndexOffset = ((houseTiberium * 5) / houseCapacity).clip(0, 4);
                    if (frameIndexOffset > 0) //Need to update silos?
                    {
                        foreach (SpriteStructureRA silo in house.Value)
                        {
                            silo.mFrameIndex += frameIndexOffset;
                        }
                    }
                }
            }
        }

        public static void add(MapRA map, List<SpriteStructureRA> structures) //Add normal and base structures if configured so.
        {
            addNormal(map, structures);
            if (GameRA.Config.AddBaseStructures)
            {
                addBase(map, structures);
            }
        }

        public static void addNormal(MapRA map, List<SpriteStructureRA> structures)
        {
            addNormal(map, map.FileIni, structures);
        }

        public static void addNormal(MapRA map, FileIni fileIni, List<SpriteStructureRA> structures)
        {
            //Format: number=house,id,health,tileNumber,direction,trigger,sellable,repair
            //Example: 19=USSR,BRL3,256,7632,0,None,1,0
            //Direction only(?) works for AGUN, GUN and SAM turrets.
            IniSection iniSection = fileIni.findSection("STRUCTURES");
            if (iniSection != null)
            {
                FixedMath ratioYellow = map.getRulesGeneral().getConditionYellow();
                foreach (IniKey key in iniSection.Keys)
                {
                    string[] values = key.Value.Split(',');
                    HouseRA house = HouseRA.create(values[0]);
                    string id = values[1];
                    string health = values[2];
                    TilePos tilePos = MapRA.toTilePos(values[3]);
                    string direction = values[4];
                    string trigger = values[5];
                    string repair = values[7];

                    RulesBuildingRA rules = map.getRulesBuilding(id);
                    FileShpSpriteSetTDRA fileShp = map.getFileShpFromRule(rules);
                    SpriteStructureRA spr = new SpriteStructureRA(id, tilePos, fileShp);
                    //Set values same for all structures.
                    spr.mDrawMode = getDrawMode(rules, house, map, false);
                    spr.mHasBib = rules.getHasBib();
                    spr.mHouse = house;
                    spr.mTrigger = trigger;
                    spr.mIsRepaired = toIsRepaired(repair);
                    Condition condition = getCondition(health, rules.getMaxStrength(), ratioYellow);
                    addInner(spr, condition, direction, map, structures);
                }
            }
        }

        public static void addBase(MapRA map, List<SpriteStructureRA> structures)
        {
            //Format: number=id,tileNumber
            //Example: 004=TSLA,3653
            //Also always contain a player key e.g. Player=USSR and a count key e.g. Count=9.
            //Player key specifies which AI will build the base structures during game.
            IniSection iniSection = map.FileIni.findSection("Base");
            if (iniSection != null)
            {
                int count = iniSection.getKey("Count").valueAsInt32();
                if (count > 0)
                {
                    HouseRA house = HouseRA.create(iniSection.getKey("Player").Value);
                    string direction = "0"; //Default north direction.
                    Condition condition = Condition.Green; //Default condition green.
                    foreach (IniKey key in iniSection.Keys)
                    {
                        if (key.Id != "Count" && key.Id != "Player")
                        {
                            int baseNumber = key.idAsInt32();
                            if (baseNumber < count)
                            {
                                string[] values = key.Value.Split(',');
                                string id = values[0];
                                TilePos tilePos = MapRA.toTilePos(values[1]);

                                RulesBuildingRA rules = map.getRulesBuilding(id);
                                FileShpSpriteSetTDRA fileShp = map.getFileShpFromRule(rules);
                                SpriteStructureRA spr = new SpriteStructureRA(id, tilePos, fileShp);
                                //Set values same for all structures.
                                spr.mDrawMode = getDrawMode(rules, house, map, true);
                                spr.mHasBib = rules.getHasBib();
                                spr.mHouse = house;
                                spr.mBaseNumber = baseNumber;
                                addInner(spr, condition, direction, map, structures);

                                //Set priority for base structures against normal structures.
                                spr.mPriOffset.Offset(PriOffsetAddBase);
                            }
                        }
                    }
                }
            }
        }

        private static void addInner(SpriteStructureRA spr, Condition condition, string direction, MapRA map, List<SpriteStructureRA> structures)
        {
            //TODO: PDOX (chronosphere) damaged frame index is weird in the game. Seems to cycle 21,22,23 frames?
            //Probably an animation bug because at init it starts correctly at frame 29.
            //So I'm not sure if I want to copy this behavior.

            //BARL, BRL3, V01-V37, LAR1, LAR2, BARB, BRIK, CYCL, FENC, SBAG and WOOD uses neutral color remap.
            //All other structures, including mines, use house color remap. Checked in source.

            switch (spr.Id)
            {
                case "AFLD": //Airstrip.
                    spr.mStructure = StructureRA.AFLD; addPriOffsetCenter(spr, condition, structures); break;
                case "AGUN": //Anti-aircraft gun.
                    spr.mStructure = StructureRA.AGUN; addAgun(spr, condition, direction, structures); break;
                case "APWR": //Advanced power plant.
                    spr.mStructure = StructureRA.APWR; addFrameIndexDefault(spr, condition, structures); break;
                case "ATEK": //Advanced technology center.
                    spr.mStructure = StructureRA.ATEK; addFrameIndexDefault(spr, condition, structures); break;
                case "BARL": //Barrel (one).
                    spr.mStructure = StructureRA.BARL; addBarrel(spr, condition, structures); break;
                case "BARR": //Barracks (Allied).
                    spr.mStructure = StructureRA.BARR; addPriOffsetCenter(spr, condition, structures); break;
                case "BIO": //Bio-research laboratory.
                    spr.mStructure = StructureRA.BIO; addFrameIndexAlt(spr, condition, structures); break;
                case "BRL3": //Barrels (three).
                    spr.mStructure = StructureRA.BRL3; addBarrel(spr, condition, structures); break;
                case "DOME": //Radar dome.
                    spr.mStructure = StructureRA.DOME; addFrameIndexDefault(spr, condition, structures); break;
                case "DOMF": //Fake radar dome (DOME).
                    spr.mStructure = StructureRA.DOMF; addFrameIndexDefault(spr, condition, structures); addFakeStructSign(spr, map); break;
                case "FACT": //Construction yard.
                    spr.mStructure = StructureRA.FACT; addFrameIndexDefault(spr, condition, structures); break;
                case "FACF": //Fake construction yard (FACT).
                    spr.mStructure = StructureRA.FACF; addFrameIndexDefault(spr, condition, structures); addFakeStructSign(spr, map); break;
                case "FCOM": //Forward command post.
                    spr.mStructure = StructureRA.FCOM; addFrameIndexDefault(spr, condition, structures); break;
                case "FIX": //Repair bay.
                    spr.mStructure = StructureRA.FIX; addFix(spr, condition, structures); break;
                case "FTUR": //Flame turret.
                    spr.mStructure = StructureRA.FTUR; addFrameIndexDefault(spr, condition, structures); break;
                case "GAP": //Gap generator.
                    spr.mStructure = StructureRA.GAP; addFrameIndexDefault(spr, condition, structures); break;
                case "GUN": //Gun turret.
                    spr.mStructure = StructureRA.GUN; addGun(spr, condition, direction, structures); break;
                case "HBOX": //Camouflaged pillbox.
                    spr.mStructure = StructureRA.HBOX; addHbox(spr, condition, map, structures); break;
                case "HOSP": //Hospital.
                    spr.mStructure = StructureRA.HOSP; addFrameIndexAlt(spr, condition, structures); break;
                case "HPAD": //Helipad.
                    spr.mStructure = StructureRA.HPAD; addPriOffsetCenter(spr, condition, structures); break;
                case "IRON": //Iron curtain.
                    spr.mStructure = StructureRA.IRON; addFrameIndexDefault(spr, condition, structures); break;
                case "KENN": //Dog kennel.
                    spr.mStructure = StructureRA.KENN; addFrameIndexDefault(spr, condition, structures); break;
                case "MINP": //Anti-personnel mine.
                    spr.mStructure = StructureRA.MINP; addMine(spr, map, structures); break;
                case "MINV": //Anti-vehicle mine.
                    spr.mStructure = StructureRA.MINV; addMine(spr, map, structures); break;
                case "MISS": //Technology center.
                    spr.mStructure = StructureRA.MISS; addFrameIndexAlt(spr, condition, structures); break;
                case "MSLO": //Missile silo.
                    spr.mStructure = StructureRA.MSLO; addFrameIndexDefault(spr, condition, structures); break;
                case "PBOX": //Pillbox.
                    spr.mStructure = StructureRA.PBOX; addPbox(spr, condition, structures); break;
                case "PDOX": //Chronosphere.
                    spr.mStructure = StructureRA.PDOX; addFrameIndexDefault(spr, condition, structures); break;
                case "POWR": //Power plant.
                    spr.mStructure = StructureRA.POWR; addFrameIndexDefault(spr, condition, structures); break;
                case "PROC": //Ore refinery.
                    spr.mStructure = StructureRA.PROC; addPriOffsetCenter(spr, condition, structures); break;
                case "SAM": //SAM site.
                    spr.mStructure = StructureRA.SAM; addSam(spr, condition, direction, structures); break;
                case "SILO": //Ore silo.
                    spr.mStructure = StructureRA.SILO; addFrameIndexDefault(spr, condition, structures); break;
                case "SPEN": //Submarine pen.
                    spr.mStructure = StructureRA.SPEN; addOffsetSea(spr, condition, structures); break;
                case "SPEF": //Fake submarine pen (SPEN).
                    spr.mStructure = StructureRA.SPEF; addOffsetSea(spr, condition, structures); addFakeStructSign(spr, map); break;
                case "STEK": //Soviet technology center.
                    spr.mStructure = StructureRA.STEK; addOffsetSea(spr, condition, structures); break;
                case "SYRD": //Ship yard.
                    spr.mStructure = StructureRA.SYRD; addOffsetSea(spr, condition, structures); break;
                case "SYRF": //Fake ship yard (SYRD).
                    spr.mStructure = StructureRA.SYRF; addOffsetSea(spr, condition, structures); addFakeStructSign(spr, map); break;
                case "TENT": //Barracks (Soviet).
                    spr.mStructure = StructureRA.TENT; addFrameIndexDefault(spr, condition, structures); break;
                case "TSLA": //Tesla coil.
                    spr.mStructure = StructureRA.TSLA; addFrameIndexDefault(spr, condition, structures); break;
                case "WEAP": //Weapons factory.
                    spr.mStructure = StructureRA.WEAP; addWeap(spr, condition, map, structures); break;
                case "WEAF": //Fake weapons factory (WEAP).
                    spr.mStructure = StructureRA.WEAF; addWeap(spr, condition, map, structures); addFakeStructSign(spr, map); break;

                case "LAR1": //Ant larva (one). Counterstrike.
                    spr.mStructure = StructureRA.LAR1; addAntLarva(spr, structures); break;
                case "LAR2": //Ant larvas (two). Counterstrike.
                    spr.mStructure = StructureRA.LAR2; addAntLarva(spr, structures); break;
                case "QUEE": //Ant queen. Counterstrike.
                    spr.mStructure = StructureRA.QUEE; addFrameIndexDefault(spr, condition, structures); break;

                case "BARB": //Barbwire fence (Tiberian Dawn).
                    spr.mStructure = StructureRA.BARB; addWall(spr, structures); break;
                case "BRIK": //Concrete wall.
                    spr.mStructure = StructureRA.BRIK; addWall(spr, structures); break;
                case "CYCL": //Chain link fence.
                    spr.mStructure = StructureRA.CYCL; addWall(spr, structures); break;
                case "FENC": //Barbwire fence (Soviet).
                    spr.mStructure = StructureRA.FENC; addWall(spr, structures); break;
                case "SBAG": //Sandbag wall.
                    spr.mStructure = StructureRA.SBAG; addWall(spr, structures); break;
                case "WOOD": //Wooden fence.
                    spr.mStructure = StructureRA.WOOD; addWall(spr, structures); break;

                case "V01": //Civilian structure.
                    spr.mStructure = StructureRA.V01; addV01_V18(spr, condition, structures); break;
                case "V02": //Civilian structure.
                    spr.mStructure = StructureRA.V02; addV01_V18(spr, condition, structures); break;
                case "V03": //Civilian structure.
                    spr.mStructure = StructureRA.V03; addV01_V18(spr, condition, structures); break;
                case "V04": //Civilian structure.
                    spr.mStructure = StructureRA.V04; addV01_V18(spr, condition, structures); break;
                case "V05": //Civilian structure.
                    spr.mStructure = StructureRA.V05; addV01_V18(spr, condition, structures); break;
                case "V06": //Civilian structure.
                    spr.mStructure = StructureRA.V06; addV01_V18(spr, condition, structures); break;
                case "V07": //Civilian structure.
                    spr.mStructure = StructureRA.V07; addV01_V18(spr, condition, structures); break;
                case "V08": //Civilian structure.
                    spr.mStructure = StructureRA.V08; addV01_V18(spr, condition, structures); break;
                case "V09": //Civilian structure.
                    spr.mStructure = StructureRA.V09; addV01_V18(spr, condition, structures); break;
                case "V10": //Civilian structure.
                    spr.mStructure = StructureRA.V10; addV01_V18(spr, condition, structures); break;
                case "V11": //Civilian structure.
                    spr.mStructure = StructureRA.V11; addV01_V18(spr, condition, structures); break;
                case "V12": //Farmland.
                    spr.mStructure = StructureRA.V12; addV01_V18(spr, condition, structures); break;
                case "V13": //Farmland.
                    spr.mStructure = StructureRA.V13; addV01_V18(spr, condition, structures); break;
                case "V14": //Farmland.
                    spr.mStructure = StructureRA.V14; addV01_V18(spr, condition, structures); break;
                case "V15": //Farmland.
                    spr.mStructure = StructureRA.V15; addV01_V18(spr, condition, structures); break;
                case "V16": //Farmland.
                    spr.mStructure = StructureRA.V16; addV01_V18(spr, condition, structures); break;
                case "V17": //Farmland.
                    spr.mStructure = StructureRA.V17; addV01_V18(spr, condition, structures); break;
                case "V18": //Farmland.
                    spr.mStructure = StructureRA.V18; addV01_V18(spr, condition, structures); break;
                case "V19": //Civilian oil derrick pump.
                    spr.mStructure = StructureRA.V19; addV19_V37(spr, condition, structures); break;
                case "V20": //Civilian structure.
                    spr.mStructure = StructureRA.V20; addV19_V37(spr, condition, structures); break;
                case "V21": //Civilian structure.
                    spr.mStructure = StructureRA.V21; addV19_V37(spr, condition, structures); break;
                case "V22": //Civilian structure.
                    spr.mStructure = StructureRA.V22; addV19_V37(spr, condition, structures); break;
                case "V23": //Civilian structure.
                    spr.mStructure = StructureRA.V23; addV19_V37(spr, condition, structures); break;
                case "V24": //Civilian structure.
                    spr.mStructure = StructureRA.V24; addV19_V37(spr, condition, structures); break;
                case "V25": //Civilian structure.
                    spr.mStructure = StructureRA.V25; addV19_V37(spr, condition, structures); break;
                case "V26": //Civilian structure.
                    spr.mStructure = StructureRA.V26; addV19_V37(spr, condition, structures); break;
                case "V27": //Civilian structure.
                    spr.mStructure = StructureRA.V27; addV19_V37(spr, condition, structures); break;
                case "V28": //Civilian structure.
                    spr.mStructure = StructureRA.V28; addV19_V37(spr, condition, structures); break;
                case "V29": //Civilian structure.
                    spr.mStructure = StructureRA.V29; addV19_V37(spr, condition, structures); break;
                case "V30": //Civilian structure.
                    spr.mStructure = StructureRA.V30; addV19_V37(spr, condition, structures); break;
                case "V31": //Civilian structure.
                    spr.mStructure = StructureRA.V31; addV19_V37(spr, condition, structures); break;
                case "V32": //Civilian structure.
                    spr.mStructure = StructureRA.V32; addV19_V37(spr, condition, structures); break;
                case "V33": //Civilian structure.
                    spr.mStructure = StructureRA.V33; addV19_V37(spr, condition, structures); break;
                case "V34": //Civilian structure.
                    spr.mStructure = StructureRA.V34; addV19_V37(spr, condition, structures); break;
                case "V35": //Civilian structure.
                    spr.mStructure = StructureRA.V35; addV19_V37(spr, condition, structures); break;
                case "V36": //Civilian structure.
                    spr.mStructure = StructureRA.V36; addV19_V37(spr, condition, structures); break;
                case "V37": //Civilian structure.
                    spr.mStructure = StructureRA.V37; addV19_V37(spr, condition, structures); break;

                default: //Undefined structure id.
                    spr.mStructure = StructureRA.Default; addUndefined(spr, condition, structures); break;
            }
        }

        private static void addAgun(SpriteStructureRA spr, Condition condition, string direction, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexGun(spr, condition, direction);
            addDefault(spr, structures);
        }

        private static void addGun(SpriteStructureRA spr, Condition condition, string direction, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexGun(spr, condition, direction);
            addDefault(spr, structures);
            //Gun turrets are often hidden behind terrain.
            spr.mPriPlane = GameRA.Config.ExposeConcealed ? PriPlaneHigh : spr.mPriPlane;
        }

        private static void addSam(SpriteStructureRA spr, Condition condition, string direction, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexSam(spr, condition, direction);
            addDefault(spr, structures);
        }

        private static void addHbox(SpriteStructureRA spr, Condition condition, MapRA map, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexHbox(spr, condition, map);
            addDefault(spr, structures);
            //Pillboxes are often hidden behind terrain.
            spr.mPriPlane = GameRA.Config.ExposeConcealed ? PriPlaneHigh : spr.mPriPlane;
        }

        private static void addPbox(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            addDefault(spr, structures);
            //Pillboxes are often hidden behind terrain.
            spr.mPriPlane = GameRA.Config.ExposeConcealed ? PriPlaneHigh : spr.mPriPlane;
        }

        private static void addFix(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            //FIX (repair bay) has no priority offset. Checked in source.
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            spr.mColorRemap = spr.mHouse.ColorRemap;
            spr.mPriOffset = PriOffsetNone; //Priority offset isn't changed from default which should be none.
            structures.Add(spr);
        }

        private static void addWeap(SpriteStructureRA spr, Condition condition, MapRA map, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            addDefault(spr, structures);
            spr.mAddSprite = getAddSpriteWeap(spr, condition, map); //Last so owner is properly set up first.
        }

        private static void addFrameIndexDefault(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            addDefault(spr, structures);
        }

        private static void addFrameIndexAlt(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexAlt(spr, condition);
            addDefault(spr, structures);
        }

        private static void addPriOffsetCenter(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            spr.mColorRemap = spr.mHouse.ColorRemap;
            spr.mPriOffset = getPriOffsetCenter(spr);
            structures.Add(spr);
        }

        private static void addOffsetSea(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            //Sea bound structures has a draw offset. Strangely enough, soviet technology center (STEK) also
            //has this offset. Why? Bug? Checked in game.
            spr.mFrameIndex = getFrameIndexDefault(spr, condition);
            spr.mColorRemap = spr.mHouse.ColorRemap;
            spr.mDrawOffset = toDrawOffset(0, 12);
            spr.mPriOffset = getPriOffsetSea(spr);
            structures.Add(spr);
        }

        private static void addBarrel(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            //Barrels use neutral color scheme regardless of player/house. Checked in source.
            spr.mDrawOffset = getDrawOffsetCenter(spr);
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addMine(SpriteStructureRA spr, MapRA map, List<SpriteStructureRA> structures)
        {
            //MINP and MINV isn't affected by health (even if 0). They don't even have a energy bar. Checked in game.
            //Default frame index method could be used anyway though because they only have one frame.
            spr.mColorRemap = getColorRemapMine(spr, map);
            spr.mPriOffset = getPriOffsetMines(spr);
            structures.Add(spr);
        }

        private static void addV01_V18(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            //Civilian structures use neutral color scheme regardless of player/house. Checked in game/source.
            spr.mFrameIndex = getFrameIndexCiv(spr, condition);
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addV19_V37(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            //Civilian structures use neutral color scheme regardless of player/house. Checked in game/source.

            //V20-V37 are defined in the source code, but there are no SHP-files for them? Probably Tiberian Dawn remnants?
            //They are added and are present in the map during game (you can hover the mouse over them, they block
            //pathing of units and are visible on the radar), but nothing is drawn (graphic is missing). Checked in game.
            //According to the source code they should behave similar to V19.
            spr.mFrameIndex = getFrameIndexAlt(spr, condition);
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addAntLarva(SpriteStructureRA spr, List<SpriteStructureRA> structures)
        {
            //Ant larvas use neutral color scheme regardless of player/house. Checked in source.
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addWall(SpriteStructureRA spr, List<SpriteStructureRA> structures)
        {
            //Walls BARB, BRIK, CYCL, FENC, SBAG and WOOD uses neutral color remap. Checked in source.
            //Structure walls are converted to overlay walls when map is loaded?
            //Not affected by health or direction?
            //Walls in structure section crashes the game. Only works if map is NewINIFormat=1 or 2?
            //But only NewINIFormat=3 is supported by Red Alert?
            structures.Add(spr);
        }

        private static void addFakeStructSign(SpriteStructureRA spr, MapRA map)
        {
            if (GameRA.Config.DrawFakeStructSigns)
            {
                //Add fake sign last after sprite is set up with any additional sprites
                //so the bounding box of visible pixels is correct.
                //Use sprites instead of directly drawing the sign afterwards, like other map info,
                //so that the sign is overlapped correctly if the structure is. The sign sort of
                //belongs to the structure anyway.
                spr.setAddSpriteLast(getAddSpriteFakeSign(spr, map)); //Add sign last in chain of additional sprites.
            }
        }

        private static void addDefault(SpriteStructureRA spr, List<SpriteStructureRA> structures)
        {
            spr.mColorRemap = spr.mHouse.ColorRemap;
            spr.mPriOffset = getPriOffsetDefault(spr);
            structures.Add(spr);
        }

        private static void addUndefined(SpriteStructureRA spr, Condition condition, List<SpriteStructureRA> structures)
        {
            Program.warn(string.Format("Undefined structure sprite id '{0}'!", spr.Id));
            if (GameRA.Config.AddUndefinedSprites)
            {
                addFrameIndexDefault(spr, condition, structures);
            }
        }

        private static bool toIsRepaired(string repair)
        {
            return repair != "0";
        }

        private static Condition getCondition(string health, UInt16 maxStrength, FixedMath ratioYellow)
        {
            //Health is a ratio of max strength, health/256*maxStrength i.e. 256 = 100%, 128 = 50% and so on.
            int healthVal = toHealth(health);
            UInt32 strength = FixedMath.mulFrac(maxStrength, (UInt16)healthVal, 256); //Current = maxStrength*(health/256).
            if (strength > (maxStrength - 3)) //Checked in source.
            {
                strength = maxStrength;
            }
            FixedMath ratio = FixedMath.fraction(strength, maxStrength); //Percentage of max?
            if (ratio == FixedMath.Zero) return Condition.Black; //Dead.
            if (ratio <= ratioYellow) return Condition.Yellow; //Damaged.
            return Condition.Green; //Normal.

            //TODO: Figure out why some buildings are damaged already at 129 health?
            //Some buildings are damaged (yellow energy bar) already at health 129 instead of 128 in the game.
            //E.g. BARR, BIO, FIX, HBOX, HPAD, SILO, STEK, TENT and probably a few more.
            //I can't figure out why :(. I checked fixed math result and it was the same.
            //And I set power positive for tested structures so it's probably not because
            //of damage from low power?
        }

        private static DrawMode getDrawMode(RulesBuildingRA rules, HouseRA house, MapRA map, bool isBase)
        {
            if (map.isInvisibleEnemyByRule(rules, house)) return DrawMode.Invisible; //Invisible enemy?
            else if (isBase) return DrawMode.Dithered;
            return DrawMode.Normal;
        }

        private static int getFrameIndexDefault(SpriteStructureRA spr, Condition condition)
        {
            //Most structures have 2 sets of frames (normal, damaged).
            return condition <= Condition.Yellow ? spr.FrameCount / 2 : 0;
        }

        private static int getFrameIndexAlt(SpriteStructureRA spr, Condition condition)
        {
            //BIO, HOSP, MISS, V19 have 3 sets of frames (normal, damaged, dead?).
            //The last set is not really used though because it is removed instead.
            //This anomaly is probably because these structures are remnants from Tiberian Dawn.
            if (condition >= Condition.Green) return 0; //Normal.
            if (condition > Condition.Black) return (spr.FrameCount - 1) / 2; //Damaged.
            return spr.FrameCount - 1; //Destroyed! Structure is removed in the game.
        }

        private static int getFrameIndexHbox(SpriteStructureRA spr, Condition condition, MapRA map)
        {
            //HBOX (Camouflaged pillbox) has two frame sets. The extra set is less visible (if not damaged)
            //and used by enemy houses. Doesn't matter if enemy is an ally. Checked in game.
            int frameIndex = getFrameIndexDefault(spr, condition);
            if (!GameRA.Config.ExposeEnemyPillboxes && spr.mHouse != map.Player)
            {
                frameIndex += 1; //Use less visible set.
            }
            return frameIndex;
        }

        private static int getFrameIndexCiv(SpriteStructureRA spr, Condition condition)
        {
            //Civilian structures usually have 2 frames (1 normal and 1 damaged),
            //but some have 4 frames (2 normal and 2 damaged). The game always uses
            //frame 1 when damaged though. Seems like a bug? Checked in game.
            int sprFrameCount = spr.FrameCount;
            if (sprFrameCount == 4) //Four frame civilian building?
            {
                sprFrameCount = 2; //Treat it like it only has two frames.
            }
            return condition <= Condition.Yellow ? sprFrameCount / 2 : 0;
        }

        private static int getFrameIndexSam(SpriteStructureRA spr, Condition condition, string direction)
        {
            //SAM has 68 frames in total and damaged frame set starts at 34, but the
            //game starts at 35 for some reason. Bug? Checked in source/game.
            int frameIndex = condition <= Condition.Yellow ? (spr.FrameCount / 2) + 1 : 0; //Select set (starting index).
            return frameIndex + getFrameIndex32Dir(direction); //Add rotation to set.
        }

        private static int getFrameIndexGun(SpriteStructureRA spr, Condition condition, string direction)
        {
            //AGUN and GUN has 2 sets of frames (normal, damaged). Each is 64 frames for 128 in total.
            int frameIndex = condition <= Condition.Yellow ? spr.FrameCount / 2 : 0; //Select set (starting index).
            return frameIndex + getFrameIndex32Dir(direction); //Add rotation to set.
        }

        private static byte[] getColorRemapMine(SpriteStructureRA spr, MapRA map)
        {
            //MINP and MINV are unit shadow filtered in the game. Even though they can be color remapped
            //depending on house they always use a shadow remapped neutral scheme. Checked in game.
            return GameRA.Config.UseHouseColorMines ? spr.mHouse.ColorRemap : ((TheaterRA)map.Theater).getMineRemap();
        }

        private static SpriteStructureRA getAddSpriteWeap(SpriteStructureRA owner, Condition condition, MapRA map)
        {
            //Weapons factory consists of two sprites, WEAP and WEAP2.
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet("WEAP2");
            SpriteStructureRA spr = new SpriteStructureRA(fileShp.Id, owner.TilePos, fileShp);
            owner.setAddSpriteDraw(spr);
            spr.mFrameIndex = condition <= Condition.Yellow ? fileShp.FrameCount / 2 : 0;
            return spr;
        }

        private static SpriteStructureRA getAddSpriteFakeSign(SpriteStructureRA owner, MapRA map)
        {
            //Return a sign to be placed over fake structures.
            FileShpSpriteSetTDRA fileShp = ((MapInfoDrawerRA)map.Theater.MapInfoDrawer).getFakeStructSign();
            SpriteStructureRA spr = new SpriteStructureRA(fileShp.Id, owner.TilePos, fileShp);
            //spr.mDrawMode = owner.mDrawMode; //Inherit draw mode? Don't want to dither sign if base structure?
            //Place sign in visible center (bounding box of opaque pixels) of owner structure.
            Rectangle box = owner.getBoundingBox();
            spr.mDrawOffset = owner.mDrawOffset.getOffset(
                box.X + (box.Width / 2) - (fileShp.Width / 2),
                box.Y + (box.Height / 2) - (fileShp.Height / 2));
            return spr;
        }

        private static Point getPriOffsetDefault(SpriteStructureRA spr)
        {
            return getPriOffsetDefault(spr.getSizeInTiles());
        }

        private static Point getPriOffsetCenter(SpriteStructureRA spr)
        {
            return getPriOffsetCenter(spr.getSizeInTiles());
        }

        private static Point getPriOffsetSea(SpriteStructureRA spr)
        {
            //My tests indicate that STEK, SPEN, SPEF, SYRD and SYRF have higher draw priority
            //than other structures. I can't find anything in the source explaining this though.
            //They all have that weird sea draw offset in common. And because that offset is
            //half a tile (12 pixels) I guess that the priority offset is the same (128 leptons).
            //Any draw offset will usually not affect priority though, but maybe these are special?
            //TODO: Figure out exact priority offset for sea structures? How?
            return getPriOffsetDefault(spr).getOffset(0, 128); //Assume y+128!
        }

        private static Point getPriOffsetMines(SpriteStructureRA spr)
        {
            //MINP and MINV uses the pretty complex "Move_Point()" function to offset a default center
            //priority by about -254 vertically. Checked in source.

            //Seems rather unnecessary. Why not just subtract Y by like 256 instead? I'm not 100% sure about
            //the value, but that's what I got from running the code. See "Sort_Y()" in "BUILDING.CPP".

            //return getPriOffsetCenter(spr).getOffset(movePoint(Dir8Way.North, 256)); //What the game does.
            return getPriOffsetCenter(spr).getOffset(0, -254); //Same result?
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
            //It seems to instead check if the structure is visible (affected by the invisible rule?).
            //BARL, BRL3, V01-V18, LAR1 and LAR2 are set (IsStealthy=true in "BDATA.CPP"),
            //but still visible on the radar. Checked in source and game.

            //MINP and MINV are also set, but they normally also have the invisible rule set,
            //so they are not drawn on the radar, even if owned by the player.

            //if (!IsBase) //&& mStructure.IsRadarVisible) //Game radar renderer ignores IsRadarVisible.
            if (!IsBase && (GameRA.Config.ShowRadarInvisibleByRule || !((MapRA)map).getRulesBuilding(Id).getIsInvisible()))
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
