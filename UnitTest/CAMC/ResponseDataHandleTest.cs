using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Integration.NDSMessage;
using Integration.Library.NDS;
using PayMedia.Framework.Integration.Contracts;
using Integration.Library.Framework;
using Integration.Library.Common;
using System.Xml;
using NDSMessage.ReponseAction;
using NDSMessage.SubscriberMessage;

namespace UnitTest.CAMC
{
    [TestFixture]
    public class ResponseDataHandleTest
    {
        public const string RESPONSE_DATA = "0002M003A0100060006201502210545195A09OES058744341E17210796";

        public UnitTestCommand Command { get; set; }
        public ResponseDataHandleTest()
        {
            this.Command = new UnitTestCommand(new IntegrationMailMessage());
        }

        [Test]
        public void CAMC_response_data_ctor_test()
        {
            var camc_message = new CamcResponseMessage(RESPONSE_DATA);

            Assert.AreEqual(256, camc_message.Envelope.FromId);
            Assert.AreEqual(6, camc_message.Envelope.ToId);
            Assert.AreEqual(23049, camc_message.Envelope.SequenceId);
            Assert.AreEqual("8744341E17210796", camc_message.Signature);
            Assert.AreEqual("OES05", camc_message.Body.Action);
        }

        [Test]
        public void CAMC_response_get_error_code_test()
        {
            //Command.Execute();
            WriteCommonLogEntryForReceive("0002M003A0100060006201502210545195A09OES058744341E17210796", "A05FE00000000A00A091300000000A00A051E00000000A00A04CD00000000A00A04CC00000000A00A045E00000000A00YA6B");
        }

        [Test]
        public void CommonLogEntry_serialize_test()
        {
            var entry = new NDSCommLogEntry();
            entry.ErrorCode = "ES05";
            entry.ErrorDescription = "Response is invalid. Error Code: ES05";
            entry.OriginalMessageAction = "A05FE00000000A00A091300000000A00A051E00000000A00A04CD00000000A00A04CC00000000A00A045E00000000A00YA6B";
            entry.UnformattedMessage = "0002M003A0100060006201502210545195A09OES058744341E17210796";

            SubscriberMessageBase request = new SubscriberMessageBase();
            var message = request.Deserialize(entry.OriginalMessageAction, "", false);

            entry.Message = (CamcMessageBase)message;

            List<Type> knownTypes = new List<Type>();
            knownTypes.AddRange(SubscriberActionFactory.KnownActionTypes);
            knownTypes.AddRange(SubscriberResponseActionFactory.KnownActionTypes);
            knownTypes.AddRange(AddressingResponseActionFactory.KnownActionTypes);

            var output = SerializationUtilities<NDSCommLogEntry>.DataContract.Serialize(entry, knownTypes);
        }

        private string GetErrorCode(string actions)
        {
            if (actions == "") return "";
            if (actions[0] == 'E')  //Error Found
            {
                if (actions.Length < 4) return "";
                return actions.Substring(0, 4);
            }
            else if (actions[0] == 'O')  //Okay, keep looking
            {
                actions = actions.Substring(1);
                return GetErrorCode(actions);
            }
            return "";
        }

        public void WriteCommonLogEntryForReceive(string logText, string messageAction)
        {
            var commLogEntry = new NDSCommLogEntry();
            commLogEntry.OriginalMessageAction = messageAction;
            commLogEntry.UnformattedMessage = logText;

            const int startPosition = 37;
            int endPosition = logText.Length - 37 - 16;
            var actions = logText.Substring(startPosition, endPosition);
            var errorCodeTemp = GetErrorCode(actions);
            if (errorCodeTemp != "") //error found
            {
                var errorCode = errorCodeTemp;

                XmlDocument xErrorsDoc = new XmlDocument();
                xErrorsDoc.Load(FileUtilities.GetFullPath(@"D:\\Temp\\camc_es05.xml"));

                var errorDescription = XmlUtilities.SafeSelectText(xErrorsDoc, "Errors/Error[Code = '" + errorCode + "']/Message");
                commLogEntry.ErrorCode = errorCode;
                commLogEntry.ErrorDescription = errorDescription;

                var errorMessageNode = xErrorsDoc.CreateNode(XmlNodeType.Element, "ErrorMessage", string.Empty);
                xErrorsDoc.FirstChild.AppendChild(errorMessageNode);

                var errorMessageErrorCodeNode = xErrorsDoc.CreateNode(XmlNodeType.Element, "ErrorCode", string.Empty);
                errorMessageErrorCodeNode.InnerText = errorCode;
                errorMessageNode.AppendChild(errorMessageErrorCodeNode);

                var errorMessageErrorDescriptionNode = xErrorsDoc.CreateNode(XmlNodeType.Element, "ErrorDescription", string.Empty);
                errorMessageErrorDescriptionNode.InnerText = errorDescription;
                errorMessageNode.AppendChild(errorMessageErrorDescriptionNode);

                xErrorsDoc.Save(@"D:\\Temp\\camc_es05_2.xml");
            }
        }
    }

    public class NDSCommLogEntry
    {
        //[DataMember]
        public string ErrorCode;

        //[DataMember]
        public string ErrorDescription;

        public CamcMessageBase Message;

        //[DataMember]
        public string OriginalMessageAction;

        //[DataMember]
        public string UnformattedMessage;
    }


    [Serializable]
    public class UnitTestCommand : NDSBaseCommand
    {
        public UnitTestCommand(IntegrationMailMessage args)
            : base(args)
        {

        }
        private IOutputConnector _connector;
        public IOutputConnector Connector
        {
            get
            {
                if (_connector == null)
                {
                    _connector = CreateOutputConnector();
                }
                return _connector;
            }
        }

        /*
        public void Execute()
        {
            this.Execute((NdsOutputConnector)Connector);
        }

        protected override void Execute(NdsOutputConnector ndsOutputConnector)
        {
            ndsOutputConnector.WriteCommLogEntry = WriteCommLogEntry;
        }

        protected override void ResponseReceived(object sender, Library.Framework.ResponseEventArguments e)
        {
            base.ResponseReceived(sender, e);
        }
         * */
    }
}
