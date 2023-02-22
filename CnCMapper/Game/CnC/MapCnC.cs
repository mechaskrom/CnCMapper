using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC
{
    class MapCnC
    {
        private const int BorderMargin = 48; //48 pixels margin which is normally 3 tiles in D2 and 2 tiles in TDRA.
        private const int BezelMargin = 24; //Max margin beyond a map's full size.

        protected readonly FileIni mFileIni;
        protected readonly string mNamesakeCount; //Used when saving maps with same name, but different content.
        protected readonly Rectangle mBordersInTiles;

        protected struct RenderParams
        {
            public string folderPath;
            public string folderPathRadar;
            public IndexedImage image; //Pre-allocated and reused for every map.
            public IndexedImage[] imagesRadar; //Pre-allocated (for each scale) and reused for every map.
        }

        protected MapCnC(FileIni fileIni, int namesakeCount, Rectangle bordersInTiles)
        {
            mFileIni = fileIni;
            mNamesakeCount = namesakeCount <= 1 ? string.Empty : " (" + namesakeCount + ")";
            mBordersInTiles = bordersInTiles;
        }

        public FileIni FileIni
        {
            get { return mFileIni; }
        }

        public Rectangle BordersInTiles
        {
            get { return mBordersInTiles; }
        }

        protected static TilePos toTilePos(string tileNumber, int widthInTiles)
        {
            return toTilePos(toTileNum(tileNumber), widthInTiles);
        }

        protected static TilePos toTilePos(int tileNumber, int widthInTiles)
        {
            return new TilePos(tileNumber % widthInTiles, tileNumber / widthInTiles);
        }

        public static int toTileNum(string tileNumber)
        {
            return int.Parse(tileNumber);
        }

        protected static int toTileNum(TilePos tilePos, int widthInTiles)
        {
            return tilePos.X + (tilePos.Y * widthInTiles);
        }

        protected static Rectangle toInPixels(Rectangle rectInTiles, int tileWidth, int tileHeight)
        {
            return new Rectangle(
                rectInTiles.X * tileWidth, rectInTiles.Y * tileHeight,
                rectInTiles.Width * tileWidth, rectInTiles.Height * tileHeight);
        }

        protected static Rectangle getTrimRect(Rectangle borders, ConfigCnC.Outside outside, Rectangle mapArea)
        {
            if (outside == ConfigCnC.Outside.remove) //Remove all outside borders?
            {
                return borders;
            }
            if (outside == ConfigCnC.Outside.margin) //Keep a margin outside borders?
            {
                borders.Inflate(BorderMargin, BorderMargin); //Inflate borders.
                mapArea.Inflate(BezelMargin, BezelMargin);
                borders.Intersect(mapArea); //Limit margin to not exceed a bezeled full map.
                return borders;
            }
            if (outside == ConfigCnC.Outside.bezel) //Keep all and add a black bezel/frame?
            {
                mapArea.Inflate(BezelMargin, BezelMargin);
                return mapArea;
            }
            //Keep all outside borders i.e. full map size.
            return mapArea;
        }

        protected void saveImage(Rectangle trimRect, Palette6Bit palette, MapInfoDrawerCnC mapInfoDrawer, bool doDrawHeader, string filePath, IndexedImage image)
        {
            image = image.getImageTrimmed(trimRect); //Will resize image if needed.

            if (doDrawHeader) //Draw map header and footer on finished image? Must do this last because image may have been resized.
            {
                mapInfoDrawer.drawHeader(mFileIni, image);
            }

            image.save(image.Clip, palette, filePath); //Save visible (clipped) part of image.
        }

        protected static void renderAll<T>(List<T> maps, RenderParams renderParams,
            Action<T, RenderParams> renderMap) where T : MapCnC
        {
            //Visible part of image is always completely overwritten so it can be pre-allocated and reused for every map.
            //Reduces allocations and makes rendering many maps a bit faster. Same for radar images also.
            try
            {
                Program.message(string.Format("Rendering of {0} maps started...", maps.Count));

                //Make sure map and radar folders exist.
                Directory.CreateDirectory(renderParams.folderPath);
                Directory.CreateDirectory(renderParams.folderPathRadar);

                for (int i = 0; i < maps.Count; i++)
                {
                    T map = maps[i];
                    Program.message(string.Format("Rendering {0}/{1} = '{2}'", i + 1, maps.Count, map.FileIni.Id));
                    renderMap(map, renderParams);
                }
                Program.message("Rendering finished!");
            }
#if DEBUG
            finally
            {
            }
#else
            catch (Exception ex)
            {
                Program.message("Couldn't finish rendering because: " + ex.Message);
            }
#endif
        }
    }
}
