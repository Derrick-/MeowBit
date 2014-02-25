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
    class NmcClient
    {
        static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5.0);

        public delegate void OnAvailableChangedHandler(NmcClient source, bool available);
        public static event OnAvailableChangedHandler OnAvailableChanged;

        public static NmcClient Instance { get; private set; }

        [CallPriority(MemberPriority.Highest)]
        public static void Initialize()
        {
            Console.WriteLine("Initializing NmcClient...");
            Instance = new NmcClient();
        }

        public bool Available { get; private set; }

        public NmcClient()
        {
            CheckConnection();
           // Timer.DelayCall(CheckInterval, CheckInterval, new TimerCallback(CheckConnection));
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
        public NameShowResponse LookupHost(string domainname)
        {
            if (domainname.EndsWith(".bit") && domainname.Length > 4)
            {
                string name = "d/" + domainname.Remove(domainname.Length - 4);

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
                Console.Write("Making RPC Connection: ");
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
                InvokeOnAvailableChanged();
            }

            return result;
        }

        private void InvokeOnAvailableChanged()
        {
            if (OnAvailableChanged != null)
                OnAvailableChanged(this, Available);
        }
    }
}