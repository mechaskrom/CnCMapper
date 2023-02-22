using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class SpriteUnitTDRA : SpriteTDRA
    {
        //Unit draw priority offset is center with 128 added to Y. Checked in source.
        private static readonly Point PriOffsetUnit = toPriOffset(128, 128 + 128);

        protected static bool mIsAddingHelipadAircrafts = false; //Flag set while adding aircrafts to helipads.

        protected enum AircraftRotors
        {
            None,
            Single,
            Dual
        }

        protected bool mIsSpecialEffect = false; //Unit sprite is a special effect i.e. an aircraft shadow or a boat wake.

        protected delegate T Create<T>(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp); //Derived unit constructor call.

        protected SpriteUnitTDRA(string id, TilePos tilePos, FileShpSpriteSetTDRA fileShp, bool exposeConcealed)
            : base(id, tilePos, fileShp)
        {
            mPriOffset = PriOffsetUnit;
            //Units are often hidden behind terrain.
            mPriPlane = exposeConcealed ? PriPlaneHigh : PriPlaneDefault;
        }

        protected static T createAddSprite<T>(T owner, FileShpSpriteSetTDRA fileShp, int frameIndex, Point drawOffset, Create<T> create)
            where T : SpriteUnitTDRA
        {
            T spr = create(fileShp.Id, owner.TilePos, fileShp);
            owner.setAddSpriteDraw(spr);
            spr.mFrameIndex = frameIndex;
            spr.mDrawOffset = drawOffset;
            return spr;
        }

        protected static T createAircraftRotorSprite<T>(T owner, Dir8Way direction, AircraftRotors rotors, MapTDRA map, Create<T> create)
            where T : SpriteUnitTDRA
        {
            if (rotors == AircraftRotors.None) //No rotors?
            {
                return null;
            }

            //LROTOR and RROTOR have two sets of frames. First (0-3) is fast spin (in flight),
            //last (4-11) is slow spin (landed).
            //Rotor has no ground shadow, only the body of the aircraft. Checked in game.

            //Fetch the first rotor.
            FileShpSpriteSetTDRA fileShpRRotor = map.Theater.getSpriteSet("RROTOR");
            Point drawOffsetRRotor = getDrawOffsetCenter(fileShpRRotor);
            drawOffsetRRotor.Y -= 2;
            if (rotors == AircraftRotors.Single) //Single centered rotor?
            {
                return createAddSprite(owner, fileShpRRotor, 11, drawOffsetRRotor, create);
            }
            else if (rotors == AircraftRotors.Dual) //Dual rotors? Offset them along direction axis.
            {
                //Tiberian Dawn: See "Draw_It()" in "AIRCRAFT.CPP".
                //Red Alert: See "Draw_Rotors()" in "AIRCRAFT.CPP".
                int distance = 9;
                if (direction == Dir8Way.North || direction == Dir8Way.South)
                {
                    distance--;
                }
                else if (direction == Dir8Way.West || direction == Dir8Way.East)
                {
                    distance++;
                }

                //Fetch the second rotor.
                FileShpSpriteSetTDRA fileShpLRotor = map.Theater.getSpriteSet("LROTOR");
                Point drawOffsetLRotor = getDrawOffsetCenter(fileShpLRotor);
                drawOffsetLRotor.Y -= 2;

                //Move the two rotors away from the center along the aircraft's direction axis.
                Point moveOffset = movePoint(direction, distance); //Move from center towards front.
                drawOffsetRRotor.Offset(moveOffset);
                moveOffset.Offset(movePoint(toDir8WayInverted(direction), distance * 2)); //Move from front towards back.
                drawOffsetLRotor.Offset(moveOffset);

                //Chain the two rotor sprites together.
                T spr = createAddSprite(owner, fileShpRRotor, 11, drawOffsetRRotor, create);
                spr.mAddSprite = createAddSprite(owner, fileShpLRotor, 11, drawOffsetLRotor, create);
                return spr;
            }
            throw new ArgumentException(); //Should never happen.
        }

        protected static T createAircraftShadowSprite<T>(T owner, Create<T> create)
            where T : SpriteUnitTDRA
        {
            //The shadow on the ground is just the aircraft sprite with a special remap filter.
            Point drawOffset = owner.mDrawOffset.getOffset(1, 2); //Offset at ground i.e. landed.
            T spr = createAddSprite(owner, owner.mFileShp, owner.mFrameIndex, drawOffset, create);
            spr.mDrawMode = DrawMode.AircraftShadow;
            spr.setPriorityJustUnder(owner);
            spr.mIsSpecialEffect = true;
            return spr;
        }

        public bool IsSpecialEffect
        {
            get { return mIsSpecialEffect; }
        }

        protected static Dir8Way toDir8Way(string direction)
        {
            //Same in both Tiberian Dawn and Red Alert.
            byte dirVal = toDirection(direction);
            //Transform 256 directions to 8. Invert value also to match frame index.
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

        protected static int getFrameIndexDefault(Dir8Way direction)
        {
            //Units usually have 32 frames of rotation, but direction value only selects 8 ways. Checked in game.
            //Same in both Tiberian Dawn and Red Alert.
            switch (direction)
            {
                case Dir8Way.North: return 0;
                case Dir8Way.Northwest: return 3;
                case Dir8Way.West: return 8;
                case Dir8Way.Southwest: return 13;
                case Dir8Way.South: return 16;
                case Dir8Way.Southeast: return 19;
                case Dir8Way.East: return 24;
                case Dir8Way.Northeast: return 29;
                default: throw new ArgumentException(); //Should never happen.
            }
        }

        protected static Point getDrawOffsetRamp(Dir8Way direction)
        {
            //Draw offset for additional turret sprites. Add owner's draw offset to returned value. Checked in game.
            //Same in both Tiberian Dawn and Red Alert.
            switch (direction)
            {
                case Dir8Way.North: return toDrawOffset(1, 2);
                case Dir8Way.Northwest: return toDrawOffset(2, 0);
                case Dir8Way.West: return toDrawOffset(6, -3);
                case Dir8Way.Southwest: return toDrawOffset(3, -5);
                case Dir8Way.South: return toDrawOffset(0, -5);
                case Dir8Way.Southeast: return toDrawOffset(-3, -5);
                case Dir8Way.East: return toDrawOffset(-5, -3);
                case Dir8Way.Northeast: return toDrawOffset(-3, 0);
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}
