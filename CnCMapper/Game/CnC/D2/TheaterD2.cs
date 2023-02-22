using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    enum UnitsShpFile //Units SHP-files in "DUNE.PAK".
    {
        Nr0 = 0, //"UNITS.SHP".
        Nr1 = 1, //"UNITS1.SHP".
        Nr2 = 2, //"UNITS2.SHP".
    }

    //Dune 2 doesn't have theaters like Tiberian Dawn and Red Alert, but I liked that concept so
    //let's pretend there is a "DESERT" theater in Dune 2. This manages graphics.
    class TheaterD2 : TheaterCnC
    {
        private static TheaterD2 mDesert = null;

        private readonly FileMapTileSetsD2 mFileMap;
        private readonly FileIcnTilesD2 mFileIcn;
        private readonly FileShpSpriteSetD2 mFileShpUnits;
        private readonly FileShpSpriteSetD2 mFileShpUnits1;
        private readonly FileShpSpriteSetD2 mFileShpUnits2;

        private readonly PaletteD2 mFilePalette; //Palette as stored in file.
        private readonly PaletteD2 mGamePalette; //Palette after some adjustments.
        private readonly MapInfoDrawerD2 mMapInfoDrawer; //Stored here as it is bound to the theater's game palette.

        private byte[] mAircraftShadowRemap;
        private byte[] mOutsideRemap; //Custom color remap for pixels outside a map's borders.

        private TheaterD2(string name, Palette6Bit palette, FileMapTileSetsD2 fileMap, FileIcnTilesD2 fileIcn,
            FileShpSpriteSetD2 fileShpUnits, FileShpSpriteSetD2 fileShpUnits1, FileShpSpriteSetD2 fileShpUnits2)
            : base(name)
        {
            mFileMap = fileMap;
            mFileIcn = fileIcn;
            mFileShpUnits = fileShpUnits;
            mFileShpUnits1 = fileShpUnits1;
            mFileShpUnits2 = fileShpUnits2;

            mFilePalette = new PaletteD2(palette);
            mGamePalette = PaletteD2.getCopyWithColorCycling(palette);
            mMapInfoDrawer = new MapInfoDrawerD2(mGamePalette);

            //Init later if/when requested instead.
            mAircraftShadowRemap = null;
            mOutsideRemap = null;
        }

        private static TheaterD2 createDesert()
        {
            FilePakArchiveWw filePakDune = GameD2.FilePakDune.get();
            Palette6Bit palette = filePakDune.getFileAs<FilePalPalette6Bit>("IBM.PAL").Palette;
            FileMapTileSetsD2 fileMap = filePakDune.getFileAs<FileMapTileSetsD2>("ICON.MAP");
            FileIcnTilesD2 fileIcn = filePakDune.getFileAs<FileIcnTilesD2>("ICON.ICN");
            FileShpSpriteSetD2 fileShpUnits = filePakDune.getFileAs<FileShpSpriteSetD2>("UNITS.SHP");
            FileShpSpriteSetD2 fileShpUnits1 = filePakDune.getFileAs<FileShpSpriteSetD2>("UNITS1.SHP");
            FileShpSpriteSetD2 fileShpUnits2 = filePakDune.getFileAs<FileShpSpriteSetD2>("UNITS2.SHP");
            return new TheaterD2("DESERT", palette, fileMap, fileIcn, fileShpUnits, fileShpUnits1, fileShpUnits2);
        }

        public static TheaterD2 Desert
        {
            get
            {
                if (mDesert == null)
                {
                    mDesert = createDesert();
                }
                return mDesert;
            }
        }

        public PaletteD2 FilePalette
        {
            get { return mFilePalette; }
        }

        public PaletteD2 GamePalette //Use this only when saving final image of map/radar.
        {
            get { return mGamePalette; }
        }

        public override PaletteCnC getFilePaletteCnC()
        {
            return FilePalette;
        }

        public override PaletteCnC getGamePaletteCnC()
        {
            return GamePalette;
        }

        public MapInfoDrawerD2 MapInfoDrawer
        {
            get { return mMapInfoDrawer; }
        }

        public UInt16 getTileIndex(int tileSetId, int tileSetIndex) //Returns index to tile in ICN-file.
        {
            return mFileMap.getTileIndex(tileSetId, tileSetIndex);
        }

        public LandTypeD2 getLandType(int tileSetId, int tileSetIndex)
        {
            return getLandType(tileSetId, tileSetIndex, false);
        }

        public LandTypeD2 getLandType(int tileSetId, int tileSetIndex, bool hasStructure)
        {
            return LandTypeD2.get(tileSetId, tileSetIndex, mFileMap, hasStructure);
        }

        public Frame getTileFrame(int tileSetId, int tileSetIndex)
        {
            return mFileIcn.getTileRemapped(getTileIndex(tileSetId, tileSetIndex));
        }

        public Frame getUnitFrame(UnitsShpFile unitsShp, int frameIndex)
        {
            return getUnitShpFile(unitsShp).getFrameRemapped(frameIndex);
        }

        public bool isUnitFrameRemapped(UnitsShpFile unitsShp, int frameIndex)
        {
            return getUnitShpFile(unitsShp).isFrameRemapped(frameIndex);
        }

        private FileShpSpriteSetD2 getUnitShpFile(UnitsShpFile unitsShp)
        {
            if (unitsShp == UnitsShpFile.Nr0) return mFileShpUnits;
            if (unitsShp == UnitsShpFile.Nr1) return mFileShpUnits1;
            if (unitsShp == UnitsShpFile.Nr2) return mFileShpUnits2;
            throw new ArgumentException("Only 0-2 are valid units SHP-file numbers!");
        }

        public byte[] getOutsideRemap()
        {
            //Custom color remap to darken pixels outside a map's borders.
            if (mOutsideRemap == null)
            {
                mOutsideRemap = mFilePalette.createRemapTable(12, 144);
            }
            return mOutsideRemap;
        }

        public byte[] getAircraftShadowFilter()
        {
            if (mAircraftShadowRemap == null)
            {
                //Dune 2 seems to calculate this remap table from the palette i.e. it is
                //not using a constant (pre-calculated) table. Changing the "IBM.PAL"
                //palette will also change the remap table. Checked in game.

                //According to tests in the game and OpenDUNE the Dune 2's create remap table
                //function is very similar to Tiberian Dawn's except it does matches same
                //as the target index a bit different.

                //The parameters were found by matching a table captured from the game
                //using a test. These values are used by OpenDUNE too.
                //https://github.com/OpenDUNE/OpenDUNE/blob/master/src/opendune.c#L961
                mAircraftShadowRemap = mFilePalette.createRemapTable(12, 85);

                //Index 223, 239 and 255 (0xDF,0xEF,0xFF) are not remapped.
                mAircraftShadowRemap[0xDF] = 0xDF;
                mAircraftShadowRemap[0xEF] = 0xEF;
                mAircraftShadowRemap[0xFF] = 0xFF;
            }
            return mAircraftShadowRemap;
        }

        public void debugSaveTileSets(bool saveAsSheet)
        {
            if (saveAsSheet)
            {
                mFileIcn.debugSaveIcnSheet(Name, mFilePalette, GfxSheetBackgroundIndex, true);
            }
            else
            {
                mFileIcn.debugSaveIcnImages(Name, mFilePalette, true);
            }
        }

        public void debugSaveTileSetTemplates()
        {
            mFileMap.debugSaveTileSetsImages(Name, mFileIcn, mFilePalette);
        }

        public void debugSaveSpriteSets(bool saveAsSheet)
        {
            foreach (FileShpSpriteSetD2 fileShp in new FileShpSpriteSetD2[] { mFileShpUnits, mFileShpUnits1, mFileShpUnits2 })
            {
                if (saveAsSheet)
                {
                    fileShp.debugSaveShpSheet(Name, mFilePalette, GfxSheetBackgroundIndex, true);
                }
                else
                {
                    fileShp.debugSaveShpImages(Name, mFilePalette, true);
                }
            }
        }

        public void debugSaveColorRemaps()
        {
            List<KeyValuePair<string, byte[]>> remaps = new List<KeyValuePair<string, byte[]>>();
            remaps.Add(new KeyValuePair<string, byte[]>("AirShadowRemap", getAircraftShadowFilter()));
            remaps.Add(new KeyValuePair<string, byte[]>("OutsideRemap", getOutsideRemap()));
            debugSaveColorRemaps(this, remaps);
        }

        public static void debugSaveAll(bool saveGfxAsSheet)
        {
            foreach (TheaterD2 theater in new TheaterD2[] { Desert })
            {
                string theaterName = theater.Name;
                Console.WriteLine("theater.debugSaveAll() for " + theaterName);

                Console.WriteLine("theater.debugSaveTileSets()");
                theater.debugSaveTileSets(saveGfxAsSheet);

                Console.WriteLine("theater.debugSaveTileSetTemplates()");
                theater.debugSaveTileSetTemplates();

                Console.WriteLine("theater.debugSaveSpriteSets()");
                theater.debugSaveSpriteSets(saveGfxAsSheet);

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
