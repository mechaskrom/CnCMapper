using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //There are many structures and they use pretty varied values compared to other
    //sprites in Red Alert. Let's use a lot of code to manage this a bit better.
    class StructureRA
    {
        private readonly bool mIsWall;
        private readonly bool mIsFake;
        private readonly bool mIsRadarVisible; //Ignored by the game radar renderer.
        private readonly Point[] mOccupies; //Offsets to tiles occupied from tile position.

        private StructureRA(bool isWall, bool isFake, bool isRadarVisible, Point[] occupies)
        {
            mIsWall = isWall;
            mIsFake = isFake;
            mIsRadarVisible = isRadarVisible;
            mOccupies = occupies;
        }

        public bool IsWall
        {
            get { return mIsWall; }
        }

        public bool IsFake
        {
            get { return mIsFake; }
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
        private static bool _isWall(bool b) { return b; }
        private static bool _isFake(bool b) { return b; }
        private static bool _isRadarVisible(bool b) { return b; }
        private static Point[] _occupies(Point[] o) { return o; }

        //All values checked in source.

        //Tile position offset lists.
        private static readonly Point[] offsets_0x0 = new Point[] { p(0, 0) };
        private static readonly Point[] offsets_0x1 = new Point[] { p(0, 1) };
        private static readonly Point[] offsets_0x0_1x0 = new Point[] { p(0, 0), p(1, 0) };
        private static readonly Point[] offsets_0x1_1x1 = new Point[] { p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_1x1 = new Point[] { p(0, 0), p(1, 0), p(1, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1 = new Point[] { p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_0x0_1x0_0x1_1x1 = new Point[] { p(0, 0), p(1, 0), p(0, 1), p(1, 1) };
        private static readonly Point[] offsets_1x0_0x1_1x1_2x1_0x2 = new Point[] { p(1, 0), p(0, 1), p(1, 1), p(2, 1), p(0, 2) };
        private static readonly Point[] offsets_1x0_0x1_1x1_2x1_1x2 = new Point[] { p(1, 0), p(0, 1), p(1, 1), p(2, 1), p(1, 2) };
        private static readonly Point[] offsets_0x0_1x0_2x0_0x1_1x1_2x1 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(0, 1), p(1, 1), p(2, 1) };
        private static readonly Point[] offsets_0x1_1x1_2x1_0x2_1x2_2x2 = new Point[] { p(0, 1), p(1, 1), p(2, 1), p(0, 2), p(1, 2), p(2, 2) };
        private static readonly Point[] offsets_1x0_2x0_3x0_1x1_2x1_3x1 = new Point[] { p(1, 0), p(2, 0), p(3, 0), p(1, 1), p(2, 1), p(3, 1) };
        private static readonly Point[] offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2 = new Point[] { p(0, 0), p(1, 0), p(2, 0), p(0, 1), p(1, 1), p(2, 1), p(0, 2), p(1, 2), p(2, 2) };

        public static readonly StructureRA Default = new StructureRA(
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA AFLD = new StructureRA( //Airstrip.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureRA AGUN = new StructureRA( //Anti-aircraft gun.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1)
        );

        public static readonly StructureRA APWR = new StructureRA( //Advanced power plant.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA ATEK = new StructureRA( //Advanced technology center.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA BARL = new StructureRA( //Barrel (one).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA BARR = new StructureRA( //Barracks (Allied).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA BIO = new StructureRA( //Bio-research laboratory.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA BRL3 = new StructureRA( //Barrels (three).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA DOME = new StructureRA( //Radar dome.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA DOMF = new StructureRA( //Fake radar dome (DOME).
            _isWall(false),
            _isFake(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA FACT = new StructureRA( //Construction yard.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA FACF = new StructureRA( //Fake construction yard (FACT).
            _isWall(false),
            _isFake(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA FCOM = new StructureRA( //Forward command post.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA FIX = new StructureRA( //Repair bay.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1_2x1_1x2)
        );

        public static readonly StructureRA FTUR = new StructureRA( //Flame turret.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA GAP = new StructureRA( //Gap generator.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1)
        );

        public static readonly StructureRA GUN = new StructureRA( //Gun turret.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA HBOX = new StructureRA( //Camouflaged pillbox.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA HOSP = new StructureRA( //Hospital.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA HPAD = new StructureRA( //Helipad.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA IRON = new StructureRA( //Iron curtain.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA KENN = new StructureRA( //Dog kennel.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA MINP = new StructureRA( //Anti-personnel mine.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA MINV = new StructureRA( //Anti-vehicle mine.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA MISS = new StructureRA( //Technology center.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureRA MSLO = new StructureRA( //Missile silo.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA PBOX = new StructureRA( //Pillbox.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA PDOX = new StructureRA( //Chronosphere.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA POWR = new StructureRA( //Power plant.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA PROC = new StructureRA( //Ore refinery.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1_2x1_0x2)
        );

        public static readonly StructureRA SAM = new StructureRA( //SAM site.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA SILO = new StructureRA( //Ore silo.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA SPEN = new StructureRA( //Submarine pen.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA SPEF = new StructureRA( //Fake submarine pen (SPEN).
            _isWall(false),
            _isFake(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA STEK = new StructureRA( //Soviet technology center.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA SYRD = new StructureRA( //Ship yard.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA SYRF = new StructureRA( //Fake ship yard (SYRD).
            _isWall(false),
            _isFake(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1_0x2_1x2_2x2)
        );

        public static readonly StructureRA TENT = new StructureRA( //Barracks (Soviet).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_0x1_1x1)
        );

        public static readonly StructureRA TSLA = new StructureRA( //Tesla coil.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1)
        );

        public static readonly StructureRA WEAP = new StructureRA( //Weapons factory.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureRA WEAF = new StructureRA( //Fake weapons factory (WEAP).
            _isWall(false),
            _isFake(true),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_2x0_0x1_1x1_2x1)
        );

        public static readonly StructureRA LAR1 = new StructureRA( //Ant larva (one).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA LAR2 = new StructureRA( //Ant larvas (two).
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA QUEE = new StructureRA( //Ant queen.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA BARB = new StructureRA( //Barbwire fence (Tiberian Dawn).
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA BRIK = new StructureRA( //Concrete wall.
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA CYCL = new StructureRA( //Chain link fence.
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA FENC = new StructureRA( //Barbwire fence (Soviet).
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA SBAG = new StructureRA( //Sandbag wall.
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA WOOD = new StructureRA( //Wooden fence.
            _isWall(true),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V01 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA V02 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA V03 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_1x0_0x1_1x1)
        );

        public static readonly StructureRA V04 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA V05 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V06 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V07 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V08 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V09 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V10 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V11 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V12 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V13 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V14 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V15 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V16 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V17 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V18 = new StructureRA( //Farmland.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(false),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V19 = new StructureRA( //Civilian oil derrick pump.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V20 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA V21 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0_1x1)
        );

        public static readonly StructureRA V22 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V23 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V24 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x1_1x1)
        );

        public static readonly StructureRA V25 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_0x1_1x1)
        );

        public static readonly StructureRA V26 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V27 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V28 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V29 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V30 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V31 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V32 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V33 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0_1x0)
        );

        public static readonly StructureRA V34 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V35 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V36 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_0x0)
        );

        public static readonly StructureRA V37 = new StructureRA( //Civilian structure.
            _isWall(false),
            _isFake(false),
            _isRadarVisible(true),
            _occupies(offsets_1x0_2x0_3x0_1x1_2x1_3x1)
        );
    }
}
