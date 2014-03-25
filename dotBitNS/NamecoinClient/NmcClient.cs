// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using NamecoinLib.ExceptionHandling;
using NamecoinLib.Responses;
using NamecoinLib.RPC;
using System;
using System.Diagnostics;
using System.Net;


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
                    if(ex.InnerException != null)
                    Console.WriteLine(" Inner: {0}", ex.InnerException.Message);
                }
                else
                {
                    WebException webEx = ex.InnerException as WebException;
                    if (webEx != null && webEx.Response is HttpWebResponse && ((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine(" Not authorized to connect to wallet...");
                        TryUpdateApiCreds();
                    }
                    else
                    {
                        Console.WriteLine("An RPC Error Occurred: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }
                }
                ok = false;
            }

            if (ok != Available)
            {
                Available = ok;
                EventSink.InvokeNameServerAvailableChanged(this, new NameServerAvailableChangedEventArgs(Available));
            }

            return result;
        }

        private void TryUpdateApiCreds()
        {
            Debug.WriteLine("Trying to find creds...");
            string path = 
                dotBitNs.ConfigFile.FindDataPathFromRunningWallet("namecoind") ??
                dotBitNs.ConfigFile.FindDataPathFromRunningWallet("namecoin-qt");

            if (path != null)
            {
                Debug.WriteLine(string.Format(" Found running wallet using {0}", path));
                ConfigFile config = new ConfigFile(System.IO.Path.Combine(path,ConfigFile.NmcConfigFileName));

                if (!config.Exists)
                {
                    Debug.WriteLine(" Config does not exist.");
                }
                else
                {
                    try
                    {
                        var user = config.GetSetting("rpcuser");
                        if (config.Read)
                        {
                            var pass = config.GetSetting("rpcpassword");
                            var portstr = config.GetSetting("rpcport");

                            ushort port;

                            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass) && ushort.TryParse(portstr, out port) && port > 0)
                            {
                                Properties.Settings.Default.RpcUsername = user;
                                Properties.Settings.Default.RpcPassword = pass;
                                Properties.Settings.Default.RpcPort = port;

                                Debug.WriteLine(" Read auth from config.");
                            }
                            else
                                Debug.WriteLine(" Config did not contain auth info.");
                        }
                        else
                            Debug.WriteLine(" Could not open file.");
                    }
                    catch (System.IO.IOException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}