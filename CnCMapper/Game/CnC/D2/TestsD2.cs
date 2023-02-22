using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;
using System.Drawing;

namespace CnCMapper.Game.CnC.D2
{
    //Creates test map/mission INI-files that is used to compare output from game with.

    //The input files folder (test\D2\template) should contain these files:
    //-DUNE src.PAK
    //-SCENARIO src.PAK
    //They are just copies from the game folder with " src" added to their names.
    //It should also contain "SCENA001.INI" which is a copy from the game, but modified
    //to fit testing better. Essentially it is a blank map with most objects stripped and set to max size.

    //I used nyerguds dune 2 editor to increase the sight (from 2 to 64) of trikes so you only
    //need two scout trike to reveal the map. Makes testing easier.
    //http://nyerguds.arsaneus-design.com/dune/dune2edit/

    //A hex editor can be used to change the map scale table at 0x0003B696 in the EXE-file.
    //Change  01,00,01,00,3E,00,3E,00
    //to      00,00,00,00,40,00,40,00
    //This will make it possible to view the edges of the map which was useful in a few tests.

    static class TestsD2
    {
        private const int TilesPerLine = MapD2.WidthInTiles;
        private const int StartTileNum = 3 + (TilesPerLine * 5);
        private static readonly TilePos StartTilePos = new TilePos(3, 5);

        private static readonly string TestPath = Program.DebugBasePath + "test\\D2\\";
        private static readonly string TemplatePath = TestPath + "template\\"; //Files used as input (read only).
        public static readonly string PrintPath = TestPath + "print\\"; //Final output of modified files.
        private static readonly string PrintModifiedPath = TestPath + "modified\\"; //Temp output of modified files.
        private static readonly string TemplateIniPath = TemplatePath + "SCENA001.INI";

        private const string HouseGoodGuy = HouseD2.IdAtreides;
        private const string HouseBadGuy = HouseD2.IdHarkonnen;

        //All valid sprites. For checking sprite type and copy-pasting from into tests.
        //Tests should use their own local sprite arrays so they are self-contained as much as possible.
        private static readonly string[] SprIdUnits = new string[]
        {
            "Carryall", "'Thopter", "Frigate",
            "Soldier", "Infantry", "Trooper", "Troopers", "Saboteur",
            "Tank", "Siege Tank", "Devastator", "Sonic Tank", "Launcher", "Deviator",
            "Trike", "Raider Trike", "Quad",
            "Harvester", "MCV", "Sandworm"
        };

        private static readonly string[] SprIdStructures = new string[]
        {
            "Concrete", "Concrete4", "Wall", "Turret", "R-Turret",
            "Const Yard", "Windtrap", "Barracks", "Light Fctry", "Heavy Fctry", "Hi-Tech",
            "Refinery", "Spice Silo", "Outpost", "Repair", "IX", "WOR", "Palace", "Starport"
        };

        public static void run()
        {
            List<MapD2> maps = new List<MapD2>();
            FolderContainer container = new FolderContainer(PrintPath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (fileIni.isMapD2())
                {
                    maps.Add(MapD2.create(fileIni));
                }
            }
            MapD2.debugSaveRenderAll(maps);
        }

        public static void print()
        {
            Directory.CreateDirectory(PrintPath);
            Directory.CreateDirectory(PrintModifiedPath);

            List<string> iniPaths = new List<string>();

            //iniPaths.Add(testUnits8Rots(1));
            //iniPaths.Add(testUnits8Rots(2));
            //iniPaths.Add(testUnits8Rots(3));
            //iniPaths.Add(testUnits8Rots(4));
            //iniPaths.Add(testUnits8Rots(5));
            //iniPaths.Add(testUnits8Rots(6));
            //iniPaths.Add(testUnits8Rots(7)); //TODO: Saboteur. Added, but invisible/cloaked. Some way to test this unit?
            //iniPaths.Add(testUnits8Rots(8));
            //iniPaths.Add(testUnits8Rots(9)); //TODO: Sandworm. Added, but invisible/cloaked. Some way to test this unit?
            //iniPaths.Add(testUnits8Rots(10));
            //iniPaths.Add(testUnits8Rots(11));
            //iniPaths.Add(testUnits8Rots(12));

            //iniPaths.Add(testUnitsRemap()); //Uses a modified "DUNE.PAK".
            //iniPaths.Add(testUnitsRemapRange()); //Uses a modified "DUNE.PAK".

            //iniPaths.Add(testStructures());
            //iniPaths.Add(testStructuresRemap()); //Uses a modified "DUNE.PAK".
            //iniPaths.Add(testStructuresRemapRange()); //Uses a modified "DUNE.PAK".
            //iniPaths.Add(testStructuresPriority());
            //iniPaths.Add(testStructuresFormatPriority());
            //iniPaths.Add(testStructuresIxError());
            //iniPaths.Add(testWalls());

            //iniPaths.Add(testIniKeys());

            //iniPaths.Add(testSpiceFields());

            //iniPaths.Add(testRadarTiles());
            //iniPaths.Add(testRadarUnits());
            //iniPaths.Add(testRadarStructures());

            //saveToScenarioPak(iniPaths); //Save tests into SCENARIO.PAK. Copy it to game folder to test in game.
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

            //Dune 2 has a really low max unit count so can only test like 3-4 blocks at a time.
            //But a bigger problem is that units rotate around during gameplay so we need to take
            //a screenshot directly at start and only 2 blocks will fit on a screen.

            //Max aircraft count is even lower? Something like 12?
            //So let's test only the 8 directions for them. The edge values seems to be the same
            //as other units from some quick tests I did anyway.

            //Frigate max count is only 1?

            string[] us;
            int dirsAdd = 1;
            string house = HouseGoodGuy;
            if (set == 1) //Units.
            {
                //Skip raider trike. It is visually identical to a normal trike.
                us = new string[] { "Trike", /*"Raider Trike",*/ "Quad" };
            }
            else if (set == 2) //Turrets 1.
            {
                //Skip deviator? It is visually identical to a launcher.
                us = new string[] { "Launcher", "Deviator" };
            }
            else if (set == 3) //Turrets 2.
            {
                us = new string[] { "Tank", "Siege Tank" };
            }
            else if (set == 4) //Turrets 3.
            {
                us = new string[] { "Devastator", "Sonic Tank" };
            }
            else if (set == 5) //Infantry 1.
            {
                us = new string[] { "Infantry", "Troopers" };
            }
            else if (set == 6) //Infantry 2.
            {
                us = new string[] { "Soldier", "Trooper" };
            }
            else if (set == 7) //Saboteur.
            {
                us = new string[] { "Saboteur" };
                house = HouseD2.IdOrdos;
            }
            else if (set == 8) //Other.
            {
                us = new string[] { "Harvester", "MCV" };
            }
            else if (set == 9) //Sandworm.
            {
                us = new string[] { "Sandworm" };
            }
            else if (set == 10) //Carryall.
            {
                us = new string[] { "Carryall" };
                dirsAdd = 3; //Skip edge cases.
            }
            else if (set == 11) //'Thopter.
            {
                us = new string[] { "'Thopter" };
                dirsAdd = 3; //Skip edge cases.
            }
            else if (set == 12) //Frigate. Only max one added.
            {
                us = new string[] { "Frigate" };
                dirsAdd = 3; //Skip edge cases.
            }
            else throw new ArgumentException();

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            for (int i = 0; i < us.Length; i++)
            {
                TilePos blockPos = StartTilePos.getOffset((i % 4) * 8, (i / 4) * 8); //8x8 tile blocks. 4 per row.
                for (int y = 0; y < 7; y += dirsAdd)
                {
                    for (int x = 0; x < 7; x += dirsAdd)
                    {
                        int dir = dirs[(y * 7) + x];
                        if (dir != 1 && dir != 257)
                        {
                            int tileNum = toTileNum(blockPos, x, y);
                            ini.addUnit(house, us[i], 256, tileNum, dir);
                        }
                    }
                }
            }
            return ini.save(PrintPath, "testUnits8Rots set" + set);
        }

        private static string testUnitsRemap()
        {
            //Test together with modified graphic-files to display remapped palette with different houses.
            //Also tests aircraft shadow remapping in a way that makes it possible to capture the remap table from the game.
            //Uses a modified "DUNE.PAK".

            //Sometimes (at take off?) in the game an aircraft will cast two shadows (stacked) on the ground
            //making the shadow extra dark. One will have lower priority than ground units.

            string[] houses = new string[] { "Atreides", "Fremen", "Harkonnen", "Mercenary", "Ordos", "Sardaukar" };

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;
            for (int houseInd = 0; houseInd < houses.Length + 1; houseInd++) //houses.Length + 1 to add one last wall column.
            {
                TilePos blockPos = tilePos.getOffset(2 * houseInd, 0);
                for (int y = 0; y < 3; y++) //Add wall column.
                {
                    ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(blockPos, 0, y));
                }
                if (houseInd == houses.Length) break; //Added last wall column i.e. no more houses to add.
                blockPos.X += 1;
                int tileNum = toTileNum(blockPos);
                for (int y = 0; y < 3; y++) //Add unit column.
                {
                    if (y == 1)
                    {
                        ini.addUnit(houses[houseInd], "Harvester", 256, tileNum + TilesPerLine, 128);
                    }
                    else
                    {
                        ini.addConcrete(HouseGoodGuy, "Wall", tileNum + (TilesPerLine * y));
                    }
                }
            }
            tilePos.Y += 3;

            //Add some aircrafts to test aircraft shadow remapping.
            //Sprite frames modified to show a solid colored square.
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 0, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 2, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 4, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 6, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 8, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 10, 0), 0);
            ini.addUnit(HouseGoodGuy, "Carryall", 256, toTileNum(tilePos, 12, 0), 0);

            //Add some units one tile below aircrafts.
            //Sprite frames modified to show all palette colors.
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 0, 1), 0);
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 2, 1), 0);
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 4, 1), 32);
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 6, 1), 64);
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 8, 1), 96);
            ini.addUnit(HouseGoodGuy, "Quad", 256, toTileNum(tilePos, 10, 1), 128);
            ini.addUnit(HouseGoodGuy, "Trike", 256, toTileNum(tilePos, 12, 1), 0);

            //Modify files used in this test.
            //DUNE.PAK: "UNITS.SHP", ("IBM.PAL")
            FilePakArchiveWw.Editor duneEditor = FilePakArchiveWw.Editor.open(TemplatePath + "DUNE src.PAK");
            writeModifiedShp(duneEditor, "UNITS.SHP", (Frame[] frames, byte[][] remaps) =>
            {
                //Create a Harvester sprite with a remapped palette to see what color scheme houses use.
                {
                    Frame paletteFrame = Frame.createPalette();
                    Frame paletteRow = new Frame(paletteFrame, new Rectangle(0, 0, 16, 1));

                    //Create a remap table to house color range (0x90-0x98). Index: 0x00, 0x90, 0x91 ... 0x9D, 0x9E.
                    byte[] remappedRow = new byte[16];
                    Buffer.BlockCopy(paletteFrame.Pixels, 0x90, remappedRow, 1, 15); //Skip index 0 (always transparent).

                    //Write a remapped palette row (1x16) to sprite's frames.
                    for (int i = 10; i <= 14; i++)
                    {
                        //frames[i].clear(1);
                        frames[i].write(paletteRow);
                        remaps[i] = remappedRow;
                    }
                }

                //Create a Carryall sprite with a solid color to test aircraft shadow remapping.
                {
                    for (int i = 45; i <= 47; i++)
                    {
                        frames[i].clear(1);
                        remaps[i] = null;
                    }
                }

                //Create 6 unit sprites (Quad and Trike) with all palette rows to test aircraft shadow remapping.
                //16 rows to test (16*16=256). Shadow can cover max 3 rows so we need 6 sprites.
                {
                    Frame paletteFrame = Frame.createPalette();
                    Rectangle srcRect = new Rectangle(0, 0, 16, 1);
                    for (int i = 0; srcRect.Y < 16; i++) //Write 16 palette rows.
                    {
                        frames[i].clear(1);
                        remaps[i] = null;

                        //Write up to 3 palette rows per sprite frame.
                        Point dstPos = new Point(0, 4); //Start at shadow from a carryall in the tile above.
                        for (int r = 0; r < 3 && srcRect.Y < 16; r++, srcRect.Y++, dstPos.Y++)
                        {
                            frames[i].write(paletteFrame, srcRect, dstPos);
                        }
                    }
                }
            });

            ////Test if another palette will affect the aircraft shadow remap table.
            ////Is the ingame table calculated or constant? Result: Remap also changed i.e. it's calculated.
            //FilePalPalette6Bit filePal = duneEditor.getFileAs<FilePalPalette6Bit>("BENE.PAL");
            //duneEditor.replace("IBM.PAL", filePal.readAllBytes());

            duneEditor.save(PrintPath + "DUNE.PAK");

            return ini.save(PrintPath, "testUnitsRemap");
        }

        private static string testUnitsRemapRange()
        {
            //Test together with modified graphic-files to display full palette
            //so we can see which palette indices are remapped for units.
            //Uses a modified "DUNE.PAK".

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;

            //Add units which uses the 17 modified frames in a 5*4 block.
            tilePos.X += 2;
            string[] us = new string[] { "Quad", "Trike", "Harvester", "MCV" };
            for (int y = 0, i = 0; y < us.Length; y++)
            {
                //5 unique direction frames per unit. Keep going until 17 frames modified.
                for (int x = 0; x < 5 && i < 17; x++, i++)
                {
                    ini.addUnit(HouseGoodGuy, us[y], 256, toTileNum(tilePos, 2 * x, y * 2), 32 * x);
                }
            }

#if CHECK_ALL_UNIT_SHP_FRAMES
            //Check all unit frames for pixels in the excessive remap range (0x97-0x98).
            //Result: (nothing)
            //No unit frame seems to be affected by the excessive remap range.
            string[] shps = new string[] { "UNITS.SHP", "UNITS1.SHP", "UNITS2.SHP" };
            FilePakArchiveWw filePak = new FilePakArchiveWw(TemplatePath + "DUNE src.PAK");
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < shps.Length; s++)
            {
                FileShpSpriteSetD2 fileShp = filePak.getFileAs<FileShpSpriteSetD2>(shps[s]);
                for (int f = 0; f < fileShp.FrameCount; f++)
                {
                    byte[] pixels = fileShp.getFrameRemapped(f).Pixels;
                    for (int p = 0; p < pixels.Length; p++)
                    {
                        int v = pixels[p];
                        if (v >= 0x97 && v <= 0x98)
                        {
                            sb.AppendLine(string.Format("Frame '{0}' file '{1}' has pixels in excessive range!", f, fileShp.Name));
                            break;
                        }
                    }
                }
            }
            File.WriteAllText(Program.DebugOutPath + "Unit frames in excessive remap range.txt", sb.ToString());
#endif

            //Modify files used in this test.
            //DUNE.PAK: "UNITS.SHP"
            FilePakArchiveWw.Editor duneEditor = FilePakArchiveWw.Editor.open(TemplatePath + "DUNE src.PAK");
            writeModifiedShp(duneEditor, "UNITS.SHP", (Frame[] frames, byte[][] remaps) =>
            {
                //Modify all frames to display the remap row.
                Frame paletteFrame = Frame.createPalette();
                Frame paletteRow = new Frame(paletteFrame, new Rectangle(0, 0, 16, 1));

                //Only 15 colors per sprite (index 0 is always transparent) so we need
                //255/15=17 sprite frames.
                for (int i = 0; i < 17; i++)
                {
                    //Write a row of 16 colors to unit frame.
                    Frame frame = frames[i];
                    frame.clear(1);
                    frame.write(paletteRow);

                    //Change frame's remap table so it remaps to a palette row.
                    byte[] remap = remaps[i];
                    remap[0] = 0;
                    Buffer.BlockCopy(paletteFrame.Pixels, 1 + (i * 15), remap, 1, 15);
                }
            });
            duneEditor.save(PrintPath + "DUNE.PAK");

            return ini.save(PrintPath, "testUnitsRemapRange");
        }

        private static string testStructures()
        {
            //Structure entries can have two formats: one for concrete and one for buildings.
            //The game might behave strange if the wrong format is used for a type?
            //I tried all structures with both formats in the game and it seemed to work fine though?

            string[] ss = new string[]
            {
                "Concrete", "Concrete4", "Wall", "Turret", "R-Turret",
                "Const Yard", "Windtrap", "Barracks", "Light Fctry", "Heavy Fctry", "Hi-Tech",
                "Refinery", "Spice Silo", "Outpost", "Repair", "IX", "WOR", "Palace", "Starport"
            };

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            for (int i = 0; i < ss.Length; i++)
            {
                TilePos tilePos = StartTilePos.getOffset((i % 5) * 3, (i / 5) * 3); //3x3 tile blocks. 5 per row.
                ini.addBuilding(HouseGoodGuy, ss[i], 256, toTileNum(tilePos)); //Works for all types?
                //ini.addConcrete(HouseGoodGuy, ss[i], toTileNum(tilePos)); //Also works for all types?
            }

            return ini.save(PrintPath, "testStructures");
        }

        private static string testStructuresRemap()
        {
            //Test together with modified graphic-files to display remapped palette on all structures
            //so we can check if any isn't remapped. Are walls and concrete slabs remapped for example?
            //Uses a modified "DUNE.PAK".

            string[] ss = new string[]
            {
                "Concrete", "Concrete4", "Wall", "Turret", "R-Turret",
                "Const Yard", "Windtrap", "Barracks", "Light Fctry", "Heavy Fctry", "Hi-Tech",
                "Refinery", "Spice Silo", "Outpost", "Repair", "IX", "WOR", "Palace", "Starport"
            };

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            for (int i = 0; i < ss.Length; i++)
            {
                TilePos tilePos = StartTilePos.getOffset((i % 5) * 3, (i / 5) * 3); //3x3 tile blocks. 5 per row.
                ini.addBuilding(HouseGoodGuy, ss[i], 256, toTileNum(tilePos)); //Works for all types?
                //ini.addConcrete(HouseGoodGuy, ss[i], toTileNum(tilePos)); //Also works for all types?
            }

            //Modify files used in this test.
            //DUNE.PAK: "ICON.ICN"
            FilePakArchiveWw.Editor duneEditor = FilePakArchiveWw.Editor.open(TemplatePath + "DUNE src.PAK");
            writeModifiedIcn(duneEditor, "ICON.ICN", (Frame[] tiles, byte[][] remaps, byte[] remapIndices) =>
            {
                //Modify all tiles to display the remap row.
                Frame paletteFrame = Frame.createPalette();
                Frame paletteRow = new Frame(paletteFrame, new Rectangle(0, 0, 16, 1));

                //Create a remap table to house color range (0x90-0x9F). Index: 0x90, 0x91 ... 0x9E, 0x9F.
                byte[] remappedRow = new byte[16];
                Buffer.BlockCopy(paletteFrame.Pixels, 0x90, remappedRow, 0, 16);

                for (int t = 0; t < tiles.Length; t++)
                {
                    //Skip landscape tiles. Some will still look weird though because they
                    //share a remap table with a structure.
                    if (t >= 127 && t <= 207)
                    {
                        continue;
                    }

                    //Write a row of 16 colors to building tile.
                    Frame tile = tiles[t];
                    tile.clear(0);
                    tile.write(paletteRow);

                    //Change tile's remap table to remap row.
                    remaps[remapIndices[t]] = remappedRow;
                }
            });
            duneEditor.save(PrintPath + "DUNE.PAK");

            return ini.save(PrintPath, "testStructuresRemap");
        }

        private static string testStructuresRemapRange()
        {
            //Test together with modified graphic-files to display full palette with different houses
            //so we can see which palette indices are remapped for structures.
            //Uses a modified "DUNE.PAK".

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;
            string[] houses = new string[] { "Atreides", "Fremen", "Harkonnen", "Mercenary", "Ordos", "Sardaukar" };
            for (int h = 0; h < houses.Length; h++, tilePos.X += 2)
            {
                //One tile can only have max 16 colors so we need 16 tiles to display all 256 colors.
                //These 4 buildings are 2*2 tiles big and uses different remap tables for each tile.
                ini.addBuilding(houses[h], "Light Fctry", 256, toTileNum(tilePos, 0, 0));
                ini.addBuilding(houses[h], "WOR", 256, toTileNum(tilePos, 0, 2));
                ini.addBuilding(houses[h], "Outpost", 256, toTileNum(tilePos, 0, 4));
                ini.addBuilding(houses[h], "Spice Silo", 256, toTileNum(tilePos, 0, 6));
            }

            //Modify files used in this test.
            //DUNE.PAK: "ICON.ICN"
            FilePakArchiveWw.Editor duneEditor = FilePakArchiveWw.Editor.open(TemplatePath + "DUNE src.PAK");
            writeModifiedIcn(duneEditor, "ICON.ICN", (Frame[] tiles, byte[][] remaps, byte[] remapIndices) =>
            {
                //Modify tiles for the 4 buildings so they display all 256 colors.
                int[] tileIndices = new int[4 * 4] //Tile indices in the "ICON.ICN" file.
                {
                    241,242,248,249, //"Light Fctry"
                    291,292,294,295, //"WOR"
                    389,390,396,397, //"Outpost"
                    382,383,385,386, //"Spice Silo"
                };
                Frame paletteFrame = Frame.createPalette();
                Frame paletteRow = new Frame(paletteFrame, new Rectangle(0, 0, 16, 1));
                for (int i = 0; i < tileIndices.Length; i++)
                {
                    int tileIndex = tileIndices[i];

                    //Write a row of 16 colors to building tile.
                    Frame tile = tiles[tileIndex];
                    tile.clear(0);
                    tile.write(paletteRow);

                    //Change tile's remap table so it remaps to a palette row.
                    byte[] remap = remaps[remapIndices[tileIndex]];
                    Buffer.BlockCopy(paletteFrame.Pixels, i * 16, remap, 0, 16);
                }
            });
            duneEditor.save(PrintPath + "DUNE.PAK");

            return ini.save(PrintPath, "testStructuresRemapRange");
        }

        private static string testStructuresPriority()
        {
            //Test draw priority of structures. How are overlapping units and structures drawn?
            //How are overlapping structures drawn?

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;

            //Turret before silo? Turret not replaced! Only turret shown!
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 0, 0));

            //Wall before silo? Wall is replaced! Only silo shown!
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));

            //Concrete before silo? Concrete is replaced! Only silo shown!
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 0, 0));

            //Structure in all sections except top left before silo? All replaced! Only silo shown!
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 1, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 1, 1));

            //Structure in all sections except top left after silo? None added! Only silo shown!
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 1, 1));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 1, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));

            //Harvester unit before silo? Harvester shown over silo!
            tilePos.Y += 3;
            tilePos.X = StartTilePos.X;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addUnit(HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 0, 0), 0);

            //Harvester unit after silo? Harvester shown over silo!
            tilePos.X += 3;
            ini.addUnit(HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 0, 0), 0);
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));

            //Harvester units around silos? Harvesters shown over silos!
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addUnit(HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 0, -1), 0);
            ini.addUnit(HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, -1, 0), 64);

            //Silo overlapping silo (right added first)? Both silos added, overlapping sections flicker!
            tilePos.Y += 3;
            tilePos.X = StartTilePos.X;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 1, 0));

            //Silo overlapping silo (left added first)? Only left silo added.
            tilePos.X += 4;
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 1, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));

            return ini.save(PrintPath, "testStructuresPriority");
        }

        private static string testStructuresFormatPriority()
        {
            //Test add priority of structures and the two formats (GEN vs ID).
            //How are overlapping structures added/drawn?

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos.getOffset(2, 0);

            //Silo (ID) before wall (GEN)? Only silo shown! Surrounding walls indicate that the wall tile was added and present some time.
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, -1, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, -1));

            //Wall (GEN) before silo (ID)? Only silo shown! Surrounding walls indicate that the wall tile was replaced.
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 3 - 1, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 3, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 3, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 3, -1));

            //Silo (GEN) before wall (ID)? Only silo shown! Surrounding walls indicate that the wall tile was not added.
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 6 - 1, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 6, 0));
            ini.addConcrete(HouseGoodGuy, "Spice Silo", toTileNum(tilePos, 6, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 6, -1));

            //Wall (ID) before silo (GEN)? Only silo shown! Surrounding walls indicate that the wall tile was replaced.
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 9 - 1, 0));
            ini.addConcrete(HouseGoodGuy, "Spice Silo", toTileNum(tilePos, 9, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 9, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 9, -1));

            //Silo (ID) before turret (ID)? Only silo shown!
            tilePos.Y += 3;
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 0, 0));

            //Turret (ID) before silo (ID)? Only turret shown!
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 3, 0));
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 3, 0));

            //Silo (ID) before turret (GEN)? Both added?
            ini.addConcrete(HouseGoodGuy, "Turret", toTileNum(tilePos, 6, 0));
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 6, 0));

            //Turret (GEN) before silo (ID)? Only turret shown!
            ini.addBuilding(HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 9, 0));
            ini.addConcrete(HouseGoodGuy, "Turret", toTileNum(tilePos, 9, 0));

            //Slab (GEN) before wall (GEN)? Only slab shown!
            tilePos.Y += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Concrete4", toTileNum(tilePos, 0, 0));

            //Wall (GEN) before slab (GEN)? Only wall shown!
            ini.addConcrete(HouseGoodGuy, "Concrete4", toTileNum(tilePos, 3, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 3, 0));

            return ini.save(PrintPath, "testStructuresFormatPriority");
        }

        private static string testStructuresIxError()
        {
            //Test how the "IX" structure remap error/bug with different houses looks like in the game.

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;
            string[] houses = new string[] { "Atreides", "Fremen", "Harkonnen", "Mercenary", "Ordos", "Sardaukar" };
            for (int h = 0; h < houses.Length; h++)
            {
                ini.addBuilding(houses[h], "IX", 256, toTileNum(tilePos, h * 2, 0));
            }

#if CHECK_ALL_ICN_TILES
            //Check all tiles for pixels in the excessive remap range (0x97-0xA0).
            //Result:
            //Tile '286' remap '105' has pixels in excessive range!
            //Tile '287' remap '106' has pixels in excessive range!
            //Tile '288' remap '106' has pixels in excessive range!
            //Tile '289' remap '107' has pixels in excessive range!
            //Tile '290' remap '107' has pixels in excessive range!
            //All are tiles for the "IX" structure.
            FilePakArchiveWw filePak = new FilePakArchiveWw(TemplatePath + "DUNE src.PAK");
            FileIcnTilesD2 fileIcn = filePak.getFileAs<FileIcnTilesD2>("ICON.ICN");
            Frame[] tiles = fileIcn.copyTiles();
            byte[][] remaps = fileIcn.copyRemaps();
            byte[] remapIndices = fileIcn.copyRemapIndices();
            StringBuilder sb = new StringBuilder();
            for (int t = 0; t < tiles.Length; t++)
            {
                byte[] pixels = tiles[t].Pixels;
                byte[] remap = remaps[remapIndices[t]];
                for (int p = 0; p < pixels.Length; p++)
                {
                    int v = remap[pixels[p]];
                    if (v >= 0x97 && v <= 0xA0)
                    {
                        sb.AppendLine(string.Format("Tile '{0}' remap '{1}' has pixels in excessive range!", t, remapIndices[t]));
                        break;
                    }
                }
            }
            File.WriteAllText(Program.DebugOutPath + "Tiles in excessive remap range.txt", sb.ToString());

            //Also check all remaps for pixels in the excessive remap range. Potentially a problem.
            //Result:
            //Remap '105' has pixels in excessive range! Used by tile: 286,
            //Remap '106' has pixels in excessive range! Used by tile: 287,288,
            //Remap '107' has pixels in excessive range! Used by tile: 289,290,
            //Again, all are tiles for the "IX" structure.
            sb = new StringBuilder();
            for (int r = 0; r < remaps.Length; r++)
            {
                byte[] remap = remaps[r];
                for (int i = 0; i < remap.Length; i++)
                {
                    int v = remap[i];
                    if (v >= 0x97 && v <= 0xA0)
                    {
                        sb.Append(string.Format("Remap '{0}' has pixels in excessive range! Used by tile: ", r));
                        for (int t = 0; t < remapIndices.Length; t++)
                        {
                            if (remapIndices[t] == r)
                            {
                                sb.Append(t.ToString() + ",");
                            }
                        }
                        sb.AppendLine();
                        break;
                    }
                }
            }
            File.WriteAllText(Program.DebugOutPath + "Remaps in excessive remap range.txt", sb.ToString());
#endif

            return ini.save(PrintPath, "testStructuresIxError");
        }

        private static string testWalls()
        {
            //Test how walls with different adjacent walls look.
            //Need to update walls when added, but not when replaced, and mimic how duplicate
            //INI keys are handled to match the game in this test.

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;
            for (int i = 0; i < 2; i++) //Do two 3*3 blocks of walls.
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        if ((i * x * y) != 1) //Don't do center in second block.
                        {
                            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, x + (i * 4), y));
                        }
                    }
                }
            }

            //Does walls join if owned differently? Yes!
            tilePos.Y += 4;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseBadGuy, "Wall", toTileNum(tilePos, 1, 0));

            //Does walls join if created differently? Yes!
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 0));

            //Does walls overlap? One wall tile visible.
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));

            //Does walls overlap if created differently? One wall tile visible.
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));

            //How does adjacent walls update if replaced by other structure?
            tilePos.Y += 2;
            tilePos.X = StartTilePos.X;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 1));
            ini.addConcrete(HouseGoodGuy, "Concrete", toTileNum(tilePos, 0, 0)); //Replace wall with a slab? Wall was repeated (same key as concrete)!
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 0));
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 1));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 1));
            ini.addConcrete(HouseGoodGuy, "Concrete", toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was repeated (same key as concrete)!
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 0));

            //Repeat, but with replace using the other add format.
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 0));
            tilePos.X += 3;
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 1));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 0));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 1, 0));

            //Try again, but invert add format used.
            tilePos.Y += 3;
            tilePos.X = StartTilePos.X;
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 0));
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 1));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 0));

            //Repeat, but with replace using the other add format.
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addConcrete(HouseGoodGuy, "Concrete", toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 0));
            tilePos.X += 3;
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 1));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 1));
            ini.addConcrete(HouseGoodGuy, "Concrete", toTileNum(tilePos, 0, 0)); //Replace wall with a slab. Wall was replaced!
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 1, 0));

            return ini.save(PrintPath, "testWalls");
        }

        private static string testIniKeys()
        {
            //Test how duplicate INI-keys are added. Use different rotations (or id:s) to easier see
            //which line is added by the game.
            //Must use the IniKeyFinderD2 class when adding units and structures to match the game.
            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;

            //Add 5 units. The middle 3 have the same INI-key.
            int unitCount = ini.getSectionUnit().Keys.Count;
            ini.addUnit(unitCount + 0, HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 0, 0), 0);
            ini.addUnit(unitCount + 1, HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 2, 0), 32); //Not added.
            ini.addUnit(unitCount + 1, HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 4, 0), 64); //Not added.
            ini.addUnit(unitCount + 1, HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 6, 0), 96); //Added. First in file.
            ini.addUnit(unitCount + 2, HouseGoodGuy, "Harvester", 256, toTileNum(tilePos, 8, 0), 128);

            //Add 5 buildings. The middle 3 have the same INI-key.
            tilePos.Y += 2;
            int buildCount = ini.getSectionStructure().Keys.Count;
            ini.addBuilding(buildCount + 0, HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 0, 0));
            ini.addBuilding(buildCount + 1, HouseGoodGuy, "Spice Silo", 256, toTileNum(tilePos, 2, 0)); //Not added.
            ini.addBuilding(buildCount + 1, HouseGoodGuy, "Barracks", 256, toTileNum(tilePos, 4, 0)); //Not added.
            ini.addBuilding(buildCount + 1, HouseGoodGuy, "Light Fctry", 256, toTileNum(tilePos, 6, 0)); //Added. First in file.
            ini.addBuilding(buildCount + 2, HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 8, 0));

            //Add 5 buildings using the other format. The middle 3 have the same INI-key (tile position).
            //Adding these lines will prevent the western windtrap in the previous test from being added for some reason.
            //I guess using this format for adding buildings will cause issues sometimes?
            //tilePos.Y += 3;
            //ini.addConcrete(HouseGoodGuy, "Windtrap", toTileNum(tilePos, 0, 0));
            //ini.addConcrete(HouseGoodGuy, "Spice Silo", toTileNum(tilePos, 2, 0)); //Not added.
            //ini.addConcrete(HouseGoodGuy, "Barracks", toTileNum(tilePos, 2, 0)); //Not added.
            //ini.addConcrete(HouseGoodGuy, "Light Fctry", toTileNum(tilePos, 2, 0)); //Added. First in file.
            //ini.addConcrete(HouseGoodGuy, "Windtrap", toTileNum(tilePos, 8, 0));

            return ini.save(PrintPath, "testIniKeys");
        }

        private static string testSpiceFields()
        {
            //Test how spice fields are added and how other ground objects affect them.
            //Also test priority of bloom and special tiles.
            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);

            ini.updateKey("MAP", "Seed", "33"); //Seed with a pretty open sand area in top left corner.

            //Add a spice field.
            ini.updateKey("MAP", "Field", toTileNum(StartTilePos, 3, 3).ToString());

            //Add bloom tiles and a few structures in the field.
            TilePos tilePos = StartTilePos;
            ini.updateKey("MAP", "Bloom", string.Format("{0},{1},{2},{3}",
                toTileNum(tilePos, 0, 0), toTileNum(tilePos, 2, 0), toTileNum(tilePos, 6, 0), toTileNum(tilePos, 8, 0)));
            ini.addBuilding(HouseGoodGuy, "Wall", 256, toTileNum(tilePos, 0, 2));
            ini.addBuilding(HouseGoodGuy, "Turret", 256, toTileNum(tilePos, 4, 2));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 2, 0));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 5, 0));

            //Add special tiles and a few structures in the field.
            //Check if alternate structure add format makes a difference.
            tilePos.Y += 4;
            ini.updateKey("MAP", "Special", string.Format("{0},{1},{2},{3}",
                toTileNum(tilePos, 0, 0), toTileNum(tilePos, 2, 0), toTileNum(tilePos, 6, 0), toTileNum(tilePos, 8, -4)));
            ini.addConcrete(HouseGoodGuy, "Wall", toTileNum(tilePos, 0, 2));
            ini.addConcrete(HouseGoodGuy, "Turret", toTileNum(tilePos, 4, 2));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 2, 0));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(tilePos, 5, 0));

            return ini.save(PrintPath, "testSpiceFields");
        }

        private static string testRadarTiles()
        {
            //Test what radar colors ground/terrain tiles have.
            //Testing indicate that the game does something rather complicated to determine radar colors.
            //In the game the radar seems to be affected by tile index values in the terrain tile set.
            //It's not just one color index per tile in the ICN-file though. The first value in the set
            //seems to affect the color of the other tiles in it? It's really weird.
            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);
            TilePos tilePos = StartTilePos;

            //Seed=9460 generates a map with all 81 terrain tiles.
            ini.updateKey("MAP", "Seed", "9460");

            //Modify files used in this test.
            //DUNE.PAK: "ICON.MAP"
            FilePakArchiveWw.Editor duneEditor = FilePakArchiveWw.Editor.open(TemplatePath + "DUNE src.PAK");
            FileMapTileSetsD2 fileMap = duneEditor.getFileAs<FileMapTileSetsD2>("ICON.MAP");
            UInt16[][] tileSets = fileMap.copyTileSets();

            //Modify "ICON.MAP" so it will have different tiles in its terrain tile set.
            //The terrain tile set has 81 tiles so we need to run this test with different
            //tile ranges to display all 399 of them.

            ////Do the ranges in order.
            ////int tileStart = tileSets[9].Length * 0; //dune2_161.png
            //int tileStart = tileSets[9].Length * 1; //dune2_164.png
            ////int tileStart = tileSets[9].Length * 2; //dune2_166.png
            ////int tileStart = tileSets[9].Length * 3; //dune2_168.png
            //for (int i = 0, t = tileStart; i < tileSets[9].Length; i++, t++)
            //{
            //    tileSets[9][i] = (UInt16)t;
            //}

            ////Do the ranges in reverse.
            ////int tileStart = (tileSets[9].Length - 1) * 0; //dune2_172.png
            //int tileStart = (tileSets[9].Length - 1) * 1; //dune2_173.png <---[1]
            ////int tileStart = (tileSets[9].Length - 1) * 2; //dune2_174.png
            ////int tileStart = (tileSets[9].Length - 1) * 3; //dune2_175.png
            ////int tileStart = (tileSets[9].Length - 1) * 4; //dune2_176.png
            //for (int i = 1, t = tileStart; i < tileSets[9].Length; i++, t++)
            //{
            //    tileSets[9][i] = (UInt16)t;
            //}

            //Modify first value in terrain set. Seems to affect color of all other tiles in it?
            //tileSets[9][0] = 34; //--->[1] dune2_178.png

            string path = PrintModifiedPath + fileMap.Name;
            FileMapTileSetsD2.writeMap(path, tileSets);
            duneEditor.replace(fileMap.Name, path);
            duneEditor.save(PrintPath + "DUNE.PAK");

            return ini.save(PrintPath, "testRadarTiles");
        }

        private static string testRadarUnits()
        {
            //Test how units look on the radar. Some units are a bit weird.            
            //Deviator units always switch their attacked target to Ordos?
            //Aircrafts aren't displayed in the radar.
            //Sandworms use index 255 (cycles between grey and white) in the radar regardless of owner.

            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);

            string[] us = new string[]
            {
                /*"Carryall",*/ "'Thopter", /*"Frigate",*/
                "Soldier", "Infantry", "Trooper", "Troopers", "Saboteur",
                "Tank", "Siege Tank", "Devastator", "Sonic Tank", "Launcher", /*"Deviator",*/
                "Trike", "Raider Trike", "Quad",
                "Harvester", "MCV", "Sandworm"
            };

            //Function to encircle a unit with a wall to stop it from moving.
            Action<string, TilePos, bool> encircleWall = (string house, TilePos tilePos, bool doLeftSide) =>
            {
                //Encircle a 3*3 wall around tile pos.
                int startX = doLeftSide ? -1 : 0; //Skip left column?
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = startX; x <= 1; x++)
                    {
                        if (!(y == 0 && x == 0)) //Skip center?
                        {
                            ini.addConcrete(house, "Wall", toTileNum(tilePos, x, y));
                        }
                    }
                }
            };

            string[] hs = new string[] { HouseGoodGuy, HouseBadGuy };
            TilePos blockPos = StartTilePos;
            for (int h = 0; h < hs.Length; h++)
            {
                string house = hs[h];

                //Units.
                for (int i = 0; i < us.Length; i++)
                {
                    encircleWall(house, blockPos, i == 0);
                    ini.addUnit(house, us[i], 256, toTileNum(blockPos, 0, 0), 0);
                    blockPos.X += 2;
                }

                //Special for Deviator. Move it far away to avoid it switching units to house Ordos.
                blockPos.X += 11;
                encircleWall(house, blockPos, true);
                ini.addUnit(house, "Deviator", 256, toTileNum(blockPos, 0, 0), 0);

                //Start new row for next house.
                blockPos.X = StartTilePos.X;
                blockPos.Y += 11;
            }

            return ini.save(PrintPath, "testRadarUnits");
        }

        private static string testRadarStructures()
        {
            //Test how structures look on the radar.
            TestsIniAdderD2 ini = new TestsIniAdderD2(TemplateIniPath);

            //Add some windtraps to power outpost radar and all other buildings.
            TilePos windParkPos = new TilePos(60, 50);
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(windParkPos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(windParkPos, 0, 2));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(windParkPos, 0, 4));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(windParkPos, 0, 6));
            ini.addBuilding(HouseGoodGuy, "Windtrap", 256, toTileNum(windParkPos, 0, 8));

            //Adding palaces in this test seems to make the game crash so let's skip it.
            string[] normalStructs = new string[]
            {
                "Concrete", "Concrete4", "Wall", "Turret", "R-Turret",
                "Const Yard", "Windtrap", "Barracks", "Light Fctry", "Heavy Fctry", "Hi-Tech",
                "Refinery", "Spice Silo", "Outpost", "Repair", "IX", "WOR", /*"Palace",*/ "Starport"
            };

            //Test what happens if a unit overlap some structures. Seems like unit is
            //drawn over slab or wall, but under building in the radar.
            string[] overlapStructs = new string[] { "Concrete", "Concrete4", "Wall", "Windtrap" };

            string[][] groups = new string[][] { normalStructs, overlapStructs };

            string[] houses = new string[] { HouseGoodGuy, HouseBadGuy };
            TilePos blockPos = StartTilePos;
            for (int g = 0; g < groups.Length; g++)
            {
                for (int h = 0; h < houses.Length; h++)
                {
                    string house = houses[h];
                    string[] structs = groups[g];

                    //Normal structures group.
                    for (int i = 0; i < structs.Length; i++)
                    {
                        string structId = structs[i];
                        if (structId == "Concrete" || structId == "Concrete4" || structId == "Wall")
                        {
                            ini.addConcrete(house, structId, toTileNum(blockPos, 0, 0));
                        }
                        else
                        {
                            ini.addBuilding(house, structId, 256, toTileNum(blockPos, 0, 0));
                        }

                        //Unit overlap structures group.
                        if (structs == overlapStructs)
                        {
                            //Add a unit from the other house over this structure.
                            string houseId = house == HouseGoodGuy ? HouseBadGuy : HouseGoodGuy;
                            ini.addUnit(houseId, "Harvester", 256, toTileNum(blockPos, 0, 0), 0);
                        }

                        blockPos.X += SpriteStructureD2.getTileSet(structs[i]).TemplateSize.Width + 1;
                    }

                    //Start new row for next house.
                    blockPos.X = StartTilePos.X;
                    blockPos.Y += 11;
                }
            }

            //Test if using add format (concrete vs building) affects radar. Seems like it doesn't.
            ini.addConcrete(HouseGoodGuy, "Concrete", toTileNum(blockPos, 0, 0));
            ini.addBuilding(HouseGoodGuy, "Concrete", 256, toTileNum(blockPos, 2, 0));
            ini.addConcrete(HouseGoodGuy, "Barracks", toTileNum(blockPos, 4, 0));
            ini.addBuilding(HouseGoodGuy, "Barracks", 256, toTileNum(blockPos, 6, 0));

            return ini.save(PrintPath, "testRadarStructures");
        }

        private static void writeModifiedShp(FilePakArchiveWw.Editor editor, string shpName, Action<Frame[], byte[][]> action)
        {
            FileShpSpriteSetD2 fileShp = new FileShpSpriteSetD2(editor.getFile(shpName));
            Frame[] frames = fileShp.copyFrames();
            byte[][] remaps = fileShp.copyRemaps();
            action(frames, remaps);
            string path = PrintModifiedPath + fileShp.Name;
            FileShpSpriteSetD2.writeShp(path, frames, remaps, true);
            editor.replace(fileShp.Name, path);
        }

        private static void writeModifiedIcn(FilePakArchiveWw.Editor editor, string icnName, Action<Frame[], byte[][], byte[]> action)
        {
            FileIcnTilesD2 fileIcn = new FileIcnTilesD2(editor.getFile(icnName));
            Frame[] tiles = fileIcn.copyTiles();
            byte[][] remaps = fileIcn.copyRemaps();
            byte[] remapIndices = fileIcn.copyRemapIndices();
            action(tiles, remaps, remapIndices);
            string path = PrintModifiedPath + fileIcn.Name;
            FileIcnTilesD2.writeIcn(path, tiles, remaps, remapIndices);
            editor.replace(fileIcn.Name, path);
        }

        private static int toTileNum(TilePos tilePos)
        {
            return MapD2.toTileNum(tilePos);
        }

        private static int toTileNum(TilePos tilePos, int dx, int dy)
        {
            return MapD2.toTileNum(tilePos.getOffset(dx, dy));
        }

        private static void saveToScenarioPak(List<string> iniPaths)
        {
            if (iniPaths.Count == 0)
            {
                throw new ArgumentException();
            }

            FilePakArchiveWw.Editor editor = FilePakArchiveWw.Editor.open(TemplatePath + "SCENARIO src.PAK");
            int countGoodGuy = 1;
            int countBadGuy = 1;
            foreach (string iniPath in iniPaths)
            {
                //Calculate name to replace from path.
                string nameToReplace = Path.GetFileName(iniPath).Split(' ')[0].ToUpperInvariant() + ".INI";
                if (nameToReplace == "SCENA001.INI") //Use default goodguy files?
                {
                    nameToReplace = "SCENA" + countGoodGuy.ToString("D3") + ".INI";
                    countGoodGuy++;
                }
                else if (nameToReplace == "SCENH001.INI") //Use default badguy files?
                {
                    nameToReplace = "SCENH" + countBadGuy.ToString("D3") + ".INI";
                    countBadGuy++;
                }
                editor.replace(nameToReplace, iniPath);
            }
            editor.save(PrintPath + "SCENARIO.PAK");
        }
    }
}
