using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    abstract class TheaterTDRA : TheaterCnC
    {
        private readonly FileMixArchiveWw mFileMix; //Theater MIX-file.
        private readonly Dictionary<UInt16, FileIcnTileSetTDRA> mTileSetCache;
        private readonly Dictionary<string, FileShpSpriteSetTDRA> mSpriteSetCache;
        private readonly PaletteTDRA mFilePalette; //Palette as stored in file before settings adjustment.
        private readonly PaletteTDRA mGamePalette; //Palette after settings adjustment.
        private readonly MapInfoDrawerTDRA mMapInfoDrawer; //Stored here as it is bound to the theater's game palette.
        private readonly byte[][] mUnitShadowFilter; //Filters green (index == 4) to a darker version of underlying pixel.
        private byte[][] mAircraftShadowFilter; //Filters non-transparent (index != 0) to a darker version of underlying pixel.
        private byte[] mBrightenRemap; //Color remaps.
        private byte[] mGreenRemap;
        private byte[] mYellowRemap;
        private byte[] mOutsideRemap; //Custom color remap for pixels outside a map's borders.

        protected TheaterTDRA(string name, FileMixArchiveWw fileMix, PaletteTDRA filePalette, PaletteTDRA gamePalette,
            MapInfoDrawerTDRA mapInfoDrawer)
            : base(name)
        {
            mFileMix = fileMix;
            mTileSetCache = new Dictionary<UInt16, FileIcnTileSetTDRA>();
            mSpriteSetCache = new Dictionary<string, FileShpSpriteSetTDRA>();
            mFilePalette = filePalette;
            mGamePalette = gamePalette;
            mMapInfoDrawer = mapInfoDrawer;

            mUnitShadowFilter = getUnitShadowFilterInner();

            //Init later if/when requested instead.
            mAircraftShadowFilter = null;
            mBrightenRemap = null;
            mGreenRemap = null;
            mYellowRemap = null;
            mOutsideRemap = null;
        }

        protected static PaletteTDRA getGamePalette(PaletteTDRA filePalette)
        {
            //Tiberian Dawn and Red Alert uses "Adjust_Palette()" in "OPTIONS.CPP" to
            //get game palette from visual settings (brightness, color, contrast, tint).
            //This method will slightly alter the stored palette even at default settings which
            //will cause screenshots from the game to not exactly match saved images. The difference
            //is small and not really noticeable though so I'm not sure if I want to mimic this?

            //TODO: Do or don't mimic game palette discrepancy caused by "Adjust_Palette()"? Add config option?
            //Use adjusted palette in debug for now to make it easier to compare tests with screenshots from the game.
#if DEBUG
            return filePalette.getAdjusted(); //Mimic discrepancy.
#else
            return (PaletteTDRA)filePalette.getCopy(); //Ignore discrepancy and copy file palette instead.
#endif
        }

        protected abstract FileIcnTileSetTDRA getTileSetInner(UInt16 tileSetId);
        protected abstract FileShpSpriteSetTDRA getSpriteSetInner(string spriteSetId);
        protected abstract byte[][] getUnitShadowFilterInner();
        protected abstract byte[] getBrightenRemapInner();
        protected abstract byte[] getGreenRemapInner();
        protected abstract byte[] getYellowRemapInner();

        public string Id //Same as MIX-file id. Always uppercase and better to use than name to identify a theater.
        {
            get { return FileMix.Id; }
        }

        public string GfxFileExt //Theater graphic files use first three letters of MIX-file's name as extension.
        {
            get { return FileMix.GfxFileExt; }
        }

        public FileMixArchiveWw FileMix
        {
            get { return mFileMix; }
        }

        public PaletteTDRA FilePalette
        {
            get { return mFilePalette; }
        }

        public PaletteTDRA GamePalette //Use this only when saving final image of map/radar.
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

        public byte[][] UnitShadowFilter
        {
            get { return mUnitShadowFilter; }
        }

        public MapInfoDrawerTDRA MapInfoDrawer
        {
            get { return mMapInfoDrawer; }
        }

        public virtual bool isSpecified(TD.TheaterFlagsTD flags)
        {
            return false;
        }

        public virtual bool isSpecified(RA.TheaterFlagsRA flags)
        {
            return false;
        }

        public FileIcnTileSetTDRA getTileSet(UInt16 tileSetId) //Returns a dummy tile set if not present or defined in this theater.
        {
            FileIcnTileSetTDRA fileIcn;
            if (!mTileSetCache.TryGetValue(tileSetId, out fileIcn)) //Not cached?
            {
                fileIcn = getTileSetInner(tileSetId);
                mTileSetCache.Add(tileSetId, fileIcn);
            }
            return fileIcn;
        }

        public FileShpSpriteSetTDRA getSpriteSet(string spriteSetId) //Returns a dummy sprite set if not present in this theater or shared graphics MIX-file.
        {
            FileShpSpriteSetTDRA fileShp;
            if (!mSpriteSetCache.TryGetValue(spriteSetId, out fileShp)) //Not cached?
            {
                fileShp = getSpriteSetInner(spriteSetId);
                mSpriteSetCache.Add(spriteSetId, fileShp);
            }
            return fileShp;
        }

        public byte[][] getAircraftShadowFilter()
        {
            //Aircraft shadows use a special filter that is a bit lighter than unit shadows. And all indices are
            //filtered, not just index 4, i.e. the whole non-transparent part of the sprite will become a shadow.
            if (mAircraftShadowFilter == null)
            {
                //Aircraft shadow remap is always calculated. See "Init_Theater()" in "DISPLAY.CPP".
                //A conditional compile switch "_RETRIEVE" determines palette to use (file- vs remap-palette).
                //If "_RETRIEVE" then most tables are loaded from MRF-files and cycling colors are not disabled.
                //Else they are all calculated from a palette with disabled cycling colors.
                //Normally(?) "_RETRIEVE" is defined so create the aircraft shadow remap from the file palette.

                //In Red Alert the "_RETRIEVE"-switch is removed and remaps are created from the file palette.

                mAircraftShadowFilter = createFilter(2, 0); //Use first block (0 == block 1) for all palette indices.
                mAircraftShadowFilter[1] = FilePalette.createRemapTableConquer(12, 100); //Checked in source Tiberian Dawn and Red Alert.
            }
            return mAircraftShadowFilter;
        }

        public byte[] getBrightenRemap()
        {
            if (mBrightenRemap == null)
            {
                mBrightenRemap = getBrightenRemapInner();
            }
            return mBrightenRemap;
        }

        public byte[] getGreenRemap()
        {
            if (mGreenRemap == null)
            {
                mGreenRemap = getGreenRemapInner();
            }
            return mGreenRemap;
        }

        public byte[] getYellowRemap()
        {
            if (mYellowRemap == null)
            {
                mYellowRemap = getYellowRemapInner();
            }
            return mYellowRemap;
        }

        public byte[] getOutsideRemap()
        {
            //Custom color remap to darken pixels outside a map's borders.
            if (mOutsideRemap == null)
            {
                mOutsideRemap = FilePalette.createRemapTable(12, 144);
            }
            return mOutsideRemap;
        }

        protected static byte[][] createUnitShadowFilter(byte[] remapBlock)
        {
            //Helper function for creating a unit shadow filter.
            byte[][] unitShadowFilter = createFilter(2, 0xFF); //Don't filter any palette indices...
            unitShadowFilter[0][4] = 0; //...except index 4 (green).
            unitShadowFilter[1] = remapBlock; //Remap block for index 4.
            return unitShadowFilter;
        }

        private static byte[][] createFilter(int blockCount, byte clearValue)
        {
            //Helper function for creating filters.
            byte[][] filter = new byte[blockCount][];
            filter[0] = new byte[256]; //Create first block...
            filter[0].clearBytes(clearValue); //...and clear it.
            return filter;
        }

        protected void debugSaveTileSets<T>(bool saveAsSheet) where T : FileIcnTileSetTDRA, new()
        {
            foreach (T fileIcn in mFileMix.tryFilesAs<T>(GfxFileExt))
            {
                if (saveAsSheet)
                {
                    fileIcn.debugSaveIcnSheet(Name, FilePalette, GfxSheetBackgroundIndex);
                }
                else
                {
                    fileIcn.debugSaveIcnImages(Name, FilePalette);
                }
            }
        }

        public void debugSaveSpriteSets(bool saveAsSheet)
        {
            foreach (FileShpSpriteSetTDRA fileShp in mFileMix.tryFilesAs<FileShpSpriteSetTDRA>(GfxFileExt))
            {
                if (saveAsSheet)
                {
                    fileShp.debugSaveShpSheet(Name, FilePalette, GfxSheetBackgroundIndex);
                }
                else
                {
                    fileShp.debugSaveShpImages(Name, FilePalette);
                }
            }
        }

        public void debugSaveColorRemapsMrf()
        {
            foreach (FileMrfFadingTableWw fileMrf in mFileMix.tryFilesAs<FileMrfFadingTableWw>())
            {
                fileMrf.debugSaveRemapBlocks();
            }
        }

        protected static void debugSaveColorRemaps(TheaterTDRA theater, List<KeyValuePair<string, byte[]>> remaps)
        {
            remaps.Add(new KeyValuePair<string, byte[]>("UnitShadowRemap", theater.UnitShadowFilter[1]));
            remaps.Add(new KeyValuePair<string, byte[]>("AirShadowRemap", theater.getAircraftShadowFilter()[1]));
            remaps.Add(new KeyValuePair<string, byte[]>("BrightenRemap", theater.getBrightenRemap()));
            remaps.Add(new KeyValuePair<string, byte[]>("GreenRemap", theater.getGreenRemap()));
            remaps.Add(new KeyValuePair<string, byte[]>("YellowRemap", theater.getYellowRemap()));
            remaps.Add(new KeyValuePair<string, byte[]>("OutsideRemap", theater.getOutsideRemap()));
            debugSaveColorRemaps(theater, remaps);
        }
    }
}
