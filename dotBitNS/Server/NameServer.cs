// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using ARSoft.Tools.Net.Dns;
using NamecoinLib.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS.Server
{
    class NameServer
    {
        public static bool Ok { get; set; }
        
        private static DnsServer server;

        [CallPriority(MemberPriority.Normal)]
        public static void Initialize()
        {
            StartServer();
            EventSink.Shutdown += EventSink_Shutdown;
        }

        private static void StartServer()
        {
            Console.Write("Starting Nameserver.... ");
            if (server == null)
            {
                server = new DnsServer(IPAddress.Any, 10, 10, ProcessQuery);
                server.ExceptionThrown += server_ExceptionThrown;
                Ok = false;
            }

            try
            {
                server.Start();
                Console.WriteLine("Done.");
                Ok = true;
            }
            catch (SocketException ex)
            {
                using (new ConsoleUtils.Warning())
                {
                    Console.WriteLine("Unable to start nameserver, port 53 may be in use. " + ex.Message);
                }
                Ok = false;
            }
        }

        static void EventSink_Shutdown(ShutdownEventArgs e)
        {
            if (server != null)
                server.Stop();
        }

        static void server_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            using (new ConsoleUtils.Warning())
            {
                Console.WriteLine("Nameserver threw error: " + e.Exception.Message);
            }
        }

        // TODO: Instances may need to be pooled
        static Resolver ResolverInstance = new Resolver();

        static DnsMessageBase ProcessQuery(DnsMessageBase message, IPAddress clientAddress, ProtocolType protocol)
        {
            Ok = true;

            message.IsQuery = false;

            DnsMessage query = message as DnsMessage;

            if (query != null)
            {
                if (query.Questions.Count == 1)
                {
                    DnsQuestion question = query.Questions[0];
          
                    Console.WriteLine("Query: {0} {1} {2}", question.Name, question.RecordType, question.RecordClass);

                    DnsMessage answer;
                    answer = ResolverInstance.GetAnswer(question);

                    // if got an answer, copy it to the message sent to the client
                    if (answer != null)
                    {
                        foreach (DnsRecordBase record in (answer.AnswerRecords))
                        {
                            Console.WriteLine("Answer: {0}", record);
                            query.AnswerRecords.Add(record);
                        }
                        foreach (DnsRecordBase record in (answer.AdditionalRecords))
                        {
                            Console.WriteLine("Additional Answer: {0}", record);
                            query.AnswerRecords.Add(record);
                        }

                        query.ReturnCode = ReturnCode.NoError;
                        return query;
                    }
                }
                else
                    Debug.WriteLine("Too many questions ({0})", query.Questions.Count);
            }
            // Not a valid query or upstream server did not answer correct
            message.ReturnCode = ReturnCode.ServerFailure;
            return message;
        }

        internal class Resolver
        {
            private object lockResolver = new object();

            public DnsMessage GetAnswer(DnsQuestion question)
            {
                lock (lockResolver)
                    return InternalGetAnswer(question);
            }

            private DnsMessage InternalGetAnswer(DnsQuestion question)
            {
                    DnsMessage answer = ResolveDotBitAddress(question);

                    if (answer == null)
                    {
                        // send query to upstream server
                        DnsClient dnsClient;
                        if (WindowsNameServicesManager.NameServerHookMethod != WindowsNameServicesManager.NameServerHookMethodType.ChangeNS)
                            dnsClient = DnsClient.Default;
                        else
                        {
                            dnsClient = GetNonDefaultDnsClient();
                        }
                        answer = dnsClient.Resolve(question.Name, question.RecordType, question.RecordClass);
                    }

                    return answer;
            }

            private IPAddress defaultDnsServer = new IPAddress(new byte[] { 8, 8, 8, 8 });
            private DnsClient GetNonDefaultDnsClient()
            {
                var servers = WindowsNameServicesManager.GetCachedDnsServers();
                if(!servers.Any())
                    servers.Add(defaultDnsServer);
                var client = new DnsClient(servers, 2000);
                return client;
            }

            private DnsMessage ResolveDotBitAddress(DnsQuestion question)
            {
                string name = question.Name;
                return GetDotBitAnswerForName(question, name);
            }

            const int maxRecursion = 7;
            private int recursionLevel = 0;

            private DnsMessage GetDotBitAnswerForName(DnsQuestion question, string name)
            {
                try
                {
                    recursionLevel++;

                    if (recursionLevel > maxRecursion)
                    {
                        using (new ConsoleUtils.Warning())
                            Console.WriteLine("Max recursion reached");
                        return null;
                    }

                    string[] domainparts = name.Split('.');

                    if (domainparts.Length < 2 || domainparts[domainparts.Length - 1] != "bit")
                        return null;

                    NameValue value = GetNameValueForDomainname(domainparts);

                    if (value == null)
                        return null;

                    DnsMessage answer = null;

                    //TODO: delegate not implemented
                    if (!string.IsNullOrWhiteSpace(value.@delegate))
                        ConsoleUtils.WriteWarning("delegate setting not implemented: {0}", value.import);

                    //TODO: import not implemented
                    if (!string.IsNullOrWhiteSpace(value.import))
                        ConsoleUtils.WriteWarning("import setting not implemented: {0}", value.import);

                    if (value.alias != null)
                    {
                        string newLookup;
                        if (value.alias.EndsWith(".")) // absolute
                        {
                            newLookup = value.alias;
                        }
                        else // sub domain
                        {
                            newLookup = value.alias + '.';
                        }
                        DnsQuestion newQuestion = new DnsQuestion(value.alias, question.RecordType, question.RecordClass);
                        return InternalGetAnswer(newQuestion);
                    }

                    answer = new DnsMessage()
                    {
                        Questions = new List<DnsQuestion>() { question }
                    };

                    bool any = question.RecordType == RecordType.Any;

                    var nsnames = value.GetNsNames();
                    if (nsnames != null && nsnames.Count() > 0) // NS overrides all
                    {
                        List<IPAddress> nameservers = GetDotBitNameservers(value);
                        if (nameservers.Count() > 0)
                        {
                            var client = new DnsClient(nameservers, 2000);
                            if (!string.IsNullOrWhiteSpace(value.translate))
                                name = value.translate;
                            answer = client.Resolve(name, question.RecordType, question.RecordClass);
                        }
                    }
                    else
                    {
                        if (any || question.RecordType == RecordType.A)
                        {
                            var addresses = value.GetIp4Addresses();
                            if (addresses.Count() > 0)
                                foreach (var address in addresses)
                                    answer.AnswerRecords.Add(new ARecord(name, 60, address));
                        }
                        if (any || question.RecordType == RecordType.Aaaa)
                        {
                            var addresses = value.GetIp6Addresses();
                            if (addresses.Count() > 0)
                                foreach (var address in addresses)
                                    answer.AnswerRecords.Add(new AaaaRecord(name, 60, address));
                        }
                    }

                    return answer;
                }
                finally
                {
                    recursionLevel--;
                }
            }

            private static NameValue GetNameValueForDomainname(string[] domainparts)
            {
                NameValue value;
                var info = NmcClient.Instance.LookupRootName(domainparts[domainparts.Length - 2]);
                if (info == null)
                    value = null;
                else
                {
                    value = info.GetValue();
                    value = ResolveSubdomain(domainparts, value);
                }
                return value;
            }

            internal static NameValue ResolveSubdomain(string[] domainparts, NameValue value)
            {
                if (domainparts.Length > 2) // sub-domain
                {
                    for (int i = domainparts.Length - 3; i >= 0; i--)
                    {
                        if (string.IsNullOrWhiteSpace(domainparts[i]))
                        {
                            value = null;
                            break;
                        }

                        var sub = value.GetMapValue(domainparts[i]).FirstOrDefault();
                        if (sub == null)
                            sub = value.GetMapValue("*").FirstOrDefault();
                        if (sub == null)
                            sub = value.GetMapValue("").FirstOrDefault();
                        if (sub == null)
                        {
                            value = null;
                            break;
                        }
                        value = sub;
                    }

                }
                return value;
            }

            private List<IPAddress> GetDotBitNameservers(NameValue value)
            {
                List<IPAddress> nameservers = new List<IPAddress>();
                foreach (var ns in value.GetNsNames())
                {
                    IPAddress ip;
                    if (IPAddress.TryParse(ns, out ip))
                        nameservers.Add(ip);
                    else
                    {
                        var nsQuestion = new DnsQuestion(ns, RecordType.A, RecordClass.Any);
                        var nsAnswer = InternalGetAnswer(nsQuestion);
                        if (nsAnswer != null && nsAnswer.AnswerRecords.Any(m => m.RecordType == RecordType.A && m is ARecord))
                        {
                            IPAddress nsip = nsAnswer.AnswerRecords.Where(m => m.RecordType == RecordType.A).Cast<ARecord>().First().Address;
                            nameservers.Add(nsip);
                        }
                    }
                }
                return nameservers;
            }

        }
    }
}
