using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Crypt
{
    //Used to pack compressed data as base64 strings inside INI-files.
    //https://forums.cncnet.org/topic/6604-question-about-templates-in-mappack-section/
    //https://cnc.fandom.com/wiki/Red_Alert_File_Formats_Guide
    //https://web.archive.org/web/20070823093103/http://freecnc.org/
    static class Pack
    {
        private const int ChunkLength = 8192;

        public static byte[] decode(string packDataInBase64)
        {
            return decode(Convert.FromBase64String(packDataInBase64));
        }

        public static byte[] decode(byte[] packData)
        {
            //Pack data is a serie of chunks. Each chunk has a 4 byte header followed by data encoded in format80.
            //First UInt16 in chunk header is length of encoded data. Second UInt16 is length of decoded data.
            MemoryStream stream = new MemoryStream(packData);

            //Do a first pass over stream to figure out total length of decoded data.
            int decodedDataLength = 0;
            while (stream.Position < stream.Length)
            {
                //Read chunk header.
                UInt16 encodedLength = stream.readUInt16();
                UInt16 decodedLength = stream.readUInt16(); //Always 8192?
                //Skip encoded chunk data.
                stream.Seek(encodedLength, SeekOrigin.Current);
                decodedDataLength += decodedLength;
            }

            //Do the actual decoding of data in stream.
            byte[] decodedData = new byte[decodedDataLength];
            int decodedDataInd = 0;
            stream.Seek(0, SeekOrigin.Begin);
            while (stream.Position < stream.Length)
            {
                //Read chunk header.
                UInt16 encodedLength = stream.readUInt16();
                UInt16 decodedLength = stream.readUInt16(); //Always 8192?
                //Decode chunk data.
                Format80.decode(decodedData, decodedDataInd, stream);
                decodedDataInd += decodedLength;
            }
            return decodedData;
        }

        public static string encode(byte[] data) //Returns a base64 string of encoded data.
        {
            if (data.Length % ChunkLength != 0)
            {
                throw new ArgumentException("Data length should be a multiple of '" + ChunkLength + "'!");
            }

            MemoryStream encoded = new MemoryStream();
            int chunkCount = data.Length / ChunkLength;
            for (int i = 0; i < chunkCount; i++)
            {
                byte[] encodedData = Format80.encode(data, i * ChunkLength, ChunkLength);
                //Write chunk header.
                encoded.writeUInt16((UInt16)encodedData.Length);
                encoded.writeUInt16(ChunkLength);
                //Write encoded data.
                encoded.writeArray(encodedData);
            }
            return Convert.ToBase64String(encoded.ToArray());
        }
    }
}
