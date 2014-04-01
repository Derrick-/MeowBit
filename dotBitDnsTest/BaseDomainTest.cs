using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotBitDnsTest
{
    public class BaseDomainTest
    {

        public string Example_2_5_generic =
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

        public string Example_nameservers =
@"{
    ""ns""      : [""192.168.1.1"",""10.2.3.4""],
}";

   
    }
}
