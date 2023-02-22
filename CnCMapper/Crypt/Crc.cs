using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Crypt
{
    //Used to calculate a id/hash from strings.
    //http://xhp.xwis.net/documents/MIX_Format.html
    //https://web.archive.org/web/20070812201749/http://freecnc.org/dev/mix-format/
    //See "Calculate_CRC()" in "MiscAsm.cpp" in Tiberian Dawn.
    //See "CRCEngine::operator()" in "CRC.CPP" in Red Alert.
    static class Crc
    {
        public static UInt32 crcTDRA(string text) //Tiberian Dawn and Red Alert.
        {
            UInt32 crc = 0;
            for (int index = 0; index < text.Length; )
            {
                //Get next four bytes as an unsigned int.
                UInt32 add = 0;
                for (int i = 0; i < 4 && index < text.Length; i++, index++)
                {
                    add |= (UInt32)text[index] << (i * 8);
                }
                //Left rotate current crc one step and add the unsigned int.
                crc = (crc << 1 | crc >> 31) + add;
            }
            return crc;
        }
    }
}
