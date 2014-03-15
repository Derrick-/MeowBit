// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 13, 2014

using System;
namespace dotBitNs
{
    public class ApiMonitorResponse
    {
        public bool Nmc { get; set; }
        public bool Ns { get; set; }
        public bool CacheHook { get; set; }
        public DateTime? LastBlockTime { get; set; }
        public bool? Logging { get; set; }
        public string LogFolder { get; set; }
        public string Version { get; set; }
    }

    public class NmcConfigJson
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public string Port { get; set; }
        public string Logging { get; set; }
    }

}
