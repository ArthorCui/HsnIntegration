using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using NUnit.Framework;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class DataContractSerializeTest
    {
        [Test]
        public void HostDataContractMixXmlSerializeTest()
        {
            var host1 = new Host() { Id = 1, Name = "Host1", Address = "10.0.0.1" };

            new XmlSerializer(typeof(Host)).Serialize(File.OpenWrite(@"temp.xml"), host1);

            Console.WriteLine();

        }
    }

    [DataContract]
    [XmlSerializerFormat]
    public class Host
    {
        [DataMember, XmlAttribute]
        public int Id { get; set; }

        [DataMember, XmlAttribute]
        public string Name { get; set; }

        [DataMember, XmlAttribute]
        public string Address { get; set; }
    }

    [Serializable]
    public class Host2
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Address { get; set; }
    }
}
