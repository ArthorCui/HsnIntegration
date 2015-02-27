using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class OperatorTest
    {
        [Test]
        public void GreaterAndLessOperatorTest()
        {
            var s1 = 0.33;
            var s2 = 2.3;
            var s3 = 3;
            var s4 = 0.26;
            var ret1 = s1 > s4 ? true : false;
            var ret2 = s2 > s3 ? true : false;
            Assert.AreEqual(true, ret1);
            Assert.AreEqual(false, ret2);
        }
    }
}
