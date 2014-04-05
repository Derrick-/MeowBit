using ARSoft.Tools.Net.Dns;
using dotBitNs;
using dotBitNs.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NamecoinLib.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace dotBitDnsTest
{
    [TestClass]
    public class ResolveDomainTests
    {
        Mocks.NmcClientMock client;
        DotBitResolver resolver;

        [TestInitialize]
        public void Initialize()
        {
            client = new Mocks.NmcClientMock();

            client.addName("d/nx", Example_MapOnly_nx_bit);
            client.addName("d/nest", Example_MapOnly_www_nest_bit);
            client.addName("d/maponlyarray", Example_MapOnly_maponlyarray_bit);
            client.addName("d/json1", Json1);

            client.addDnsRecord("json1.com", new IPAddress(new byte[] { 0, 0, 0, 1 }));

            resolver = new DotBitResolver(client.DnsLookup, client);
        }

        const string email = "my@testemail.net";
        const string ip = "78.47.86.43";
        const string ipWWW = "78.47.86.44";
        const string ipWWW2 = "1.2.3.4";

        string Json1 = "{" +
            "    \"ip\" : \"" + ip + "\"," +
            "    \"email\": \"" + email + "\"," +
            "    \"info\": { \"status\": \"On sale.\" }," +
            "    \"map\":" +
            "    {" +
            "        \"us\":" +
            "        {" +
            "            \"ip\" : \"" + ipWWW + "\"," +
            "            \"map\": { \"www\": { \"alias\": \"\" } }" +
            "        }," +
            "        \"eu\":" +
            "        {" +
            "            \"map\": { \"www\": { \"alias\": \"us.@\" } }" +
            "        }," +
            "        \"many\":" +
            "        {" +
            "            \"ip\" : [\"" + ipWWW + "\",\"" + ipWWW2 + "\"]" +
            "        }," +
            "        \"*\": { \"alias\": \"json1.com.\" }" +
            "    }" +
            "}";

        string Example_MapOnly_nx_bit = @"{""map"": {"""":""178.248.244.15""}}";

        string Example_MapOnly_maponlyarray_bit = @"{""map"": {"""":[""1.2.3.4"",""4.3.2.1"",""2400:cb00:2049:1::adf5:3b6b""]}}";

        string Example_MapOnly_www_nest_bit = @"{""map"": {""www"":{""map"": {"""":""178.248.244.15""}}}}";

        [TestMethod]
        public void ResolveRootTest()
        {
            var qRoot = new DnsQuestion("json1.bit", RecordType.Any, RecordClass.Any);
            var answer = resolver.GetAnswer(qRoot);
            var expectedRootName = "json1.bit";
            var expectedRootAddress = IPAddress.Parse(ip);

            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(ARecord));
            Assert.AreEqual(expectedRootName, answer.AnswerRecords.First().Name);
            Assert.AreEqual(expectedRootAddress, ((ARecord)answer.AnswerRecords.First()).Address);

        }

        [TestMethod]
        public void ResolveSubdomainsTest()
        {
            var qWww = new DnsQuestion("www.json1.bit", RecordType.Any, RecordClass.Any);
            var answer = resolver.GetAnswer(qWww);
            var expectedWwwName = "json1.com";
            var expectedWwwAddress = "0.0.0.1";

            Assert.IsNotNull(answer);
 
            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(CNameRecord));
            Assert.AreEqual("www.json1.bit", answer.AnswerRecords.First().Name);
            Assert.AreEqual(expectedWwwName, ((CNameRecord)answer.AnswerRecords.First()).CanonicalName);

            Assert.IsInstanceOfType(answer.AdditionalRecords.First(), typeof(ARecord));
            Assert.AreEqual(expectedWwwName, answer.AdditionalRecords.First().Name);
            Assert.AreEqual(expectedWwwAddress, ((ARecord)answer.AdditionalRecords.First()).Address.ToString());

        }

        [TestMethod]
        public void ResolveRealCnameTest()
        {
            resolver = new DotBitResolver(NameServer.DnsResolve, client);

            var qWww = new DnsQuestion("www.hackmaine.org", RecordType.Any, RecordClass.Any);
            var answer = resolver.GetAnswer(qWww);
            var expectedWwwName = "hackmaine.org";

            Assert.IsNotNull(answer);

            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(CNameRecord));
            Assert.AreEqual("www.hackmaine.org", answer.AnswerRecords.First().Name);
            Assert.AreEqual(expectedWwwName, ((CNameRecord)answer.AnswerRecords.First()).CanonicalName);
 
            Assert.IsInstanceOfType(answer.AdditionalRecords.First(), typeof(ARecord));
            Assert.AreEqual(expectedWwwName, answer.AdditionalRecords.First().Name);

        }

        [TestMethod]
        public void ResolveMapOnlyTest()
        {

            var q = new DnsQuestion("nx.bit", RecordType.Any, RecordClass.Any);
            string expectedA = "178.248.244.15";
            var answer = resolver.GetAnswer(q);

            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(ARecord));

            ARecord a = answer.AnswerRecords.First() as ARecord;
            Assert.AreEqual(expectedA, a.Address.ToString());

        }

        [TestMethod]
        public void ResolveMapNestTest()
        {

            var q = new DnsQuestion("www.nest.bit", RecordType.Any, RecordClass.Any);
            string expectedA = "178.248.244.15";
            var answer = resolver.GetAnswer(q);

            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(ARecord));

            ARecord a = answer.AnswerRecords.First() as ARecord;
            Assert.AreEqual(expectedA, a.Address.ToString());

        }

        [TestMethod]
        public void ResolveMapOnlyArrayTest()
        {

            var q = new DnsQuestion("maponlyarray.bit", RecordType.Any, RecordClass.Any);

            string expectedA1 = "1.2.3.4";
            string expectedA2 = "4.3.2.1";

            string expectedAAAA = "2400:cb00:2049:1::adf5:3b6b";

            var answer = resolver.GetAnswer(q);

            var Aanswers = answer.AnswerRecords.Where(m => m.RecordType == RecordType.A);
            var AAAAanswers = answer.AnswerRecords.Where(m => m.RecordType == RecordType.Aaaa);

            Assert.IsInstanceOfType(Aanswers.First(), typeof(ARecord));

            ARecord a1 = Aanswers.First() as ARecord;
            ARecord a2 = Aanswers.Last() as ARecord;

            AaaaRecord aaaa = AAAAanswers.First() as AaaaRecord;

            Assert.AreEqual(expectedA1, a1.Address.ToString());
            Assert.AreEqual(expectedA2, a2.Address.ToString());

            Assert.AreEqual(expectedAAAA, aaaa.Address.ToString());
        }

    }
}
