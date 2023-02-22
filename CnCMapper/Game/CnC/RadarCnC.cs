using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC
{
    class RadarCnC
    {
        private const int BorderMarginInTiles = 2;
        private const int BezelMarginInTiles = 1; //Max margin beyond a map's full size.

        protected static int[] getConfigScales(IniKey iniKey, int scaleMin, int scaleMax)
        {
            List<int> scales = new List<int>();
            foreach (string field in iniKey.Value.Split(','))
            {
                if (field != string.Empty)
                {
                    Int32 v;
                    if (!Int32.TryParse(field, out v))
                    {
                        throw iniKey.ParentSection.ParentFile.newArgError(string.Format(
                            "Couldn't parse '{0}/{1}/{2}' as comma separated integers!",
                            iniKey.ParentSection.Id, iniKey.Id, iniKey.Value));
                    }
                    scales.Add(v.clip(scaleMin, scaleMax));
                }
            }
            return scales.Distinct().ToArray();
        }

        protected static IndexedImage[] preCreateImages(int[] radarScales, int scaleMax, Size mapSizeInTiles)
        {
            //Pre-allocate images used (re-used per map) for drawing radar at different scales.
            IndexedImage[] images = new IndexedImage[scaleMax];
            foreach (int scale in radarScales)
            {
                images[scale - 1] = new IndexedImage(mapSizeInTiles.Width * scale, mapSizeInTiles.Height * scale);
            }
            return images;
        }

        protected static Rectangle getTrimRect(Rectangle bordersInTiles, ConfigCnC.Outside outside, Rectangle mapAreaInTiles, int scale)
        {
            if (outside == ConfigCnC.Outside.remove) //Remove all outside borders?
            {
                return bordersInTiles.scaleUp(scale);
            }
            if (outside == ConfigCnC.Outside.margin) //Keep a margin outside borders?
            {
                bordersInTiles.Inflate(BorderMarginInTiles, BorderMarginInTiles); //Inflate borders.
                mapAreaInTiles.Inflate(BezelMarginInTiles, BezelMarginInTiles);
                bordersInTiles.Intersect(mapAreaInTiles); //Limit margin to not exceed a bezeled full map.
                return bordersInTiles.scaleUp(scale);
            }
            if (outside == ConfigCnC.Outside.bezel) //Keep all and add a black bezel/frame?
            {
                mapAreaInTiles.Inflate(BezelMarginInTiles, BezelMarginInTiles);
                return mapAreaInTiles.scaleUp(scale);
            }
            //Keep all outside borders i.e. full map size.
            return mapAreaInTiles.scaleUp(scale);
        }

        protected static void saveImage(Rectangle trimRect, Palette6Bit palette, string filePath, IndexedImage image)
        {
            image = image.getImageTrimmed(trimRect); //Will resize image if needed.
            image.save(image.Clip, palette, filePath); //Save visible (clipped) part of image.
        }
    }
}
