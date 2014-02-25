using Newtonsoft.Json;
using System;
using System.IO;

namespace NamecoinLib.Responses
{
    public class NameShowResponse
    {
        public string name { get; set; }
        public string value { get; set; }
        public string txid { get; set; }
        public string address { get; set; }
        public Int32 expires_in { get; set; }

        // TODO clean this up, finish typing the expected response, allow for modification
         
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

    public class NameValue
    {
        public string ip { get; set; }
        public address map { get; set; }

        public class address
        {
            public string ip { get; set; }
        }
    }
}