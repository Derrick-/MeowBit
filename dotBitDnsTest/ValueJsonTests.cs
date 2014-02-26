using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NamecoinLib.Responses;
using System.Linq;

namespace dotBitDnsTest
{
    [TestClass]
    public class ValueJsonTests
    {
        [TestMethod]
        public void JsonParseTest()
        {
            string email = "gtempns1@san.gs";
            string ip = "78.47.86.43";
            string ipWWW = "78.47.86.44";
            string Json1 = "{\"ip\": \"" + ip + "\", \"email\": \"" + email + "\", \"info\": { \"status\": \"On sale.\" }, \"map\": {\"\": \"" + ip + "\", \"www\": \"" + ipWWW + "\"}} ";

            NameShowResponse Response1 = new NameShowResponse()
            {
                address = "Json1.bit",
                value = Json1
            };

            var result = Response1.GetValue();

            Assert.AreEqual(ip, result.ip);
            Assert.AreEqual(email, result.email);

            var mapped = result.Maps;
            Assert.IsTrue(mapped.Contains(""));
            Assert.IsTrue(mapped.Contains("www"));

            Assert.AreEqual(ip, result.GetMapIps("").First());
            Assert.AreEqual(ipWWW, result.GetMapIps("www").First());

            Assert.AreEqual("On sale.", result.GetInfoValue("status").First());
        }

        [TestMethod]
        public void TranslateParseTest()
        {
            string Json1 = "{\"translate\": \"bitcoin.org\", \"ns\": [\"1.2.3.4\", \"1.2.3.5\"]}";

            NameShowResponse Response1 = new NameShowResponse()
            {
                address = "Json1.bit",
                value = Json1
            };

            var result = Response1.GetValue();
            Assert.IsNull(result.ip);

            Assert.AreEqual("bitcoin.org", result.translate);
            Assert.IsTrue(result.ns.Contains("1.2.3.4"));
            Assert.IsTrue(result.ns.Contains("1.2.3.5"));
        }
    }
}
