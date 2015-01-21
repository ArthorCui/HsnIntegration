using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using PayMedia.Integration.FrameworkService.Interfaces.Common;
using PayMedia.Integration.FrameworkService.Common;
using System.Net;

namespace Utilities.EmailAlert
{
    public class MailService : IMailService
    {
        public SmtpClient CreateSmtpClient(IMsgContext msgContext)
        {
            var smtpClient = new SmtpClient();
            smtpClient.Host = msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_HOST);
            smtpClient.Port = Convert.ToInt32(msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_PORT));
            smtpClient.EnableSsl = msgContext.GetConfigValue(IFConfigPropertyNames.SMTP_ENABLESSL, false);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_FROM), msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_PASSWORD));

            return smtpClient;
        }

        public Attachment AddAttachment(IMsgContext msgContext)
        {
            return new Attachment(msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.FILE_PATH), MediaTypeNames.Application.Octet);
        }

        public MailMessage GenerateMailMessage(IMsgContext msgContext)
        {
            var messageFromAddress = msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_FROM);
            var messageToAddresses = msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_TO);
            var messageFrom = new MailAddress(messageFromAddress);
            var messageTo = new MailAddress(messageToAddresses);
            var message = new MailMessage(messageFrom, messageTo);
            message.CC.Add(messageFrom);
            message.Subject = msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_SUBJECT);
            message.Body = msgContext.GetMsgOrConfigValue(IFConfigPropertyNames.SMTP_BODY);
            message.Attachments.Add(AddAttachment(msgContext));

            return message;
        }

        public void Send(IMsgContext msgContext)
        {
            try
            {
                var smtpClient = CreateSmtpClient(msgContext);
                var mailMessage = GenerateMailMessage(msgContext);
                smtpClient.Send(mailMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
