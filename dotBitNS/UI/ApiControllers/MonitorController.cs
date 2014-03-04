// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Web.Http;

namespace dotBitNS.UI.ApiControllers
{
    class ApiMonitorResponse
    {
        public bool Nmc { get; set; }
        public bool Ns { get; set; }
        public bool CacheHook { get; set; }
        public DateTime? LastBlockTime { get; set; }
    }

    public class MonitorController : ApiController
    {
        // GET api/Monitor 
        // GET api/Monitor/5 
        public dynamic Get(int? id = null)
        {
            return new ApiMonitorResponse() 
            { 
                Nmc = Monitor.NameCoinOnline, 
                Ns = Monitor.NameServerOnline, 
                CacheHook = WindowsNameServicesManager.AnyDotBitCacheConfigKeys(),
                LastBlockTime = Monitor.LastBlockTimeGMT
            };
        }

        // POST api/Monitor 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/Monitor/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/Monitor/5 
        public void Delete(int id)
        {
        }
    }

}
