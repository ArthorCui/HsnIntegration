using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class TryCatchTester
    {
        [Test]
        public void test_process_condition()
        {
            Assert.AreEqual(2, GetValue(1));
            Assert.AreEqual(1, GetValue(-1));
        }

        private int GetValue(int number)
        {
            var result = 1;

            try
            {
                if (number > 0)
                {
                    result = number;
                }
                else
                {
                    result = number/0;
                }
            }
            catch (Exception ex)
            {
                result = 0;
            }
            finally
            {
                result = result + 1;
                Console.WriteLine("Input number is {0}, the result number is {1}", number, result);
            }
            return result;
        }

    }
}
