// Derrick Slopey | derrick@alienseed.com
// March 10 2014
// DomainValue object

using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace dotBitNs.Models
{
    public partial class DomainValue : BaseNameValue
    {
        public DomainValue(string json) : base(json) { }
        public DomainValue(JObject domainObject) : base(domainObject) { }

        private string _Alias = null;
        private IEnumerable<IPAddress> _Ips = null;
        private IEnumerable<IPAddress> _Ip6s = null;
        private string _Email = null;
        private string _Tor = null;
        private IEnumerable<string> _Ns = null;
        private IEnumerable<string> _Info = null;
        private string _Delegate = null;
        private string _Import = null;
        private string _Translate = null;
        private IEnumerable<ServiceRecord> _Service = null;
        private Dictionary<string, DomainValue> _Maps = null;

        private void Invalidate(string p)
        {
            switch (p)
            {
                case "alias": _Alias = null; break;
                case "ip": _Ips = null; break;
                case "ip6": _Ip6s = null; break;
                case "email": _Email = null; break;
                case "tor": _Tor = null; break;
                case "ns": _Ns = null; break;
                case "info": _Info = null; break;
                case "delegate": _Delegate = null; break;
                case "import": _Import = null; break;
                case "translate": _Translate = null; break;
                case "service": _Service = null; break;
                case "maps": _Maps = null; break;
            }
        }

        public string Alias
        { get { return _Alias ?? (_Alias = GetString("alias")); } }

        public IEnumerable<IPAddress> Ips
        {
            get
            {
                if (_Ips != null) return _Ips;

                var addresses = GetStringList("ip");
                return _Ips = StringListToIPList(addresses);
            }
        }

        public IEnumerable<IPAddress> Ip6s
        {
            get
            {
                if (_Ip6s != null) return _Ip6s;

                var addresses = GetStringList("ip6");
                return _Ip6s = StringListToIPList(addresses);
            }
        }

        public string Email
        { get { return _Email ?? (_Email = GetString("email")); } }

        public string Tor
        { get { return _Tor ?? (_Tor = GetString("tor")); } }

        public IEnumerable<string> Ns
        { get { return _Ns ?? (_Ns = GetStringList("ns")); } }

        public IEnumerable<string> Info
        { get { return _Info ?? (_Info = GetStringList("info")); } }

        public string @Delegate
        { get { return _Delegate ?? (_Delegate = GetString("delegate")); } }

        public string Import
        { get { return _Import ?? (_Import = GetString("import")); } }

        public string Translate
        { get { return _Translate ?? (_Translate = GetString("translate")); } }

        public IEnumerable<ServiceRecord> Service
        {
            get
            {
                if (_Service != null) return _Service;

                string propName = "service";
                if (DomainHasProperty(propName))
                {
                    JToken services = domain.GetValue(propName);
                    if (services.Type == JTokenType.Array)
                    {
                        return _Service = services.Select(m => ServiceRecord.FromToken(m)).Where(m => m != null);
                    }
                }
                return _Service = new ServiceRecord[] { };
            }
        }

        public dynamic GetTlsForPort(string protocol, string port)
        {
            var tls = domain.GetValue("tls");
            if (tls != null)
            {
                var protObj = tls[protocol];
                if (protObj != null)
                    return protObj[port];
            }
            return null;
        }

        public Dictionary<string, DomainValue> Maps
        {
            get
            {
                if (_Maps == null)
                {
                    string propName = "map";
                    JToken maps = domain.GetValue(propName);
                    _Maps = new Dictionary<string, DomainValue>();
                    if (maps != null && maps.Type == JTokenType.Object)
                    {
                        foreach (JProperty map in maps.Where(m => m is JProperty))
                        {
                            if (map.Value.Type == JTokenType.Object)
                                _Maps.Add(map.Name, new DomainValue(map.Value.ToString()));
                            else if (map.Value.Type == JTokenType.String)
                            {
                                DomainValue newMap = DomainValue.FromIP((string)map.Value);
                                if (newMap != null)
                                    _Maps.Add(map.Name, newMap);
                            }
                            else if (map.Value.Type == JTokenType.Array)
                            {
                                var ipStrings = ((JArray)map.Value).Where(m => m.Type == JTokenType.String).Select(m => m.Value<string>());
                                DomainValue newMap = DomainValue.FromIP(ipStrings);
                                if (newMap != null)
                                    _Maps.Add(map.Name, newMap);
                            }

                        }
                    }
                }
                return _Maps;
            }
        }

        public DomainValue GetMap(string subdomain)
        {
            DomainValue value;
            if (Maps.TryGetValue(subdomain, out value))
                return value;
            return null;
        }

        public void ImportDefaultMap()
        {
            if (GetMap("") != null)
            {
                ImportValues(Maps[""]);
            }
        }

        private void ImportValues(DomainValue from, bool overwrite = false)
        {
            foreach (JProperty item in from.domain.Properties())
            {
                if (overwrite || domain[item.Name] == null)
                {
                    domain[item.Name] = item.Value;
                    Invalidate(item.Name);
                }
            }
        }
    }

}

