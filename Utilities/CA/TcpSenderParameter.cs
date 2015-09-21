using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    /// <summary>
    /// TcpSenderParameter is a parameter object for creating TCP connections
    /// </summary>
    public struct TcpSenderParameter
    {
        public TcpSenderParameter(string address, int port, Encoding encoding = null, int timeout = 0, int maxReceiveBufferSize = 8192, bool autoReconnect = true, int holdCommandInterval = 0)
            : this()
        {
            this.Address = address;
            this.Port = port;
            this.Encoding = encoding ?? Encoding.ASCII;
            this.Timeout = timeout;
            this.MaxReceiveBufferSize = maxReceiveBufferSize;
            this.AutoReconnect = autoReconnect;
            this.HoldCommandIntervalms = holdCommandInterval;

        }

        /// <summary>
        /// Target host name or IP address
        /// </summary>
        public string Address { get; private set; }
        /// <summary>
        /// TCP port
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Time out millisecond for both NetworkStream's read and write
        /// </summary>
        public int Timeout { get; private set; }
        /// <summary>
        /// Limitation of the lenth read from the Stream
        /// </summary>
        public int MaxReceiveBufferSize { get; private set; }
        /// <summary>
        /// Encoding used in the communication
        /// </summary>
        public Encoding Encoding { get; private set; }
        /// <summary>
        /// Whether reconnect if the connection lose during communication
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// Interval for sending hold command,set to 0 to disable it
        /// </summary>
        public int HoldCommandIntervalms { get; private set; }
    }
}
