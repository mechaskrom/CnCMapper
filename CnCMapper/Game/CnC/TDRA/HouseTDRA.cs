using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA
{
    abstract class HouseTDRA
    {
        //Houses both in Tiberian Dawn and Red Alert.
        public const string IdGoodGuy = "GoodGuy";
        public const string IdBadGuy = "BadGuy";
        public const string IdNeutral = "Neutral";
        public const string IdSpecial = "Special";
        public const string IdMulti1 = "Multi1";
        public const string IdMulti2 = "Multi2";
        public const string IdMulti3 = "Multi3";
        public const string IdMulti4 = "Multi4";
        public const string IdMulti5 = "Multi5";
        public const string IdMulti6 = "Multi6";

        protected readonly string mId;

        protected HouseTDRA(string id)
        {
            mId = id;
        }

        public string Id
        {
            get { return mId; }
        }

        public abstract byte[] ColorRemap { get; }
        public virtual byte[] ColorRemapAlt { get { return ColorRemap; } } //For structures and HARV and MCV units in Tiberian Dawn.
        public abstract byte RadarIndex { get; }
        public abstract byte RadarBrightIndex { get; }
    }
}
