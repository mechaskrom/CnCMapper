using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC.D2
{
    partial class SpriteUnitD2
    {
        private enum Dir8Way
        {
            North = 0,
            Northwest = 1,
            West = 2,
            Southwest = 3,
            South = 4,
            Southeast = 5,
            East = 6,
            Northeast = 7,
        }

        private static Dir8Way toDir8Way(string direction)
        {
            //Seems to be the same as in Tiberian Dawn and Red Alert.
            byte dirVal = toDirection(direction);
            //Transform 256 directions to 8. Invert value also to match enum.
            return (Dir8Way)((-(dirVal + 16) / 32) & 7); //Offset start and divide by 32 (256/8).
            //0 = north, 240-15 (wrap around after 255 to 0).
            //1 = north-west, 208-239.
            //2 = west, 176-207.
            //3 = south-west, 144-175.
            //4 = south, 112-143.
            //5 = south-east, 80-111.
            //6 = east, 48-79.
            //7 = north-east, 16-47.
        }

        private static byte toDirection(string direction)
        {
            return (byte)(int.Parse(direction) & 0xFF); //Direction is always a byte (0-255).
        }

        //Frame with a remapped flag so we know if this frame should be house recolored.
        class FrameRemap : Frame
        {
            private readonly bool mIsRemapped; //This frame has a remap table?

            public FrameRemap(Frame frame, bool isRemapped)
                : base(frame.Width, frame.Height, frame.Pixels, frame.IsEmpty)
            {
                mIsRemapped = isRemapped;
            }

            public bool IsRemapped { get { return mIsRemapped; } }
        }

        //8 direction frames for all units.
        //North, Northwest, West, Southwest, South, Southeast, East, Northeast.
        class DirFrameSet
        {
            private readonly UnitsShpFile mFileNr; //Units SHP-file (0-2).
            private readonly int[] mIndices;
            private readonly bool[] mFlipH; //Flip frame at index horizontally.
            private readonly bool[] mFlipV; //Flip frame at index vertically.
            private readonly Dictionary<TheaterD2, FrameRemap[]> mFrameSet;

            private DirFrameSet(UnitsShpFile fileNr, int[] indices, bool[] flipH, bool[] flipV)
            {
                mFileNr = fileNr;
                mIndices = indices;
                mFlipH = flipH;
                mFlipV = flipV;
                mFrameSet = new Dictionary<TheaterD2, FrameRemap[]>();
            }

            public FrameRemap getFrame(Dir8Way direction, TheaterD2 theater)
            {
                int dirIndex = (int)direction;
                FrameRemap[] frameSet = getFrameSet(theater);
                FrameRemap frameRemap = frameSet[dirIndex];
                if (frameRemap == null) //Not cached yet?
                {
                    Frame frame = theater.getUnitFrame(mFileNr, mIndices[dirIndex]);
                    frame = frame.flip(mFlipH[dirIndex], mFlipV[dirIndex]);
                    bool isRemapped = theater.isUnitFrameRemapped(mFileNr, mIndices[dirIndex]);
                    frameRemap = new FrameRemap(frame, isRemapped);
                    frameSet[dirIndex] = frameRemap;
                }
                return frameRemap;
            }

            private FrameRemap[] getFrameSet(TheaterD2 theater)
            {
                FrameRemap[] frameSet;
                if (!mFrameSet.TryGetValue(theater, out frameSet)) //Not cached yet?
                {
                    frameSet = new FrameRemap[8];
                    mFrameSet.Add(theater, frameSet);
                }
                return frameSet;
            }

            //Some dummy values used to make constructors easier to read.
            private const bool Y = true;
            private const bool N = false;

            private static readonly bool[] NoFlip = new bool[8] { N, N, N, N, N, N, N, N };

            private static readonly bool[] AircraftFlipH = new bool[8] { N, Y, Y, Y, N, N, N, N };
            private static readonly bool[] AircraftFlipV = new bool[8] { N, N, N, Y, Y, Y, N, N };

            private static readonly bool[] FootFlipH = new bool[8] { N, Y, Y, Y, N, N, N, N };
            private static readonly bool[] FootFlipV = NoFlip;

            private static readonly bool[] VehicleFlipH = new bool[8] { N, Y, Y, Y, N, N, N, N };
            private static readonly bool[] VehicleFlipV = NoFlip;

            private static readonly bool[] TurretFlipH = VehicleFlipH;
            private static readonly bool[] TurretFlipV = VehicleFlipV;

            private static DirFrameSet createAircraft(int startIndex, int interval)
            {
                return new DirFrameSet(UnitsShpFile.Nr0, getAircraftIndices(startIndex, interval), AircraftFlipH, AircraftFlipV);
            }

            private static DirFrameSet createFoot(int startIndex, int interval)
            {
                return new DirFrameSet(UnitsShpFile.Nr0, getFootIndices(startIndex, interval), FootFlipH, FootFlipV);
            }

            private static DirFrameSet createWheel(int startIndex)
            {
                return new DirFrameSet(UnitsShpFile.Nr0, getVehicleIndices(startIndex), VehicleFlipH, VehicleFlipV);
            }

            private static DirFrameSet createTrack(int startIndex)
            {
                return new DirFrameSet(UnitsShpFile.Nr2, getVehicleIndices(startIndex), VehicleFlipH, VehicleFlipV);
            }

            private static DirFrameSet createTurret(int startIndex)
            {
                return new DirFrameSet(UnitsShpFile.Nr2, getTurretIndices(startIndex), TurretFlipH, TurretFlipV);
            }

            private static int[] getAircraftIndices(int startIndex, int interval)
            {
                return new int[8]
                {
                    startIndex + (0*interval),
                    startIndex + (1*interval),
                    startIndex + (2*interval),
                    startIndex + (1*interval),
                    startIndex + (0*interval),
                    startIndex + (1*interval),
                    startIndex + (2*interval),
                    startIndex + (1*interval),
                };
            }

            private static int[] getFootIndices(int startIndex, int interval)
            {
                return new int[8]
                {
                    startIndex + (0*interval),
                    startIndex + (1*interval),
                    startIndex + (1*interval),
                    startIndex + (1*interval),
                    startIndex + (2*interval),
                    startIndex + (1*interval),
                    startIndex + (1*interval),
                    startIndex + (1*interval),
                };
            }

            private static int[] getVehicleIndices(int startIndex)
            {
                return new int[8]
                {
                    startIndex + 0,
                    startIndex + 1,
                    startIndex + 2,
                    startIndex + 3,
                    startIndex + 4,
                    startIndex + 3,
                    startIndex + 2,
                    startIndex + 1,
                };
            }

            private static int[] getTurretIndices(int startIndex)
            {
                return getVehicleIndices(startIndex);
            }

            public static class Unit
            {
                public static readonly DirFrameSet None = null;

                public static readonly DirFrameSet Carryall = createAircraft(45, 1);
                public static readonly DirFrameSet Ornithopter = createAircraft(53, 3);
                public static readonly DirFrameSet Frigate = createAircraft(60, 1);

                public static readonly DirFrameSet Saboteur = createFoot(63, 3);
                public static readonly DirFrameSet Soldier = createFoot(73, 3);
                public static readonly DirFrameSet Trooper = createFoot(82, 3);
                public static readonly DirFrameSet Infantry = createFoot(91, 4);
                public static readonly DirFrameSet Troopers = createFoot(103, 4);

                public static readonly DirFrameSet Tank = createTrack(0);
                public static readonly DirFrameSet Launcher = createTrack(0);
                public static readonly DirFrameSet Deviator = createTrack(0);
                public static readonly DirFrameSet SonicTank = createTrack(0);
                public static readonly DirFrameSet SiegeTank = createTrack(10);
                public static readonly DirFrameSet Devastator = createTrack(20);

                public static readonly DirFrameSet Quad = createWheel(0);
                public static readonly DirFrameSet Trike = createWheel(5);
                public static readonly DirFrameSet RaiderTrike = createWheel(5);
                public static readonly DirFrameSet Harvester = createWheel(10);
                public static readonly DirFrameSet MCV = createWheel(15);

                public static readonly DirFrameSet Sandworm = new DirFrameSet(UnitsShpFile.Nr1,
                    new int[8] { 68, 68, 68, 68, 68, 68, 68, 68 },
                    NoFlip,
                    NoFlip
                );
            }

            public static class Turret
            {
                public static readonly DirFrameSet None = null;

                public static readonly DirFrameSet Single = createTurret(5);
                public static readonly DirFrameSet Howitzer = createTurret(15);
                public static readonly DirFrameSet Dual = createTurret(25);
                public static readonly DirFrameSet Dish = createTurret(30);
                public static readonly DirFrameSet Ramp = createTurret(35);
            }
        }
    }
}
