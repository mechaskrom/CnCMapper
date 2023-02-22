using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.D2
{
    class HouseD2
    {
        //Houses in Dune 2.
        public const string IdAtreides = "Atreides";
        public const string IdFremen = "Fremen";
        public const string IdHarkonnen = "Harkonnen";
        public const string IdMercenary = "Mercenary";
        public const string IdOrdos = "Ordos";
        public const string IdSardaukar = "Sardaukar";

        private readonly string mId;
        private readonly ColorSchemeD2 mColorScheme;

        private HouseD2(string id, ColorSchemeD2 colorScheme)
        {
            mId = id;
            mColorScheme = colorScheme;
        }

        public string Id
        {
            get { return mId; }
        }

        public byte[] RemapUnit
        {
            get { return mColorScheme.RemapUnit; }
        }

        public byte[] RemapStructure
        {
            get { return mColorScheme.RemapStructure; }
        }

        public byte RadarIndex
        {
            get { return mColorScheme.RadarIndex; }
        }

        public static readonly HouseD2 Atreides = new HouseD2(IdAtreides, ColorSchemeD2.Blue);
        public static readonly HouseD2 Fremen = new HouseD2(IdFremen, ColorSchemeD2.Orange);
        public static readonly HouseD2 Harkonnen = new HouseD2(IdHarkonnen, ColorSchemeD2.Red);
        public static readonly HouseD2 Mercenary = new HouseD2(IdMercenary, ColorSchemeD2.Brown);
        public static readonly HouseD2 Ordos = new HouseD2(IdOrdos, ColorSchemeD2.Green);
        public static readonly HouseD2 Sardaukar = new HouseD2(IdSardaukar, ColorSchemeD2.Purple);

        private static readonly Dictionary<string, HouseD2> Undefined = new Dictionary<string, HouseD2>(); //Undefined houses encountered.

        public static HouseD2 create(string id)
        {
            switch (id)
            {
                case IdAtreides: return Atreides;
                case IdFremen: return Fremen;
                case IdHarkonnen: return Harkonnen;
                case IdMercenary: return Mercenary;
                case IdOrdos: return Ordos;
                case IdSardaukar: return Sardaukar;
                default:
                    HouseD2 house;
                    if (!Undefined.TryGetValue(id, out house)) //Already encountered?
                    {
                        Program.warn(string.Format("Undefined house id '{0}'!", id));
                        house = new HouseD2(id, ColorSchemeD2.Neutral);
                        Undefined.Add(id, house);
                    }
                    return house;
            }
        }
    }
}
