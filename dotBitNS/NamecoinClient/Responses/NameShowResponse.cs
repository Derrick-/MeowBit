using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Net;
using dotBitNS;
using dotBitNS.Models;

namespace NamecoinLib.Responses
{
    public class NameShowResponse
    {
        public string name { get; set; }
        public string value { get; set; }
        public string txid { get; set; }
        public string address { get; set; }
        public Int32 expires_in { get; set; }

        public T GetValue<T>() where T : BaseNameValue
        {
            using (var sr = new StringReader(value))
            using (var reader = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create();
                try
                {
                    return BaseNameValue.Instantiate<T>(value);
                }
            // TODO: Constructor should not throw exceptions
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
}