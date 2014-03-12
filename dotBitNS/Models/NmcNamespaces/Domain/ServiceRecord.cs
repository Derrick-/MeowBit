using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotBitNS.Models
{
    internal class ServiceRecord
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
