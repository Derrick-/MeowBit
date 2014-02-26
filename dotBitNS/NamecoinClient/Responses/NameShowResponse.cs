using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using dotBitNS;

namespace NamecoinLib.Responses
{
    public class NameShowResponse
    {
        public string name { get; set; }
        public string value { get; set; }
        public string txid { get; set; }
        public string address { get; set; }
        public Int32 expires_in { get; set; }

        public NameValue GetValue()
        {
            using (var sr = new StringReader(value))
            using (var reader = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create();
                try
                {
                    return ser.Deserialize<NameValue>(reader);
                }
                catch (JsonSerializationException ex)
                {
                    ConsoleUtils.WriteWarning(ex.Message);
                    return null;
                }
            }
        }

    }

    public class NameValue
    {
        public dynamic ip { get; set; }
        public dynamic ip6 { get; set; }
        public string email { get; set; }
        public JObject info { get; set; }
        public NameDatas map { get; set; }
        public string @delegate { get; set; }
        public string import { get; set; }
        public string alias { get; set; }
        public string translate { get; set; }
        public string[] ns { get; set; }

        public IEnumerable<IPAddress> GetIp4Addresses()
        {
            return ParseIPAddresses(ip);
        }

        public IEnumerable<IPAddress> GetIp6Addresses()
        {
            return ParseIPAddresses(ip6);
        }

        private IEnumerable<IPAddress> ParseIPAddresses(JObject addresses)
        {
            List<IPAddress> toReturn = new List<IPAddress>();
            IEnumerable<string> ips;

            if (addresses.Type == JTokenType.Array)
            {
                ips = addresses.Cast<string>();
            }
            else if (addresses.Type == JTokenType.String)
                ips = new string[] { (string)ip };
            else
                return toReturn;

            IPAddress addr;
            foreach (var item in ips)
                if (IPAddress.TryParse(item, out addr))
                    toReturn.Add(addr);
            return toReturn;
        }

        public IEnumerable<string> Maps
        {
            get
            {
                return map.Select(m => m.name);
            }
        }

        public IEnumerable<string> Infos
        {
            get
            {
                return info.Properties().Select(m => m.Name);
            }
        }

        public IEnumerable<NameValue> GetMapValue(string name)
        {
            return map.Where(m => m.name == name).Select(m => m.value);
        }


        public IEnumerable<string> GetInfoValue(string name)
        {
            return info.Properties().Where(m => m.Name == name && m.Value.Type == JTokenType.String).Select(m => (string)m.Value);
        }
    }

    [JsonConverter(typeof(MyCustomClassConverter))]
    public class NameDatas : List<NameData>
    {
        internal class MyCustomClassConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jObject = JObject.Load(reader);

                var toReturn = new NameDatas();

                foreach (var prop in jObject)
                {
                    toReturn.Add(new NameData { name = prop.Key, value = prop.Value.ToObject<NameValue>() });
                }

                return toReturn;
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(NameData).IsAssignableFrom(objectType);
            }
        }
    }

    public class NameData
    {
        [JsonProperty("_id")]
        public string name { get; set; }
        public NameValue value { get; set; }
    }


}