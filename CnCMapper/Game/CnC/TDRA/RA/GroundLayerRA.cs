using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Map's background layer which everything else is drawn upon.
    //Made of tiles also referred to as "icons" or "templates".
    //Tiles are stored in ICN-files. Arrangement table in INI-files ([MapPack] section).

    class GroundLayerRA : GroundLayerTDRA
    {
        public const int TileSetTableLength = MapRA.WidthInTiles * MapRA.HeightInTiles * 3;
        private const int SecondSectionOffset = MapRA.WidthInTiles * MapRA.HeightInTiles * 2;

        private readonly byte[] mTileSetTable;

        private GroundLayerRA(byte[] tileSetTable)
        {
            if (tileSetTable.Length != TileSetTableLength)
            {
                throw new ArgumentException(string.Format("Tile set table length '{0}' should be '{1}'!",
                    tileSetTable.Length, TileSetTableLength));
            }
            mTileSetTable = tileSetTable;
        }

        public static GroundLayerRA create(FileIni fileIni)
        {
            return new GroundLayerRA(parseTileSetTable(fileIni));
        }

        private static byte[] parseTileSetTable(FileIni fileIni)
        {
            IniSection iniSection = fileIni.findSection("MapPack");
            if (iniSection != null && iniSection.Keys.Count > 0) //[MapPack] section present and not empty?
            {
                return Crypt.Pack.decode(iniSection.getPackDataAsBase64());
            }
            //Seems like the game will crash if the [MapPack] section is missing or empty. Checked in game.
            Program.warn(string.Format(
                "Map '{0}' will use a clear background because the [MapPack] section is missing or empty in '{1}'.",
                fileIni.Id, fileIni.FullName));
            return createTileSetTableClear();
        }

        public static byte[] createTileSetTableClear()
        {
            //Creates an all clear tile set table.
            return createTileSetTableClear(0xFFFF, 0x00); //Default clear tile template.
        }

        public static byte[] createTileSetTableClear(UInt16 tileSetId, byte tileSetIndex)
        {
            //Creates a tile set table with all tiles set to specified values.
            byte tileSetIdLo = (byte)((tileSetId >> 0) & 0xFF);
            byte tileSetIdHi = (byte)((tileSetId >> 8) & 0xFF);
            byte[] tileSetTable = new byte[TileSetTableLength]; //Section 1 + 2.
            for (int i = 0; i < SecondSectionOffset; i += 2) //Section 1.
            {
                tileSetTable[i + 0] = tileSetIdLo; //Tile set id.
                tileSetTable[i + 1] = tileSetIdHi;
            }
            for (int i = SecondSectionOffset; i < tileSetTable.Length; i++) //Section 2.
            {
                tileSetTable[i] = tileSetIndex; //Tile set index.
            }
            return tileSetTable;
        }

        private static FileIcnTileSetTDRA getTileSet(byte[] tileSetTable, int tileX, int tileY, TheaterTDRA theater, out UInt16 tileSetId, out byte tileSetIndex)
        {
            //Return out tile set id as stored in table and not id of returned ICN-file. Some functions need to check the stored value.

            //Decoded map pack data is 48KB and consists of two sections:
            //-First section is 32KB (128*128*2) with tile set id data (UInt16).
            //-Second section is 16KB (128*128*1) with tile set index data (UInt8).

            int tableInd = tileX + (MapRA.WidthInTiles * tileY);
            tileSetId = toTileSetId(tileSetTable, tableInd * 2); //First section in tile set table (2 byte per entry).
            if (tileSetId == 0xFFFF) //Default clear tile template?
            {
                tileSetIndex = toTileSetIndexClear(tileX, tileY);
                return theater.getTileSet(0);
            }
            else //Use id and index in tile set table.
            {
                tileSetIndex = tileSetTable[tableInd + SecondSectionOffset]; //Second section in tile set table.
                return theater.getTileSet(tileSetId);
            }

            //Red Alert has a very messy way of deciding if tile set id:s 0x0000 and 0x00FF should be treated
            //as a clear tile template (same as 0xFFFF). I'm not sure if I got this right, but seems like
            //they are treated same as 0xFFFF in these cases:
            //Map renderer: 0x0000, 0x00FF.
            //Radar renderer: 0x00FF, but only if scale >= 2.

            //Clear tile template means:
            //-tile set id is 0 i.e. the "CLEAR1" ICN-file is used.
            //-tile set index is calculated from tile X&Y instead of using the stored value in the tile set table.

            //If not a clear tile template then the stored id and index in the tile set table is used.
        }

        private static UInt16 toTileSetId(byte[] tileSetTable, int tableIndex)
        {
            return (UInt16)(tileSetTable[tableIndex + 0] | (tileSetTable[tableIndex + 1] << 8));
        }

        private static Frame getTileFrame(FileIcnTileSetTDRA fileIcn, byte tileSetIndex, TheaterTDRA theater)
        {
            return getTileFrame(fileIcn, tileSetIndex, theater, GameRA.Config.DrawTileSetEmptyEffect);
        }

        public void draw(TheaterTDRA theater, IndexedImage image)
        {
            draw(mTileSetTable, theater, image);
        }

        private static void draw(byte[] tileSetTable, TheaterTDRA theater, IndexedImage image)
        {
            //Only draw tiles inside clip area.
            Rectangle clipInTiles = toInTiles(image.Clip);
            Point dstPos = new Point(0, clipInTiles.Y * MapRA.TileHeight);
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstPos.Y += MapRA.TileHeight)
            {
                dstPos.X = clipInTiles.X * MapRA.TileWidth; //Restore X position i.e. start a new row.
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstPos.X += MapRA.TileWidth)
                {
                    UInt16 tileSetId;
                    byte tileSetIndex;
                    FileIcnTileSetTDRA fileIcn = getTileSet(tileSetTable, tileX, tileY, theater, out tileSetId, out tileSetIndex);
                    if (tileSetId == 0x0000 || tileSetId == 0x00FF) //Treat as a clear tile template?
                    {
                        tileSetIndex = toTileSetIndexClear(tileX, tileY);
                        fileIcn = theater.getTileSet(0);
                    }
                    Frame tileFrame = getTileFrame(fileIcn, tileSetIndex, theater);
                    image.write(tileFrame, dstPos);
                }
            }

            //Value 255=0xFF="ARRO0003" (interior theater floor arrow) is a bit weird.
            //https://forums.cncnet.org/topic/6604-question-about-templates-in-mappack-section/?tab=comments#comment-53077
            //Quoting Blade: "Regarding values of 0x00FF for the tile id, those should be treated the same as 0xFFFF,
            //ie clear randomised tiles. This means that tileset 255, which is a floor arrow in the interior theater
            //is not valid and should never be displayed, it should be the black clear tile instead. This is the way
            //RA itself treats tiles and is probably related to the game originally handling old c&c map data during
            //development.
        }

        public void drawRadar(int scale, MapRA map, List<SpriteTDRA> sprites, IndexedImage image)
        {
            drawRadar(scale, mTileSetTable, map.Theater, image);

            //Tile layer drawing has a special case for scale 1 where the tile's land type is used to
            //determine the color of the pixels. The land type is affected by any overlay at the tile.
            //See "Recalc_Attributes()" in "CELL.CPP".
            if (scale == 1) //Land type drawing?
            {
                foreach (SpriteTDRA sprite in sprites) //Overlays affecting land type?
                {
                    SpriteOverlayRA spr = sprite as SpriteOverlayRA;
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
            bool isInterior = theater.Id == "INTERIOR";
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstPos.Y += scale)
            {
                dstPos.X = clipInTiles.X * scale;
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstPos.X += scale)
                {
                    UInt16 tileSetId;
                    byte tileSetIndex;
                    FileIcnTileSetTDRA fileIcn = getTileSet(tileSetTable, tileX, tileY, theater, out tileSetId, out tileSetIndex);
                    if (scale == 1)
                    {
                        //If interior theater then tile set id 0x0000 and 0xFFFF are drawn as rock (i.e. ICN-file == "CLEAR1.INT").
                        //See "Recalc_Attributes()" in "CELL.CPP".
                        byte radarIndex;
                        if (isInterior && (tileSetId == 0x0000 || tileSetId == 0xFFFF))
                        {
                            radarIndex = 21; //Rock.
                        }
                        else
                        {
                            radarIndex = toRadarColorIndex(fileIcn.getColorMap(tileSetIndex));
                        }
                        image[dstPos] = radarIndex;
                    }
                    else
                    {
                        if (tileSetId == 0x00FF) //Treat as a clear tile template?
                        {
                            tileSetIndex = toTileSetIndexClear(tileX, tileY);
                            fileIcn = theater.getTileSet(0);
                        }
                        Frame tileFrame = getTileFrame(fileIcn, tileSetIndex, theater);
                        RadarRA.drawScaled(tileFrame, image, dstPos, null, scale);
                    }

                    //The ingame radar seems to draw missing tiles a bit random.
                    //I'm guessing it's just garbage data from incorrect pointers?
                }
            }
        }

        private static byte toRadarColorIndex(byte tileColorMap)
        {
            //The tile color map value from the ICN-file is converted to a land type.
            //See "Land_Type()" in "CDATA.CPP". The land type in turn is converted to a
            //palette index ("CONST.CPP"). Let's just return the index directly.
            switch (tileColorMap)
            {
                case 0: return 141; //Clear.
                case 1: return 141; //Clear.
                case 2: return 141; //Clear.
                case 3: return 141; //Clear.
                case 4: return 141; //Clear.
                case 5: return 141; //Clear.
                case 6: return 141; //Beach.
                case 7: return 141; //Clear.
                case 8: return 21; //Rock.
                case 9: return 141; //Road.
                case 10: return 172; //Water.
                case 11: return 174; //River.
                case 12: return 141; //Clear.
                case 13: return 141; //Clear.
                case 14: return 141; //Rough.
                case 15: return 141; //Clear.
                default:
                    Program.warn(string.Format("Undefined radar tile color map '{0}'!", tileColorMap));
                    return 141; //Clear.
            }
        }
    }
}
