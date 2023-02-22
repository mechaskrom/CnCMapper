using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Map background layer tile set table. Only used by Tiberian Dawn?
    //https://moddingwiki.shikadi.net/wiki/Command_%26_Conquer_Mission_Format
    class FileBinTileSetTableTD : FileBase
    {
        //Layout: tile_set_table_entries.
        //tile set table entries:
        //-tile set id UInt8: Id of tile set file.
        //-tile set index UInt8: Index of tile inside file.

        private const int TileSetTableLength = 64 * 64 * 2; //2 bytes per entry.

        private byte[] mTileSetTable;

        public FileBinTileSetTableTD()
        {
        }

        public FileBinTileSetTableTD(string filePath)
            : base(filePath)
        {
        }

        public FileBinTileSetTableTD(FileProto fileProto)
            : base(fileProto)
        {
        }

        public static FileBinTileSetTableTD create(string fileName, byte[] tileSetTable)
        {
            if (tileSetTable.Length != TileSetTableLength)
            {
                throw new ArgumentException(string.Format("Tile set table length '{0}' should be '{1}'!",
                    tileSetTable.Length, TileSetTableLength));
            }
            MemoryStream stream = new MemoryStream(tileSetTable);
            return new FileBinTileSetTableTD(new FileProto(fileName, stream, "Internal"));
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "BIN";
        }

        protected override void parseInit(Stream stream)
        {
            checkFileLengthExpected(TileSetTableLength);

            //Tile set table.
            mTileSetTable = stream.readArray(TileSetTableLength);
        }

        public byte[] TileSetTable
        {
            get { return mTileSetTable; }
        }

        public void debugSaveContent()
        {
            string folderPath = Program.DebugOutPath + "bin\\";
            Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(folderPath + Name, mTileSetTable);
        }
    }
}
