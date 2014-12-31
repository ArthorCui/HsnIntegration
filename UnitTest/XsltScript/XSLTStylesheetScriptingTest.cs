using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Utilities.XlstScript;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class XSLTStylesheetScriptingTest
    {
        public XSLTStylesheetScripting instance { get; set; }

        public XSLTStylesheetScriptingTest()
        {
            instance = new XSLTStylesheetScripting();
        }

        [Test]
        public void SampleTest()
        {
            var prefixFilePath = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = Path.Combine(prefixFilePath, "XsltScript\\Sample\\number.xml");
            var stylesheet = Path.Combine(prefixFilePath, "XsltScript\\Sample\\calc.xsl");

            instance.Transform(fileName, stylesheet);
        }
    }
}
