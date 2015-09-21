using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Verimatriax
{
    public class VerimatrixLogEntity
    {
        public string request { get; set; }

        public string messageNumber { get; set; }

        public DateTime sendTime { get; set; }

        public string host { get; set; }

        public string commandName { get; set; }

        public string serialNumber { get; set; }

        public string customerID { get; set; }

        public string eventNumber { get; set; }

        public string eventTimeStamp { get; set; }

        public string histroyID { get; set; }

        public string externalCommandName { get; set; }

        public string userName { get; set; }
    }
}
