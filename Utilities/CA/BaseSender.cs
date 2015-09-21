using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayMedia.Integration.FrameworkService.Common;
using PayMedia.Integration.CommunicationLog.ServiceContracts;

namespace Utilities.CA
{
    public abstract class BaseSender<TRequest> : ISender<TRequest>
    {
        public string CommandName;
        public IComponentContext ComponentCtx;
        protected string HostName;

        /// <summary>
        /// Log send message 
        /// </summary>
        /// <param name="text">Request send message</param>
        /// <param name="uniqueId">unique id for mapping response</param>
        protected virtual void LogSendMessage(string text, string uniqueId)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var log = new Log
                {
                    MessageText = text,
                    MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Send,
                    Host = this.HostName,
                    ExternalReference = uniqueId,
                    Category = this.CommandName,
                    MessageTrackingId = uniqueId + "." + this.CommandName
                };
                ComponentCtx.WriteInfo(log);
            }
        }
        protected virtual void LogReceiveMessage(string text, string uniqueId = "", int messageQualifier = (int)CommunicationLogEntryMessageQualifier.Receive)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var log = new Log
                {
                    MessageText = text,
                    MessageQualifier = messageQualifier,
                    Host = this.HostName,
                    MessageTrackingId = this.CommandName,
                    ExternalReference = uniqueId
                };
                ComponentCtx.WriteInfo(log);
            }
        }

        protected virtual void LogSendMessage(Log log)
        {
            ComponentCtx.WriteInfo(log);
        }

        protected virtual void LogReceiveMessage(Log log, Dictionary<string, string> dict)
        {
            ComponentCtx.WriteAsyncInfo(log, dict);
        }

        public virtual void Send(TRequest request)
        {

        }

        public virtual void Dispose()
        {
        }
    }
}
