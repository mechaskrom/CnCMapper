using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    //Dune 2 pseudo random number generator.
    class RandomD2
    {
        private readonly byte[] mSeedBytes; //Only lower 3 bytes of seed value is used.

        public RandomD2(int seedValue)
        {
            mSeedBytes = new byte[] { (byte)(seedValue >> 0), (byte)(seedValue >> 8), (byte)(seedValue >> 16) };
        }

        public RandomD2(RandomD2 copyFrom)
        {
            mSeedBytes = copyFrom.mSeedBytes.takeBytes();
        }

        //Return a random number between 0-255 and update seed bytes.
        public int get()
        {
            byte s0 = mSeedBytes[0];
            byte s1 = mSeedBytes[1];
            byte s2 = mSeedBytes[2];
            mSeedBytes[0] = (byte)(~(s0 >> 2 ^ s0 ^ s1 >> 7) << 7 | s0 >> 1);
            mSeedBytes[1] = (byte)(s1 << 1 | s2 >> 7);
            mSeedBytes[2] = (byte)(s2 << 1 | s0 >> 1 & 1);
            return mSeedBytes[0] ^ mSeedBytes[1];
        }
    }
}
