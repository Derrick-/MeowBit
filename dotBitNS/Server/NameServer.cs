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
                        answer = DnsClient.Default.Resolve(question.Name, question.RecordType, question.RecordClass);
                    }

                    return answer;
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

                    var info = NmcClient.Instance.LookupRootName(domainparts[domainparts.Length-2]);
                    DnsMessage answer = null;
                    if (info != null)
                    {

                        var value = info.GetValue();

                        if (!string.IsNullOrWhiteSpace(value.@delegate))
                        {
                            return GetDotBitAnswerForName(question, value.@delegate);
                        }

                        //TODO: import not implemented
                        if (!string.IsNullOrWhiteSpace(value.import))
                            using (new ConsoleUtils.Warning())
                                Console.WriteLine("import setting not implemented: {0}", value.import);

                        if (domainparts.Length > 2) // sub-domain
                        {
                            for (int i = domainparts.Length - 2; i <=0 ; i--)
                            {
                                var sub = value.GetMapValue(domainparts[i]).FirstOrDefault();
                                if(sub!=null)
                                    //TODO: this isn't working at all and is probably misguided, finish it
                                    CopyProps(value,sub);
                            }

                        }

                        if (value.alias!=null)
                        {
                            string newLookup;
                            if(value.alias.EndsWith(".")) // absolute
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

                        if (value.ns != null && value.ns.Length > 0) // NS overrides all
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
                    }
                    return answer;
                }
                finally
                {
                    recursionLevel--;
                }
            }

            private void CopyProps(NameValue value, NameValue sub)
            {
        //public dynamic ip { get; set; }
        //public dynamic ip6 { get; set; }
        //public string email { get; set; }
        //public JObject info { get; set; }
        //public string alias { get; set; }
        //public string[] ns { get; set; }

                if (sub.@delegate != null)
                    value.@delegate = sub.@delegate;
                if (sub.ip != null)
                    value.ip = sub.ip;
                if (sub.email != null)
                    value.email = sub.email;
                if (sub.info != null)
                    value.info = sub.info;
                if (sub.alias != null)
                    value.alias = sub.alias;
                if (sub.ns != null)
                    value.ns = sub.ns;
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
