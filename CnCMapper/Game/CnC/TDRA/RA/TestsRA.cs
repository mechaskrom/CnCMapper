using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Creates test map/mission INI-files that is used to compare output from game with.

    //The input files folder (test\RA\template) should contain these files:
    //-CONQUER src.MIX
    //-GENERAL src.MIX
    //-HIRES src.MIX
    //-INTERIOR src.MIX
    //-SNOW src.MIX
    //-TEMPERAT src.MIX
    //They are just copies from the game folder with " src" added to their names.
    //It should also contain "scg01ea.ini" which is a copy from the game, but modified
    //to fit testing better. Essentially it is a blank map with most objects stripped and set to max size.

    static class TestsRA
    {
        private const int TilesPerLine = MapRA.WidthInTiles;
        private const int StartTileNum = TilesPerLine * 4 + 4;
        private static readonly TilePos StartTilePos = new TilePos(4, 4);

        private static readonly string TestPath = Program.DebugBasePath + "test\\RA\\";
        private static readonly string TemplatePath = TestPath + "template\\"; //Files used as input (read only).
        public static readonly string PrintPath = TestPath + "print\\"; //Final output of modified files.
        private static readonly string PrintModifiedPath = TestPath + "modified\\"; //Temp output of modified files.
        private static readonly string TemplateIniPath = TemplatePath + "scg01ea.ini";

        private const string HouseGoodGuy = HouseRA.IdGoodGuy;
        private const string HouseBadGuy = HouseRA.IdBadGuy;

        //Only uses global rules. Any local rules will not be included here.
        private static readonly Dictionary<string, RulesObjectRA> mRulesObject = new Dictionary<string, RulesObjectRA>();

        //All valid sprites plus Tiberian Dawn remnants. For checking sprite type and copy-pasting from into tests.
        //Tests should use their own local sprite arrays so they are self-contained as much as possible.
        private static readonly string[] SprIdInfantries = new string[]
        {
            "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10",
            "CHAN", "DELPHI", "DOG", "E1", "E2", "E3", "E4", "E6", "E7",
            "EINSTEIN", "GNRL", "MEDI", "SPY", "THF", "MECH", "SHOK", "E5"
        };
        private static readonly string[] SprIdUnits = new string[]
        {
            "1TNK", "2TNK", "3TNK", "4TNK", "APC", "ARTY", "HARV", "JEEP",
            "MCV", "MGG", "MNLY", "MRJ", "TRUK", "V2RL", "ANT1", "ANT2", "ANT3",
            "CTNK", "DTRK", "QTNK", "STNK", "TTNK", "FTNK", "MHQ", "MLRS",
            "BADR", "HELI", "HIND", "MIG", "TRAN", "U2", "YAK", "ORCA", "SMIG"
        };
        private static readonly string[] SprIdShips = new string[]
        {
            "CA", "DD", "LST", "PT", "SS", "CARR", "MSUB"
        };
        private static readonly string[] SprIdStructures = new string[]
        {
            "AFLD", "AGUN", "APWR", "ATEK", "BARL", "BARR", "BIO", "BRL3", "DOME", "DOMF",
            "FACT", "FACF", "FCOM", "FIX", "FTUR", "GAP", "GUN", "HBOX", "HOSP", "HPAD",
            "IRON", "KENN", "MINP", "MINV", "MISS", "MSLO", "PBOX", "PDOX", "POWR", "PROC",
            "SAM", "SILO", "SPEN", "SPEF", "STEK", "SYRD", "SYRF", "TENT", "TSLA", "WEAP",
            "WEAF", "LAR1", "LAR2", "QUEE", "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD",
            "V01", "V02", "V03", "V04", "V05", "V06", "V07", "V08", "V09", "V10", "V11", "V12",
            "V13", "V14", "V15", "V16", "V17", "V18", "V19", "V20", "V21", "V22", "V23", "V24",
            "V25", "V26", "V27", "V28", "V29", "V30", "V31", "V32", "V33", "V34", "V35", "V36", "V37"
        };
        private static readonly string[] SprIdTerrains = new string[]
        {
            "BOXES01", "BOXES02", "BOXES03", "BOXES04", "BOXES05", "BOXES06", "BOXES07",
            "BOXES08", "BOXES09", "ICE01", "ICE02", "ICE03", "ICE04", "ICE05", "MINE",
            "T01", "T02", "T03", "T05", "T06", "T07", "T08", "T10", "T11", "T12", "T13",
            "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
        };
        private static readonly string[] SprIdSmudges = new string[]
        {
            "BIB1", "BIB2", "BIB3", "CR1", "CR2", "CR3", "CR4", "CR5", "CR6", "SC1", "SC2", "SC3", "SC4", "SC5", "SC6"
        };
        private static readonly string[] SprIdOverlays = new string[]
        {
            "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD", "GEM01", "GEM02", "GEM03",
            "GEM04", "GOLD01", "GOLD02", "GOLD03", "GOLD04", "V12", "V13", "V14", "V15",
            "V16", "V17", "V18", "FPLS", "SCRATE", "WCRATE", "WWCRATE"
        };

        public static void run()
        {
            List<MapRA> maps = new List<MapRA>();
            FolderContainer container = new FolderContainer(PrintPath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (fileIni.isMapTDRA())
                {
                    maps.Add(MapRA.create(fileIni));
                }
            }
            MapRA.debugSaveRenderAll(maps);
        }

        public static void print()
        {
            Directory.CreateDirectory(PrintPath);
            Directory.CreateDirectory(PrintModifiedPath);

            List<string> iniPaths = new List<string>();

            //iniPaths.Add(testStructsHealth(1));
            //iniPaths.Add(testStructsHealth(2));
            //iniPaths.Add(testStructsHealth(3));
            //iniPaths.Add(testStruct256Rots(1));
            //iniPaths.Add(testStruct256Rots(2));
            //iniPaths.Add(testStruct256Rots(3));
            //iniPaths.Add(testStruct256Rots(4));
            //iniPaths.Add(testStruct256Rots(5));
            //iniPaths.Add(testStruct256Rots(6));
            ////---------------------------------------------
            //iniPaths.Add(testInfantry256Rots());
            //iniPaths.Add(testInfantry8Rots(1));
            //iniPaths.Add(testInfantry8Rots(2));
            //iniPaths.Add(testInfantry8Rots(3));
            ////---------------------------------------------
            //iniPaths.Add(testUnit256Rots());
            //iniPaths.Add(testUnits8Rots(1));
            //iniPaths.Add(testUnits8Rots(2));
            //iniPaths.Add(testUnits8Rots(3));
            //iniPaths.Add(testUnitsHelipad());
            ////---------------------------------------------
            //iniPaths.Add(testOverlapPrio(1));
            //iniPaths.Add(testOverlapPrio(2));
            //iniPaths.Add(testOverlapPrio(3));
            //iniPaths.Add(testTerrainOverlapPrio(1));
            //iniPaths.Add(testTerrainOverlapPrio(2));
            //iniPaths.Add(testTerrainOverlapPrio(3));
            //iniPaths.Add(testTerrainOverlapPrio(4));
            //iniPaths.Add(testTerrainOverlapPrio(5));
            //iniPaths.Add(testTerrainOverlapPrio(6));
            ////---------------------------------------------
            ////OBS! Rules.ini default maximum ships is 100 and too low in game for these tests and needs to be set to >350.
            //iniPaths.Add(testShip256Rots());
            //iniPaths.Add(testShips16Rots());
            ////---------------------------------------------
            //iniPaths.Add(testColorSchemes(1));
            //iniPaths.Add(testColorSchemes(2));
            //iniPaths.Add(testRemapsAndHidden(1)); //Uses a modified "HIRES.MIX" and "CONQUER.MIX".
            //iniPaths.Add(testRemapsAndHidden(2)); //Uses a modified "HIRES.MIX" and "CONQUER.MIX".
            ////---------------------------------------------
            //iniPaths.Add(testOverlays());
            //iniPaths.Add(testOreFields()); //Uses a modified "TEMPERAT.MIX" to numerate ore sprites.
            ////---------------------------------------------
            //iniPaths.Add(testSmudgeAndOverlayOverlap(1));
            //iniPaths.Add(testSmudgeAndOverlayOverlap(2));
            //iniPaths.Add(testSmudgeOverlap(1));
            //iniPaths.Add(testSmudgeOverlap(2));
            //iniPaths.Add(testSmudgeAndBibOverlap(1));
            //iniPaths.Add(testSmudgeAndBibOverlap(2));
            ////---------------------------------------------
            //iniPaths.Add(testIniKeys());
            ////---------------------------------------------
            //iniPaths.Add(testTileSets(1));
            //iniPaths.Add(testTileSets(2));
            //iniPaths.Add(testTileSets(3));
            //iniPaths.Add(testTileSetsWeird(1)); //Uses a modified "TEMPERAT.MIX".
            //iniPaths.Add(testTileSetsWeird(2)); //Uses a modified "TEMPERAT.MIX".
            ////---------------------------------------------
            //iniPaths.Add(testRadarSprites(1));
            //iniPaths.Add(testRadarSprites(2));
            //iniPaths.Add(testRadarSprites(3));
            ////---------------------------------------------


            //saveToGeneralMix(iniPaths); //Save tests into GENERAL.MIX. Copy it to game folder to test in game.
        }

        private static string testStructsHealth(int set)
        {
            //Test some structure health values and what frame indices it produces.
            //Values higher than 256 doesn't seem to affect anything. Clamped to 256 by the game?
            int[] health = new int[]
            {
                130,
                129,
                128,
                2,
                1,
            };

            List<string> structs;
            if (set == 1)
            {
                structs = new List<string>()
                {
                    "AFLD", "AGUN", "APWR", "ATEK", "BARR", "BIO", "DOME", "FACT",
                    "FCOM", "FIX", "FTUR", "GAP", "GUN", "HBOX", "HOSP", "HPAD", "IRON",
                    "KENN", "MISS", "MSLO", "PBOX", "PDOX", "POWR", "PROC", "SAM",
                };
            }
            else if (set == 2)
            {
                structs = new List<string>()
                {
                    "SILO", "SPEN", "STEK", "SYRD", "TENT", "TSLA", "WEAP",
                };
                for (int i = 1; i <= 19; i++) //V01-V19 Temperate & Snow.
                {
                    structs.Add("V" + i.ToString().PadLeft(2, '0'));
                }
            }
            else if (set == 3)
            {
                structs = new List<string>()
                {
                    "LAR1", "LAR2", "QUEE", //Ants.
                    "DOMF", "FACF", "SPEF", "SYRF", "WEAF", //Fakes.
                };
                structs.Add("BARL"); //Special handling of explosive barrels.
                structs.Add("BRL3");
            }
            else throw new ArgumentException();

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            TheaterRA theater = TheaterRA.getTheater(ini.Theater);
            TilePos tilePos = StartTilePos;
            for (int i = 0; i < structs.Count; i++)
            {
                string structId = structs[i];
                //Add some rules. Set power produced so buildings don't get damaged because of low power.
                ini.addRule(structId, "Power", "200");

                //And give ants some strength (default 0 if not found) to prevent them from being killed.
                //Probably better to just add these rules to the template INI instead.
                //if (structId == "QUEE")
                //{
                //    ini.addRule(structId, "Strength", "800");
                //}
                //else if (structId == "LAR1")
                //{
                //    ini.addRule(structId, "Strength", "25");
                //}
                //else if (structId == "LAR2")
                //{
                //    ini.addRule(structId, "Strength", "50");
                //}

                int paddPerRow = 4;
                int paddPerCol = getSizeInTilesUp(getSpriteSize(structId, theater)).Width + 1;
                if (structId == "BARL" || structId == "BRL3")
                {
                    //Keep explosive barrels far apart so they don't damage nearby structures.
                    tilePos.X += 7;
                    paddPerRow = 7;
                    paddPerCol = 7;
                }
                TilePos blockPos = tilePos;
                for (int y = 0; y < health.Length; y++, blockPos.Y += paddPerRow)
                {
                    int tileNum = toTileNum(blockPos);
                    ini.addStructure(HouseGoodGuy, structs[i], health[y], tileNum, 0);
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
                if (tilePos.X < 96)
                {
                    tilePos.X += paddPerCol;
                }
                else //Start a new block row.
                {
                    tilePos.X = StartTilePos.X;
                    tilePos.Y += 7 * 4;
                }
            }
            return ini.save(PrintPath, "testStructsHealth set" + set);
        }

        private static string testStruct256Rots(int set)
        {
            //Test structure directions 0-257 and what frame indices it produces.
            string structId;
            int health = set < 4 ? 256 : 32; //Normal or damaged structure?
            if (set == 1 || set == 4)
            {
                structId = "AGUN";
            }
            else if (set == 2 || set == 5)
            {
                structId = "GUN";
            }
            else if (set == 3 || set == 6)
            {
                structId = "SAM";
            }
            else throw new ArgumentException();

            return sprite256Rotator(structId, (TestsIniAdderRA ini, int tileNum, int dir) =>
                ini.addStructure(HouseGoodGuy, structId, health, tileNum, dir), Size.Empty, "testStruct256Rots set" + set);
        }

        private static string testInfantry256Rots()
        {
            //Test infantry directions 0-257 and what frame indices it produces.
            string infId = "E3";
            return sprite256Rotator(infId, (TestsIniAdderRA ini, int tileNum, int dir) =>
                ini.addInfantry(HouseGoodGuy, infId, 256, tileNum, 0, dir), new Size(1, 1), "testInfantry256Rots");
        }

        private static string testInfantry8Rots(int set)
        {
            //Test infantry direction edge values (transitions between the 8 rotations).
            //7x7 tile square with 8 directions * 3 values (min, mid, max) around the edge. 1 = ignore, 257 = center.
            int[] dirs = new int[]
            {
                224,240,241,  0, 13, 14, 32,
                202,  1,  1,  1,  1,  1, 45,
                201,  1,  1,  1,  1,  1, 46,
                192,  1,  1,  257,1,  1, 64,
                174,  1,  1,  1,  1,  1, 73,
                173,  1,  1,  1,  1,  1, 74,
                160,142,141,128,113,112, 96,
            };

            string[] infs;
            if (set == 1)
            {
                infs = new string[] { "DOG", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "GNRL", "MEDI" };
            }
            else if (set == 2)
            {
                infs = new string[] { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10" };
            }
            else if (set == 3)
            {
                infs = new string[] { "CHAN", "DELPHI", "EINSTEIN", "MECH", "SHOK", "SPY", "THF" };
            }
            else throw new Exception();

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            for (int i = 0; i < infs.Length; i++)
            {
                //Cram infantry together so you can quickly screenshot them before they start moving.
                TilePos blockPos = new TilePos(4 + ((i % 4) * 4), 4 + ((i / 4) * 4)); //4x4 tile blocks. 4 per row.
                for (int y = 0; y < 7; y++)
                {
                    for (int x = 0; x < 7; x++)
                    {
                        int dir = dirs[(y * 7) + x];
                        if (dir != 1)
                        {
                            int tileNum = toTileNum(blockPos, x / 2, y / 2);
                            if (dir == 257) //Remove fog of war and test all sub pos draw offsets?
                            {
                                //Add center infantry with sub pos 0.
                                ini.addInfantry(HouseGoodGuy, infs[i], 256, tileNum, 0, 128);
                                //Surround it with scouts to remove fog of war from block.
                                for (int j = 1; j < 5; j++)
                                {
                                    ini.addInfantry(HouseGoodGuy, "E1", 256, tileNum, j, 128);
                                }
                            }
                            else
                            {
                                int subPos = (1 + (x % 2)) + (2 * (y % 2));
                                ini.addInfantry(HouseGoodGuy, infs[i], 256, tileNum, subPos, dir);
                            }
                        }
                    }
                }
            }
            return ini.save(PrintPath, "testInfantry8Rots set" + set);
        }

        private static string testUnit256Rots()
        {
            //Test unit directions 0-257 and what frame indices it produces.
            string unitId = "JEEP";
            return sprite256Rotator(unitId, (TestsIniAdderRA ini, int pos, int dir) =>
                ini.addUnit(HouseGoodGuy, unitId, 256, pos, dir), Size.Empty, "testUnit256Rots");
        }

        private static string testUnits8Rots(int set)
        {
            //Test unit direction edge values (transitions between the 8 rotations).
            //7x7 tile square with 8 directions * 3 values (min, mid, max) around the edge. 1 = ignore, 257 = center.
            int[] dirs = new int[]
            {
                224,239,240,  0, 15, 16, 32,
                208,  1,  1,  1,  1,  1, 47,
                207,  1,  1,  1,  1,  1, 48,
                192,  1,  1,  257,1,  1, 64,
                176,  1,  1,  1,  1,  1, 79,
                175,  1,  1,  1,  1,  1, 80,
                160,144,143,128,112,111, 96,
            };

            string[] us;
            if (set == 1)
            {
                us = new string[] { "1TNK", "2TNK", "3TNK", "4TNK", "APC", "ARTY", "HARV", "JEEP", "MCV", "MGG", "MNLY" };
            }
            else if (set == 2)
            {
                us = new string[] { "MRJ", "TRUK", "V2RL", "FTNK", "MHQ", "MLRS", "ANT1", "ANT2", "ANT3", "CTNK", "DTRK" };
            }
            else if (set == 3)
            {
                //Aircrafts are not shown in the game. Only airstrips/helipads can summon them?
                us = new string[] { "QTNK", "STNK", "TTNK", "BADR", "HELI", "HIND", "MIG", "ORCA", "SMIG", "TRAN", "U2", "YAK" };
            }
            else throw new ArgumentException();

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            for (int i = 0; i < us.Length; i++)
            {
                TilePos blockPos = StartTilePos.getOffset((i % 5) * 10, (i / 5) * 10); //10x10 tile blocks. 5 per row.
                for (int y = 0; y < 7; y++)
                {
                    for (int x = 0; x < 7; x++)
                    {
                        int dir = dirs[(y * 7) + x];
                        if (dir != 1)
                        {
                            int tileNum = toTileNum(blockPos, x, y);
                            ini.addUnit(HouseGoodGuy, us[i], 256, tileNum, dir);
                        }
                        if (dir == 257) //Center?
                        {
                            //Add scouts below and right of block.
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 8, 3), 128);
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 3, 8), 128);
                        }
                    }
                }
            }
            return ini.save(PrintPath, "testUnits8Rots set" + set);
        }

        private static string testUnitsHelipad()
        {
            //Test what aircraft unit a helipad gets with different houses.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            string[] houses = new string[]
            {
                HouseRA.IdBadGuy, HouseRA.IdEngland, HouseRA.IdFrance, HouseRA.IdGermany,
                HouseRA.IdGoodGuy, HouseRA.IdGreece, HouseRA.IdMulti1, HouseRA.IdMulti2,
                HouseRA.IdMulti3, HouseRA.IdMulti4, HouseRA.IdMulti5, HouseRA.IdMulti6,
                HouseRA.IdMulti7, HouseRA.IdMulti8, HouseRA.IdNeutral, HouseRA.IdSpain,
                HouseRA.IdSpecial, HouseRA.IdTurkey, HouseRA.IdUkraine, HouseRA.IdUSSR
            };
            TilePos tilePos = StartTilePos;
            for (int i = 0; i < houses.Length; i++)
            {
                string house = houses[i];
                tilePos.X = (i % 6) * 3 + StartTilePos.X;
                tilePos.Y = (i / 6) * 3 + StartTilePos.Y;
                ini.addStructure(house, "HPAD", 256, toTileNum(tilePos), 0);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 0), 0);
            }
            return ini.save(PrintPath, "testUnitsHelipad");
        }

        private static string testOverlapPrio(int set)
        {
            //Test draw priority of some overlapping sprites.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            string cmpId;
            List<string> sprites = new List<string>();
            if (set == 1 || set == 2) //Temperate or snow T11.
            {
                ini.Theater = set == 1 ? "TEMPERATE" : "SNOW";
                cmpId = "T11";
                //Add some snow/temperate terrain.
                sprites.AddRange(new string[]
                {
                    "ICE01", "ICE02", "ICE03", "ICE04", "ICE05", "MINE", "T01", "T02",
                    "T03", "T05", "T06", "T07", "T08", "T10", "T11", "T12", "T13",
                    "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
                });
            }
            else if (set == 3) //Interior.
            {
                ini.Theater = "INTERIOR";
                cmpId = "BOXES01";
                //Add some interior terrain.
                sprites.AddRange(new string[]
                {
                    "BOXES01", "BOXES02", "BOXES03", "BOXES04", "BOXES05", "BOXES06", "BOXES07", "BOXES08", "BOXES09"
                });
            }
            else throw new ArgumentException();

            //Add structures.
            sprites.AddRange(new string[]
            {
                "AFLD", "AGUN", "APWR", "ATEK", "BARL", "BARR", "BIO", "BRL3", "DOME", "DOMF", "FACT",
                "FACF", "FCOM", "FIX", "FTUR", "GAP", "GUN", "HBOX", "HOSP", "HPAD", "IRON", "KENN",
                "MINP", "MINV", "MISS", "MSLO", "PBOX", "PDOX", "POWR", "PROC", "SAM", "SILO", "SPEN",
                "SPEF", "STEK", "SYRD", "SYRF", "TENT", "TSLA", "WEAP", "WEAF", "LAR1", "LAR2", "QUEE"
            });

            TheaterRA theater = TheaterRA.getTheater(ini.Theater);
            Size cmpTiles = getSizeInTilesUp(getSpriteSize(cmpId, theater));
            TilePos tilePos = StartTilePos;
            int overlapTiles = 2; //How many tiles should compare sprite overlap with test sprite?
            int maxTilesWidth = cmpTiles.Width;
            foreach (string sprId in sprites)
            {
                Size sprTiles = getSizeInTilesUp(getSpriteSize(sprId, theater));
                int maxTilesHeight = Math.Max(sprTiles.Height, cmpTiles.Height);

                //Add current sprite.
                int sprTileNum = toTileNum(tilePos, 0, maxTilesHeight - sprTiles.Height);
                if (isStructure(sprId)) //Structure?
                {
                    ini.addStructure(HouseGoodGuy, sprId, 256, sprTileNum, 0);
                }
                else if (isTerrain(sprId)) //Terrain?
                {
                    ini.addTerrain(sprId, sprTileNum);
                }
                else if (isUnit(sprId)) //Unit?
                {
                    ini.addUnit(HouseGoodGuy, sprId, 256, sprTileNum, 0);
                }
                else
                {
                    throw new Exception();
                }

                //Add compare terrain sprite directly to the right of current sprite.
                int cmpTileNum = toTileNum(tilePos,
                    Math.Max(1, sprTiles.Width - overlapTiles), maxTilesHeight - cmpTiles.Height);
                ini.addTerrain(cmpId, cmpTileNum);

                //Add a JEEP left of current position to remove fog of war there.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, sprTileNum - 1, 0);

                maxTilesWidth = Math.Max(maxTilesWidth, sprTiles.Width + cmpTiles.Width - overlapTiles);
                if (tilePos.Y <= 51) //Keep going down column.
                {
                    tilePos.Y += maxTilesHeight + 1;
                }
                else //Start new column.
                {
                    tilePos.Y = StartTilePos.Y;
                    tilePos.X += maxTilesWidth + 2;
                    maxTilesWidth = 0;
                }
            }
            return ini.save(PrintPath, "testOverlapPrio set" + set);
        }

        private static string testTerrainOverlapPrio(int set)
        {
            //Test draw priority of overlapping terrain sprites.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            //Many sprites to compare so split sprite-list in chunks that will fit on map.
            int startInd = 0;
            int endInd = 0;
            List<string> sprites = new List<string>();
            //Add some snow/temperate terrain.
            sprites.AddRange(new string[]
                {
                    "ICE01", "ICE02", "ICE03", "ICE04", "ICE05", "MINE", "T01", "T02",
                    "T03", "T05", "T06", "T07", "T08", "T10", "T11", "T12", "T13",
                    "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
                });
            if (set == 1 || set == 2 || set == 3) //Temperate.
            {
                ini.Theater = "TEMPERATE";
                //Temperate terrain will fit in three chunks.
                startInd = (int)(sprites.Count / 3.0 * (set - 1) + 0.75); //Prefer rounding up.
                endInd = (int)(sprites.Count / 3.0 * (set - 0) + 0.75);
            }
            else if (set == 4 || set == 5 || set == 6) //Snow.
            {
                ini.Theater = "SNOW";
                //Snow terrain will fit in three chunks.
                startInd = (int)(sprites.Count / 3.0 * (set - 4) + 0.75); //Prefer rounding up.
                endInd = (int)(sprites.Count / 3.0 * (set - 3) + 0.75);
            }
            //No need to test interior because it only have a few small boxes as terrain.
            else throw new ArgumentException();

            //Add MSLO structure as an additional sprite to compare terrain with.
            //But don't include it with terrain sprites i.e. calculate start/end index before adding it.
            sprites.Add("MSLO");

            TheaterRA theater = TheaterRA.getTheater(ini.Theater);
            TilePos tilePos = StartTilePos;
            int maxTilesWidth = 0;
            for (int j = startInd; j < endInd; j++) //Terrains to compare excluding MSLO structure last in list.
            {
                string cmpId = sprites[j];
                Size cmpTiles = getSizeInTilesNear(getSpriteSize(sprites[j], theater));
                for (int i = 0; i < sprites.Count; i++) //Terrains to compare including MSLO structure last in list.
                {
                    string sprId = sprites[i];
                    Size sprTiles = getSizeInTilesNear(getSpriteSize(sprites[i], theater));
                    int maxTilesHeight = Math.Max(cmpTiles.Height, sprTiles.Height);

                    //Add compare terrain sprite.
                    int cmpTileNum = toTileNum(tilePos, 0, maxTilesHeight - cmpTiles.Height);
                    if (isTerrain(cmpId)) //Terrain?
                    {
                        ini.addTerrain(cmpId, cmpTileNum);
                    }
                    else
                    {
                        throw new Exception();
                    }

                    //Add current sprite directly to the right of compare sprite.
                    int sprTileNum = toTileNum(tilePos, 1, maxTilesHeight - sprTiles.Height);
                    if (isStructure(sprId)) //Structure?
                    {
                        ini.addStructure(HouseGoodGuy, sprId, 256, sprTileNum, 0);
                    }
                    else if (isTerrain(sprId)) //Terrain?
                    {
                        ini.addTerrain(sprId, sprTileNum);
                    }
                    else
                    {
                        throw new Exception();
                    }

                    //Add a JEEP left of current position to remove fog of war there.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, cmpTileNum - 1, 0);

                    maxTilesWidth = Math.Max(maxTilesWidth, Math.Max(cmpTiles.Width, sprTiles.Width));
                    if (tilePos.Y <= 51) //Keep going down column.
                    {
                        tilePos.Y += maxTilesHeight + 1;
                    }
                    else //Start new column.
                    {
                        tilePos.Y = StartTilePos.Y;
                        tilePos.X += maxTilesWidth + 2;
                        maxTilesWidth = 0;
                    }
                }
            }
            return ini.save(PrintPath, "testTerrainOverlapPrio set" + set);
        }

        private static string testShip256Rots()
        {
            //Test ship directions 0-257 and what frame indices it produces.
            string shipId = "DD";
            return sprite256Rotator(shipId, (TestsIniAdderRA ini, int pos, int dir) =>
                ini.addShip(HouseGoodGuy, shipId, 256, pos, dir), new Size(2, 2), "testShip256Rots");
        }

        private static string testShips16Rots()
        {
            //Test ship direction edge values (transitions between the 16 rotations).
            //13x13 tile square with 16 directions * 3 values (min, mid, max) around the edge. 1 = ignore, 257 = center.
            int[] dirs = new int[]
            {
                224,231,232,240,247,248,  0,  7,  8, 16, 23, 24, 32,
                216,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 39,
                215,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 40,
                208,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 48,
                200,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 55,
                199,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 56,
                192,  1,  1,  1,  1,  1,257,  1,  1,  1,  1,  1, 64,
                184,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 71,
                183,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 72,
                176,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 80,
                168,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 87,
                167,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 88,
                160,152,151,144,136,135,128,120,119,112,104,103, 96,
            };

            string[] ss = new string[] { "SS", "MSUB", "PT", "CA", "DD", "LST", "CARR" };
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            for (int i = 0; i < ss.Length; i++)
            {
                TilePos blockPos = new TilePos(5 + ((i % 3) * 15), 5 + ((i / 3) * 15)); //15x15 tile blocks. 3 per row.
                for (int y = 0; y < 13; y++)
                {
                    for (int x = 0; x < 13; x++)
                    {
                        int dir = dirs[(y * 13) + x];
                        if (dir == 257) //Center?
                        {
                            //Add scouts to block.
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 10, 2), 128);
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 2, 2), 128);
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 6, 6), 128);
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 2, 10), 128);
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 10, 10), 128);
                        }
                        else if (dir != 1)
                        {
                            int tileNum = toTileNum(blockPos, x, y);
                            ini.addShip(HouseGoodGuy, ss[i], 256, tileNum, dir);
                        }
                    }
                }
            }
            return ini.save(PrintPath, "testShips16Rots");
        }

        private static string testColorSchemes(int set)
        {
            //Test house color schemes. Red Alert doesn't have alternative
            //color schemes for some units like Tiberian Dawn had.
            string[] ids = new string[] {
                "DOG","E1","E2","E3","E4","E6","E7","GNRL","MEDI","SPY","THF","MECH",
                "SHOK","1TNK","2TNK","3TNK","4TNK","APC","ARTY","HARV","JEEP","MCV",
                "MGG","MNLY","MRJ","TRUK","V2RL","ANT1","ANT2","ANT3","CTNK","DTRK",
                "QTNK","STNK","TTNK","CA","DD","LST","PT","SS","CARR","MSUB"};

            string[] houses;
            if (set == 1)
            {
                houses = new string[]{
                    "GoodGuy","England","Germany","France","Ukraine","USSR","Greece","Turkey","Spain","BadGuy"};
            }
            else if (set == 2)
            {
                houses = new string[]{
                    "Neutral","Special","Multi1","Multi2","Multi3","Multi4","Multi5","Multi6","Multi7","Multi8"};
            }
            else throw new ArgumentException();

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            ini.Theater = "INTERIOR"; //Use interior because units can't move on its clear tiles.
            for (int i = 0; i < ids.Length; i++)
            {
                //Disable weapons for all units so they don't start killing each other.
                //Setting allies doesn't seem to work?
                ini.addRule(ids[i], "Primary", "none");
                ini.addRule(ids[i], "Secondary", "none");
            }
            TilePos blockPos = new TilePos(5, 5);
            for (int j = 0; j < houses.Length; j++, blockPos.Y += 4)
            {
                TilePos tilePos = blockPos;
                for (int i = 0; i < ids.Length; i++, tilePos.X++)
                {
                    string sprId = ids[i];
                    int tileNum = toTileNum(tilePos);
                    if (isUnit(sprId))
                    {
                        ini.addUnit(houses[j], sprId, 256, tileNum, 128);
                    }
                    else if (isInfantry(sprId))
                    {
                        ini.addInfantry(houses[j], sprId, 256, tileNum, 0, 128);
                    }
                    else if (isShip(sprId))
                    {
                        ini.addShip(houses[j], sprId, 256, tileNum, 128);
                    }

                    if (tilePos.X % 5 == 0)
                    {
                        ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, 0, 2), 128);
                    }
                }
            }
            return ini.save(PrintPath, "testColorSchemes set" + set);
        }

        private static string testRemapsAndHidden(int set)
        {
            //Test palette remaps (house color scheme, shadow unit/air) and hidden/disguised sprites.
            //Uses a modified "HIRES.MIX" and "CONQUER.MIX".
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1)
            {
                ini.Theater = "TEMPERATE";
            }
            else if (set == 2)
            {
                ini.Theater = "SNOW";
            }
            else throw new ArgumentException();

            //Disable weapons for scouts so they don't start killing things.
            ini.addRule("JEEP", "Primary", "none");
            ini.addRule("JEEP", "Secondary", "none");

            //Test C1-C10 color remaps (use modified C1 and C2 SHP-files with a full palette in them).
            TilePos blockPos = StartTilePos;
            for (int i = 0; i < 10; i++)
            {
                TilePos tilePos = blockPos.getOffset(i * 2, 0); //1 row.
                ini.addInfantry(HouseGoodGuy, "C" + (i + 1).ToString(), 256, toTileNum(tilePos), 0, 128);
                if (i % 2 == 0)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, 0, 2), 128); //Add scout.
                }
            }

            //Test house color schemes and unit shadow remap by overlapping AGUN (use modified AGUN SHP-file
            //with full palette and solid frame in it).
            blockPos.Y += 3;
            string[] houses = new string[]{
                "GoodGuy","England","Germany","France","Ukraine","USSR","Greece","Turkey","Spain","BadGuy",
                "Neutral","Special","Multi1","Multi2","Multi3","Multi4","Multi5","Multi6","Multi7","Multi8"};
            for (int i = 0; i < houses.Length; i++)
            {
                TilePos tilePos = blockPos.getOffset((i % 10) * 2, (i / 10) * 4); //2x10 rows.
                ini.addStructure(houses[i], "AGUN", 256, toTileNum(tilePos, 0, 0), 0);
                ini.addStructure(houses[i], "AGUN", 256, toTileNum(tilePos, 0, 1), 0);
                if (i % 2 == 0)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, 1, 1), 128); //Add scout.
                }
            }

            //Add a helipad so we can do some air shadow testing also. (use modified HELI SHP-file with
            //solid color and HPAD SHP-file with full palette in it.
            blockPos.Y += 8;
            ini.addStructure(HouseGoodGuy, "HPAD", 256, toTileNum(blockPos), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, -1, -1), 128); //Add scout.

            //Add some hidden/disguised sprites (HBOX, MINP, MINV and SPY) to see how owned by enemy affects them.
            blockPos.Y += 4;
            //Disable weapons.
            ini.addRule("STNK", "Primary", "none");
            ini.addRule("STNK", "Secondary", "none");
            {
                TilePos tilePos = blockPos;
                string[] hidden = new string[] { "HBOX", "MINP", "MINV" };
                ini.addInfantry(HouseGoodGuy, "SPY", 256, toTileNum(tilePos, 0, 0), 0, 0);
                ini.addUnit(HouseGoodGuy, "STNK", 256, toTileNum(tilePos, 0, 1), 0); //Add scout.
                ini.addInfantry(HouseBadGuy, "SPY", 256, toTileNum(tilePos, 0, 7), 0, 0);
                tilePos.X += 6;
                for (int i = 0; i < hidden.Length; i++, tilePos.X += 6)
                {
                    ini.addStructure(HouseGoodGuy, hidden[i], 64, toTileNum(tilePos, 0, 0), 0); //Damaged.
                    ini.addStructure(HouseGoodGuy, hidden[i], 256, toTileNum(tilePos, 1, 0), 0); //Normal.
                    ini.addUnit(HouseGoodGuy, "STNK", 256, toTileNum(tilePos, 0, 1), 0); //Add scout.
                    ini.addStructure(HouseBadGuy, hidden[i], 64, toTileNum(tilePos, 0, 7), 0); //Damaged.
                    ini.addStructure(HouseBadGuy, hidden[i], 256, toTileNum(tilePos, 1, 7), 0); //Normal.
                }
            }

            //Modify files used in this test.
            //HIRES: "C1", "C2"
            FileMixArchiveWw.Editor hiresEditor = FileMixArchiveWw.Editor.open(TemplatePath + "HIRES src.MIX");
            writeModifiedC1C2(hiresEditor);
            hiresEditor.save(PrintPath + "HIRES.MIX");

            //CONQUER: "AGUN", "BRIK", "HELI", "HPAD", "MINP", "MINV"
            FileMixArchiveWw.Editor conquerEditor = FileMixArchiveWw.Editor.open(TemplatePath + "CONQUER src.MIX");
            writeModifiedAgun(conquerEditor);
            writeModifiedBrik(conquerEditor);
            writeModifiedHeli(conquerEditor);
            writeModifiedHpad(conquerEditor);
            writeModifiedMinp(conquerEditor);
            writeModifiedMinv(conquerEditor);
            conquerEditor.save(PrintPath + "CONQUER.MIX");

            return ini.save(PrintPath, "testRemapsAndHidden set" + set);
        }

        private static string testOverlays()
        {
            //Test how overlay sprites behave, especially those that are also valid structure sprites (walls and farmlands).
            //Also test what happens if a known id is put in the "wrong" section e.g. "ARTY"-unit in "[STRUCTURES]".
            //Can't really test overlapping overlays in Red Alert because they are stored per tile (overlay pack).
            //And it can only(?) handle maps with "NewINIFormat=3" i.e. can't switch to old overlay format.

            //Walls in "[STRUCTURES]" crashes the game when map is loaded. Not sure if I'm doing something wrong
            //or if they just arent supported any longer? Wall structures worked in Tiberian Dawn at least.
            //I'm guessing they only work if the old overlay format is used (NewINIFormat=1 or 2)?
            //Many of the next tests adapted from Tiberian Dawn can't be performed any longer because of this problem.
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);

            //Try all defined overlay sprites.
            TilePos blockPos = StartTilePos;
            ini.addUnit(HouseGoodGuy, "2TNK", 256, toTileNum(StartTilePos, -2, 2), 0); //Add some tanks to try damaging overlays.
            ini.addUnit(HouseGoodGuy, "2TNK", 256, toTileNum(StartTilePos, -2, 3), 0);
            string[] ovls = new string[] { "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD", "GEM01", "GEM02", "GEM03", "GEM04",
                "GOLD01", "GOLD02", "GOLD03", "GOLD04", "V12", "V13", "V14", "V15", "V16", "V17", "V18",
                "FPLS", "SCRATE", "WCRATE", "WWCRATE" };
            for (int i = 0; i < ovls.Length; i++)
            {
                int tileNum = toTileNum(blockPos, (i % 16) * 2, (i / 16) * 2);
                ini.setOverlay(ovls[i], tileNum);
                if (i % 2 == 1)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
            }

            //Try all defined overlay structure sprites +E1&ARTY (unit in wrong section). +CREDSA (random SHP-file).
            blockPos.X = StartTilePos.X;
            blockPos.Y += ((ovls.Length / 16) + 1) * 2;
            string[] strs = new string[] { /*"BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD",*/ //Walls will crash!
                "V12", "V13", "V14", "V15", "V16", "V17", "V18", "E1", "ARTY", "CREDSA" };
            for (int i = 0; i < strs.Length; i++)
            {
                int tileNum = toTileNum(blockPos, (i % 16) * 2, (i / 16) * 2);
                ini.addStructure(HouseGoodGuy, strs[i], 256, tileNum, 0);
                if (i % 2 == 1)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
            }
            //Seems like all work except E1, ARTY, CREDSA i.e. undefined sprites are not added.
            //Farmlands from structure have an energy bar and can be destroyed unlike those from overlay.
            //So it seems like farmland-structures are not converted to overlay-structures. They are different.

            /* //Walls will crash!
            //Test how walls from overlay and structure behave together.
            blockPos.X = StartTilePos.X;
            blockPos.Y += ((strs.Length / 16) + 1) * 2;
            string[] walls = new string[] { "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD" };
            for (int i = 0; i < walls.Length; i++)
            {
                TilePos tilePos = blockPos.getOffset((i % 16) * 2, (i / 16) * 2);

                //Do walls with different owners connect?
                ini.addStructure(HouseGoodGuy, walls[i], 256, toTileNum(tilePos, 0, 0), 0);
                ini.addStructure(HouseGoodGuy, walls[i], 256, toTileNum(tilePos, 0, 1), 0);
                ini.addStructure(HouseBadGuy, walls[i], 256, toTileNum(tilePos, 0, 2), 0);

                //Do walls color remap?
                ini.addStructure(HouseBadGuy, walls[i], 256, toTileNum(tilePos, 0, 4), 0);

                //Do structure walls connect with overlay walls? Must be neutral owner?
                ini.addStructure(HouseRA.IdNeutral, walls[i], 256, toTileNum(tilePos, 0, 6), 0);
                ini.setOverlay(walls[i], toTileNum(tilePos, 0, 7));
                ini.addStructure(HouseGoodGuy, walls[i], 256, toTileNum(tilePos, 0, 8), 0);
                ini.setOverlay(walls[i], toTileNum(tilePos, 0, 9));

                //Add scouts.
                if (i % 2 == 0)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 1), 128);
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 5), 128);
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 9), 128);
                }
            }
            */

            /* //Walls will crash!
            //Test if health and direction fields affect walls. And overlapping structure walls.
            blockPos.X = StartTilePos.X;
            blockPos.Y += 13;
            //Do health and direction fields affect walls?
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, -1, 0), 128); //Add scout.
            ini.addStructure(HouseGoodGuy, "BRIK", 64, toTileNum(blockPos, 0, 0), 0);
            ini.addStructure(HouseGoodGuy, "BRIK", 1, toTileNum(blockPos, 2, 0), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 3, 0), 128); //Add scout.
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 4, 0), 64);
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 6, 0), 128);
            //How do overlap behave?
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 7, 0), 128); //Add scout.
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 8, 0), 0);
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 8, 0), 0);
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 10, 0), 0);
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 10, 0), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 11, 0), 128); //Add scout.
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 12, 0), 0);
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 12, 0), 0);
            ini.addStructure(HouseGoodGuy, "SBAG", 256, toTileNum(blockPos, 12, 0), 0);
            ini.addStructure(HouseGoodGuy, "SBAG", 256, toTileNum(blockPos, 14, 0), 0);
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 14, 0), 0);
            ini.addStructure(HouseGoodGuy, "BRIK", 256, toTileNum(blockPos, 14, 0), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 15, 0), 128); //Add scout.
            ini.addStructure(HouseGoodGuy, "SBAG", 256, toTileNum(blockPos, 16, 0), 0);
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 16, 0), 0);
            ini.addStructure(HouseGoodGuy, "FENC", 256, toTileNum(blockPos, 16, 0), 0);
            ini.addStructure(HouseGoodGuy, "FENC", 256, toTileNum(blockPos, 18, 0), 0);
            ini.addStructure(HouseGoodGuy, "CYCL", 256, toTileNum(blockPos, 18, 0), 0);
            ini.addStructure(HouseGoodGuy, "SBAG", 256, toTileNum(blockPos, 18, 0), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 19, 0), 128); //Add scout.
            */

            //Try overlapping overlays with wall and farmland structures.
            //Try all defined overlay sprites. Don't include all gems and gold. I assume they behave the same.
            blockPos.X = StartTilePos.X;
            blockPos.Y += 4;
            ovls = new string[] { "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD", "GOLD01",
                "V12", "V13", "V14", "V15", "V16", "V17", "V18", "FPLS", "SCRATE", "WCRATE", "WWCRATE" };
            strs = new string[] { /*"CYCL",*/ "V12" }; //Walls will crash!
            for (int y = 0; y < strs.Length; y++)
            {
                for (int x = 0; x < ovls.Length; x++)
                {
                    int tileNum = toTileNum(blockPos, x * 2, y * 2);
                    ini.setOverlay(ovls[x], tileNum); //Add overlay.
                    ini.addStructure(HouseGoodGuy, strs[y], 256, tileNum, 0); //Add overlapping structure.
                    if (y % 2 == 0)
                    {
                        ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                    }
                }
            }

            return ini.save(PrintPath, "testOverlays");
        }

        private static string testOreFields()
        {
            //Test differently sized square fields of gems/gold and see what SHP-files and frame indices are used.
            //Uses a modified "TEMPERAT.MIX" to numerate ore sprites.
            string[] oresRand = new string[] { "GEM02", "GOLD03", "GOLD02", "GEM04", "GOLD01", "GEM03", "GEM01", "GOLD04" };
            //string[] oresIds = new string[] { "GEM01", "GEM02", "GEM03", "GEM04", "GOLD01", "GOLD02", "GOLD03", "GOLD04" };
            string[] oresIds = new string[] { "GEM02", "GOLD03" };
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            for (int i = 0; i < oresIds.Length; i++)
            {
                TilePos blockPos = new TilePos(4, 4 + (i * 6));
                for (int adjCount = 0; adjCount < 9; adjCount++)
                {
                    int adj = 0;
                    int ind = 0;
                    for (int y = 0; y < 5; y++)
                    {
                        for (int x = 0; x < 5; x++, ind++)
                        {
                            if (x != 2 || y != 2)
                            {
                                int tileNum = toTileNum(blockPos, x, y);
                                if (y == 0 || y == 4 || x == 0 || x == 4)
                                {
                                    ini.setOverlay(oresRand[ind % oresRand.Length], tileNum);
                                }
                                else if (adj < adjCount)
                                {
                                    ini.setOverlay(oresRand[ind % oresRand.Length], tileNum);
                                    adj++;
                                }
                            }
                        }
                    }
                    ini.setOverlay(oresIds[i], toTileNum(blockPos, 2, 2)); //Add centrum ore.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 5, 2), 128); //Add scouts.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(blockPos, 2, 5), 128);
                    blockPos.offset(6, 0);
                }
            }

            //Modify ore sprites to make them easier to identify.
            //TEMPERAT: "GOLD01", "GOLD02", "GOLD03", "GOLD04", "GEM01", "GEM02", "GEM03", "GEM04"
            FileMixArchiveWw.Editor temperateEditor = FileMixArchiveWw.Editor.open(TemplatePath + "TEMPERAT src.MIX");
            writeModifiedOres(temperateEditor);
            temperateEditor.save(PrintPath + "TEMPERAT.MIX");

            return ini.save(PrintPath, "testOreFields");
        }

        private static string testSmudgeAndOverlayOverlap(int set)
        {
            //Test how overlapping smudge and overlay sprites behave. Set value selects INI-key system to use in [SMUDGE] section:
            //Set==1 -> Use numbered INI-keys.
            //Set==2 -> Use tile number INI-keys (normal behavior).
            //Set 2 test needs correct handling of duplicate INI-keys to match game.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1)
            {
                ini.UseNumberedKeysSmudge = true;
            }
            else if (set == 2)
            {
                ini.UseNumberedKeysSmudge = false;
            }
            else throw new ArgumentException();

            Action<string, int> iniAdder = (string id, int tileNum) =>
            {
                if (isSmudge(id))
                {
                    ini.addSmudge(id, tileNum);
                }
                else //Assume overlay.
                {
                    ini.setOverlay(id, tileNum);
                }
            };

            KeyValuePair<string, string>[] overlaps = new KeyValuePair<string, string>[]
            {
                //under, over.
                new KeyValuePair<string, string>("CR3", "SC4"), //Game -> CR1-index1 [CR1-0]. Inside [] = unique keys.
                new KeyValuePair<string, string>("SC4", "CR3"), //Game -> SC4-0 [SC4-0]
                new KeyValuePair<string, string>("CR4", "SC3"), //Game -> CR1-1 [CR1-0]
                new KeyValuePair<string, string>("SC3", "CR4"), //Game -> CR1-1 [SC3-0]
                new KeyValuePair<string, string>("CR4", "CR3"), //Game -> CR1-1 [CR1-1]

                new KeyValuePair<string, string>("CR3", "CR4"), //Game -> CR1-1 [CR1-1]
                new KeyValuePair<string, string>("CR4", "GEM02"),
                new KeyValuePair<string, string>("GEM02", "CR4"),
                new KeyValuePair<string, string>("BRIK", "CR2"),
                new KeyValuePair<string, string>("CR2", "BRIK"),
            };

            TilePos tilePos = StartTilePos;
            foreach (KeyValuePair<string, string> overlap in overlaps)
            {
                for (int y = 0; y < 3; y++) //3x3 field with overlap in the center.
                {
                    for (int x = 0; x < 3; x++)
                    {
                        int tileNum = toTileNum(tilePos, x, y);
                        iniAdder(overlap.Key, tileNum);
                        if (x == 1 && y == 1) //Add overlap to test in the center.
                        {
                            iniAdder(overlap.Value, tileNum);
                        }
                        else if (x == 0 && y == 1) //Add scouts.
                        {
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128);
                        }
                    }
                }
                if (tilePos.X < 19)
                {
                    tilePos.X += 4;
                }
                else
                {
                    tilePos.X = StartTilePos.X;
                    tilePos.Y += 4;
                }
            }

            //Will brik walls connect even if center has smudge?
            for (int y = 0; y < 3; y++) //3x3 field with overlap in the center.
            {
                for (int x = 0; x < 3; x++)
                {
                    int tileNum = toTileNum(tilePos, x, y);
                    if (x == 0 && y == 1) //Add scouts.
                    {
                        ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128);
                    }
                    else if (x == 1 && y == 1)
                    {
                        ini.addSmudge("CR4", tileNum);
                    }
                    ini.setOverlay("BRIK", tileNum);
                }
            }

            //Also test how progression field affect smudges.
            tilePos.Y += 4;
            for (int i = 0; i < 6; i++)
            {
                int tileNum1 = toTileNum(tilePos, i * 3, 0);
                ini.addSmudge("BIB3", tileNum1, i);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum1 - 1, 128);

                int tileNum2 = toTileNum(tilePos, i * 3, 3);
                ini.addSmudge("CR2", tileNum2, i);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum2 - 1, 128);

                int tileNum3 = toTileNum(tilePos, i * 3, 5);
                ini.addSmudge("SC2", tileNum3, i);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum3 - 1, 128);
            }

            return ini.save(PrintPath, "testSmudgeAndOverlayOverlap set" + set);
        }

        private static string testSmudgeOverlap(int set)
        {
            //Test how overlapping smudge sprites behave. Set value selects INI-key system to use in [SMUDGE] section:
            //Set==1 -> Use numbered INI-keys.
            //Set==2 -> Use tile number INI-keys (normal behavior).
            //Set 2 test needs correct handling of duplicate INI-keys to match game.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1)
            {
                ini.UseNumberedKeysSmudge = true;
            }
            else if (set == 2)
            {
                ini.UseNumberedKeysSmudge = false;
            }
            else throw new ArgumentException();

            string[][] overlapPatterns = new string[][]
            {
                //Row 1.
                new string[]{"CR1"}, //Game -> CR1-index0 [CR1-0]. Inside [] = unique key.
                new string[]{"CR2"}, //Game -> CR1-0 [CR1-0]
                new string[]{"CR3"}, //Game -> CR1-0 [CR1-0]
                new string[]{"CR4"}, //Game -> CR1-0 [CR1-0]
                new string[]{"CR5"}, //Game -> CR1-0 [CR1-0]
                new string[]{"CR6"}, //Game -> CR1-0 [CR1-0]
                //Row 2.
                new string[]{"CR1"}, //Game -> CR1-0 [CR1-0]
                new string[]{"CR1", "CR1"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR1", "CR1", "CR1"}, //Game -> CR1-2 [CR1-2]
                new string[]{"CR1", "CR1", "CR1", "CR1"}, //Game -> CR1-3 [CR1-3]
                new string[]{"CR1", "CR1", "CR1", "CR1", "CR1"}, //Game -> CR1-4 [CR1-4]
                new string[]{"CR1", "CR1", "CR1", "CR1", "CR1", "CR1"}, //Game -> CR1-4 [CR1-4]
                //Row 3.
                new string[]{"CR6", "CR1"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR5", "CR2"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR4", "CR3"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR3", "CR4"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR2", "CR5"}, //Game -> CR1-1 [CR1-1]
                new string[]{"CR1", "CR6"}, //Game -> CR1-1 [CR1-1]
                //Row 4.
                new string[]{"SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC2"}, //Game -> SC2-0 [SC2-0]
                new string[]{"SC3"}, //Game -> SC3-0 [SC3-0]
                new string[]{"SC4"}, //Game -> SC4-0 [SC4-0]
                new string[]{"SC5"}, //Game -> SC5-0 [SC5-0]
                new string[]{"SC6"}, //Game -> SC6-0 [SC6-0]
                //Row 5.
                new string[]{"SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC1", "SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC1", "SC1", "SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC1", "SC1", "SC1", "SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC1", "SC1", "SC1", "SC1", "SC1"}, //Game -> SC1-0 [SC1-0]
                new string[]{"SC1", "SC1", "SC1", "SC1", "SC1", "SC1"}, //Game -> SC1-0 [SC1-0]
                //Row 6.
                new string[]{"SC6", "SC1"}, //Game -> SC1-0 [SC6-0]
                new string[]{"SC5", "SC2"}, //Game -> SC5-0 [SC5-0]
                new string[]{"SC4", "SC3"}, //Game -> SC3-0 [SC4-0]
                new string[]{"SC3", "SC4"}, //Game -> SC4-0 [SC3-0]
                new string[]{"SC2", "SC5"}, //Game -> SC5-0 [SC2-0]
                new string[]{"SC1", "SC6"}, //Game -> SC1-0 [SC1-0]
                //Row 7.
                new string[]{"CR2", "SC2"}, //Game -> SC2-0 [CR1-0]
                new string[]{"SC2", "CR2"}, //Game -> CR1-1 [SC2-0]
                new string[]{"SC1", "SC2", "CR3"}, //Game -> SC1-0 [SC1-0]
                new string[]{"CR3", "SC1", "SC2"}, //Game -> SC1-0 [CR1-0]
                new string[]{"CR4", "CR5", "SC4"}, //Game -> CR1-2 [CR1-1]
                new string[]{"SC4", "CR5", "CR4"}, //Game -> CR1-2 [SC4-0]
                //Row 8.
                new string[]{"SC4", "CR5", "SC3"}, //Game -> SC3-0 [SC4-0]
                new string[]{"CR4", "SC5", "CR3"}, //Game -> CR1-2 [CR1-1]
            };

            TilePos blockPos = StartTilePos;
            for (int i = 0; i < overlapPatterns.Length; i++)
            {
                string[] overlapPattern = overlapPatterns[i];
                int tileNum = toTileNum(blockPos, (i % 6) * 3, (i / 6) * 3); //3x3 tile blocks. 6 per row.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128);
                foreach (string smudgeId in overlapPattern) //Add overlapping smudges at tile.
                {
                    ini.addSmudge(smudgeId, tileNum);
                }
            }
            return ini.save(PrintPath, "testSmudgeOverlap set" + set);
        }

        private static string testSmudgeAndBibOverlap(int set)
        {
            //Test how overlapping smudge and bib sprites behave. Set value selects INI-key system to use in [SMUDGE] section:
            //Set==1 -> Use numbered INI-keys.
            //Set==2 -> Use tile number INI-keys (normal behavior).
            //Set 2 test needs correct handling of duplicate INI-keys to match game.

            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1)
            {
                ini.UseNumberedKeysSmudge = true;
            }
            else if (set == 2)
            {
                ini.UseNumberedKeysSmudge = false;
            }
            else throw new ArgumentException();

            int tileNum = StartTileNum; //Row 1.
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0); //Scorch after structure bib.
            ini.addSmudge("SC1", tileNum + (TilesPerLine * 1));
            ini.addSmudge("SC1", tileNum + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("SC1", tileNum + (TilesPerLine * 1)); //Scorch before structure bib.
            ini.addSmudge("SC1", tileNum + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB3", tileNum); //Scorch after smudge bib.
            ini.addSmudge("SC1", tileNum);
            ini.addSmudge("SC1", tileNum + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("SC1", tileNum); //Scorch before smudge bib.
            ini.addSmudge("SC1", tileNum + TilesPerLine);
            ini.addSmudge("BIB3", tileNum);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);

            tileNum = StartTileNum + (TilesPerLine * 4); //Row 2.
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0); //Crater after structure bib.
            ini.addSmudge("CR1", tileNum + (TilesPerLine * 1));
            ini.addSmudge("CR1", tileNum + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("CR1", tileNum + (TilesPerLine * 1)); //Crater before structure bib.
            ini.addSmudge("CR1", tileNum + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB3", tileNum); //Crater after smudge bib.
            ini.addSmudge("CR1", tileNum);
            ini.addSmudge("CR1", tileNum + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("CR1", tileNum); //Crater before smudge bib.
            ini.addSmudge("CR1", tileNum + TilesPerLine);
            ini.addSmudge("BIB3", tileNum);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);

            tileNum = StartTileNum + (TilesPerLine * 8); //Row 3.
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0); //Smudge bib after structure bib.
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 1));
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 1)); //Smudge bib before structure bib.
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB3", tileNum); //Smudge bib after smudge bib.
            ini.addSmudge("BIB2", tileNum);
            ini.addSmudge("BIB2", tileNum + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB2", tileNum); //Smudge bib before smudge bib.
            ini.addSmudge("BIB2", tileNum + TilesPerLine);
            ini.addSmudge("BIB3", tileNum);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);

            tileNum = StartTileNum + (TilesPerLine * 13); //Row 4.
            ini.addSmudge("SC1", tileNum + TilesPerLine); //Smudge bib after smudge replaced by structure bib.
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0);
            ini.addSmudge("BIB2", tileNum + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addStructure(HouseGoodGuy, "BARR", 255, tileNum, 0); //Smudge bib after try place smudge on structure bib.
            ini.addSmudge("SC1", tileNum + TilesPerLine);
            ini.addSmudge("BIB2", tileNum + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("SC1", tileNum + 1); //Smudge bib after smudge replaced by smudge bib.
            ini.addSmudge("BIB3", tileNum);
            ini.addSmudge("BIB3", tileNum + 1);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB3", tileNum);
            ini.addSmudge("SC1", tileNum + 1); //Smudge bib after try place smudge on smudge bib.
            ini.addSmudge("BIB3", tileNum + 1);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);

            tileNum = StartTileNum + (TilesPerLine * 18); //Row 5.
            ini.addSmudge("BIB1", tileNum + (TilesPerLine * 0));
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 1));
            ini.addSmudge("BIB3", tileNum + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);
            tileNum += 4;
            ini.addSmudge("BIB3", tileNum + (TilesPerLine * 0));
            ini.addSmudge("BIB2", tileNum + (TilesPerLine * 1));
            ini.addSmudge("BIB1", tileNum + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - TilesPerLine, 128);

            return ini.save(PrintPath, "testSmudgeAndBibOverlap set" + set);
        }

        private static string testIniKeys()
        {
            //Test how duplicate INI-keys are added. Use different rotations (or id:s) to easier see
            //which line is added by the game.
            //Needs correct handling of duplicate INI-keys when adding infantry, units, structures and ships to match the game.
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            TilePos tilePos = StartTilePos;

            string[] sprs = new string[] { "E1", "JEEP", "GUN", "PT" };
            int[] keyNumAdds = new int[] { 0, 0, 1, 1, -1, 3, 3, 2, 2 }; //Ascending, delimiter(-1), descending.
            //int[] keyNumAdds = new int[] { 0, 0, 1, 1, 1, -1, 3, 3, 3, 2, 2 }; //Ascending, delimiter(-1), descending.

            for (int i = 0; i < sprs.Length; i++)
            {
                string sprId = sprs[i];
                int count;
                Action<int, int, int> adder;
                if (isInfantry(sprId))
                {
                    count = ini.getSectionInfantry().Keys.Count;
                    adder = (int keyNum, int tileNum, int direction) =>
                        ini.addInfantry(keyNum, HouseGoodGuy, sprId, 256, tileNum, 0, direction);
                }
                else if (isUnit(sprId))
                {
                    count = ini.getSectionUnit().Keys.Count;
                    adder = (int keyNum, int tileNum, int direction) =>
                        ini.addUnit(keyNum, HouseGoodGuy, sprId, 256, tileNum, direction);
                }
                else if (isStructure(sprId))
                {
                    count = ini.getSectionStructure().Keys.Count;
                    adder = (int keyNum, int tileNum, int direction) =>
                        ini.addStructure(keyNum, HouseGoodGuy, sprId, 256, tileNum, direction);
                }
                else if (isShip(sprId))
                {
                    count = ini.getSectionShip().Keys.Count;
                    adder = (int keyNum, int tileNum, int direction) =>
                        ini.addShip(keyNum, HouseGoodGuy, sprId, 256, tileNum, direction);
                }
                else throw new ArgumentException();

                int dir = 0; //0,64,128,192,0,64,etc.
                for (int j = 0; j < keyNumAdds.Length; j++)
                {
                    int keyNumAdd = keyNumAdds[j];
                    if (keyNumAdd == -1) //Add delimiter.
                    {
                        ini.addStructure(HouseGoodGuy, "V19", 256, toTileNum(tilePos, j, 0), 0); //Delimiter.
                    }
                    else
                    {
                        adder(count + keyNumAdd, toTileNum(tilePos, j, 0), dir);
                        dir = (dir + 64) & 0xFF;
                    }
                }
                tilePos.Y += 2;
            }
            return ini.save(PrintPath, "testIniKeys");
        }

        private static string testTileSets(int set)
        {
            //Test all tile set and all their indices. Mostly to check what land type (color) tiles
            //have in the radar renderer at scale 1.
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1) //Interior.
            {
                ini.Theater = "INTERIOR";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear(268, 0)); //Walkable floor tiles.
            }
            else if (set == 2) //Temperate.
            {
                ini.Theater = "TEMPERATE";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear());
            }
            else if (set == 3) //Snow.
            {
                ini.Theater = "SNOW";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear());
            }
            else throw new ArgumentException();
            TheaterRA theater = TheaterRA.getTheater(ini.Theater);

            TilePos tilePosOrg = StartTilePos;
            TilePos tilePos = tilePosOrg;
            for (UInt16 tileId = 0; tileId <= 400; tileId++)
            {
                FileIcnTileSetTDRA fileIcn = theater.getTileSet(tileId);
                for (byte tileIndex = 0; tileIndex < fileIcn.IndexEntryCount; tileIndex++)
                {
                    //Only add tile set index if it's not empty (not defined nor present in theater).
                    if (!fileIcn.getTile(tileIndex).IsEmpty)
                    {
                        ini.setTileSetTableTile(tileId, tileIndex, toTileNum(tilePos));
                        tilePos.X++;
                        if (tilePos.X >= 99) //Start a new row?
                        {
                            tilePos.X = tilePosOrg.X;
                            tilePos.Y += 2;
                        }
                    }
                }
            }

            //Add scouting jeeps at the right side of the map.
            TilePos scoutPos = new TilePos(99, 3);
            for (; scoutPos.Y < 65; scoutPos.Y += 2)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(100, 2);
            ini.addStructure(HouseGoodGuy, "DOME", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 9; i++) //9 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "APWR", 256, toTileNum(auxPos, 0, i * 4), 0);
            }

            return ini.save(PrintPath, "testTileSets set" + set);
        }

        private static string testTileSetsWeird(int set)
        {
            //Test how the game handles some weird tile set cases. Both the map and the radar renderer.
            //Uses a modified "TEMPERAT.MIX".
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);

            if (set == 1)
            {
                ini.Theater = "INTERIOR";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear(268, 0));
            }
            else if (set == 2)
            {
                ini.Theater = "TEMPERATE";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear());
            }
            else throw new ArgumentException();

            //Set some valid tiles to use as reference locations.
            int tileNum = toTileNum(new TilePos(4, 6));
            for (int i = 0; i < 16; i++, tileNum += 2)
            {
                ini.setTileSetTableTile(97, 0, tileNum); //case 97: fileId = "B1";
            }

            //Write start marker.
            tileNum = toTileNum(new TilePos(4, 8));
            ini.setTileSetTableTile(97, 0, tileNum); //case 97: fileId = "B1";

            //Tile set theater defined but missing.
            //Result: HOM (Hall-Of-Mirrors effect).
            tileNum += 2;
            ini.setTileSetTableTile(100, 0, tileNum); //case 100: fileId = "B4";

            //Tile set present in theater but not specified for it. Uses a modified theater file.
            //Result: HOM.
            tileNum += 2;
            ini.setTileSetTableTile(292, 0, tileNum); //case 292: fileId = "LWAL0002";

            //Tile set 0x0000 same as 0xFFFF.
            //Result: Tile set index is not used. Calculated from tile X&Y instead. Radar draws it normally though.
            tileNum += 2;
            ini.setTileSetTableTile(0x0000, 3, tileNum); //Tile set id 0x0000.

            //Tile set 0x00FF same as 0xFFFF.
            //Result: Tile set index is not used. Calculated from tile X&Y instead. Radar draws it normally though.
            tileNum += 2;
            ini.setTileSetTableTile(0x00FF, 3, tileNum); //Tile set id 0x00FF.

            //Tile set 0xFFFF with non-zero index. Usually a 0xFFFF tile set id has a 0x00 tile set index in INI-files.
            //Result: Tile set index is not used. Calculated from tile X&Y instead.
            tileNum += 2;
            ini.setTileSetTableTile(0xFFFF, 3, tileNum); //Tile set id 0xFFFF.

            //Tile set index normal (for comparison).
            tileNum += 2;
            ini.setTileSetTableTile(162, 1, tileNum); //case 162: fileId = "S28";

            //Tile set index empty template tile.
            //Result: HOM.
            tileNum += 2;
            ini.setTileSetTableTile(162, //case 162: fileId = "S28";
                2, tileNum); //Empty tile index.

            //Tile set index high.
            //Result: Tile index 0 drawn. Index is wrapped around or set to 0?
            tileNum += 2;
            ini.setTileSetTableTile(162, //case 162: fileId = "S28";
                4, tileNum); //File only has 4 tiles (0-3).

            //Tile set index high land type.
            //Result: Tile index 0 drawn, radar land type 0x0A. Index is set to 0? But land type is wrapped around?
            tileNum += 2;
            ini.setTileSetTableTile(43, //case 43: fileId = "SH41";
                17, tileNum); //File only has 16 tiles (0-15). Should wrap around to tile index 1 which uses the 0x0A land type.

            //Tile set index high land type.
            //Result: Tile index 0 drawn, radar land type 0x0A. Index is set to 0? But land type is wrapped around?
            tileNum += 2;
            ini.setTileSetTableTile(43, //case 43: fileId = "SH41";
                27, tileNum); //File only has 16 tiles (0-15). Should wrap around to tile index 11 which uses the 0x0A land type.

            //Tile set index in file higher than tiles in it. Uses a modified file.
            //Result: HOM.
            tileNum += 2;
            ini.setTileSetTableTile(164, //case 164: fileId = "S30";
                1, tileNum); //Modify file to make index 1 point to tile >= 4. File only has 4 tiles (0-3).

            //Tile set index is to a garbage(?) tile. Uses a modified file.
            //Result: HOM.
            tileNum += 2;
            ini.setTileSetTableTile(103, //case 103: fileId = "P01";
                1, tileNum); //Modify file to make index 1 point to 5th tile. File has room for 5 tiles (0-4).

            //Write end marker.
            tileNum += 2;
            ini.setTileSetTableTile(97, 0, tileNum); //case 97: fileId = "B1";

            //Write the modified files and theater.
            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "TEMPERAT src.MIX");
            //Make one of the indices too high in a tile set.
            FileIcnTileSetRA fileIcn1 = editor.getFileAs<FileIcnTileSetRA>("S30.TEM");
            byte[] fileIcnBytes1 = fileIcn1.readAllBytes();
            fileIcnBytes1[fileIcn1.IndexEntriesOffset + 1] = 5; //Point index 1 to 6th tile. File only has 4 tiles (0-3).
            File.WriteAllBytes(PrintModifiedPath + "S30INDEX.TEM", fileIcnBytes1);
            editor.replace("S30.TEM", fileIcnBytes1);
            //Replace a defined tile set with the weird "LWAL0002.INT". Also add it for another test.
            FileIcnTileSetRA fileIcn2 = new FileMixArchiveWw(TemplatePath + "INTERIOR src.MIX").getFileAs<FileIcnTileSetRA>("LWAL0002.INT");
            byte[] fileIcnBytes2 = fileIcn2.readAllBytes();
            fileIcnBytes2[fileIcn2.IndexEntriesOffset + 1] = 4; //Point index 1 to 5th tile. File has room for 5 tiles (0-4).
            File.WriteAllBytes(PrintModifiedPath + "LWAL0002.INT", fileIcnBytes2);
            editor.replace("P01.TEM", fileIcnBytes2); //For testing garbage 5th tile.
            editor.add("LWAL0002.TEM", fileIcnBytes2); //For testing present, but theater unspecified tile set.
            //Save modified theater.
            editor.save(PrintPath + "TEMPERAT.MIX");

            //Add scouting jeeps at the left and right sides of the map.
            TilePos scoutPos = new TilePos(3, 4);
            for (; scoutPos.Y < 20; scoutPos.Y += 3)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 64);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos, 96, 0), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(100, 2);
            ini.addStructure(HouseGoodGuy, "DOME", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 6; i++) //6 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "APWR", 256, toTileNum(auxPos, 0, i * 4), 0);
            }

            return ini.save(PrintPath, "testTileSetsWeird set" + set);
        }

        private static string testRadarSprites(int set)
        {
            //Test how all sprites look on the radar.
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (set == 1) //Interior.
            {
                ini.Theater = "INTERIOR";
                ini.setTileSetTable(GroundLayerRA.createTileSetTableClear(268, 0)); //Walkable floor tiles.
            }
            else if (set == 2) //Temperate.
            {
                ini.Theater = "TEMPERATE";
            }
            else if (set == 3) //Snow.
            {
                ini.Theater = "SNOW";
            }
            else throw new ArgumentException();
            TheaterRA theater = TheaterRA.getTheater(ini.Theater);

            //Add infantries.
            TilePos startPos = new TilePos(5, 5);
            TilePos tilePos = startPos;
            string[] infantries = new string[]
            {
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10",
                "CHAN", "DELPHI", "DOG", "E1", "E2", "E3", "E4", "E6", "E7",
                "EINSTEIN", "GNRL", "MEDI", "SPY", "THF", "MECH", "SHOK", "E5"
            };
            for (int i = 0; i < infantries.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < infantries.Length; tilePos.X += 2, i++)
                {
                    int subPos = i % 5;
                    ini.addInfantry(HouseGoodGuy, infantries[i], 256, toTileNum(tilePos), subPos, 0);
                }
            }

            //Add units.
            tilePos.Y += 1;
            string[] units = new string[]
            {
                "1TNK", "2TNK", "3TNK", "4TNK", "APC", "ARTY", "HARV", "JEEP",
                "MCV", "MGG", "MNLY", "MRJ", "TRUK", "V2RL", "ANT1", "ANT2", "ANT3",
                "CTNK", "DTRK", "QTNK", "STNK", "TTNK", "FTNK", "MHQ", "MLRS",
                "BADR", "HELI", "HIND", "MIG", "TRAN", "U2", "YAK", "ORCA", "SMIG"
            };
            for (int i = 0; i < units.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < units.Length; tilePos.X += 2, i++)
                {
                    ini.addUnit(HouseGoodGuy, units[i], 256, toTileNum(tilePos), 0);
                }
            }

            //Add ships.
            tilePos.Y += 1;
            string[] ships = new string[]
            {
                "CA", "DD", "LST", "PT", "SS", "CARR", "MSUB"
            };
            for (int i = 0; i < ships.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < ships.Length; tilePos.X += 2, i++)
                {
                    ini.addShip(HouseGoodGuy, ships[i], 256, toTileNum(tilePos), 0);
                }
            }

            //Add structures.
            tilePos.Y += 2;
            string[] structs =
            {
                "AFLD", "AGUN", "APWR", "ATEK", "BARL", "BARR", "BIO", "BRL3", "DOME", "DOMF",
                "FACT", "FACF", "FCOM", "FIX", "FTUR", "GAP", "GUN", "HBOX", "HOSP", "HPAD",
                "IRON", "KENN", "MINP", "MINV", "MISS", "MSLO", "PBOX", "PDOX", "POWR", "PROC",
                "SAM", "SILO", "SPEN", "SPEF", "STEK", "SYRD", "SYRF", "TENT", "TSLA", "WEAP",
                "WEAF", "LAR1", "LAR2", "QUEE", "V01", "V02", "V03", "V04", "V05", "V06", "V07",
                "V08", "V09", "V10", "V11", "V12", "V13", "V14", "V15", "V16", "V17", "V18", "V19"
            };
            for (int i = 0; i < structs.Length; )
            {
                tilePos.X = startPos.X;
                int maxHeight = 0;
                for (; tilePos.X < 95 && i < structs.Length; i++)
                {
                    ini.addStructure(HouseGoodGuy, structs[i], 256, toTileNum(tilePos), 0);
                    Size size = getSizeInTilesUp(getSpriteSize(structs[i], theater));
                    tilePos.X += size.Width + 1;
                    maxHeight = Math.Max(maxHeight, size.Height);
                }
                tilePos.Y += maxHeight + 2;
            }

            //Add missing graphic structures. Let all be 3*3 in size.
            tilePos.Y += 2;
            string[] structsMissing =
            {
                "V20", "V21", "V22", "V23", "V24", "V25", "V26", "V27", "V28",
                "V29", "V30", "V31", "V32", "V33", "V34", "V35", "V36", "V37"
            };
            for (int i = 0; i < structsMissing.Length; )
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < structsMissing.Length; i++)
                {
                    ini.addStructure(HouseGoodGuy, structsMissing[i], 256, toTileNum(tilePos), 0);
                    tilePos.X += 3 + 1;
                }
                tilePos.Y += 3 + 2;
            }

            //Add terrains.
            tilePos.Y += 1;
            string[] terrains =
            {
                "BOXES01", "BOXES02", "BOXES03", "BOXES04", "BOXES05", "BOXES06", "BOXES07",
                "BOXES08", "BOXES09", "ICE01", "ICE02", "ICE03", "ICE04", "ICE05", "MINE",
                "T01", "T02", "T03", "T05", "T06", "T07", "T08", "T10", "T11", "T12", "T13",
                "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
            };
            for (int i = 0; i < terrains.Length; )
            {
                tilePos.X = startPos.X;
                int maxHeight = 0;
                for (; tilePos.X < 95 && i < terrains.Length; i++)
                {
                    ini.addTerrain(terrains[i], toTileNum(tilePos));
                    Size size = getSizeInTilesUp(getSpriteSize(terrains[i], theater));
                    tilePos.X += size.Width + 1;
                    maxHeight = Math.Max(maxHeight, size.Height);
                }
                tilePos.Y += maxHeight + 1;
            }

            //Add overlays.
            tilePos.Y += 1;
            string[] overlays =
            {
                "BARB", "BRIK", "CYCL", "FENC", "SBAG", "WOOD", "GEM01", "GEM02", "GEM03",
                "GEM04", "GOLD01", "GOLD02", "GOLD03", "GOLD04", "V12", "V13", "V14", "V15",
                "V16", "V17", "V18", "FPLS", "SCRATE", "WCRATE", "WWCRATE"
            };
            for (int i = 0; i < overlays.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < overlays.Length; tilePos.X += 2, i++)
                {
                    ini.setOverlay(overlays[i], toTileNum(tilePos));
                }
            }

            //Add smudges.
            string[] smudges =
            {
                "BIB1", "BIB2", "BIB3", "CR1", "CR2", "CR3", "CR4", "CR5", "CR6", "SC1", "SC2", "SC3", "SC4", "SC5", "SC6"
            };
            for (int i = 0; i < smudges.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 95 && i < smudges.Length; tilePos.X += 2, i++)
                {
                    ini.addSmudge(smudges[i], toTileNum(tilePos));
                    if (smudges[i] == "BIB1") tilePos.X += 3;
                    if (smudges[i] == "BIB2") tilePos.X += 2;
                    if (smudges[i] == "BIB3") tilePos.X += 1;
                }
            }

            //Add other houses to check their colors. Surround with concrete wall so they can't move too far.
            tilePos.Y += 2;
            string[] houses =
            {
                "GoodGuy", "England", "Germany", "France", "Ukraine", "USSR", "Greece", "Turkey", "Spain", "BadGuy",
                "Neutral", "Special", "Multi1", "Multi2", "Multi3", "Multi4", "Multi5", "Multi6", "Multi7", "Multi8"
            };
            Size houseTestSize = new Size(1, 7);
            tilePos.X = startPos.X + 6;
            for (int i = 0; i < houses.Length; i++, tilePos.X += 4)
            {
                //Add a concrete wall around tested house.
                for (int brikY = -1; brikY <= houseTestSize.Height; brikY++)
                {
                    for (int brikX = -1; brikX <= houseTestSize.Width; brikX++)
                    {
                        if (brikX != 0 || brikY == -1 || brikY == houseTestSize.Height)
                        {
                            ini.setOverlay("BRIK", toTileNum(tilePos.getOffset(brikX, brikY)));
                        }
                    }
                }
                //Add things to test.
                ini.addStructure(houses[i], "KENN", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
                ini.addStructure(houses[i], "MINP", 256, toTileNum(tilePos.getOffset(0, 1)), 0);
                ini.addStructure(houses[i], "MINV", 256, toTileNum(tilePos.getOffset(0, 2)), 0);
                ini.addStructure(houses[i], "HBOX", 256, toTileNum(tilePos.getOffset(0, 3)), 0);
                ini.addInfantry(houses[i], "SPY", 256, toTileNum(tilePos.getOffset(0, 4)), 0, 0);
                ini.addUnit(houses[i], "MNLY", 256, toTileNum(tilePos.getOffset(0, 5)), 128);
                ini.addUnit(houses[i], "TTNK", 256, toTileNum(tilePos.getOffset(0, 6)), 128);

                //Add scouting jeep at the right side.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(2, houseTestSize.Height / 2)), 64);

                //Also test MRJ. Add them far away so they don't affect other tests.
                TilePos tilePosMrj = tilePos.getOffset(0, 12);
                for (int brikY = -1; brikY <= 1; brikY++)
                {
                    for (int brikX = -1; brikX <= 1; brikX++)
                    {
                        if (brikX == 0 && brikY == 0)
                        {
                            ini.addUnit(houses[i], "MRJ", 256, toTileNum(tilePosMrj.getOffset(brikX, brikY)), 128);
                        }
                        else
                        {
                            ini.setOverlay("BRIK", toTileNum(tilePosMrj.getOffset(brikX, brikY)));
                        }
                    }
                }
            }
            ini.addStructure(HouseBadGuy, "DOMF", 256, toTileNum(tilePos), 0); //Also add a fake enemy structure.
            ini.addRule("HBOX", "Primary", "none"); //Disable pillbox's weapon.
            ini.addRule("TTNK", "Primary", "none"); //Disable tesla tank's weapon.
            ini.addRule("JEEP", "Primary", "none"); //Disable jeep's weapon.

            //Add scouting jeeps at the left and right sides of the map.
            TilePos scoutPos = new TilePos(3, 4);
            for (; scoutPos.Y < 65; scoutPos.Y += 3)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 64);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos, 96, 0), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(100, 2);
            ini.addStructure(HouseGoodGuy, "DOME", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 9; i++) //9 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "APWR", 256, toTileNum(auxPos, 0, i * 4), 0);
            }

            return ini.save(PrintPath, "testRadarSprites set" + set);
        }

        private static string sprite256Rotator(string sprId, Action<TestsIniAdderRA, int, int> adder, Size size, string testName)
        {
            //Test sprite directions 0-257 and what frame indices it produces.
            TestsIniAdderRA ini = new TestsIniAdderRA(TemplateIniPath);
            if (size.IsEmpty)
            {
                size = getSizeInTilesUp(getSpriteSize(sprId, TheaterRA.getTheater(ini.Theater)));
            }
            TilePos tilePos = StartTilePos;
            for (int dir = 0; dir < 258; dir++)
            {
                int tileNum = toTileNum(tilePos, (dir % 16) * size.Width, (dir / 16) * size.Height);
                adder(ini, tileNum, dir);
            }
            return ini.save(PrintPath, testName);
        }

        private static void saveToGeneralMix(List<string> iniPaths)
        {
            if (iniPaths.Count == 0)
            {
                throw new ArgumentException();
            }

            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "GENERAL src.MIX");
            int count = 1;
            foreach (string iniPath in iniPaths)
            {
                //Calculate name to replace from path.
                string nameToReplace = Path.GetFileName(iniPath).Split(' ')[0].ToLowerInvariant() + ".ini";
                if (nameToReplace == "scg01ea.ini") //Use default files?
                {
                    nameToReplace = "scg" + count.ToString("D2") + "ea.ini";
                    count++;
                }
                editor.replace(nameToReplace, iniPath);
            }
            editor.save(PrintPath + "GENERAL.MIX");
        }

        private static void writeModifiedAgun(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create an AGUN sprite with a solid shadow at top and a full palette 24 pixels down.
            //This way we can overlap AGUNs and see what a full palette looks like with a shadow over it.
            Frame paletteFrame = Frame.createPalette();
            Frame shadowFrame = Frame.createSolid(4, paletteFrame.Size); //Solid shadow (index 4 green).

            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(conquerEditor.getFile("AGUN.SHP"));
            Frame[] frames = fileShp.copyFrames();
            foreach (Frame frame in frames)
            {
                frame.write(shadowFrame, new Point(0, 0));
                frame.write(paletteFrame, new Point(0, 24));
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, fileShp.Size);
            conquerEditor.replace(fileShp.Name, path);
        }

        private static void writeModifiedBrik(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a BRIK sprite with a full palette to see what colors are produced in the game.
            writeModifiedPalette(conquerEditor, "BRIK.SHP", new Point(0, 0));
        }

        private static void writeModifiedHeli(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a HELI sprite with a solid color to see how an air shadow affect colors.
            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(conquerEditor.getFile("HELI.SHP"));
            Frame[] frames = fileShp.copyFrames();
            foreach (Frame frame in frames)
            {
                frame.clear(1);
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, fileShp.Size);
            conquerEditor.replace(fileShp.Name, path);
        }

        private static void writeModifiedHpad(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create an HPAD sprite with a full palette to see how colors are affected by an air
            //shadow from a HELI landing/taking off.
            writeModifiedPalette(conquerEditor, "HPAD.SHP", new Point(16, 17));
        }

        private static void writeModifiedMinp(FileMixArchiveWw.Editor conquerEditor)
        {
            //MINP are shaded in the game. Create a MINP sprite with a full palette to see if
            //it matches a normal shadow filter.
            writeModifiedPalette(conquerEditor, "MINP.SHP", new Point(0, 0));
        }

        private static void writeModifiedMinv(FileMixArchiveWw.Editor conquerEditor)
        {
            //MINV are shaded in the game. Create a MINV sprite with a full palette to see if
            //it matches a normal shadow filter.
            writeModifiedPalette(conquerEditor, "MINV.SHP", new Point(0, 0));
        }

        private static void writeModifiedC1C2(FileMixArchiveWw.Editor hiresEditor)
        {
            //Create C1 and C2 sprites with a full palette to see what color scheme civilians have.
            writeModifiedPalette(hiresEditor, "C1.SHP", new Point(0, 0));
            writeModifiedPalette(hiresEditor, "C2.SHP", new Point(0, 0));
        }

        private static void writeModifiedPalette(FileMixArchiveWw.Editor editor, string nameShp, Point position)
        {
            //Write a full palette (16x16) to position in sprite's frames.
            Frame paletteFrame = Frame.createPalette();

            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(editor.getFile(nameShp));
            Frame[] frames = fileShp.copyFrames();
            foreach (Frame frame in frames)
            {
                frame.write(paletteFrame, position);
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, new Size(fileShp.Width, fileShp.Height));
            editor.replace(fileShp.Name, path);
        }

        private static void writeModifiedOres(FileMixArchiveWw.Editor theaterEditor)
        {
            TextDrawer td = new TextDrawer(MapInfoDrawerTDRA.getFileFnt6x10(),
                new byte[] { 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); //Yellow text.
            for (int i = 1; i <= 8; i++)
            {
                int oreShpNumber;
                string oreShpName;
                byte solidIndex;
                if (i <= 4)
                {
                    oreShpNumber = i;
                    oreShpName = "GOLD0" + oreShpNumber.ToString() + ".TEM";
                    solidIndex = 1;
                }
                else
                {
                    oreShpNumber = (i - 4);
                    oreShpName = "GEM0" + oreShpNumber.ToString() + ".TEM";
                    solidIndex = 2;
                }

                FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(theaterEditor.getFile(oreShpName));
                Size shpSize = fileShp.Size;
                Frame frameBg = Frame.createSolid(solidIndex, 5, shpSize); //Yellow edge.
                Frame[] frames = new Frame[fileShp.FrameCount];
                //Draw ore id and frame number on frames.
                for (int k = 0; k < frames.Length; k++)
                {
                    frames[k] = new Frame(frameBg); //Copy background frame.
                    TextDrawer.TextDrawInfo textDi = td.getTextDrawInfo((oreShpNumber * 16 + k).ToString("X"));
                    Point center = new Point(
                        (shpSize.Width / 2) - (textDi.Width / 2),
                        (shpSize.Height / 2) - (textDi.Height / 2));
                    textDi.draw(center, frames[k]);
                }

                string path = PrintModifiedPath + fileShp.Name;
                FileShpSpriteSetTDRA.writeShp(path, frames, shpSize);
                theaterEditor.replace(fileShp.Name, path);
            }
        }

        private static int toTileNum(TilePos tilePos)
        {
            return MapRA.toTileNum(tilePos);
        }

        private static int toTileNum(TilePos tilePos, int dx, int dy)
        {
            return MapRA.toTileNum(tilePos.getOffset(dx, dy));
        }

        private static RulesObjectRA getRulesObject(string objectId)
        {
            RulesObjectRA rulesObject;
            if (!mRulesObject.TryGetValue(objectId, out rulesObject))
            {
                rulesObject = new RulesObjectRA(objectId, null);
                mRulesObject.Add(objectId, rulesObject);
            }
            return rulesObject;
        }

        private static Size getSpriteSize(string sprId, TheaterRA theater)
        {
            //Get actual sprite set id from global rules.
            string spriteSetId = getRulesObject(sprId).getFileShpOrObjectId();
            return theater.getSpriteSet(spriteSetId).Size;
        }

        private static Size getSizeInTilesUp(Size size)
        {
            int w = (int)Math.Ceiling(size.Width / 24.0);
            int h = (int)Math.Ceiling(size.Height / 24.0);
            return new Size(w, h);
        }

        private static Size getSizeInTilesNear(Size size)
        {
            int w = (size.Width + 11) / 24;
            int h = (size.Height + 11) / 24;
            return new Size(w, h);
        }

        private static bool isInfantry(string id)
        {
            return SprIdInfantries.Contains(id);
        }

        private static bool isUnit(string id)
        {
            return SprIdUnits.Contains(id);
        }

        static bool isShip(string id)
        {
            return SprIdShips.Contains(id);
        }

        private static bool isStructure(string id)
        {
            return SprIdStructures.Contains(id);
        }

        private static bool isTerrain(string id)
        {
            return SprIdTerrains.Contains(id);
        }

        private static bool isSmudge(string id)
        {
            return SprIdSmudges.Contains(id);
        }

        private static bool isOverlay(string id)
        {
            return SprIdOverlays.Contains(id);
        }
    }
}
