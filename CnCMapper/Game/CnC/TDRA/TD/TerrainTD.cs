using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class TerrainTD
    {
        private readonly Point mPriOffset;
        private readonly TheaterFlagsTD mTheaterFlags; //Specified for use in these theaters only.
        private readonly Point[] mOccupies; //Offsets to occupied tiles from tile position.
        private readonly Point[] mOverlaps; //Offsets to overlapped tiles from tile position.

        private TerrainTD(Point priOffset, TheaterFlagsTD theaterFlags, Point[] occupies, Point[] overlaps)
        {
            mPriOffset = priOffset;
            mTheaterFlags = theaterFlags;
            mOccupies = occupies;
            mOverlaps = overlaps;
        }

        public Point PriOffset
        {
            get { return mPriOffset; }
        }

        public TheaterFlagsTD TheaterFlags
        {
            get { return mTheaterFlags; }
        }

        public Point[] Occupies
        {
            get { return mOccupies; }
        }

        public Point[] Overlaps
        {
            get { return mOverlaps; }
        }

        //Some dummy methods used to make constructors easier to read.
        private static Point p(int x, int y) { return new Point(x, y); }
        private static Point _priOffset(int pixelX, int pixelY) { return SpriteTerrainTD.getPriOffset(pixelX, pixelY); }
        private static TheaterFlagsTD _theaterFlags(TheaterFlagsTD t) { return t; }
        private static Point[] _occupies(Point[] o) { return o; }
        private static Point[] _overlaps(Point[] o) { return o; }

        //All values checked in source.

        //Tile position offset lists.
        private static readonly Point[] offsets_none = new Point[] { };
        private static readonly Point[] offsets_0x0 = new Point[] { p(0, 0) };
        private static readonly Point[] offsets_0x1 = new Point[] { p(0, 1) };
        private static readonly Point[] offsets_1x0 = new Point[] { p(1, 0) };
        private static readonly Point[] offsets_1x1 = new Point[] { p(1, 1) };
        private static readonly Point[] offsets_2x0 = new Point[] { p(2, 0) };
        private static readonly Point[] offsets_4x0 = new Point[] { p(4, 0) };
        private static readonly Point[] offsets_0x0_1x0 = new Point[] { p(0, 0), p(1, 0) };
        private static readonly Point[] offsets_0x0_1x1 = new Point[] { p(0, 0), p(1, 1) };
        private static readonly Point[] offsets_0x1_1x1 = new Point[] { p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_1x1 = new Point[] { p(0, 0), p(1, 0), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 1) };
        private static readonly Point[] offsets_0x0_2x0_2x1 = new Point[] { p(0, 0), p(2, 0), p(2, 1) };
        private static readonly Point[] offsets_0x1_1x1_2x1 = new Point[] { p(0, 1), p(1, 1), p(2, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1 = new Point[] { p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_0x1_1x1 = new Point[] { p(0, 0), p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x0_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(2, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x0_3x0 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(3, 0) };
        private static readonly Point[] offsets_0x1_1x1_2x1_0x2 = new Point[] { p(0, 1), p(1, 1), p(2, 1), p(0, 2) };
        private static readonly Point[] offsets_0x0_1x0_2x0_0x1_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(0, 1), p(2, 1) };
        private static readonly Point[] offsets_0x0_1x0_3x1_0x2_3x2 = new Point[] { p(0, 0), p(1, 0), p(3, 1), p(0, 2), p(3, 2) };
        private static readonly Point[] offsets_0x0_1x0_2x0_3x1_1x2_2x2 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(3, 1), p(1, 2), p(2, 2) };
        private static readonly Point[] offsets_2x0_0x1_1x1_2x1_1x2_2x2 = new Point[] { p(2, 0), p(0, 1), p(1, 1), p(2, 1), p(1, 2), p(2, 2) };

        public static readonly TerrainTD Default = new TerrainTD(
            _priOffset(0, 0),
            _theaterFlags(TheaterFlagsTD.Unknown),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainTD ROCK1 = new TerrainTD( //Rock.
            _priOffset(33, 41),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0_2x0_2x1)
        );

        public static readonly TerrainTD ROCK2 = new TerrainTD( //Rock.
            _priOffset(24, 23),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0_1x0),
            _overlaps(offsets_2x0)
        );

        public static readonly TerrainTD ROCK3 = new TerrainTD( //Rock.
            _priOffset(20, 39),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0_2x1)
        );

        public static readonly TerrainTD ROCK4 = new TerrainTD( //Rock.
            _priOffset(12, 20),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0),
            _overlaps(offsets_1x0)
        );

        public static readonly TerrainTD ROCK5 = new TerrainTD( //Rock.
            _priOffset(17, 19),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0),
            _overlaps(offsets_1x0)
        );

        public static readonly TerrainTD ROCK6 = new TerrainTD( //Rock.
            _priOffset(28, 40),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x1_1x1_2x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainTD ROCK7 = new TerrainTD( //Rock.
            _priOffset(57, 22),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0_1x0_2x0_3x0),
            _overlaps(offsets_4x0)
        );

        public static readonly TerrainTD SPLIT2 = new TerrainTD( //Blossom/Tiberium tree.
            _priOffset(18, 44),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x0_1x1)
        );

        public static readonly TerrainTD SPLIT3 = new TerrainTD( //Blossom/Tiberium tree.
            _priOffset(18, 44),
            _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x0_1x1)
        );

        public static readonly TerrainTD T01 = new TerrainTD( //Tree.
            _priOffset(11, 41),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T02 = new TerrainTD( //Tree.
            _priOffset(11, 44),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T03 = new TerrainTD( //Tree.
            _priOffset(12, 45),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T04 = new TerrainTD( //Tree.
            _priOffset(8, 9),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainTD T05 = new TerrainTD( //Tree.
            _priOffset(15, 41),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T06 = new TerrainTD( //Tree.
            _priOffset(16, 37),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T07 = new TerrainTD( //Tree.
            _priOffset(15, 41),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T08 = new TerrainTD( //Tree.
            _priOffset(14, 22),
            _theaterFlags(TheaterFlagsTD.DesertTemperateWinter),
            _occupies(offsets_0x0),
            _overlaps(offsets_1x0)
        );

        public static readonly TerrainTD T09 = new TerrainTD( //Tree.
            _priOffset(11, 22),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_0x0),
            _overlaps(offsets_1x0)
        );

        public static readonly TerrainTD T10 = new TerrainTD( //Tree.
            _priOffset(25, 43),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainTD T11 = new TerrainTD( //Tree.
            _priOffset(23, 44),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainTD T12 = new TerrainTD( //Tree.
            _priOffset(14, 36),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T13 = new TerrainTD( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x0_1x1)
        );

        public static readonly TerrainTD T14 = new TerrainTD( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainTD T15 = new TerrainTD( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainTD T16 = new TerrainTD( //Tree.
            _priOffset(13, 36),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T17 = new TerrainTD( //Tree.
            _priOffset(18, 44),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainTD T18 = new TerrainTD( //Tree.
            _priOffset(33, 40),
            _theaterFlags(TheaterFlagsTD.Desert),
            _occupies(offsets_1x1),
            _overlaps(offsets_0x0_1x0_2x0_0x1_2x1)
        );

        public static readonly TerrainTD TC01 = new TerrainTD( //Tree clump.
            _priOffset(28, 41),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0_2x1)
        );

        public static readonly TerrainTD TC02 = new TerrainTD( //Tree clump.
            _priOffset(38, 41),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_1x0_0x1_1x1),
            _overlaps(offsets_0x0_2x0_2x1)
        );

        public static readonly TerrainTD TC03 = new TerrainTD( //Tree clump.
            _priOffset(33, 35),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x0_1x0_0x1_1x1),
            _overlaps(offsets_2x0)
        );

        public static readonly TerrainTD TC04 = new TerrainTD( //Tree clump.
            _priOffset(44, 49),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_0x1_1x1_2x1_0x2),
            _overlaps(offsets_0x0_1x0_2x0_3x1_1x2_2x2)
        );

        public static readonly TerrainTD TC05 = new TerrainTD( //Tree clump.
            _priOffset(49, 58),
            _theaterFlags(TheaterFlagsTD.TemperateWinter),
            _occupies(offsets_2x0_0x1_1x1_2x1_1x2_2x2),
            _overlaps(offsets_0x0_1x0_3x1_0x2_3x2)
        );
    }
}
