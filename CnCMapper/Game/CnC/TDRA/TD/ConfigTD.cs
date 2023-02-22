using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class ConfigTD : ConfigTDRA
    {
        private const string FileConfigId = "TiberianDawn";

        private readonly bool mAddBoatWakes;
        private const string AddBoatWakesKey = "AddBoatWakes";
        [Option(Group.sprite, AddBoatWakesKey, typeof(Answer), "Add patrolling gun boat wakes.")]
        public bool AddBoatWakes { get { return mAddBoatWakes; } }

        private readonly bool mExposeTiberiumTrees;
        private const string ExposeTiberiumTreesKey = "ExposeTiberiumTrees";
        [Option(Group.expose, ExposeTiberiumTreesKey, typeof(Answer), "Expose tiberium trees by setting them to a fully transformed state.")]
        public bool ExposeTiberiumTrees { get { return mExposeTiberiumTrees; } }

        private ConfigTD(IniSection iniSection)
            : base(iniSection)
        {
            mAddBoatWakes = iniSection.getKey(AddBoatWakesKey).valueAsBoolAnswer();
            mExposeTiberiumTrees = iniSection.getKey(ExposeTiberiumTreesKey).valueAsBoolAnswer();
        }

        public static ConfigTD create() //Use default config file.
        {
            return create(new FileIni(Program.ProgramPath + FileConfigId + ".ini"));
        }

        public static ConfigTD create(FileIni fileConfig)
        {
            checkIsFileConfig(fileConfig, FileConfigId);
            return new ConfigTD(fileConfig.Sections[0]);
        }

        public static void printDefault(string configPath)
        {
            printDefault(configPath, FileConfigId, typeof(ConfigTD));
        }

        public static bool isFileConfig(FileIni fileConfig)
        {
            return isFileConfig(fileConfig, FileConfigId);
        }
    }
}
