using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Utilities.EmailAlert
{
    public interface IMailService
    {
        SmtpClient CreateSmtpClient();
        Attachment AddAttachment(string filePath);
        MailMessage GenerateMailMessage(Attachment attachment);

        void Send();

    }
}
