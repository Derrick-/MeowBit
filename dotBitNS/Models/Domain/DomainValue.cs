// Derrick Slopey | derrick@alienseed.com
// March 10 2014
// DomainValue object

using System;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace dotBitDnsTest
{
    public class DomainValue
    {
        JObject domain;

        public DomainValue(string json)
        {
            domain = JsonDeserialize(json);
        }

        public string Alias
        {
            get { return GetString("alias"); }
        }

        public IEnumerable<string> Ip
        { get { return GetStringList("ip"); } }

        public IEnumerable<string> Ip6
        { get { return GetStringList("ip6"); } }

        public string Email
        { get { return GetString("email"); } }

        public string Tor
        { get { return GetString("tor"); } }

        public IEnumerable<string> Ns
        { get { return GetStringList("ns"); } }

        public IEnumerable<string> Info
        { get { return GetStringList("info"); } }

        public IEnumerable<ServiceRecord> Service
        {
            get
            {
                string propName = "service";
                if (DomainHasProperty(propName))
                {
                    JToken services = domain.GetValue(propName);
                    if (services.Type == JTokenType.Array)
                    {
                        return services.Select(m => ServiceRecord.FromToken(m)).Where(m => m != null);
                    }
                }
                return null;
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

        public DomainValue GetMap(string subdomain)
        {
            string propName = "map";
            JToken maps = domain.GetValue(propName);
            if (maps != null && maps.Type == JTokenType.Object)
            {
                var map = maps[subdomain];
                if (map != null && map.Type == JTokenType.Object)
                    return new DomainValue(map.ToString());
            }
            return null;
        }

        private string GetString(string propName)
        {
            if (DomainHasProperty(propName))
            {
                JToken value = domain.GetValue(propName);
                if (value.Type == JTokenType.String)
                    return value.ToString();
            }
            return null;
        }

        private IEnumerable<string> GetStringList(string propName)
        {
            if (DomainHasProperty(propName))
            {
                JToken value = domain.GetValue(propName);
                if (value.Type == JTokenType.Array)
                    return value.Select(m => m.ToString());
                return new string[] { value.ToString() };
            }
            return null;
        }

        private bool DomainHasProperty(string propName)
        {
            return HasProperty(domain, propName);
        }

        private static bool HasProperty(JObject obj, string propName)
        {
            return obj.Properties().Any(m => m.Name == propName);
        }

        public static dynamic JsonDeserialize(string json)
        {
            using (var sr = new StringReader(json))
            using (var reader = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create();
                return ser.Deserialize<JObject>(reader);
            }
        }

    }

    public class ServiceRecord
    {

        public static ServiceRecord FromToken(JToken item)
        {
            ServiceRecord srv = null;
            if (item[0].Type == JTokenType.String
                && item[1].Type == JTokenType.String
                && item[2].Type == JTokenType.Integer
                && item[3].Type == JTokenType.Integer
                && item[4].Type == JTokenType.Integer
                && item[5].Type == JTokenType.String)
            {
                srv = new ServiceRecord((string)item[0], (string)item[1], (int)item[2], (int)item[3], (int)item[4], (string)item[5]);
            }
            return srv;
        }

        public ServiceRecord(string SrvName, string Protocol, int Priority, int Weight, int Port, string Target)
        {
            this.SrvName = SrvName;
            this.Protocol = Protocol;
            this.Priority = Priority;
            this.Weight = Weight;
            this.Port = Port;
            this.Target = Target;
        }

        public string SrvName { get; set; }

        public string Protocol { get; set; }

        public int Priority { get; set; }

        public int Weight { get; set; }

        public int Port { get; set; }

        public string Target { get; set; }
    }


}

