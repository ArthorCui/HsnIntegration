using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using PayMedia.Integration.FrameworkService.Common;

namespace Utilities.CA
{
    [DataContract]
    [Serializable]
    public class CACommand
    {
        [DataMember]
        public string CommandName { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }

        public virtual List<Type> KnownTypes
        {
            get { return new List<Type>(); }
        }

        public CACommand()
        {
            Parameters = new Dictionary<string, object>();

        }

        public CACommand(string commandName)
            : this()
        {
            this.CommandName = commandName;
        }

        public object Execute()
        {
            LogSendCommLogEntry(GenerateMessage());
            var response = ExecuteCore();
            LogRecieveCommLogEntry(response);
            return response;
        }

        protected virtual void LogRecieveCommLogEntry(object response)
        {
            WriteReceiveLog(response, this.CommandName, this.GetHostName());
        }

        protected virtual void LogSendCommLogEntry(string generateMessage)
        {
            WriteSendLog(generateMessage, this.CommandName, this.GetHostName());
        }

        public virtual object ExecuteCore()
        {
            return null;
        }

        public virtual string GenerateMessage()
        {
            return string.Empty;
        }

        public virtual string GetHostName()
        {
            return string.Empty;
        }

        public void AddParameter(string key, object value)
        {
            lock (Parameters)
            {
                if (!Parameters.ContainsKey(key))
                {
                    Parameters.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Handler for formatting the response, used for log only
        /// </summary>
        public Func<object, object> FormatResponse;

        /// <summary>
        /// Handler for formatting the send request, used for log only
        /// </summary>
        public Func<string, string> FormatSendRequest;

        protected void WriteReceiveLog(object response, string commandName, string hostName)
        {
            var responseText = FormatResponse != null ? FormatResponse(response).ToString() : (response == null ? string.Empty : response.ToString());
            if (!string.IsNullOrEmpty(responseText))
            {
                var responseAdditionalInfo = "Message Received(Raw):\n" + responseText;
                var log = new Log
                {
                    MessageText = responseText,
                    MessageQualifier = 2,
                    Host = hostName,
                    AdditionalInformation = responseAdditionalInfo,
                    MessageTrackingId = commandName
                };
                //DataContext.Current.Cache[Constants.COMMON_LOG_ENTRY] = log;
                DataContext.Current.WriteInfo(log);
            }
        }


        protected void WriteSendLog(string request, string commandName, string hostName)
        {
            var requestText = FormatSendRequest != null ? FormatSendRequest(request) : request;
            if (!string.IsNullOrEmpty(requestText))
            {
                var requestAdditionalInfo = "Message Sent(Raw):\n" + request;

                var log = new Log
                {
                    MessageText = requestText,
                    Host = hostName,
                    MessageQualifier = 1,
                    AdditionalInformation = requestAdditionalInfo,
                    MessageTrackingId = commandName
                };

                //DataContext.Current.Cache[Constants.COMMON_LOG_ENTRY] = log;
                DataContext.Current.WriteInfo(log);
            }
        }
    }
}
