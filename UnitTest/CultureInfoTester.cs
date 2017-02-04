using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    }
}
