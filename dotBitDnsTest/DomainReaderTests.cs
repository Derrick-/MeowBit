using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

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


        // http://forum.namecoin.info/viewtopic.php?f=5&t=1137
//    tls (http://dot-bit.org/Namespace:Domain_names_v2.0#TLS_support)
//    Specifies one or more certificate fingerprints and allow to enforce secured connections (see HSTS)
//    Allowed values :
//    - sha1 : upper or lower case, with or without ':'
//    - enforce : "self" (current FQDN) or "*" (all subdomains)

//    "tls": {
//    "sha1": ["15:91:52:97:10:88:CD:44:9C:F7:23:81:78:C3:50:3B:09:20:56:2A", "630884E279CB1107F1FB8A6B11A64D1B14763F8E"],
//    "enforce": "*"
//    }

        [TestMethod]
        public void ReadJson()
        {
            var domain = JsonDeserialize(Example_2_5_generic);
            Assert.IsNotNull(domain);
        }

        [TestMethod]
        public void ReadDomain_V2_5()
        {
            var domain = JsonDeserialize(Example_2_5_generic);

            Assert.AreEqual("192.168.1.1", (string)domain.ip);
            Assert.AreEqual("2001:4860:0:1001::68", (string)domain.ip6);
            Assert.AreEqual("eqt5g4fuenphqinx.onion", (string)domain.tor);
            Assert.AreEqual("eqt5g4fuenphqinx.onion", (string)domain.tor);
            Assert.AreEqual("hostmaster@example.bit", (string)domain.email);
            Assert.AreEqual("Example & Sons Co.", (string)domain.info);

            var servicerecord = domain.service[0];
            Assert.AreEqual("smtp", (string)servicerecord[0]);
            Assert.AreEqual("tcp", (string)servicerecord[1]);
            Assert.AreEqual(10, (int)servicerecord[2]);
            Assert.AreEqual(0, (int)servicerecord[3]);
            Assert.AreEqual(25, (int)servicerecord[4]);
            Assert.AreEqual("mail", (string)servicerecord[5]);

            var p443 = GetTlsForPort(domain, "tcp", "443");
            var p443first = p443.First;
            Assert.AreEqual(1, (int)p443first[0]);
            Assert.AreEqual("660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787", (string)p443first[1]);
            Assert.AreEqual(1, (int)p443first[2]);

            var p25 = GetTlsForPort(domain, "tcp","25");
            var p25first = p25.First;
            Assert.AreEqual(1, (int)p25first[0]);
            Assert.AreEqual("660008F91C07DCF9058CDD5AD2BAF6CC9EAE0F912B8B54744CB7643D7621B787", (string)p25first[1]);
            Assert.AreEqual(1, (int)p25first[2]);

            var www = GetMap(domain, "www");
            var wwwAlias = GetAlias(www);
            Assert.AreEqual("", (string)wwwAlias);

            var ftp = GetMap(domain, "ftp");
            var ftpIp = GetIp(ftp);
            Assert.AreEqual("10.2.3.4", (string)ftpIp[0]);
            Assert.AreEqual("10.4.3.2", (string)ftpIp[1]);
            
            var mail = GetMap(domain, "mail");
            var mailNs = GetNs(mail);
            Assert.AreEqual("ns1.host.net", (string)mailNs[0]);
            Assert.AreEqual("ns12.host.net", (string)mailNs[1]);
        }

        private object GetAlias(dynamic domain)
        {
            return domain.alias;
        }

        private object GetIp(dynamic domain)
        {
            return domain.ip;
        }

        private object GetNs(dynamic domain)
        {
            return domain.ns;
        }
        
        private static dynamic GetTlsForPort(dynamic domain, string protocol, string port)
        {
            return domain.tls[protocol][port];
        }

        private object GetMap(dynamic domain, string subdomain)
        {
            return domain.map[subdomain];
        }


        dynamic JsonDeserialize(string json)
        {
            using (var sr = new StringReader(json))
            using (var reader = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create();
                return ser.Deserialize<dynamic>(reader);
            }
        }
    }

}

