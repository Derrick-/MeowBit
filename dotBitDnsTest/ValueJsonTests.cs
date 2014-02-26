using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NamecoinLib.Responses;
using System.Linq;

namespace dotBitDnsTest
{
    [TestClass]
    public class ValueJsonTests
    {
        const string email = "my@testemail.net";
        const string ip = "78.47.86.43";
        const string ipWWW = "78.47.86.44";

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
            "        \"*\": { \"alias\": \"\" }" +
            "    }" +
            "}";

        [TestMethod]
        public void JsonParseTest()
        {
            NameShowResponse Response1 = new NameShowResponse()
            {
                address = "Json1.bit",
                value = Json1
            };

            var result = Response1.GetValue();

            Assert.AreEqual(ip, result.ip);
            Assert.AreEqual(email, result.email);

            var mapped = result.map;
            Assert.IsTrue(mapped.Any(m => m.name == "*"));
            Assert.IsTrue(mapped.Any(m => m.name == "us"));

            Assert.AreEqual("us.@", result.GetMapValue("eu").First().GetMapValue("www").First().alias);
            Assert.AreEqual("", result.GetMapValue("*").First().alias);
            Assert.AreEqual(ipWWW, result.GetMapValue("us").First().ip);

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
