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
                "v3basic***JSxt9Tc5sKJbOizmyprgPBEf461m1bh9/P4lB6FtGeqEfWCtpfdDrQCPtm254YjN4CPexGa9BjW1ZlOhU18yy7WBIpeN84kSLLj1Pi52GIQAkKGA3yG8bB75chpU4q0BbayOwKYzT3M3Kez+uyCA+mzBA5BO2O0iyXdmaVFVgKI=";

            //Console.WriteLine(instance.Decrypt(encrypt_string) + "\r\n" + instance.Result);
            Console.WriteLine(instance.Decrypt(encrypt_str2) + "\r\n" + instance.Result);
        }
    }
}
