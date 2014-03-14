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
    public class DomainValue : BaseNameValue
    {
        public DomainValue(string json) : base(json) { }

        private string _Alias = null;
        private IEnumerable<IPAddress> _Ips = null;
        private IEnumerable<IPAddress> _Ip6s = null;
        private IEnumerable<string> _IpNames = null;
        private IEnumerable<string> _Ip6Names = null;
        private string _Email = null;
        private string _Tor = null;
        private IEnumerable<string> _Ns = null;
        private IEnumerable<string> _Info = null;
        private string _Delegate = null;
        private string _Import = null;
        private string _Translate = null;
        private IEnumerable<ServiceRecord> _Service = null;
        private JObject _Maps = null;

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

        public IEnumerable<string> IpNames
        { get { return _IpNames ?? (_IpNames = GetStringList("ip")); } }

        public IEnumerable<string> Ip6Names
        { get { return _Ip6Names ?? (_Ip6Names = GetStringList("ip6")); } }

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

        public JObject Maps
        {
            get
            {
                if (_Maps != null) return _Maps;

                string propName = "map";
                JToken maps = domain.GetValue(propName);
                if (maps != null && maps.Type == JTokenType.Object)
                {
                    _Maps = (JObject)maps;
                }
                else
                    _Maps = new JObject();
                return _Maps;
            }
        }

        public DomainValue GetMap(string subdomain)
        {
                var map = Maps[subdomain];
                if (map != null && map.Type == JTokenType.Object)
                    return new DomainValue(map.ToString());
                return null;
        }

    }

}

