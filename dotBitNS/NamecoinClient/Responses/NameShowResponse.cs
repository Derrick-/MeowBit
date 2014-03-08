using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
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
                }
                catch (JsonReaderException ex)
                {
                    ConsoleUtils.WriteWarning(ex.Message);
                }
                ConsoleUtils.WriteWarning(" - Json: {0}", value);
                return null;
            }
        }

    }

    public class NameValue
    {
        public dynamic ip { get; set; }
        public dynamic ip6 { get; set; }
        public string email { get; set; }
        public dynamic info { get; set; }
        public NameDatas map { get; set; }
        public string @delegate { get; set; }
        public string import { get; set; }
        public string alias { get; set; }
        public string translate { get; set; }
        public dynamic ns { get; set; }

        public IEnumerable<IPAddress> GetIp4Addresses()
        {
            return ParseIPAddresses(ip);
        }

        public IEnumerable<IPAddress> GetIp6Addresses()
        {
            return ParseIPAddresses(ip6);
        }

        public IEnumerable<string> GetNsNames()
        {
            IEnumerable<string> toReturn=null;
            if (ns != null)
            {
                if (ns is IEnumerable<string>)
                    toReturn = ns.Cast<string>();
                else if (ns is JArray)
                    toReturn = ((JArray)ns).Select(m => (string)m);
                else if (ns is string)
                    toReturn = new string[] { (string)ip };
            }
            return toReturn;
        }

        private IEnumerable<IPAddress> ParseIPAddresses(dynamic addresses)
        {
            List<IPAddress> toReturn = new List<IPAddress>();
            IEnumerable<string> ips;
            if (addresses != null)
            {
                if (addresses is IEnumerable<string>)
                    ips = addresses.Cast<string>();
                else if (addresses is JArray)
                    ips = ((JArray)addresses).Select(m => (string)m);
                else if (addresses is string)
                    ips = new string[] { (string)ip };
                else
                    return toReturn;

                IPAddress addr;
                foreach (var item in ips)
                    if (IPAddress.TryParse(item, out addr))
                        toReturn.Add(addr);
            }
            return toReturn;
        }

        static class Lists<T>
        {
            public static readonly List<T> Empty = new List<T>();
        }

        public IEnumerable<string> Maps
        {
            get
            {
                if (map == null) return Lists<string>.Empty;
                return map.Select(m => m.name);
            }
        }

        //public IEnumerable<string> Infos
        //{
        //    get
        //    {
        //        if (info == null) return Lists<string>.Empty;
        //        return info.Properties().Select(m => m.Name);
        //    }
        //}

        public IEnumerable<NameValue> GetMapValue(string name)
        {
            if (map == null) return Lists<NameValue>.Empty;
            return map.Where(m => m.name == name).Select(m => m.value);
        }


        //public IEnumerable<string> GetInfoValue(string name)
        //{
        //    if (info == null) return Lists<string>.Empty;
        //    return info.Properties().Where(m => m.Name == name && m.Value.Type == JTokenType.String).Select(m => (string)m.Value);
        //}
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
                    try
                    {
                        toReturn.Add(new NameData { name = prop.Key, value = prop.Value.ToObject<NameValue>() });
                    }
                    catch (JsonSerializationException)
                    {
                        ConsoleUtils.WriteWarning(" - ERR: \"{0}\":\"{1}\"", prop.Key, prop.Value);
                        return null;
                    }
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