using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteTerrainTD : SpriteTDRA
    {
        private TerrainTD mTerrain = null; //Constant data associated with terrain type.

        private SpriteTerrainTD(TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(fileShp.Id, tilePos, fileShp)
        {
        }

        public static void endAdd(List<SpriteTerrainTD> terrains, List<SpriteTDRA> sprites)
        {
            sprites.AddDerivedRange(terrains);
        }

        public static void add(MapTD map, List<SpriteTerrainTD> terrains)
        {
            //Format: tileNumber=id,trigger
            //Example: 1621=ROCK1,None
            //Trigger doesn't seem to work for terrain? Always 'None'?
            IniSection iniSection = map.FileIni.findSection("TERRAIN");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    TilePos tilePos = MapTD.toTilePos(key.Id);
                    string[] values = key.Value.Split(',');
                    string id = values[0];
                    string trigger = values[1];
                    SpriteTerrainTD spr = new SpriteTerrainTD(tilePos, map.Theater.getSpriteSet(id));
                    spr.mTrigger = trigger;
                    addInner(spr, map, terrains);
                }
            }
        }

        private static void addInner(SpriteTerrainTD spr, MapTD map, List<SpriteTerrainTD> terrains)
        {
            //ROCK1-ROCK7, T04, T09 and T18 are only defined for DESERT theater.
            //SPLIT3 and T08 are only defined for TEMPERATE, WINTER and DESERT theater.
            //All other terrain are only defined for TEMPERATE and WINTER theater.

            switch (spr.Id)
            {
                case "ROCK1": //Rock.
                    spr.mTerrain = TerrainTD.ROCK1; addDefault(spr, map, terrains); break;
                case "ROCK2": //Rock.
                    spr.mTerrain = TerrainTD.ROCK2; addDefault(spr, map, terrains); break;
                case "ROCK3": //Rock.
                    spr.mTerrain = TerrainTD.ROCK3; addDefault(spr, map, terrains); break;
                case "ROCK4": //Rock.
                    spr.mTerrain = TerrainTD.ROCK4; addDefault(spr, map, terrains); break;
                case "ROCK5": //Rock.
                    spr.mTerrain = TerrainTD.ROCK5; addDefault(spr, map, terrains); break;
                case "ROCK6": //Rock.
                    spr.mTerrain = TerrainTD.ROCK6; addDefault(spr, map, terrains); break;
                case "ROCK7": //Rock.
                    spr.mTerrain = TerrainTD.ROCK7; addDefault(spr, map, terrains); break;
                case "SPLIT2": //Blossom/Tiberium tree.
                    spr.mTerrain = TerrainTD.SPLIT2; addSplit(spr, map, terrains); break;
                case "SPLIT3": //Blossom/Tiberium tree.
                    spr.mTerrain = TerrainTD.SPLIT3; addSplit(spr, map, terrains); break;
                case "T01": //Tree.
                    spr.mTerrain = TerrainTD.T01; addDefault(spr, map, terrains); break;
                case "T02": //Tree.
                    spr.mTerrain = TerrainTD.T02; addDefault(spr, map, terrains); break;
                case "T03": //Tree.
                    spr.mTerrain = TerrainTD.T03; addDefault(spr, map, terrains); break;
                case "T04": //Tree.
                    spr.mTerrain = TerrainTD.T04; addDefault(spr, map, terrains); break;
                case "T05": //Tree.
                    spr.mTerrain = TerrainTD.T05; addDefault(spr, map, terrains); break;
                case "T06": //Tree.
                    spr.mTerrain = TerrainTD.T06; addDefault(spr, map, terrains); break;
                case "T07": //Tree.
                    spr.mTerrain = TerrainTD.T07; addDefault(spr, map, terrains); break;
                case "T08": //Tree.
                    spr.mTerrain = TerrainTD.T08; addDefault(spr, map, terrains); break;
                case "T09": //Tree.
                    spr.mTerrain = TerrainTD.T09; addDefault(spr, map, terrains); break;
                case "T10": //Tree.
                    spr.mTerrain = TerrainTD.T10; addDefault(spr, map, terrains); break;
                case "T11": //Tree.
                    spr.mTerrain = TerrainTD.T11; addDefault(spr, map, terrains); break;
                case "T12": //Tree.
                    spr.mTerrain = TerrainTD.T12; addDefault(spr, map, terrains); break;
                case "T13": //Tree.
                    spr.mTerrain = TerrainTD.T13; addDefault(spr, map, terrains); break;
                case "T14": //Tree.
                    spr.mTerrain = TerrainTD.T14; addDefault(spr, map, terrains); break;
                case "T15": //Tree.
                    spr.mTerrain = TerrainTD.T15; addDefault(spr, map, terrains); break;
                case "T16": //Tree.
                    spr.mTerrain = TerrainTD.T16; addDefault(spr, map, terrains); break;
                case "T17": //Tree.
                    spr.mTerrain = TerrainTD.T17; addDefault(spr, map, terrains); break;
                case "T18": //Tree.
                    spr.mTerrain = TerrainTD.T18; addDefault(spr, map, terrains); break;
                case "TC01": //Tree clump.
                    spr.mTerrain = TerrainTD.TC01; addDefault(spr, map, terrains); break;
                case "TC02": //Tree clump.
                    spr.mTerrain = TerrainTD.TC02; addDefault(spr, map, terrains); break;
                case "TC03": //Tree clump.
                    spr.mTerrain = TerrainTD.TC03; addDefault(spr, map, terrains); break;
                case "TC04": //Tree clump.
                    spr.mTerrain = TerrainTD.TC04; addDefault(spr, map, terrains); break;
                case "TC05": //Tree clump.
                    spr.mTerrain = TerrainTD.TC05; addDefault(spr, map, terrains); break;
                default: //Undefined terrain id.
                    spr.mTerrain = TerrainTD.Default; addUndefined(spr, terrains); break;
            }
        }

        private static void addSplit(SpriteTerrainTD spr, MapTD map, List<SpriteTerrainTD> terrains)
        {
            //Frame 30 is more obvious blossom/tiberium tree than default (frame 0).
            spr.mFrameIndex = GameTD.Config.ExposeTiberiumTrees ? 30 : spr.mFrameIndex;
            addDefault(spr, map, terrains);
        }

        private static void addDefault(SpriteTerrainTD spr, MapTD map, List<SpriteTerrainTD> terrains)
        {
            if (!map.Theater.isSpecified(spr.mTerrain.TheaterFlags)) //Terrain isn't specified for theater?
            {
                Program.warn(string.Format("Terrain sprite id '{0}' isn't specified for theater '{1}'!", spr.Id, map.Theater.Name));
                if (!GameTD.Config.AddUnspecifiedTheaterGraphics)
                {
                    return; //Don't add terrain.
                }
            }
            spr.mPriOffset = spr.mTerrain.PriOffset;
            terrains.Add(spr);
        }

        private static void addUndefined(SpriteTerrainTD spr, List<SpriteTerrainTD> terrains)
        {
            Program.warn(string.Format("Undefined terrain sprite id '{0}'!", spr.Id));
            if (GameTD.Config.AddUndefinedSprites)
            {
                spr.mPriOffset = getPriOffset(spr.Width / 2, (spr.Height / 4) * 3); //Assume near bottom center.
                terrains.Add(spr);
            }
        }

        public static Point getPriOffset(int pixelX, int pixelY)
        {
            //Terrain draw priority is from a pixel position (usually near base/root of rock/tree).
            //Tiberian Dawn does no rounding unlike Red Alert. Checked in source.
            return toPriOffset((pixelX * 256) / MapTD.TileWidth, (pixelY * 256) / MapTD.TileHeight);
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            if (scale == 1)
            {
                //Draw a pixel at every tile the terrain occupies or overlaps.
                //From source, see "Render_Terrain()" in "RADAR.CPP".
                foreach (Point occupyOffset in mTerrain.Occupies)
                {
                    image[TilePos.X + occupyOffset.X, TilePos.Y + occupyOffset.Y] = 60;
                }
                foreach (Point overlapOffset in mTerrain.Overlaps)
                {
                    image[TilePos.X + overlapOffset.X, TilePos.Y + overlapOffset.Y] = 60;
                }
            }
            else
            {
                RadarTD.drawMiniTiles(scale, this, map.Theater.getBrightenRemap(), image, false);
            }
        }
    }
}
