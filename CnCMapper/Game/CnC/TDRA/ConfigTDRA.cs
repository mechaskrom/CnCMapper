using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class ConfigTDRA : ConfigCnC
    {
        private readonly MapType mIncludeMapTypes;
        private const string IncludeMapTypesKey = "IncludeMapTypes";
        [Option(Group.map, IncludeMapTypesKey, typeof(MapType), "Select what type of maps to include.")]
        public MapType IncludeMapTypes { get { return mIncludeMapTypes; } }

        private readonly bool mAddUndefinedSprites;
        private const string AddUndefinedSpritesKey = "AddUndefinedSprites";
        [Option(Group.sprite, AddUndefinedSpritesKey, typeof(Answer), "Add undefined sprites.")]
        public bool AddUndefinedSprites { get { return mAddUndefinedSprites; } }

        private readonly bool mAddUnspecifiedTheaterGraphics;
        private const string AddUnspecifiedTheaterGraphicsKey = "AddUnspecifiedTheaterGraphics";
        [Option(Group.sprite, AddUnspecifiedTheaterGraphicsKey, typeof(Answer), "Add unspecified theater graphics.")]
        public bool AddUnspecifiedTheaterGraphics { get { return mAddUnspecifiedTheaterGraphics; } }

        private readonly bool mAddAircraftSprites;
        private const string AddAircraftSpritesKey = "AddAircraftSprites";
        [Option(Group.sprite, AddAircraftSpritesKey, typeof(Answer), "Add any aircraft sprites listed in the [UNITS] section.")]
        public bool AddAircraftSprites { get { return mAddAircraftSprites; } }

        private readonly bool mAddHelipadAircraft;
        private const string AddHelipadAircraftKey = "AddHelipadAircraft";
        [Option(Group.sprite, AddHelipadAircraftKey, typeof(Answer), "Add an aircraft to helipad structures.")]
        public bool AddHelipadAircraft { get { return mAddHelipadAircraft; } }

        private readonly bool mAddBaseStructures;
        private const string AddBaseStructuresKey = "AddBaseStructures";
        [Option(Group.sprite, AddBaseStructuresKey, typeof(Answer), "Add base structures i.e. structures that the AI will try to build during a game.")]
        public bool AddBaseStructures { get { return mAddBaseStructures; } }

        private readonly bool mExposeConcealed;
        private const string ExposeConcealedKey = "ExposeConcealed";
        [Option(Group.expose, ExposeConcealedKey, typeof(Answer), "Draw some sprites often concealed behind terrains, structures, etc. with a higher priority.")]
        public bool ExposeConcealed { get { return mExposeConcealed; } }

        private readonly bool mHighlightCrates;
        private const string HighlightCratesKey = "HighlightCrates";
        [Option(Group.expose, HighlightCratesKey, typeof(Answer), "Make crates more visible.")]
        public bool HighlightCrates { get { return mHighlightCrates; } }

        private readonly bool mDrawTileSetEmptyEffect; //This setting is internal only for now.
        private const string DrawTileSetEmptyEffectKey = "DrawTileSetEmptyEffect";
        //[Option(Group.expose, DrawTileSetEmptyEffectKey, typeof(Answer), "Draw a Hall-Of-Mirror effect indicator instead of black for missing background tiles.")]
        public bool DrawTileSetEmptyEffect { get { return mDrawTileSetEmptyEffect; } }

        private readonly bool mDrawSpriteTriggers;
        private const string DrawSpriteTriggersKey = "DrawSpriteTriggers";
        [Option(Group.note, DrawSpriteTriggersKey, typeof(Answer), "Draw sprite triggers.")]
        public bool DrawSpriteTriggers { get { return mDrawSpriteTriggers; } }

        private readonly bool mDrawBaseNumbers;
        private const string DrawBaseNumbersKey = "DrawBaseNumbers";
        [Option(Group.note, DrawBaseNumbersKey, typeof(Answer), "Draw build order of base structures. ? = repaired structure in Red Alert.")]
        public bool DrawBaseNumbers { get { return mDrawBaseNumbers; } }

        private readonly bool mDrawWaypoints;
        private const string DrawWaypointsKey = "DrawWaypoints";
        [Option(Group.note, DrawWaypointsKey, typeof(Answer), "Draw waypoints.")]
        public bool DrawWaypoints { get { return mDrawWaypoints; } }

        private readonly bool mDrawCellTriggers;
        private const string DrawCellTriggersKey = "DrawCellTriggers";
        [Option(Group.note, DrawCellTriggersKey, typeof(Answer), "Draw cell triggers.")]
        public bool DrawCellTriggers { get { return mDrawCellTriggers; } }

        private readonly int[] mRadarScales;
        private const string RadarScalesKey = "RadarScales";
        [Option(Group.radar, RadarScalesKey, RadarTDRA.ConfigScaleValues, "", "Scales to render radar at. Leave empty to disable radar rendering.")]
        public int[] RadarScales { get { return mRadarScales; } }

        protected ConfigTDRA(IniSection iniSection)
            : base(iniSection)
        {
            mIncludeMapTypes = iniSection.getKey(IncludeMapTypesKey).valueAsEnum<MapType>();
            mAddUndefinedSprites = iniSection.getKey(AddUndefinedSpritesKey).valueAsBoolAnswer();
            mAddUnspecifiedTheaterGraphics = iniSection.getKey(AddUnspecifiedTheaterGraphicsKey).valueAsBoolAnswer();
            mAddAircraftSprites = iniSection.getKey(AddAircraftSpritesKey).valueAsBoolAnswer();
            mAddHelipadAircraft = iniSection.getKey(AddHelipadAircraftKey).valueAsBoolAnswer();
            mAddBaseStructures = iniSection.getKey(AddBaseStructuresKey).valueAsBoolAnswer();
            mExposeConcealed = iniSection.getKey(ExposeConcealedKey).valueAsBoolAnswer();
            mHighlightCrates = iniSection.getKey(HighlightCratesKey).valueAsBoolAnswer();
            mDrawTileSetEmptyEffect = true; //iniSection.getKey(DrawTileSetEmptyEffectKey).valueAsBoolAnswer();
            mDrawSpriteTriggers = iniSection.getKey(DrawSpriteTriggersKey).valueAsBoolAnswer();
            mDrawBaseNumbers = iniSection.getKey(DrawBaseNumbersKey).valueAsBoolAnswer();
            mDrawWaypoints = iniSection.getKey(DrawWaypointsKey).valueAsBoolAnswer();
            mDrawCellTriggers = iniSection.getKey(DrawCellTriggersKey).valueAsBoolAnswer();
            mRadarScales = RadarTDRA.getConfigScales(iniSection.getKey(RadarScalesKey));
        }

        public enum MapType
        {
            all,
            single,
            multi,
        }
    }
}
