using NamecoinLib.ExceptionHandling;
using NamecoinLib.Requests;
using NamecoinLib.Responses;
using NamecoinLib.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
                    case "UseTestNet": return "false";
                    case "DaemonUrl": return string.IsNullOrWhiteSpace(NmcConfig.RpcPort) ? null : "http://127.0.0.1:" + NmcConfig.RpcPort ;
                    case "RpcUsername": return NmcConfig.RpcUser;
                    case "RpcPassword": return NmcConfig.RpcPass;
                    case "RpcRequestTimeoutInSeconds": return "1";
                }
                return null;
            }
        }

    }

    class NmcClient
    {

        private static NmcClient _Instance=null;
        public static NmcClient Instance
        {
            get { return NmcClient._Instance ?? (NmcClient._Instance=new NmcClient()); }
            private set { NmcClient._Instance = value; }
        }

        [CallPriority(MemberPriority.AboveNormal)]
        public static void Initialize()
        {
            Console.WriteLine("Initializing NmcClient...");
        }

        public bool Available { get; private set; }

        public GetInfoResponse GetInfo()
        {
            return MakeRequest<GetInfoResponse>(RpcMethods.getinfo);
        }

        private object lockLookup = new object();
        public NameShowResponse LookupRootName(string root)
        {
            if (!string.IsNullOrWhiteSpace(root))
            {
                string name = "d/" + root;

                NameShowResponse info;
                lock (lockLookup)
                {
                    info = MakeRequest<NameShowResponse>(RpcMethods.name_show, name);
                }
                return info;
            }
            return null;
        }

        private readonly IRpcConnector _rpcConnector = new RpcConnector();

        T MakeRequest<T>(RpcMethods method, params object[] parameters)
        {
            bool ok;
            T result = default(T);
            try
            {
                result = _rpcConnector.MakeRequest<T>(method, parameters);
                if (result == null)
                {
                    Console.WriteLine("NMC Connector is misconfigured");
                    ok = false;
                }
                else
                    ok = true;
            }
            catch (RpcException ex)
            {
                if ((ex.InnerException is System.IO.IOException) || (ex.InnerException is WebException && ((WebException)ex.InnerException).Response==null))
                {
                    Console.WriteLine("Unable to connect to Namecoin client: {0}", ex.Message);
                    ok = false;
                }
                else
                {
                    Console.WriteLine("An RPC Error Occurred: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    ok = false;
                }
            }

            if (ok != Available)
            {
                Available = ok;
                EventSink.InvokeNameServerAvailableChanged(this, new NameServerAvailableChangedEventArgs(Available));
            }

            return result;
        }
    }
}