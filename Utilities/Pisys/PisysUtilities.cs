using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Pisys
{
    /// <summary>
    /// import old IC pisys Utilities xlst script
    /// </summary>
    public class PisysUtilities
    {
        public string Utf8HexStringConvert(string stringToChange)
        {
            byte prefixByte = (byte)0x15; // 0x15 signifies that the data is UTF-8, 0x1E would signify UTF-16
            StringBuilder sb = new StringBuilder();
            byte[] byteArray = Encoding.UTF8.GetBytes(stringToChange);
            byte[] utf8Bytes = new byte[byteArray.Length + 1];
            utf8Bytes[0] = prefixByte;
            byteArray.CopyTo(utf8Bytes, 1);

            foreach (byte myByte in utf8Bytes)
            {
                sb.Append(myByte.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string IntegerToHexByteString(int valueToConvert, byte byteSize)
        {
            System.Collections.Generic.List<byte> bytes = new System.Collections.Generic.List<byte>();
            string output = string.Empty;

            if (byteSize != 1 && byteSize != 2 && byteSize != 4)
                throw new Exception(string.Format("Invalid byte size of '{0}' passed to IntegerToHexByteString.\r\n" +
                    "Please ensure that your value will be split into valid byte-sized pieces.  Valid values are 1, 2, and 4.", byteSize));

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(ms))
            {
                writer.Write(valueToConvert);
                bytes.AddRange(ms.ToArray());
            }

            int byteCount = bytes.Count;
            if (byteCount < byteSize)
                for (int i = byteCount; i <= byteSize - byteCount; i++)
                    bytes.Add(0);

            byteCount = bytes.Count;
            if (byteCount > byteSize)
            {
                // Carefully trim off extra zeros from the array
                try
                {
                    for (int i = byteCount - 1; i >= byteSize; i--)
                    {
                        if (bytes[i] > 0)
                            throw new Exception();
                        bytes.RemoveAt(i);
                    }
                }
                catch
                {
                    throw new Exception(string.Format("The conversion of value '{0}' to a hex byte string resulted in a byte count of '{1}'.\r\n" +
                        "This is greater than the byte size that was specified of '{2}'.", valueToConvert, bytes.Count, byteSize));
                }
            }

            for (int i = bytes.Count - 1; i >= 0; i--)
                output += bytes[i].ToString("X").PadLeft(2, '0');

            return output;
        }

        public string GetCurrentKmsMessageTimestamp()
        {
            long timestamp = ConvertToKmsMessageTimestamp(DateTime.Now);
            string result = timestamp.ToString("X");
            return result;
        }

        public string GetKmsMessageTimestamp(string dateTime, string format)
        {
            DateTime dtDateTime = DateTime.ParseExact(dateTime, format, null);
            long timestamp = ConvertToKmsMessageTimestamp(dtDateTime);
            string result = timestamp.ToString("X");
            return result;
        }

        public long ConvertToKmsMessageTimestamp(DateTime dateTime)
        {
            /*
             * (this text is copied from a document from KMS that describes the "MSG_TIMESTAMP (DataTag = 0x0A)")
             * The message timestamp when the SOUSER message is initiated and/or sent (from billing system). 
             * This 40-bit field contains the end datetime in Universal Time, Co-ordinated (UTC) and 
             * Modified Julian Date (MJD). (Refer This field is coded as 16 bits giving the 16 LSBs 
             * of MJD followed by 24 bits coded as 6 digits in 4-bit Binary Coded Decimal (BCD). 
             * 
             * EXAMPLE 1: message timestamp 2012/05/13 12:45:00 is coded as "0xDAFC124500".
             */

            DateTime utcDateTime = dateTime.ToUniversalTime();
            double julianDate = getJulianDay_DateTime(utcDateTime);
            double mjd = getModifiedJulian_Julian(julianDate);

            long result = (long)mjd; // trucate any time portion.

            // now add the time portion of our date as a BCD.
            result <<= 4;
            result += utcDateTime.Hour / 10;
            result <<= 4;
            result += utcDateTime.Hour % 10;
            result <<= 4;
            result += utcDateTime.Minute / 10;
            result <<= 4;
            result += utcDateTime.Minute % 10;
            result <<= 4;
            result += utcDateTime.Second / 10;
            result <<= 4;
            result += utcDateTime.Second % 10;

            //string xxHexString = result.ToString("X");

            return result;
        }

        public double getJulianDay_DateTime(DateTime date)
        {
            //Calculate the day of the year and the
            double dDay = date.DayOfYear;
            dDay += Convert.ToDouble(date.Hour) / 24;
            dDay += Convert.ToDouble(date.Minute) / 1440;
            dDay += Convert.ToDouble(date.Second) / 86400;

            return dDay + getJulianDay_Year(date.Year);
        }

        public double getJulianDay_Year(int year)
        {
            double dYear = year - 1;
            double A = Math.Floor(dYear / 100);
            double B = 2 - A + Math.Floor(A / 4);
            //The use of 30.600000000000001 is to correct for floating point rounding problems
            double dResult = Math.Floor(365.25 * dYear) + 1721422.9 + B;
            return dResult;
        }

        public long ConvertToJulian(DateTime dt)
        {
            int m = dt.Month;
            int d = dt.Day;
            int y = dt.Year;


            if (m < 3)
            {
                m = m + 12;
                y = y - 1;
            }
            long jd = d + (153 * m - 457) / 5 + 365 * y + (y / 4) - (y / 100) + (y / 400) + 1721119;
            return jd;
        }

        public DateTime ConvertFromJulian(int m_JulianDate)
        {
            long L = m_JulianDate + 68569;
            long N = (long)((4 * L) / 146097);
            L = L - ((long)((146097 * N + 3) / 4));
            long I = (long)((4000 * (L + 1) / 1461001));
            L = L - (long)((1461 * I) / 4) + 31;
            long J = (long)((80 * L) / 2447);
            int Day = (int)(L - (long)((2447 * J) / 80));
            L = (long)(J / 11);
            int Month = (int)(J + 2 - 12 * L);
            int Year = (int)(100 * (N - 49) + I + L);

            DateTime dt = new DateTime(Year, Month, Day);
            return dt;
        }

        public double getJulianDay_SatEpoch(int year, double dSatelliteEpoch)
        {
            //Tidy up the year and put it into the correct century
            year = year % 100;
            if (year < 57) year += 2000;
            else year += 1900;

            double dResult = getJulianDay_Year(year);
            dResult += dSatelliteEpoch;

            return dResult;
        }

        public double getModifiedJulian_Julian(double dJulian)
        {
            return dJulian - 2400000.5;
        }

        public double getJulian_ModifiedJulian(double dModifiedJulian)
        {
            return dModifiedJulian + 2400000.5;
        }

        public int CompareDate(string strDate1, string strDate2)
        {
            DateTime date1;
            DateTime date2;

            bool result1 = DateTime.TryParseExact(strDate1, "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date1);
            bool result2 = DateTime.TryParseExact(strDate2, "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date2);

            return DateTime.Compare(date1, date2);
        }

        public string GetTodaysDate()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        public string PadLeft(string text, string character, int lenOfString)
        {
            return text.PadLeft(lenOfString, character[0]);
        }

        public string PadRight(string text, string character, int lenOfString)
        {
            return text.PadRight(lenOfString, character[0]);
        }

        public string StringToHex(string input)
        {
            string hex = string.Empty;
            foreach (char c in input)
            {
                int tempChar = c;
                hex += String.Format("{0:X2}", (uint)Convert.ToUInt32(tempChar.ToString()));
            }
            return hex;
        }

        public string HexToString(string hexString)
        {
            StringBuilder convertedString = new StringBuilder();

            int hexStringLength = hexString.Length;

            for (int i = 1; i <= hexStringLength; i += 2)
            {
                convertedString.Append(Char.ConvertFromUtf32(Convert.ToInt32(hexString.Substring(0, 2), 16)));
                hexString = hexString.Substring(2);
            }
            return convertedString.ToString();
        }

        public string IntToBinaryString(int integerValue, int length)
        {
            string binaryValue = string.Empty;
            binaryValue = Convert.ToString(integerValue, 2);

            int temp = binaryValue.Length % length;

            if (temp != 0)
                binaryValue = new string('0', length - temp) + binaryValue;

            return binaryValue;
        }

        public string IntToHex(int integerValue)
        {
            string hexValue = string.Empty;
            hexValue = string.Format("{0:X2}", integerValue);

            return hexValue;
        }

        public string SumHexString(string hexString)
        {
            int decimalEquivalent = 0;
            int hexStringLength = hexString.Length;

            for (int i = 1; i <= hexStringLength; i += 2)
            {
                decimalEquivalent += Convert.ToInt32(hexString.Substring(0, 2), 16);
                hexString = hexString.Substring(2);
            }
            return DecimalToHex(decimalEquivalent);
        }

        public string DecimalToHex(int decimalValue)
        {
            return string.Format("{0:X2}", decimalValue);
        }

        public int HexToInt(string input)
        {
            return Convert.ToInt32(input, 16);
        }

        public string CalculateChecksum(string hexString)
        {
            string checksumInHex = string.Empty;

            string sumOfHexString = SumHexString(hexString);
            int integerValue = HexToInt(sumOfHexString);
            int twosComplement = CalculateTwosComplement(integerValue);
            checksumInHex = IntToHex(twosComplement);
            checksumInHex = checksumInHex.Substring(checksumInHex.Length - 2, 2).ToUpper();

            return checksumInHex;
        }

        public string HexToBinary(string hexValue)
        {
            string binaryValue = string.Empty;
            binaryValue = Convert.ToString(Convert.ToInt32(hexValue, 16), 2);

            int temp = binaryValue.Length % 4;

            if (temp != 0)
                binaryValue = new string('0', 4 - temp) + binaryValue;

            return binaryValue;
        }

        public string BinaryToHex(string binaryValue)
        {
            int temp = binaryValue.Length % 4;
            if (temp != 0)
                binaryValue = new string('0', 4 - temp) + binaryValue;

            string hexValue = string.Empty;

            for (int i = 0; i <= binaryValue.Length - 4; i += 4)
                hexValue += string.Format("{0:X}", Convert.ToByte(binaryValue.Substring(i, 4), 2));

            return hexValue;
        }

        public int CalculateTwosComplement(int integerValue)
        {
            integerValue = ~integerValue;
            integerValue += 1;
            return integerValue;
        }

        public string FormatCurrencyWithoutCode(double money, string cultureName)
        {
            return String.Format(System.Globalization.CultureInfo.CreateSpecificCulture(cultureName), "{0:N}", money);
        }

        public string FormatCurrencyWithCode(double money, string cultureName)
        {
            return String.Format(System.Globalization.CultureInfo.CreateSpecificCulture(cultureName), "{0:C}", money);
        }

        public string FormatDate(string date, string format)
        {
            return DateTime.Parse(date).ToString(format);
        }

        public string Substring(string inputString, int length)
        {
            if (inputString.Length > length)
                return inputString.Substring(0, length);
            else
                return inputString;
        }

        public string RemoveCheckDigit(string inputString, int checkDigitLength)
        {
            if (inputString.Length < checkDigitLength)
                throw new Exception(string.Format("Length of SerialNumber {0} is smaller than the length of the Check Digit {1}", inputString, checkDigitLength));

            return inputString.Substring(0, inputString.Length - checkDigitLength);
        }
    }
}
