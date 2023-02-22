using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Crypt
{
    //Westwood's LCW compression algorithm.
    //http://www.shikadi.net/moddingwiki/Westwood_LCW
    //http://xhp.xwis.net/documents/ccfiles4.txt
    //https://github.com/OpenDUNE/OpenDUNE/tree/master/src/codec
    static class Format80 //Westwood LCW.
    {
        public static void decode(byte[] dst, Stream stream)
        {
            decode(dst, 0, stream);
        }

        public static void decode(byte[] dst, int dstIndex, Stream stream)
        {
            //Starts decoding from stream's current position and stores result in dst array at dst index.
            if (dst.Length == 0) //Nothing to decode?
            {
                return;
            }

            int dstOrgIndex = dstIndex; //Store original dst index. Needed for absolute offsets.
            bool isRelative = stream.readUInt8() == 0; //Command 3 and 5 uses absolute or relative offsets?
            if (!isRelative)
            {
                stream.Seek(-1, SeekOrigin.Current);
            }
            while (true)
            {
                byte b1 = stream.readUInt8();
                if (b1 < 0x80) //Command 2: Existing block relative copy.
                {
                    //0CCCPPPP,PPPPPPPP -> C=count-3, P=offset (4 MSB of offset in first byte).
                    //Source = dst[dstIndex - offset].
                    int count = (b1 >> 4) + 3;
                    byte b2 = stream.readUInt8();
                    int offset = ((b1 & 0x0F) << 8) | b2;
                    int srcIndex = dstIndex - offset;
                    for (int i = 0; i < count; i++, dstIndex++, srcIndex++)
                    {
                        dst[dstIndex] = dst[srcIndex];
                    }
                }
                else if (b1 < 0xC0) //Command 1: Short copy.
                {
                    //10CCCCCC -> C=count.
                    //Source = src[srcIndex].
                    int count = b1 & 0x3F;
                    if (count == 0) //0x80 -> Exit code.
                    {
                        break;
                    }
                    stream.Read(dst, dstIndex, count);
                    dstIndex += count;
                }
                else if (b1 < 0xFE) //Command 3: Existing block medium-length copy.
                {
                    //11CCCCCC,PPPPPPPP,PPPPPPPP -> C=count-3, P=offset.
                    //Source = dst[dstIndex - offset] if relative.
                    //Source = dst[offset] if absolute.
                    int count = (b1 & 0x3F) + 3;
                    int offset = stream.readUInt16();
                    int srcIndex = isRelative ? dstIndex - offset : dstOrgIndex + offset;
                    for (int i = 0; i < count; i++, dstIndex++, srcIndex++)
                    {
                        dst[dstIndex] = dst[srcIndex];
                    }
                }
                else if (b1 < 0xFF) //Command 4: Repeat value.
                {
                    //11111110,CCCCCCCC,CCCCCCCC,VVVVVVVV -> C=count, V=value.
                    //Source = value.
                    int count = stream.readUInt16();
                    byte value = stream.readUInt8();
                    for (int i = 0; i < count; i++, dstIndex++)
                    {
                        dst[dstIndex] = value;
                    }
                }
                else //if (b1 == 0xFF) //Command 5: Existing block long copy.
                {
                    //11111111,CCCCCCCC,CCCCCCCC,PPPPPPPP,PPPPPPPP -> C=count, P=offset.
                    //Source = dst[dstIndex - offset] if relative.
                    //Source = dst[offset] if absolute.
                    int count = stream.readUInt16();
                    int offset = stream.readUInt16();
                    int srcIndex = isRelative ? dstIndex - offset : dstOrgIndex + offset;
                    for (int i = 0; i < count; i++, dstIndex++, srcIndex++)
                    {
                        dst[dstIndex] = dst[srcIndex];
                    }
                }
            }
        }

        public static byte[] encode(Stream stream)
        {
            return encode(stream, stream.Length);
        }

        public static byte[] encode(Stream stream, long lengthToEncode)
        {
            //Starts encoding from stream's current position and reads "lengthToEncode" bytes from it.
            if (lengthToEncode < 1 || lengthToEncode > int.MaxValue)
            {
                throw new ArgumentException("Length to encode must be between '1' and '" + int.MaxValue + "'!");
            }
            if ((stream.Position + lengthToEncode) > stream.Length)
            {
                throw new ArgumentException("Stream is too short to encode specified length from its current position!");
            }

            return encode(stream.readArray(lengthToEncode));
        }

        public static byte[] encode(byte[] src, int srcIndex, long lengthToEncode)
        {
            //Starts encoding from "srcIndex" and reads "lengthToEncode" bytes from "src".
            if (lengthToEncode < 1 || lengthToEncode > int.MaxValue)
            {
                throw new ArgumentException("Length to encode must be between '1' and '" + int.MaxValue + "'!");
            }
            if ((srcIndex + lengthToEncode) > src.Length)
            {
                throw new ArgumentException("Source is too short to encode specified length from its current position!");
            }

            if (srcIndex == 0 && lengthToEncode == src.Length)
            {
                return encode(src);
            }
            byte[] slice = new byte[lengthToEncode];
            Buffer.BlockCopy(src, srcIndex, slice, 0, slice.Length);
            return encode(slice);
        }

        public static byte[] encode(byte[] src)
        {
            //Not very well tested, especially relative mode on large (>64K) files.
            //Seems to produce valid format 80 encoded data at decent compression so far though.

            if (src.LongLength < 1 || src.LongLength > int.MaxValue)
            {
                throw new ArgumentException("Source length to encode must be between '1' and '" + int.MaxValue + "'!");
            }

            bool isRelative = src.Length > UInt16.MaxValue; //Use relative offsets for command 3 and 5?
            int srcPos = 0;
            MemoryStream encoded = new MemoryStream();

            //Encoded data with relative offsets start with 0 as a flag to the decoder.
            if (isRelative)
            {
                encoded.writeUInt8(0);
            }

            //Encoded data should start with a command 1.
            byte[] copyCmd = new byte[64]; //Command byte + up to 63 bytes to copy.
            copyCmd[0] = 0x81;
            copyCmd[1] = src[srcPos++];

            while (srcPos < src.Length)
            {
                //Check how many times the next value is repeated. Maybe repeat value command 4 can be used.
                int repeatCount = 1;
                byte repeatValue = src[srcPos];
                while ((srcPos + repeatCount) < src.Length && repeatCount < UInt16.MaxValue &&
                    src[srcPos + repeatCount] == repeatValue)
                {
                    repeatCount++;
                }

                //A block copy starting at the last encoded byte (source position - 1) will repeat it
                //and can be used instead of command 4. A block copy must start before current source
                //position, but command 1 (there's a good chance one is in progress) can be used first
                //to copy the byte to repeat.
                //Command 3 is 3 bytes long and can copy 64 (61+3) bytes -> 67 bytes total (61+3+3).
                //A command 1 to add the repeat value brings the total to 69 (67+2) bytes.
                //Command 4 is 4 bytes long which means that it's only better than command 3 if
                //it can repeat at least 65 bytes (69-4).

                //Check the next values for longest sequence matching already encoded values. Maybe
                //one of the block copy commands can be used.
                int blockLengthMax = 1;
                int blockStartMax = 0;
                if (repeatCount < 65) //Command 4 not already best to use?
                {
                    //Determine where to start checking for blocks in already encoded values.
                    int blockStart = isRelative ? Math.Max(0, srcPos - UInt16.MaxValue) : 0;
                    while (blockStart < srcPos) //Examine already encoded values.
                    {
                        //Values before source position will be available when decoding.
                        //Which means that it's fine to let a block run past current source
                        //position as long as it just starts before it.
                        int blockLength = 0;
                        while ((srcPos + blockLength) < src.Length && blockLength < UInt16.MaxValue &&
                            src[blockStart + blockLength] == src[srcPos + blockLength])
                        {
                            blockLength++;
                        }
                        //Update max even if equal to keep position near current
                        //so the relative offset is as small as possible.
                        if (blockLength >= blockLengthMax) //An equal or longer matching sequence was found?
                        {
                            blockLengthMax = blockLength;
                            blockStartMax = blockStart;
                        }
                        blockStart++; //Shift start of block and check again.
                    }
                }

                //Write best command (least bytes used) for encoding next value(s).
                if (repeatCount < 65 && blockLengthMax < 3) //Command 1?
                {
                    //Command 1: Short copy.
                    //10CCCCCC -> C=count.
                    if (copyCmd[0] == 0xBF) //A command 1 is in progress that can't fit more data?
                    {
                        //Write encoded data and start a new one.
                        encoded.Write(copyCmd, 0, 0x3F + 1);
                        copyCmd[0] = 0x80;
                    }
                    copyCmd[0]++; //Increase count in command 1.
                    copyCmd[copyCmd[0] & 0x3F] = src[srcPos++]; //Add next byte to command 1.
                }
                else //Command 2, 3, 4 or 5?
                {
                    //Write any command 1 in progress.
                    writeCopyCmd(copyCmd, encoded);

                    int relativePos = srcPos - blockStartMax;
                    int blockPos = isRelative ? relativePos : blockStartMax;
                    if (blockLengthMax <= 10 && relativePos <= 4095 && repeatCount < 65) //Command 2?
                    {
                        //Command 2: Existing block relative copy.
                        //0CCCPPPP,PPPPPPPP -> C=count-3, P=offset (4 MSB of offset in first byte).
                        encoded.writeUInt8((byte)(((blockLengthMax - 3) << 4) | (relativePos >> 8)));
                        encoded.writeUInt8((byte)relativePos);
                        srcPos += blockLengthMax;
                    }
                    else if (blockLengthMax <= 64 && repeatCount < 65) //Command 3?
                    {
                        //Command 3: Existing block medium-length copy.
                        //11CCCCCC,PPPPPPPP,PPPPPPPP -> C=count-3, P=offset.
                        encoded.writeUInt8((byte)(0xC0 | (blockLengthMax - 3)));
                        encoded.writeUInt16((UInt16)blockPos);
                        srcPos += blockLengthMax;

                        //Command 3 has a 6-bit counter, but values 62 and 63 are used to
                        //denote command 4 and 5, so max count for it is actually 64 (61+3).
                    }
                    else if (repeatCount >= 65) //Command 4?
                    {
                        //Command 4: Repeat value
                        //11111110,CCCCCCCC,CCCCCCCC,VVVVVVVV -> C=count, V=value.
                        encoded.writeUInt8(0xFE);
                        encoded.writeUInt16((UInt16)repeatCount);
                        encoded.writeUInt8(repeatValue);
                        srcPos += repeatCount;
                    }
                    else //Command 5.
                    {
                        //Command 5: Existing block long copy.
                        //11111111,CCCCCCCC,CCCCCCCC,PPPPPPPP,PPPPPPPP -> C=count, P=offset.
                        encoded.writeUInt8(0xFF);
                        encoded.writeUInt16((UInt16)blockLengthMax);
                        encoded.writeUInt16((UInt16)blockPos);
                        srcPos += blockLengthMax;
                    }
                }
            }
            //Write any command 1 in progress.
            writeCopyCmd(copyCmd, encoded);
            encoded.writeUInt8(0x80); //Exit code.
            return encoded.ToArray();
        }

        private static void writeCopyCmd(byte[] copyCmd, MemoryStream encoded)
        {
            if (copyCmd[0] > 0x80)
            {
                encoded.Write(copyCmd, 0, (copyCmd[0] & 0x3F) + 1); //Write count+command bytes.
                copyCmd[0] = 0x80;
            }
        }
    }
}
