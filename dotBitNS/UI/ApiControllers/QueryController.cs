// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 12, 2014

using System;
using System.Collections.Generic;
using System.Web.Http;

namespace dotBitNs.UI.ApiControllers
{
    public class QueryController : ApiController
    {
        // GET api/NmcQuery/d/meowbit 
        public dynamic Get(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return NmcClient.Instance.LookupNameValue(name);
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
