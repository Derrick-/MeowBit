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

            var answer = resolver.GetAnswer(qFtp);

            ARecord a = answer.AnswerRecords.FirstOrDefault() as ARecord;

            Assert.AreEqual("10.0.1.2", a.Address.ToString());

        }

    }
}
