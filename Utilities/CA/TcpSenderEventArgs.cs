using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Utilities.CA
{
    /// <summary>
    /// TcpSenderEventArgs is an event arguments for Tcp sender events
    /// </summary>
    public class TcpSenderEventArgs : EventArgs
    {
        /// <summary>
        /// The internal Tcpclient which the TcpSender using
        /// </summary>
        public TcpClient TcpClient { get; set; }
        /// <summary>
        /// The request content of the Send() method, available only in some events
        /// </summary>
        public string Request { get; set; }
        /// <summary>
        /// The response content of the Send() method, available only in some events
        /// </summary>
        public string Respose { get; set; }
        /// <summary>
        /// The request content in bytes format
        /// </summary>
        public byte[] RequestBytes { get; set; }
        /// <summary>
        /// The response content in byte format
        /// </summary>
        public byte[] ResponseBytes { get; set; }
    }
}
