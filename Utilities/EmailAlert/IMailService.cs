using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using PayMedia.Integration.FrameworkService.Interfaces.Common;

namespace Utilities.EmailAlert
{
    public interface IMailService
    {
        SmtpClient CreateSmtpClient(IMsgContext msgContext);
        Attachment AddAttachment(IMsgContext msgContext);
        MailMessage GenerateMailMessage(IMsgContext msgContext);
        void Send(IMsgContext msgContext);

    }
}
