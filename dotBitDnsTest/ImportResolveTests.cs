using ARSoft.Tools.Net.Dns;
using dotBitNs.Models;
using dotBitNs.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace dotBitDnsTest
{
    [TestClass]
    public class ImportResolveTests
    {
        Mocks.NmcClientMock client;
        DotBitResolver resolver;

        private string import_example =
@"{
    ""import"": [ [""s/shareddata"", ""www""], [""s/shareddata"", ""ftp""] ],
    ""ip""    : ""10.2.3.4"",
    ""map""   : { ""ftp"" : { ""email"": ""example@mail.bit"" } }
}";

        private string shareddata =
@"{
    ""ip"" : ""10.0.0.1"",
    ""map"": { 
        ""www"" : { ""alias"": """" },
        ""ftp"" : { ""ip"": ""10.0.1.2"", ""email"": ""shared@mail.bit"" }
    }
}";
  
        [TestInitialize]
        public void Initialize()
        {
            client = new Mocks.NmcClientMock();

            client.addName("d/import_www", import_example);
            client.addName("s/shareddata", shareddata);

            resolver = new DotBitResolver(client.DnsLookup, client);
        }

        [TestMethod]
        public void ImportToMapTest()
        {
            var qFtp = new DnsQuestion("ftp.import_www.bit", RecordType.Any, RecordClass.Any);
            var qWww = new DnsQuestion("www.import_www.bit", RecordType.Any, RecordClass.Any);

            var answerFtp = resolver.GetAnswer(qFtp);
            var answerWww = resolver.GetAnswer(qWww);

            ARecord aFtp = answerFtp.AnswerRecords.FirstOrDefault() as ARecord;
            CNameRecord cWww = answerWww.AnswerRecords.FirstOrDefault() as CNameRecord;
            ARecord aWww = answerWww.AdditionalRecords.FirstOrDefault() as ARecord;

            Assert.AreEqual("10.0.1.2", aFtp.Address.ToString());

            Assert.AreEqual("www.import_www.bit", cWww.Name);
            Assert.AreEqual("import_www.bit", cWww.CanonicalName);
            Assert.AreEqual("10.2.3.4", aWww.Address.ToString());

        }

    }
}
