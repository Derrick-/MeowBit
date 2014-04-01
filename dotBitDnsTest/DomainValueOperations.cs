using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotBitNs.Models;
using System.Linq;
using System.Net;

namespace dotBitDnsTest
{

    [TestClass]
    public class DomainValueOperations :BaseDomainTest
    {

        public string Example_mapsonly1 =
@"{
    ""map"":
    {
        """"    : { ""ip"": ""1.1.1.1"" },
        ""www"" : { ""alias"": """" },
        ""ftp"" : { ""ip"": [""10.2.3.4"", ""10.4.3.2""] },
        ""mail"": { ""ns"": [""ns1.host.net"", ""ns12.host.net""] }
    }
}";
    
        public string Example_mapsonly2 =
@"{
    ""map"":
    {
        """"    : { ""ns"": [""ns1.host.net"", ""ns12.host.net""], ""ip"": [""10.2.3.4"", ""10.4.3.2""] },
    }
}";

        //[TestMethod]
        //public void TestAddDomainIp()
        //{
        //    DomainValue domain = new DomainValue(Example_2_5_generic);

        //    int initialCount=domain.Ips.Count();

        //    domain.AddIp(new IPAddress(new byte[] { 1, 1, 1, 1 }));

        //    int finalCount=domain.Ips.Count();

        //    Assert.AreEqual(initialCount + 1, finalCount);
        //}

        [TestMethod]
        public void ReadMapsOnly1()
        {
            var domain = new DomainValue(Example_mapsonly1);

            domain.ImportDefaultMap();

            IPAddress expected = new IPAddress(new byte[] { 1, 1, 1, 1 });

            Assert.AreEqual(expected, domain.Ips.Single());
        }

        [TestMethod]
        public void ReadMapsOnly2()
        {
            var domain = new DomainValue(Example_mapsonly2);

            domain.ImportDefaultMap();

            IPAddress expectedIp1 = new IPAddress(new byte[] { 10, 2, 3, 4 });
            IPAddress expectedIp2 = new IPAddress(new byte[] { 10, 4, 3, 2 });

            int expectedNsCount = 2;

            Assert.AreEqual(expectedIp1, domain.Ips.First());
            Assert.AreEqual(expectedIp2, domain.Ips.Last());

            Assert.AreEqual(expectedNsCount, domain.Ns.Count());
        }

    }
}
