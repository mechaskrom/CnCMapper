using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Crypt
{
    //Westwood's XOR Delta compression algorithm.
    //http://www.shikadi.net/moddingwiki/Westwood_XOR_Delta
    //http://xhp.xwis.net/documents/ccfiles4.txt
    //https://github.com/OpenDUNE/OpenDUNE/tree/master/src/codec
    static class Format40 //Westwood XOR Delta.
    {
        public static void decode(byte[] dst, Stream stream)
        {
            decode(dst, 0, stream);
        }

        public static void decode(byte[] dst, int dstIndex, Stream stream)
        {
            //Starts decoding from stream's current position and stores result in dst array at dst index.
            //The dst array must already contain decoded data to XOR stream with.
            if (dst.Length == 0)
            {
                return;
            }

            while (true)
            {
                byte b1 = stream.readUInt8();
                if (b1 > 0x80) //Command 1: Short skip.
                {
                    //1CCCCCCC -> C=count.
                    int count = b1 & 0x7F;
                    dstIndex += count;
                }
                else if (b1 == 0x80)
                {
                    int count = stream.readUInt16();
                    if (count == 0) //0x80,0x00,0x00 -> Exit code.
                    {
                        break;
                    }
                    else if (count < 0x8000) //Command 2: Long skip.
                    {
                        //10000000,0CCCCCCC,CCCCCCCC -> C=count.
                        dstIndex += count;
                    }
                    else if (count < 0xC000) //Command 3: Long XOR.
                    {
                        //10000000,10CCCCCC,CCCCCCCC -> C=count.
                        count &= 0x3FFF;
                        for (int i = 0; i < count; i++, dstIndex++)
                        {
                            dst[dstIndex] ^= stream.readUInt8();
                        }
                    }
                    else //if(count >= 0xC000) //Command 4: XOR with value.
                    {
                        //10000000,11CCCCCC,CCCCCCCC,VVVVVVVV -> C=count, V=value.
                        count &= 0x3FFF;
                        byte value = stream.readUInt8();
                        for (int i = 0; i < count; i++, dstIndex++)
                        {
                            dst[dstIndex] ^= value;
                        }
                    }
                }
                else if (b1 > 0x00) //Command 5: Short XOR.
                {
                    //0CCCCCCC -> C=count.
                    int count = b1;
                    for (int i = 0; i < count; i++, dstIndex++)
                    {
                        dst[dstIndex] ^= stream.readUInt8();
                    }
                }
                else //if(b1 == 0x00) //Command 6: Long XOR with value.
                {
                    //00000000,CCCCCCCC,VVVVVVVV -> C=count, V=value.
                    int count = stream.readUInt8();
                    byte value = stream.readUInt8();
                    for (int i = 0; i < count; i++, dstIndex++)
                    {
                        dst[dstIndex] ^= value;
                    }
                }
            }
        }
    }
}
