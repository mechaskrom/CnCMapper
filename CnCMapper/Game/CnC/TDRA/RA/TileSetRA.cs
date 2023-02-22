using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //TODO: Remove Red Alert land type enum? Not currently used for anything.
    //enum LandType
    //{
    //    Clear,    //"Clear" terrain.
    //    Road,     //Road terrain.
    //    Water,    //Water.
    //    Rock,     //Impassable rock.
    //    Wall,     //Wall (blocks movement).
    //    Tiberium, //Tiberium field.
    //    Beach,    //Beach terrain.
    //    Rough,    //Rocky terrain.
    //    River,    //Rocky riverbed.
    //}

    static class TileSetRA
    {
        private const UInt16 TileSetIdMax = 400;

        public static void get(UInt16 tileSetId, out string fileId, out TheaterFlagsRA theaterFlags)
        {
            //Tile sets are only valid in specific theaters.
            //0 = INTERIOR, SNOW and TEMPERATE theater.
            //253-377 & 384-399 = INTERIOR theater (id:s with 4 letters + 4 digits).
            //400 = TEMPERATE theater.
            //Everything else = SNOW and TEMPERATE theater.
            //Nothing is drawn (Hall-Of-Mirrors effect) if not specified for theater. Checked in source.

            switch (tileSetId)
            {
                case 0: fileId = "CLEAR1"; theaterFlags = TheaterFlagsRA.AllDefined; break; //Clear.
                case 1: fileId = "W1"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Water.
                case 2: fileId = "W2"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Water2.
                case 3: fileId = "SH01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore01.
                case 4: fileId = "SH02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore02.
                case 5: fileId = "SH03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore03.
                case 6: fileId = "SH04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore04.
                case 7: fileId = "SH05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore05.
                case 8: fileId = "SH06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore06.
                case 9: fileId = "SH07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore07.
                case 10: fileId = "SH08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore08.
                case 11: fileId = "SH09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore09.
                case 12: fileId = "SH10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore10.
                case 13: fileId = "SH11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore11.
                case 14: fileId = "SH12"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore12.
                case 15: fileId = "SH13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore13.
                case 16: fileId = "SH14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore14.
                case 17: fileId = "SH15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore15.
                case 18: fileId = "SH16"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore16.
                case 19: fileId = "SH17"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore17.
                case 20: fileId = "SH18"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore18.
                case 21: fileId = "SH19"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore19.
                case 22: fileId = "SH20"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore20.
                case 23: fileId = "SH21"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore21.
                case 24: fileId = "SH22"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore22.
                case 25: fileId = "SH23"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore23.
                case 26: fileId = "SH24"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore24.
                case 27: fileId = "SH25"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore25.
                case 28: fileId = "SH26"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore26.
                case 29: fileId = "SH27"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore27.
                case 30: fileId = "SH28"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore28.
                case 31: fileId = "SH29"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore29.
                case 32: fileId = "SH30"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore30.
                case 33: fileId = "SH31"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore31.
                case 34: fileId = "SH32"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore32.
                case 35: fileId = "SH33"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore33.
                case 36: fileId = "SH34"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore34.
                case 37: fileId = "SH35"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore35.
                case 38: fileId = "SH36"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore36.
                case 39: fileId = "SH37"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore37.
                case 40: fileId = "SH38"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore38.
                case 41: fileId = "SH39"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore39.
                case 42: fileId = "SH40"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore40.
                case 43: fileId = "SH41"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore41.
                case 44: fileId = "SH42"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore42.
                case 45: fileId = "SH43"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore43.
                case 46: fileId = "SH44"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore44.
                case 47: fileId = "SH45"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore45.
                case 48: fileId = "SH46"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore46.
                case 49: fileId = "SH47"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore47.
                case 50: fileId = "SH48"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore48.
                case 51: fileId = "SH49"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore49.
                case 52: fileId = "SH50"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore50.
                case 53: fileId = "SH51"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore51.
                case 54: fileId = "SH52"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore52.
                case 55: fileId = "SH53"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore53.
                case 56: fileId = "SH54"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore54.
                case 57: fileId = "SH55"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore55.
                case 58: fileId = "SH56"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Shore56.
                case 59: fileId = "WC01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff01.
                case 60: fileId = "WC02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff02.
                case 61: fileId = "WC03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff03.
                case 62: fileId = "WC04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff04.
                case 63: fileId = "WC05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff05.
                case 64: fileId = "WC06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff06.
                case 65: fileId = "WC07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff07.
                case 66: fileId = "WC08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff08.
                case 67: fileId = "WC09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff09.
                case 68: fileId = "WC10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff10.
                case 69: fileId = "WC11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff11.
                case 70: fileId = "WC12"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff12.
                case 71: fileId = "WC13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff13.
                case 72: fileId = "WC14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff14.
                case 73: fileId = "WC15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff15.
                case 74: fileId = "WC16"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff16.
                case 75: fileId = "WC17"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff17.
                case 76: fileId = "WC18"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff18.
                case 77: fileId = "WC19"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff19.
                case 78: fileId = "WC20"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff20.
                case 79: fileId = "WC21"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff21.
                case 80: fileId = "WC22"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff22.
                case 81: fileId = "WC23"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff23.
                case 82: fileId = "WC24"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff24.
                case 83: fileId = "WC25"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff25.
                case 84: fileId = "WC26"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff26.
                case 85: fileId = "WC27"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff27.
                case 86: fileId = "WC28"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff28.
                case 87: fileId = "WC29"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff29.
                case 88: fileId = "WC30"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff30.
                case 89: fileId = "WC31"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff31.
                case 90: fileId = "WC32"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff32.
                case 91: fileId = "WC33"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff33.
                case 92: fileId = "WC34"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff34.
                case 93: fileId = "WC35"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff35.
                case 94: fileId = "WC36"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff36.
                case 95: fileId = "WC37"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff37.
                case 96: fileId = "WC38"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //ShoreCliff38.
                case 97: fileId = "B1"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder1.
                case 98: fileId = "B2"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder2.
                case 99: fileId = "B3"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder3.
                case 100: fileId = "B4"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder4.
                case 101: fileId = "B5"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder5.
                case 102: fileId = "B6"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Boulder6.
                case 103: fileId = "P01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch01.
                case 104: fileId = "P02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch02.
                case 105: fileId = "P03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch03.
                case 106: fileId = "P04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch04.
                case 107: fileId = "P07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch07.
                case 108: fileId = "P08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch08.
                case 109: fileId = "P13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch13.
                case 110: fileId = "P14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch14.
                case 111: fileId = "P15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Patch15.
                case 112: fileId = "RV01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River01.
                case 113: fileId = "RV02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River02.
                case 114: fileId = "RV03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River03.
                case 115: fileId = "RV04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River04.
                case 116: fileId = "RV05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River05.
                case 117: fileId = "RV06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River06.
                case 118: fileId = "RV07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River07.
                case 119: fileId = "RV08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River08.
                case 120: fileId = "RV09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River09.
                case 121: fileId = "RV10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River10.
                case 122: fileId = "RV11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River11.
                case 123: fileId = "RV12"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River12.
                case 124: fileId = "RV13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River13.
                case 125: fileId = "FALLS1"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Falls1.
                case 126: fileId = "FALLS1A"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Falls1a.
                case 127: fileId = "FALLS2"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Falls2.
                case 128: fileId = "FALLS2A"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Falls2a.
                case 129: fileId = "FORD1"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Ford1.
                case 130: fileId = "FORD2"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Ford2.
                case 131: fileId = "BRIDGE1"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1.
                case 132: fileId = "BRIDGE1D"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1d.
                case 133: fileId = "BRIDGE2"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2.
                case 134: fileId = "BRIDGE2D"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2d.
                case 135: fileId = "S01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope01.
                case 136: fileId = "S02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope02.
                case 137: fileId = "S03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope03.
                case 138: fileId = "S04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope04.
                case 139: fileId = "S05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope05.
                case 140: fileId = "S06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope06.
                case 141: fileId = "S07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope07.
                case 142: fileId = "S08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope08.
                case 143: fileId = "S09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope09.
                case 144: fileId = "S10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope10.
                case 145: fileId = "S11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope11.
                case 146: fileId = "S12"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope12.
                case 147: fileId = "S13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope13.
                case 148: fileId = "S14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope14.
                case 149: fileId = "S15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope15.
                case 150: fileId = "S16"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope16.
                case 151: fileId = "S17"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope17.
                case 152: fileId = "S18"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope18.
                case 153: fileId = "S19"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope19.
                case 154: fileId = "S20"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope20.
                case 155: fileId = "S21"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope21.
                case 156: fileId = "S22"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope22.
                case 157: fileId = "S23"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope23.
                case 158: fileId = "S24"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope24.
                case 159: fileId = "S25"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope25.
                case 160: fileId = "S26"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope26.
                case 161: fileId = "S27"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope27.
                case 162: fileId = "S28"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope28.
                case 163: fileId = "S29"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope29.
                case 164: fileId = "S30"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope30.
                case 165: fileId = "S31"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope31.
                case 166: fileId = "S32"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope32.
                case 167: fileId = "S33"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope33.
                case 168: fileId = "S34"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope34.
                case 169: fileId = "S35"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope35.
                case 170: fileId = "S36"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope36.
                case 171: fileId = "S37"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope37.
                case 172: fileId = "S38"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Slope38.
                case 173: fileId = "D01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road01.
                case 174: fileId = "D02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road02.
                case 175: fileId = "D03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road03.
                case 176: fileId = "D04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road04.
                case 177: fileId = "D05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road05.
                case 178: fileId = "D06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road06.
                case 179: fileId = "D07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road07.
                case 180: fileId = "D08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road08.
                case 181: fileId = "D09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road09.
                case 182: fileId = "D10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road10.
                case 183: fileId = "D11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road11.
                case 184: fileId = "D12"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road12.
                case 185: fileId = "D13"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road13.
                case 186: fileId = "D14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road14.
                case 187: fileId = "D15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road15.
                case 188: fileId = "D16"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road16.
                case 189: fileId = "D17"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road17.
                case 190: fileId = "D18"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road18.
                case 191: fileId = "D19"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road19.
                case 192: fileId = "D20"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road20.
                case 193: fileId = "D21"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road21.
                case 194: fileId = "D22"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road22.
                case 195: fileId = "D23"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road23.
                case 196: fileId = "D24"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road24.
                case 197: fileId = "D25"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road25.
                case 198: fileId = "D26"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road26.
                case 199: fileId = "D27"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road27.
                case 200: fileId = "D28"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road28.
                case 201: fileId = "D29"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road29.
                case 202: fileId = "D30"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road30.
                case 203: fileId = "D31"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road31.
                case 204: fileId = "D32"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road32.
                case 205: fileId = "D33"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road33.
                case 206: fileId = "D34"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road34.
                case 207: fileId = "D35"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road35.
                case 208: fileId = "D36"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road36.
                case 209: fileId = "D37"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road37.
                case 210: fileId = "D38"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road38.
                case 211: fileId = "D39"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road39.
                case 212: fileId = "D40"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road40.
                case 213: fileId = "D41"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road41.
                case 214: fileId = "D42"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road42.
                case 215: fileId = "D43"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road43.
                case 216: fileId = "RF01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough01.
                case 217: fileId = "RF02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough02.
                case 218: fileId = "RF03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough03.
                case 219: fileId = "RF04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough04.
                case 220: fileId = "RF05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough05.
                case 221: fileId = "RF06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough06.
                case 222: fileId = "RF07"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough07.
                case 223: fileId = "RF08"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough08.
                case 224: fileId = "RF09"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough09.
                case 225: fileId = "RF10"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough10.
                case 226: fileId = "RF11"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Rough11.
                case 227: fileId = "D44"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road44.
                case 228: fileId = "D45"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Road45.
                case 229: fileId = "RV14"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River14.
                case 230: fileId = "RV15"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //River15.
                case 231: fileId = "RC01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //RiverCliff01.
                case 232: fileId = "RC02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //RiverCliff02.
                case 233: fileId = "RC03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //RiverCliff03.
                case 234: fileId = "RC04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //RiverCliff04.
                case 235: fileId = "BR1A"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1a.
                case 236: fileId = "BR1B"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1b.
                case 237: fileId = "BR1C"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1c.
                case 238: fileId = "BR2A"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2a.
                case 239: fileId = "BR2B"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2b.
                case 240: fileId = "BR2C"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2c.
                case 241: fileId = "BR3A"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3a.
                case 242: fileId = "BR3B"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3b.
                case 243: fileId = "BR3C"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3c.
                case 244: fileId = "BR3D"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3d.
                case 245: fileId = "BR3E"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3e.
                case 246: fileId = "BR3F"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge3f.
                case 247: fileId = "F01"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F01.
                case 248: fileId = "F02"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F02.
                case 249: fileId = "F03"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F03.
                case 250: fileId = "F04"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F04.
                case 251: fileId = "F05"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F05.
                case 252: fileId = "F06"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //F06.
                case 253: fileId = "ARRO0001"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0001.
                case 254: fileId = "ARRO0002"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0002.
                case 255: fileId = "ARRO0003"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0003.
                case 256: fileId = "ARRO0004"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0004.
                case 257: fileId = "ARRO0005"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0005.
                case 258: fileId = "ARRO0006"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0006.
                case 259: fileId = "ARRO0007"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0007.
                case 260: fileId = "ARRO0008"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0008.
                case 261: fileId = "ARRO0009"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0009.
                case 262: fileId = "ARRO0010"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0010.
                case 263: fileId = "ARRO0011"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0011.
                case 264: fileId = "ARRO0012"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0012.
                case 265: fileId = "ARRO0013"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0013.
                case 266: fileId = "ARRO0014"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0014.
                case 267: fileId = "ARRO0015"; theaterFlags = TheaterFlagsRA.Interior; break; //ARRO0015.
                case 268: fileId = "FLOR0001"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0001.
                case 269: fileId = "FLOR0002"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0002.
                case 270: fileId = "FLOR0003"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0003.
                case 271: fileId = "FLOR0004"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0004.
                case 272: fileId = "FLOR0005"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0005.
                case 273: fileId = "FLOR0006"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0006.
                case 274: fileId = "FLOR0007"; theaterFlags = TheaterFlagsRA.Interior; break; //FLOR0007.
                case 275: fileId = "GFLR0001"; theaterFlags = TheaterFlagsRA.Interior; break; //GFLR0001.
                case 276: fileId = "GFLR0002"; theaterFlags = TheaterFlagsRA.Interior; break; //GFLR0002.
                case 277: fileId = "GFLR0003"; theaterFlags = TheaterFlagsRA.Interior; break; //GFLR0003.
                case 278: fileId = "GFLR0004"; theaterFlags = TheaterFlagsRA.Interior; break; //GFLR0004.
                case 279: fileId = "GFLR0005"; theaterFlags = TheaterFlagsRA.Interior; break; //GFLR0005.
                case 280: fileId = "GSTR0001"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0001.
                case 281: fileId = "GSTR0002"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0002.
                case 282: fileId = "GSTR0003"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0003.
                case 283: fileId = "GSTR0004"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0004.
                case 284: fileId = "GSTR0005"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0005.
                case 285: fileId = "GSTR0006"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0006.
                case 286: fileId = "GSTR0007"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0007.
                case 287: fileId = "GSTR0008"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0008.
                case 288: fileId = "GSTR0009"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0009.
                case 289: fileId = "GSTR0010"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0010.
                case 290: fileId = "GSTR0011"; theaterFlags = TheaterFlagsRA.Interior; break; //GSTR0011.
                case 291: fileId = "LWAL0001"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0001.
                case 292: fileId = "LWAL0002"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0002.
                case 293: fileId = "LWAL0003"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0003.
                case 294: fileId = "LWAL0004"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0004.
                case 295: fileId = "LWAL0005"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0005.
                case 296: fileId = "LWAL0006"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0006.
                case 297: fileId = "LWAL0007"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0007.
                case 298: fileId = "LWAL0008"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0008.
                case 299: fileId = "LWAL0009"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0009.
                case 300: fileId = "LWAL0010"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0010.
                case 301: fileId = "LWAL0011"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0011.
                case 302: fileId = "LWAL0012"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0012.
                case 303: fileId = "LWAL0013"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0013.
                case 304: fileId = "LWAL0014"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0014.
                case 305: fileId = "LWAL0015"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0015.
                case 306: fileId = "LWAL0016"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0016.
                case 307: fileId = "LWAL0017"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0017.
                case 308: fileId = "LWAL0018"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0018.
                case 309: fileId = "LWAL0019"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0019.
                case 310: fileId = "LWAL0020"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0020.
                case 311: fileId = "LWAL0021"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0021.
                case 312: fileId = "LWAL0022"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0022.
                case 313: fileId = "LWAL0023"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0023.
                case 314: fileId = "LWAL0024"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0024.
                case 315: fileId = "LWAL0025"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0025.
                case 316: fileId = "LWAL0026"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0026.
                case 317: fileId = "LWAL0027"; theaterFlags = TheaterFlagsRA.Interior; break; //LWAL0027.
                case 318: fileId = "STRP0001"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0001.
                case 319: fileId = "STRP0002"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0002.
                case 320: fileId = "STRP0003"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0003.
                case 321: fileId = "STRP0004"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0004.
                case 322: fileId = "STRP0005"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0005.
                case 323: fileId = "STRP0006"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0006.
                case 324: fileId = "STRP0007"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0007.
                case 325: fileId = "STRP0008"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0008.
                case 326: fileId = "STRP0009"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0009.
                case 327: fileId = "STRP0010"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0010.
                case 328: fileId = "STRP0011"; theaterFlags = TheaterFlagsRA.Interior; break; //STRP0011.
                case 329: fileId = "WALL0001"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0001.
                case 330: fileId = "WALL0002"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0002.
                case 331: fileId = "WALL0003"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0003.
                case 332: fileId = "WALL0004"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0004.
                case 333: fileId = "WALL0005"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0005.
                case 334: fileId = "WALL0006"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0006.
                case 335: fileId = "WALL0007"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0007.
                case 336: fileId = "WALL0008"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0008.
                case 337: fileId = "WALL0009"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0009.
                case 338: fileId = "WALL0010"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0010.
                case 339: fileId = "WALL0011"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0011.
                case 340: fileId = "WALL0012"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0012.
                case 341: fileId = "WALL0013"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0013.
                case 342: fileId = "WALL0014"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0014.
                case 343: fileId = "WALL0015"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0015.
                case 344: fileId = "WALL0016"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0016.
                case 345: fileId = "WALL0017"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0017.
                case 346: fileId = "WALL0018"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0018.
                case 347: fileId = "WALL0019"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0019.
                case 348: fileId = "WALL0020"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0020.
                case 349: fileId = "WALL0021"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0021.
                case 350: fileId = "WALL0022"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0022.
                case 351: fileId = "WALL0023"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0023.
                case 352: fileId = "WALL0024"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0024.
                case 353: fileId = "WALL0025"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0025.
                case 354: fileId = "WALL0026"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0026.
                case 355: fileId = "WALL0027"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0027.
                case 356: fileId = "WALL0028"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0028.
                case 357: fileId = "WALL0029"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0029.
                case 358: fileId = "WALL0030"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0030.
                case 359: fileId = "WALL0031"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0031.
                case 360: fileId = "WALL0032"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0032.
                case 361: fileId = "WALL0033"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0033.
                case 362: fileId = "WALL0034"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0034.
                case 363: fileId = "WALL0035"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0035.
                case 364: fileId = "WALL0036"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0036.
                case 365: fileId = "WALL0037"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0037.
                case 366: fileId = "WALL0038"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0038.
                case 367: fileId = "WALL0039"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0039.
                case 368: fileId = "WALL0040"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0040.
                case 369: fileId = "WALL0041"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0041.
                case 370: fileId = "WALL0042"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0042.
                case 371: fileId = "WALL0043"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0043.
                case 372: fileId = "WALL0044"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0044.
                case 373: fileId = "WALL0045"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0045.
                case 374: fileId = "WALL0046"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0046.
                case 375: fileId = "WALL0047"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0047.
                case 376: fileId = "WALL0048"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0048.
                case 377: fileId = "WALL0049"; theaterFlags = TheaterFlagsRA.Interior; break; //WALL0049.
                case 378: fileId = "BRIDGE1H"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1h.
                case 379: fileId = "BRIDGE2H"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2h.
                case 380: fileId = "BR1X"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1ax.
                case 381: fileId = "BR2X"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2ax.
                case 382: fileId = "BRIDGE1X"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge1x.
                case 383: fileId = "BRIDGE2X"; theaterFlags = TheaterFlagsRA.SnowTemperate; break; //Bridge2x.
                case 384: fileId = "XTRA0001"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0001.
                case 385: fileId = "XTRA0002"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0002.
                case 386: fileId = "XTRA0003"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0003.
                case 387: fileId = "XTRA0004"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0004.
                case 388: fileId = "XTRA0005"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0005.
                case 389: fileId = "XTRA0006"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0006.
                case 390: fileId = "XTRA0007"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0007.
                case 391: fileId = "XTRA0008"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0008.
                case 392: fileId = "XTRA0009"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0009.
                case 393: fileId = "XTRA0010"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0010.
                case 394: fileId = "XTRA0011"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0011.
                case 395: fileId = "XTRA0012"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0012.
                case 396: fileId = "XTRA0013"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0013.
                case 397: fileId = "XTRA0014"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0014.
                case 398: fileId = "XTRA0015"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0015.
                case 399: fileId = "XTRA0016"; theaterFlags = TheaterFlagsRA.Interior; break; //Xtra0016.
                case 400: fileId = "HILL01"; theaterFlags = TheaterFlagsRA.Temperate; break; //AntHill.
                default:
                    System.Diagnostics.Debug.Assert(tileSetId > TileSetIdMax); //Check that we didn't forget a switch case.
                    Program.warn(string.Format("Unknown tile set id value '{0}'!", tileSetId));
                    fileId = "unknown_tile_id_" + tileSetId.ToString(); theaterFlags = TheaterFlagsRA.AllDefined; break;
            }
        }
    }
}
