using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Helper
{
    public static class StringUtilities
    {
        public static string IntegerToHexByteString(int valueToConvert, byte byteSize)
        {
            if (byteSize != 1 && byteSize != 2 && byteSize != 4)
                throw new IntegrationException(string.Format("Invalid byte size of '{0}' passed to IntegerToHexByteString.\r\n" +
                    "Please ensure that your value will be split into valid byte-sized pieces.  Valid values are 1, 2, and 4.", byteSize));

            byte[] bytes = new byte[byteSize];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i == bytes.Length - 1)
                {
                    bytes[i] = (byte)(valueToConvert & 0x00FF);
                }
                else
                {
                    bytes[i] = (byte)(valueToConvert / (256 * (bytes.Length - i - 1)));
                }
            }
            return Encoding.ASCII.GetString(bytes, 0, byteSize);
        }


        public static string ToHexString(string input)
        {
            var hex = string.Empty;
            foreach (var c in input)
            {
                int tempChar = c;
                hex += String.Format("{0:X2}", Convert.ToUInt32(tempChar.ToString()));
            }
            return hex;
        }

        public static int HexByteStringToInteger(string hexByteStringToConvert, byte byteSize)
        {
            if (byteSize != 1 && byteSize != 2 && byteSize != 4)
                throw new IntegrationException(string.Format("Invalid byte size of '{0}' passed to HexByteStringToInteger.\r\n" +
                    "Please ensure that your hex string has a valid number of byte-sized pieces.  Valid byte size values are 1, 2, and 4.", byteSize));

            if (hexByteStringToConvert.Length > byteSize)
                throw new IntegrationException(string.Format("The conversion of value '{0}' to an integer resulted in a byte count of '{1}'.\r\n" +
                    "This is greater than the byte size that was specified of '{2}'.", hexByteStringToConvert, hexByteStringToConvert.Length, byteSize));

            byte[] bytes = new byte[hexByteStringToConvert.Length];

            for (int i = 0; i < hexByteStringToConvert.Length; i++)
                bytes[i] = (byte)hexByteStringToConvert[i];

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            int result = 0;
            switch (byteSize)
            {
                case 1:
                    result = bytes[0];
                    break;
                case 2:
                    result = BitConverter.ToInt16(bytes, 0);
                    break;
                case 4:
                    result = BitConverter.ToInt32(bytes, 0);
                    break;
            }

            return result;
        }

        public static string IntToBinaryString(int integerValue, int length)
        {
            string binaryValue = string.Empty;
            binaryValue = Convert.ToString(integerValue, 2);

            int temp = binaryValue.Length % length;

            if (temp != 0)
                binaryValue = new string('0', length - temp) + binaryValue;

            return binaryValue;
        }

        public static string BinaryToHex(params string[] values)
        {
            var binaryValue = string.Join("", values);
            int temp = binaryValue.Length % 4;
            if (temp != 0)
                binaryValue = new string('0', 4 - temp) + binaryValue;

            string hexValue = string.Empty;

            for (int i = 0; i <= binaryValue.Length - 4; i += 4)
            {
                hexValue += string.Format("{0:X}", Convert.ToByte(binaryValue.Substring(i, 4), 2));
            }

            return hexValue;
        }


        public static byte[] GetBytesFromHexString(string hexString, out int discarded)
        {
            discarded = 0;
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            foreach (char t in hexString)
            {
                c = t;
                if (IsHexDigit(c))
                    newString += c;
                else
                    discarded++;
            }
            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                discarded++;
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            var bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }


        public static bool IsHexDigit(Char c)
        {
            int numChar;
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            if (numChar >= num1 && numChar < (num1 + 10))
                return true;
            return false;
        }

        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = ParseByte(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

        private static byte ParseByte(string input, System.Globalization.NumberStyles style)
        {
            byte output;
            if (!byte.TryParse(input, style, null, out output))
                throw new Exception(string.Format("Failed to parse value \"{0}\" as an byte.", input));

            return output;
        }
    }
}
