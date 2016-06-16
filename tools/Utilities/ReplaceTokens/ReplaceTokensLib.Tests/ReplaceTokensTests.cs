using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace ReplaceTokensLib.Tests
{
    [TestFixture]
    public class ReplaceTokensTests
    {
        [Test]
        public void TestReplaceTokens()
        {
            List<string> missingTokens;
            TokenUtils.ReplaceTokens("TestFiles\\MACHINE_ANSWER_FILE_MI.xml",
                "TestFiles\\Integration.MassImport.exe.config.template",
                "TestFiles\\Integration.MassImport.exe.config", out missingTokens);
        }
    }
}

