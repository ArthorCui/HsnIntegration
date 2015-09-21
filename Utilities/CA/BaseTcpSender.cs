using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Timers;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Utilities.CA
{
    /// <summary>
    /// A basic Tcp sender implement a simple BaseSender by overriding Send method which send a string to target and get a string response.
    /// </summary>
    public class BaseTcpSender : BaseSender<string>
    {
        protected NetworkStream stream;
        protected TcpClient client;
        private TcpSenderParameter tcpSenderParameter;
        private readonly static object send_lock = new object();
        private readonly static object read_lock = new object();
        private bool iSendHoldCommand;

        protected bool IsConnected = false;

        protected byte[] RequestData;
        protected bool IsSendHoldCommand;
        protected System.Threading.Timer HoldtlTimer;
        protected long LastCommandSentTicks;
        protected int HoldtlCommandIntervalMs;

        /// <summary>
        /// Recconnected event triggered every time the connection is reconnected.
        /// 
        /// In this event, the TcpSenderEventArgs.TcpClient is connect, 
        /// but the TcpSenderEventArgs.Request and TcpSenderEventArgs.Response 
        /// has not been available yet.
        /// </summary>
        public event EventHandler<TcpSenderEventArgs> Reconnected;
        /// <summary>
        /// InitConnected event triggered by the first time when the client program calls Connect().
        /// 
        /// In this event, the TcpSenderEventArgs.TcpClient is connect, 
        /// but the TcpSenderEventArgs.Request and TcpSenderEventArgs.Response 
        /// has not been available yet.
        /// </summary>
        public event EventHandler<TcpSenderEventArgs> InitConnected;
        /// <summary>
        /// BeforeSending event triggered before the request has been sent to target.
        /// 
        /// In this event, the TcpSenderEventArgs.TcpClient is connect and TcpSenderEventArgs.Request is ready, 
        /// but TcpSenderEventArgs.Response has not been available yet.
        /// </summary>
        public event EventHandler<TcpSenderEventArgs> BeforeSending;
        /// <summary>
        /// AfterReceived event triggered after get response from target.
        /// 
        /// All parts in TcpSenderEventArgs are available in this event.
        /// </summary>
        public event EventHandler<TcpSenderEventArgs> AfterReceived;
        /// <summary>
        /// If the hold command is necessary to be sent on time interval
        /// It fires on every configured time interval
        /// </summary>
        public event EventHandler<TcpSenderEventArgs> OnSendHoldCommand;

        public event EventHandler UnConnect;
        /// <summary>
        /// Triggered when client response message timeout
        /// </summary>
        public WaitOrTimerCallback OnClientReceiveTimeOut;


        /// <summary>
        /// Constructor to build BasicTcpSender.
        /// 
        /// After contruction, the Tcpclient has not been connected to target yet.
        /// To establish the connection, call Connect() next. The Reconnected and InitConnected
        /// should be registered before Connect() if you want receive these notification, 
        /// otherwise they will not be triggered in frist time.
        /// </summary>
        /// <param name="tcpSenderParameter"></param>
        public BaseTcpSender(IComponentContext context, TcpSenderParameter tcpSenderParameter, bool isSendHoldCommand)
        {
            this.tcpSenderParameter = tcpSenderParameter;
            this.IsSendHoldCommand = isSendHoldCommand;
            this.HoldtlCommandIntervalMs = tcpSenderParameter.HoldCommandIntervalms;
            this.iSendHoldCommand = isSendHoldCommand;
            base.ComponentCtx = context;
        }

        #region HoldCommand
        private DateTime LastCommandSentTime
        {
            get { return new DateTime(Interlocked.Read(ref LastCommandSentTicks)); }
        }

        private void HoldtlTimer_TimeOn(object state)
        {
            DisableTimer();
            try
            {
                if (LastCommandSentTime.AddMilliseconds(HoldtlCommandIntervalMs) <= DateTime.Now && IsConnected)
                {
                    OnSendHoldCommand(this, new TcpSenderEventArgs { TcpClient = this.client });
                }
            }
            catch (ObjectDisposedException)
            {
                // there's a chance that the innerSender has disposed first but the timer just happens to trigger the callback
                // handle this rare case by catch ObjectDisposedException
            }
            catch (IOException)
            {
                // if the target closes the socket during HOLDTL command, just ignore this exception
            }
            finally
            {
                EnableTimer();
            }
        }
        private void DisableTimer()
        {
            try
            {
                HoldtlTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // in case the timer has been disposed
            }
        }

        private void EnableTimer()
        {
            try
            {
                HoldtlTimer.Change(HoldtlCommandIntervalMs, HoldtlCommandIntervalMs);
            }
            catch (ObjectDisposedException)
            {
                // in case the timer has been disposed
            }
        }
        #endregion

        protected virtual bool IsDataReadCompleted(byte[] bytes, ref byte[] totalBytes, int readCount, ref int totalCount)
        {
            return true;
        }

        /// <summary>
        /// Send the request to target through underlining NetworkStream.
        /// 
        /// This method is thread safe. This method uses blocking 
        /// network call to write request to the stream and read response from it.
        /// </summary>
        /// <param name="request">The content needs to be sent.</param>
        /// <returns>The decoded string from target's response.</returns>
        public override void Send(string request)
        {
            if (!this.IsConnected)
            {
                if (this.tcpSenderParameter.AutoReconnect)
                {
                    try
                    {
                        this.Reconnect();
                    }
                    catch (InvalidOperationException e)
                    {
                        //Reconnect failed
                    }
                }
                else
                {
                    throw new InvalidOperationException("TcpClient hasn't connected yet.");
                }
            }
            try
            {
                lock (this)
                {
                    var encoding = tcpSenderParameter.Encoding;
                    RequestData = encoding.GetBytes(request);
                    if (BeforeSending != null)
                    {
                        BeforeSending(this, new TcpSenderEventArgs { TcpClient = this.client, Request = request, RequestBytes = RequestData });
                    }

                    Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, RequestData, 0, RequestData.Length, null);
                }
            }
            catch (IOException ioe)
            {
                // the target closes the socket, we need set the flag to indicate reconnect next time
                IsConnected = false;
                throw ioe;
            }

        }


        protected void AsyncReceiveResponse()
        {
            try
            {
                if (stream == null || !stream.CanRead)
                {
                    return;
                }
                var encoding = tcpSenderParameter.Encoding;
                while (IsConnected && stream != null)
                {
                    if (stream.DataAvailable && client.Available > 0)
                    {
                        var totalBytes = new byte[0];
                        int totalCount = 0;
                        int readCount = 0;
                        var rdata = new byte[tcpSenderParameter.MaxReceiveBufferSize];
                        readCount = stream.Read(rdata, 0, rdata.Length);
                        try
                        {
                            lock (read_lock)
                            {
                                totalBytes = new byte[0];
                                totalCount = 0;
                                while (!IsDataReadCompleted(rdata, ref totalBytes, readCount, ref totalCount) && stream != null)
                                {
                                    rdata = new byte[tcpSenderParameter.MaxReceiveBufferSize];
                                    if (stream.DataAvailable && client.Available > 0)
                                    {
                                        readCount = stream.Read(rdata, 0, rdata.Length);
                                    }
                                    else
                                    {
                                        //Read data from stream error
                                        break;
                                    }
                                }
                                var response = encoding.GetString(totalBytes, 0, totalCount);
                                Interlocked.Exchange(ref LastCommandSentTicks, DateTime.Now.Ticks);
                                if (AfterReceived != null)
                                {
                                    AfterReceived(this,
                                        new TcpSenderEventArgs
                                        {
                                            TcpClient = this.client,
                                            Respose = response,
                                            ResponseBytes = totalBytes
                                        });
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //string.Format("Receive message error while :totalCount:{0},readCount:{1},error:{2},totalBytes:{3}", totalCount, readCount, err.Message, encoding.GetString(totalBytes, 0, totalCount)));
                        }
                    }
                }
            }
            catch (IOException)
            {
                this.IsConnected = false;
                if (UnConnect != null)
                    UnConnect(this, null);
                //error {0} in AsyncReceiveResponse", err.Message
            }
            catch (Exception)
            {
                //error {0} in AsyncReceiveResponse", err.Message
            }
        }

        public string Receive(string response)
        {
            return response;
        }

        public override void Dispose()
        {
            IsConnected = false;
            if (HoldtlTimer != null)
            {
                HoldtlTimer.Dispose();
                HoldtlTimer = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        private void Reconnect()
        {
            this.Dispose();
            this.Connect(false);
        }

        /// <summary>
        /// Connect the Tcpclient
        /// 
        /// This method can be called many times, but only the first one takes effect.
        /// </summary>
        public void Connect()
        {
            this.Connect(true);
        }

        private void Connect(bool firstTimeConnect)
        {
            if (!IsConnected)
            {
                IsConnected = true;
                try
                {
                    client = new TcpClient(tcpSenderParameter.Address, tcpSenderParameter.Port) { SendBufferSize = tcpSenderParameter.MaxReceiveBufferSize, SendTimeout = tcpSenderParameter.Timeout, ReceiveTimeout = tcpSenderParameter.Timeout };
                    stream = client.GetStream();
                    Task.Factory.StartNew(AsyncReceiveResponse);
                    if (iSendHoldCommand && HoldtlCommandIntervalMs > 0)
                    {
                        HoldtlTimer = new System.Threading.Timer(HoldtlTimer_TimeOn, null, HoldtlCommandIntervalMs, HoldtlCommandIntervalMs);
                    }
                }
                catch
                {
                    IsConnected = false;
                    throw;
                }
            }

            if (firstTimeConnect)
            {
                if (InitConnected != null)
                {
                    InitConnected(this, new TcpSenderEventArgs { TcpClient = this.client });
                }
            }

            if (Reconnected != null)
            {
                Reconnected(this, new TcpSenderEventArgs { TcpClient = this.client });
            }

        }
    }
}
