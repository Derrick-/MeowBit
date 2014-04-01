using ARSoft.Tools.Net.Dns;
using dotBitNs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNs.Server
{
    internal class DotBitResolver
    {
        public delegate DnsMessage ExternalResolveHandler(string name, RecordType recordType, RecordClass recordClass);
        
        public delegate NamecoinLib.Responses.NameShowResponse LookupDomainValueRootHandler(string root);

        private readonly ExternalResolveHandler DnsResolve;
        private readonly LookupDomainValueRootHandler LookupDomainValueRoot;

        public DotBitResolver(ExternalResolveHandler externalDnsResolver, LookupDomainValueRootHandler lookupDomainValueRootHandler = null)
        {
            DnsResolve = externalDnsResolver;
            LookupDomainValueRoot = lookupDomainValueRootHandler ?? NmcClient.Instance.LookupDomainValue;
        }

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
                answer = DnsResolve(question.Name, question.RecordType, question.RecordClass);
            }

            return answer;
        }

        private DnsMessage ResolveDotBitAddress(DnsQuestion question)
        {
            string name = question.Name;
            return GetDotBitAnswerForName(question, name);
        }

        const int maxRecursion = 16;
        private int recursionLevel = 0;

        private DnsMessage GetDotBitAnswerForName(DnsQuestion question, string name)
        {
            try
            {
                recursionLevel++;

                if (recursionLevel > maxRecursion)
                {
                    ConsoleUtils.WriteWarning("Max recursion reached");
                    return null;
                }

                DomainValue value = GetDomainValue(name);
                if (value == null)
                    return null;

                value.ImportDefaultMap();

                DnsMessage answer = null;

                //TODO: delegate not implemented
                if (!string.IsNullOrWhiteSpace(value.@Delegate))
                    ConsoleUtils.WriteWarning("delegate setting not implemented: {0}", value.Import);

                //TODO: import not implemented
                if (!string.IsNullOrWhiteSpace(value.Import))
                    ConsoleUtils.WriteWarning("import setting not implemented: {0}", value.Import);

                if (value.Alias != null)
                {
                    string newLookup;
                    if (value.Alias.EndsWith(".")) // absolute
                    {
                        newLookup = value.Alias;
                    }
                    else // sub domain
                    {
                        newLookup = value.Alias + '.';
                    }
                    DnsQuestion newQuestion = new DnsQuestion(value.Alias, question.RecordType, question.RecordClass);
                    return InternalGetAnswer(newQuestion);
                }

                answer = new DnsMessage()
                {
                    Questions = new List<DnsQuestion>() { question }
                };

                bool any = question.RecordType == RecordType.Any;

                var nsnames = value.Ns;
                if (nsnames != null && nsnames.Count() > 0) // NS overrides all
                {
                    List<IPAddress> nameservers = GetDotBitNameservers(nsnames);
                    if (nameservers.Count() > 0)
                    {
                        var client = new DnsClient(nameservers, 2000);
                        if (!string.IsNullOrWhiteSpace(value.Translate))
                            name = value.Translate;
                        answer = client.Resolve(name, question.RecordType, question.RecordClass);
                    }
                }
                else
                {
                    if (any || question.RecordType == RecordType.A)
                    {
                        var addresses = value.Ips;
                        if (addresses.Count() > 0)
                            foreach (var address in addresses)
                                answer.AnswerRecords.Add(new ARecord(name, 60, address));
                    }
                    if (any || question.RecordType == RecordType.Aaaa)
                    {
                        var addresses = value.Ip6s;
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

        private DomainValue GetDomainValue(string name)
        {
            DomainValue value = null;
            string[] domainparts = name.Split('.');

            if (domainparts.Length < 2 || domainparts[domainparts.Length - 1] != "bit")
                value = null;
            else
                value = GetNameValueForDomainname(domainparts);

            return value;
        }

        private DomainValue GetNameValueForDomainname(string[] domainparts)
        {
            DomainValue value;
            var root = domainparts[domainparts.Length - 2];

            var info = LookupDomainValueRoot(root);
            if (info == null)
                value = null;
            else
            {
                value = info.GetValue<DomainValue>();
                value = ResolveSubdomain(domainparts, value);
            }
            return value;
        }

        internal static DomainValue ResolveSubdomain(string[] domainparts, DomainValue value)
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

                    var sub = value.GetMap(domainparts[i]);
                    if (sub == null)
                        sub = value.GetMap("*");
                    if (sub == null)
                        break;
                    value = sub;
                }

            }
            return value;
        }

        private List<IPAddress> GetDotBitNameservers(IEnumerable<string> nsnames)
        {
            List<IPAddress> nameservers = new List<IPAddress>();
            foreach (var ns in nsnames)
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
