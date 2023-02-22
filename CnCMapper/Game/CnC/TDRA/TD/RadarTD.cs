using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class RadarTD : RadarTDRA
    {
        //Consider the current radar renderer to be in a beta stage. It isn't that well tested, especially at certain
        //scales, and doesn't 100% match the game in some cases. Particularly draw priority of overlapping objects.
        //It should handle common scales and all normal mission maps just fine though as far as I can tell.

        //Different types of radar jamming and cloaking is ignored and anything hidden on the radar by that is drawn.

        //Scale can be seen as pixels per tile i.e. how many pixels should 24 pixels be turned into.
        //Scale in TD is 6 (zoomed in) or 2 (zoomed out, whole map) i.e. 384*384 (64*6=384) or 128*128 (64*2=128) image.
        //Scale in DOS-version is 3 (zoomed in) or 1 (zoomed out, whole map) i.e. 192*192 (64*3=192) or 64*64 (64*1=64) image.
        //Depends on screen resolution, windows is 640*480 while DOS is only 320*200.

        public static IndexedImage[] preCreateImages()
        {
            return preCreateImages(GameTD.Config.RadarScales, ScaleMax, MapTD.SizeInTiles);
        }

        public static void saveRadarImage(MapTD map, GroundLayerTD groundLayer, List<SpriteTDRA> sprites, string folderPath, string fileName, IndexedImage[] images)
        {
            //Set radar color mode before rendering and saving the image.
            if (map.FileIni.Id.StartsWith("SCJ", StringComparison.Ordinal)) //Map is a funpark/jurassic mission?
            {
                HouseTD.setRadarColorMode(ColorSchemeTD.Radar.Mode.Jurassic);
            }
            else
            {
                HouseTD.setRadarColorMode(ColorSchemeTD.Radar.Mode.Default);
            }

            string filePath = Path.Combine(folderPath, fileName + " [scale ");
            foreach (int scale in GameTD.Config.RadarScales)
            {
                IndexedImage image = images[scale - 1];
                System.Diagnostics.Debug.Assert(image != null, "Image not pre-created for all used scales!");

                //Trim used later when saving image.
                Rectangle trimRect = getTrimRect(map.BordersInTiles, GameTD.Config.OutsideMapBorders, MapTD.AreaInTiles, scale);
                image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

                draw(scale, map, groundLayer, sprites, image);

                if (GameTD.Config.ShadeOutsideMapBorders) //Shade area outside borders?
                {
                    image.drawOutsideShade(map.BordersInTiles.scaleUp(scale), map.Theater.getOutsideRemap());
                }

                System.Diagnostics.Debug.Assert(image.Width == MapTD.WidthInTiles * scale &&
                    image.Height == MapTD.HeightInTiles * scale, "Image should be the size of a full radar!");

                saveImage(trimRect, map.Theater.GamePalette, filePath + scale + "].png", image);
            }
        }

        private static void draw(int scale, MapTD map, GroundLayerTD groundLayer, List<SpriteTDRA> sprites, IndexedImage image)
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
                if (sprite is SpriteStructureTD)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //3. Overlays.
            {
                if (sprite is SpriteOverlayTD)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //4. Terrain.
            {
                if (sprite is SpriteTerrainTD)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteTDRA sprite in sprites) //5. Infantries and units.
            {
                if (sprite is SpriteInfantryTD || sprite is SpriteUnitTD)
                {
                    sprite.drawRadar(scale, map, image);
                }
            }
        }
    }
}
