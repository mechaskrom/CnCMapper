using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //There are many structures and they use pretty varied values compared to other
    //sprites in Tiberian Dawn. Let's use a lot of code to manage this a bit better.
    class StructureTD
    {
        private readonly bool mHasBib;
        private readonly int mStorage;
        private readonly bool mIsWall;
        private readonly bool mIsRadarVisible; //Ignored by the game radar renderer.
        private readonly Point[] mOccupies; //Offsets to tiles occupied from tile position.

        private StructureTD(bool hasBib, int storage, bool isWall, bool isRadarVisible, Point[] occupies)
        {
            mHasBib = hasBib;
            mStorage = storage;
            mIsWall = isWall;
            mIsRadarVisible = isRadarVisible;
            mOccupies = occupies;
        }

        public bool HasBib
        {
            get { return mHasBib; }
        }

        public int Storage
        {
            get { return mStorage; }
        }

        public bool IsWall
        {
            get { return mIsWall; }
        }

        public bool IsRadarVisible
        {
            get { return mIsRadarVisible; }
        }

        public Point[] Occupies
        {
            get { return mOccupies; }
        }

        //Some dummy methods used to make constructors easier to read.
        private static Point p(int x, int y) { return new Point(x, y); }
        private static bool _hasBib(bool b) { return b; }
        private static int _storage(int i) { return i; }
        private static bool _isWall(bool b) { return b; }
        private static bool _isRadarVisible(bool b) { return b; }
        private static Point[] _occupies(Point[] o) { return o; }

        //All values checked in source.

        //Tile position offset lists.
        private static readonly Point[] offsets_0x0 = new Point[] { p(0, 0) };
        private static readonly Point[] offsets_0x1 = new Point[] { p(0, 1) };
        private static readonly Point[] offsets_0x0_1x0 = new Point[] { p(0, 0), p(1, 0) };
        private static readonly Point[] offsets_0x1_1x1 = new Point[] { p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_0x1_1x1 = new Point[] { p(0, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_1x1 = new Point[] { p(0, 0), p(1, 0), p(1, 1) };
        private static readonly Point[] offsets_0x1_1x1_1x2 = new Point[] { p(0, 1), p(1, 1), p(1, 2) };
        private static readonly Point[] offsets_1x0_0x1_1x1 = new Point[] { p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_0x1_1x1 = new Point[] { p(0, 0), p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1_2x1 = new Point[] { p(1, 0), p(0, 1), p(1, 1), p(2, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1_2x1_1x2 = new Point[] { p(1, 0), p(0, 1), p(1, 1), p(2, 1), p(1, 2) };
        private static readonly Point[] offsets_0x0_1x0_2x0_0x1_1x1_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(0, 1), p(1, 1), p(2, 1) };
        private static readonly Point[] offsets_0x1_1x1_2x1_0x2_1x2_2x2 = new Point[] { p(0, 1), p(1, 1), p(2, 1), p(0, 2), p(1, 2), p(2, 2) };
        private static readonly Point[] offsets_1x0_2x0_3x0_1x1_2x1_3x1 = new Point[] { p(1, 0), p(2, 0), p(3, 0), p(1, 1), p(2, 1), p(3, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x0_3x0_0x1_1x1_2x1_3x1 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(3, 0), p(0, 1), p(1, 1), p(2, 1), p(3, 1) };

        public static readonly StructureTD Default = new StructureTD(
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD AFLD = new StructureTD( //Airstrip.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_3x0_0x1_1x1_2x1_3x1)
        );

        public static readonly StructureTD ATWR = new StructureTD( //Advanced guard tower.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1)
        );

        public static readonly StructureTD BIO = new StructureTD( //Bio-research laboratory.
            _hasBib(true),
            _storage(100),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureTD EYE = new StructureTD( //Advanced communications center.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_0x1_1x1)
        );

        public static readonly StructureTD FACT = new StructureTD( //Construction yard.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureTD FIX = new StructureTD( //Repair bay.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1_2x1_1x2)
        );

        public static readonly StructureTD GTWR = new StructureTD( //Guard tower.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD GUN = new StructureTD( //Gun turret.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD HAND = new StructureTD( //Hand of Nod.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1_1x2)
        );

        public static readonly StructureTD HOSP = new StructureTD( //Hospital.
            _hasBib(true),
            _storage(100),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureTD HPAD = new StructureTD( //Helipad.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureTD HQ = new StructureTD( //Communications center.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_0x1_1x1)
        );

        public static readonly StructureTD MISS = new StructureTD( //Technology center.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureTD NUK2 = new StructureTD( //Advanced power plant.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_0x1_1x1)
        );

        public static readonly StructureTD NUKE = new StructureTD( //Power plant.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_0x1_1x1)
        );

        public static readonly StructureTD OBLI = new StructureTD( //Obelisk of light.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1)
        );

        public static readonly StructureTD PROC = new StructureTD( //Tiberium refinery.
            _hasBib(true),
            _storage(1000),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1_2x1)
        );

        public static readonly StructureTD PYLE = new StructureTD( //Barracks.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD SAM = new StructureTD( //SAM site.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD SILO = new StructureTD( //Tiberium silo.
            _hasBib(true),
            _storage(1500),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD TMPL = new StructureTD( //Temple of Nod.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureTD WEAP = new StructureTD( //Weapons factory.
            _hasBib(true),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureTD BARB = new StructureTD( //Barbwire fence.
            _hasBib(false),
            _storage(0),
            _isWall(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD BRIK = new StructureTD( //Concrete wall.
            _hasBib(false),
            _storage(0),
            _isWall(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD CYCL = new StructureTD( //Chain link fence.
            _hasBib(false),
            _storage(0),
            _isWall(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD SBAG = new StructureTD( //Sandbag wall.
            _hasBib(false),
            _storage(0),
            _isWall(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD WOOD = new StructureTD( //Wooden fence.
            _hasBib(false),
            _storage(0),
            _isWall(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD ARCO = new StructureTD( //Civilian oil tanker.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V01 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureTD V02 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureTD V03 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_1x0_0x1_1x1)
        );

        public static readonly StructureTD V04 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureTD V05 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V06 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V07 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V08 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V09 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V10 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V11 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V12 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V13 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V14 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V15 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V16 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V17 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V18 = new StructureTD( //Farmland.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V19 = new StructureTD( //Civilian oil derrick pump.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V20 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureTD V21 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_1x1)
        );

        public static readonly StructureTD V22 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V23 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V24 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureTD V25 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1)
        );

        public static readonly StructureTD V26 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V27 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V28 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V29 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V30 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V31 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V32 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V33 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureTD V34 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V35 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V36 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureTD V37 = new StructureTD( //Civilian structure.
            _hasBib(false),
            _storage(0),
            _isWall(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_2x0_3x0_1x1_2x1_3x1)
        );
    }
}
