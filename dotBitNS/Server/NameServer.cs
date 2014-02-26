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

        class Resolver
        {
            const int maxRecursion = 5;
            private int recursionLevel = 0;

            private object lockResolver = new object();

            public DnsMessage GetAnswer(DnsQuestion question)
            {
                lock (lockResolver)
                    return InternalGetAnswer(question);
            }

            private DnsMessage InternalGetAnswer(DnsQuestion question)
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

                    DnsMessage answer = null;

                    if (question.Name.EndsWith(".bit"))
                    {
                        answer = ResolveDotBitAddress(question);
                    }

                    if (answer == null)
                    {
                        // send query to upstream server
                        answer = DnsClient.Default.Resolve(question.Name, question.RecordType, question.RecordClass);
                    }

                    return answer;
                }
                finally
                {
                    recursionLevel--;
                }
            }

            private DnsMessage ResolveDotBitAddress(DnsQuestion question)
            {
                DnsMessage answer = null;

                var info = NmcClient.Instance.LookupHost(question.Name);
                if (info != null)
                {
                    answer = new DnsMessage()
                    {
                        Questions = new List<DnsQuestion>() { question }
                    };

                    bool any = question.RecordType == RecordType.Any;

                    if (any || question.RecordType == RecordType.A)
                    {
                        // TODO: Make real and complete responses
                        IPAddress address;
                        var value = info.GetValue();

                        if (IPAddress.TryParse(value.ip, out address))
                            answer.AnswerRecords.Add(new ARecord(question.Name, 60, address));
                        else if (value.ns != null && value.ns.Length > 0)
                        {
                            List<IPAddress> nameservers = GetDotBitNameservers(value);
                            if (nameservers.Count() > 0)
                            {
                                var client = new DnsClient(nameservers, 2000);
                                string name;
                                if (!string.IsNullOrWhiteSpace(value.translate))
                                    name = value.translate;
                                else
                                    name = question.Name;
                                answer = client.Resolve(name, question.RecordType, question.RecordClass);
                            }
                        }
                    }
                }
                return answer;
            }

            private List<IPAddress> GetDotBitNameservers(NameValue value)
            {
                List<IPAddress> nameservers = new List<IPAddress>(value.ns.Length);
                foreach (var ns in value.ns)
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
