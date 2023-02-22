using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Crypt
{
    //Westwood's RLE-Zero compression algorithm. The version used in Dune 2 SHP-files.
    //https://moddingwiki.shikadi.net/wiki/Westwood_RLE-Zero
    static class RleZeroD2
    {
        public static byte[] decode(byte[] srcData, int width, int height)
        {
            //Usually for compressing transparent pixels (index 0) in sprites.
            //Compressed data is read byte-by-byte per line. If a zero byte is
            //encountered then the next byte indicates how many zeroes to output.
            int srcInd = 0;
            int srcIndEnd = srcData.Length;
            byte[] dstData = new byte[width * height];
            for (int y = 0; y < height; y++) //Decode data in lines.
            {
                int dstInd = y * width; //Start of line to output.
                int dstIndEnd = dstInd + width; //End of line to output.
                while (dstInd < dstIndEnd && srcInd < srcIndEnd)
                {
                    byte b = srcData[srcInd++];
                    if (b == 0) //Zero byte encountered?
                    {
                        if (srcInd < srcIndEnd)
                        {
                            //Next byte indicates how many zeroes to output.
                            int zeroCount = srcData[srcInd++];
                            for (; zeroCount > 0 && dstInd < dstIndEnd; zeroCount--)
                            {
                                dstData[dstInd++] = 0;
                            }
                        }
                    }
                    else //Write byte to output.
                    {
                        dstData[dstInd++] = b;
                    }
                }
            }
            return dstData;
        }

        public static byte[] encode(byte[] srcData, int width, int height)
        {
            if (srcData.Length != width * height)
            {
                throw new ArgumentException("Source data length doesn't match width * height!");
            }

            MemoryStream dstMs = new MemoryStream();
            for (int y = 0; y < height; y++) //Encode data in lines.
            {
                int srcInd = y * width; //Start of line to input.
                int srcIndEnd = srcInd + width; //End of line to input.
                while (srcInd < srcIndEnd)
                {
                    byte b = srcData[srcInd++];
                    if (b == 0) //Zero byte encountered?
                    {
                        //Check how many times zero is repeated.
                        byte zeroCount = 1;
                        for (; zeroCount < 255 && srcInd < srcIndEnd; zeroCount++, srcInd++)
                        {
                            if (srcData[srcInd] != 0)
                            {
                                break;
                            }
                        }
                        dstMs.writeUInt8(0); //Write zero byte flag...
                        dstMs.writeUInt8(zeroCount); //...and count.
                    }
                    else //Write byte to output.
                    {
                        dstMs.writeUInt8(b);
                    }
                }
            }
            return dstMs.ToArray();
        }
    }
}
