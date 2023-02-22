using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class ConfigD2 : ConfigCnC
    {
        private const string FileConfigId = "DuneII";

        private readonly bool mLimitRemapRange;
        private const string LimitRemapRangeKey = "LimitRemapRange";
        [Option(Group.sprite, LimitRemapRangeKey, typeof(Answer), "Limit remap range which fixes the IX house color bug.")]
        public bool LimitRemapRange { get { return mLimitRemapRange; } }

        private readonly int[] mRadarScales;
        private const string RadarScalesKey = "RadarScales";
        [Option(Group.radar, RadarScalesKey, RadarD2.ConfigScaleValues, "", "Scales to render radar at. Leave empty to disable radar rendering.")]
        public int[] RadarScales { get { return mRadarScales; } }

        private ConfigD2(IniSection iniSection)
            : base(iniSection)
        {
            mLimitRemapRange = iniSection.getKey(LimitRemapRangeKey).valueAsBoolAnswer();
            mRadarScales = RadarD2.getConfigScales(iniSection.getKey(RadarScalesKey));
        }

        public static ConfigD2 create() //Use default config file.
        {
            return create(new FileIni(Program.ProgramPath + FileConfigId + ".ini"));
        }

        public static ConfigD2 create(FileIni fileConfig)
        {
            checkIsFileConfig(fileConfig, FileConfigId);
            return new ConfigD2(fileConfig.Sections[0]);
        }

        public static void printDefault(string configPath)
        {
            printDefault(configPath, FileConfigId, typeof(ConfigD2));
        }

        public static bool isFileConfig(FileIni fileConfig)
        {
            return isFileConfig(fileConfig, FileConfigId);
        }
    }
}
