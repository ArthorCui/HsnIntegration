using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Verimatriax
{
    public struct VerimatrixSenderParameter
    {
        public VerimatrixSenderParameter(string registCmdName, string registCmdMsgNumber, string registOperator, string holdCmdName, string holdCmdMsgNumber, string shutdownCmdName, string shutdownMsgNumber)
            : this()
        {
            RegistCommandName = registCmdName;
            RegistCommandMsgNumber = registCmdMsgNumber;
            RegistOperator = registOperator;
            HoldCommandName = holdCmdName;
            HoldCommandMsgNumber = holdCmdMsgNumber;
            ShutdownCommandName = shutdownCmdName;
            ShutdownMsgNumber = shutdownMsgNumber;
        }

        public string RegistCommandName { get; private set; }
        public string RegistCommandMsgNumber { get; private set; }
        public string RegistOperator { get; private set; }
        public string HoldCommandName { get; private set; }
        public string HoldCommandMsgNumber { get; private set; }
        public string ShutdownCommandName { get; private set; }
        public string ShutdownMsgNumber { get; private set; }
    }
}
