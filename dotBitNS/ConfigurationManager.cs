using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS
{
    //TODO: remove ConfigurationManager mock
    static class ConfigurationManager
    {
        public static class AppSettings
        {
            public static string Get(string key)
            {
                switch (key)
                {
                    case "DaemonUrl": return string.IsNullOrWhiteSpace(NmcConfig.RpcPort) ? null : "http://127.0.0.1:" + NmcConfig.RpcPort;
                    case "RpcUsername": return NmcConfig.RpcUser;
                    case "RpcPassword": return NmcConfig.RpcPass;
                    case "RpcRequestTimeoutInSeconds": return "1";
                    case "ServiceLogging": return "false";
                }
                return null;
            }
        }

    }

}
