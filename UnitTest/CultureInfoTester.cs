using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class CultureInfoTester
    {
        [Test]
        public void currency_by_server_test()
        {
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                Console.WriteLine(string.Format("Name: {0}, English Name: {1}\n", culture.Name, culture.EnglishName));
            }
        }

        [Test]
        public void currency_india_test()
        {
            var culture = CultureInfo.GetCultureInfo("en-IN");

            Assert.AreEqual(".", culture.NumberFormat.CurrencyDecimalSeparator);
            Assert.AreEqual(",", culture.NumberFormat.CurrencyGroupSeparator);
            Assert.AreEqual(new int[] { 3, 2 }, culture.NumberFormat.CurrencyGroupSizes);

        }

        [Test]
        public void currency_regex_match_test()
        {
            var standard_pattern = "^\\s*[+-]?(\\d{1,3}(,\\d{3})*(\\.\\d+)?|\\d+(\\.\\d+)?)\\s*$";

            Assert.AreEqual(true, VerifyMatch("-12,122,100.00", standard_pattern));
            Assert.AreEqual(true, VerifyMatch("-12,122,100.00", standard_pattern));
            Assert.AreEqual(false, VerifyMatch("-121,221,00.00", standard_pattern));
            Assert.AreEqual(false, VerifyMatch("-1,21,22,100.00", standard_pattern));

            var india_pattern = "^\\s*[+-]?(\\d{1,3}((,\\d{2})*(,\\d{3})?)(\\.\\d+)?|\\d+(\\.\\d+)?)\\s*$";
            Assert.AreEqual(true, VerifyMatch("-1,21,22,100.00", india_pattern));
            Assert.AreEqual(true, VerifyMatch("-100.00", india_pattern));
            Assert.AreEqual(true, VerifyMatch("-1,100.00", india_pattern));
            Assert.AreEqual(false, VerifyMatch("-1231,100.00", india_pattern));

        }

        private bool VerifyMatch(string inputString, string pattern)
        {
            var regex = new Regex(pattern);
            Match match = regex.Match(inputString);
            return match.Success;
        }
    }
}
