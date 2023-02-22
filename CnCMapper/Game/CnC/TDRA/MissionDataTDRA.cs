using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //Stores data about all known normal singleplayer missions in Tiberian Dawn and Red Alert.
    class MissionDataTDRA
    {
        private static readonly char[] InvalidChars = System.IO.Path.GetInvalidFileNameChars();

        protected readonly string mMission;
        protected readonly int mNumber;
        protected readonly string mName;
        protected readonly string mCity; //May be null (not present).
        protected readonly string mCountry; //May be null (not present).

        protected MissionDataTDRA(string mission, int number, string name, string city, string country)
        {
            mMission = mission;
            mNumber = number;
            mName = name;
            mCity = city;
            mCountry = country;
        }

        protected static string getHeader(string gameName, FileIni fileIni, MissionDataTDRA md, bool isTD)
        {
            string header = gameName + "\n" + fileIni.Id;
            if (md != null)
            {
                header += string.Format(" - {0} {1}, \"{2}\"", md.mMission, md.mNumber, md.mName);
                if (md.mCountry != null)
                {
                    header += ", " + md.mCountry;
                }
            }
            else //Unknown INI-file.
            {
                string internalName = getInternalName(fileIni, isTD);
                if (internalName != null)
                {
                    header += " - " + internalName;
                }
            }
            return header;
        }

        protected static string getFileName(FileIni fileIni, MissionDataTDRA md, bool isTD)
        {
            string fileName;
            if (md != null)
            {
                fileName = string.Format("{0} - {1} - {2}", md.mMission, fileIni.Id, md.mName);
                //Don't add country to file name? It's already too long as it is.
                //if (md.mCountry != null) //Add country if present to file name.
                //{
                //    fileName += " - " + md.mCountry;
                //}
            }
            else //Unknown INI-file.
            {
                fileName = fileIni.Id;
                string internalName = getInternalName(fileIni, isTD);
                if (internalName != null)
                {
                    fileName += " - " + internalName;
                }
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

        private static string getInternalName(FileIni fileIni, bool isTD)
        {
            //Check if INI-file contains name of map i.e. [Basic] section has a "Name" key?
            IniSection basicSection = isTD ? TD.MapTD.findBasicSection(fileIni) : fileIni.findSection("Basic");
            if (basicSection != null)
            {
                IniKey nameKey = basicSection.findKey("Name");
                if (nameKey != null)
                {
                    return nameKey.Value;
                }
            }
            return null;
        }
    }
}
