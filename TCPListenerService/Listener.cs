using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integration.Library.Common;

namespace TCPListenerService
{
    public class Listener
    {
        public string LogFolder { get; set; }
        public TcpEndpoint TcpEndPoint { get; set; }
    }
}
