using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TcpServer
{
    public class Connection
    {
        public void Create()
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6666);

            //Binding client
            TcpClient tcp_Client = new TcpClient(localEP);

            //Listener
            Listener.Start(10);


            byte[] SendBuf = Encoding.UTF8.GetBytes("Hello,Client!");
            IPEndPointlocalEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6666);
            TcpListenerListener = new TcpListener(localEP);
            Listener.Start(10);
            Console.WriteLine("Server is listening...");
            TcpClientremoteClient = Listener.AcceptTcpClient();
            Console.WriteLine("Client:{0} connected!", remoteClient.Client.RemoteEndPoint.ToString());
            remoteClient.Client.Send(SendBuf);
            remoteClient.Close();
        }
    }
}
