using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class RadarD2 : RadarCnC
    {
        //Consider the current radar renderer to be in a beta stage. It isn't that well tested.
        //It should handle all normal mission maps just fine though as far as I can tell.

        //Scale can be seen as pixels per tile i.e. how many pixels should 16 pixels be turned into.
        //Scale in D2 is always 1?

        private const int ScaleMin = 1;
        private const int ScaleMax = 8;
        public const string ConfigScaleValues = "comma separated integers, 1 to 8"; //For config file attribute. Must be const.

        public static int[] getConfigScales(IniKey iniKey)
        {
            return getConfigScales(iniKey, ScaleMin, ScaleMax);
        }

        public static IndexedImage[] preCreateImages()
        {
            return preCreateImages(GameD2.Config.RadarScales, ScaleMax, MapD2.SizeInTiles);
        }

        public static void saveRadarImage(MapD2 map, GroundLayerD2 groundLayer, List<SpriteD2> sprites, string folderPath, string fileName, IndexedImage[] images)
        {
            string filePath = Path.Combine(folderPath, fileName + " [scale ");
            foreach (int scale in GameD2.Config.RadarScales)
            {
                IndexedImage image = images[scale - 1];
                System.Diagnostics.Debug.Assert(image != null, "Image not pre-created for all used scales!");

                //Trim used later when saving image.
                Rectangle trimRect = getTrimRect(map.BordersInTiles, GameD2.Config.OutsideMapBorders, MapD2.AreaInTiles, scale);
                image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

                draw(scale, map, groundLayer, sprites, image);

                if (GameD2.Config.ShadeOutsideMapBorders) //Shade area outside borders?
                {
                    image.drawOutsideShade(map.BordersInTiles.scaleUp(scale), map.Theater.getOutsideRemap());
                }

                System.Diagnostics.Debug.Assert(image.Width == MapD2.WidthInTiles * scale &&
                    image.Height == MapD2.HeightInTiles * scale, "Image should be the size of a full radar!");

                saveImage(trimRect, map.Theater.GamePalette, filePath + scale + "].png", image);
            }
        }

        private static void draw(int scale, MapD2 map, GroundLayerD2 groundLayer, List<SpriteD2> sprites, IndexedImage image)
        {
            //The many foreach loops may look a bit inefficient, but it's faster than making a copy
            //of the sprite list and then sort it with a special comparer for radar sprites.

            //Using the sprite list as is though means that any draw priority tweaks (i.e. exposed
            //sprites with elevated priority) will show in the radar. But considering how the
            //radar is drawn in layers this should rarely affect anything.
            //The radar should reflect the drawn map anyway so this shouldn't really be a problem.

            //Units have higher draw priority than structures in the map, but in the radar the
            //opposite is true. Except concrete slabs and walls which are drawn before units.

            //Draw order is: ground layer -> structures (concrete) -> units -> structures (buildings).
            groundLayer.drawRadar(scale, image); //1. Ground layer.

            foreach (SpriteD2 sprite in sprites) //2. Structures (concrete).
            {
                SpriteStructureD2 structure = sprite as SpriteStructureD2;
                if (structure != null && structure.IsSlabOrWall)
                {
                    structure.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteD2 sprite in sprites) //3. Units.
            {
                SpriteUnitD2 unit = sprite as SpriteUnitD2;
                if (unit != null)
                {
                    unit.drawRadar(scale, map, image);
                }
            }
            foreach (SpriteD2 sprite in sprites) //4. Structures (buildings).
            {
                SpriteStructureD2 structure = sprite as SpriteStructureD2;
                if (structure != null && !structure.IsSlabOrWall)
                {
                    structure.drawRadar(scale, map, image);
                }
            }
        }
    }
}
