using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Add entries to sections in an existing mission INI-file. Used for writing test-files.
    class TestsIniAdderRA : TestsIniAdderTDRA
    {
        private IniSection mSectionShip = null;
        private IniSection mSectionOverlayPack = null;
        private byte[] mOverlayIdInTiles = null; //128*128*1.
        private IniSection mSectionMapPack = null;
        private byte[] mTileSetTable = null; //128*128*3.

        public TestsIniAdderRA(string path)
            : base(path)
        {
        }

        public void addStructure(string house, string id, int health, int tileNumber, int direction)
        {
            addStructure(getSectionStructure().Keys.Count, house, id, health, tileNumber, direction, 0);
        }

        public void addStructure(string house, string id, int health, int tileNumber, int direction, int repair)
        {
            addStructure(getSectionStructure().Keys.Count, house, id, health, tileNumber, direction, repair);
        }

        public void addStructure(int keyNumber, string house, string id, int health, int tileNumber, int direction)
        {
            addStructure(keyNumber, house, id, health, tileNumber, direction, 0);
        }

        public void addStructure(int keyNumber, string house, string id, int health, int tileNumber, int direction, int repair)
        {
            //Format: number=house,id,health,tileNumber,direction,trigger,sellable,repair
            addNumberedKey(getSectionStructure(), keyNumber,
                string.Format("{0},{1},{2},{3},{4},None,0,{5}", house, id, health, tileNumber, direction, repair));
        }

        public void addTerrain(string id, int tileNumber)
        {
            //Format: tileNumber=id
            getSectionTerrain().addKey(tileNumber.ToString(), id);
        }

        public void addShip(string house, string id, int health, int tileNumber, int direction)
        {
            addShip(getSectionShip().Keys.Count, house, id, health, tileNumber, direction);
        }

        public void addShip(int keyNumber, string house, string id, int health, int tileNumber, int direction)
        {
            //Format: number=house,id,health,tileNumber,direction,action,trigger
            addNumberedKey(getSectionShip(), keyNumber,
                string.Format("{0},{1},{2},{3},{4},Guard,None", house, id, health, tileNumber, direction));
        }

        public void addRule(string ruleSectionId, string ruleKey, string ruleValue) //Updates rule if it already exists.
        {
            updateKey(mIniEditor.findOrAddSectionStart(ruleSectionId), ruleKey, ruleValue); //Add rule at start.
            //Rules are usually at the start of INI-files, but it probably doesn't matter where they are located.
        }

        protected override IniSection getSectionMap()
        {
            return mIniEditor.getSection("Map");
        }

        public IniSection getSectionShip()
        {
            return initSection("SHIPS", ref mSectionShip);
        }

        private IniSection getSectionOverlayPack()
        {
            return initSection("OverlayPack", ref mSectionOverlayPack);
        }

        private IniSection getSectionMapPack()
        {
            return initSection("MapPack", ref mSectionMapPack);
        }

        public void setOverlay(string id, int tileNum)
        {
            if (mOverlayIdInTiles == null)
            {
                mOverlayIdInTiles = Crypt.Pack.decode(getSectionOverlayPack().getPackDataAsBase64());
            }
            mOverlayIdInTiles[tileNum] = SpriteOverlayRA.toOverlayIdValue(id);
        }

        public void setTileSetTableTile(UInt16 tileId, byte tileIndex, int tileNum)
        {
            if (mTileSetTable == null)
            {
                mTileSetTable = Crypt.Pack.decode(getSectionMapPack().getPackDataAsBase64());
            }
            //A tile set table is 48KB and consists of two sections:
            //-First section is 32KB (128*128*2) with tile set id data (UInt16).
            //-Second section is 16KB (128*128*1) with tile set index data (UInt8).

            //First section in tile set table (2 byte per entry).
            int ind1st = tileNum * 2;
            mTileSetTable[ind1st + 0] = (byte)((tileId >> 0) & 0xFF); //Id low byte.
            mTileSetTable[ind1st + 1] = (byte)((tileId >> 8) & 0xFF); //Id high byte.

            //Second section in tile set table.
            int ind2nd = (MapRA.WidthInTiles * MapRA.HeightInTiles * 2) + tileNum;
            mTileSetTable[ind2nd] = tileIndex; //Index.
        }

        public void setTileSetTable(byte[] tileSetTable)
        {
            if (tileSetTable.Length != GroundLayerRA.TileSetTableLength)
            {
                throw new ArgumentException(string.Format("Tile set table length '{0}' should be '{1}'!",
                    tileSetTable.Length, GroundLayerRA.TileSetTableLength));
            }
            mTileSetTable = tileSetTable;
        }

        public override string save(string folder, string name)
        {
            if (mOverlayIdInTiles != null) //Overlay tiles were changed i.e. need to update overlay data in INI-file?
            {
                IniSection section = getSectionOverlayPack();
                //Clear existing overlay data.
                section.Keys.Clear();
                //Write new overlay data.
                string packData = Crypt.Pack.encode(mOverlayIdInTiles);
                foreach (KeyValuePair<string, string> line in IniSection.toPackDataFormatted(packData))
                {
                    section.addKey(line.Key, line.Value);
                }
            }
            if (mTileSetTable != null) //Map tiles were changed i.e. need to update tile data in INI-file?
            {
                IniSection section = getSectionMapPack();
                //Clear existing overlay data.
                section.Keys.Clear();
                //Write new tile data.
                string packData = Crypt.Pack.encode(mTileSetTable);
                foreach (KeyValuePair<string, string> line in IniSection.toPackDataFormatted(packData))
                {
                    section.addKey(line.Key, line.Value);
                }
            }
            return saveInner(folder, name);
        }
    }
}
