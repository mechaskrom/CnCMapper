using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class RadarRA : RadarTDRA
    {
        //Consider the current radar renderer to be in a beta stage. It isn't that well tested, especially at certain
        //scales, and doesn't 100% match the game in some cases. Particularly draw priority of overlapping objects.
        //It should handle common scales and all normal mission maps just fine though as far as I can tell.

        //Different types of radar jamming and cloaking is ignored and anything hidden on the radar by that is drawn.

        //Scale can be seen as pixels per tile i.e. how many pixels should 24 pixels be turned into.
        //Scale in RA is 3 (zoomed in) or 1 (zoomed out, whole map) i.e. a 384x384 (128*3=384) or a 128x128 (128*1=128) image.

        public static IndexedImage[] preCreateImages()
        {
            return preCreateImages(GameRA.Config.RadarScales, ScaleMax, MapRA.SizeInTiles);
        }

        public static void saveRadarImage(MapRA map, GroundLayerRA groundLayer, List<SpriteTDRA> sprites, string folderPath, string fileName, IndexedImage[] images)
        {
            string filePath = Path.Combine(folderPath, fileName + " [scale ");
            foreach (int scale in GameRA.Config.RadarScales)
            {
                IndexedImage image = images[scale - 1];
                System.Diagnostics.Debug.Assert(image != null, "Image not pre-created for all used scales!");

                //Trim used later when saving image.
                Rectangle trimRect = getTrimRect(map.BordersInTiles, GameRA.Config.OutsideMapBorders, MapRA.AreaInTiles, scale);
                image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

                draw(scale, map, groundLayer, sprites, image);

                if (GameRA.Config.ShadeOutsideMapBorders) //Shade area outside borders?
                {
                    image.drawOutsideShade(map.BordersInTiles.scaleUp(scale), map.Theater.getOutsideRemap());
                }

                System.Diagnostics.Debug.Assert(image.Width == MapRA.WidthInTiles * scale &&
                    image.Height == MapRA.HeightInTiles * scale, "Image should be the size of a full radar!");

                saveImage(trimRect, map.Theater.GamePalette, filePath + scale + "].png", image);
            }
        }

        private static void draw(int scale, MapRA map, GroundLayerRA groundLayer, List<SpriteTDRA> sprites, IndexedImage image)
        {
            //The many foreach loops may look a bit inefficient, but it's faster than making a copy
            //of the sprite list and then sort it with a special comparer for radar sprites.

            //Using the sprite list as is though means that any draw priority tweaks (i.e. exposed
            //sprites with elevated priority) will show in the radar. But considering how the
            //radar is drawn in layers this should rarely affect anything.
            //The radar should reflect the drawn map anyway so this shouldn't really be a problem.

            //Draw order is: ground layer -> structures -> overlays -> terrain -> infantries & units.
            groundLayer.drawRadar(scale, map, sprites, image); //1. Ground layer.

            foreach (SpriteTDRA sprite in sprites) //2. Structures.
            {
                if (sprite is SpriteStructureRA)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //3. Overlays.
            {
                if (sprite is SpriteOverlayRA)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //4. Terrain.
            {
                if (sprite is SpriteTerrainRA)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //5. Infantries, units and ships.
            {
                if (sprite is SpriteInfantryRA || sprite is SpriteUnitRA || sprite is SpriteShipRA)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
        }
    }
}
