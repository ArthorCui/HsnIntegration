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
        public IMessageAction Process(IMsgContext msgContext)
        {
            this.SendMail(msgContext);
            return MessageAction.ContinueProcessing;
        }

        public void SendMail(IMsgContext msgContext)
        {

        }
    }
}
