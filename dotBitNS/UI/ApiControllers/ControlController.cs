using System.Collections.Generic;
using System.Web.Http;

namespace dotBitNS.UI.ApiControllers
{
    public class NmcConfigJson
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public string Port { get; set; }
    }

    public class ControlController : ApiController
    {
        // GET api/Control 
        // GET api/Control/5 
        public dynamic Get(int? id = null)
        {
            return null;
        }

        // POST api/Control 
        public dynamic Post(NmcConfigJson value)
        {
            if (!string.IsNullOrWhiteSpace(value.User)) NmcConfig.RpcUser = value.User;
            if (!string.IsNullOrWhiteSpace(value.Pass)) NmcConfig.RpcPass = value.Pass;
            if (!string.IsNullOrWhiteSpace(value.Port)) NmcConfig.RpcPort = value.Port;
            return new { status = "ok" };
        }

        //// PUT api/Control/{value}
        //public void Put(string id, NmcConfigJson value)
        //{
        //        if (!string.IsNullOrWhiteSpace(value.User)) NmcConfig.RpcUser = value.User;
        //        if (!string.IsNullOrWhiteSpace(value.Pass)) NmcConfig.RpcPass = value.Pass;
        //        if (!string.IsNullOrWhiteSpace(value.Port)) NmcConfig.RpcPort = value.Port;
        //}

        // DELETE api/Control/5 
        public void Delete(int id)
        {
        }
    }

}
