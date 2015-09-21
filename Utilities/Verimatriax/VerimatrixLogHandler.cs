using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayMedia.Integration.FrameworkService.Common;
using Utilities.CA;
using PayMedia.Integration.CommunicationLog.ServiceContracts;

namespace Utilities.Verimatriax
{
    public interface ISendLogHandler
    {
        Log GetSendLog(string request, string host, string commandname);
        Log GetConnectionLog(string request, string host, string commandname);
    }

    public interface IReceiveLogHandler
    {
        Log GetReceiveLog();
    }

    public class VerimatrixLogHandler : ISendLogHandler, IReceiveLogHandler
    {
        private VerimatrixResponse _response;
        private IComponentContext _componentContext;
        private VerimatrixLogEntity _entity;

        public VerimatrixLogHandler(IComponentContext context, VerimatrixResponse response, VerimatrixLogEntity entity)
        {
            this._response = response;
            this._componentContext = context;
            this._entity = entity;
        }

        public VerimatrixLogHandler()
        {

        }

        public Log GetConnectionLog(string request, string host, string commandname)
        {
            var msg = string.Empty;
            var category = string.Empty;
            var messageNumber = request.GetMessageNumber();

            var log = new Log()
            {
                MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Send,
                Host = host,
                MessageTrackingId = messageNumber + "." + commandname
            };

            switch (messageNumber)
            {
                case VerimatrixConstants.REGIST_MESSAGE_NUMBER:
                    msg = "Registration request sent: " + request;
                    category = VerimatrixConstants.REGIST_COMMAND_NAME;
                    break;
                case VerimatrixConstants.SHUTDOWN_MESSAGE_NUMBER:
                    msg = "Shutdown request sent: " + request;
                    category = VerimatrixConstants.SHUTDOWN_COMMAND_NAME;
                    break;
                case VerimatrixConstants.HOLDTL_MESSAGE_NUMBER:
                    msg = "Hold request sent: " + request;
                    category = VerimatrixConstants.HOLDTL_COMMAND_NAME;
                    break;
            }
            log.MessageText = msg;
            log.Category = category;
            return log;
        }

        public Log GetSendLog(string request, string host, string commandname)
        {
            var messageNumber = request.GetMessageNumber();
            var log = new Log()
            {
                MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Send,
                Host = host,
                MessageTrackingId = messageNumber + "." + commandname,
                MessageText = string.Format(
                    "Command {0} sent successfully.\r\n\r\n" +
                    "Message sent: {1}", commandname, request)
            };
            return log;
        }

        public Log GetReceiveLog()
        {
            var msg = string.Empty;
            var messageNumber = _response.Response.GetMessageNumber();

            if (_entity == null)
            {
                return GetTimeOutLog();
            }

            Log log = new Log()
            {
                Host = _entity.host,
                MessageTrackingId = messageNumber + "." + _entity.commandName
            };

            if (_response.Status == Enums.Status.Success)
            {
                msg = string.Format(
                    "Response to command {0} received successfully.\r\n\r\n" +
                    "Message received: {4},{2},{3}\r\n\r\n" +
                    "Message sent: {1}",
                    _entity.commandName, _entity.request, _response.ReturnCode, GetSuccessMessage(_response.ReturnCode), messageNumber);

                log.MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Receive;
                log.MessageText = msg;
            }
            else
            {
                msg = string.Format(
                    "Response to command {0} received with error.\r\n\r\n" +
                    "Error: {4},{2},{3}\r\n\r\n" +
                    "Message sent: {1}",
                    _entity.commandName, _entity.request, _response.ReturnCode, GetErrorMessage(_response.ReturnCode), messageNumber);

                log.MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Error;
                log.MessageText = msg;
            }

            return log;
        }

        private Log GetTimeOutLog()
        {
            var log = new Log()
            {
                MessageText = string.Format(
                    "Message received with timeout.\r\n\r\n" +
                    "Message received: {0}", _response.Response),
                Host = string.Empty,
                MessageQualifier = (int)CommunicationLogEntryMessageQualifier.Error,
                MessageTrackingId = _response.Response.GetMessageNumber() + ".",
            };
            return log;
        }

        private string GetSuccessMessage(string successCode)
        {
            string successMessage = string.Empty;

            var successMapping = ToCodeMappingDictionary("VERIMATRIX_CONFIG_SUCCESS_CODE_MAPPING");
            if (successMapping.ContainsKey(successCode))
                successMessage = successMapping[successCode];

            return successMessage;
        }

        private string GetErrorMessage(string errorCode)
        {
            string errorMessage = "Unknown error.";

            var errorMapping = ToCodeMappingDictionary("VERIMATRIX_CONFIG_ERROR_CODE_MAPPING");
            if (errorMapping.ContainsKey(errorCode))
                errorMessage = errorMapping[errorCode];

            return errorMessage;
        }

        private IDictionary<string, string> ToCodeMappingDictionary(string configKey)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            var valueString = this._componentContext.MessageContext.Config[configKey];
            if (!string.IsNullOrEmpty(valueString))
            {
                var lines = valueString.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    int indexOfComma = line.IndexOf(",");
                    if (indexOfComma < 0)
                    {
                        throw new ArgumentException(
                            string.Format("Code mapping config error: Expecting a value that can be split by comma \"{0}\"", valueString));
                    }

                    string key = line.Substring(0, indexOfComma);
                    string value = line.Substring(indexOfComma + 1);
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
    }
}
