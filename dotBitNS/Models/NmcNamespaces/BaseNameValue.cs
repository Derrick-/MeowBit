using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS.Models
{
    public class BaseNameValue
    {
        protected readonly JObject domain;

        public static T Instantiate<T>(string json) where T : BaseNameValue
        {
            return (T)Activator.CreateInstance(typeof(T), json);
        }

        public BaseNameValue(string json)
        {
            domain = JsonDeserialize(json);
        }

        protected string GetString(string propName)
        {
            if (DomainHasProperty(propName))
            {
                JToken value = domain.GetValue(propName);
                if (value.Type == JTokenType.String)
                    return value.ToString();
            }
            return null;
        }

        protected IEnumerable<string> GetStringList(string propName)
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

        protected static IEnumerable<IPAddress> StringListToIPList(IEnumerable<string> addresses)
        {
            List<IPAddress> toReturn = new List<IPAddress>();
            if (addresses != null)
                foreach (var name in addresses)
                {
                    IPAddress ip;
                    if (IPAddress.TryParse(name, out ip))
                        toReturn.Add(ip);
                }
            return toReturn;
        }

        protected bool DomainHasProperty(string propName)
        {
            return HasProperty(domain, propName);
        }

        protected static bool HasProperty(JObject obj, string propName)
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
}
