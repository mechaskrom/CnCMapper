using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    //Map's background layer which everything else is drawn upon.
    //Made of tiles also referred to as "icons" or "templates".
    //Tile sets are stored in the "ICON.MAP" file. Tiles are stored in the "ICON.ICN" file.
    //Arrangement table is generated from a seed value in the INI-file.

    class GroundLayerD2 : GroundLayerCnC
    {
        private const int TileSetTableLength = MapD2.WidthInTiles * MapD2.HeightInTiles;

        private readonly MapD2 mMap;
        private readonly byte[][] mTileSetTable; //2 arrays, tile set id and tile set index.

        private GroundLayerD2(MapD2 map)
        {
            mMap = map;
            mTileSetTable = new byte[2][];
        }

        public static GroundLayerD2 create(MapD2 map, List<SpriteStructureD2> structures)
        {
            //Need a list of structures because spice fields are affected by them.
            //Spice fields are only added to sand tiles which structures can't be built on.
            //So normally it shouldn't matter, but structures in an INI-file can be
            //placed on sand tiles.

            IniSection mapSection = map.FileIni.getSection("MAP");

            int seed = mapSection.getKey("Seed").valueAsInt32();

            IniKey fieldKey = mapSection.findKey("Field");
            string[] fieldValues = fieldKey != null ? fieldKey.Value.Split(',') : null;

            IniKey bloomKey = mapSection.findKey("Bloom");
            string[] bloomValues = bloomKey != null ? bloomKey.Value.Split(',') : null;

            IniKey specialKey = mapSection.findKey("Special");
            string[] specialValues = specialKey != null ? specialKey.Value.Split(',') : null;

            GroundLayerD2 groundLayer = new GroundLayerD2(map);
            groundLayer.load(seed, fieldValues, bloomValues, specialValues, structures);
            return groundLayer;
        }

        private void load(int seed, string[] fieldValues, string[] bloomValues, string[] specialValues, List<SpriteStructureD2> structures)
        {
            RandomD2 random = new RandomD2(seed);

            //Generate terrain from seed value.
            mTileSetTable[0] = new byte[TileSetTableLength];
            mTileSetTable[0].clear(TileSetD2.IdTerrain);
            mTileSetTable[1] = MapGeneratorD2.createTerrain(random);

            //Add spice fields, spice blooms and spice specials. Order is bloom, field
            //and special. Checked in game. Draw priority is lower than sprites (structures
            //and units) in the game i.e. they exist in the ground layer.
            if (bloomValues != null)
            {
                foreach (string bloomValue in bloomValues)
                {
                    int tableInd = MapD2.toTileNum(bloomValue);
                    mTileSetTable[0][tableInd] = TileSetD2.IdSpiceBloom; //Tile set id.
                    mTileSetTable[1][tableInd] = 0; //Tile set index.
                }
            }

            if (fieldValues != null)
            {
                SpiceFieldD2 spiceField = new SpiceFieldD2(mTileSetTable, mMap, random, structures);
                foreach (string fieldValue in fieldValues)
                {
                    int tableInd = MapD2.toTileNum(fieldValue);
                    spiceField.add(tableInd, 5); //Radius of fields is 5.
                }
            }

            if (specialValues != null)
            {
                foreach (string specialValue in specialValues)
                {
                    int tableInd = MapD2.toTileNum(specialValue);
                    mTileSetTable[0][tableInd] = TileSetD2.IdSpiceBloom; //Tile set id.
                    mTileSetTable[1][tableInd] = 1; //Tile set index.
                }
            }
        }

        private static Rectangle toInTiles(Rectangle rectInPixels)
        {
            return toInTiles(rectInPixels, MapD2.TileWidth, MapD2.TileHeight);
        }

        public void draw(IndexedImage image)
        {
            draw(mTileSetTable, mMap.Theater, image);
        }

        private static void draw(byte[][] tileSetTable, TheaterD2 theater, IndexedImage image)
        {
            //Only draw tiles inside clip area.
            Rectangle clipInTiles = toInTiles(image.Clip);
            Point dstPos = new Point(0, clipInTiles.Y * MapD2.TileHeight);
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstPos.Y += MapD2.TileHeight)
            {
                dstPos.X = clipInTiles.X * MapD2.TileWidth; //Restore X position i.e. start a new row.
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstPos.X += MapD2.TileWidth)
                {
                    int tableInd = tileX + (tileY * MapD2.WidthInTiles);
                    byte tileSetId = tileSetTable[0][tableInd];
                    byte tileSetIndex = tileSetTable[1][tableInd];
                    Frame tileFrame = theater.getTileFrame(tileSetId, tileSetIndex);
                    image.write(tileFrame, dstPos);
                }
            }
        }

        public void drawRadar(int scale, IndexedImage image)
        {
            drawRadar(scale, mTileSetTable, mMap.Theater, image);
        }

        private static void drawRadar(int scale, byte[][] tileSetTable, TheaterD2 theater, IndexedImage image)
        {
            //Only draw tiles inside clip area.
            Rectangle clipInTiles = toInTiles(image.Clip, scale);
            Rectangle dstRect = new Rectangle(0, clipInTiles.Y * scale, scale, scale);
            for (int tileY = clipInTiles.Y; tileY < clipInTiles.Bottom; tileY++, dstRect.Y += scale)
            {
                dstRect.X = clipInTiles.X * scale;
                for (int tileX = clipInTiles.X; tileX < clipInTiles.Right; tileX++, dstRect.X += scale)
                {
                    int tableInd = tileX + (tileY * MapD2.WidthInTiles);
                    byte tileSetId = tileSetTable[0][tableInd];
                    byte tileSetIndex = tileSetTable[1][tableInd];

                    byte radarIndex = theater.getLandType(tileSetId, tileSetIndex).RadarIndex;
                    drawRadarTile(image.Pixels, image.Stride, dstRect, radarIndex);
                }
            }
        }

        private static void drawRadarTile(byte[] dstPixels, int dstStride, Rectangle dstRect, byte radarIndex)
        {
            //Draw a solid colored rectangle at destination.
            int k = dstRect.X + (dstRect.Y * dstStride);
            for (int y = 0; y < dstRect.Height; y++, k += dstStride)
            {
                for (int x = 0; x < dstRect.Width; x++)
                {
                    dstPixels[k + x] = radarIndex;
                }
            }
        }
    }
}
