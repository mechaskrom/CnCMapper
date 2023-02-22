using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Map's background layer which everything else is drawn upon.
    //Made of tiles also referred to as "icons" or "templates".
    //Tiles are stored in ICN-files. Arrangement table in BIN-files.
    //It seems Tiberian Dawn at some point used a [TEMPLATE] section in INI-files to arrange the background,
    //but it was abandoned for BIN-files instead.

    class GroundLayerTD : GroundLayerTDRA
    {
        public const int TileSetTableLength = MapTD.WidthInTiles * MapTD.HeightInTiles * 2;

        private readonly byte[] mTileSetTable;

        private GroundLayerTD(byte[] tileSetTable)
        {
            if (tileSetTable.Length != TileSetTableLength)
            {
                throw new ArgumentException(string.Format("Tile set table length '{0}' should be '{1}'!",
                    tileSetTable.Length, TileSetTableLength));
            }
            mTileSetTable = tileSetTable;
        }

        public static GroundLayerTD create(FileBinTileSetTableTD fileBin, FileIni fileIni, TheaterTDRA theater)
        {
            byte[] tileSetTable;
            if (fileBin != null) //Map has a corresponding BIN-file?
            {
                tileSetTable = fileBin.TileSetTable;
            }
            else //Create a clear background and use the [TEMPLATE] section instead.
            {
                Program.warn(string.Format(
                    "Map '{0}' will use a clear background and the [TEMPLATE] section because '{1}' couldn't be found in '{2}'.",
                    fileIni.Id, fileIni.Id + ".BIN", fileIni.FullPath));
                tileSetTable = parseTileSetTable(fileIni, theater);
            }
            return new GroundLayerTD(tileSetTable);
        }

        private static byte[] parseTileSetTable(FileIni fileIni, TheaterTDRA theater)
        {
            //Creates a tile set table from the INI-file's [TEMPLATE] section.
            //[TEMPLATE] section is only read and used if BIN-file is missing. Checked in game and source.
            //Probably an old legacy thing used before BIN-files were introduced instead?

            //[TEMPLATE] is present, but empty, in these files: SCG01EA.INI, SCG02EA.INI, SCG04EA.INI, SCG04WB.INI.
            //[TEMPLATE] is present, and used(*1), in these files: SCG05EA.INI, SCG06EA.INI, SCG08EA.INI, SCG09EA.INI.
            //*1=Nothing interesting though, just a shoreline with some water at the bottom.
            //All early missions (assuming GDI campaign was made first) which also indicates this
            //was an old system that was abandoned.

            //Start with an all clear tile set table. That is also the result if the [TEMPLATE] section
            //is missing. Checked in game.
            byte[] tileSetTable = createTileSetTableClear();

            //Format: tileNumber=id
            IniSection iniSection = fileIni.findSection("TEMPLATE");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    TileSetTD.IdPair tsIdPair = TileSetTD.get(key.Value);
                    if (tsIdPair != null && tsIdPair.Id != 0) //Tile set text id found and not default clear?
                    {
                        int tileNum = key.idAsInt32();
                        byte tileSetId = tsIdPair.Id;
                        byte tileSetIndex = 0;
                        Size templateSize = tsIdPair.TileSet.TemplateSize;
                        FileIcnTileSetTDRA fileIcn = theater.getTileSet(tileSetId);

                        //It seems like no more templates are added/drawn afterwards a missing ICN-file is
                        //encountered? Checked in game. Not sure if this is something I want to mimic though?
                        if (fileIcn.IsOriginMissing) //ICN-file wasn't found in theater?
                        {
                            break; //Stop parsing [TEMPLATE] section.
                        }

                        //Write the tile set template to the table.
                        for (int tileY = 0; tileY < templateSize.Height; tileY++, tileNum += MapTD.WidthInTiles)
                        {
                            int tableInd = tileNum * 2; //2 bytes per tile set table entry.
                            for (int tileX = 0; tileX < templateSize.Width; tileX++, tableInd += 2, tileSetIndex++)
                            {
                                //An empty tile index isn't added/drawn i.e. it won't replace an existing tile. Checked in game.
                                if (fileIcn.isEmptyTileIndex(tileSetIndex)) //Empty tile index?
                                {
                                    continue;
                                }

                                //Tile outside right edge continues on next row i.e. tile is added if tile number
                                //is in range (0-4095). Checked in game.
                                if (tableInd >= 0 && tableInd <= (TileSetTableLength - 2))
                                {
                                    tileSetTable[tableInd + 0] = tileSetId;
                                    tileSetTable[tableInd + 1] = tileSetIndex;
                                }
                            }
                        }
                    }
                }
            }
            return tileSetTable;
        }

        public static byte[] createTileSetTableClear()
        {
            //Creates an all clear tile set table.
            return createTileSetTableClear(0xFF, 0x00); //Default clear tile template.
        }

        public static byte[] createTileSetTableClear(byte tileSetId, byte tileSetIndex)
        {
            //Creates a tile set table with all tiles set to specified values.
            byte[] tileSetTable = new byte[TileSetTableLength];
            for (int i = 0; i < tileSetTable.Length; i += 2)
            {
                tileSetTable[i + 0] = tileSetId; //Tile set id.
                tileSetTable[i + 1] = tileSetIndex; //Tile set index.
            }
            return tileSetTable;
        }

        private static FileIcnTileSetTDRA getTileSet(byte[] tileSetTable, int tileX, int tileY, TheaterTDRA theater, out byte tileSetId, out byte tileSetIndex)
        {
            //Return out tile set id as id of returned ICN-file.

            //Map BIN-file data is 8KB (64*64*2) of tile set table entries (UInt16).
            //A tile set table entry consists of tile set id (UInt8) and tile set index (UInt8).

            int tableInd = (tileX + (MapTD.WidthInTiles * tileY)) * 2;
            tileSetId = tileSetTable[tableInd + 0];
            if (tileSetId == 0xFF) //Default clear tile template?
            {
                tileSetId = 0;
                tileSetIndex = toTileSetIndexClear(tileX, tileY);
                return theater.getTileSet(0);
            }
            else //Use id and index in tile set table.
            {
                tileSetIndex = tileSetTable[tableInd + 1];
                FileIcnTileSetTDRA fileIcn = theater.getTileSet(tileSetId);
                if (fileIcn.isEmptyTileIndex(tileSetIndex)) //Empty tile index?
                {
                    //Empty tile index treated same as 0xFF (clear tile template) in Tiberian Dawn. Checked in game.
                    tileSetId = 0;
                    tileSetIndex = toTileSetIndexClear(tileX, tileY);
                    return theater.getTileSet(0);
                }
                return fileIcn;
            }
        }

        private static Frame getTileFrame(FileIcnTileSetTDRA fileIcn, byte tileSetIndex, TheaterTDRA theater)
        {
            return getTileFrame(fileIcn, tileSetIndex, theater, GameTD.Config.DrawTileSetEmptyEffect);
        }

        public void draw(TheaterTDRA theater, IndexedImage image)
        {
            draw(mTileSetTable, theater, image);
        }

        private static void draw(byte[] tileSetTable, TheaterTDRA theater, IndexedImage image)
        {
            //Only draw tiles inside clip area.
            Rectangle clipInTiles = toInTiles(image.Clip);
            Point dstPos = new Point(0, clipInTiles.Y * MapTD.TileHeight);
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstPos.Y += MapTD.TileHeight)
            {
                dstPos.X = clipInTiles.X * MapTD.TileWidth; //Restore X position i.e. start a new row.
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstPos.X += MapTD.TileWidth)
                {
                    byte tileSetId;
                    byte tileSetIndex;
                    FileIcnTileSetTDRA fileIcn = getTileSet(tileSetTable, tileX, tileY, theater, out tileSetId, out tileSetIndex);
                    Frame tileFrame = getTileFrame(fileIcn, tileSetIndex, theater);
                    image.write(tileFrame, dstPos);
                }
            }
        }

        public void drawRadar(int scale, MapTD map, List<SpriteTDRA> sprites, IndexedImage image)
        {
            drawRadar(scale, mTileSetTable, map.Theater, image);

            //Tile layer drawing has a special case for scale 1 where the tile's land type is used to
            //determine the color of the pixels. The land type is affected by any overlay at the tile.
            //See "Recalc_Attributes()" in "CELL.CPP".
            if (scale == 1) //Land type drawing?
            {
                foreach (SpriteTDRA sprite in sprites) //Overlays affecting land type?
                {
                    SpriteOverlayTD spr = sprite as SpriteOverlayTD;
                    if (spr != null)
                    {
                        image[spr.TilePos.Location] = spr.getLandTypeRadarIndex();
                    }
                }
            }
        }

        private static void drawRadar(int scale, byte[] tileSetTable, TheaterTDRA theater, IndexedImage image)
        {
            //Only draw tiles inside clip area.
            Rectangle clipInTiles = toInTiles(image.Clip, scale);
            Point dstPos = new Point(0, clipInTiles.Y * scale);
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstPos.Y += scale)
            {
                dstPos.X = clipInTiles.X * scale;
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstPos.X += scale)
                {
                    byte tileSetId;
                    byte tileSetIndex;
                    FileIcnTileSetTDRA fileIcn = getTileSet(tileSetTable, tileX, tileY, theater, out tileSetId, out tileSetIndex);
                    if (scale == 1)
                    {
                        TileSetTD tileSet = TileSetTD.get(tileSetId);
                        image[dstPos] = toRadarColorIndex(tileSet.getLandType(tileSetIndex));
                    }
                    else
                    {
                        Frame tileFrame = getTileFrame(fileIcn, tileSetIndex, theater);
                        RadarTD.drawScaled(tileFrame, image, dstPos, null, scale);
                    }

                    //The ingame radar seems to crash on missing tiles.
                }
            }
        }

        private static byte toRadarColorIndex(LandType landType)
        {
            //Values from source, see "CONST.CPP".
            switch (landType)
            {
                case LandType.Clear: return 66;
                case LandType.Road: return 68;
                case LandType.Water: return 11; //BLUE
                case LandType.Rock: return 13; //DKGREY
                case LandType.Wall: return 13; //DKGREY
                case LandType.Tiberium: return 143;
                case LandType.Beach: return 66;
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}