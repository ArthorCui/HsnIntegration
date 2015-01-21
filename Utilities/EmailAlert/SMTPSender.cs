using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayMedia.Integration.FrameworkService.Interfaces;
using PayMedia.Integration.FrameworkService.Interfaces.Common;
using PayMedia.Integration.FrameworkService.Common;

namespace Utilities.EmailAlert
{
    public class SMTPSender : IFComponent
    {
        private IMailService _mailService;
        public IMailService MailService
        {
            get
            {
                if (_mailService == null)
                {
                    _mailService = new MailService();
                }
                return _mailService;
            }
            set
            {
                value = this._mailService;
            }
        }

        public IMessageAction Process(IMsgContext msgContext)
        {
            MailService.Send(msgContext);
            return MessageAction.ContinueProcessing;
        }
    }
}
