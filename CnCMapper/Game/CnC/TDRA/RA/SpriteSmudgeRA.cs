using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteSmudgeRA : SpriteSmudgeTDRA
    {
        private SpriteSmudgeRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
        }

        private static SpriteSmudgeRA create(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
        {
            return new SpriteSmudgeRA(id, tilePos, fileShp);
        }

        public static void endAdd(List<SpriteSmudgeRA> smudges, List<SpriteStructureRA> structures, MapRA map, List<SpriteTDRA> sprites)
        {
            //Smudges and structures in passed in lists must be in the order they were added from the INI-file.
            structures = structures.ToList(); //Make a local copy so source isn't sorted.
            structures.SortStable(compareDrawOrderRA); //Sort structures so higher priority bibs are added later.
            endAdd(smudges, structures, map, MapRA.SizeInTiles, sprites, create);
        }

        public static void add(MapRA map, List<SpriteSmudgeRA> smudges)
        {
            IniKeyFinderRA keyFinder = IniKeyFinderRA.create(map.FileIni.findSection("SMUDGE"));
            add(keyFinder, MapRA.toTilePos, map, GameRA.Config.AddUndefinedSprites, smudges, create);
        }
    }
}
