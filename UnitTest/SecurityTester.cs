using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Utilities.Entriq;

namespace UnitTest
{
    public class SecurityTester
    {
        internal Encryption instance = new Encryption();

        [Test]
        public void encrypt()
        {
            var connection_string = "user id=dr_roger;password=dr_roger;data source=ICDB";

            Console.WriteLine(instance.Encrypt(connection_string) + "\r\n" + instance.Result);
            var encrypt_string = "v3basic***neGZWCLd9+p34cvZcSJUJQVIYqflFa03vSv98yoekzsw04AhgqQ3Nywc+9NOO/0Uz0C7vMd6t9/f4P+uoIj1blOiWZ6WOwG686gM/F15t+0Ex1gQu0fB4ntdtuAntlrasR081DBdxnryhFBvSXUGQg==";

            var encrypt_str2 =
                "v3basic***yEFAgCx3FaGZ9qQHu8LXYvpZXMmGSPF8PTzX00vkKALJqv8D5CGibd+25Ag727sSihNNKirdST2CfAZrizHcUDEQotvNDRDUzjbOAsxjB7z84O7dPPulrZGCW/2M86Un7zz8AL07mLfenNX8nDG2hkJrNuvBS2VHRev2P1bVdQ8=";

            //Console.WriteLine(instance.Decrypt(encrypt_string) + "\r\n" + instance.Result);
            Console.WriteLine(instance.Decrypt(encrypt_str2) + "\r\n" + instance.Result);
        }
    }
}
