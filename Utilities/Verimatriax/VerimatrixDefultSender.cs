using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.CA;
using System.Threading;
using PayMedia.Integration.FrameworkService.Common;
using PayMedia.Integration.CommunicationLog.ServiceContracts;
using PayMedia.Integration.FrameworkService.Interfaces.Common;

namespace Utilities.Verimatriax
{
    public class VerimatrixDefultSender : BaseTcpSender
    {
        private VerimatrixSenderParameter senderParameter;
        private TcpSenderParameter tcpSenderParameter;
        private bool IsRegistered;
        private AutoResetEvent registerWaitHandler;
        private bool IsShutdown;

        public VerimatrixDefultSender(IComponentContext context, TcpSenderParameter tcpSenderParameter, VerimatrixSenderParameter senderParameter)
            : base(context, tcpSenderParameter, true)
        {
            this.senderParameter = senderParameter;
            this.tcpSenderParameter = tcpSenderParameter;

            this.Reconnected += VerimatrixTcpSender_Reconnected;
            this.AfterReceived += VerimatrixTcpSender_AfterReceived;
            this.OnSendHoldCommand += VerimatrixTcpSender_SendHoldCommand;
            this.BeforeSending += VerimatrixTcpSender_BeforeSending;

            registerWaitHandler = new AutoResetEvent(false);

            this.HostName = GetHostName();
            this.Connect();
        }

        private void VerimatrixTcpSender_BeforeSending(object sender, TcpSenderEventArgs e)
        {
            ISendLogHandler sendLog = new VerimatrixLogHandler();

            if (IsRegistered && IsConnected)
            {
                //con current dic check expire
                VerimatrixMessageDataAccess.Inspection(tcpSenderParameter.Timeout);

                if (!this.IsSendingHoldShutdownCommand(e.Request))
                {
                    VerimatrixMessageDataAccess.LogRequestMessage(e.Request, this.GetLogEntity(e.Request));

                    this.LogSendMessage(sendLog.GetSendLog(e.Request, this.GetHostName(), this.CommandName));

                    //Logger.Info(string.Format("Message sent: {0}", e.Request));
                }
                else
                {
                    //hold & shutdown
                    this.WriteLog(sendLog.GetConnectionLog(e.Request, this.GetHostName(), this.CommandName));
                }
            }
            else
            {
                //register
                this.WriteLog(sendLog.GetConnectionLog(e.Request, this.GetHostName(), this.CommandName));
            }
        }

        protected override bool IsDataReadCompleted(byte[] bytes, ref byte[] totalBytes, int readCount, ref int totalCount)
        {
            Array.Resize(ref totalBytes, totalCount + readCount);
            Array.Copy(bytes, 0, totalBytes, totalCount, readCount);

            totalCount += readCount;

            string response = ASCIIEncoding.ASCII.GetString(totalBytes);
            if (response[response.Length - 1] == VerimatrixConstants.RESPONSEEND) // '\n' 
                return true;

            return false;
        }

        private void VerimatrixTcpSender_AfterReceived(object sender, TcpSenderEventArgs e)
        {
            Func<string, List<string>> SplitReceivedMessage = delegate(string tMessage)
            {
                return tMessage.Split(new char[] { VerimatrixConstants.RESPONSEEND }, StringSplitOptions.RemoveEmptyEntries).ToList();
            };

            var rMessages = SplitReceivedMessage(e.Respose);

            rMessages.ForEach((Response) =>
            {
                ProcessMessage(Response);
            });
        }

        private void ProcessMessage(string messageResponse)
        {
            var response = new VerimatrixResponse(messageResponse);

            try
            {
                if (!IsRegistered)
                {
                    if (response.Status == Enums.Status.Register)
                    {
                        this.IsRegistered = true;
                        this.registerWaitHandler.Set();
                    }
                    // registation failed
                    else
                    {
                        this.registerWaitHandler.Set();
                    }
                }
                else
                {
                    //Logger.Info(string.Format("Message received: {0}", messageResponse));

                    var logEntity = VerimatrixMessageDataAccess.GetRequestData(messageResponse);
                    IReceiveLogHandler logHandler = new VerimatrixLogHandler(this.ComponentCtx, response, logEntity);

                    // hold command doesn't has response
                    if (HoldtlTimer != null)
                    {
                        HoldtlTimer.Change(this.tcpSenderParameter.HoldCommandIntervalms,
                            this.tcpSenderParameter.HoldCommandIntervalms);
                    }

                    if (response.Status == Enums.Status.Success || response.Status == Enums.Status.Error)
                    {
                        this.LogReceiveMessage(logHandler.GetReceiveLog(), this.GetAsyncLogDictionary(logEntity));
                    }

                    if (response.Status == Enums.Status.Shutdown)
                    {
                        this.IsShutdown = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log()
                {
                    MessageText = ex.Message,
                    MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Error,
                    Host = this.HostName,
                    MessageTrackingId = this.CommandName
                };
                ComponentCtx.WriteInfo(log);
            }
        }

        private void VerimatrixTcpSender_Reconnected(object sender, TcpSenderEventArgs e)
        {
            SendRegisterCommand();
            if (IsRegistered | registerWaitHandler.WaitOne(tcpSenderParameter.Timeout))
            {
                if (!IsRegistered)
                {
                    base.Dispose();
                    throw new Exception("Registration message returns error");
                }
            }
            else
            {
                base.Dispose();
                throw new IntegrationException(string.Format("Registration failed with timeout ({0} ms)", tcpSenderParameter.Timeout));
            }
        }

        private void VerimatrixTcpSender_SendHoldCommand(object sender, TcpSenderEventArgs e)
        {
            if (IsRegistered)
            {
                var holdCommand = string.Format("{0},{1}\n", this.senderParameter.HoldCommandMsgNumber, this.senderParameter.HoldCommandName);

                base.Send(holdCommand);
            }
        }

        private void SendRegisterCommand()
        {
            var registCommand = string.Format("{0},{1},{2}\n", this.senderParameter.RegistCommandMsgNumber, this.senderParameter.RegistCommandName, this.senderParameter.RegistOperator);

            base.Send(registCommand);
        }

        private void SendShtdwnCommand()
        {
            var shtdwnCommand = string.Format("{0},{1}\n", this.senderParameter.ShutdownMsgNumber, this.senderParameter.ShutdownCommandName);

            base.Send(shtdwnCommand);
        }

        private void WriteLog(Log log)
        {
            if (ComponentCtx != null) // in some cases, the DataContext may have not been set yet
            {
                ComponentCtx.MessageContext.Services.Logger.Info(log);
            }
            //Logger.Info(log.MessageText);
        }

        private bool IsSendingHoldShutdownCommand(string request)
        {
            return request.Contains(this.senderParameter.HoldCommandName) || request.Contains(this.senderParameter.ShutdownCommandName);
        }

        private string GetHostName()
        {
            return string.Format("{0}:{1}", this.tcpSenderParameter.Address, this.tcpSenderParameter.Port);
        }

        private VerimatrixLogEntity GetLogEntity(string request)
        {
            return new VerimatrixLogEntity()
            {
                request = request,
                messageNumber = request.GetMessageNumber(),
                commandName = this.CommandName,
                host = this.HostName,
                serialNumber = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.DEVICE_SERIAL_NUMBER],
                customerID = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.CUSTOMER_ID],
                eventNumber = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.EVENT_NUMBER],
                histroyID = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.HISTORY_ID],
                externalCommandName = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.PROV_EXT_CMD_ID],
                userName = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.USER_NAME],
                sendTime = DateTime.Now,
                eventTimeStamp = this.ComponentCtx.MessageContext.Event[IFEventPropertyNames.MSG_UTC]
            };
        }

        private Dictionary<string, string> GetAsyncLogDictionary(VerimatrixLogEntity logEntity)
        {
            if (logEntity != null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add(IFEventPropertyNames.DEVICE_SERIAL_NUMBER, logEntity.serialNumber);
                dict.Add(IFEventPropertyNames.CUSTOMER_ID, logEntity.customerID);
                dict.Add(IFEventPropertyNames.EVENT_NUMBER, logEntity.eventNumber);
                dict.Add(IFEventPropertyNames.MSG_UTC, logEntity.eventTimeStamp);
                dict.Add(IFEventPropertyNames.HISTORY_ID, logEntity.histroyID);
                dict.Add(IFEventPropertyNames.USER_NAME, logEntity.userName);
                dict.Add(IFEventPropertyNames.PROV_EXT_CMD_ID, logEntity.externalCommandName);
                return dict;
            }
            return null;
        }

        public override void Dispose()
        {
            SendShtdwnCommand();

            this.IsRegistered = false;
            VerimatrixMessageDataAccess.Reset();
            if (IsShutdown)
            {
                this.IsShutdown = false;
                base.Dispose();
            }
        }
    }
}
