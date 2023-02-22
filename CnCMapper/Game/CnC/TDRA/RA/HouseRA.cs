using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class HouseRA : HouseTDRA
    {
        //New houses in Red Alert.
        public const string IdEngland = "England";
        public const string IdGermany = "Germany";
        public const string IdFrance = "France";
        public const string IdUkraine = "Ukraine";
        public const string IdUSSR = "USSR";
        public const string IdGreece = "Greece";
        public const string IdTurkey = "Turkey";
        public const string IdSpain = "Spain";
        public const string IdMulti7 = "Multi7";
        public const string IdMulti8 = "Multi8";

        private readonly ColorSchemeRA mColorScheme;

        private HouseRA(string id, ColorSchemeRA colorScheme)
            : base(id)
        {
            mColorScheme = colorScheme;
        }

        public override byte[] ColorRemap
        {
            get { return mColorScheme.Remap; }
        }

        public override byte RadarIndex
        {
            get { return mColorScheme.RadarIndex; }
        }

        public override byte RadarBrightIndex
        {
            get { return mColorScheme.RadarBrightIndex; }
        }

        public static readonly HouseRA Spain = new HouseRA(IdSpain, ColorSchemeRA.Gold); //Gold (unremapped).
        public static readonly HouseRA Greece = new HouseRA(IdGreece, ColorSchemeRA.LightBlue); //LtBlue.
        public static readonly HouseRA USSR = new HouseRA(IdUSSR, ColorSchemeRA.Red); //Red.
        public static readonly HouseRA England = new HouseRA(IdEngland, ColorSchemeRA.Green); //Green.
        public static readonly HouseRA Ukraine = new HouseRA(IdUkraine, ColorSchemeRA.Orange); //Orange.
        public static readonly HouseRA Germany = new HouseRA(IdGermany, ColorSchemeRA.Grey); //Grey.
        public static readonly HouseRA France = new HouseRA(IdFrance, ColorSchemeRA.Blue); //Blue.
        public static readonly HouseRA Turkey = new HouseRA(IdTurkey, ColorSchemeRA.Brown); //Brown.
        public static readonly HouseRA GoodGuy = new HouseRA(IdGoodGuy, ColorSchemeRA.LightBlue); //Global Defense Initiative.
        public static readonly HouseRA BadGuy = new HouseRA(IdBadGuy, ColorSchemeRA.Red); //Brotherhood of Nod.
        public static readonly HouseRA Neutral = new HouseRA(IdNeutral, ColorSchemeRA.Gold); //Civilians.
        public static readonly HouseRA Special = new HouseRA(IdSpecial, ColorSchemeRA.Gold); //(JP) Disaster Containment Team.
        public static readonly HouseRA Multi1 = new HouseRA(IdMulti1, ColorSchemeRA.Gold); //Multi-Player house #1.
        public static readonly HouseRA Multi2 = new HouseRA(IdMulti2, ColorSchemeRA.LightBlue); //Multi-Player house #2.
        public static readonly HouseRA Multi3 = new HouseRA(IdMulti3, ColorSchemeRA.Red); //Multi-Player house #3.
        public static readonly HouseRA Multi4 = new HouseRA(IdMulti4, ColorSchemeRA.Green); //Multi-Player house #4.
        public static readonly HouseRA Multi5 = new HouseRA(IdMulti5, ColorSchemeRA.Orange); //Multi-Player house #5.
        public static readonly HouseRA Multi6 = new HouseRA(IdMulti6, ColorSchemeRA.Grey); //Multi-Player house #6.
        public static readonly HouseRA Multi7 = new HouseRA(IdMulti7, ColorSchemeRA.Blue); //Multi-Player house #7.
        public static readonly HouseRA Multi8 = new HouseRA(IdMulti8, ColorSchemeRA.Brown); //Multi-Player house #8.

        private static readonly Dictionary<string, HouseRA> Undefined = new Dictionary<string, HouseRA>(); //Undefined houses encountered.

        public static HouseRA create(string id)
        {
            switch (id)
            {
                case IdSpain: return Spain;
                case IdGreece: return Greece;
                case IdUSSR: return USSR;
                case IdEngland: return England;
                case IdUkraine: return Ukraine;
                case IdGermany: return Germany;
                case IdFrance: return France;
                case IdTurkey: return Turkey;
                case IdGoodGuy: return GoodGuy;
                case IdBadGuy: return BadGuy;
                case IdNeutral: return Neutral;
                case IdSpecial: return Special;
                case IdMulti1: return Multi1;
                case IdMulti2: return Multi2;
                case IdMulti3: return Multi3;
                case IdMulti4: return Multi4;
                case IdMulti5: return Multi5;
                case IdMulti6: return Multi6;
                case IdMulti7: return Multi7;
                case IdMulti8: return Multi8;
                default:
                    HouseRA house;
                    if (!Undefined.TryGetValue(id, out house)) //Already encountered?
                    {
                        Program.warn(string.Format("Undefined house id '{0}'!", id));
                        house = new HouseRA(id, ColorSchemeRA.Neutral);
                        Undefined.Add(id, house);
                    }
                    return house;
            }
        }
    }
}
