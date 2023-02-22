using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Flags used to specify what some graphics a Red Alert theater can use.
    enum TheaterFlagsRA
    {
        Interior = 1 << 1, //Interior, Temperate and Snow defined in source code.
        Snow = 1 << 2,
        Temperate = 1 << 3,

        SnowTemperate = Snow | Temperate,
        AllDefined = Interior | Snow | Temperate,
        Unknown = AllDefined, //Allow unknown theater to use everything other theaters can.
    }

    class TheaterRA : TheaterTDRA
    {
        private static readonly Dictionary<string, TheaterRA> mTheaters = new Dictionary<string, TheaterRA>();

        //"MAIN.MIX\CONQUER.MIX" has additional graphics shared between theaters.
        private static FileMixArchiveWw mFileMixSharedGfx1 = null;

        //"REDALERT.MIX\HIRES.MIX" has additional graphics shared between theaters.
        //HiRes has big infantry, LoRes has small dito.
        private static FileMixArchiveWw mFileMixSharedGfx2 = null;

        //"EXPAND.MIX" (counterstrike?) and "EXPAND2.MIX" (counterstrike+aftermath?) has additional graphics
        //shared between theaters. These are only present if expansions (counterstrike & aftermath) are installed?
        private static FileMixArchiveWw mFileMixSharedGfx3 = null;

        //"HIRES1.MIX" and "LORES1.MIX" has additional graphics shared between theaters.
        //HiRes has big infantry, LoRes has small dito. These are only present if aftermath expansion is installed?
        private static FileMixArchiveWw mFileMixSharedGfx4 = null;

        private readonly TheaterFlagsRA mFlags;
        private byte[] mMineRemap = null; //Color remap for shaded mines (MINP and MINV).

        private TheaterRA(string name, FileMixArchiveWw fileMix, PaletteTDRA filePalette, PaletteTDRA gamePalette, TheaterFlagsRA flags)
            : base(name, fileMix, filePalette, gamePalette, new MapInfoDrawerRA(gamePalette))
        {
            mFlags = flags;
        }

        private static TheaterRA create(string name)
        {
            FileMixArchiveWw fileMix;
            TheaterFlagsRA flags;
            string fileId = name.Substring(0, Math.Min(name.Length, 8)).ToUpperInvariant();
            switch (fileId)
            {
                case "INTERIOR": fileMix = GameRA.FileMixInterior.get(); flags = TheaterFlagsRA.Interior; break;
                case "SNOW": fileMix = GameRA.FileMixSnow.get(); flags = TheaterFlagsRA.Snow; break;
                case "TEMPERAT": fileMix = GameRA.FileMixTemperat.get(); flags = TheaterFlagsRA.Temperate; break;
                default: //Unknown theater. Search its MIX-file in game folder and "MAIN.MIX".
                    fileMix = new FileMixSearch(fileId + ".MIX", GameRA.FileMixMain).get(); flags = TheaterFlagsRA.Unknown; break;
            }
            string filePalName = fileId + ".PAL";
            //Red Alert theater palettes are usually(?) inside "REDALERT.MIX\LOCAL.MIX", but search game folder first.
            FilePalPalette6Bit filePal = new FileSearch<FilePalPalette6Bit>(filePalName, GameRA.FileMixRedAlertLocal).find();
            if (filePal == null) //If not found, assume it must be in the theater's MIX-file.
            {
                filePal = fileMix.getFileAs<FilePalPalette6Bit>(filePalName);
            }
            PaletteTDRA filePalette = new PaletteRA(filePal.Palette);
            return new TheaterRA(name, fileMix, filePalette, getGamePalette(filePalette), flags);
        }

        public override bool isSpecified(TheaterFlagsRA flags)
        {
            return (mFlags & flags) != 0;
        }

        public static TheaterRA getTheater(string theaterName)
        {
            TheaterRA theater;
            if (!mTheaters.TryGetValue(theaterName, out theater)) //Not cached?
            {
                theater = create(theaterName);
                mTheaters.Add(theaterName, theater);
            }
            return theater;
        }

        protected override byte[][] getUnitShadowFilterInner()
        {
            //Return filter used to make index 4 (green) in sprites into a translucent shadow.
            //Unit shadow filter is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
            byte fraction = (byte)(Id == "SNOW" ? 75 : 130); //Snow theater uses a bit lighter shadow.
            return createUnitShadowFilter(FilePalette.createRemapTableConquer(12, fraction)); //Checked in source.
        }

        protected override byte[] getBrightenRemapInner()
        {
            //Brighten remap is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
            byte[] brightenRemap = FilePalette.createRemapTableFull(15, 25); //Checked in source.
            brightenRemap[0] = 0; //Fix transparent black.
            return brightenRemap;
        }

        protected override byte[] getGreenRemapInner()
        {
            //Green remap is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
            return FilePalette.createRemapTable(3, 110); //Checked in source.
        }

        protected override byte[] getYellowRemapInner()
        {
            //Yellow remap is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
            return FilePalette.createRemapTable(5, 140); //Checked in source.
        }

        public byte[] getMineRemap()
        {
            if (mMineRemap == null)
            {
                //Same as the remap block in the unit shadow filter, but green (index 4) isn't remapped.
                mMineRemap = UnitShadowFilter[1].takeBytes(); //Make a copy.
                mMineRemap[4] = 4; //Don't remap green.
            }
            return mMineRemap;
        }

        protected override FileIcnTileSetTDRA getTileSetInner(UInt16 tileSetId)
        {
            string fileId;
            TheaterFlagsRA theaterFlags;
            TileSetRA.get(tileSetId, out fileId, out theaterFlags);

            string tileSetFileName = fileId + "." + GfxFileExt;
            if (!isSpecified(theaterFlags)) //Tile set isn't specified for theater?
            {
                Program.warn(string.Format("Tile set '{0}' isn't specified for theater '{1}'!", tileSetFileName, Name));
                if (!GameRA.Config.AddUnspecifiedTheaterGraphics) //Don't add unspecified graphics?
                {
                    return FileIcnTileSetRA.createDummy(tileSetFileName, FileBase.Origin.Other);
                    //Theater unspecified tile sets aren't drawn so return a transparent dummy tile.
                }
            }

            FileProto file = FileMix.findFile(tileSetFileName);
            if (file == null)
            {
                file = findFileInSharedGfx(tileSetFileName);
                if (file == null)
                {
                    Program.warn(string.Format("Tile set '{0}' isn't present in theater '{1}'!", tileSetFileName, Name));
                    return FileIcnTileSetRA.createDummy(tileSetFileName, FileBase.Origin.Missing);
                    //Game seems to just ignore (draw nothing) files not found so return a transparent dummy tile.

                    //This is true for both Tiberian Dawn and Red Alert. Checked in game.
                    //Quoting Vladan Bato "http://xhp.xwis.net/documents/cncmap1f.txt"
                    //"There are many templates that exist only in one theater and will show
                    //as black holes in the others (causing the Hall-Of-Mirrors effect)."
                }
            }
            return new FileIcnTileSetRA(file);
        }

        protected override FileShpSpriteSetTDRA getSpriteSetInner(string spriteSetId)
        {
            string spriteSetFileName = spriteSetId + "." + GfxFileExt;
            FileProto file = FileMix.findFile(spriteSetFileName);
            if (file == null) //Not present? Look in shared graphics MIX-file instead.
            {
                file = findFileInSharedGfx(spriteSetId + ".SHP");
                if (file == null)
                {
                    Program.warn(string.Format("Sprite set '{0}' isn't present in theater '{1}'!", spriteSetFileName, Name));
                    return FileShpSpriteSetTDRA.createDummy(spriteSetFileName, FileBase.Origin.Missing);
                    //Game seems to just ignore (draw nothing) files not found so return a transparent dummy sprite.
                }
            }
            return new FileShpSpriteSetTDRA(file);
        }

        private static FileProto findFileInSharedGfx(string fileName)
        {
            FileProto file = null;
            file = getSharedGfx1().findFile(fileName);
            if (file == null)
            {
                file = getSharedGfx2().findFile(fileName);
                if (file == null)
                {
                    file = getSharedGfx3().findFile(fileName);
                    if (file == null)
                    {
                        file = getSharedGfx4().findFile(fileName);
                    }
                }
            }
            return file;
        }

        private static FileMixArchiveWw getSharedGfx1()
        {
            //"MAIN.MIX\CONQUER.MIX" has additional graphics shared between theaters.
            if (mFileMixSharedGfx1 == null)
            {
                mFileMixSharedGfx1 = GameRA.FileMixMainConquer.get();
            }
            return mFileMixSharedGfx1;
        }

        private static FileMixArchiveWw getSharedGfx2()
        {
            //"REDALERT.MIX\HIRES.MIX" has additional graphics shared between theaters.
            //HiRes has big infantry, LoRes has small dito.
            if (mFileMixSharedGfx2 == null)
            {
                mFileMixSharedGfx2 = GameRA.Config.DrawSmallInfantry ? GameRA.FileMixRedAlertLoRes.get() : GameRA.FileMixRedAlertHiRes.get();
            }
            return mFileMixSharedGfx2;
        }

        private static FileMixArchiveWw getSharedGfx3()
        {
            //"EXPAND.MIX" (counterstrike?) and "EXPAND2.MIX" (counterstrike+aftermath?) has additional graphics
            //shared between theaters. These are only present if expansions (counterstrike & aftermath) are installed?
            if (mFileMixSharedGfx3 == null)
            {
                mFileMixSharedGfx3 = GameRA.FileMixExpand2.find(); //Prefer second version if it exists.
                if (mFileMixSharedGfx3 == null)
                {
                    mFileMixSharedGfx3 = GameRA.FileMixExpand.find(); //Try first version next.
                    if (mFileMixSharedGfx3 == null) //MIX-file not found. Use a dummy.
                    {
                        mFileMixSharedGfx3 = FileMixArchiveWw.createDummy("ExpandDummy.mix", FileBase.Origin.Missing);
                    }
                }
            }
            return mFileMixSharedGfx3;
        }

        private static FileMixArchiveWw getSharedGfx4()
        {
            //"HIRES1.MIX" and "LORES1.MIX" has additional graphics shared between theaters.
            //HiRes has big infantry, LoRes has small dito. These are only present if aftermath expansion is installed?
            if (mFileMixSharedGfx4 == null)
            {
                mFileMixSharedGfx4 = GameRA.Config.DrawSmallInfantry ? GameRA.FileMixLoRes1.find() : GameRA.FileMixHiRes1.find();
                if (mFileMixSharedGfx4 == null) //MIX-file not found. Use a dummy.
                {
                    mFileMixSharedGfx4 = FileMixArchiveWw.createDummy("HI1DUMM.MIX", FileBase.Origin.Missing);
                }
            }
            return mFileMixSharedGfx4;
        }

        public void debugSaveTileSets(bool saveAsSheet)
        {
            debugSaveTileSets<FileIcnTileSetRA>(saveAsSheet);
        }

        public void debugSaveTileSetTemplates()
        {
            foreach (FileIcnTileSetRA fileIcn in FileMix.tryFilesAs<FileIcnTileSetRA>(GfxFileExt))
            {
                fileIcn.debugSaveIcnTemplate(Name, FilePalette, GfxSheetBackgroundIndex);
            }
        }

        public void debugSaveSpriteSetsShared(bool saveAsSheet)
        {
            FileMixArchiveWw[] shared = new FileMixArchiveWw[] { getSharedGfx1(), getSharedGfx2(), getSharedGfx3(), getSharedGfx4() };
            for (int i = 0; i < shared.Length; i++)
            {
                string folderName = Name + " shared" + (i + 1);
                foreach (FileShpSpriteSetTDRA fileShp in shared[i].tryFilesAs<FileShpSpriteSetTDRA>(GfxFileExt))
                {
                    if (saveAsSheet)
                    {
                        fileShp.debugSaveShpSheet(folderName, FilePalette, GfxSheetBackgroundIndex);
                    }
                    else
                    {
                        fileShp.debugSaveShpImages(folderName, FilePalette);
                    }
                }
            }
        }

        public void debugSaveColorRemaps()
        {
            List<KeyValuePair<string, byte[]>> remaps = new List<KeyValuePair<string, byte[]>>();
            remaps.Add(new KeyValuePair<string, byte[]>("MineRemap", getMineRemap()));
            debugSaveColorRemaps(this, remaps);
        }

        public static void debugSaveAll(bool saveGfxAsSheet)
        {
            foreach (string theaterName in new string[] { "INTERIOR", "SNOW", "TEMPERATE" })
            {
                Console.WriteLine("theater.debugSaveAll() for " + theaterName);
                TheaterRA theater = getTheater(theaterName);

                Console.WriteLine("theater.FileMix.debugSaveMixFileEntries()");
                theater.FileMix.debugSaveMixFileEntries();

                Console.WriteLine("theater.debugSaveTileSets()");
                theater.debugSaveTileSets(saveGfxAsSheet);

                Console.WriteLine("theater.debugSaveTileSetTemplates()");
                theater.debugSaveTileSetTemplates();

                Console.WriteLine("theater.debugSaveSpriteSets()");
                theater.debugSaveSpriteSets(saveGfxAsSheet);

                if (theaterName == "TEMPERATE") //Save shared once.
                {
                    Console.WriteLine("theater.debugSaveSpriteSetsShared()");
                    theater.debugSaveSpriteSetsShared(saveGfxAsSheet);
                }

                Console.WriteLine("theater.debugSaveColorRemapsMrf()");
                theater.debugSaveColorRemapsMrf();

                Console.WriteLine("theater.debugSaveColorRemaps()");
                theater.debugSaveColorRemaps();

                Console.WriteLine("theater.FilePal.debugSavePalette()");
                theater.FilePalette.debugSavePalette(theaterName);

                Console.WriteLine("theater.FilePal.debugSavePalette() adjusted");
                theater.GamePalette.debugSavePalette(theaterName + " adjusted");

                Console.WriteLine();
            }
        }
    }
}
