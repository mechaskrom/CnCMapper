#define USE_ALT_RULE_IMPL //Use alternative implementation for rules in Red Alert?
//I can't really decide which implementation I like most so I'll keep both here for now.
//First implementation is maybe a bit complicated (recursive call to global value) and
//quite a bit of code is needed to support a new rule value. A lot of calls to findSection()
//in INI-file also, but caching sections doesn't make a noticeable difference so not an issue?

//The alternative implementation parses all values needed in the INI-section.
//A bit simpler, but does a lot of extra work if only a few values are used.
//Will maybe get worse if more values are added in the future?

//TODO: Decide which rule implementation to use? Or maybe figure out something better?

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
#if !USE_ALT_RULE_IMPL
    //Rule values in an INI-section.
    abstract class RulesRA
    {
        //"REDALERT.MIX\LOCAL.MIX\RULES.INI" has game settings.
        private static FileIni mIniGlobalRules = null;
        //"EXPAND2.MIX"\AFTRMATH.INI" has game settings for aftermath expansion.
        private static FileIni mIniAftermathRules = null;

        protected readonly string mSectionId;
        protected readonly FileIni mIniLocalRules; //Null if this is a global rule.

        protected RulesRA(string sectionId, FileIni iniLocalRules)
        {
            mSectionId = sectionId;
            mIniLocalRules = iniLocalRules;
        }

        protected bool IsLocal //Is this a map specific rule (i.e. not a global rule)?
        {
            get { return mIniLocalRules != null; }
        }

        public string Id
        {
            get { return mSectionId; }
        }

        protected IniKey findKey(string keyId) //Returns null if section id or key id not found.
        {
            IniKey iniKey;
            if (IsLocal) //Check local rules.
            {
                iniKey = findKeyLocal(keyId);
            }
            else //Check global rules.
            {
                iniKey = findKeyGlobal(keyId); //Read original vanilla rules first, then...
                if (iniKey == null)
                {
                    iniKey = findKeyAftermath(keyId); //...aftermath rules if not found.

                    //Not sure if aftermath override vanilla rules or vice versa.
                    //Hopefully the order won't affect sprite visuals in some way.

                    //Maybe also check "EXPAND2.MIX"\MPLAYER.INI" which has settings
                    //for aftermath multiplayer maps?

                    //Looking at the source code it seems like rules are read in this order (first to last):
                    //RULES.INI, AFTRMATH.INI (if present) and MPLAYER.INI (if present and multiplayer?).
                    //Later values found overwrites current.
                    //But the order is maybe affected by other factors? Like where the INI-files are located?
                }
            }
            return iniKey;
        }

        private IniKey findKeyLocal(string keyId)
        {
            return findKey(findSectionLocal(), keyId);
        }

        private IniKey findKeyGlobal(string keyId)
        {
            return findKey(findSectionGlobal(mSectionId), keyId);
        }

        private IniKey findKeyAftermath(string keyId)
        {
            return findKey(findSectionAftermath(mSectionId), keyId);
        }

        private static IniKey findKey(IniSection iniSection, string keyId)
        {
            return iniSection != null ? iniSection.findKey(keyId) : null;
        }

        private IniSection findSectionLocal()
        {
            return mIniLocalRules.findSection(mSectionId);
        }

        private static IniSection findSectionGlobal(string sectionId)
        {
            //"REDALERT.MIX\LOCAL.MIX\RULES.INI" has game settings.
            if (mIniGlobalRules == null)
            {
                mIniGlobalRules = new FileSearch<FileIni>("RULES.INI", GameFilesRA.FileMixRedAlertLocal).get();
            }
            return mIniGlobalRules.findSection(sectionId);
        }

        private static IniSection findSectionAftermath(string sectionId)
        {
            //"EXPAND2.MIX"\AFTRMATH.INI" has game settings for aftermath expansion.
            if (mIniAftermathRules == null)
            {
                mIniAftermathRules = new FileSearch<FileIni>("AFTRMATH.INI", GameFilesRA.FileMixExpand2).find();
                if (mIniAftermathRules == null) //Not found?
                {
                    //Set dummy empty INI-file to not search for aftermath file again.
                    mIniAftermathRules = new FileIni();
                }
            }
            return mIniAftermathRules.findSection(sectionId);
        }
    }

    //Rule values in INI-section [General].
    class RulesGeneralRA : RulesRA
    {
        //Cache global rules.
        private static readonly RulesGeneralRA mRulesGeneralGlobal = new RulesGeneralRA(null);

        private FixedMath? mConditionYellow = null; //ConditionYellow = when damaged to this percentage, health bar turns yellow.

        public RulesGeneralRA(FileIni fileIniLocalRules)
            : base("General", fileIniLocalRules)
        {
        }

        public FixedMath getConditionYellow()
        {
            if (!mConditionYellow.HasValue) //Not cached?
            {
                IniKey keyConditionYellow = findKey("ConditionYellow");
                if (keyConditionYellow != null)
                {
                    mConditionYellow = keyConditionYellow.valueAsFixed();
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mConditionYellow = mRulesGeneralGlobal.getConditionYellow();
                }
                else
                {
                    mConditionYellow = FixedMath.Frac1_2; //Default 1/2 (50%). Checked in source.
                }
            }
            return mConditionYellow.Value;
        }
    }

    //Rule values in INI-section with provided object id.
    class RulesObjectRA : RulesRA
    {
        //Cache global rules.
        private static readonly Dictionary<string, RulesObjectRA> mRulesObjectGlobal = new Dictionary<string, RulesObjectRA>();

        private string mFileShpId = string.Empty; //Image = name of graphic data to use for this object (def=same as object identifier).
        private bool? mIsInvisible; //Invisible = Is completely and always invisible to enemy (def=no)?
        private UInt16? mMaxStrength; //Strength = strength (hit points) of this object.

        public RulesObjectRA(string objectId, FileIni fileIniLocalRules)
            : base(objectId, fileIniLocalRules)
        {
            if (IsLocal && !mRulesObjectGlobal.ContainsKey(objectId)) //Add to global cache also?
            {
                mRulesObjectGlobal.Add(objectId, new RulesObjectRA(objectId, null));
            }
        }

        public string getFileShpOrObjectId() //Returns object id if image rule isn't present.
        {
            string fileShpId = getFileShpId();
            return fileShpId != null ? fileShpId : Id;
        }

        public string getFileShpId()
        {
            if (mFileShpId == string.Empty) //Not cached?
            {
                IniKey keyImage = findKey("Image");
                if (keyImage != null)
                {
                    mFileShpId = keyImage.Value;
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mFileShpId = mRulesObjectGlobal[mSectionId].getFileShpId();
                }
                else
                {
                    mFileShpId = null; //Default null. Checked in source.
                }
            }
            return mFileShpId;
        }

        public bool getIsInvisible()
        {
            if (!mIsInvisible.HasValue) //Not cached?
            {
                IniKey keyInvisible = findKey("Invisible");
                if (keyInvisible != null)
                {
                    mIsInvisible = keyInvisible.valueAsBool();
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mIsInvisible = mRulesObjectGlobal[mSectionId].getIsInvisible();
                }
                else
                {
                    mIsInvisible = false; //Default false. Checked in source.
                }
            }
            return mIsInvisible.Value;
        }

        public UInt16 getMaxStrength()
        {
            if (!mMaxStrength.HasValue) //Not cached?
            {
                IniKey keyStrength = findKey("Strength");
                if (keyStrength != null)
                {
                    mMaxStrength = (UInt16)keyStrength.valueAsUInt32();
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mMaxStrength = mRulesObjectGlobal[mSectionId].getMaxStrength();
                }
                else
                {
                    mMaxStrength = 0; //Default 0. Checked in source.
                }
            }
            return mMaxStrength.Value;
        }
    }

    //Rule values in INI-section with provided object id. Applies only to building types.
    class RulesBuildingRA : RulesObjectRA
    {
        //Cache global rules.
        private static readonly Dictionary<string, RulesBuildingRA> mRulesBuildingGlobal = new Dictionary<string, RulesBuildingRA>();

        private bool? mHasBib; //Bib = Should the building have an attached bib (def=no)?
        private int? mStorage; //Storage = the number of credits this building can store (def=0).

        public RulesBuildingRA(string buildingId, FileIni fileIniLocalRules)
            : base(buildingId, fileIniLocalRules)
        {
            if (IsLocal && !mRulesBuildingGlobal.ContainsKey(buildingId)) //Add to global cache also?
            {
                mRulesBuildingGlobal.Add(buildingId, new RulesBuildingRA(buildingId, null));
            }
        }

        public bool getHasBib()
        {
            if (!mHasBib.HasValue) //Not cached?
            {
                IniKey keyBib = findKey("Bib");
                if (keyBib != null)
                {
                    mHasBib = keyBib.valueAsBool();
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mHasBib = mRulesBuildingGlobal[mSectionId].getHasBib();
                }
                else
                {
                    mHasBib = false; //Default false. Checked in source.
                }
            }
            return mHasBib.Value;
        }

        public int getStorage()
        {
            if (!mStorage.HasValue) //Not cached?
            {
                IniKey keyStorage = findKey("Storage");
                if (keyStorage != null)
                {
                    mStorage = keyStorage.valueAsInt32();
                }
                else if (IsLocal) //Get value from global rule?
                {
                    mStorage = mRulesBuildingGlobal[mSectionId].getStorage();
                }
                else
                {
                    mStorage = 0; //Default 0. Checked in source.
                }
            }
            return mStorage.Value;
        }
    }

#else //Alternative implementation. -------------------------------------------------------------------------------------------

    //Rule values in an INI-section.
    abstract class RulesRA
    {
        //"REDALERT.MIX\LOCAL.MIX\RULES.INI" has game settings.
        private static FileIni mIniGlobalRules = null;
        //"EXPAND2.MIX"\AFTRMATH.INI" has game settings for aftermath expansion.
        private static FileIni mIniAftermathRules = null;

        protected readonly string mSectionId;

        protected RulesRA(string sectionId)
        {
            mSectionId = sectionId;
        }

        public string Id
        {
            get { return mSectionId; }
        }

        protected void adjustGlobal()
        {
            //Read any aftermath rules first, then override with rules present in original vanilla.
            adjust(findSectionAftermath(mSectionId));
            adjust(findSectionGlobal(mSectionId));

            //Not sure if aftermath override vanilla rules or vice versa.
            //Hopefully the order won't affect sprite visuals in some way.

            //Maybe also check "EXPAND2.MIX"\MPLAYER.INI" which has settings
            //for aftermath multiplayer maps?

            //Looking at the source code it seems like rules are read in this order (first to last):
            //RULES.INI, AFTRMATH.INI (if present) and MPLAYER.INI (if present and multiplayer?).
            //Later values found overwrites current.
            //But the order is maybe affected by other factors? Like where the INI-files are located?
        }

        private static IniSection findSectionGlobal(string sectionId)
        {
            //"REDALERT.MIX\LOCAL.MIX\RULES.INI" has game settings.
            if (mIniGlobalRules == null)
            {
                mIniGlobalRules = new FileSearch<FileIni>("RULES.INI", GameRA.FileMixRedAlertLocal).get();
            }
            return mIniGlobalRules.findSection(sectionId);
        }

        private static IniSection findSectionAftermath(string sectionId)
        {
            //"EXPAND2.MIX"\AFTRMATH.INI" has game settings for aftermath expansion.
            if (mIniAftermathRules == null)
            {
                mIniAftermathRules = new FileSearch<FileIni>("AFTRMATH.INI", GameRA.FileMixExpand2).find();
                if (mIniAftermathRules == null) //Not found?
                {
                    //Set dummy empty INI-file to not search for aftermath file again.
                    mIniAftermathRules = FileIni.createDummy("AftrmathDummy.ini", FileBase.Origin.Missing);
                }
            }
            return mIniAftermathRules.findSection(sectionId);
        }

        protected abstract void adjust(IniSection iniSection);
    }

    //Rule values in INI-section [General].
    class RulesGeneralRA : RulesRA
    {
        private const string SectionIdGeneral = "General";

        //Cache global rules.
        private static RulesGeneralRA mRulesGeneralGlobal = null;

        //Enclose rule values in a struct so they can be copied easily.
        private struct Values
        {
            public FixedMath ConditionYellow; //ConditionYellow = when damaged to this percentage, health bar turns yellow.
        }
        private Values mValues;

        private RulesGeneralRA()
            : base(SectionIdGeneral)
        {
            //Defaults. Checked in source.
            mValues.ConditionYellow = FixedMath.Frac1_2; //Default 1/2 (50%).
        }

        public RulesGeneralRA(FileIni fileIniLocalRules)
            : base(SectionIdGeneral)
        {
            if (mRulesGeneralGlobal == null)
            {
                mRulesGeneralGlobal = new RulesGeneralRA();
                mRulesGeneralGlobal.adjustGlobal();
            }
            //Copy global values so they aren't affected by local adjust.
            mValues = mRulesGeneralGlobal.mValues;

            //Adjust with any local rules.
            if (fileIniLocalRules != null)
            {
                adjust(fileIniLocalRules.findSection(SectionIdGeneral));
            }
        }

        protected override void adjust(IniSection iniSection)
        {
            //Adjust any rules found in section.
            if (iniSection != null)
            {
                IniKey conditionYellowKey = iniSection.findKey("ConditionYellow");
                if (conditionYellowKey != null)
                {
                    mValues.ConditionYellow = conditionYellowKey.valueAsFixed();
                }
            }
        }

        public FixedMath getConditionYellow()
        {
            return mValues.ConditionYellow;
        }
    }

    //Rule values in INI-section with provided object id.
    class RulesObjectRA : RulesRA
    {
        //Cache global rules.
        private static readonly Dictionary<string, RulesObjectRA> mRulesObjectGlobal = new Dictionary<string, RulesObjectRA>();

        //Enclose rule values in a struct so they can be copied easily.
        protected struct ValuesObject
        {
            public string FileShpId; //Image = name of graphic data to use for this object (def=same as object identifier).
            public bool IsInvisible; //Invisible = Is completely and always invisible to enemy (def=no)?
            public UInt16 MaxStrength; //Strength = strength (hit points) of this object.
        }
        protected ValuesObject mValuesObject;

        protected RulesObjectRA(string objectId)
            : base(objectId)
        {
            //Defaults. Checked in source.
            mValuesObject.FileShpId = null;
            mValuesObject.IsInvisible = false;
            mValuesObject.MaxStrength = 0;
        }

        public RulesObjectRA(string objectId, FileIni fileIniLocalRules)
            : base(objectId)
        {
            RulesObjectRA rulesObjectGlobal;
            if (!mRulesObjectGlobal.TryGetValue(objectId, out rulesObjectGlobal))
            {
                rulesObjectGlobal = new RulesObjectRA(objectId);
                rulesObjectGlobal.adjustGlobal();
                mRulesObjectGlobal.Add(objectId, rulesObjectGlobal);
            }
            //Copy global values so they aren't affected by local adjust.
            mValuesObject = rulesObjectGlobal.mValuesObject;

            //Adjust with any local rules.
            if (fileIniLocalRules != null)
            {
                adjust(fileIniLocalRules.findSection(objectId));
            }
        }

        protected override void adjust(IniSection iniSection)
        {
            //Adjust any rules found in section.
            if (iniSection != null)
            {
                IniKey imageKey = iniSection.findKey("Image");
                if (imageKey != null)
                {
                    mValuesObject.FileShpId = imageKey.Value;
                }
                IniKey invisibleKey = iniSection.findKey("Invisible");
                if (invisibleKey != null)
                {
                    mValuesObject.IsInvisible = invisibleKey.valueAsBool();
                }
                IniKey strengthKey = iniSection.findKey("Strength");
                if (strengthKey != null)
                {
                    mValuesObject.MaxStrength = (UInt16)strengthKey.valueAsUInt32();
                }
            }
        }

        public string getFileShpOrObjectId() //Returns object id if image rule isn't present.
        {
            string fileShpId = getFileShpId();
            return fileShpId != null ? fileShpId : Id;
        }

        public string getFileShpId()
        {
            return mValuesObject.FileShpId;
        }

        public bool getIsInvisible()
        {
            return mValuesObject.IsInvisible;
        }

        public UInt16 getMaxStrength()
        {
            return mValuesObject.MaxStrength;
        }
    }

    //Rule values in INI-section with provided object id. Applies only to building types.
    class RulesBuildingRA : RulesObjectRA
    {
        //Cache global rules.
        private static readonly Dictionary<string, RulesBuildingRA> mRulesBuildingGlobal = new Dictionary<string, RulesBuildingRA>();

        //Enclose rule values in a struct so they can be copied easily.
        private struct ValuesBuilding
        {
            public bool HasBib; //Bib = Should the building have an attached bib (def=no)?
            public int Storage; //Storage = the number of credits this building can store (def=0).
        }
        private ValuesBuilding mValuesBuilding;

        private RulesBuildingRA(string buildingId)
            : base(buildingId)
        {
            //Defaults. Checked in source.
            mValuesBuilding.HasBib = false;
            mValuesBuilding.Storage = 0;
        }

        public RulesBuildingRA(string buildingId, FileIni fileIniLocalRules)
            : base(buildingId)
        {
            RulesBuildingRA rulesBuildingGlobal;
            if (!mRulesBuildingGlobal.TryGetValue(buildingId, out rulesBuildingGlobal))
            {
                rulesBuildingGlobal = new RulesBuildingRA(buildingId);
                rulesBuildingGlobal.adjustGlobal();
                mRulesBuildingGlobal.Add(buildingId, rulesBuildingGlobal);
            }
            //Copy global values so they aren't affected by local adjust.
            mValuesObject = rulesBuildingGlobal.mValuesObject;
            mValuesBuilding = rulesBuildingGlobal.mValuesBuilding;

            //Adjust with any local rules.
            if (fileIniLocalRules != null)
            {
                adjust(fileIniLocalRules.findSection(buildingId));
            }
        }

        protected override void adjust(IniSection iniSection)
        {
            //Adjust any rules found in section.
            if (iniSection != null)
            {
                base.adjust(iniSection);

                IniKey bibKey = iniSection.findKey("Bib");
                if (bibKey != null)
                {
                    mValuesBuilding.HasBib = bibKey.valueAsBool();
                }
                IniKey storageKey = iniSection.findKey("Storage");
                if (storageKey != null)
                {
                    mValuesBuilding.Storage = storageKey.valueAsInt32();
                }
            }
        }

        public bool getHasBib()
        {
            return mValuesBuilding.HasBib;
        }

        public int getStorage()
        {
            return mValuesBuilding.Storage;
        }
    }
#endif
}