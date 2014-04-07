using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotBitNs.Server;
using ARSoft.Tools.Net.Dns;
using System.Linq;

namespace dotBitDnsTest
{
    [TestClass]
    public class TranslateTests
    {
        Mocks.NmcClientMock client;
        DotBitResolver resolver;
        
        [TestInitialize]
        public void Initialize()
        {
            client = new Mocks.NmcClientMock();

            client.addName("d/translate1", translate1);
            client.addName("d/otherhost", otherhost);

            resolver = new DotBitResolver(client.DnsLookup, client);
        }


        public string translate1 =
@"{
    ""translate"": ""otherhost.bit.""
}";

        public string otherhost =
@"{
    ""ip"":     ""9.8.7.5"",
    ""map"":
    {
        ""*""    : { ""ip"": ""1.2.3.4"" }
    }
}";

        [TestMethod]
        public void translateRoot()
        {
            var question = new DnsQuestion("translate1.bit", RecordType.Any, RecordClass.Any);

            var answer = resolver.GetAnswer(question);

            Assert.IsNotNull(answer, "No answer");

            DNameRecord d = answer.AnswerRecords.FirstOrDefault() as DNameRecord;
            ARecord a = answer.AdditionalRecords.FirstOrDefault() as ARecord;

            Assert.IsNotNull(d, "No DNAME record found");
            Assert.IsNotNull(a, "No A record found");

            Assert.AreEqual("translate1.bit", d.Name);
            Assert.AreEqual("otherhost.bit", d.Target);
            Assert.AreEqual("9.8.7.5", a.Address.ToString());
        }

        [TestMethod]
        public void translateSub()
        {
            var question = new DnsQuestion("www.translate1.bit", RecordType.Any, RecordClass.Any);

            var answer = resolver.GetAnswer(question);

            Assert.IsNotNull(answer, "No answer");

            DNameRecord d = answer.AnswerRecords.FirstOrDefault() as DNameRecord;
            ARecord a = answer.AdditionalRecords.FirstOrDefault() as ARecord;

            Assert.IsNotNull(d, "No DNAME record found");
            Assert.IsNotNull(a, "No A record found");

            Assert.AreEqual("www.translate1.bit", d.Name);
            Assert.AreEqual("www.otherhost.bit", d.Target);
            Assert.AreEqual("1.2.3.4", a.Address.ToString());
        }
    }
}
