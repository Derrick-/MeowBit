﻿using ARSoft.Tools.Net.Dns;
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
        int defaultTTL = 60;

        public delegate DnsMessage ExternalResolveHandler(string name, RecordType recordType, RecordClass recordClass);
        
        public delegate NamecoinLib.Responses.NameShowResponse LookupDomainValueRootHandler(string root);

        private readonly ExternalResolveHandler ExternalDnsResolve;
        private readonly INmcClient Client;

        public DotBitResolver(ExternalResolveHandler externalDnsResolver, INmcClient client = null)
        {
            ExternalDnsResolve = externalDnsResolver;
            this.Client = client ?? NmcClient.Instance;
        }

        private object lockResolver = new object();

        public DnsMessage GetAnswer(DnsQuestion question)
        {
            lock (lockResolver)
                return InternalGetAnswer(question);
        }

        private DnsMessage InternalGetAnswer(DnsQuestion question)
        {
            DnsMessage answer = GetDotBitAnswerForName(question);

            if (answer == null)
                answer = ExternalDnsResolve(question.Name, question.RecordType, question.RecordClass);

            return answer;
        }

        const int maxRecursion = 16;
        private int recursionLevel = 0;

        private DnsMessage GetDotBitAnswerForName(DnsQuestion question)
        {
            string fqdn = question.Name;
            try
            {
                recursionLevel++;

                if (recursionLevel > maxRecursion)
                {
                    ConsoleUtils.WriteWarning("Max recursion reached");
                    return null;
                }

                DomainValue value = GetDomainValue(fqdn);
                if (value == null)
                    return null;

                if (!string.IsNullOrWhiteSpace(value.@Delegate))
                    ConsoleUtils.WriteWarning("delegate setting not implemented: {0}", value.@Delegate);

                value.ImportDefaultMap();

                if (!string.IsNullOrWhiteSpace(value.Translate))
                {
                    string newLookup;
                    if (value.Translate.EndsWith("."))
                        newLookup = value.Translate.Substring(0, value.Translate.Length - 1);
                    else
                        newLookup = value.Translate + '.' + fqdn;
                    DnsQuestion newQuestion = new DnsQuestion(newLookup, question.RecordType, question.RecordClass);
                    return InternalGetAnswer(newQuestion);
                }

                DnsMessage answer = null;

                if (value.Alias != null)
                {
                    string newLookup;
                    if (value.Alias == "")
                    {
                        var sa = fqdn.Split('.');
                        newLookup = string.Format("{0}.{1}", sa[sa.Length - 2], sa[sa.Length - 1]);
                    }
                    else if (value.Alias.EndsWith(".")) // absolute
                    {
                        newLookup = value.Alias.Substring(0,value.Alias.Length-1);
                    }
                    else // sub domain
                    {
                        var sa = fqdn.Split('.');
                        sa[0] = value.Alias;
                        newLookup = string.Join(".", sa);
                    }
                    DnsQuestion newQuestion = new DnsQuestion(newLookup, question.RecordType, question.RecordClass);

                    DnsMessage toReturn = new DnsMessage();
                    toReturn.AnswerRecords.Add(new CNameRecord(fqdn, defaultTTL, newLookup));

                    var additional = InternalGetAnswer(newQuestion);
                    toReturn.AdditionalRecords = additional.AnswerRecords;

                    return toReturn;
                }

                answer = new DnsMessage()
                {
                    Questions = new List<DnsQuestion>() { question }
                };

                bool any = question.RecordType == RecordType.Any;

                var nsnames = value.Ns;
                if (nsnames != null && nsnames.Count() > 0) // NS overrides all
                {
                    List<IPAddress> nameservers = GetNameserverAddresses(nsnames);
                    if (nameservers.Count() > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(value.Translate))
                            fqdn = value.Translate;
                        answer = NameServer.DnsResolve(nameservers, fqdn, question.RecordType, question.RecordClass);
                    }
                }
                else
                {
                    if (any || question.RecordType == RecordType.A)
                    {
                        var addresses = value.Ips;
                        if (addresses.Count() > 0)
                            foreach (var address in addresses)
                                answer.AnswerRecords.Add(new ARecord(fqdn, defaultTTL, address));
                    }
                    if (any || question.RecordType == RecordType.Aaaa)
                    {
                        var addresses = value.Ip6s;
                        if (addresses.Count() > 0)
                            foreach (var address in addresses)
                                answer.AnswerRecords.Add(new AaaaRecord(fqdn, defaultTTL, address));
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

            var info = Client.LookupDomainValue(root);

            if (info == null)
                value = null;
            else
            {
                value = info.GetValue<DomainValue>();

                if (value.Import != null && value.Import.Count() > 0)
                {
                    var key = domainparts.Length > 2 ? domainparts[domainparts.Length - 3] : "";
                    foreach (var domain in value.Import.Keys)
                    {
                        if (domain == key)
                        {
                            var name = value.Import[domain];
                            var info2 = Client.LookupNameValue(name);
                            if (info2 != null)
                            {
                                DomainValue import = info2.GetValue<DomainValue>();
                                if (import != null)
                                {
                                    value.ImportValues(import, overwrite: false);
                                    var importMap = import.Maps[domain];
                                    if (importMap != null)
                                    {
                                        var valueMap = value.GetMap(domain);
                                        if (valueMap == null)
                                            value.Maps[domain] = importMap;
                                        else
                                            valueMap.ImportValues(importMap, overwrite: false);
                                    }
                                }
                            }
                        }
                    }
                }


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

        private List<IPAddress> GetNameserverAddresses(IEnumerable<string> nsnames)
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
