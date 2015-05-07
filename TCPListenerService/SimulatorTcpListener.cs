using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integration.Main.Listeners.Tcp;
using System.Net.Sockets;
using Integration.Main.Common;

namespace TCPListenerService
{
    public class SimulatorTcpListener : TcpBaseService
    {
        private Listener listener = null;
        private Encoding encoding = new UTF8Encoding(false);

        public SimulatorTcpListener(Listener listener, ListenerConfiguration configuration)
        {
            this.listener = listener;
            base.Initialize(configuration);
        }

        public override TcpBaseServiceConnection CreateClientConnectionHandler(TcpClient tcpClient, TcpEndpoint tcpEndPoint, string hostListenerName, bool closeAfterSend)
        {
            return new TcpServiceConnection(tcpClient, tcpEndPoint, hostListenerName, closeAfterSend, listener, encoding);
        }

        private class TcpServiceConnection : TcpBaseServiceConnection
        {
            private readonly Listener listener;
            private readonly Encoding encoding;

            public TcpServiceConnection(TcpClient tcpClient, TcpEndpoint tcpEndPoint, string hostListenerName, bool closeAfterSend, Listener listener, Encoding encoding)
                : base(tcpClient, tcpEndPoint, hostListenerName, closeAfterSend)
            {
                this.listener = listener;
                this.encoding = encoding;
            }
            public override string ReceiveTcpMessage(string message, int port, string address)
            {
                return message;
            }
        }
    }
}
