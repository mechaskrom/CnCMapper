using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteTerrainRA : SpriteTDRA
    {
        private TerrainRA mTerrain = null; //Constant data associated with terrain type.

        private SpriteTerrainRA(TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(fileShp.Id, tilePos, fileShp)
        {
        }

        public static void endAdd(List<SpriteTerrainRA> terrains, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(terrains);
        }

        public static void add(MapRA map, List<SpriteTerrainRA> terrains)
        {
            //Format: tileNumber=id
            //Example: 4801=TC02
            IniSection iniSection = map.FileIni.findSection("TERRAIN");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    TilePos tilePos = MapRA.toTilePos(key.Id);
                    string id = key.Value;
                    SpriteTerrainRA spr = new SpriteTerrainRA(tilePos, map.Theater.getSpriteSet(id));
                    addInner(spr, map, terrains);
                }
            }
        }

        private static void addInner(SpriteTerrainRA spr, MapRA map, List<SpriteTerrainRA> terrains)
        {
            //BOXES01-BOXES09 are only defined for INTERIOR theater.
            //ICE01-ICE05 are only defined for SNOW theater.
            //All other terrain (gold mine, tree and tree clump (MINE, T01-T17, TC01-TC05)) are only
            //defined for SNOW and TEMPERATE theater.
            //Ice sheet SHP-files are present in temperate theater MIX-file, but will not appear.

            //Ice sheets act a bit weird when overlapping with other terrain. Sometimes only
            //one terrain per tile is drawn (ice or other terrain). Seems a bit random though
            //and usually ice sheets don't overlap so not worth worrying about?

            switch (spr.Id)
            {
                case "BOXES01": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES01; addDefault(spr, map, terrains); break;
                case "BOXES02": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES02; addDefault(spr, map, terrains); break;
                case "BOXES03": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES03; addDefault(spr, map, terrains); break;
                case "BOXES04": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES04; addDefault(spr, map, terrains); break;
                case "BOXES05": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES05; addDefault(spr, map, terrains); break;
                case "BOXES06": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES06; addDefault(spr, map, terrains); break;
                case "BOXES07": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES07; addDefault(spr, map, terrains); break;
                case "BOXES08": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES08; addDefault(spr, map, terrains); break;
                case "BOXES09": //Boxes.
                    spr.mTerrain = TerrainRA.BOXES09; addDefault(spr, map, terrains); break;
                case "ICE01": //Ice sheets.
                    spr.mTerrain = TerrainRA.ICE01; addDefault(spr, map, terrains); break;
                case "ICE02": //Ice sheets.
                    spr.mTerrain = TerrainRA.ICE02; addDefault(spr, map, terrains); break;
                case "ICE03": //Ice sheets.
                    spr.mTerrain = TerrainRA.ICE03; addDefault(spr, map, terrains); break;
                case "ICE04": //Ice sheets.
                    spr.mTerrain = TerrainRA.ICE04; addDefault(spr, map, terrains); break;
                case "ICE05": //Ice sheets.
                    spr.mTerrain = TerrainRA.ICE05; addDefault(spr, map, terrains); break;
                case "MINE": //Gold mine.
                    spr.mTerrain = TerrainRA.MINE; addDefault(spr, map, terrains); break;
                case "T01": //Tree.
                    spr.mTerrain = TerrainRA.T01; addDefault(spr, map, terrains); break;
                case "T02": //Tree.
                    spr.mTerrain = TerrainRA.T02; addDefault(spr, map, terrains); break;
                case "T03": //Tree.
                    spr.mTerrain = TerrainRA.T03; addDefault(spr, map, terrains); break;
                case "T05": //Tree.
                    spr.mTerrain = TerrainRA.T05; addDefault(spr, map, terrains); break;
                case "T06": //Tree.
                    spr.mTerrain = TerrainRA.T06; addDefault(spr, map, terrains); break;
                case "T07": //Tree.
                    spr.mTerrain = TerrainRA.T07; addDefault(spr, map, terrains); break;
                case "T08": //Tree.
                    spr.mTerrain = TerrainRA.T08; addDefault(spr, map, terrains); break;
                case "T10": //Tree.
                    spr.mTerrain = TerrainRA.T10; addDefault(spr, map, terrains); break;
                case "T11": //Tree.
                    spr.mTerrain = TerrainRA.T11; addDefault(spr, map, terrains); break;
                case "T12": //Tree.
                    spr.mTerrain = TerrainRA.T12; addDefault(spr, map, terrains); break;
                case "T13": //Tree.
                    spr.mTerrain = TerrainRA.T13; addDefault(spr, map, terrains); break;
                case "T14": //Tree.
                    spr.mTerrain = TerrainRA.T14; addDefault(spr, map, terrains); break;
                case "T15": //Tree.
                    spr.mTerrain = TerrainRA.T15; addDefault(spr, map, terrains); break;
                case "T16": //Tree.
                    spr.mTerrain = TerrainRA.T16; addDefault(spr, map, terrains); break;
                case "T17": //Tree.
                    spr.mTerrain = TerrainRA.T17; addDefault(spr, map, terrains); break;
                case "TC01": //Tree clump.
                    spr.mTerrain = TerrainRA.TC01; addDefault(spr, map, terrains); break;
                case "TC02": //Tree clump.
                    spr.mTerrain = TerrainRA.TC02; addDefault(spr, map, terrains); break;
                case "TC03": //Tree clump.
                    spr.mTerrain = TerrainRA.TC03; addDefault(spr, map, terrains); break;
                case "TC04": //Tree clump.
                    spr.mTerrain = TerrainRA.TC04; addDefault(spr, map, terrains); break;
                case "TC05": //Tree clump.
                    spr.mTerrain = TerrainRA.TC05; addDefault(spr, map, terrains); break;
                default: //Undefined terrain id.
                    spr.mTerrain = TerrainRA.Default; addUndefined(spr, terrains); break;
            }
        }

        private static void addDefault(SpriteTerrainRA spr, MapRA map, List<SpriteTerrainRA> terrains)
        {
            if (!map.Theater.isSpecified(spr.mTerrain.TheaterFlags)) //Terrain isn't specified for theater?
            {
                Program.warn(string.Format("Terrain sprite id '{0}' isn't specified for theater '{1}'!", spr.Id, map.Theater.Name));
                if (!GameRA.Config.AddUnspecifiedTheaterGraphics)
                {
                    return; //Don't add terrain.
                }
            }
            spr.mPriOffset = spr.mTerrain.PriOffset;
            terrains.Add(spr);
        }

        private static void addUndefined(SpriteTerrainRA spr, List<SpriteTerrainRA> terrains)
        {
            Program.warn(string.Format("Undefined terrain sprite id '{0}'!", spr.Id));
            if (GameRA.Config.AddUndefinedSprites)
            {
                spr.mPriOffset = getPriOffset(spr.Width / 2, (spr.Height / 4) * 3); //Assume near bottom center.
                terrains.Add(spr);
            }
        }

        public static Point getPriOffset(int pixelX, int pixelY)
        {
            //Terrain draw priority is from a pixel position (usually near base/root of rock/tree).
            //Red Alert almost same as Tiberian Dawn, but does rounding up. Checked in source.
            //Source uses tile width for both axis (bug?), but they are always(?) same anyway.
            return toPriOffset(
                ((pixelX * 256) + (MapRA.TileWidth / 2)) / MapRA.TileWidth,
                ((pixelY * 256) + (MapRA.TileHeight / 2)) / MapRA.TileHeight);
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            if (scale == 1)
            {
                //Draw a pixel at every tile the terrain occupies or overlaps.
                //From source, see "Render_Terrain()" in "RADAR.CPP".
                foreach (Point occupyOffset in mTerrain.Occupies)
                {
                    image[TilePos.X + occupyOffset.X, TilePos.Y + occupyOffset.Y] = 21;
                }
                foreach (Point overlapOffset in mTerrain.Overlaps)
                {
                    image[TilePos.X + overlapOffset.X, TilePos.Y + overlapOffset.Y] = 21;
                }
            }
            else
            {
                RadarRA.drawMiniTiles(scale, this, map.Theater.getBrightenRemap(), image, false);
            }
        }
    }
}
