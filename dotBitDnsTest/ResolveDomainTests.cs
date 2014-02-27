using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NamecoinLib.Responses;
using dotBitNS.Server;
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


        [TestMethod]
        public void ResolveSubdomainsTest()
        {
            NameShowResponse Response1 = new NameShowResponse()
            {
                address = "Json1.bit",
                value = Json1
            };

            NameValue value = Response1.GetValue();

            NameValue valueRoot = NameServer.Resolver.ResolveSubdomain(new string[] { "Json1", "bit" }, value);

            NameValue valueUs = NameServer.Resolver.ResolveSubdomain(new string[] {"us", "Json1", "bit" }, value);
            NameValue valueUsWww = NameServer.Resolver.ResolveSubdomain(new string[] { "www", "us", "Json1", "bit" }, value);

            NameValue valueEu = NameServer.Resolver.ResolveSubdomain(new string[] { "eu", "Json1", "bit" }, value);
            NameValue valueEuWww = NameServer.Resolver.ResolveSubdomain(new string[] { "www", "eu", "Json1", "bit" }, value);

            NameValue valueMany = NameServer.Resolver.ResolveSubdomain(new string[] { "many", "Json1", "bit" }, value);

            NameValue valueOther = NameServer.Resolver.ResolveSubdomain(new string[] { "blah", "Json1", "bit" }, value);

            Assert.AreEqual(ip, (String)valueRoot.ip);

            Assert.AreEqual(ipWWW, (String)valueUs.ip);
            Assert.AreEqual(null, (String)valueUsWww.ip);
            Assert.AreEqual("", (String)valueUsWww.alias);

            Assert.AreEqual(null, (String)valueEu.ip);
            Assert.AreEqual("us.@", (String)valueEuWww.alias);

            var ipMany=valueMany.GetIp4Addresses();
            Assert.IsTrue(ipMany.Any(m => m.Equals(IPAddress.Parse(ipWWW))));
            Assert.IsTrue(ipMany.Any(m => m.Equals(IPAddress.Parse(ipWWW2))));

            Assert.AreEqual("json1.com.", valueOther.alias);
        }
    }
}
