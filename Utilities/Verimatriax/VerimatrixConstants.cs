using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Verimatriax
{
    public class VerimatrixConstants
    {
        public const string REGIST_COMMAND_NAME = "REGIST";
        public const string REGIST_MESSAGE_NUMBER = "209999999";
        public const string HOLDTL_COMMAND_NAME = "HOLDTL";
        public const string HOLDTL_MESSAGE_NUMBER = "209999998";
        public const string SHUTDOWN_COMMAND_NAME = "SHTDWN";
        public const string SHUTDOWN_MESSAGE_NUMBER = "209999997";

        public const string REGISTRATION_OK = "registration_OK";
        public const string SHUTDOWN = "shutdown";
        public const string HOLDTL = "holdtl";

        public const char COMMA = ',';
        public const int MESSAGE_NUMBER_INDEX = 0;
        public const int MESSAGE_COMMAND_NAME_INDEX = 1;
        public const int MESSAGE_RETURN_CODE = 1;

        public const char RESPONSEEND = '\n';
    }
}
