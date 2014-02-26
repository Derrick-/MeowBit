using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Linq;

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
                return ser.Deserialize<NameValue>(reader);
            }
        }

    }

    // {"translate": "bitcoin.org", "ns": ["1.2.3.4", "1.2.3.5"]} 
    // {"ip": "78.47.86.43", "email": "gtempns1@san.gs", "info": { "status": "On sale." }, "map": {"": "78.47.86.43", "www": "78.47.86.43"}}
    // {"ip": "78.47.86.43", "map": {"": "78.47.86.43", "www": "78.47.86.43"}}

    public class NameValue
    {
        public string ip { get; set; }
        public string email { get; set; }
        public JObject info { get; set; }
        public JObject map { get; set; }
        public string translate { get; set; }
        public string[] ns { get; set; } 

        public IEnumerable<string> Maps
        {
            get
            {
                return map.Properties().Select(m => m.Name);
            }
        }

        public IEnumerable<string> Infos
        {
            get
            {
                return info.Properties().Select(m => m.Name);
            }
        }

        public IEnumerable<string> GetMapIps(string name)
        {
            return map.Properties().Where(m => m.Name == name && m.Value.Type == JTokenType.String).Select(m => (string)m.Value);
        }


        public IEnumerable<string> GetInfoValue(string name)
        {
            return info.Properties().Where(m => m.Name == name && m.Value.Type == JTokenType.String).Select(m => (string)m.Value);
        }
    }
}