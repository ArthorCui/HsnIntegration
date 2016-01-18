using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;

namespace UnitTest
{
    [TestFixture]
    public class StringTest
    {
        public void StringConstructorTest()
        {
            var value = "0x01".GetValue<string>();
            var character = (char)0.GetValue<char>();
            var size = 8.GetValue<int>();
            var str = new string(character, size - value.Length) + value;
            Console.WriteLine(str);
        }

        public void CharConvertTest()
        {
            char[] chars = new char[4];

            chars[0] = 'X';
            chars[1] = '\x0058';
            chars[2] = (char)88;
            chars[3] = '\u0058';

            foreach (var c in chars)
            {
                Console.WriteLine(c + " ");
            }
        }

        public void PadLeftTest()
        {
            var id = "512";
            Console.WriteLine(id.PadLeft(4,'0'));
            var amount = 10.26;
            //var d_amount = 10E;
            var format_amount = (amount * 100).ToString("#").PadLeft(17, '0');
            Console.WriteLine(format_amount);
            var round_amount = Math.Round(Convert.ToDecimal(amount), 2);
            Console.WriteLine(round_amount);
        }

        [Test]
        public void BankStringSplitTest()
        {
            var inputString = "DTM+7:20071222:102'\r\n";
            string firstPartOfString = inputString.Substring(inputString.IndexOf('+') + 1);
            int dtmPeriodQualifier = int.Parse(firstPartOfString.Substring(0, firstPartOfString.IndexOf(':')));

            string dateFormatQualifier = inputString.Split('+', '\'', ':')[4];
            string inputDate = inputString.Split('+', '\'', ':')[3];

            Console.WriteLine("firstPartOfString: " + firstPartOfString);
            Console.WriteLine("dtmPeriodQualifier: " + dtmPeriodQualifier);
            Console.WriteLine("dateFormatQualifier: " + dateFormatQualifier);
            Console.WriteLine("inputDate: " + inputDate);

            var parsedDate = DateTime.ParseExact("20071222", "yyyyMMdd", null);
            Console.WriteLine(parsedDate);
        }

        [Test]
        public void DecimailTest()
        {
            var d = "-1437.50";
            decimal r = decimal.Parse(d);
            var s = r.ToString("0.##");
            decimal amount = Math.Abs(decimal.Parse(s));
            Console.WriteLine(amount);
            decimal d1 = 0.125m;
            Console.WriteLine(d1.ToString("0.##"));

            decimal t1;
            decimal.TryParse(d, NumberStyles.Any, CultureInfo.InvariantCulture, out t1);
            Console.WriteLine(t1);
            //decimal t = Math.Abs(decimal.Parse(""));
            //Console.WriteLine(t);
        }

        [Test]
        public void testconvert()
        {
            decimal invTotal = 0;

            var tt = ConvertToAmountString(invTotal.ToString("0.##"));
            var total = ConvertToAmountString("-1437.50");

            Console.WriteLine(tt);
            Console.WriteLine(total);

        }

        private bool TryParseExtension(string input)
        {
            decimal value;
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private string ConvertToAmountString(string amountString)
        {
            decimal amount = 0;

            try
            {
                amount = Math.Abs(decimal.Parse(amountString));
            }
            catch (Exception ex)
            {
                /* MTarvin 03/02/2009 Mercury 11113 - IC (Bank and FS) Improve error messages when parsing export files
                 * Set a reference (Customer ID) and print what user the error occured on, helps the user find issue
                 */
                throw new Exception(string.Format("\r\nError parsing the following value to decimal: {0}\r\n\r\n", amountString), ex);
            }

            string amountDataString = string.Format("{0:000000000000000}", amount * 100);

            return amountDataString;
        }
    }

    public static class ObjectExtension
    {
        public static T GetValue<T>(this object value)
        {
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                return (T)Enum.Parse(typeof(T), Convert.ToString(value));
            }
            if (value is IConvertible)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return (T)value;
        }
    }


}
