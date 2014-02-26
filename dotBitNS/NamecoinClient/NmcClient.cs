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
                    case "DaemonUrl": return "http://127.0.0.1:" + NmcConfig.RpcPort ;
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
        public static bool Ok { get { return Instance.Available; } }

        static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5.0);

        public static NmcClient Instance { get; private set; }

        [CallPriority(MemberPriority.AboveNormal)]
        public static void Initialize()
        {
            Console.WriteLine("Initializing NmcClient...");
            Instance = new NmcClient();
        }

        public bool Available { get; private set; }

        public NmcClient()
        {
            CheckConnection();
            Timer.DelayCall(CheckInterval, CheckInterval, new TimerCallback(CheckConnection));
        }

        void CheckConnection()
        {
            var info = GetInfo();
            if(info!=null)
            Debug.WriteLine("Success: Wallet version {0}", info.Version);
        }

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
                //Console.Write("Making RPC Connection: ");
                result = _rpcConnector.MakeRequest<T>(method, parameters);
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
                    Console.WriteLine("An RPC Error Occurred: {0}", ex.Message);
                    ok = true;
                }
            }

            if (ok != Available)
            {
                Console.WriteLine("Namecoin client is now {0}.", ok ? "online" : "offline");
                Available = ok;
                EventSink.InvokeNameServerAvailableChanged(this, new NameServerAvailableChangedEventArgs(Available));
            }

            return result;
        }
    }
}