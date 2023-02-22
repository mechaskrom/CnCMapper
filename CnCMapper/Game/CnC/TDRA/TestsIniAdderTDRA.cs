using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //Add entries to sections in an existing mission INI-file. Used for writing test-files.
    abstract class TestsIniAdderTDRA
    {
        protected readonly FileIni.Editor mIniEditor;
        protected IniKey mKeyTheater = null;
        protected IniSection mSectionInfantry = null;
        protected IniSection mSectionUnit = null;
        protected IniSection mSectionStructure = null;
        protected IniSection mSectionTerrain = null;
        protected IniSection mSectionSmudge = null;
        protected bool mUseNumberedKeysSmudge = false; //Use numbered INI-keys in [SMUDGE] section instead of same as tile number.

        protected TestsIniAdderTDRA(string path)
        {
            mIniEditor = FileIni.Editor.open(path);
        }

        public string Theater
        {
            get { return getKeyTheater().Value; }
            set { getKeyTheater().Value = value; }
        }

        public bool UseNumberedKeysSmudge
        {
            get { return mUseNumberedKeysSmudge; }
            set { mUseNumberedKeysSmudge = value; }
        }

        public void updateKey(string sectionId, string keyId, string keyValue)
        {
            updateKey(mIniEditor.findOrAddSection(sectionId), keyId, keyValue);
        }

        protected void updateKey(IniSection section, string keyId, string keyValue) //Adds section/key if it doesn't exist.
        {
            IniKey key = section.findKey(keyId);
            if (key == null)
            {
                section.addKey(keyId, keyValue);
            }
            else
            {
                key.Value = keyValue;
            }
        }

        public void addInfantry(string house, string id, int health, int tileNumber, int subPos, int direction)
        {
            addInfantry(getSectionInfantry().Keys.Count, house, id, health, tileNumber, subPos, direction);
        }

        public void addInfantry(int keyNumber, string house, string id, int health, int tileNumber, int subPos, int direction)
        {
            //Format: number=house,id,health,tileNumber,subPos,action,direction,trigger
            addNumberedKey(getSectionInfantry(), keyNumber,
                string.Format("{0},{1},{2},{3},{4},Guard,{5},None", house, id, health, tileNumber, subPos, direction));
        }

        public void addUnit(string house, string id, int health, int tileNumber, int direction)
        {
            addUnit(getSectionUnit().Keys.Count, house, id, health, tileNumber, direction);
        }

        public void addUnit(int keyNumber, string house, string id, int health, int tileNumber, int direction)
        {
            //Format: number=house,id,health,tileNumber,direction,action,trigger
            addNumberedKey(getSectionUnit(), keyNumber,
                string.Format("{0},{1},{2},{3},{4},Guard,None", house, id, health, tileNumber, direction));
        }

        public void addSmudge(string id, int tileNumber)
        {
            addSmudge(id, tileNumber, 0);
        }

        public void addSmudge(string id, int tileNumber, int progression)
        {
            //Default is to use the smudge's tile number as its INI-key also.
            //Can be configured to use numbered INI-keys instead like most other section do.
            int keyNumber = mUseNumberedKeysSmudge ? getSectionSmudge().Keys.Count : tileNumber;
            addSmudge(keyNumber, id, tileNumber, progression);
        }

        public void addSmudge(int keyNumber, string id, int tileNumber, int progression)
        {
            //Format: tileNumber1=id,tileNumber2,progression?
            getSectionSmudge().addKey(keyNumber.ToString(), string.Format("{0},{1},{2}", id, tileNumber, progression));
        }

        protected virtual void addNumberedKey(IniSection section, int keyNumber, string keyValue)
        {
            section.addKey(keyNumber.ToString(), keyValue);
        }

        protected IniKey getKeyTheater()
        {
            if (mKeyTheater == null)
            {
                mKeyTheater = getSectionMap().getKey("Theater");
            }
            return mKeyTheater;
        }

        protected abstract IniSection getSectionMap();

        public IniSection getSectionInfantry()
        {
            return initSection("INFANTRY", ref mSectionInfantry);
        }

        public IniSection getSectionUnit()
        {
            return initSection("UNITS", ref mSectionUnit);
        }

        public IniSection getSectionStructure()
        {
            return initSection("STRUCTURES", ref mSectionStructure);
        }

        public IniSection getSectionTerrain()
        {
            return initSection("TERRAIN", ref mSectionTerrain);
        }

        public IniSection getSectionSmudge()
        {
            return initSection("SMUDGE", ref mSectionSmudge);
        }

        protected IniSection initSection(string sectionId, ref IniSection section)
        {
            if (section == null)
            {
                section = mIniEditor.findOrAddSection(sectionId);
            }
            return section;
        }

        protected static int getSectionKeyMaxNumber(IniSection section)
        {
            int maxNumber = -1;
            foreach (IniKey key in section.Keys)
            {
                int number;
                if (int.TryParse(key.Id, out number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }
            return maxNumber;
        }

        public virtual string save(string folder, string name) //Returns path to saved INI-file.
        {
            return saveInner(folder, name);
        }

        protected string saveInner(string folder, string name) //Returns path to saved INI-file.
        {
            string nameSource = Path.GetFileNameWithoutExtension(mIniEditor.SourceInfo.FullName);
            string path = Path.Combine(folder, nameSource + " " + name) + ".ini";
            mIniEditor.save(path);
            return path;
        }
    }
}
