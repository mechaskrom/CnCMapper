using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteSmudgeTD : SpriteSmudgeTDRA
    {
        private SpriteSmudgeTD(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
        }

        private static SpriteSmudgeTD create(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
        {
            return new SpriteSmudgeTD(id, tilePos, fileShp);
        }

        public static void endAdd(List<SpriteSmudgeTD> smudges, List<SpriteStructureTD> structures, MapTD map, List<SpriteTDRA> sprites)
        {
            //Smudges and structures in passed in lists must be in the order they were added from the INI-file.
            structures = structures.ToList(); //Make a local copy so source isn't sorted.
            structures.SortStable(compareDrawOrderTD); //Sort structures so higher priority bibs are added later.
            endAdd(smudges, structures, map, MapTD.SizeInTiles, sprites, create);
        }

        public static void add(MapTD map, List<SpriteSmudgeTD> smudges)
        {
            IniKeyFinderTD keyFinder = IniKeyFinderTD.create(map.FileIni.findSection("SMUDGE"));
            add(keyFinder, MapTD.toTilePos, map, GameTD.Config.AddUndefinedSprites, smudges, create);
        }
    }
}
