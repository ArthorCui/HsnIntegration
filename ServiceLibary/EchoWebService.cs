using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Xml;
using System.IO;
using System.Web;

namespace ServiceLibary
{
    [WebService(Namespace = "localhost", Name = "ServiceLibary")]
    public class EchoWebService : WebService
    {
        [WebMethod(Description = "Echo Soap Request")]
        public XmlDocument EchoSoapRequest(int input)
        {
            var xmlSoapRequest = new XmlDocument();

            using (Stream receiveStream = HttpContext.Current.Request.InputStream)
            {
                receiveStream.Position = 0;
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    xmlSoapRequest.Load(readStream);
                }
            }
            return xmlSoapRequest;
        }
    }
}
