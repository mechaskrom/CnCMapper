using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class SpriteSmudgeTDRA : SpriteTDRA
    {
        //Smudges in Tiberian Dawn and Red Alert are the same. Only difference is how INI-keys are found
        //which will cause differences with duplicate keys.

        private enum SmudgeType
        {
            None = 0,
            Crater,
            Scorch,
            SmudgeBib, //Added from INI-section.
            StructBib, //Added from and owned by a structure.
            Undefined,
        }
        private SmudgeType mType = SmudgeType.None;

        private SpriteStructureTDRA mBibOwner = null; //Not null if this is a structure bib.

        protected delegate T Create<T>(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp); //Derived smudge constructor call.

        protected SpriteSmudgeTDRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(id, tilePos, fileShp)
        {
            mPriPlane = PriPlaneSmudge;
        }

        private bool IsBaseStructBib
        {
            get { return mBibOwner != null && mBibOwner.IsBase; }
        }

        private bool IsRebuiltBaseStructBib
        {
            get { return mBibOwner != null && mBibOwner.IsRebuilt && mBibOwner.IsBase; }
        }

        protected static void endAdd<TSmudge, TStruct>(List<TSmudge> smudges, List<TStruct> structures, MapTDRA map, Size mapSizeInTiles,
            List<SpriteTDRA> sprites, Create<TSmudge> create)
            where TSmudge : SpriteSmudgeTDRA
            where TStruct : SpriteStructureTDRA
        {
            //Get bibs from structures.
            List<TSmudge> baseBibs = new List<TSmudge>();
            List<TSmudge> structBibs = new List<TSmudge>();
            foreach (TStruct structure in structures)
            {
                if (structure.HasBib)
                {
                    if (structure.IsBase)
                    {
                        addStructureBibs(structure, map, baseBibs, create);
                    }
                    else
                    {
                        addStructureBibs(structure, map, structBibs, create);
                    }
                }
            }

            //Add base structure bibs directly to sprites because they don't affect smudges.
            sprites.AddDerivedRange(baseBibs);

            TileTracker<TSmudge> smudgeTracker = new TileTracker<TSmudge>(mapSizeInTiles);
            //Structure bibs can only be replaced by smudge bibs so do them first.
            foreach (TSmudge bib in structBibs)
            {
                smudgeTracker.replaceItem(bib);
            }

            //Smudges don't overlap, only first smudge at a tile number is added. Except bibs which are special.
            foreach (TSmudge smudge in smudges)
            {
                TSmudge smudgeInTile = smudgeTracker[smudge.TilePos];
                if (smudgeInTile == null || //Empty tile?
                    smudge.mType == SmudgeType.SmudgeBib) //Smudge bib? Replaces everything.
                {
                    smudgeTracker.replaceItem(smudge);
                }
                else if (smudge.mType == SmudgeType.Crater && smudgeInTile.mType == SmudgeType.Crater) //Crater over crater?
                {
                    smudgeInTile.increaseFrameIndex(); //Increase existing crater's frame index.
                }
            }

            //Add final smudge left in tiles to sprite list.
            sprites.AddDerivedRange(smudgeTracker.getTileItems());
        }

        protected static void add<T>(IniKeyFinderTDRA keyFinder, Func<string, TilePos> toTilePos, MapTDRA map,
            bool doAddUndefined, List<T> smudges, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            //Format: tileNumber1=id,tileNumber2,progression?
            //Example: 3118=CR1,3118,0
            //TileNumber1 has no effect(used as key only?), only TileNumber2 is used.
            //Progression field seems to affect frame index (checked in game):
            //-bib: Top left tile not drawn if value higher than frame count. Rest of bib is unaffected.
            //-scorch: Not drawn if value higher than frame count.
            //-crater: Unaffected?!
            //A smudge is added even if to high progression value, it's just not drawn. Checked in game.

            //Smudges are positioned top left in tile even if they aren't always 24*24 in size.

            //The game assumes that all INI-keys (first field in a line before '=') in a section
            //are unique and duplicate keys will cause weird effects.
            //The fact that smudges use the tile number also as the key probably means that there
            //should only be one smudge per tile. Using unique keys instead of tile number works
            //fine though and overlapping smudges then work like this:
            //-added by INI-file order i.e. ascending line-number.
            //-craters and scorch marks do not replace existing smudges.
            //-existing crater's frame index is increased (max 4) if a crater tried to replace it.
            //-structure bib replaces existing smudges it covers (except smudge bibs).
            //-smudge bib replaces everything it covers.
            //Structure bib = added from structure, smudge bib = added from INI-section.
            //So you could say that structure bibs are added first then smudges, but only smudge
            //bibs can replace already added smudges.

            //Same in both Tiberian Dawn and Red Alert, but RA has a different method to find INI-keys
            //which will cause differences with duplicate keys.

            if (keyFinder != null)
            {
                foreach (IniKey key in keyFinder.findKeys())
                {
                    string[] values = key.Value.Split(',');
                    string id = values[0];
                    TilePos tilePos = toTilePos(values[1]); //TileNumber2 value always used.
                    string progression = values[2];

                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                    T spr = create(id, tilePos, fileShp);
                    addInner(spr, progression, map, doAddUndefined, smudges, create);
                }
            }
        }

        private static void addInner<T>(T spr, string progression, MapTDRA map, bool doAddUndefined, List<T> smudges, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            switch (spr.Id)
            {
                case "BIB1": //Smudge bib.
                case "BIB2":
                case "BIB3":
                    addBib(spr, progression, smudges, create); break;
                case "CR1": //Crater.
                    addC1(spr, smudges); break;
                case "CR2": //Crater.
                case "CR3":
                case "CR4":
                case "CR5":
                case "CR6":
                    addCrater(spr, map, smudges, create); break;
                case "SC1": //Scorch mark.
                case "SC2":
                case "SC3":
                case "SC4":
                case "SC5":
                case "SC6":
                    addScorch(spr, progression, smudges); break;
                default: //Undefined smudge id.
                    addUndefined(spr, doAddUndefined, smudges); break;
            }
        }

        private static int toFrameIndex(string progression)
        {
            return int.Parse(progression);
        }

        private static void addBib<T>(T spr, string progression, List<T> smudges, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            //Add a complete smudge bib to sprite list.
            addSmudgeBibs(spr, progression, smudges, create);
        }

        private static void addC1<T>(T spr, List<T> smudges)
            where T : SpriteSmudgeTDRA
        {
            addDefault(spr, SmudgeType.Crater, smudges);
        }

        private static void addCrater<T>(T spr, MapTDRA map, List<T> smudges, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            //All craters are added as CR1 for some weird reason. Seems like a bug? Checked in both games.
            //Also supported by the fact that all mission maps in Tiberian Dawn only have CR1 in them.
            string id = "CR1";
            FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
            addC1(create(id, spr.TilePos, fileShp), smudges);
        }

        private static void addScorch<T>(T spr, string progression, List<T> smudges)
            where T : SpriteSmudgeTDRA
        {
            spr.mFrameIndex = toFrameIndex(progression);
            addDefault(spr, SmudgeType.Scorch, smudges);
        }

        private static void addDefault<T>(T spr, SmudgeType type, List<T> smudges)
            where T : SpriteSmudgeTDRA
        {
            spr.mType = type;
            smudges.Add(spr);
        }

        private static void addUndefined<T>(T spr, bool doAddUndefined, List<T> smudges)
            where T : SpriteSmudgeTDRA
        {
            Program.warn(string.Format("Undefined smudge sprite id '{0}'!", spr.Id));
            if (doAddUndefined)
            {
                addDefault(spr, SmudgeType.Undefined, smudges);
            }
        }

        private static void addSmudgeBibs<T>(T smudge, string progression, List<T> bibs, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            T bibStart = smudge;
            bibStart.mFrameIndex = toFrameIndex(progression);
            bibStart.mType = SmudgeType.SmudgeBib;
            bibStart.mBibOwner = null;
            addCompleteBib(bibStart, bibs, create);
        }

        private static void addStructureBibs<T>(SpriteStructureTDRA structure, MapTDRA map, List<T> bibs, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            Size structSize = structure.getSizeInTiles();
            string bibId;
            if (structSize.Width == 2) bibId = "BIB3";
            else if (structSize.Width == 3) bibId = "BIB2";
            else if (structSize.Width == 4) bibId = "BIB1";
            else
            {
                Program.warn(string.Format("Structure '{0}' is '{1}' tiles wide which doesn't match any bib (2-4 tiles wide)!",
                    structure.Id, structSize.Width));
                return; //Don't add any bibs.
            }
            T bibStart = create(bibId, structure.getBibPos(), map.Theater.getSpriteSet(bibId));
            bibStart.mFrameIndex = FrameIndex0;
            bibStart.mType = SmudgeType.StructBib;
            bibStart.mBibOwner = structure;
            addCompleteBib(bibStart, bibs, create);
        }


        private static void addCompleteBib<T>(T bibStart, List<T> bibs, Create<T> create)
            where T : SpriteSmudgeTDRA
        {
            System.Diagnostics.Debug.Assert(
                (bibStart.mType == SmudgeType.SmudgeBib && bibStart.mBibOwner == null) ||
                (bibStart.mType == SmudgeType.StructBib && bibStart.mBibOwner != null));

            //A complete bib is two tiles high so width can be calculated from its frame count.
            int bibWidth = bibStart.FrameCount / 2;
            TilePos tilePos = bibStart.TilePos;
            for (int y = 0, i = 0; y < 2; y++, tilePos.Y++)
            {
                tilePos.X = bibStart.TilePos.X;
                for (int x = 0; x < bibWidth; x++, i++, tilePos.X++)
                {
                    T bib;
                    if (i == 0) //Start bib i.e. top left tile?
                    {
                        bib = bibStart;
                    }
                    else
                    {
                        //Create bib tiles in addition to start bib.
                        bib = create(bibStart.Id, tilePos, bibStart.FileShp);
                        bib.mFrameIndex = i;
                        bib.mType = bibStart.mType;
                        bib.mBibOwner = bibStart.mBibOwner;
                    }

                    if (bib.IsBaseStructBib)
                    {
                        bib.mDrawMode = DrawMode.Dithered;

                        //Set priority for base structure bibs against normal structure bibs.
                        bib.mPriOffset.Offset(SpriteStructureTDRA.PriOffsetAddBase);
                    }
                    bibs.Add(bib);
                }
            }
        }

        public override void draw(TheaterTDRA theater, IndexedImage image)
        {
            //Smudges aren't drawn if frame index is too high. Can happen if the weird progression field
            //in INI-files are set too high. Checked in game.
            //Probably true for all sprites, but only smudges can have a too high frame index normally?

            //Don't draw if frame index is too high or it's a rebuilt base structure bib (covered by normal structure bib).
            if (mFrameIndex < FrameCount && !IsRebuiltBaseStructBib)
            {
                base.draw(theater, image);
            }
        }
    }
}
