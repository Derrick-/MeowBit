using System.Collections.Generic;
using System.Web.Http;

namespace dotBitNS.UI.ApiControllers
{
    public class MonitorController : ApiController
    {
        // GET api/Monitor 
        // GET api/Monitor/5 
        public dynamic Get(int? id = null)
        {
            return new { Nmc = Monitor.NameCoinOnline, Ns = Monitor.NameServerOnline };
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
