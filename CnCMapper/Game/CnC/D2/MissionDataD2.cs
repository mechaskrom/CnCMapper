using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    //Interesting list of fixed errors in mission INI-files: https://forum.dune2k.com/topic/20168-dune-2-v107-fix/
    //-Fixed invalid spice bloom placement in SCENA005. [*1]
    //-Removed junk lines (probably obsolete map data) from SCENH002 and SCENH007.
    //-Fixed incorrect hit point values for pre-placed buildings in SCENH006.
    //-Reinstated the sandworms that were commented out in SCENH008. [*2]
    //-Removed duplicate Spice Silo and Light Factory structures in SCENO005 (two identical structures were positioned at the same coordinates).

    //I checked these errors and they were the same in the version 1.00 and version 1.07 of the game I have.
    //Except the sandworms [*2] which were only commented out in version 1.07.
    //Good example of weird things in INI-files that this program must handle.

    //The spice bloom tile in SCENA005 [*1] is actually added. If triggered it will turn into a rock tile and create
    //a spice field around it on the few tiles that can become spice (the nearby sand, west and south of it).

    //I saved all map images from version 1.00 and 1.07 and then compared them visually. Version 1.07 changes:
    //-SCENA002: A construction yard added in the Ordos base.
    //-SCENA003: A construction yard added in the Ordos base.
    //-SCENA004: A construction yard added in the Ordos base.
    //-SCENA011: 4 units removed in the Ordos base.
    //-SCENA012: 5 units removed in the Ordos base.
    //-SCENA013: 8 units removed in the Ordos base.
    //-SCENH002: A windtrap moved and a construction yard added to its old location in the Atreides base.
    //-SCENH003: A windtrap and a barracks moved and a construction yard added in the Atreides base.
    //-SCENH004: A construction yard added in the Atreides base.
    //-SCENH008: 3 sandworms removed. Many small layout changes in the Ordos base.
    //-SCENH011: 13 units removed and 1 unit added in the Atreides base.
    //-SCENH012: 12 units removed in the Atreides base.
    //-SCENH013: 11 units removed in the Atreides base.
    //-SCENH014: A starport and a long wall removed in the Ordos base.
    //-SCENO002: A construction yard added in the Harkonnen base.
    //-SCENO003: A construction yard added in the Harkonnen base.
    //-SCENO004: A construction yard added in the Harkonnen base.
    //-SCENO011: 9 units removed in the Harkonnen base.
    //-SCENO012: 6 units removed in the Harkonnen base.
    //-SCENO013: 10 units removed in the Harkonnen base.
    //-SCENO014: 2 rocket turrets added and some small layout changes in the Atreides base (High tech factory no longer partially outside map borders).

    class MissionDataD2
    {
        private static readonly char[] InvalidChars = System.IO.Path.GetInvalidFileNameChars();

        private const string GameName = "Dune II: The Building of a Dynasty / Battle for Arrakis";

        private const string MissionAtreides = HouseD2.IdAtreides;
        private const string MissionHarkonnen = HouseD2.IdHarkonnen;
        private const string MissionOrdos = HouseD2.IdOrdos;

        private readonly string mMission; //House campaign.
        private readonly int mLevel;
        private readonly char mVariant;

        private MissionDataD2(string mission, int level, char variant)
        {
            mMission = mission;
            mLevel = level;
            mVariant = variant;
        }

        public static string getHeader(FileIni fileIni)
        {
            return getHeader(GameName, fileIni, getData(fileIni.Id));
        }

        public static string getFileName(FileIni fileIni)
        {
            return getFileName(fileIni, getData(fileIni.Id));
        }

        protected static string getHeader(string gameName, FileIni fileIni, MissionDataD2 md)
        {
            string header = gameName + "\n" + fileIni.Id;
            if (md != null)
            {
                header += string.Format(" - {0} {1}{2}", md.mMission, md.mLevel, md.mVariant);
            }
            return header;
        }

        protected static string getFileName(FileIni fileIni, MissionDataD2 md)
        {
            string fileName;
            if (md != null)
            {
                fileName = string.Format("{0} - {1} - {2}{3}", md.mMission, fileIni.Id, md.mLevel, md.mVariant);
            }
            else //Unknown INI-file.
            {
                fileName = fileIni.Id;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in fileName)
            {
                if (InvalidChars.Contains(c)) //Replace invalid chars.
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static MissionDataD2 getData(string mapId)
        {
            //Dune 2 mission INI-file name format: SCENhxxx.
            // h=house [A,H,O].
            // x=number [001-022].
            if (mapId.Length == 8 && mapId.StartsWith("SCEN"))
            {
                string mission;
                if (mapId[4] == 'A') mission = MissionAtreides;
                else if (mapId[4] == 'H') mission = MissionHarkonnen;
                else if (mapId[4] == 'O') mission = MissionOrdos;
                else return null; //Unknown INI-file.

                int number;
                if (!int.TryParse(mapId.Substring(5, 3), out number))
                {
                    return null; //Unknown INI-file.
                }

                //Figure out which level and variant this number corresponds to.
                //For level 1 and 9 there is only one variant.
                //Level 8 has two and the rest, level 2-7, have three.
                int level;
                int variant = 0;
                if (number == 1) //Level 1.
                {
                    level = 1;
                }
                else if (number >= 2 && number <= 21) //Level 2-8.
                {
                    level = (number + 4) / 3;
                    variant = (number + 4) % 3;
                }
                else if (number == 22) //Level 9.
                {
                    level = 9;
                }
                else
                {
                    return null; //Unknown INI-file.
                }

                return new MissionDataD2(mission, level, (char)('A' + variant));
            }
            return null;

            //Number      Level      Variant
            //1           1          a
            //2           2          a
            //3           2          b
            //4           2          c
            //5           3          a
            //6           3          b
            //7           3          c
            //8           4          a
            //9           4          b
            //10          4          c
            //11          5          a
            //12          5          b
            //13          5          c
            //14          6          a
            //15          6          b
            //16          6          c
            //17          7          a
            //18          7          b
            //19          7          c
            //20          8          a
            //21          8          b
            //22          9          a
        }
    }
}
