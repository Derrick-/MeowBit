// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using ARSoft.Tools.Net.Dns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace dotBitNs.Server
{
    class NameServer
    {
        public const int nsTimeout = 5000;
        private static IPAddress defaultDnsServer = new IPAddress(new byte[] { 8, 8, 8, 8 });
        
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
                ConsoleUtils.WriteWarning("Unable to start nameserver, port 53 may be in use. " + ex.Message);
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
            ConsoleUtils.WriteWarning("Nameserver threw error: " + e.Exception.Message);
        }

        // TODO: Instances may need to be pooled
        static DotBitResolver ResolverInstance = new DotBitResolver(DnsResolve);

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

        internal static DnsMessage DnsResolve(List<IPAddress> nameservers, string name, RecordType recordType, RecordClass recordClass)
        {
            var client = GetClientForServers(nameservers);
            DnsMessage answer = client.Resolve(name, recordType, recordClass);
            return answer;
        }

        internal static DnsMessage DnsResolve(string name, RecordType recordType, RecordClass recordClass)
        {
            DnsClient dnsClient;
            if (WindowsNameServicesManager.NameServerHookMethod != WindowsNameServicesManager.NameServerHookMethodType.ChangeNS)
                dnsClient = DnsClient.Default;
            else
            {
                dnsClient = GetNonDefaultDnsClient();
            }
            return dnsClient.Resolve(name, recordType, recordClass);
        }

        private static DnsClient GetNonDefaultDnsClient()
        {
            var servers = WindowsNameServicesManager.GetCachedDnsServers();
            if (!servers.Any())
                servers.Add(defaultDnsServer);
            var client = GetClientForServers(servers);
            return client;
        }

        private static DnsClient GetClientForServers(List<IPAddress> servers)
        {
            var client = new DnsClient(servers, nsTimeout);
            return client;
        }
    }
}
