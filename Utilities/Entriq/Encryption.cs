using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entriq.Security.Encryption;

namespace Utilities.Entriq
{
    public class Encryption
    {
        public string Result { get { return output; } }

        private const string KEY = "secret";

        private string output;

        public bool Encrypt(string input, string username = KEY)
        {
            return MANEncryptor.Encrypt(input, username, EncryptMode.V3basic, out output);
        }

        public bool Decrypt(string input, string username = KEY)
        {
            return MANEncryptor.Decrypt(input, username, out output);
        }
    }
}
