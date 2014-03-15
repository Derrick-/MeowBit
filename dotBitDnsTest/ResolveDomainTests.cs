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

            var qWww = new DnsQuestion("www.Json1.bit", RecordType.Any, RecordClass.Any);
            var answer = resolver.GetAnswer(qWww);
            var expectedWwwName = "json1.com.";
            var expectedWwwAddress = dnsMockRecords[expectedWwwName];

            Assert.IsNotNull(answer);
            Assert.IsInstanceOfType(answer.AnswerRecords.First(), typeof(ARecord));
            Assert.AreEqual(expectedWwwName, answer.AnswerRecords.First().Name);
            Assert.AreEqual(expectedWwwAddress, ((ARecord)answer.AnswerRecords.First()).Address);

        }

        private NameShowResponse mockLookupDotBit(string root)
        {
            return new NameShowResponse()
            {
                name = "d/" + root,
                value = Json1,
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
