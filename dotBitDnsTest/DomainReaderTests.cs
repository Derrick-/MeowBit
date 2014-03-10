// Derrick Slopey | derrick@alienseed.com
// March 8 2014
// Tests for DomainValue object

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using dotBitNS.Models;
using System.Net;

namespace dotBitDnsTest
{
    [TestClass]
    public class DomainReaderTests
    {
        string Example_2_5_generic =
@"{
    ""ip""      : ""192.168.1.1"",
    ""ip6""     : ""2001:4860:0:1001::68"",
    ""tor""     : ""eqt5g4fuenphqinx.onion"",
    ""email""   : ""hostmaster@example.bit"",
    ""info""    : ""Example & Sons Co."",
    ""service"" : [ [""smtp"", ""tcp"", 10, 0, 25, ""mail""] ],
    ""tls"": {
        ""tcp"": 
	    {
            ""443"": [ [1, ""660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787"", 1] ],
            ""25"": [ [1, ""660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787"", 1] ]
	    }
    },
    ""map"":
    {
        ""www"" : { ""alias"": """" },
        ""ftp"" : { ""ip"": [""10.2.3.4"", ""10.4.3.2""] },
        ""mail"": { ""ns"": [""ns1.host.net"", ""ns12.host.net""] }
    }
}";

        string Example_nameservers =
@"{
    ""ns""      : [""192.168.1.1"",""10.2.3.4""],
}";

// TLS 2.5:  http://forum.namecoin.info/viewtopic.php?f=5&t=1137
//    "tls": {
//        "protocol": {
//            port: [[matchtype, "matchvalue", includeSubdomains],[matchtype, "matchvalue", includeSubdomains]]
//        } {
//            port: [[matchtype, "matchvalue", includeSubdomains]]
//        }
//    }
//    Port:
//        Decimal representation of the port number on which a TLS-based service is assumed to exist. The number has no leading zeros. 
//    Protocol:
//        Lower or uppercase string: tcp, udp, sctp
//    matchtype: 
//        0: Exact match (the entire certificate)
//        1: SHA-256
//        2: SHA-512 
//    matchvalue:
//        The hash or certificate in hex without any delimiters, as a string in hex digits. 
//    includeSubdomains:
//        0: do not enforce this rule for subdomains
//        1: enforce this rule for subdomains
//        The includeSubdomains rule cannot be revoked by a subdomains that turn it off. 


        [TestMethod]
        public void ReadJson()
        {
            var domain = DomainValue.JsonDeserialize(Example_2_5_generic);
            Assert.IsNotNull(domain);
        }

        [TestMethod]
        public void ReadDomain_V2_5()
        {
            var domain = new DomainValue(Example_2_5_generic);

            Assert.AreEqual("192.168.1.1", domain.IpNames.First());
            Assert.AreEqual("2001:4860:0:1001::68", domain.Ip6Names.First());
            Assert.AreEqual("eqt5g4fuenphqinx.onion", domain.Tor);
            Assert.AreEqual("hostmaster@example.bit", domain.Email);
            Assert.AreEqual("Example & Sons Co.", domain.Info.First());
        }

        [TestMethod]
        public void ReadIpLists()
        {
            var domain = new DomainValue(Example_2_5_generic);

            IPAddress ip4expected = IPAddress.Parse("192.168.1.1");
            IPAddress ip6expected = IPAddress.Parse("2001:4860:0:1001::68");

            Assert.AreEqual(ip4expected, domain.Ips.First());
            Assert.AreEqual(ip6expected, domain.Ip6s.First());
        }

        [TestMethod]
        public void ReadNameServerList()
        {
            var domain = new DomainValue(Example_nameservers);
            var nameservers = domain.Ns;
            Assert.AreEqual("192.168.1.1", nameservers.First());
            Assert.AreEqual("10.2.3.4", nameservers.Last());

        }

        [TestMethod]
        public void ReadServiceRecord()
        {
            var domain = new DomainValue(Example_2_5_generic);

            ServiceRecord servicerecord = domain.Service.First();
            Assert.AreEqual("smtp", servicerecord.SrvName);
            Assert.AreEqual("tcp", servicerecord.Protocol);
            Assert.AreEqual(10, servicerecord.Priority);
            Assert.AreEqual(0, servicerecord.Weight);
            Assert.AreEqual(25, servicerecord.Port);
            Assert.AreEqual("mail", servicerecord.Target);
        }

        [TestMethod]
        public void ReadTLSRecord()
        {
            var domain = new DomainValue(Example_2_5_generic);

            var p443 = domain.GetTlsForPort("tcp", "443");
            var p443first = p443.First;
            Assert.AreEqual(1, (int)p443first[0]);
            Assert.AreEqual("660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787", (string)p443first[1]);
            Assert.AreEqual(1, (int)p443first[2]);

            var p25 = domain.GetTlsForPort("tcp", "25");
            var p25first = p25.First;
            Assert.AreEqual(1, (int)p25first[0]);
            Assert.AreEqual("660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787", (string)p25first[1]);
            Assert.AreEqual(1, (int)p25first[2]);

            var p123 = domain.GetTlsForPort("tcp", "123");
            Assert.IsNull(p123);

            var p321 = domain.GetTlsForPort("bull", "321");
            Assert.IsNull(p321);

        }

        [TestMethod]
        public void ReadMaps()
        {
            var domain = new DomainValue(Example_2_5_generic);

            var www = domain.GetMap("www");
            var wwwAlias = www.Alias;
            Assert.AreEqual("", wwwAlias);

            var ftp = domain.GetMap("ftp");
            var ftpIp = ftp.IpNames;
            Assert.AreEqual("10.2.3.4", ftpIp.First());
            Assert.AreEqual("10.4.3.2", ftpIp.Skip(1).First());

            var mail = domain.GetMap("mail");
            var mailNs = mail.Ns;
            Assert.AreEqual("ns1.host.net", mailNs.First());
            Assert.AreEqual("ns12.host.net", mailNs.Skip(1).First());

            var none = domain.GetMap("none");
            Assert.IsNull(none);
        }
    }

}

