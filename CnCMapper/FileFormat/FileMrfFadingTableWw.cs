using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Fading Table. Used to remap palette indices.
    //http://www.shikadi.net/moddingwiki/Westwood_Fading_Table
    class FileMrfFadingTableWw : FileBase
    {
        //Layout: remap_blocks.
        //remap block UInt8[256]: Remap indices.

        private const int BlockLength = 256; //Indices (bytes) per block.

        private byte[][] mRemapBlocks;

        public FileMrfFadingTableWw()
        {
        }

        public FileMrfFadingTableWw(string filePath)
            : base(filePath)
        {
        }

        public FileMrfFadingTableWw(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "MRF";
        }

        protected override void parseInit(Stream stream)
        {
            if (Length % BlockLength != 0)
            {
                throwParseError(string.Format("File length '{0}' isn't a multiple of '{1}'!", Length, BlockLength));
            }

            //Remap blocks.
            int blockCount = (int)(Length / BlockLength);
            mRemapBlocks = new byte[blockCount][];
            for (int i = 0; i < blockCount; i++)
            {
                mRemapBlocks[i] = stream.readArray(BlockLength);
            }
        }

        public byte getRemapIndex(byte indexSrc, byte indexDst) //Src=pixel to draw, dst=existing pixel to draw over.
        {
            //If an MRF-file has more than one block then the first block is a translucent filter:
            //-If filter value is 0xFF then palette index is not remapped at all.
            //-Else use the filter value to select which of the blocks (after the first) to use on destination index.
            byte val = mRemapBlocks[0][indexSrc];
            if (mRemapBlocks.Length == 1) //One block i.e. no filter present?
            {
                return val; //Normal remap of index.
            }
            else if (val != 0xFF) //Index is remapped in the filter?
            {
                return mRemapBlocks[val + 1][indexDst]; //Use block indicated by filter value to remap destination index.
            }
            return indexSrc; //Index is not remapped in the filter.
        }

        public byte[][] getRemapBlocks()
        {
            //See "getRemapIndex() function above for how to use the remap block(s).
            return mRemapBlocks;
        }

        public void debugSaveRemapBlocks()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mRemapBlocks.Length; i++)
            {
                byte[] block = mRemapBlocks[i];
                for (int j = 0; j < block.Length; j++)
                {
                    if (j % 16 == 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(block[j].ToString("X2") + ",");
                }
                sb.AppendLine();
            }
            string folderPath = Program.DebugOutPath + "mrf\\";
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(folderPath + Name + ".txt", sb.ToString());
        }
    }
}
