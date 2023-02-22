using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class HouseTD : HouseTDRA
    {
        private readonly ColorSchemeTD mColorScheme;
        private readonly ColorSchemeTD mColorSchemeAlt; //Used only by BadGuy. Only in singleplayer and not multiplayer?
        private ColorSchemeTD.Radar mColorSchemeRadar;

        private HouseTD(string id, ColorSchemeTD colorScheme, ColorSchemeTD.Radar colorSchemeRadar)
            : this(id, colorScheme, colorScheme, colorSchemeRadar)
        {
        }

        private HouseTD(string id, ColorSchemeTD colorScheme, ColorSchemeTD colorSchemeAlt, ColorSchemeTD.Radar colorSchemeRadar)
            : base(id)
        {
            mColorScheme = colorScheme;
            mColorSchemeAlt = colorSchemeAlt;
            mColorSchemeRadar = colorSchemeRadar;
        }

        public override byte[] ColorRemap
        {
            get { return mColorScheme.Remap; }
        }

        public override byte[] ColorRemapAlt
        {
            get
            {
                //BadGuy uses an alternative color on structures and HARV and MCV units. Checked in game.
                //I've checked all houses on most sprites and it seems that BadGuy is
                //the only one that uses an alternative color on some sprites.
                return mColorSchemeAlt.Remap;

                //Looking at the source code it seems like this alternative color is used only in singleplayer?
                //BadGuy uses its normal color remap (LightBlue) in multiplayer? See "Remap_Table()" in "HOUSE.CPP".
                //Doesn't really matter for this program though because multiplayer maps usually starts with
                //no house owned objects (structures, units, etc).
            }
        }

        public override byte RadarIndex
        {
            get { return mColorSchemeRadar.Index; }
        }

        public override byte RadarBrightIndex
        {
            get { return mColorSchemeRadar.BrightIndex; }
        }

        public static readonly HouseTD GoodGuy = new HouseTD(IdGoodGuy, ColorSchemeTD.Gold, ColorSchemeTD.Radar.Gold); //Global Defense Initiative.
        public static readonly HouseTD BadGuy = new HouseTD(IdBadGuy, ColorSchemeTD.LightBlue, ColorSchemeTD.Red, ColorSchemeTD.Radar.Red); //Brotherhood of Nod.
        public static readonly HouseTD Neutral = new HouseTD(IdNeutral, ColorSchemeTD.Gold, ColorSchemeTD.Radar.Grey); //Civilians.
        public static readonly HouseTD Special = new HouseTD(IdSpecial, ColorSchemeTD.Gold, ColorSchemeTD.Radar.Grey); //(JP) Disaster Containment Team.
        public static readonly HouseTD Multi1 = new HouseTD(IdMulti1, ColorSchemeTD.Blue, ColorSchemeTD.Radar.Grey); //Multi-Player house #1.
        public static readonly HouseTD Multi2 = new HouseTD(IdMulti2, ColorSchemeTD.Orange, ColorSchemeTD.Radar.Grey); //Multi-Player house #2.
        public static readonly HouseTD Multi3 = new HouseTD(IdMulti3, ColorSchemeTD.Green, ColorSchemeTD.Radar.Grey); //Multi-Player house #3.
        public static readonly HouseTD Multi4 = new HouseTD(IdMulti4, ColorSchemeTD.LightBlue, ColorSchemeTD.Radar.Grey); //Multi-Player house #4.
        public static readonly HouseTD Multi5 = new HouseTD(IdMulti5, ColorSchemeTD.Gold, ColorSchemeTD.Radar.Grey); //Multi-Player house #5.
        public static readonly HouseTD Multi6 = new HouseTD(IdMulti6, ColorSchemeTD.Red, ColorSchemeTD.Radar.Grey); //Multi-Player house #6.

        private static readonly Dictionary<string, HouseTD> Undefined = new Dictionary<string, HouseTD>(); //Undefined houses encountered.

        public static HouseTD create(string id)
        {
            switch (id)
            {
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
                default:
                    HouseTD house;
                    if (!Undefined.TryGetValue(id, out house)) //Already encountered?
                    {
                        Program.warn(string.Format("Undefined house id '{0}'!", id));
                        house = new HouseTD(id, ColorSchemeTD.Neutral, ColorSchemeTD.Radar.Grey);
                        Undefined.Add(id, house);
                    }
                    return house;
            }
        }

        private static ColorSchemeTD.Radar.Mode mRadarColorMode = ColorSchemeTD.Radar.Mode.Default;

        public static void setRadarColorMode(ColorSchemeTD.Radar.Mode radarColorMode)
        {
            //Switches between a few different radar color modes.
            if (mRadarColorMode != radarColorMode) //Current mode changed?
            {
                mRadarColorMode = radarColorMode;

                //Custom, more colorful, radar color schemes for multiplayer houses.
                if (radarColorMode == ColorSchemeTD.Radar.Mode.CustomMulti) //Use custom colors?
                {
                    Multi1.mColorSchemeRadar = ColorSchemeTD.Radar.Teal;
                    Multi2.mColorSchemeRadar = ColorSchemeTD.Radar.Orange;
                    Multi3.mColorSchemeRadar = ColorSchemeTD.Radar.Green;
                    Multi4.mColorSchemeRadar = ColorSchemeTD.Radar.LightGrey;
                    Multi5.mColorSchemeRadar = ColorSchemeTD.Radar.Yellow;
                    Multi6.mColorSchemeRadar = ColorSchemeTD.Radar.Red;
                }
                else //Default ingame. All grey.
                {
                    Multi1.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                    Multi2.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                    Multi3.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                    Multi4.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                    Multi5.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                    Multi6.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                }

                //House "Special" uses a red radar color scheme in the hidden funpark jurassic missions. Checked game/source.
                if (radarColorMode == ColorSchemeTD.Radar.Mode.Jurassic) //Use jurassic color?
                {
                    Special.mColorSchemeRadar = ColorSchemeTD.Radar.Red;
                }
                else //Default ingame.
                {
                    Special.mColorSchemeRadar = ColorSchemeTD.Radar.Grey;
                }
            }
        }
    }
}
