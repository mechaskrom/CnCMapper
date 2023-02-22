using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Crypt
{
    //http://xhp.xwis.net/documents/MIX_Format.html
    //http://www.shikadi.net/moddingwiki/MIX_Format_(Westwood)
    //https://forums.cncnet.org/topic/2088-encrypted-mix-file-reading/page/2/?tab=comments#comment-21357
    //https://en.wikipedia.org/wiki/X.690#DER_encoding

    //TODO: Replace BigInteger with something better? Preferably something supported by net 3.5.
    //net 4.0 has a built in BigInteger.
    //I tried the System.Security.Cryptography.RSACryptoServiceProvider class, but its very
    //finicky about the RSA parameters and I never figured out ones that it accepted.

    static class BlowfishKey
    {
        //The blowfish key in encrypted MIX-files is itself encrypted with RSA.

        //Quoting http://www.shikadi.net/moddingwiki/MIX_Format_(Westwood):
        //"...the next 80 bytes are two RSA encrypted blocks that need to be decrypted with
        //the Westwood public key. For the purposes of the RSA algorithm, the blocks should be
        //treated as 2 40 byte "big" integers in little endian byte order. 0x10001 should be used
        //as the public exponent for the decryption."

        //Private key is only needed for encrypting.
        //Quoting http://www.shikadi.net/moddingwiki/MIX_Format_(Westwood):
        //"The keys are Base64 encoded and DER encoded big endian "big" integers and represent
        //the modulus and the private exponent respectively in the RSA encryption scheme."
        private const string PublicKeyString = "AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V";
        //private const string PrivateKeyString = "AigKVje8mROcR8QixnxUEF5b29Curkq01DNDWCdOG99XBqH79OaCiTCB";

        private static readonly BigInteger mRsaMod = toBig(PublicKeyString);
        private static readonly BigInteger mRsaPubExp = new BigInteger(0x10001);
        //private static readonly BigInteger mRsaPriExp = toBig(PrivateKeyString);

        public static byte[] decrypt(byte[] src)
        {
            //Source is two 40 byte big ints in little endian. BigInteger uses big endian so reverse them.
            BigInteger rsaMsg1 = new BigInteger(src.Take(40).Reverse().ToArray());
            BigInteger rsaMsg2 = new BigInteger(src.Skip(40).Reverse().ToArray());
            BigInteger rsaDec1 = rsaMsg1.modPow(mRsaPubExp, mRsaMod);
            BigInteger rsaDec2 = rsaMsg2.modPow(mRsaPubExp, mRsaMod);

            //Concatenate decrypted big ints and reverse bytes to convert back to little endian.
            return rsaDec2.getBytes().Concat(rsaDec1.getBytes()).Reverse().ToArray();
        }

        //TODO: Figure out why encrypt doesn't work?
        //public static byte[] encrypt(byte[] src)
        //{
        //    //Encrypt doesn't work!!! But it should be something like this.

        //    //BigInteger rsaMsg1 = new BigInteger(src.Take(40).Reverse().ToArray());
        //    //BigInteger rsaMsg2 = new BigInteger(src.Skip(40).Reverse().ToArray());

        //    byte[] src1 = src.Take(40).Reverse().ToArray();
        //    byte[] src2 = new byte[40];
        //    Array.Copy(src, 40, src2, 0, src.Length - 40);
        //    src2 = src2.Reverse().ToArray();
        //    BigInteger rsaMsg1 = new BigInteger(src1);
        //    BigInteger rsaMsg2 = new BigInteger(src2);

        //    BigInteger rsaEnc1 = rsaMsg1.modPow(mRsaPriExp, mRsaMod);
        //    BigInteger rsaEnc2 = rsaMsg2.modPow(mRsaPriExp, mRsaMod);

        //    return rsaEnc2.getBytes().Concat(rsaEnc1.getBytes()).Reverse().ToArray();
        //}

        private static BigInteger toBig(string keyString)
        {
            //Not a complete DER-decoder. Just enough to read big integer from public and private key.
            byte[] keyDer = Convert.FromBase64String(keyString);
            if (keyDer[0] != 2) //Not a primitive integer type?
            {
                throw new ArgumentException("Key string is not a DER-encoded primitive integer!");
            }
            int keyDerContentLength = keyDer[1];
            if ((keyDerContentLength & 0x80) != 0) //Not using definite short length?
            {
                throw new NotSupportedException("Only DER-encoded definite short length supported!");
            }
            //Content is big endian integer so we can copy it directly to BigInteger.
            return new BigInteger(keyDer.Skip(2).Take(keyDerContentLength).ToArray());
        }
    }
}
