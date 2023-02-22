using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    //Add entries to sections in an existing mission INI-file. Used for writing test-files.
    class TestsIniAdderD2
    {
        private readonly FileIni.Editor mIniEditor;
        private IniSection mSectionUnit = null;
        private IniSection mSectionStructure = null;

        public TestsIniAdderD2(string path)
        {
            mIniEditor = FileIni.Editor.open(path);
        }

        public void updateKey(string sectionId, string keyId, string keyValue)
        {
            updateKey(mIniEditor.findOrAddSection(sectionId), keyId, keyValue);
        }

        private void updateKey(IniSection section, string keyId, string keyValue) //Adds section/key if it doesn't exist.
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

        public void addConcrete(string house, string id, int tileNumber)
        {
            //Format: GEN<tileNumber>=house,id
            getSectionStructure().insertKey(0, "GEN" + tileNumber,
                string.Format("{0},{1}", house, id));
        }

        public void addBuilding(string house, string id, int health, int tileNumber)
        {
            addBuilding(getSectionStructure().Keys.Count, house, id, health, tileNumber);
        }

        public void addBuilding(int keyNumber, string house, string id, int health, int tileNumber)
        {
            //Format: ID<number>=house,id,health,tileNumber
            getSectionStructure().insertKey(0, "ID" + keyNumber.ToString("D3"),
                string.Format("{0},{1},{2},{3}", house, id, health, tileNumber));
        }

        public void addUnit(string house, string id, int health, int tileNumber, int direction)
        {
            addUnit(getSectionUnit().Keys.Count, house, id, health, tileNumber, direction);
        }

        public void addUnit(int keyNumber, string house, string id, int health, int tileNumber, int direction)
        {
            //Format: ID<number>=house,id,health,tileNumber,direction,action
            getSectionUnit().insertKey(0, "ID" + keyNumber.ToString("D3"),
                string.Format("{0},{1},{2},{3},{4},Guard", house, id, health, tileNumber, direction));
        }

        public IniSection getSectionUnit()
        {
            return initSection("UNITS", ref mSectionUnit);
        }

        public IniSection getSectionStructure()
        {
            return initSection("STRUCTURES", ref mSectionStructure);
        }

        public void sortStructures() //Sort structures by their format and id like most Dune 2 INI-files do.
        {
            IniSection sectionStructures = getSectionStructure();
            sectionStructures.Keys.SortStable(compareStructureSortValue);
        }

        private static int compareStructureSortValue(IniKey x, IniKey y)
        {
            int xs = getStructureSortValue(x.Id);
            int ys = getStructureSortValue(y.Id);
            return ys.CompareTo(xs); //Sort in reverse order.
        }

        private static int getStructureSortValue(string iniKeyId)
        {
            if (iniKeyId.StartsWith("ID")) //ID format?
            {
                return int.Parse(iniKeyId.Substring(2));
            }
            //Assume GEN format. Add a large value to separate them from the ID format.
            return int.Parse(iniKeyId.Substring(3)) + (int.MaxValue / 2);
        }

        private IniSection initSection(string sectionId, ref IniSection section)
        {
            if (section == null)
            {
                section = mIniEditor.findOrAddSection(sectionId);
            }
            return section;
        }

        public string save(string folder, string name) //Returns path to saved INI-file.
        {
            string nameSource = Path.GetFileNameWithoutExtension(mIniEditor.SourceInfo.FullName);
            string path = Path.Combine(folder, nameSource + " " + name) + ".INI";
            //I read somewhere that Dune 2 INI-files must start with a comment,
            //but I tested without and it seemed to work fine in the game.
            //Let's add one anyway just to be on the safe side.
            File.WriteAllLines(path, new string[]
            {
                "; " + name, //Start with name as a comment.
                string.Empty, //And an empty line.
                mIniEditor.getTextContent() //Before actual content.
            });
            return path;
        }
    }
}
