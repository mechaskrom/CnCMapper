using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CnCMapper.FileFormat
{
    //Describes sets/groups of tiles (icons) in Dune 2.
    //Kind of similar to ICN files and templates in Tiberian Dawn and Red Alert.
    //In Dune 2 tiles (icons) are stored in the "ICON.ICN" file and tile sets in the "ICON.MAP" file.
    //The MAP file maps tiles (icons) from the ICN file into multiple tile sets.
    class FileMapTileSetsD2 : FileBase
    {
        //A MAP-file is just a list of UInt16 values.

        //Layout: tile_set_offsets,tile_indices.

        //Offsets are a list of UInt16 values. Each value is an index to a UInt16 value inside the file
        //i.e. multiply it with 2 to get the file position in bytes.
        //First offset value should be the same as the total number of offsets i.e. tile set count.
        //The difference between previous and next offset tells how many tile indices a set has.
        //The length of the last tile set must be calculated from the last stored offset and the max
        //number of UInt16 values that can fit in the file's size (size/2).

        //Tile indices are just UInt16 values. Each value is an index to the icon in the "ICON.ICN" file.

        //It's difficult to find information about the "ICON.MAP" file format.
        //All comments and code within here are my understanding of it from reading forum posts
        //and other projects' source code. It may be incorrect, but seems to work well enough
        //to handle Dune 2 at least.

        private UInt16[][] mTileSets;

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "MAP";
        }

        protected override void parseInit(Stream stream)
        {
            //Index offsets.
            checkFileLengthMin(2); //Can read first offset?
            UInt16 prevOffset = stream.readUInt16();
            if (prevOffset < 1)
            {
                throwParseError("Index offset count is less than '1'!");
            }
            checkFileLengthMin(prevOffset * 2); //Can read all offsets?
            UInt16[][] tileSets = new UInt16[prevOffset][]; //First offset same as the total number.
            for (int i = 0; i < (tileSets.Length - 1); i++) //Do last tile set later.
            {
                UInt16 nextOffset = stream.readUInt16();
                //Calculate the number of tile indices in each set from previous and next offset.
                tileSets[i] = new UInt16[nextOffset - prevOffset];
                prevOffset = nextOffset;
            }
            //Use the max number of 16-bit values that can fit in this file as the last offset.
            UInt16 lastOffset = (UInt16)(Length / 2);
            tileSets[tileSets.Length - 1] = new UInt16[lastOffset - prevOffset]; //Length of last set.

            //Tile indices.
            for (int i = 0; i < tileSets.Length; i++)
            {
                UInt16[] tileSet = tileSets[i];
                for (int j = 0; j < tileSet.Length; j++)
                {
                    tileSet[j] = stream.readUInt16();
                }
            }

            mTileSets = tileSets;
        }

        public UInt16 getTileIndex(int tileSetId, int tileSetIndex) //Returns index to tile in an ICN-file.
        {
            return mTileSets[tileSetId][tileSetIndex];
        }

        public UInt16[] getTileSet(int tileSetId)
        {
            return mTileSets[tileSetId];
        }

        public UInt16[][] copyTileSets()
        {
            UInt16[][] tileSets = new UInt16[mTileSets.Length][];
            for (int i = 0; i < tileSets.Length; i++)
            {
                tileSets[i] = mTileSets[i].takeValues(); //Copy array.
            }
            return tileSets;
        }

        public static void writeMap(string path, UInt16[][] tileSets)
        {
            using (FileStream fs = File.Create(path))
            {
                writeMap(fs, tileSets);
            }
        }

        private static void writeMap(Stream stream, UInt16[][] tileSets)
        {
            //Simple method to save custom tile sets into a valid MAP-file.

            //Offsets.
            for (int i = 0, offset = tileSets.Length; i < tileSets.Length; i++)
            {
                stream.writeUInt16((UInt16)offset);
                offset += tileSets[i].Length;
            }

            //Tile set indices.
            for (int i = 0; i < tileSets.Length; i++)
            {
                UInt16[] tileSet = tileSets[i];
                for (int j = 0; j < tileSet.Length; j++)
                {
                    stream.writeUInt16(tileSet[j]);
                }
            }
        }

        public void debugSaveTileSetsIndices()
        {
            string folderPath = Program.DebugOutPath + "map\\";
            Directory.CreateDirectory(folderPath);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mTileSets.Length; i++)
            {
                for (int j = 0; j < mTileSets[i].Length; j++)
                {
                    sb.AppendFormat("{0:D2},", mTileSets[i][j]);
                }
                sb.AppendLine();
            }
            File.WriteAllText(folderPath + Name + " indices.txt", sb.ToString());
        }

        public void debugSaveTileSetsImages(string folderName, FileIcnTilesD2 fileIcn, Palette6Bit palette)
        {
            string folderPath = Program.DebugOutPath + "map\\" + folderName + " sets\\";
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < mTileSets.Length; i++)
            {
                string subfolderPath = folderPath + i + "\\";
                Directory.CreateDirectory(subfolderPath);
                Size sizeInTiles = Game.CnC.D2.TileSetD2.get(i).TemplateSize;
                for (int j = 0; j < mTileSets[i].Length; )
                {
                    Frame set = new Frame(sizeInTiles.Width * FileIcnTilesD2.TileWidth, sizeInTiles.Height * FileIcnTilesD2.TileHeight);
                    for (int y = 0; y < sizeInTiles.Height; y++)
                    {
                        for (int x = 0; x < sizeInTiles.Width; x++, j++)
                        {
                            Frame tile = fileIcn.getTileRemapped(mTileSets[i][j]);
                            set.write(tile, new Point(x * FileIcnTilesD2.TileWidth, y * FileIcnTilesD2.TileHeight));
                        }
                    }
                    set.save(palette, subfolderPath + ((j / (sizeInTiles.Width * sizeInTiles.Height)) - 1) + ".png");
                }
            }
        }
    }
}
