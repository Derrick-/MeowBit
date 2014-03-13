// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

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


namespace dotBitNs
{
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

        public GetBlockResponse GetLastBlock()
        {
            GetBlockResponse block=null;
            var id = GetBlockCount();
            if (id.HasValue)
            {
                var hash = GetBlockHash(id.Value);
                if (hash != null)
                    block = GetBlockInfo(hash);
            }
            return block;
        }

        public long? GetBlockCount()
        {
            return MakeRequest<long?>(RpcMethods.getblockcount);
        }

        public string GetBlockHash(long blockid)
        {
            return MakeRequest<string>(RpcMethods.getblockhash, blockid);
        }

        public GetBlockResponse GetBlockInfo(string hash)
        {
            return MakeRequest<GetBlockResponse>(RpcMethods.getblock, hash);
        }

        public NameShowResponse LookupDomainValue(string root)
        {
            return LookupNameValueInNamespace("d/", root);
        }

        public NameShowResponse LookupProductValue(string root)
        {
            return LookupNameValueInNamespace("p/", root);
        }

        private NameShowResponse LookupNameValueInNamespace(string namespacePrexix, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string fullNamePath = namespacePrexix + name;
                return LookupNameValue(fullNamePath);
            }
            return null;
        }

        private object lockLookup = new object();
        public NameShowResponse LookupNameValue(string fullNamePath)
        {
            NameShowResponse info;
            lock (lockLookup)
            {
                info = MakeRequest<NameShowResponse>(RpcMethods.name_show, fullNamePath);
            }
            return info;
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
                    Console.WriteLine("NMC Connector is not configured");
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