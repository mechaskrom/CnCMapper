using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class TerrainRA
    {
        private readonly Point mPriOffset;
        private readonly TheaterFlagsRA mTheaterFlags; //Specified for use in these theaters only.
        private readonly Point[] mOccupies; //Offsets to occupied tiles from tile position.
        private readonly Point[] mOverlaps; //Offsets to overlapped tiles from tile position.

        private TerrainRA(Point priOffset, TheaterFlagsRA theaterFlags, Point[] occupies, Point[] overlaps)
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

        public TheaterFlagsRA TheaterFlags
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
        private static Point _priOffset(int pixelX, int pixelY) { return SpriteTerrainRA.getPriOffset(pixelX, pixelY); }
        private static TheaterFlagsRA _theaterFlags(TheaterFlagsRA t) { return t; }
        private static Point[] _occupies(Point[] o) { return o; }
        private static Point[] _overlaps(Point[] o) { return o; }

        //All values checked in source.

        //Tile position offset lists.
        private static readonly Point[] offsets_none = new Point[] { };
        private static readonly Point[] offsets_0x0 = new Point[] { p(0, 0) };
        private static readonly Point[] offsets_0x1 = new Point[] { p(0, 1) };
        private static readonly Point[] offsets_1x0 = new Point[] { p(1, 0) };
        private static readonly Point[] offsets_2x0 = new Point[] { p(2, 0) };
        private static readonly Point[] offsets_0x0_0x1 = new Point[] { p(0, 0), p(0, 1) };
        private static readonly Point[] offsets_0x0_1x0 = new Point[] { p(0, 0), p(1, 0) };
        private static readonly Point[] offsets_0x0_1x1 = new Point[] { p(0, 0), p(1, 1) };
        private static readonly Point[] offsets_0x1_1x1 = new Point[] { p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_1x1 = new Point[] { p(0, 0), p(1, 0), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 1) };
        private static readonly Point[] offsets_0x0_2x0_2x1 = new Point[] { p(0, 0), p(2, 0), p(2, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1 = new Point[] { p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_0x1_1x1 = new Point[] { p(0, 0), p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x1_1x1_2x1_0x2 = new Point[] { p(0, 1), p(1, 1), p(2, 1), p(0, 2) };
        private static readonly Point[] offsets_0x0_1x0_3x1_0x2_3x2 = new Point[] { p(0, 0), p(1, 0), p(3, 1), p(0, 2), p(3, 2) };
        private static readonly Point[] offsets_0x0_1x0_2x0_3x1_1x2_2x2 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(3, 1), p(1, 2), p(2, 2) };
        private static readonly Point[] offsets_2x0_0x1_1x1_2x1_1x2_2x2 = new Point[] { p(2, 0), p(0, 1), p(1, 1), p(2, 1), p(1, 2), p(2, 2) };

        public static readonly TerrainRA Default = new TerrainRA(
            _priOffset(0, 0),
            _theaterFlags(TheaterFlagsRA.Unknown),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES01 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES02 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES03 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES04 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES05 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES06 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES07 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES08 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA BOXES09 = new TerrainRA( //Boxes.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Interior),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA ICE01 = new TerrainRA( //Ice sheets.
            _priOffset(24, 24),
            _theaterFlags(TheaterFlagsRA.Snow),
            _occupies(offsets_0x0_1x0_0x1_1x1),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA ICE02 = new TerrainRA( //Ice sheets.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.Snow),
            _occupies(offsets_0x0_0x1),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA ICE03 = new TerrainRA( //Ice sheets.
            _priOffset(24, 12),
            _theaterFlags(TheaterFlagsRA.Snow),
            _occupies(offsets_0x0_1x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA ICE04 = new TerrainRA( //Ice sheets.
            _priOffset(12, 12),
            _theaterFlags(TheaterFlagsRA.Snow),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA ICE05 = new TerrainRA( //Ice sheets.
            _priOffset(12, 12),
            _theaterFlags(TheaterFlagsRA.Snow),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA MINE = new TerrainRA( //Gold mine.
            _priOffset(12, 24),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x0),
            _overlaps(offsets_none)
        );

        public static readonly TerrainRA T01 = new TerrainRA( //Tree.
            _priOffset(11, 41),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T02 = new TerrainRA( //Tree.
            _priOffset(11, 44),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T03 = new TerrainRA( //Tree.
            _priOffset(12, 45),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T05 = new TerrainRA( //Tree.
            _priOffset(15, 41),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T06 = new TerrainRA( //Tree.
            _priOffset(16, 37),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T07 = new TerrainRA( //Tree.
            _priOffset(15, 41),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T08 = new TerrainRA( //Tree.
            _priOffset(14, 22),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x0),
            _overlaps(offsets_1x0)
        );

        public static readonly TerrainRA T10 = new TerrainRA( //Tree.
            _priOffset(25, 43),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainRA T11 = new TerrainRA( //Tree.
            _priOffset(23, 44),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainRA T12 = new TerrainRA( //Tree.
            _priOffset(14, 36),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T13 = new TerrainRA( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x0_1x1)
        );

        public static readonly TerrainRA T14 = new TerrainRA( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainRA T15 = new TerrainRA( //Tree.
            _priOffset(19, 40),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0)
        );

        public static readonly TerrainRA T16 = new TerrainRA( //Tree.
            _priOffset(13, 36),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA T17 = new TerrainRA( //Tree.
            _priOffset(18, 44),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1),
            _overlaps(offsets_0x0_1x1)
        );

        public static readonly TerrainRA TC01 = new TerrainRA( //Tree clump.
            _priOffset(28, 41),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1),
            _overlaps(offsets_0x0_1x0_2x1)
        );

        public static readonly TerrainRA TC02 = new TerrainRA( //Tree clump.
            _priOffset(38, 41),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_1x0_0x1_1x1),
            _overlaps(offsets_0x0_2x0_2x1)
        );

        public static readonly TerrainRA TC03 = new TerrainRA( //Tree clump.
            _priOffset(33, 35),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x0_1x0_0x1_1x1),
            _overlaps(offsets_2x0)
        );

        public static readonly TerrainRA TC04 = new TerrainRA( //Tree clump.
            _priOffset(44, 49),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_0x1_1x1_2x1_0x2),
            _overlaps(offsets_0x0_1x0_2x0_3x1_1x2_2x2)
        );

        public static readonly TerrainRA TC05 = new TerrainRA( //Tree clump.
            _priOffset(49, 58),
            _theaterFlags(TheaterFlagsRA.SnowTemperate),
            _occupies(offsets_2x0_0x1_1x1_2x1_1x2_2x2),
            _overlaps(offsets_0x0_1x0_3x1_0x2_3x2)
        );
    }
}
