using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class ConfigRA : ConfigTDRA
    {
        private const string FileConfigId = "RedAlert";

        private readonly bool mDrawSmallInfantry;
        private const string DrawSmallInfantryKey = "DrawSmallInfantry";
        [Option(Group.sprite, DrawSmallInfantryKey, typeof(Answer), "Draw infantry in their small size.")]
        public bool DrawSmallInfantry { get { return mDrawSmallInfantry; } }

        private readonly bool mExposeEnemySpies;
        private const string ExposeEnemySpiesKey = "ExposeEnemySpies";
        [Option(Group.expose, ExposeEnemySpiesKey, typeof(Answer), "Expose enemy spies instead of drawing them as infantry owned by player.")]
        public bool ExposeEnemySpies { get { return mExposeEnemySpies; } }

        private readonly bool mExposeEnemyPillboxes;
        private const string ExposeEnemyPillboxesKey = "ExposeEnemyPillboxes";
        [Option(Group.expose, ExposeEnemyPillboxesKey, typeof(Answer), "Expose camouflaged enemy pillboxes.")]
        public bool ExposeEnemyPillboxes { get { return mExposeEnemyPillboxes; } }

        private readonly bool mUseHouseColorMines;
        private const string UseHouseColorMinesKey = "UseHouseColorMines";
        [Option(Group.expose, UseHouseColorMinesKey, typeof(Answer), "Use owner's colors on mines instead of shading them.")]
        public bool UseHouseColorMines { get { return mUseHouseColorMines; } }

        private readonly bool mShowInvisibleEnemies;
        private const string ShowInvisibleEnemiesKey = "ShowInvisibleEnemies";
        [Option(Group.expose, ShowInvisibleEnemiesKey, typeof(Answer), "Show invisible enemy sprites.")]
        public bool ShowInvisibleEnemies { get { return mShowInvisibleEnemies; } }

        private readonly bool mDrawFakeStructSigns;
        private const string DrawFakeStructSignsKey = "DrawFakeStructSigns";
        [Option(Group.note, DrawFakeStructSignsKey, typeof(Answer), "Draw a sign over fake structures.")]
        public bool DrawFakeStructSigns { get { return mDrawFakeStructSigns; } }

        private readonly bool mShowRadarInvisibleByRule;
        private const string ShowRadarInvisibleByRuleKey = "ShowRadarInvisibleByRule";
        [Option(Group.radar, ShowRadarInvisibleByRuleKey, typeof(Answer), "Show sprites set by a rule to be invisible on radar.")]
        public bool ShowRadarInvisibleByRule { get { return mShowRadarInvisibleByRule; } }
        //Sprites with an invisible rule set (normally only MINP and MINV) aren't drawn on the radar regardless of owner.

        private readonly bool mUseRadarBrightColor;
        private const string UseRadarBrightColorKey = "UseRadarBrightColor";
        [Option(Group.radar, UseRadarBrightColorKey, typeof(Answer), "Use a brighter radar color for units like Tiberian Dawn does.")]
        public bool UseRadarBrightColor { get { return mUseRadarBrightColor; } }
        //The radar in Red Alert only uses one color per house unlike Tiberian Dawn which used two colors.

        private ConfigRA(IniSection iniSection)
            : base(iniSection)
        {
            mDrawSmallInfantry = iniSection.getKey(DrawSmallInfantryKey).valueAsBoolAnswer();
            mExposeEnemySpies = iniSection.getKey(ExposeEnemySpiesKey).valueAsBoolAnswer();
            mExposeEnemyPillboxes = iniSection.getKey(ExposeEnemyPillboxesKey).valueAsBoolAnswer();
            mUseHouseColorMines = iniSection.getKey(UseHouseColorMinesKey).valueAsBoolAnswer();
            mShowInvisibleEnemies = iniSection.getKey(ShowInvisibleEnemiesKey).valueAsBoolAnswer();
            mDrawFakeStructSigns = iniSection.getKey(DrawFakeStructSignsKey).valueAsBoolAnswer();
            mShowRadarInvisibleByRule = iniSection.getKey(ShowRadarInvisibleByRuleKey).valueAsBoolAnswer();
            mUseRadarBrightColor = iniSection.getKey(UseRadarBrightColorKey).valueAsBoolAnswer();
        }

        public static ConfigRA create() //Use default config file.
        {
            return create(new FileIni(Program.ProgramPath + FileConfigId + ".ini"));
        }

        public static ConfigRA create(FileIni fileConfig)
        {
            checkIsFileConfig(fileConfig, FileConfigId);
            return new ConfigRA(fileConfig.Sections[0]);
        }

        public static void printDefault(string configPath)
        {
            printDefault(configPath, FileConfigId, typeof(ConfigRA));
        }

        public static bool isFileConfig(FileIni fileConfig)
        {
            return isFileConfig(fileConfig, FileConfigId);
        }
    }
}
