using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Flags used to specify what some graphics a Tiberian Dawn theater can use.
    enum TheaterFlagsTD
    {
        Desert = 1 << 1, //Desert, Jungle, Temperate and Winter defined in source code. Jungle is rarely used though.
        Jungle = 1 << 2,
        Temperate = 1 << 3,
        Winter = 1 << 4,

        DesertTemperateWinter = Desert | Temperate | Winter,
        DesertTemperate = Desert | Temperate,
        TemperateWinter = Temperate | Winter,
        AllDefined = Desert | Jungle | Temperate | Winter,
        Unknown = AllDefined, //Allow unknown theater to use everything other theaters can.
    }

    class TheaterTD : TheaterTDRA
    {
        private static readonly Dictionary<string, TheaterTD> mTheaters = new Dictionary<string, TheaterTD>();

        //"CONQUER.MIX" has additional graphics shared between theaters.
        private static FileMixArchiveWw mFileMixSharedGfx = null;

        private readonly TheaterFlagsTD mFlags;
        private PaletteTD mRemapPalette = null; //Palette without water color cycling used to calculate remaps from.

        private TheaterTD(string name, FileMixArchiveWw fileMix, PaletteTDRA filePalette, PaletteTDRA gamePalette, TheaterFlagsTD flags)
            : base(name, fileMix, filePalette, gamePalette, new MapInfoDrawerTD(gamePalette))
        {
            mFlags = flags;
        }

        private static TheaterTD create(string name)
        {
            FileMixArchiveWw fileMix;
            TheaterFlagsTD flags;
            string fileId = name.Substring(0, Math.Min(name.Length, 8)).ToUpperInvariant();
            switch (fileId)
            {
                case "DESERT": fileMix = GameTD.FileMixDesert.get(); flags = TheaterFlagsTD.Desert; break;
                case "JUNGLE": fileMix = GameTD.FileMixJungle.get(); flags = TheaterFlagsTD.Jungle; break;
                case "TEMPERAT": fileMix = GameTD.FileMixTemperat.get(); flags = TheaterFlagsTD.Temperate; break;
                case "WINTER": fileMix = GameTD.FileMixWinter.get(); flags = TheaterFlagsTD.Winter; break;
                default: //Unknown theater. Search its MIX-file in game folder.
                    fileMix = new FileMixSearch(fileId + ".MIX").get(); flags = TheaterFlagsTD.Unknown; break;
            }
            //Tiberian Dawn theater palettes are always(?) inside their MIX-file.
            FilePalPalette6Bit filePal = fileMix.getFileAs<FilePalPalette6Bit>(fileId + ".PAL");
            PaletteTDRA filePalette = new PaletteTD(filePal.Palette);
            return new TheaterTD(name, fileMix, filePalette, getGamePalette(filePalette), flags);
        }

        public override bool isSpecified(TheaterFlagsTD flags)
        {
            return (mFlags & flags) != 0;
        }

        public static TheaterTD getTheater(string theaterName)
        {
            TheaterTD theater;
            if (!mTheaters.TryGetValue(theaterName, out theater)) //Not cached?
            {
                theater = create(theaterName);
                mTheaters.Add(theaterName, theater);
            }
            return theater;
        }

        private PaletteTD getRemapPalette()
        {
            if (mRemapPalette == null)
            {
                mRemapPalette = PaletteTD.getCopyWithoutColorCycling(FilePalette);
            }
            return mRemapPalette;
        }

        protected override byte[][] getUnitShadowFilterInner()
        {
            //Return filter used to make index 4 (green) in sprites into a translucent shadow.
            byte[][] unitShadowFilter;
            //Load remap from file.
            FileProto file = FileMix.findFile(Id[0] + "UNITS.MRF");
            if (file != null)
            {
                unitShadowFilter = new FileMrfFadingTableWw(file).getRemapBlocks();
            }
            else
            {
                //File not found. Let's generate the remap instead. Same result as ?UNITS.MRF for all theaters.
                //See "Init_Theater()" in "DISPLAY.CPP".
                unitShadowFilter = createUnitShadowFilter(getRemapPalette().createRemapTableConquer(12, 130)); //Checked in source.
            }
            return unitShadowFilter;
        }

        protected override byte[] getBrightenRemapInner()
        {
            //Brighten remap is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
            //A conditional compile switch "_RETRIEVE" determines palette to use (file- vs remap-palette).
            //If "_RETRIEVE" then most tables are loaded from MRF-files and cycling colors are not disabled.
            //Else they are all calculated from a palette with disabled cycling colors.
            //Normally(?) "_RETRIEVE" is defined so create the brighten remap from the file palette.
            return FilePalette.createRemapTable(15, 25); //Checked in source.
        }

        protected override byte[] getGreenRemapInner()
        {
            byte[] greenRemap;
            //Load remap from file.
            FileProto file = FileMix.findFile(Id[0] + "GREEN.MRF");
            if (file != null)
            {
                greenRemap = new FileMrfFadingTableWw(file).getRemapBlocks()[0];
            }
            else
            {
                //File not found. Let's generate the remap instead. Same result as ?GREEN.MRF for all theaters.
                //See "Init_Theater()" in "DISPLAY.CPP".
                greenRemap = getRemapPalette().createRemapTable(3, 110); //Checked in source.
            }

            if (Id == "DESERT") //Done regardless if remap is from file or generated. Checked in source.
            {
                greenRemap[196] = 160;
            }
            return greenRemap;
        }

        protected override byte[] getYellowRemapInner()
        {
            byte[] yellowRemap;
            //Load remap from file.
            FileProto file = FileMix.findFile(Id[0] + "YELLOW.MRF");
            if (file != null)
            {
                yellowRemap = new FileMrfFadingTableWw(file).getRemapBlocks()[0];
            }
            else
            {
                //File not found. Let's generate the remap instead. Same result as ?YELLOW.MRF for all theaters.
                //See "Init_Theater()" in "DISPLAY.CPP".
                yellowRemap = getRemapPalette().createRemapTable(5, 140); //Checked in source.
            }
            return yellowRemap;
        }

        protected override FileIcnTileSetTDRA getTileSetInner(UInt16 tileSetId)
        {
            TileSetTD tileSet = TileSetTD.get((byte)tileSetId);
            string tileSetFileName = tileSet.FileId + "." + GfxFileExt;
            if (!isSpecified(tileSet.TheaterFlags)) //Tile set isn't specified for theater?
            {
                Program.warn(string.Format("Tile set '{0}' isn't specified for theater '{1}'!", tileSetFileName, Name));
                if (!GameTD.Config.AddUnspecifiedTheaterGraphics) //Don't add unspecified graphics?
                {
                    return FileIcnTileSetTD.createDummy(tileSetFileName, FileBase.Origin.Other);
                    //Theater unspecified tile sets aren't drawn so return a transparent dummy tile.
                }
            }

            FileProto file = FileMix.findFile(tileSetFileName);
            if (file == null)
            {
                Program.warn(string.Format("Tile set '{0}' isn't present in theater '{1}'!", tileSetFileName, Name));
                return FileIcnTileSetTD.createDummy(tileSetFileName, FileBase.Origin.Missing);
                //Game seems to just ignore (draw nothing) files not found so return a transparent dummy tile.

                //This is true for both Tiberian Dawn and Red Alert. Checked in game.
                //Quoting Vladan Bato "http://xhp.xwis.net/documents/cncmap1f.txt"
                //"There are many templates that exist only in one theater and will show
                //as black holes in the others (causing the Hall-Of-Mirrors effect)."
            }
            return new FileIcnTileSetTD(file);
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
            return getSharedGfx().findFile(fileName);
        }

        private static FileMixArchiveWw getSharedGfx()
        {
            if (mFileMixSharedGfx == null)
            {
                mFileMixSharedGfx = GameTD.FileMixConquer.get();
            }
            return mFileMixSharedGfx;
        }

        public void debugSaveTileSets(bool saveAsSheet)
        {
            debugSaveTileSets<FileIcnTileSetTD>(saveAsSheet);
        }

        public void debugSaveTileSetTemplates()
        {
            foreach (FileIcnTileSetTD fileIcn in FileMix.tryFilesAs<FileIcnTileSetTD>(GfxFileExt))
            {
                TileSetTD.IdPair tsIdPair = TileSetTD.get(fileIcn.Id);
                Size templateSize = tsIdPair != null ? tsIdPair.TileSet.TemplateSize : new Size(1, 1);
                fileIcn.debugSaveIcnTemplate(Name, FilePalette, GfxSheetBackgroundIndex, templateSize);
            }
        }

        public void debugSaveSpriteSetsShared(bool saveAsSheet)
        {
            string folderName = Name + " shared";
            foreach (FileShpSpriteSetTDRA fileShp in getSharedGfx().tryFilesAs<FileShpSpriteSetTDRA>(GfxFileExt))
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

        public void debugSaveColorRemaps()
        {
            debugSaveColorRemaps(this, new List<KeyValuePair<string, byte[]>>());
        }

        public static void debugSaveAll(bool saveGfxAsSheet)
        {
            foreach (string theaterName in new string[] { "DESERT", "TEMPERATE", "WINTER" })
            {
                Console.WriteLine("theater.debugSaveAll() for " + theaterName);
                TheaterTD theater = getTheater(theaterName);

                Console.WriteLine("theater.FileMix.debugSaveMixFileEntries()");
                theater.FileMix.debugSaveMixFileEntries();

                Console.WriteLine("theater.debugSaveTileSets()");
                theater.debugSaveTileSets(saveGfxAsSheet);

                Console.WriteLine("theater.debugSaveTileSetTemplates()");
                theater.debugSaveTileSetTemplates();

                Console.WriteLine("theater.debugSaveSpriteSets()");
                theater.debugSaveSpriteSets(saveGfxAsSheet);

                if (theaterName == "DESERT") //Save shared once.
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
