using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    enum LandType
    {
        Clear,    //"Clear" terrain.
        Road,     //Road terrain.
        Water,    //Water.
        Rock,     //Impassable rock.
        Wall,     //Wall (blocks movement).
        Tiberium, //Tiberium field.
        Beach,    //Beach terrain.
    }

    class TileSetTD
    {
        //Because tile sets in Tiberian Dawn have a lot of data (compared to Red Alert at least) it is
        //probably worth using a pre-allocated static array with all of them instead of a big switch statement.
        //Increases program startup time a bit (nothing noticeable though), but nicer to use and similar
        //speed when rendering many maps.

        //Tile sets are only valid in specific theaters. The specified theaters for tile sets in
        //Tiberian Dawn are a pretty messy combination of DESERT, TEMPERATE and WINTER theater.
        //Nothing is drawn (Hall-Of-Mirrors effect) if not specified for theater. Checked in source.

        //Template size is used to create tile templates. Templates are pretty much only needed
        //by map editors and the [TEMPLATE] section (rarely/never used?) in mission INI-files.

        private readonly string mFileId;
        private readonly TheaterFlagsTD mTheaterFlags; //Specified for use in these theaters only.
        private readonly Size mTemplateSize; //Size of template in tiles. If 1*1 then all tiles are individual.
        private readonly LandType mLandType;
        private readonly LandType mLandTypeAlt; //Alternative land type.
        private readonly byte[] mLandTypeAltTiles; //List of tile set indexes in the template that uses the alternative land type.

        private TileSetTD(string fileId, TheaterFlagsTD theaterFlags, Size templateSize, LandType landType, LandType landTypeAlt, byte[] landTypeAltTiles)
        {
            mFileId = fileId;
            mTheaterFlags = theaterFlags;
            mTemplateSize = templateSize;
            mLandType = landType;
            mLandTypeAlt = landTypeAlt;
            mLandTypeAltTiles = landTypeAltTiles;
        }

        public string FileId
        {
            get { return mFileId; }
        }

        public TheaterFlagsTD TheaterFlags
        {
            get { return mTheaterFlags; }
        }

        public Size TemplateSize
        {
            get { return mTemplateSize; }
        }

        public LandType getLandType(byte tileSetIndex)
        {
            //Seems like tile index isn't checked if too high, only if it's present in the alt tiles list. Checked in source.
            return mLandTypeAltTiles.Contains(tileSetIndex) ? mLandTypeAlt : mLandType; //Indexed tile uses the alternative land type?
        }

        //Some dummy methods used to make constructors easier to read.
        private static string _fileId(string f) { return f; }
        private static TheaterFlagsTD _theaterFlags(TheaterFlagsTD t) { return t; }
        private static Size _templateSize(int width, int height) { return new Size(width, height); }
        private static LandType _landType(LandType l) { return l; }
        private static LandType _landTypeAlt(LandType l) { return l; }
        private static byte[] _landTypeAltTiles(byte[] l) { return l; }

        //Alternative land type template tile lists.
        private static readonly byte[] tiles_none = new byte[] { };
        private static readonly byte[] tiles_0 = new byte[] { 0 };
        private static readonly byte[] tiles_1 = new byte[] { 1 };
        private static readonly byte[] tiles_2 = new byte[] { 2 };
        private static readonly byte[] tiles_3 = new byte[] { 3 };
        private static readonly byte[] tiles_5 = new byte[] { 5 };
        private static readonly byte[] tiles_6 = new byte[] { 6 };
        private static readonly byte[] tiles_7 = new byte[] { 7 };
        private static readonly byte[] tiles_0_3 = new byte[] { 0, 3 };
        private static readonly byte[] tiles_1_22 = new byte[] { 1, 22 };
        private static readonly byte[] tiles_2_3 = new byte[] { 2, 3 };
        private static readonly byte[] tiles_3_5 = new byte[] { 3, 5 };
        private static readonly byte[] tiles_5_8 = new byte[] { 5, 8 };
        private static readonly byte[] tiles_6_7 = new byte[] { 6, 7 };
        private static readonly byte[] tiles_6_8 = new byte[] { 6, 8 };
        private static readonly byte[] tiles_0_1_2 = new byte[] { 0, 1, 2 };
        private static readonly byte[] tiles_0_1_3 = new byte[] { 0, 1, 3 };
        private static readonly byte[] tiles_0_3_6 = new byte[] { 0, 3, 6 };
        private static readonly byte[] tiles_1_2_3 = new byte[] { 1, 2, 3 };
        private static readonly byte[] tiles_2_5_8 = new byte[] { 2, 5, 8 };
        private static readonly byte[] tiles_0_1_2_4 = new byte[] { 0, 1, 2, 4 };
        private static readonly byte[] tiles_1_8_15_22 = new byte[] { 1, 8, 15, 22 };
        private static readonly byte[] tiles_2_3_12_13 = new byte[] { 2, 3, 12, 13 };
        private static readonly byte[] tiles_0_1_3_4_6 = new byte[] { 0, 1, 3, 4, 6 };
        private static readonly byte[] tiles_2_3_4_5_8 = new byte[] { 2, 3, 4, 5, 8 };
        private static readonly byte[] tiles_0_1_2_3_4_5 = new byte[] { 0, 1, 2, 3, 4, 5 };
        private static readonly byte[] tiles_0_1_2_4_7_8 = new byte[] { 0, 1, 2, 4, 7, 8 };
        private static readonly byte[] tiles_0_1_18_19_23_24 = new byte[] { 0, 1, 18, 19, 23, 24 };
        private static readonly byte[] tiles_2_3_6_9_12_13 = new byte[] { 2, 3, 6, 9, 12, 13 };
        private static readonly byte[] tiles_3_4_6_11_19_25 = new byte[] { 3, 4, 6, 11, 19, 25 };
        private static readonly byte[] tiles_0_1_2_3_4_5_6_7 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        private static readonly byte[] tiles_0_1_2_3_4_5_7_8 = new byte[] { 0, 1, 2, 3, 4, 5, 7, 8 };
        private static readonly byte[] tiles_0_1_6_12_18_19_23_24 = new byte[] { 0, 1, 6, 12, 18, 19, 23, 24 };
        private static readonly byte[] tiles_3_4_6_9_11_14_19_24_25 = new byte[] { 3, 4, 6, 9, 11, 14, 19, 24, 25 };

        private static readonly TileSetTD[] mTileSets = new TileSetTD[]
        {
            new TileSetTD( //000: Clear.
                _fileId("CLEAR1"),
                _theaterFlags(TheaterFlagsTD.AllDefined),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //001: Water.
                _fileId("W1"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(1, 1),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //002: Water2.
                _fileId("W2"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //003: Shore1.
                _fileId("SH1"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2_3_4_5)
            ),
            new TileSetTD( //004: Shore2.
                _fileId("SH2"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2)
            ),
            new TileSetTD( //005: Shore3.
                _fileId("SH3"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //006: Shore4.
                _fileId("SH4"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //007: Shore5.
                _fileId("SH5"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2_3_4_5)
            ),
            new TileSetTD( //008: Shore11.
                _fileId("SH11"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_3)
            ),
            new TileSetTD( //009: Shore12.
                _fileId("SH12"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_5_8)
            ),
            new TileSetTD( //010: Shore13.
                _fileId("SH13"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_6)
            ),
            new TileSetTD( //011: Shore14.
                _fileId("SH14"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_6_7)
            ),
            new TileSetTD( //012: Shore15.
                _fileId("SH15"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_6_8)
            ),
            new TileSetTD( //013: Slope1.
                _fileId("S01"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2)
            ),
            new TileSetTD( //014: Slope2.
                _fileId("S02"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //015: Slope3.
                _fileId("S03"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //016: Slope4.
                _fileId("S04"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //017: Slope5.
                _fileId("S05"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //018: Slope6.
                _fileId("S06"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //019: Slope7.
                _fileId("S07"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //020: Slope8.
                _fileId("S08"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //021: Slope9.
                _fileId("S09"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_3)
            ),
            new TileSetTD( //022: Slope10.
                _fileId("S10"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //023: Slope11.
                _fileId("S11"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //024: Slope12.
                _fileId("S12"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //025: Slope13.
                _fileId("S13"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_5)
            ),
            new TileSetTD( //026: Slope14.
                _fileId("S14"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1_2_3)
            ),
            new TileSetTD( //027: Slope15.
                _fileId("S15"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //028: Slope16.
                _fileId("S16"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //029: Slope17.
                _fileId("S17"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //030: Slope18.
                _fileId("S18"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //031: Slope19.
                _fileId("S19"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //032: Slope20.
                _fileId("S20"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_5)
            ),
            new TileSetTD( //033: Slope21.
                _fileId("S21"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(1, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //034: Slope22.
                _fileId("S22"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //035: Slope23.
                _fileId("S23"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_5)
            ),
            new TileSetTD( //036: Slope24.
                _fileId("S24"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //037: Slope25.
                _fileId("S25"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //038: Slope26.
                _fileId("S26"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //039: Slope27.
                _fileId("S27"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2_3)
            ),
            new TileSetTD( //040: Slope28.
                _fileId("S28"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //041: Slope29.
                _fileId("S29"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //042: Slope30.
                _fileId("S30"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //043: Slope31.
                _fileId("S31"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //044: Slope32.
                _fileId("S32"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //045: Slope33.
                _fileId("S33"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //046: Slope34.
                _fileId("S34"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //047: Slope35.
                _fileId("S35"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //048: Slope36.
                _fileId("S36"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //049: Slope37.
                _fileId("S37"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //050: Slope38.
                _fileId("S38"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //051: Shore32.
                _fileId("SH32"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //052: Shore33.
                _fileId("SH33"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_2)
            ),
            new TileSetTD( //053: Shore20.
                _fileId("SH20"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //054: Shore21.
                _fileId("SH21"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //055: Shore22.
                _fileId("SH22"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //056: Shore23.
                _fileId("SH23"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //057: Brush1.
                _fileId("BR1"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //058: Brush2.
                _fileId("BR2"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //059: Brush3.
                _fileId("BR3"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //060: Brush4.
                _fileId("BR4"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //061: Brush5.
                _fileId("BR5"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //062: Brush6.
                _fileId("BR6"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //063: Brush7.
                _fileId("BR7"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //064: Brush8.
                _fileId("BR8"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //065: Brush9.
                _fileId("BR9"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //066: Brush10.
                _fileId("BR10"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //067: Patch1.
                _fileId("P01"),
                _theaterFlags(TheaterFlagsTD.DesertTemperate),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //068: Patch2.
                _fileId("P02"),
                _theaterFlags(TheaterFlagsTD.DesertTemperate),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //069: Patch3.
                _fileId("P03"),
                _theaterFlags(TheaterFlagsTD.DesertTemperate),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //070: Patch4.
                _fileId("P04"),
                _theaterFlags(TheaterFlagsTD.DesertTemperate),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //071: Patch5.
                _fileId("P05"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //072: Patch6.
                _fileId("P06"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 4),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //073: Patch7.
                _fileId("P07"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(4, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //074: Patch8.
                _fileId("P08"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //075: Shore16.
                _fileId("SH16"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //076: Shore17.
                _fileId("SH17"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //077: Shore18.
                _fileId("SH18"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //078: Shore19.
                _fileId("SH19"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //079: Patch13.
                _fileId("P13"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //080: Patch14.
                _fileId("P14"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //081: Patch15.
                _fileId("P15"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //082: Boulder1.
                _fileId("B1"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //083: Boulder2.
                _fileId("B2"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //084: Boulder3.
                _fileId("B3"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //085: Boulder4.
                _fileId("B4"),
                _theaterFlags(TheaterFlagsTD.Temperate),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //086: Boulder5.
                _fileId("B5"),
                _theaterFlags(TheaterFlagsTD.Temperate),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //087: Boulder6.
                _fileId("B6"),
                _theaterFlags(TheaterFlagsTD.Temperate),
                _templateSize(1, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //088: Shore6.
                _fileId("SH6"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2_3_4_5)
            ),
            new TileSetTD( //089: Shore7.
                _fileId("SH7"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //090: Shore8.
                _fileId("SH8"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2_3_4_5_6_7)
            ),
            new TileSetTD( //091: Shore9.
                _fileId("SH9"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_0_1_2_3_4_5_7_8)
            ),
            new TileSetTD( //092: Shore10.
                _fileId("SH10"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Beach),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //093: Road1.
                _fileId("D01"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //094: Road2.
                _fileId("D02"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //095: Road3.
                _fileId("D03"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(1, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //096: Road4.
                _fileId("D04"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //097: Road5.
                _fileId("D05"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 4),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //098: Road6.
                _fileId("D06"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //099: Road7.
                _fileId("D07"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //100: Road8.
                _fileId("D08"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //101: Road9.
                _fileId("D09"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(4, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //102: Road10.
                _fileId("D10"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(4, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //103: Road11.
                _fileId("D11"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //104: Road12.
                _fileId("D12"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //105: Road13.
                _fileId("D13"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(4, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //106: Road14.
                _fileId("D14"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //107: Road15.
                _fileId("D15"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //108: Road16.
                _fileId("D16"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //109: Road17.
                _fileId("D17"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //110: Road18.
                _fileId("D18"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //111: Road19.
                _fileId("D19"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //112: Road20.
                _fileId("D20"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //113: Road21.
                _fileId("D21"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //114: Road22.
                _fileId("D22"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //115: Road23.
                _fileId("D23"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //116: Road24.
                _fileId("D24"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //117: Road25.
                _fileId("D25"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //118: Road26.
                _fileId("D26"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //119: Road27.
                _fileId("D27"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //120: Road28.
                _fileId("D28"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //121: Road29.
                _fileId("D29"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //122: Road30.
                _fileId("D30"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //123: Road31.
                _fileId("D31"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //124: Road32.
                _fileId("D32"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //125: Road33.
                _fileId("D33"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //126: Road34.
                _fileId("D34"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //127: Road35.
                _fileId("D35"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //128: Road36.
                _fileId("D36"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //129: Road37.
                _fileId("D37"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //130: Road38.
                _fileId("D38"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //131: Road39.
                _fileId("D39"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //132: Road40.
                _fileId("D40"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //133: Road41.
                _fileId("D41"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //134: Road42.
                _fileId("D42"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //135: Road43.
                _fileId("D43"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //136: River1.
                _fileId("RV01"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(5, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //137: River2.
                _fileId("RV02"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(5, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //138: River3.
                _fileId("RV03"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_7)
            ),
            new TileSetTD( //139: River4.
                _fileId("RV04"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //140: River5.
                _fileId("RV05"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //141: River6.
                _fileId("RV06"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //142: River7.
                _fileId("RV07"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //143: River8.
                _fileId("RV08"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //144: River9.
                _fileId("RV09"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //145: River10.
                _fileId("RV10"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //146: River11.
                _fileId("RV11"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(2, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //147: River12.
                _fileId("RV12"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //148: River13.
                _fileId("RV13"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //149: River14.
                _fileId("RV14"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //150: River15.
                _fileId("RV15"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //151: River16.
                _fileId("RV16"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //152: River17.
                _fileId("RV17"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 5),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //153: River18.
                _fileId("RV18"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //154: River19.
                _fileId("RV19"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //155: River20.
                _fileId("RV20"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 8),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //156: River21.
                _fileId("RV21"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(5, 8),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //157: River22.
                _fileId("RV22"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //158: River23.
                _fileId("RV23"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //159: River24.
                _fileId("RV24"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //160: River25.
                _fileId("RV25"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Rock),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //161: Ford1.
                _fileId("FORD1"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2_3_4_5_8)
            ),
            new TileSetTD( //162: Ford2.
                _fileId("FORD2"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0_1_2_4_7_8)
            ),
            new TileSetTD( //163: Falls1.
                _fileId("FALLS1"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //164: Falls2.
                _fileId("FALLS2"),
                _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //165: Bridge1.
                _fileId("BRIDGE1"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2_3_6_9_12_13)
            ),
            new TileSetTD( //166: Bridge1d.
                _fileId("BRIDGE1D"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(4, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2_3_12_13)
            ),
            new TileSetTD( //167: Bridge2.
                _fileId("BRIDGE2"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(5, 5),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0_1_6_12_18_19_23_24)
            ),
            new TileSetTD( //168: Bridge2d.
                _fileId("BRIDGE2D"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(5, 5),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0_1_18_19_23_24)
            ),
            new TileSetTD( //169: Bridge3.
                _fileId("BRIDGE3"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 5),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_3_4_6_9_11_14_19_24_25)
            ),
            new TileSetTD( //170: Bridge3d.
                _fileId("BRIDGE3D"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 5),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_3_4_6_11_19_25)
            ),
            new TileSetTD( //171: Bridge4.
                _fileId("BRIDGE4"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1_8_15_22)
            ),
            new TileSetTD( //172: Bridge4d.
                _fileId("BRIDGE4D"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 4),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1_22)
            ),
            new TileSetTD( //173: Shore24.
                _fileId("SH24"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_5)
            ),
            new TileSetTD( //174: Shore25.
                _fileId("SH25"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_3)
            ),
            new TileSetTD( //175: Shore26.
                _fileId("SH26"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //176: Shore27.
                _fileId("SH27"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //177: Shore28.
                _fileId("SH28"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 1),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //178: Shore29.
                _fileId("SH29"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_7)
            ),
            new TileSetTD( //179: Shore30.
                _fileId("SH30"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 2),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //180: Shore31.
                _fileId("SH31"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Rock),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //181: Patch16.
                _fileId("P16"),
                _theaterFlags(TheaterFlagsTD.Winter),
                _templateSize(2, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //182: Patch17.
                _fileId("P17"),
                _theaterFlags(TheaterFlagsTD.Winter),
                _templateSize(4, 2),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //183: Patch18.
                _fileId("P18"),
                _theaterFlags(TheaterFlagsTD.Winter),
                _templateSize(4, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //184: Patch19.
                _fileId("P19"),
                _theaterFlags(TheaterFlagsTD.Winter),
                _templateSize(4, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //185: Patch20.
                _fileId("P20"),
                _theaterFlags(TheaterFlagsTD.Winter),
                _templateSize(4, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //186: Shore34.
                _fileId("SH34"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_2_5_8)
            ),
            new TileSetTD( //187: Shore35.
                _fileId("SH35"),
                _theaterFlags(TheaterFlagsTD.TemperateWinter),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_0_3_6)
            ),
            new TileSetTD( //188: Shore36.
                _fileId("SH36"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //189: Shore37.
                _fileId("SH37"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //190: Shore38.
                _fileId("SH38"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //191: Shore39.
                _fileId("SH39"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //192: Shore40.
                _fileId("SH40"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //193: Shore41.
                _fileId("SH41"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Water),
                _landTypeAltTiles(tiles_0_1_3_4_6)
            ),
            new TileSetTD( //194: Shore42.
                _fileId("SH42"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //195: Shore43.
                _fileId("SH43"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //196: Shore44.
                _fileId("SH44"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //197: Shore45.
                _fileId("SH45"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(1, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //198: Shore46.
                _fileId("SH46"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0_1_3)
            ),
            new TileSetTD( //199: Shore47.
                _fileId("SH47"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //200: Shore48.
                _fileId("SH48"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //201: Shore49.
                _fileId("SH49"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //202: Shore50.
                _fileId("SH50"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_7)
            ),
            new TileSetTD( //203: Shore51.
                _fileId("SH51"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //204: Shore52.
                _fileId("SH52"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //205: Shore53.
                _fileId("SH53"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0_1_2_4)
            ),
            new TileSetTD( //206: Shore54.
                _fileId("SH54"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //207: Shore55.
                _fileId("SH55"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_2)
            ),
            new TileSetTD( //208: Shore56.
                _fileId("SH56"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //209: Shore57.
                _fileId("SH57"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(3, 2),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //210: Shore58.
                _fileId("SH58"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //211: Shore59.
                _fileId("SH59"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_0)
            ),
            new TileSetTD( //212: Shore60.
                _fileId("SH60"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_3_5)
            ),
            new TileSetTD( //213: Shore61.
                _fileId("SH61"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(2, 3),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_1)
            ),
            new TileSetTD( //214: Shore62.
                _fileId("SH62"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(6, 1),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            ),
            new TileSetTD( //215: Shore63.
                _fileId("SH63"),
                _theaterFlags(TheaterFlagsTD.Desert),
                _templateSize(4, 1),
                _landType(LandType.Water),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            )
        };

        public static TileSetTD get(byte tileSetId)
        {
            if (tileSetId < mTileSets.Length)
            {
                return mTileSets[tileSetId];
            }
            Program.warn(string.Format("Unknown tile set id value '{0}'!", tileSetId));
            return createUnknownTileSet("unknownTileId_" + tileSetId.ToString());
        }

        private static Dictionary<string, IdPair> mFileIds = null; //File id to tile set with numeric id lookup table.
        public static IdPair get(string fileId) //File id must be in uppercase. Returns null if not found.
        {
            if (mFileIds == null) //Lookup table not initialized?
            {
                mFileIds = new Dictionary<string, IdPair>();
                for (int i = 0; i < mTileSets.Length; i++)
                {
                    TileSetTD tileSet = mTileSets[i];
                    mFileIds.Add(tileSet.FileId, new IdPair(tileSet, (byte)i));
                }
            }
            IdPair tsIdPair;
            if (mFileIds.TryGetValue(fileId, out tsIdPair))
            {
                return tsIdPair;
            }
            Program.warn(string.Format("Unknown tile set file id value '{0}'!", fileId));
            return null;
        }

        public class IdPair //Tile set paired with its numeric id value.
        {
            //Numeric id could've been added directly to the tile set class,
            //but it adds many lines of code for a feature that is rarely used.
            private readonly TileSetTD mTileSet;
            private readonly byte mId;

            public IdPair(TileSetTD tileSet, byte id)
            {
                mTileSet = tileSet;
                mId = id;
            }

            public TileSetTD TileSet { get { return mTileSet; } }
            public byte Id { get { return mId; } }
        }

        private static TileSetTD createUnknownTileSet(string fileId)
        {
            return new TileSetTD(
                _fileId(fileId),
                _theaterFlags(TheaterFlagsTD.AllDefined),
                _templateSize(1, 1),
                _landType(LandType.Clear),
                _landTypeAlt(LandType.Clear),
                _landTypeAltTiles(tiles_none)
            );
        }
    }
}
