using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Some support for Red Alert's fixed point math type. It is represented by a UInt32 consisting of two parts,
    //a UInt16 whole part and a UInt16 fraction part. It only supports values from 0 to 65536.
    //It is used instead of a floating point type.
    struct FixedMath
    {
        private const UInt32 Precision = 65536;

        public static readonly FixedMath Zero = fraction(0, 0); //0/0=0.0
        public static readonly FixedMath Frac1_2 = fraction(1, 2); //1/2=0.5

        private UInt32 mValue;

        public FixedMath(UInt32 wholePart, UInt32 fractionPart) //value.fraction->fixed
        {
            mValue = (UInt32)(((UInt64)wholePart * Precision) + fractionPart);
        }

        public static FixedMath fraction(UInt32 numerator, UInt32 denominator) //numerator/denominator->fixed
        {
            FixedMath fixedResult;
            if (denominator == 0)
            {
                fixedResult.mValue = 0;
            }
            else
            {
                fixedResult.mValue = (UInt32)(((UInt64)numerator * Precision) / (UInt64)denominator);
            }
            return fixedResult;
        }

        public static UInt32 mulFrac(UInt32 factor, UInt32 numerator, UInt32 denominator) //factor*(numerator/denominator)->uint
        {
            return fraction(numerator, denominator).mul(factor).toUInt32();
        }

        public FixedMath mul(UInt32 factor) //factor*fixed->fixed
        {
            FixedMath fixedFactor = new FixedMath(factor, 0);
            FixedMath fixedResult;
            fixedResult.mValue = (UInt32)(((UInt64)fixedFactor.mValue * mValue) / Precision);
            return fixedResult;
        }

        public UInt32 toUInt32() //fixed (wholepart + fracpart rounded) to uint.
        {
            return (UInt32)(((UInt64)mValue + (Precision >> 1)) / Precision);
        }

        public override int GetHashCode()
        {
            return mValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return mValue.Equals(obj);
        }

        public static bool operator ==(FixedMath lhs, FixedMath rhs)
        {
            return lhs.mValue == rhs.mValue;
        }

        public static bool operator !=(FixedMath lhs, FixedMath rhs)
        {
            return lhs.mValue != rhs.mValue;
        }

        public static bool operator <=(FixedMath lhs, FixedMath rhs)
        {
            return lhs.mValue <= rhs.mValue;
        }

        public static bool operator >=(FixedMath lhs, FixedMath rhs)
        {
            return lhs.mValue >= rhs.mValue;
        }

        public static bool tryParse(string value, out FixedMath result)
        {
            //X%, X, .X, Y.X are valid fixed math values.
            if (value.Length > 2 && value.EndsWith("%"))
            {
                UInt32 percent;
                if (UInt32.TryParse(value.Substring(0, value.Length - 1), out percent))
                {
                    result = fraction(percent, 100);
                    return true;
                }
            }
            else
            {
                string[] parts = value.Split('.');
                if (parts.Length == 1) //No dot in value?
                {
                    UInt32 wholePart;
                    if (UInt32.TryParse(value, out wholePart))
                    {
                        result = new FixedMath(wholePart, 0);
                        return true;
                    }
                }
                else if (parts.Length == 2) //One dot in value?
                {
                    //Treat no digits before dot as 0 i.e. ".X" = "0.X".
                    UInt32 wholePart = 0;
                    UInt32 fracPart = 0;
                    if ((parts[0].Length == 0 || UInt32.TryParse(parts[0], out wholePart)) &&
                        (parts[1].Length > 0 && UInt32.TryParse(parts[1], out fracPart)))
                    {
                        fracPart = fraction(fracPart, (UInt32)Math.Pow(10, parts[1].Length)).mValue;
                        result = new FixedMath(wholePart, fracPart);
                        return true;
                    }
                }
            }
            result.mValue = 0;
            return false;
        }

        public static void debugTest()
        {
            //Test if same result as game's fixed math version.
            for (ushort max = 0; max < 65535; max++)
            //for (ushort max = 0; max < 256; max++)
            {
                for (int str = 0; str <= 256; str++)
                {
                    UInt32 strGood = debugMulFracVerified(max, str, 256);

                    UInt32 strTest = mulFrac(max, (UInt32)str, 256);
                    if (strGood != strTest)
                    {
                        continue; //Set breakpoint here or throw exception.
                    }
                }
            }
        }

        private static UInt32 debugMulFracVerified(int factor, int numerator, int denominator) //factor*(numerator/denominator)
        {
            //Same result as game. Checked in source. Returns 0 if denominator is 0.
            UInt32 rvalue = (UInt32)(((UInt64)numerator * 65536) / (UInt64)denominator); //fixed(strength, 256)
            UInt32 lvalue = (UInt32)(((UInt16)factor) * 65536); //int to fixed
            UInt32 res = (UInt32)(((UInt64)lvalue * rvalue) / 65536); //fixed*fixed
            return (UInt32)(((UInt64)res + (65536 >> 1)) / 65536); //fixed to uint
        }

        public static void debugTestParse()
        {
            string[] values = new string[] { "100", "50%", "1.0", ".5", ".50", ".25", "1.", "1.0.0" };
            //Correct output: 100=6553600, 50%=32768, 1.0=65536, .5=32768, .50=32768, .25=16384
            foreach (string value in values)
            {
                FixedMath result;
                if (tryParse(value, out result))
                {
                    Console.WriteLine(value + "=" + result.mValue);
                }
                else
                {
                    continue;
                }
            }
        }
    }

    static class IniKeyExt
    {
        public static FixedMath valueAsFixed(this FileFormat.IniKey iniKey)
        {
            FixedMath result;
            if (FixedMath.tryParse(iniKey.Value, out result))
            {
                return result;
            }
            throw new ArgumentException(string.Format("Couldn't parse '{0}/{1}/{2}' as a fixed point value in INI-file '{3}'",
                iniKey.ParentSection.Id, iniKey.Id, iniKey.Value, iniKey.ParentSection.ParentFile.FullName));
        }
    }
}
