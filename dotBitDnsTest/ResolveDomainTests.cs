using ARSoft.Tools.Net.Dns;
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

        static readonly Dictionary<string, IPAddress> dnsMockRecords = new Dictionary<string, IPAddress>()
        {
            {"json1.com.", new IPAddress(new byte[]{0,0,0,1})}
        };

        [TestMethod]
        public void ResolveRootTest()
        {
            var resolver = new DotBitResolver(mockResolveDns, new dotBitNs.Server.DotBitResolver.LookupDomainValueRootHandler(mockLookupDotBit));

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

            var resolver = new DotBitResolver(mockResolveDns, new dotBitNs.Server.DotBitResolver.LookupDomainValueRootHandler(mockLookupDotBit));

            var qWww = new DnsQuestion("www.json1.bit", RecordType.Any, RecordClass.Any);
            var answer = resolver.GetAnswer(qWww);
            var expectedWwwName = "json1.com.";
            var expectedWwwAddress = dnsMockRecords[expectedWwwName];

            Assert.IsNotNull(answer);
            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(ARecord));
            Assert.AreEqual(expectedWwwName, answer.AnswerRecords.First().Name);
            Assert.AreEqual(expectedWwwAddress, ((ARecord)answer.AnswerRecords.First()).Address);

        }

        [TestMethod]
        public void ResolveMapOnlyTest()
        {
            var resolver = new DotBitResolver(mockResolveDns, new dotBitNs.Server.DotBitResolver.LookupDomainValueRootHandler(mockLookupDotBit));
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
            var resolver = new DotBitResolver(mockResolveDns, new dotBitNs.Server.DotBitResolver.LookupDomainValueRootHandler(mockLookupDotBit));
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
            var resolver = new DotBitResolver(mockResolveDns, new dotBitNs.Server.DotBitResolver.LookupDomainValueRootHandler(mockLookupDotBit));
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


        private NameShowResponse mockLookupDotBit(string root)
        {
            string value;

            switch (root)
            {
                case "nx":
                    value = Example_MapOnly_nx_bit;
                    break;
                case "nest":
                    value = Example_MapOnly_www_nest_bit;
                    break;
                case "maponlyarray":
                    value = Example_MapOnly_maponlyarray_bit;
                    break;
                case "json1":
                    value = Json1;
                    break;
                default:
                    throw new ArgumentException(root + " is not configured for testing.");
            }

            return new NameShowResponse()
            {
                name = "d/" + root,
                value = value,
            };
        }

        private DnsMessage mockResolveDns(string name, RecordType recordType, RecordClass recordClass)
        {
            DnsMessage answer = null;
            bool any = recordType == RecordType.Any;
            if ((any || recordType == RecordType.A) && dnsMockRecords.ContainsKey(name))
            {
                var ip = dnsMockRecords[name];
                answer = new DnsMessage();
                answer.AnswerRecords.Add(new ARecord(name, 0, ip));
            }
            return answer;
        }

    }
}
