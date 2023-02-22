using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC
{
    abstract class ConfigCnC
    {
        //Some groups for coarse sorting of settings.
        protected enum Group
        {
            map = 0, //Map settings.
            sprite, //Mostly for how sprites are added.
            expose, //Mostly for how to make sprites extra visible.
            note, //Text on sprites/tiles.
            radar, //Radar settings.
        }

        private readonly Outside mOutsideMapBorders;
        private const string OutsideMapBordersKey = "OutsideMapBorders";
        [Option(Group.map, OutsideMapBordersKey, typeof(Outside), "Select what to do with the area outside a map's borders.")]
        public Outside OutsideMapBorders { get { return mOutsideMapBorders; } }

        private readonly bool mShadeOutsideMapBorders;
        private const string ShadeOutsideMapBordersKey = "ShadeOutsideMapBorders";
        [Option(Group.map, ShadeOutsideMapBordersKey, typeof(Answer), "Shade the area outside a map's borders.")]
        public bool ShadeOutsideMapBorders { get { return mShadeOutsideMapBorders; } }

        private readonly bool mDrawMapHeader;
        private const string DrawMapHeaderKey = "DrawMapHeader";
        [Option(Group.map, DrawMapHeaderKey, typeof(Answer), "Draw a header with map info in the top left corner.")]
        public bool DrawMapHeader { get { return mDrawMapHeader; } }

        private readonly bool mDrawSpriteActions;
        private const string DrawSpriteActionsKey = "DrawSpriteActions";
        [Option(Group.note, DrawSpriteActionsKey, typeof(Answer), "Draw sprite actions.")]
        public bool DrawSpriteActions { get { return mDrawSpriteActions; } }

        protected ConfigCnC(IniSection iniSection)
        {
            mOutsideMapBorders = iniSection.getKey(OutsideMapBordersKey).valueAsEnum<Outside>();
            mShadeOutsideMapBorders = iniSection.getKey(ShadeOutsideMapBordersKey).valueAsBoolAnswer();
            mDrawMapHeader = iniSection.getKey(DrawMapHeaderKey).valueAsBoolAnswer();
            mDrawSpriteActions = iniSection.getKey(DrawSpriteActionsKey).valueAsBoolAnswer();
        }

        protected static void printDefault(string configPath, string fileConfigId, Type type)
        {
            //Create an INI-config file with default settings in specified folder.
            string configFilePath = configPath + fileConfigId + ".ini";
            if (!File.Exists(configFilePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[" + fileConfigId + "]");
                printOptions(sb, type);
                File.WriteAllText(configFilePath, sb.ToString());
            }
        }

        private static void printOptions(StringBuilder sb, Type type)
        {
            Type oaType = typeof(OptionAttribute);
            List<OptionAttribute> options = new List<OptionAttribute>();
            foreach (System.Reflection.PropertyInfo pi in type.GetPropertiesBaseFirst())
            {
                options.AddNotNull(Attribute.GetCustomAttribute(pi, oaType) as OptionAttribute);
            }

            //Sort options by group. Use a stable sort method to keep the original order of options in the groups.
            options.SortStable(OptionAttribute.compareGroup);
            foreach (OptionAttribute oa in options)
            {
                sb.AppendLine(";{0} [{1} (default = {2})]", oa.Description, oa.Values, oa.DefVal);
                sb.AppendLine("{0}={1}", oa.Key, oa.DefVal);
            }
        }

        protected static void checkIsFileConfig(FileIni fileConfig, string fileConfigId)
        {
            if (!isFileConfig(fileConfig, fileConfigId))
            {
                throw new ArgumentException(string.Format("A config file must have one section whose name is '[{0}]'.", fileConfigId));
            }
        }

        protected static bool isFileConfig(FileIni fileConfig, string fileConfigId)
        {
            return fileConfig.Sections.Count == 1 && fileConfig.Sections[0].Id == fileConfigId;
        }

        protected class OptionAttribute : Attribute
        {
            private readonly Group mGroup;
            private readonly string mKey;
            private readonly string mValues;
            private readonly string mDefVal;
            private readonly string mDescription;

            public OptionAttribute(Group group, string key, Type enumType, string description)
                : this(group, key, description)
            {
                string[] enumNames = Enum.GetNames(enumType);
                string enumValues = "";
                for (int i = 0; i < enumNames.Length; i++)
                {
                    enumValues += enumNames[i];
                    if (i < (enumNames.Length - 1)) //Add a ',' if not the last item.
                    {
                        enumValues += ", ";
                    }
                }
                mValues = enumValues;
                mDefVal = enumNames[0];
            }

            public OptionAttribute(Group group, string key, string values, string defVal, string description)
                : this(group, key, description)
            {
                mValues = values;
                mDefVal = defVal;
            }

            private OptionAttribute(Group group, string key, string description)
            {
                mGroup = group;
                mKey = key;
                mDescription = description;
            }

            public Group Group { get { return mGroup; } }
            public string Key { get { return mKey; } }
            public string Values { get { return mValues; } }
            public string DefVal { get { return mDefVal; } }
            public string Description { get { return mDescription; } }

            public static int compareGroup(OptionAttribute x, OptionAttribute y)
            {
                int xg = (int)x.mGroup;
                int yg = (int)y.mGroup;
                return xg.CompareTo(yg);
            }
        }

        public enum Answer
        {
            no,
            yes,
        }

        public enum Outside //Outside map's borders.
        {
            remove, //Remove it.
            margin, //Remove it, but leave a margin around it i.e. inflate the borders a bit.
            keep, //Keep it.
            bezel, //Keep it, but with a black bezel/frame around it.
        }
    }

    static class IniKeyExt
    {
        public static bool valueAsBoolAnswer(this IniKey iniKey)
        {
            return iniKey.valueAsEnum<ConfigCnC.Answer>() == ConfigCnC.Answer.yes;
        }
    }
}
