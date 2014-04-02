using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNs.Models
{
    partial class DomainValue
    {
        private static DomainValue FromIP(string p)
        {
            IPAddress ip = TryGetIP(p);
            if (ip != null)
                return DomainValue.FromIP(ip);
            else
                return null;
        }

        private static DomainValue FromIP(IEnumerable<string> ipStrings)
        {
            if (ipStrings.Count() > 0)
            {
                List<IPAddress> ips = new List<IPAddress>(ipStrings.Count());
                foreach (var ipString in ipStrings)
                {
                    IPAddress ip = TryGetIP(ipString);
                    if (ip != null)
                        ips.Add(ip);
                }
                if (ips.Count > 0)
                {
                    var ip4s = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    var ip6s = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

                    bool anyIp4 = ip4s.Any();
                    bool anyIp6 = ip6s.Any();

                    if (anyIp4 || anyIp6)
                    {
                        JObject domainObject = new JObject();
                        if (anyIp4)
                            domainObject["ip"] = new JArray(ip4s.Select(m => m.ToString()));
                        if (anyIp6)
                            domainObject["ip6"] = new JArray(ip6s.Select(m => m.ToString()));

                        return new DomainValue(domainObject);
                    }
                }
            }
            return null;
        }

        public static DomainValue FromIP(IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return new DomainValue(string.Format("{{\"ip\":\"{0}\"}}", ip.ToString()));

            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return new DomainValue(string.Format("{{\"ip6\":\"{0}\"}}", ip.ToString()));

            return null;
        }

        static IPAddress TryGetIP(string ipString)
        {
            IPAddress ip;
            if (IPAddress.TryParse((string)ipString, out ip))
                return ip;
            return null;
        }


    }
}
