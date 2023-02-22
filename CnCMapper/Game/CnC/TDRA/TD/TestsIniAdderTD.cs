using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Add entries to sections in an existing mission INI-file. Used for writing test-files.
    class TestsIniAdderTD : TestsIniAdderTDRA
    {
        private IniSection mSectionOverlay = null;
        private byte[] mTileSetTable = null; //64*64*2.

        public TestsIniAdderTD(string path)
            : base(path)
        {
        }

        public void addStructure(string house, string id, int health, int tileNumber, int direction)
        {
            addStructure(getSectionStructure().Keys.Count, house, id, health, tileNumber, direction);
        }

        public void addStructure(int keyNumber, string house, string id, int health, int tileNumber, int direction)
        {
            //Format: number=house,id,health,tileNumber,direction,trigger
            addNumberedKey(getSectionStructure(), keyNumber,
                string.Format("{0},{1},{2},{3},{4},None", house, id, health, tileNumber, direction));
        }

        public void addTerrain(string id, int tileNumber)
        {
            //Format: tileNumber=id,trigger?
            getSectionTerrain().addKey(tileNumber.ToString(), string.Format("{0},None", id));
        }

        public void addOverlay(string id, int tileNumber)
        {
            //Format: tileNumber=id
            getSectionOverlay().addKey(tileNumber.ToString(), id);
        }

        protected override void addNumberedKey(IniSection section, int keyNumber, string keyValue)
        {
            section.insertKey(0, keyNumber.ToString().PadLeft(3, '0'), keyValue);
        }

        protected override IniSection getSectionMap()
        {
            return mIniEditor.getSection("MAP");
        }

        public IniSection getSectionOverlay()
        {
            return initSection("OVERLAY", ref mSectionOverlay);
        }

        public void setTileSetTableTile(byte tileId, byte tileIndex, int tileNum)
        {
            if (mTileSetTable == null)
            {
                string fileBinName = mIniEditor.SourceInfo.FullName.ReplaceEnd("bin");
                try //Read from corresponding BIN-file first.
                {
                    FileBinTileSetTableTD fileBin = new FileBinTileSetTableTD(fileBinName);
                    mTileSetTable = fileBin.TileSetTable;
                }
                catch //If that failed, create and use an all clear tile set table.
                {
                    mTileSetTable = GroundLayerTD.createTileSetTableClear();
                }
            }
            //A tile set table is 8KB (64*64*2) of tile set table entries (UInt16).
            //A tile set table entry consists of tile set id (UInt8) and tile set index (UInt8).

            int ind = tileNum * 2;
            mTileSetTable[ind + 0] = tileId; //Tile set id.
            mTileSetTable[ind + 1] = tileIndex; //Tile set index.
        }

        public void setTileSetTable(byte[] tileSetTable)
        {
            if (tileSetTable.Length != GroundLayerTD.TileSetTableLength)
            {
                throw new ArgumentException(string.Format("Tile set table length '{0}' should be '{1}'!",
                    tileSetTable.Length, GroundLayerTD.TileSetTableLength));
            }
            mTileSetTable = tileSetTable;
        }

        public override string save(string folder, string name)
        {
            //Save INI-file with a corresponding BIN-file.
            string iniPath = saveInner(folder, name);
            string binPath = iniPath.ReplaceEnd("bin");
            if (mTileSetTable != null) //Map tiles were changed i.e. need to save BIN-file?
            {
                File.WriteAllBytes(binPath, mTileSetTable);
            }
            else if (!File.Exists(binPath)) //Map has no corresponding BIN-file?
            {
                File.WriteAllBytes(binPath, GroundLayerTD.createTileSetTableClear()); //Save a default all clear tiles.
            }
            return iniPath;
        }
    }
}
