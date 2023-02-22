using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Creates test map/mission INI-files that is used to compare output from game with.

    //The input files folder (test\TD\template) should contain these files:
    //-CONQUER src.MIX
    //-DESERT src.MIX
    //-GENERAL src.GDI (DOS version)
    //-GENERAL src.MIX
    //-SC-000 src.MIX
    //-TEMPERAT src.MIX
    //They are just copies from the game folder with " src" added to their names.
    //It should also contain "scg01ea.ini" and "scb01ea.ini" which are copies from the game, but modified
    //to fit testing better. Essentially they are blank maps with most objects stripped and set to max size.

    static class TestsTD
    {
        private const int TilesPerLine = MapTD.WidthInTiles;
        private const int StartTileNum = TilesPerLine * 4 + 4;
        private static readonly TilePos StartTilePos = new TilePos(4, 4);

        private static readonly string TestPath = Program.DebugBasePath + "test\\TD\\";
        private static readonly string TemplatePath = TestPath + "template\\"; //Files used as input (read only).
        public static readonly string PrintPath = TestPath + "print\\"; //Final output of modified files.
        private static readonly string PrintModifiedPath = TestPath + "modified\\"; //Temp output of modified files.
        private static readonly string TemplateIniPathGoodGuy = TemplatePath + "scg01ea.ini";
        private static readonly string TemplateIniPathBadGuy = TemplatePath + "scb01ea.ini";

        private const string HouseGoodGuy = HouseTD.IdGoodGuy;
        private const string HouseBadGuy = HouseTD.IdBadGuy;

        //All valid sprites. For checking sprite type and copy-pasting from into tests.
        //Tests should use their own local sprite arrays so they are self-contained as much as possible.
        private static readonly string[] SprIdInfantries = new string[]
        {
            "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10",
            "CHAN", "DELPHI", "E1", "E2", "E3", "E4", "E5", "E6", "MOEBIUS", "RMBO"
        };
        private static readonly string[] SprIdUnits = new string[]
        {
            "APC", "ARTY", "BGGY", "BIKE", "BOAT", "FTNK", "HARV", "HTNK", "JEEP",
            "LST", "LTNK", "MCV", "MHQ", "MLRS", "MSAM", "MTNK", "STNK", "VICE",
            "RAPT", "STEG", "TREX", "TRIC", "A10", "C17", "HELI", "ORCA", "TRAN"
        };
        private static readonly string[] SprIdStructures = new string[]
        {
            "AFLD", "ATWR", "BIO", "EYE", "FACT", "FIX", "GTWR", "GUN", "HAND", "HOSP", "HPAD",
            "HQ", "MISS", "NUK2", "NUKE", "OBLI", "PROC", "PYLE", "SAM", "SILO", "TMPL", "WEAP",
            "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "ARCO", "V01", "V02", "V03", "V04", "V05",
            "V06", "V07", "V08", "V09", "V10", "V11", "V12", "V13", "V14", "V15", "V16", "V17",
            "V18", "V19", "V20", "V21", "V22", "V23", "V24", "V25", "V26", "V27", "V28", "V29",
            "V30", "V31", "V32", "V33", "V34", "V35", "V36", "V37"
        };
        private static readonly string[] SprIdTerrains = new string[]
        {
            "ROCK1", "ROCK2", "ROCK3", "ROCK4", "ROCK5", "ROCK6", "ROCK7", "SPLIT2", "SPLIT3",
            "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09", "T10", "T11", "T12",
            "T13", "T14", "T15", "T16", "T17", "T18", "TC01", "TC02", "TC03", "TC04", "TC05"
        };
        private static readonly string[] SprIdSmudges = new string[]
        {
            "BIB1", "BIB2", "BIB3", "CR1", "CR2", "CR3", "CR4", "CR5", "CR6", "SC1", "SC2", "SC3", "SC4", "SC5", "SC6"
        };
        private static readonly string[] SprIdOverlays = new string[]
        {
            "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "TI1", "TI2", "TI3", "TI4", "TI5", "TI6",
            "TI7", "TI8", "TI9", "TI10", "TI11", "TI12", "V12", "V13", "V14", "V15", "V16",
            "V17", "V18", "FPLS", "SCRATE", "WCRATE", "CONC", "ROAD", "SQUISH"
        };

        public static void run()
        {
            List<MapTD> maps = new List<MapTD>();
            FolderContainer container = new FolderContainer(PrintPath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (fileIni.isMapTDRA())
                {
                    maps.Add(MapTD.create(fileIni, container));
                }
            }
            MapTD.debugSaveRenderAll(maps);
        }

        public static void print()
        {
            Directory.CreateDirectory(PrintPath);
            Directory.CreateDirectory(PrintModifiedPath);

            List<string> iniPaths = new List<string>();

            //iniPaths.Add(testStructsHealth(1));
            //iniPaths.Add(testStructsHealth(2));
            //iniPaths.Add(testStructsHealth(3));
            //iniPaths.Add(testStructsHealth(4));
            //iniPaths.Add(testStructsHealth(5)); //Special! Set 3 with BadGuy to test color scheme of these structures.
            //iniPaths.Add(testStructsHealth(6)); //Special! Set 4 with BadGuy to test color scheme of these structures.
            //iniPaths.Add(testGun256Rots());
            ////---------------------------------------------
            //iniPaths.Add(testInfantry256Rots());
            //iniPaths.Add(testInfantry8Rots(1));
            //iniPaths.Add(testInfantry8Rots(2));
            ////---------------------------------------------
            //iniPaths.Add(testUnit256Rots());
            //iniPaths.Add(testUnits8Rots(1));
            //iniPaths.Add(testUnits8Rots(2));
            //string iniPathDinosaurs = testUnits8Rots(3); //Dinosaurs.
            //saveToDinosaurMix(iniPathDinosaurs);
            //iniPaths.Add(testUnits8Rots(4));
            //iniPaths.Add(testBoat256Rots());
            //iniPaths.Add(testBoat256Health());
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
            //iniPaths.Add(testAltColor(1));
            //iniPaths.Add(testAltColor(2));
            //iniPaths.Add(testPalette(1)); //Uses a modified "CONQUER.MIX".
            //iniPaths.Add(testPalette(2)); //Uses a modified "CONQUER.MIX".
            ////---------------------------------------------
            //iniPaths.Add(testOverlays());
            //iniPaths.Add(testOverlayOverlap());
            //iniPaths.Add(testTiberiumFields(1)); //Uses a modified "TEMPERAT.MIX" to numerate tiberium sprites.
            //iniPaths.Add(testTiberiumPatterns(1)); //Uses a modified "TEMPERAT.MIX" to numerate tiberium sprites.
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
            //iniPaths.Add(testTileSetsWeird(1)); //Uses a modified "DESERT.MIX".
            //iniPaths.Add(testTileSetsWeird(2)); //Uses a modified "DESERT.MIX".
            ////---------------------------------------------
            //iniPaths.Add(testRadarSprites(1));
            //iniPaths.Add(testRadarSprites(2));
            //iniPaths.Add(testRadarSprites(3));
            //iniPaths.Add(testRadarScalingDos()); //Uses a modified "DESERT.MIX" and "CONQUER.MIX".
            ////---------------------------------------------
            //iniPaths.Add(testOverlayConcrete32());
            //iniPaths.Add(testOverlayConcrete256(1));
            //iniPaths.Add(testOverlayConcrete256(2));
            //iniPaths.Add(testOverlayConcrete256(3));
            //iniPaths.Add(testOverlayConcrete256(4));
            //iniPaths.Add(testOverlayConcretePair256(1));
            //iniPaths.Add(testOverlayConcretePair256(2));
            //iniPaths.Add(testOverlayConcretePair256(3));
            //iniPaths.Add(testOverlayConcretePair256(4));
            //iniPaths.Add(testOverlayConcretePair256(5));
            //iniPaths.Add(testOverlayConcretePair256(6));
            //iniPaths.Add(testOverlayConcretePair256(7));
            //iniPaths.Add(testOverlayConcretePair256(8));
            ////---------------------------------------------
            //iniPaths.Add(testTemplate()); //Uses a modified "TEMPERAT.MIX".
            //saveToGeneralMix(iniPaths, true); //Remove BIN-file so [TEMPLATE] section can be tested.
            ////---------------------------------------------


            //saveToGeneralMix(iniPaths); //Save tests into GENERAL.MIX. Copy it to game folder to test in game.

            //The DOS version of Tiberian Dawn is very unstable in DOSBox for me which makes testing difficult. :(
        }

        private static string testStructsHealth(int set)
        {
            //Test structure health values 0-257 and what frame indices it produces.
            //Values higher than 256 doesn't seem to affect anything. Clamped to 256 by the game?
            int[] health = new int[]
            {
                256,
                128,
                127,
                2,
                1,
                0,
            };

            int padd = 3;
            string iniPath = TemplateIniPathGoodGuy;
            string iniTheater = "TEMPERATE";
            string house = HouseGoodGuy;
            List<string> structs;
            if (set == 1)
            {
                padd = 4;
                structs = new List<string> { "FACT", "FIX", "HAND", "MISS", "PROC", "TMPL", "WEAP", "AFLD" };
            }
            else if (set == 2)
            {
                structs = new List<string> { "ATWR", "BIO", "EYE", "GUN", "GTWR", "HOSP", "HPAD",
                    "HQ", "NUK2", "NUKE", "OBLI", "PYLE", "SAM", "SILO" };
            }
            else if (set == 3 || set == 5)
            {
                if (set == 5)
                {
                    //Special set to test neutral civilian structures color scheme with different owner.
                    iniPath = TemplateIniPathBadGuy;
                    house = HouseBadGuy;
                }
                structs = new List<string>(); //V01-V19 Temperate & Winter, ARCO and V19 shared.
                structs.Add("ARCO");
                for (int i = 1; i <= 19; i++)
                {
                    structs.Add("V" + i.ToString().PadLeft(2, '0'));
                }
            }
            else if (set == 4 || set == 6)
            {
                iniTheater = "DESERT";
                if (set == 6)
                {
                    //Special set to test neutral civilian structures color scheme with different owner.
                    iniPath = TemplateIniPathBadGuy;
                    house = HouseBadGuy;
                }
                structs = new List<string>(); //V19-V37 Desert, ARCO and V19 shared.
                structs.Add("ARCO");
                for (int i = 19; i <= 37; i++)
                {
                    structs.Add("V" + i.ToString().PadLeft(2, '0'));
                }
            }
            else throw new ArgumentException();

            TestsIniAdderTD ini = new TestsIniAdderTD(iniPath);
            ini.Theater = iniTheater;
            for (int i = 0; i < structs.Count; i++)
            {
                TilePos blockPos = new TilePos((i % 16), (i / 16) * 7);
                for (int y = 0; y < health.Length; y++)
                {
                    int tileNum = StartTileNum + (toTileNum(blockPos, 0, y) * padd);
                    ini.addStructure(house, structs[i], health[y], tileNum, 0);
                    ini.addUnit(house, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
            }
            return ini.save(PrintPath, "testStructsHealth set" + set);
        }

        private static string testGun256Rots()
        {
            //Test structure GUN directions 0-257 and what frame indices it produces.
            return sprite256Rotator((TestsIniAdderTD ini, int pos, int dir) =>
                ini.addStructure(HouseGoodGuy, "GUN", 256, pos, dir), new Size(1, 1), "testGun256Rots");
        }

        private static string testInfantry256Rots()
        {
            //Test infantry directions 0-257 and what frame indices it produces.
            return sprite256Rotator((TestsIniAdderTD ini, int pos, int dir) =>
                ini.addInfantry(HouseGoodGuy, "E3", 256, pos, 0, dir), new Size(1, 1), "testInfantry256Rots");
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

            string[] inf;
            if (set == 1)
            {
                inf = new string[] { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10" };
            }
            else if (set == 2)
            {
                inf = new string[] { "CHAN", "DELPHI", "MOEBIUS", "E1", "E2", "E3", "E4", "E5", "E6", "RMBO" };
            }
            else throw new Exception();

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            for (int i = 0; i < inf.Length; i++)
            {
                TilePos blockPos = StartTilePos.getOffset((i % 5) * 5, (i / 5) * 5);
                for (int y = 0; y < 7; y++)
                {
                    for (int x = 0; x < 7; x++)
                    {
                        int dir = dirs[(y * 7) + x];
                        if (dir != 1)
                        {
                            int tileNum = toTileNum(blockPos, x / 2, y / 2);
                            int sub = dir != 257 ? (1 + (x % 2)) + (2 * (y % 2)) : 0;
                            ini.addInfantry(HouseGoodGuy, inf[i], 256, tileNum, sub, dir);

                            if (dir == 257) //Add center scout to remove fog of war.
                            {
                                ini.addInfantry(HouseGoodGuy, "E1", 256, tileNum, 4, 0);
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
            return sprite256Rotator((TestsIniAdderTD ini, int pos, int dir) =>
                ini.addUnit(HouseGoodGuy, "JEEP", 256, pos, dir), new Size(1, 1), "testUnit256Rots");
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

            //Aircrafts (A10, C17, HELI, ORCA, TRAN) aren't rendered. Need a helipad? Nope!, still no show!

            int padd = 1;
            bool addScouts = true;
            string[] us;
            if (set == 1) //Units.
            {
                us = new string[] { "VICE", "STNK", "APC", "A10", "ARTY", "C17", "BIKE", "HELI",
                    "FTNK", "LST", "HARV", "ORCA", "MCV", "TRAN"};
            }
            else if (set == 2) //Turrets.
            {
                us = new string[] { "BGGY", "HTNK", "JEEP", "LTNK", "MHQ", "MLRS", "MSAM", "MTNK" };
            }
            else if (set == 3) //Dinosaurs.
            {
                us = new string[] { "RAPT", "STEG", "TREX", "TRIC" };
            }
            else if (set == 4) //Boat.
            {
                padd = 2;
                addScouts = false;
                us = new string[] { "BOAT" };
            }
            else throw new ArgumentException();

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
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
                            int tileNum = toTileNum(blockPos, x * padd, y * padd);
                            ini.addUnit(HouseGoodGuy, us[i], 256, tileNum, dir);
                        }
                        if (dir == 257 && addScouts) //Center?
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

        private static string testBoat256Rots()
        {
            //Test unit BOAT directions 0-257 and what frame indices it produces.
            return sprite256Rotator((TestsIniAdderTD ini, int pos, int dir) =>
                ini.addUnit(HouseGoodGuy, "BOAT", 256, pos, dir), new Size(2, 2), "testBoat256Rots");
        }

        private static string testBoat256Health()
        {
            //Test unit BOAT health values 0-257 and what frame indices it produces.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            TilePos tilePos = StartTilePos;
            for (int health = 0; health < 258; health++)
            {
                int tileNum = toTileNum(tilePos, (health % 16) * 2, (health / 16) * 2);
                ini.addUnit(HouseGoodGuy, "BOAT", health, tileNum, 0);
            }
            return ini.save(PrintPath, "testBoat256Health");
        }

        private static string testUnitsHelipad()
        {
            //Test what aircraft unit a helipad gets with different houses.

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            string[] houses = new string[]
            {
                HouseTD.IdBadGuy, HouseTD.IdGoodGuy, HouseTD.IdMulti1, HouseTD.IdMulti2,
                HouseTD.IdMulti3, HouseTD.IdMulti4, HouseTD.IdMulti5, HouseTD.IdMulti6,
                HouseTD.IdNeutral, HouseTD.IdSpecial
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
            string cmpId;
            List<string> sprites = new List<string>();
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1) //Desert T18.
            {
                ini.Theater = "DESERT";
                cmpId = "T18";
                //Add some desert terrains.
                sprites.AddRange(new string[]
                {
                    "ROCK1", "ROCK2", "ROCK3", "ROCK4", "ROCK5", "ROCK6", "ROCK7", "SPLIT3", "T04", "T08", "T09", "T18"
                });
                //Add some desert civilian structures.
                sprites.AddRange(new string[]
                {
                    "V19", "V20", "V21", "V22", "V23", "V24", "V25", "V26", "V27", "V28",
                    "V29", "V30", "V31", "V32", "V33", "V34", "V35", "V36", "V37"
                });
            }
            else if (set == 2 || set == 3) //Temperate T10 or T11
            {
                ini.Theater = "TEMPERATE";
                cmpId = set == 2 ? "T10" : "T11";
                //Add some temperate terrains.
                sprites.AddRange(new string[]
                {
                    "SPLIT2", "SPLIT3", "T01", "T02", "T03", "T05", "T06", "T07", "T08", "T10", "T11",
                    "T12", "T13", "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
                });
                //Add some temperate civilian structures.
                sprites.AddRange(new string[]
                {
                    "V01", "V02", "V03", "V04", "V05", "V06", "V07", "V08", "V09", "V10",
                    "V11", "V12", "V13", "V14", "V15", "V16", "V17", "V18", "V19"
                });
            }
            else throw new ArgumentException();

            //Add structures.
            sprites.AddRange(new string[]
            {
                "AFLD", "ARCO", "ATWR", "BIO", "EYE", "FACT", "FIX", "GTWR", "GUN", "HAND", "HOSP", "HPAD",
                "HQ", "MISS", "NUK2", "NUKE", "OBLI", "PROC", "PYLE", "SAM", "SILO", "TMPL", "WEAP"
            });
            ////Add units.
            //sprites.AddRange(new string[]
            //{
            //    "APC", "ARTY", "BGGY", "BIKE", "BOAT", "FTNK", "HARV", "HTNK", "JEEP",
            //    "LST", "LTNK", "MCV", "MHQ", "MLRS", "MSAM", "MTNK", "STNK", "RAPT",
            //    "STEG", "TREX", "TRIC", "VICE", "A10", "C17", "HELI", "ORCA", "TRAN"
            //});

            TheaterTD theater = TheaterTD.getTheater(ini.Theater);
            Size cmpTiles = getSizeInTilesUp(getSpriteSize(cmpId, theater));
            TilePos tilePos = StartTilePos;
            int overlapTiles = 2; //How many tiles should compare sprite overlap with test sprite?
            int maxTilesWidth = cmpTiles.Width;
            foreach (string sprId in sprites)
            {
                Size sprTiles = getSizeInTilesUp(getSpriteSize(sprId, theater));
                int maxTilesHeight = Math.Max(sprTiles.Height, cmpTiles.Height);

                //Add current sprite.
                int sprPos = toTileNum(tilePos, 0, maxTilesHeight - sprTiles.Height);
                if (isStructure(sprId)) //Structure?
                {
                    ini.addStructure(HouseGoodGuy, sprId, 256, sprPos, 0);
                }
                else if (isTerrain(sprId)) //Terrain?
                {
                    ini.addTerrain(sprId, sprPos);
                }
                else if (isUnit(sprId)) //Unit?
                {
                    ini.addUnit(HouseGoodGuy, sprId, 256, sprPos, 0);
                }
                else
                {
                    throw new Exception();
                }

                //Add compare terrain sprite directly to the right of current sprite.
                int cmpPos = toTileNum(tilePos, Math.Max(1, sprTiles.Width - overlapTiles), maxTilesHeight - cmpTiles.Height);
                ini.addTerrain(cmpId, cmpPos);

                //Add a JEEP left of current position to remove fog of war there.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, sprPos - 1, 0);

                maxTilesWidth = Math.Max(maxTilesWidth, sprTiles.Width + cmpTiles.Width - overlapTiles);
                if (tilePos.Y <= 50) //Keep going down column.
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
            return ini.save(PrintPath, "testOverlapPrio set" + set + " " + cmpId);
        }

        private static string testTerrainOverlapPrio(int set)
        {
            //Test draw priority of overlapping terrain sprites.
            //Many sprites to compare so split sprite-list in chunks that will fit on map.
            int startInd = 0;
            int endInd = 0;
            List<string> sprites = new List<string>();
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1 || set == 2) //Desert.
            {
                ini.Theater = "DESERT";
                //Add some desert terrains.
                sprites.AddRange(new string[]
                {
                    "ROCK1", "ROCK2", "ROCK3", "ROCK4", "ROCK5", "ROCK6", "ROCK7", "SPLIT3", "T04", "T08", "T09", "T18"
                });
                //Desert terrain will fit in two chunks.
                startInd = (int)(sprites.Count / 2.0 * (set - 1) + 0.5);
                endInd = (int)(sprites.Count / 2.0 * (set - 0) + 0.5);
            }
            else if (set == 3 || set == 4 || set == 5 || set == 6) //Temperate.
            {
                ini.Theater = "TEMPERATE";
                //Add some temperate terrains.
                sprites.AddRange(new string[]
                {
                    "SPLIT2", "SPLIT3", "T01", "T02", "T03", "T05", "T06", "T07", "T08", "T10", "T11",
                    "T12", "T13", "T14", "T15", "T16", "T17", "TC01", "TC02", "TC03", "TC04", "TC05"
                });
                //Temperate terrain will fit in four chunks.
                startInd = (int)(sprites.Count / 4.0 * (set - 3) + 0.5);
                endInd = (int)(sprites.Count / 4.0 * (set - 2) + 0.5);
            }
            else throw new ArgumentException();

            //Add ARCO structure as an additional sprite to compare terrain with.
            //But don't include it with terrain sprites i.e. calculate start/end index before adding it.
            sprites.Add("ARCO");

            TheaterTD theater = TheaterTD.getTheater(ini.Theater);
            TilePos tilePos = StartTilePos;
            int maxTilesWidth = 0;
            for (int j = startInd; j < endInd; j++) //Terrains to compare excluding ARCO structure last in list.
            {
                string cmpId = sprites[j];
                Size cmpTiles = getSizeInTilesNear(getSpriteSize(sprites[j], theater));
                for (int i = 0; i < sprites.Count; i++) //Terrains to compare including ARCO structure last in list.
                {
                    string sprId = sprites[i];
                    Size sprTiles = getSizeInTilesNear(getSpriteSize(sprites[i], theater));
                    int maxTilesHeight = Math.Max(cmpTiles.Height, sprTiles.Height);

                    //Add compare terrain sprite.
                    int cmpPos = toTileNum(tilePos, 0, maxTilesHeight - cmpTiles.Height);
                    if (isTerrain(cmpId)) //Terrain?
                    {
                        ini.addTerrain(cmpId, cmpPos);
                    }
                    else
                    {
                        throw new Exception();
                    }

                    //Add current sprite directly to the right of compare sprite.
                    int sprPos = toTileNum(tilePos, 1, maxTilesHeight - sprTiles.Height);
                    if (isStructure(sprId)) //Structure?
                    {
                        ini.addStructure(HouseGoodGuy, sprId, 256, sprPos, 0);
                    }
                    else if (isTerrain(sprId)) //Terrain?
                    {
                        ini.addTerrain(sprId, sprPos);
                    }
                    else
                    {
                        throw new Exception();
                    }

                    //Add a JEEP left of current position to remove fog of war there.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, cmpPos - 1, 0);

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

        private static string testAltColor(int set)
        {
            //Test if sprite uses normal or alternative color scheme for owner.
            string[] ids = new string[] {
                "APC", "ARTY", "BIKE", "FTNK", "HARV", "MCV", "STNK", "BGGY", "HTNK", "JEEP",
                "LTNK", "MHQ", "MLRS", "MSAM", "MTNK", "E1", "E2", "E3", "E4", "E5", "E6", "RMBO" };

            string[] houses;
            if (set == 1)
            {
                houses = new string[] { "GoodGuy", "Neutral", "Special", "BadGuy", "Multi1" };
            }
            else if (set == 2)
            {
                houses = new string[] { "Multi2", "Multi3", "Multi4", "Multi5", "Multi6" };
            }
            else throw new ArgumentException();

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            TilePos tilePos = StartTilePos;
            for (int j = 0; j < houses.Length; j++)
            {
                //Surround house by brik walls so he can't move too much.
                for (int x = 0; x < 41; x++)
                {
                    ini.addOverlay("BRIK", toTileNum(tilePos, x, 0));
                    ini.addOverlay("BRIK", toTileNum(tilePos, x, 2));
                }
                ini.addOverlay("BRIK", toTileNum(tilePos, 0, 1));
                ini.addOverlay("BRIK", toTileNum(tilePos, 40, 1));
                tilePos.Y += 1;

                //Add scout.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -2, 1), 128);

                for (int i = 0; i < ids.Length; i++)
                {
                    int tileNum = toTileNum(tilePos, 1, 0);
                    string sprId = ids[i];
                    if (isUnit(sprId))
                    {
                        ini.addUnit(houses[j], sprId, 256, tileNum, 128);
                    }
                    else if (isInfantry(sprId))
                    {
                        ini.addInfantry(houses[j], sprId, 256, tileNum, 0, 128);
                    }
                    tilePos.X += 1;
                }
                tilePos.X = StartTilePos.X;
                tilePos.Y += 3;
            }
            tilePos.Y += 2;
            for (int j = 0; j < houses.Length; j++)
            {
                //Add boats last, away from other units.
                ini.addUnit(houses[j], "BOAT", 256, toTileNum(tilePos, 0, j * 2), 0);
            }

            return ini.save(PrintPath, "testAltColor set" + set);
        }

        private static string testPalette(int set)
        {
            //Test together with modified graphic-files to display full palette with different houses.
            //Uses a modified "CONQUER.MIX".
            string[] houses = new string[] { "GoodGuy", "Neutral", "Special", "BadGuy", "Multi1",
                "Multi2", "Multi3", "Multi4", "Multi5", "Multi6" };

            string[] units = new string[] { "HARV", "MHQ" };

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1)
            {
                ini.Theater = "TEMPERATE";
            }
            else if (set == 2)
            {
                ini.Theater = "DESERT";
            }
            else throw new ArgumentException();

            TilePos tilePos = StartTilePos;
            for (int unitInd = 0; unitInd < units.Length; unitInd++)
            {
                for (int houseInd = 0; houseInd < houses.Length + 1; houseInd++) //houses.Length + 1 to add one last brik column.
                {
                    TilePos blockPos = tilePos.getOffset(2 * houseInd, 0);
                    for (int y = 0; y < 3; y++) //Add brik column.
                    {
                        ini.addOverlay("BRIK", toTileNum(blockPos, 0, y));
                    }
                    if (houseInd == houses.Length) break; //Added last brik column i.e. no more houses to add.
                    blockPos.X += 1;
                    int tileNum = toTileNum(blockPos);
                    for (int y = 0; y < 3; y++) //Add unit column.
                    {
                        if (y == 1)
                        {
                            ini.addUnit(houses[houseInd], units[unitInd], 256, tileNum + TilesPerLine, 128);
                            ini.addUnit(HouseGoodGuy, "MHQ", 256, tileNum - TilesPerLine, 128); //Add scout.
                        }
                        else
                        {
                            ini.addOverlay("BRIK", tileNum + (TilesPerLine * y));
                        }
                    }
                }
                tilePos.Y += 4;
            }

            //Test house color schemes and unit shadow remap by overlapping SILO (use modified SILO SHP-file
            //with full palette and solid frame in it).
            tilePos.X = StartTilePos.X;
            for (int i = 0; i < houses.Length; i++)
            {
                int tileNum = toTileNum(tilePos, (i % 5) * 4, (i / 5) * 3);
                ini.addUnit(HouseGoodGuy, "MHQ", 256, tileNum - 1, 128); //Add scout.
                ini.addStructure(houses[i], "SILO", 256, tileNum + 0, 0);
                ini.addStructure(houses[i], "SILO", 256, tileNum + 1, 0);
            }

            //Add a helipad so we can do some air shadow testing also. (use modified ORCA SHP-file with
            //solid color and HPAD SHP-file with full palette in it.
            tilePos.Y += 6;
            ini.addStructure(HouseGoodGuy, "HPAD", 256, toTileNum(tilePos), 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, -1), 128); //Add scout.

            //Modify files used in this test.
            //CONQUER: "HARV", "HPAD", "MHQ", "ORCA", "SILO"
            FileMixArchiveWw.Editor conquerEditor = FileMixArchiveWw.Editor.open(TemplatePath + "CONQUER src.MIX");
            writeModifiedHarv(conquerEditor);
            writeModifiedHpad(conquerEditor);
            writeModifiedMhq(conquerEditor);
            writeModifiedOrca(conquerEditor);
            writeModifiedSilo(conquerEditor);
            conquerEditor.save(PrintPath + "CONQUER.MIX");

            return ini.save(PrintPath, "testPalette set" + set);
        }

        private static string testOverlays()
        {
            //Test how overlay sprites behave, especially those that are also valid structure sprites (walls and farmlands).
            //Also test what happens if a known id is put in the "wrong" section e.g. "BIKE"-unit in "[OVERLAY]".
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);

            //Try all defined overlay sprites +BIKE (unit in wrong section) +CREDS (random SHP-file).
            TilePos blockPos = StartTilePos;
            ini.addUnit(HouseGoodGuy, "MTNK", 256, toTileNum(StartTilePos, -2, 2), 0); //Add some tanks to try damaging overlays.
            ini.addUnit(HouseGoodGuy, "MTNK", 256, toTileNum(StartTilePos, -2, 3), 0);
            string[] ovls = new string[] { "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "TI1", "TI2", "TI3", "TI4", "TI5",
                "TI6", "TI7", "TI8", "TI9", "TI10", "TI11", "TI12", "V12", "V13", "V14", "V15", "V16", "V17", "V18",
                "FPLS", "WCRATE", "SCRATE", "ROAD", "CONC", "SQUISH", "BIKE", "CREDS" };
            for (int i = 0; i < ovls.Length; i++)
            {
                int tileNum = toTileNum(blockPos, (i % 16) * 2, (i / 16) * 2);
                ini.addOverlay(ovls[i], tileNum);
                if (i % 2 == 1)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
            }
            //Seems like all work except SQUISH, BIKE, CREDS because squish is weird and undefined sprites are not added.

            //Try all defined overlay structure sprites +E1&BIKE (unit in wrong section). +CREDS (random SHP-file).
            blockPos.X = StartTilePos.X;
            blockPos.Y += ((ovls.Length / 16) + 1) * 2;
            string[] strs = new string[] { "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "V12", "V13", "V14", "V15", "V16",
                "V17", "V18", "ROAD", "E1", "BIKE", "CREDS" };
            for (int i = 0; i < strs.Length; i++)
            {
                int tileNum = toTileNum(blockPos, (i % 16) * 2, (i / 16) * 2);
                ini.addStructure(HouseGoodGuy, strs[i], 256, tileNum, 0);
                if (i % 2 == 1)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                }
            }
            //Seems like all work except ROAD, E1, BIKE, CREDS i.e. undefined sprites are not added (road is obsolete).
            //Farmlands from structure have an energy bar and can be destroyed unlike those from overlay.
            //So it seems like farmland-structures are not converted to overlay-structures. They are different.

            //Test how walls from overlay and structure behave together.
            blockPos.X = StartTilePos.X;
            blockPos.Y += ((strs.Length / 16) + 1) * 2;
            string[] walls = new string[] { "BARB", "BRIK", "CYCL", "SBAG", "WOOD" };
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
                ini.addStructure(HouseTD.IdNeutral, walls[i], 256, toTileNum(tilePos, 0, 6), 0);
                ini.addOverlay(walls[i], toTileNum(tilePos, 0, 7));
                ini.addStructure(HouseGoodGuy, walls[i], 256, toTileNum(tilePos, 0, 8), 0);
                ini.addOverlay(walls[i], toTileNum(tilePos, 0, 9));

                //Add scouts.
                if (i % 2 == 0)
                {
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 1), 128);
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 5), 128);
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -1, 9), 128);
                }
            }
            //Seems like structure-walls are converted and added like neutral normal overlay-walls.

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
            //Seems like health and direction fields don't affect walls. And last wall (highest line number in INI-file) is added.

            return ini.save(PrintPath, "testOverlays");
        }

        private static string testOverlayOverlap()
        {
            //Test how overlapping overlay sprites behave, especially those that are also valid structure sprites (walls and farmlands).
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);

            //Try all defined overlay sprites. Don't include TI2-TI12. I assume they behave the same as TI1.
            TilePos blockPos = StartTilePos;
            ini.addUnit(HouseGoodGuy, "MTNK", 256, toTileNum(StartTilePos, -2, 2), 0); //Add some tanks to try damage overlays.
            ini.addUnit(HouseGoodGuy, "MTNK", 256, toTileNum(StartTilePos, -2, 3), 0);
            string[] ovls = new string[] { "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "TI1",
                "V12", "V13", "V14", "V15", "V16", "V17", "V18", "FPLS", "WCRATE", "SCRATE", "ROAD", "CONC", "SQUISH" };
            for (int y = 0; y < ovls.Length; y++)
            {
                for (int x = 0; x < ovls.Length; x++)
                {
                    int tileNum = toTileNum(blockPos, x * 2, y * 2);
                    ini.addOverlay(ovls[y], tileNum); //Add ground overlay.
                    ini.addOverlay(ovls[x], tileNum); //Add overlapping overlay.
                    if (y % 2 == 1 && x % 2 == 0)
                    {
                        ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                    }
                }
            }

            //Try overlapping overlays with wall and farmland structures.
            blockPos.X = StartTilePos.X;
            blockPos.Y += ovls.Length * 2 + 2;
            string[] strs = new string[] { "CYCL", "V12" };
            for (int y = 0; y < strs.Length; y++)
            {
                for (int x = 0; x < ovls.Length; x++)
                {
                    int tileNum = toTileNum(blockPos, x * 2, y * 2);
                    ini.addOverlay(ovls[x], tileNum); //Add overlay.
                    ini.addStructure(HouseGoodGuy, strs[y], 256, tileNum, 0); //Add overlapping structure.
                    if (y % 2 == 0)
                    {
                        ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128); //Add scout.
                    }
                }
            }
            return ini.save(PrintPath, "testOverlayOverlap");
        }

        private static string testTiberiumFields(int set)
        {
            //Test differently sized square fields of tiberium and see what SHP-files and frame indices are used.
            //Uses a modified "TEMPERAT.MIX" to numerate tiberium sprites.
            string tibId = "TI" + set; //Set 1-12 = TI1-TI12.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            TilePos tilePos = StartTilePos;
            for (int s = 1; s <= 8; s++) //Field size (s*s).
            {
                for (int y = 0; y < s; y++)
                {
                    for (int x = 0; x < s; x++)
                    {
                        int tileNum = toTileNum(tilePos, x, y);
                        ini.addOverlay(tibId, tileNum);

                        //Add scouts.
                        if ((x == s / 2 && (y == 0 || y == s - 1)) || (y == s / 2 && (x == 0 || x == s - 1)))
                        {
                            if (y == 0) tileNum -= TilesPerLine;
                            else if (y == s - 1) tileNum += TilesPerLine;
                            else if (x == 0) tileNum--;
                            else if (x == s - 1) tileNum++;
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum, 128);
                        }
                    }
                }
                if (tilePos.X < (StartTilePos.X + 15))
                {
                    tilePos.X += s + 2;
                }
                else
                {
                    tilePos.X = StartTilePos.X;
                    tilePos.Y += s + 2;
                }
            }

            //Modify tiberium sprites to make them easier to identify.
            //TEMPERAT: "TI1"-"TI12"
            FileMixArchiveWw.Editor temperateEditor = FileMixArchiveWw.Editor.open(TemplatePath + "TEMPERAT src.MIX");
            writeModifiedTiberium(temperateEditor);
            temperateEditor.save(PrintPath + "TEMPERAT.MIX");

            return ini.save(PrintPath, "testTiberiumFields set" + set);
        }

        private static string testTiberiumPatterns(int set)
        {
            //Test what SHP-files and frame indices are used for a tiberium tile with different
            //amount of adjacent tiberium tiles.
            //Uses a modified "TEMPERAT.MIX" to numerate tiberium sprites.
            string tibId = "TI" + set; //Set 1-12 = TI1-TI12.
            List<bool[,]> tibPatterns = new List<bool[,]>();
            for (int maxAdjTib = 0; maxAdjTib <= 8; maxAdjTib++) //Max number of adjacent tiberium tiles.
            {
                bool[,] tibPattern = new bool[3, 3];
                tibPattern[1, 1] = true; //Always add the center tile.
                int adjTib = 0; //Number of adjacent tiberium tiles.
                for (int y = 0; y < 3; y++) //3*3 field with always one tiberium tile in the center.
                {
                    for (int x = 0; x < 3; x++)
                    {
                        if (adjTib < maxAdjTib && !tibPattern[y, x]) //Skip pre-added center tile.
                        {
                            tibPattern[y, x] = true;
                            adjTib++;
                        }
                    }
                }
                tibPatterns.Add(tibPattern);
            }
            tibPatterns.Add(new bool[,]
            {
                {true,false,true},
                {false,true,false},
                {true,false,true}
            });
            tibPatterns.Add(new bool[,]
            {
                {false,true,false},
                {true,true,true},
                {false,true,false}
            });

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            for (int i = 0; i < tibPatterns.Count; i++)
            {
                bool[,] pattern = tibPatterns[i];
                TilePos blockPos = StartTilePos.getOffset((i % 5) * 4, (i / 5) * 4);
                for (int y = 0; y < 3; y++) //3*3 field with always one tiberium tile in the center.
                {
                    for (int x = 0; x < 3; x++)
                    {
                        int tileNum = toTileNum(blockPos, x, y);
                        if (pattern[y, x])
                        {
                            ini.addOverlay(tibId, tileNum);
                        }
                        //Add scouts.
                        if (x == 0 && y == 1)
                        {
                            ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128);
                        }
                    }
                }
            }

            //Modify tiberium sprites to make them easier to identify.
            //TEMPERAT: "TI1"-"TI12"
            FileMixArchiveWw.Editor temperateEditor = FileMixArchiveWw.Editor.open(TemplatePath + "TEMPERAT src.MIX");
            writeModifiedTiberium(temperateEditor);
            temperateEditor.save(PrintPath + "TEMPERAT.MIX");

            return ini.save(PrintPath, "testTiberiumPatterns set" + set);
        }

        private static string testSmudgeAndOverlayOverlap(int set)
        {
            //Test how overlapping smudge and overlay sprites behave. Set value selects INI-key system to use in [SMUDGE] section:
            //Set==1 -> Use numbered INI-keys.
            //Set==2 -> Use tile number INI-keys (normal behavior).
            //Set 2 test needs correct handling of duplicate INI-keys to match game.

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1)
            {
                ini.UseNumberedKeysSmudge = true;
            }
            else if (set == 2)
            {
                ini.UseNumberedKeysSmudge = false;
            }
            else throw new ArgumentException();

            Action<string, int> iniAdder = (string id, int tileNumber) =>
            {
                if (isSmudge(id))
                {
                    ini.addSmudge(id, tileNumber);
                }
                else //Assume overlay.
                {
                    ini.addOverlay(id, tileNumber);
                }
            };

            KeyValuePair<string, string>[] overlaps = new KeyValuePair<string, string>[]
            {
                //under, over.
                new KeyValuePair<string, string>("TI5", "BRIK"),
                new KeyValuePair<string, string>("BRIK", "TI5"),
                new KeyValuePair<string, string>("SBAG", "WOOD"),
                new KeyValuePair<string, string>("WOOD", "SBAG"),
                new KeyValuePair<string, string>("WCRATE", "TI4"),

                new KeyValuePair<string, string>("TI4", "WCRATE"),
                new KeyValuePair<string, string>("CR3", "SC4"),
                new KeyValuePair<string, string>("SC4", "CR3"),
                new KeyValuePair<string, string>("CR4", "SC3"),
                new KeyValuePair<string, string>("SC3", "CR4"),

                new KeyValuePair<string, string>("CR4", "CR3"),
                new KeyValuePair<string, string>("CR3", "CR4"),
                new KeyValuePair<string, string>("CR4", "TI4"),
                new KeyValuePair<string, string>("TI4", "CR4"),
                new KeyValuePair<string, string>("BRIK", "CR2"),

                new KeyValuePair<string, string>("CR2", "BRIK"),
                new KeyValuePair<string, string>("V17", "V12"),
                new KeyValuePair<string, string>("V15", "WCRATE"),

            };

            TilePos tilePos = StartTilePos;
            foreach (KeyValuePair<string, string> overlap in overlaps)
            {
                for (int y = 0; y < 3; y++) //3*3 field with overlap in the center.
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
                if (tilePos.X < (StartTilePos.X + 15))
                {
                    tilePos.X += 4;
                }
                else
                {
                    tilePos.X = StartTilePos.X;
                    tilePos.Y += 4;
                }
            }

            //Will brik walls connect even if center has tiberium?
            for (int y = 0; y < 3; y++) //3*3 field with overlap in the center.
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
                        ini.addOverlay("TI4", tileNum);
                    }
                    ini.addOverlay("BRIK", tileNum);
                }
            }

            //Also test how progression field affect smudges.
            tilePos.X = StartTilePos.X;
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

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
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
                new string[]{"CR1"},
                new string[]{"CR2"},
                new string[]{"CR3"},
                new string[]{"CR4"},
                new string[]{"CR5"},
                new string[]{"CR6"},
                //Row 2.
                new string[]{"CR1"},
                new string[]{"CR1", "CR1"},
                new string[]{"CR1", "CR1", "CR1"},
                new string[]{"CR1", "CR1", "CR1", "CR1"},
                new string[]{"CR1", "CR1", "CR1", "CR1", "CR1"},
                new string[]{"CR1", "CR1", "CR1", "CR1", "CR1", "CR1"},
                //Row 3.
                new string[]{"CR6", "CR1"},
                new string[]{"CR5", "CR2"},
                new string[]{"CR4", "CR3"},
                new string[]{"CR3", "CR4"},
                new string[]{"CR2", "CR5"},
                new string[]{"CR1", "CR6"},
                //Row 4.
                new string[]{"SC1"},
                new string[]{"SC2"},
                new string[]{"SC3"},
                new string[]{"SC4"},
                new string[]{"SC5"},
                new string[]{"SC6"},
                //Row 5.
                new string[]{"SC1"},
                new string[]{"SC1", "SC1"},
                new string[]{"SC1", "SC1", "SC1"},
                new string[]{"SC1", "SC1", "SC1", "SC1"},
                new string[]{"SC1", "SC1", "SC1", "SC1", "SC1"},
                new string[]{"SC1", "SC1", "SC1", "SC1", "SC1", "SC1"},
                //Row 6.
                new string[]{"SC6", "SC1"},
                new string[]{"SC5", "SC2"},
                new string[]{"SC4", "SC3"},
                new string[]{"SC3", "SC4"},
                new string[]{"SC2", "SC5"},
                new string[]{"SC1", "SC6"},
                //Row 7.
                new string[]{"CR2", "SC2"},
                new string[]{"SC2", "CR2"},
                new string[]{"SC1", "SC2", "CR3"},
                new string[]{"CR3", "SC1", "SC2"},
                new string[]{"CR4", "CR5", "SC4"},
                new string[]{"SC4", "CR5", "CR4"},
                //Row 8.
                new string[]{"SC4", "CR5", "SC3"},
                new string[]{"CR4", "SC5", "CR3"},
            };

            for (int i = 0; i < overlapPatterns.Length; i++)
            {
                string[] overlapPattern = overlapPatterns[i];
                int tileNum = toTileNum(StartTilePos, (i % 6) * 3, (i / 6) * 3);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, tileNum - 1, 128);
                foreach (string smudgeId in overlapPattern)
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

            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1)
            {
                ini.UseNumberedKeysSmudge = true;
            }
            else if (set == 2)
            {
                ini.UseNumberedKeysSmudge = false;
            }
            else throw new ArgumentException();

            int tn = StartTileNum; //Row 1.
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0); //Scorch after structure bib.
            ini.addSmudge("SC1", tn + (TilesPerLine * 1));
            ini.addSmudge("SC1", tn + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("SC1", tn + (TilesPerLine * 1)); //Scorch before structure bib.
            ini.addSmudge("SC1", tn + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB3", tn); //Scorch after smudge bib.
            ini.addSmudge("SC1", tn);
            ini.addSmudge("SC1", tn + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("SC1", tn); //Scorch before smudge bib.
            ini.addSmudge("SC1", tn + TilesPerLine);
            ini.addSmudge("BIB3", tn);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);

            tn = StartTileNum + (TilesPerLine * 4); //Row 2.
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0); //Crater after structure bib.
            ini.addSmudge("CR1", tn + (TilesPerLine * 1));
            ini.addSmudge("CR1", tn + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("CR1", tn + (TilesPerLine * 1)); //Crater before structure bib.
            ini.addSmudge("CR1", tn + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB3", tn); //Crater after smudge bib.
            ini.addSmudge("CR1", tn);
            ini.addSmudge("CR1", tn + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("CR1", tn); //Crater before smudge bib.
            ini.addSmudge("CR1", tn + TilesPerLine);
            ini.addSmudge("BIB3", tn);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);

            tn = StartTileNum + (TilesPerLine * 8); //Row 3.
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0); //Smudge bib after structure bib.
            ini.addSmudge("BIB2", tn + (TilesPerLine * 1));
            ini.addSmudge("BIB2", tn + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB2", tn + (TilesPerLine * 1)); //Smudge bib before structure bib.
            ini.addSmudge("BIB2", tn + (TilesPerLine * 2));
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB3", tn); //Smudge bib after smudge bib.
            ini.addSmudge("BIB2", tn);
            ini.addSmudge("BIB2", tn + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB2", tn); //Smudge bib before smudge bib.
            ini.addSmudge("BIB2", tn + TilesPerLine);
            ini.addSmudge("BIB3", tn);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);

            tn = StartTileNum + (TilesPerLine * 13); //Row 4.
            ini.addSmudge("SC1", tn + TilesPerLine); //Smudge bib after smudge replaced by structure bib.
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0);
            ini.addSmudge("BIB2", tn + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addStructure(HouseGoodGuy, "PYLE", 255, tn, 0); //Smudge bib after try place smudge on structure bib.
            ini.addSmudge("SC1", tn + TilesPerLine);
            ini.addSmudge("BIB2", tn + TilesPerLine);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("SC1", tn + 1); //Smudge bib after smudge replaced by smudge bib.
            ini.addSmudge("BIB3", tn);
            ini.addSmudge("BIB3", tn + 1);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB3", tn);
            ini.addSmudge("SC1", tn + 1); //Smudge bib after try place smudge on smudge bib.
            ini.addSmudge("BIB3", tn + 1);
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);

            tn = StartTileNum + (TilesPerLine * 18); //Row 5.
            ini.addSmudge("BIB1", tn + (TilesPerLine * 0));
            ini.addSmudge("BIB2", tn + (TilesPerLine * 1));
            ini.addSmudge("BIB3", tn + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);
            tn += 4;
            ini.addSmudge("BIB3", tn + (TilesPerLine * 0));
            ini.addSmudge("BIB2", tn + (TilesPerLine * 1));
            ini.addSmudge("BIB1", tn + (TilesPerLine * 2));
            ini.addUnit(HouseGoodGuy, "JEEP", 256, tn - TilesPerLine, 128);

            return ini.save(PrintPath, "testSmudgeAndBibOverlap set" + set);
        }

        private static string testIniKeys()
        {
            //Test how duplicate INI-keys are added. Use different rotations (or id:s) to easier see
            //which line is added by the game.
            //Must use the IniKeyFinderTD class when adding infantry, units and structures to match the game.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            TilePos tilePos = StartTilePos;

            string[] sprs = new string[] { "E1", "JEEP", "GUN" };
            int[] keyNumAdds = new int[] { 0, 0, 1, 1, -1, 3, 3, 2, 2 }; //Descending, delimiter(-1), ascending.
            //int[] keyNumAdds = new int[] { 0, 0, 1, 1, 1, -1, 3, 3, 3, 2, 2 }; //Descending, delimiter(-1), ascending.

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
                else throw new ArgumentException();

                int dir = 192; //192,128,64,0,192,128,etc.
                int keyNumAddsCount = keyNumAdds.Count((int ka) => ka >= 0); //Don't count any delimiters.
                for (int j = 0; j < keyNumAdds.Length; j++)
                {
                    int keyNumAdd = keyNumAdds[j];
                    if (keyNumAdd == -1) //Add delimiter.
                    {
                        ini.addStructure(HouseGoodGuy, "V19", 256, toTileNum(tilePos, j, 0), 0); //Delimiter.
                    }
                    else
                    {
                        adder(count + keyNumAdd, toTileNum(tilePos, keyNumAddsCount - j, 0), dir);
                        dir = (dir - 64) & 0xFF;
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
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1) //Temperate.
            {
                ini.Theater = "TEMPERATE";
            }
            else if (set == 2) //Desert.
            {
                ini.Theater = "DESERT";
            }
            else if (set == 3) //Winter.
            {
                ini.Theater = "WINTER";
            }
            else throw new ArgumentException();
            TheaterTD theater = TheaterTD.getTheater(ini.Theater);

            ini.setTileSetTable(GroundLayerTD.createTileSetTableClear());

            TilePos tilePosOrg = StartTilePos;
            TilePos tilePos = tilePosOrg;
            for (byte tileId = 0; tileId <= 215; tileId++)
            {
                FileIcnTileSetTDRA fileIcn = theater.getTileSet(tileId);
                for (byte tileIndex = 0; tileIndex < fileIcn.IndexEntryCount; tileIndex++)
                {
                    //Only add tile set index if it's not empty (not defined nor present in theater).
                    //Empty tile index is fine though, but it's returned as an empty frame so we need to check for that.
                    if (fileIcn.isEmptyTileIndex(tileIndex) || !fileIcn.getTile(tileIndex).IsEmpty)
                    {
                        ini.setTileSetTableTile(tileId, tileIndex, toTileNum(tilePos));
                        tilePos.X++;
                        if (tilePos.X >= 59) //Start a new row?
                        {
                            tilePos.X = tilePosOrg.X;
                            tilePos.Y += 2;
                        }
                    }

                    //"P03.TEM" and "P04.TEM" both have a lot of index entries (103!), but only 2 actual tiles
                    //so ignore tile index 3-102 to save space for other tiles on testing map.
                    if (tileIndex >= 2 && (fileIcn.NameUpper == "P03.TEM" || fileIcn.NameUpper == "P04.TEM")) break;
                }
            }

            //Add scouting jeeps at the right side of the map.
            TilePos scoutPos = new TilePos(59, 3);
            for (; scoutPos.Y < 50; scoutPos.Y += 2)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(60, 2);
            ini.addStructure(HouseGoodGuy, "EYE", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 6; i++) //6 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "NUK2", 256, toTileNum(auxPos, 0, i * 3), 0);
            }

            return ini.save(PrintPath, "testTileSets set" + set);
        }

        private static string testTileSetsWeird(int set)
        {
            //Test how the game handles some weird tile set cases. Both the map and the radar renderer.
            //Uses a modified "DESERT.MIX".
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            ini.Theater = "DESERT";

            ini.setTileSetTable(GroundLayerTD.createTileSetTableClear());

            //Set some valid tiles to use as reference locations.
            int tileNum = toTileNum(new TilePos(4, 6));
            for (int i = 0; i < 16; i++, tileNum += 2)
            {
                ini.setTileSetTableTile(82, 0, tileNum); //case 82: fileId = "B1";
            }

            //Write start marker.
            tileNum = toTileNum(new TilePos(4, 8));
            ini.setTileSetTableTile(82, 0, tileNum); //case 82: fileId = "B1";

            //Tile set theater defined but missing.
            //Result (map / radar): HOM (Hall-Of-Mirrors effect) / crash.
            tileNum += 2;
            if (set != 2) //Avoid crashing radar.
            {
                ini.setTileSetTableTile(74, 0, tileNum); //case 74: fileId = "P08";
            }

            //Tile set present in theater but not specified for it.
            //Result: HOM / crash.
            tileNum += 2;
            if (set != 2) //Avoid crashing radar.
            {
                ini.setTileSetTableTile(85, 0, tileNum); //case 85: fileId = "B4";
            }

            //Tile set 0 same as 0xFF.
            //Result: Tile set index is used i.e. not same as 0xFF.
            tileNum += 2;
            ini.setTileSetTableTile(0x00, 3, tileNum); //Tile set id 0x00.

            //Tile set 0xFF with non-zero index. Usually a 0xFF tile set id has a 0x00 tile set index in BIN-files.
            //Result: Tile set index is not used. Calculated from tile X&Y instead.
            tileNum += 2;
            ini.setTileSetTableTile(0xFF, 3, tileNum); //Tile set id 0xFF.

            //Tile set index normal (for comparison).
            tileNum += 2;
            ini.setTileSetTableTile(40, 1, tileNum); //case 40: fileId = "S28";

            //Tile set index empty template tile.
            //Result: Same as if tile set id 0xFF.
            tileNum += 2;
            ini.setTileSetTableTile(40, //case 40: fileId = "S28";
                2, tileNum); //Empty tile index.

            //Tile set index high.
            //Result: Tile index 0 drawn. Index is wrapped around or set to 0?
            tileNum += 2;
            ini.setTileSetTableTile(40, //case 40: fileId = "S28";
                4, tileNum); //File only has 4 tiles (0-3).

            //Tile set index high alternative land type.
            //Result: Tile index 0 drawn, radar land type normal. Index is set to 0? Index isn't in alternative land type list?
            tileNum += 2;
            ini.setTileSetTableTile(193, //case 193: fileId = "SH41";
                10, tileNum); //File only has 9 tiles (0-8). Should wrap around to tile index 1 which uses the alternative land type.

            //Tile set index high alternative land type.
            //Result: Tile index 0 drawn, radar land type normal. Index is set to 0? Index isn't in in alternative land type list?
            tileNum += 2;
            ini.setTileSetTableTile(193, //case 193: fileId = "SH41";
                16, tileNum); //File only has 9 tiles (0-8). Should wrap around to tile index 7 which uses the normal land type.

            //Tile set index in file higher than tiles in it. Uses a modified file.
            //Result: HOM / crash.
            tileNum += 2;
            if (set != 2) //Avoid crashing radar.
            {
                ini.setTileSetTableTile(42, //case 42: fileId = "S30";
                    1, tileNum); //Modify file to make index 1 point to tile >= 4. File only has 4 tiles (0-3).
            }

            //Write end marker.
            tileNum += 2;
            ini.setTileSetTableTile(82, 0, tileNum); //case 82: fileId = "B1";

            //Write the modified file and theater.
            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "DESERT src.MIX");
            FileIcnTileSetTD fileIcn = editor.getFileAs<FileIcnTileSetTD>("S30.DES");
            byte[] fileBytes = fileIcn.readAllBytes();
            fileBytes[fileIcn.IndexEntriesOffset + 1] = 5; //Point index 1 to 6th tile. File only has 4 tiles (0-3).
            File.WriteAllBytes(PrintModifiedPath + "S30INDEX.DES", fileBytes);
            editor.replace("S30.DES", fileBytes);
            editor.save(PrintPath + "DESERT.MIX");

            //Add scouting jeeps at the left and right sides of the map.
            TilePos scoutPos = new TilePos(3, 4);
            for (; scoutPos.Y < 20; scoutPos.Y += 3)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 64);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos, 56, 0), 192);
            }

            if (set == 2)
            {
                //Add radar and power.
                TilePos auxPos = new TilePos(60, 2);
                ini.addStructure(HouseGoodGuy, "EYE", 256, toTileNum(auxPos, 0, 0), 0);
                for (int i = 1; i <= 6; i++) //6 power plants should be enough to power the radar and everything else.
                {
                    ini.addStructure(HouseGoodGuy, "NUK2", 256, toTileNum(auxPos, 0, i * 3), 0);
                }
            }

            return ini.save(PrintPath, "testTileSetsWeird set" + set);
        }

        private static string testRadarSprites(int set)
        {
            //Test how all sprites look on the radar.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            if (set == 1) //Temperate.
            {
                ini.Theater = "TEMPERATE";
            }
            else if (set == 2) //Desert.
            {
                ini.Theater = "DESERT";
            }
            else if (set == 3) //Winter.
            {
                ini.Theater = "WINTER";
            }
            else throw new ArgumentException();
            TheaterTD theater = TheaterTD.getTheater(ini.Theater);

            //Add infantries.
            TilePos startPos = new TilePos(5, 5);
            TilePos tilePos = startPos;
            string[] infantries = new string[]
            {
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10",  "CHAN",
                "DELPHI", "E1", "E2", "E3", "E4", "E5", "E6", "MOEBIUS", "RMBO"
            };
            for (int i = 0; i < infantries.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 58 && i < infantries.Length; tilePos.X += 2, i++)
                {
                    int subPos = i % 5;
                    ini.addInfantry(HouseGoodGuy, infantries[i], 256, toTileNum(tilePos), subPos, 0);
                }
            }

            //Add units.
            tilePos.Y += 1;
            string[] units = new string[]
            {
                "APC", "ARTY", "BGGY", "BIKE", "FTNK", "HARV", "HTNK", "JEEP", "LST",
                "LTNK", "MCV", "MHQ", "MLRS", "MSAM", "MTNK", "STNK", "VICE", "RAPT",
                "STEG", "TREX", "TRIC", "A10", "C17", "HELI", "ORCA", "TRAN"
            };
            for (int i = 0; i < units.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 58 && i < units.Length; tilePos.X += 2, i++)
                {
                    ini.addUnit(HouseGoodGuy, units[i], 256, toTileNum(tilePos), 0);
                }
            }

            //Add gunboat.
            tilePos.Y += 1;
            tilePos.X = startPos.X + 2;
            ini.addUnit(HouseGoodGuy, "BOAT", 256, toTileNum(tilePos), 0);

            //Add structures.
            tilePos.Y += 2;
            string[] structs =
            {
                "AFLD", "ATWR", "BIO", "EYE", "FACT", "FIX", "GTWR", "GUN", "HAND", "HOSP", "HPAD",
                "HQ", "MISS", "NUK2", "NUKE", "OBLI", "PROC", "PYLE", "SAM", "SILO", "TMPL", "WEAP",
                "ARCO", "V01", "V02", "V03", "V04", "V05", "V06", "V07", "V08", "V09", "V10", "V11",
                "V12", "V13", "V14", "V15", "V16", "V17", "V18", "V19", "V20", "V21", "V22", "V23",
                "V24", "V25", "V26", "V27", "V28", "V29", "V30", "V31", "V32", "V33", "V34", "V35", "V36", "V37"
            };
            for (int i = 0; i < structs.Length; )
            {
                tilePos.X = startPos.X;
                int maxHeight = 0;
                for (; tilePos.X < 58 && i < structs.Length; i++)
                {
                    ini.addStructure(HouseGoodGuy, structs[i], 256, toTileNum(tilePos), 0);
                    Size size = getSizeInTilesUp(getSpriteSize(structs[i], theater));
                    tilePos.X += size.Width + 1;
                    maxHeight = Math.Max(maxHeight, size.Height);
                }
                tilePos.Y += maxHeight + 1;
            }

            //Add terrains.
            tilePos.Y += 1;
            string[] terrains =
            {
                "ROCK1", "ROCK2", "ROCK3", "ROCK4", "ROCK5", "ROCK6", "ROCK7", "SPLIT2", "SPLIT3",
                "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09", "T10", "T11", "T12",
                "T13", "T14", "T15", "T16", "T17", "T18", "TC01", "TC02", "TC03", "TC04", "TC05"
            };
            for (int i = 0; i < terrains.Length; )
            {
                tilePos.X = startPos.X;
                int maxHeight = 0;
                for (; tilePos.X < 58 && i < terrains.Length; i++)
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
                "BARB", "BRIK", "CYCL", "SBAG", "WOOD", "TI1", "TI2", "TI3", "TI4", "TI5", "TI6",
                "TI7", "TI8", "TI9", "TI10", "TI11", "TI12", "V12", "V13", "V14", "V15", "V16",
                "V17", "V18", "FPLS", "SCRATE", "WCRATE", "CONC", "ROAD", "SQUISH"
            };
            for (int i = 0; i < overlays.Length; tilePos.Y += 2)
            {
                tilePos.X = startPos.X;
                for (; tilePos.X < 44 && i < overlays.Length; tilePos.X += 2, i++)
                {
                    ini.addOverlay(overlays[i], toTileNum(tilePos));
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
                for (; tilePos.X < 44 && i < smudges.Length; tilePos.X += 2, i++)
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
                "BadGuy", "Neutral", "Special", "Multi1", "Multi2", "Multi3", "Multi4", "Multi5", "Multi6"
            };
            Size houseTestSize = new Size(1, 3);
            tilePos.X = startPos.X + 6;
            for (int i = 0; i < houses.Length; i++, tilePos.X += 2)
            {
                //Add a concrete wall around tested house.
                for (int brikY = -1; brikY <= houseTestSize.Height; brikY++)
                {
                    for (int brikX = -1; brikX <= houseTestSize.Width; brikX++)
                    {
                        if ((brikX == -1 && i == 0) || //Left column. Only if first house.
                            (brikX == houseTestSize.Width) || //Right column.
                            (brikY == -1) || (brikY == houseTestSize.Height)) //Top and bottom row.
                        {
                            ini.addOverlay("BRIK", toTileNum(tilePos.getOffset(brikX, brikY)));
                        }
                    }
                }
                //Add structure+unit.
                ini.addStructure(houses[i], "OBLI", 256, toTileNum(tilePos.getOffset(0, 0)), 0); //Should have no power and therefore harmless.
                ini.addUnit(houses[i], "MHQ", 256, toTileNum(tilePos.getOffset(0, 2)), 128);
            }

            //Add scouting jeeps at the left and right sides of the map.
            TilePos scoutPos = new TilePos(3, 4);
            for (; scoutPos.Y < 50; scoutPos.Y += 3)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 64);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos, 56, 0), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(60, 2);
            ini.addStructure(HouseGoodGuy, "EYE", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 6; i++) //6 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "NUK2", 256, toTileNum(auxPos, 0, i * 3), 0);
            }

            return ini.save(PrintPath, "testRadarSprites set" + set);
        }

        private static string testRadarScalingDos()
        {
            //Test how a scaled pattern will look on the DOS version's radar. Maybe figure out a scaling method from it.
            //Only background tiles, overlays and terrains are scaled so it's enough to just test them.
            //Uses a modified "DESERT.MIX" and "CONQUER.MIX".
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            ini.Theater = "DESERT";

            //Add a bg tile.
            ini.setTileSetTableTile(82, 0, toTileNum(StartTilePos.getOffset(1, 1))); //B1=082: Boulder1.

            //Add an overlay.
            ini.addOverlay("BARB", toTileNum(StartTilePos.getOffset(3, 1)));

            //Add a terrain.
            ini.addTerrain("T04", toTileNum(StartTilePos.getOffset(5, 1)));

            //Write the modified files and theater.
            Frame paletteFrame = new Frame(24, 24);
            paletteFrame.clear(254);
            paletteFrame.draw(Frame.createPalette(), new Point(0, 0));
            FileMixArchiveWw.Editor editorDes = FileMixArchiveWw.Editor.open(TemplatePath + "DESERT src.MIX");
            FileMixArchiveWw.Editor editorCon = FileMixArchiveWw.Editor.open(TemplatePath + "CONQUER src.MIX");
            //Change clear bg tile to solid color so tests are easier to read.
            {
                string fileName = "CLEAR1.DES";
                FileIcnTileSetTD fileIcn = editorDes.getFileAs<FileIcnTileSetTD>(fileName);
                byte[] fileBytes = fileIcn.readAllBytes();
                for (int i = 0; i < 24 * 24 * 16; i++) //Set all 16 tiles (each 24*24 big) in the set to a solid color.
                {
                    fileBytes[fileIcn.TileEntriesOffset + i] = 2;
                }
                File.WriteAllBytes(PrintModifiedPath + fileName, fileBytes);
                editorDes.replace(fileName, fileBytes);
            }
            //Write a greyscale palette.
            {
                string fileName = "DESERT.PAL";
                FilePalPalette6Bit filePal = editorDes.getFileAs<FilePalPalette6Bit>(fileName);
                byte[] fileBytes = filePal.readAllBytes();
                for (int i = 0; i < 256; i++)
                {
                    //Use lower 3 bits in each color channel to store the palette index (max 512, but only 0-255 used).
                    fileBytes[(i * 3) + 0] = (byte)((fileBytes[(i * 3) + 0] & 0xF8) | ((i >> 0) & 0x07));
                    fileBytes[(i * 3) + 1] = (byte)((fileBytes[(i * 3) + 1] & 0xF8) | ((i >> 3) & 0x07));
                    fileBytes[(i * 3) + 2] = (byte)((fileBytes[(i * 3) + 2] & 0xF8) | ((i >> 6) & 0x07));
                }
                File.WriteAllBytes(PrintModifiedPath + fileName, fileBytes);
                editorDes.replace(fileName, fileBytes);
            }
            //Write a full palette to bg tile.
            {
                string fileName = "B1.DES";
                FileIcnTileSetTD fileIcn = editorDes.getFileAs<FileIcnTileSetTD>(fileName);
                byte[] fileBytes = fileIcn.readAllBytes();
                Buffer.BlockCopy(paletteFrame.Pixels, 0, fileBytes, (int)fileIcn.TileEntriesOffset, paletteFrame.Length);
                File.WriteAllBytes(PrintModifiedPath + fileName, fileBytes);
                editorDes.replace(fileName, fileBytes);
            }

            //Write a full palette to terrain.
            {
                string fileName = "T04.DES";
                FileShpSpriteSetTDRA fileShp = editorDes.getFileAs<FileShpSpriteSetTDRA>(fileName);
                Frame[] shpFrames = fileShp.copyFrames();
                shpFrames[0].draw(paletteFrame, new Point(0, 0));
                FileShpSpriteSetTDRA.writeShp(PrintModifiedPath + fileName, shpFrames, fileShp.Size);
                editorDes.replace(fileName, PrintModifiedPath + fileName);
            }

            //Write a full palette to overlay.
            {
                string fileName = "BARB.SHP";
                FileShpSpriteSetTDRA fileShp = editorCon.getFileAs<FileShpSpriteSetTDRA>(fileName);
                Frame[] shpFrames = fileShp.copyFrames();
                shpFrames[0].draw(paletteFrame, new Point(0, 0));
                FileShpSpriteSetTDRA.writeShp(PrintModifiedPath + fileName, shpFrames, fileShp.Size);
                editorCon.replace(fileName, PrintModifiedPath + fileName);
            }
            editorDes.save(PrintPath + "DESERT.MIX");
            editorCon.save(PrintPath + "CONQUER.MIX");

            //Add scouting jeeps at the left and right sides of the map.
            TilePos scoutPos = new TilePos(3, 4);
            for (; scoutPos.Y < 50; scoutPos.Y += 3)
            {
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos), 64);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(scoutPos, 56, 0), 192);
            }

            //Add radar and power.
            TilePos auxPos = new TilePos(60, 2);
            ini.addStructure(HouseGoodGuy, "EYE", 256, toTileNum(auxPos, 0, 0), 0);
            for (int i = 1; i <= 6; i++) //6 power plants should be enough to power the radar and everything else.
            {
                ini.addStructure(HouseGoodGuy, "NUK2", 256, toTileNum(auxPos, 0, i * 3), 0);
            }

            return ini.save(PrintPath, "testRadarScalingDos");

            ////Run the game with the modified files and then use something like below to read back the pattern
            ////from each tested object (tile, terrain and overlay). Save a screenshot and copy-paste each object.
            //Bitmap bmp = new Bitmap(Program.ProgramPath + "testpattern.png");
            //StringBuilder sb = new StringBuilder();
            //for (int y = 0; y < bmp.Height; y++)
            //{
            //    for (int x = 0; x < bmp.Width; x++)
            //    {
            //        Color c = bmp.GetPixel(x, y);
            //        int index = (((c.R >> 2) & 0x07) << 0) | (((c.G >> 2) & 0x07) << 3) | (((c.B >> 2) & 0x07) << 6);
            //        sb.Append(index.ToString("D3") + ",");
            //    }
            //    sb.AppendLine();
            //}
            //File.WriteAllText(Program.ProgramPath + "testpattern.txt", sb.ToString());
        }

        private static string testOverlayConcrete32()
        {
            //Test how concrete (CONC) is affected by up to 5 adjacent concrete tiles (32 combinations).
            //Looking at the source code it seems like concrete is only affected by 5 tiles.
            //Even: N,S,SW,W,NW
            //Odd: N,NE,E,SE,S
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);

            TilePos orgPos = new TilePos(6, 6);
            TilePos tilePos = orgPos;
            bool isEven = true;
            for (int ind = 0; ind < 64; ind++)
            {
                //Add a center concrete tile and surround its west/east side with up to five concrete tiles.
                ini.addOverlay("CONC", toTileNum(tilePos)); //Center.

                int i = ind & 0x1F; //0-31.
                if (isEven) //even: {FACING_N, FACING_S, FACING_SW, FACING_W, FACING_NW};
                {
                    if ((i & (1 << 0)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, -1)); //N.
                    if ((i & (1 << 1)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, 1)); //S.
                    if ((i & (1 << 2)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, 1)); //SW.
                    if ((i & (1 << 3)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, 0)); //W.
                    if ((i & (1 << 4)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, -1)); //NW.

                    //Add scout right of block.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, 2, 0), 128);
                }
                else //odd: {FACING_N, FACING_NE, FACING_E, FACING_SE, FACING_S};
                {
                    if ((i & (1 << 0)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, -1)); //N.
                    if ((i & (1 << 1)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, -1)); //NE.
                    if ((i & (1 << 2)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, 0)); //E.
                    if ((i & (1 << 3)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, 1)); //SE.
                    if ((i & (1 << 4)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, 1)); //S.

                    //Add scout left of block.
                    ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -2, 0), 128);
                }

                tilePos.X += 6;
                if (tilePos.X >= 61)
                {
                    tilePos.X = orgPos.X;
                    tilePos.Y += 6;
                }

                if (ind == 31) //Start testing odd?
                {
                    tilePos.X = orgPos.X = 5;
                    tilePos.Y += 6;
                    isEven = false;
                }
            }

            return ini.save(PrintPath, "testOverlayConcrete32");
        }

        private static string testOverlayConcrete256(int set)
        {
            //Test how concrete (CONC) is affected by up to 8 adjacent concrete tiles (256 combinations).
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);

            int startI = set == 1 || set == 3 ? 0 : 128; //0-127 or 128-255?
            int endI = set == 1 || set == 3 ? 128 : 256;
            int offset = set == 1 || set == 2 ? 0 : 1; //Even or odd offset?
            if (set < 1 || set > 4) throw new ArgumentException();

            TilePos orgPos = new TilePos(6 + offset, 6);
            TilePos tilePos = orgPos;
            for (int i = startI; i < endI; i++)
            {
                //Add a center concrete tile and surround it with up to eight concrete tiles.
                ini.addOverlay("CONC", toTileNum(tilePos)); //Center.
                if ((i & (1 << 0)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, -1)); //N.
                if ((i & (1 << 1)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, -1)); //NE.
                if ((i & (1 << 2)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, 0)); //E.
                if ((i & (1 << 3)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 1, 1)); //SE.
                if ((i & (1 << 4)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, 0, 1)); //S.
                if ((i & (1 << 5)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, 1)); //SW.
                if ((i & (1 << 6)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, 0)); //W.
                if ((i & (1 << 7)) != 0) ini.addOverlay("CONC", toTileNum(tilePos, -1, -1)); //NW.

                //Add scout left of block.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -2, 0), 128);

                tilePos.X += 4;
                if (tilePos.X >= 60)
                {
                    tilePos.X = orgPos.X;
                    tilePos.Y += 4;
                }
            }

            return ini.save(PrintPath, "testOverlayConcrete256 set" + set);
        }

        private static string testOverlayConcretePair256(int set)
        {
            //Test how concrete (CONC) is affected by up to 8 adjacent concrete tiles (256 combinations).
            //Try pairs of even&odd tiles instead of single tiles.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);

            if (set < 1 || set > 8) throw new ArgumentException();
            int startI = (256 / 4) * ((set - 1) % 4);
            int endI = (256 / 4) * (set - (set <= 4 ? 0 : 4));
            int offset = set <= 4 ? 0 : 1; //Even or odd offset?

            //if (set < 1 || set > 10) throw new ArgumentException();
            //int startI = (256 / 5) * ((set - 1) % 5);
            //int endI = (256 / 5) * (set - (set <= 5 ? 0 : 5));
            //int offset = set <= 5 ? 0 : 1; //Even or odd offset?

            if (startI != 0) startI += 1;
            if (endI > 255) endI = 255;

            Action<TestsIniAdderTD, TilePos, int, int> addConcPair = (TestsIniAdderTD ia, TilePos tp, int offX, int offY) =>
            {
                ia.addOverlay("CONC", toTileNum(tp, offX + 0, offY)); //Left.
                ia.addOverlay("CONC", toTileNum(tp, offX + 1, offY)); //Right.
            };

            TilePos orgPos = new TilePos(6 + offset, 6);
            TilePos tilePos = orgPos;
            for (int i = startI; i <= endI; i++)
            {
                //Add a center concrete tile pair and surround it with up to eight concrete tile pairs.
                addConcPair(ini, tilePos, 0, 0); //Center.
                if ((i & (1 << 0)) != 0) addConcPair(ini, tilePos, 0, -1); //N.
                if ((i & (1 << 1)) != 0) addConcPair(ini, tilePos, 2, -1); //NE.
                if ((i & (1 << 2)) != 0) addConcPair(ini, tilePos, 2, 0); //E.
                if ((i & (1 << 3)) != 0) addConcPair(ini, tilePos, 2, 1); //SE.
                if ((i & (1 << 4)) != 0) addConcPair(ini, tilePos, 0, 1); //S.
                if ((i & (1 << 5)) != 0) addConcPair(ini, tilePos, -2, 1); //SW.
                if ((i & (1 << 6)) != 0) addConcPair(ini, tilePos, -2, 0); //W.
                if ((i & (1 << 7)) != 0) addConcPair(ini, tilePos, -2, -1); //NW.

                //Add scout left and top of block.
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, -3, 0), 128);
                ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos, 0, -2), 128);

                tilePos.X += 8;
                if (tilePos.X >= 58)
                {
                    tilePos.X = orgPos.X;
                    tilePos.Y += 5;
                }
            }

            return ini.save(PrintPath, "testOverlayConcretePair256 set" + set);
        }

        private static string testTemplate()
        {
            //Test [TEMPLATE] section and some weird cases in it. Only read if BIN-file is missing.
            //Uses a modified "TEMPERAT.MIX".
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
            ini.Theater = "TEMPERATE";

            TilePos tilePos = StartTilePos;
            tilePos.Y += 2;

            //Normal with an empty tile index.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "D01");
            tilePos.Y += 4;

            //Undefined, but file present in theater. Uses a modified theater!
            //Result: Nothing drawn.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "S39");
            tilePos.Y += 4;

            //Defined, but file missing in theater. Uses a modified theater!
            //Result: Nothing drawn here nor afterwards!!! [TEMPLATE] section drawing/parsing is aborted?
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            //ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "S38");
            tilePos.Y += 4;

            //1*1 size, but multiple tiles where one is transparent black (transparent flag isn't set).
            //Result: Only first tile drawn.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "P03");
            tilePos.Y += 4;

            //Clear with different offsets.
            //Result: Ignored? No change to background with all clear tiles.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "CLEAR1");
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(6, 1)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(7, 1)).ToString(), "CLEAR1");
            tilePos.Y += 6;

            //Transparent black tile and transparent flag 0/1. Uses a modified theater!
            //Result: Black tile if flag 0, Hall-of-Mirror effect if flag 1.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "D29"); //D29 = modified D28 with black tile and flag 0.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(6, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(7, 0)).ToString(), "D30"); //D30 = modified D28 with black tile and flag 1.
            tilePos.Y += 4;

            //Unknown id.
            //Result: Nothing drawn.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "JEEP");
            tilePos.Y += 4;

            //Overlaps, add right last in first test. Left last in second test.
            //Result: Last added (INI-file order) seems to replace existing. Empty tiles (0xFF) are ignored though and don't replace existing.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(0, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(1, 0)).ToString(), "D01"); //Left first.
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(2, 0)).ToString(), "D01"); //Right last.
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(tilePos.getOffset(4, 0)), 0);
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(6, 0)).ToString(), "D01"); //Right first.
            ini.updateKey("TEMPLATE", toTileNum(tilePos.getOffset(5, 0)).ToString(), "D01"); //Left last.
            tilePos.Y += 4;

            //At the edge.
            //Result: Didn't crash! Template just continues on next row i.e. tile is added if tile number is in range (0-4095).
            //ini.updateKey("TEMPLATE", toTileNum(new TilePos(1, 1)).ToString(), "BRIDGE2");
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(new TilePos(61, 7)), 0);
            ini.updateKey("TEMPLATE", toTileNum(new TilePos(62, 7)).ToString(), "BRIDGE2");
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(new TilePos(6, 62)), 0);
            ini.updateKey("TEMPLATE", toTileNum(new TilePos(7, 62)).ToString(), "BRIDGE2");
            ini.addUnit(HouseGoodGuy, "JEEP", 256, toTileNum(new TilePos(61, 62)), 0);
            ini.updateKey("TEMPLATE", toTileNum(new TilePos(62, 62)).ToString(), "BRIDGE2");

            //Write the modified files and theater.
            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "TEMPERAT src.MIX");
            {
                FileIcnTileSetTD fileIcn = editor.getFileAs<FileIcnTileSetTD>("S38.TEM");
                editor.add("S39.TEM", fileIcn.readAllBytes()); //Add an undefined ICN-file to theater.
                editor.remove("S38.TEM"); //Remove a defined ICN-file in theater.
            }
            {
                //Make an ICN-file with a transparent black tile and transparent flag 0/1.
                FileIcnTileSetTD fileIcn = editor.getFileAs<FileIcnTileSetTD>("D28.TEM");
                byte[] fileBytes = fileIcn.readAllBytes();
                for (int i = 0; i < 24 * 24; i++) //Set first tile entry to transparent black.
                {
                    fileBytes[fileIcn.TileEntriesOffset + i] = 0;
                }
                fileBytes[fileIcn.TransFlagEntriesOffset + 0] = 0; //0 flag for tile 0.
                File.WriteAllBytes(PrintModifiedPath + "D28T0.TEM", fileBytes);
                editor.replace("D29.TEM", fileBytes.takeBytes()); //Make a copy, we'll edit the file some more.
                fileBytes[fileIcn.TransFlagEntriesOffset + 0] = 1; //1 flag for tile 0.
                File.WriteAllBytes(PrintModifiedPath + "D28T1.TEM", fileBytes);
                editor.replace("D30.TEM", fileBytes);
            }
            editor.save(PrintPath + "TEMPERAT.MIX");

            return ini.save(PrintPath, "testTemplate");
        }

        private static string sprite256Rotator(Action<TestsIniAdderTD, int, int> adder, Size size, string testName)
        {
            //Test sprite directions 0-257 and what frame indices it produces.
            TestsIniAdderTD ini = new TestsIniAdderTD(TemplateIniPathGoodGuy);
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
            saveToGeneralMix(iniPaths, false);
        }

        private static void saveToGeneralMix(List<string> iniPaths, bool doRemoveBinFile)
        {
            if (iniPaths.Count == 0)
            {
                throw new ArgumentException();
            }

            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "GENERAL src.MIX");
            int countGoodGuy = 1;
            int countBadGuy = 1;
            foreach (string iniPath in iniPaths)
            {
                //Calculate name to replace from path.
                string nameToReplace = Path.GetFileName(iniPath).Split(' ')[0].ToLowerInvariant() + ".ini";
                if (nameToReplace == "scg01ea.ini") //Use default goodguy files?
                {
                    nameToReplace = "scg" + countGoodGuy.ToString("D2") + "ea.ini";
                    countGoodGuy++;
                }
                else if (nameToReplace == "scb01ea.ini") //Use default badguy files?
                {
                    nameToReplace = "scb" + countBadGuy.ToString("D2") + "ea.ini";
                    countBadGuy++;
                }
                editor.replace(nameToReplace, iniPath);

                //Also replace or remove corresponding BIN-file if it exists.
                string binNameToReplace = nameToReplace.ReplaceEnd("bin");
                if (doRemoveBinFile && editor.hasFile(binNameToReplace)) //Remove BIN-file? For testing [TEMPLATE] section.
                {
                    editor.remove(binNameToReplace);
                }
                else //Replace BIN-file in MIX-archive with an external one.
                {
                    string binPath = iniPath.ReplaceEnd("bin");
                    if (File.Exists(binPath))
                    {
                        editor.replace(binNameToReplace, binPath);
                    }
                }
            }
            editor.save(PrintPath + "GENERAL.MIX"); //Windows version.
            //editor.save(PrintPath + "GENERAL.GDI"); //DOS version.
        }

        private static void saveToDinosaurMix(string iniPath) //Special for dinosaurs.
        {
            //Seems like dinosaurs only work if you:
            //-start the game in funpark mode (add "funpark –cd." without quotes to shortcut).
            //-need to have GDI95 CD or ISO mounted.
            //-replace bin/ini-files in "SC-000.MIX".
            //-easiest to replace "scj01ea" with test-file and start a new game.

            FileMixArchiveWw.Editor editor = FileMixArchiveWw.Editor.open(TemplatePath + "SC-000 src.MIX");
            editor.replace("scj01ea.ini", iniPath);
            //Also replace corresponding BIN-file.
            editor.replace("scj01ea.bin", iniPath.ReplaceEnd("bin"));
            editor.save(PrintPath + "SC-000.MIX");
        }

        private static void writeModifiedHarv(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a HARV sprite with a full palette to see what alt color scheme houses use.
            writeModifiedPalette(conquerEditor, "HARV.SHP", new Point(0, 0));
        }

        private static void writeModifiedHpad(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create an HPAD sprite with a full palette to see how colors are affected by an air
            //shadow from an ORCA landing/taking off.
            writeModifiedPalette(conquerEditor, "HPAD.SHP", new Point(16, 15));
        }

        private static void writeModifiedMhq(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a MHQ sprite with a full palette to see what color scheme houses use.
            writeModifiedPalette(conquerEditor, "MHQ.SHP", new Point(0, 0), 32); //Modify turret frames.
        }

        private static void writeModifiedOrca(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a ORCA sprite with a solid color to see how an air shadow affect colors.
            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(conquerEditor.getFile("ORCA.SHP"));
            Frame[] frames = fileShp.copyFrames();
            foreach (Frame frame in frames)
            {
                frame.clear(1);
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, fileShp.Size);
            conquerEditor.replace(fileShp.Name, path);
        }

        private static void writeModifiedSilo(FileMixArchiveWw.Editor conquerEditor)
        {
            //Create a SILO sprite with a solid shadow at left side and a full palette 24 pixels right.
            //This way we can overlap SILOs and see what a full palette looks like with a shadow over it.
            Frame paletteFrame = Frame.createPalette();
            Frame shadowFrame = Frame.createSolid(4, paletteFrame.Size); //Solid shadow (index 4 green).

            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(conquerEditor.getFile("SILO.SHP"));
            Frame[] frames = fileShp.copyFrames();
            foreach (Frame frame in frames)
            {
                frame.write(shadowFrame, new Point(0, 0));
                frame.write(paletteFrame, new Point(24, 0));
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, fileShp.Size);
            conquerEditor.replace(fileShp.Name, path);
        }

        private static void writeModifiedPalette(FileMixArchiveWw.Editor editor, string nameShp, Point position)
        {
            writeModifiedPalette(editor, nameShp, position, 0);
        }

        private static void writeModifiedPalette(FileMixArchiveWw.Editor editor, string nameShp, Point position, int startInd)
        {
            //Write a full palette (16x16) to position in sprite's frames.
            Frame paletteFrame = Frame.createPalette();

            FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(editor.getFile(nameShp));
            Frame[] frames = fileShp.copyFrames();
            for (int i = startInd; i < frames.Length; i++)
            {
                frames[i].write(paletteFrame, position);
            }
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetTDRA.writeShp(path, frames, fileShp.Size);
            editor.replace(fileShp.Name, path);
        }

        private static void writeModifiedTiberium(FileMixArchiveWw.Editor theaterEditor)
        {
            //Create tiberium sprites (TI1-TI12) with id and frame number. Makes it easier to see how tiberium sprites are used.
            TextDrawer td = new TextDrawer(MapInfoDrawerTDRA.getFileFnt6x10(),
                new byte[] { 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); //Yellow text.
            for (int i = 1; i <= 12; i++)
            {
                string tibShpName = "TI" + i.ToString() + ".TEM";
                FileShpSpriteSetTDRA fileShp = new FileShpSpriteSetTDRA(theaterEditor.getFile(tibShpName));
                Size shpSize = fileShp.Size;
                Frame frameBg = Frame.createSolid(1, 5, shpSize); //Magenta with yellow edge.
                Frame[] frames = new Frame[fileShp.FrameCount];
                //Draw tiberium id and frame number on frames.
                for (int k = 0; k < frames.Length; k++)
                {
                    frames[k] = new Frame(frameBg); //Copy background frame.
                    TextDrawer.TextDrawInfo textDi = td.getTextDrawInfo((i * 16 + k).ToString("X"));
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
            return MapTD.toTileNum(tilePos);
        }

        private static int toTileNum(TilePos tilePos, int dx, int dy)
        {
            return MapTD.toTileNum(tilePos.getOffset(dx, dy));
        }

        private static Size getSpriteSize(string sprId, TheaterTD theater)
        {
            return theater.getSpriteSet(sprId).Size;
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
