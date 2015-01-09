using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Utilities.EmailAlert
{
    public class MailService : IMailService
    {
        public SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }

        public Attachment AddAttachment(string filePath)
        {
            try
            {
                return null;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public MailMessage GenerateMailMessage(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public void Send()
        {
            throw new NotImplementedException();
        }
    }
}
