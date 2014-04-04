using ARSoft.Tools.Net.Dns;
using dotBitNs;
using NamecoinLib.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dotBitDnsTest.Mocks
{
    class NmcClientMock : INmcClient
    {

        readonly Dictionary<string, string> Names = new Dictionary<string, string>();
        readonly Dictionary<string, IPAddress> dnsRecords = new Dictionary<string, IPAddress>();

        public void addName(string name, string value)
        {
            Names[name] = value;
        }

        internal void addDnsRecord(string p, System.Net.IPAddress iPAddress)
        {
            dnsRecords[p] = iPAddress;
        }

        public bool Available
        {
            get { return true; }
        }

        public NamecoinLib.Responses.NameShowResponse LookupDomainValue(string root)
        {
            return LookupNameValue("d/" + root);
        }

        public NamecoinLib.Responses.NameShowResponse LookupProductValue(string root)
        {
            return LookupNameValue("p/" + root);
        }
        
        public NamecoinLib.Responses.NameShowResponse LookupNameValue(string fullNamePath)
        {
            if (Names.ContainsKey(fullNamePath))
                return new NameShowResponse()
                {
                    name = fullNamePath,
                    value = Names[fullNamePath],
                };

            throw new ArgumentException(fullNamePath + " is not configured for testing.");
        }

        public long? GetBlockCount()
        {
            throw new NotImplementedException();
        }

        public string GetBlockHash(long blockid)
        {
            throw new NotImplementedException();
        }

        public NamecoinLib.Responses.GetBlockResponse GetBlockInfo(string hash)
        {
            throw new NotImplementedException();
        }

        public NamecoinLib.Responses.GetInfoResponse GetInfo()
        {
            throw new NotImplementedException();
        }

        public NamecoinLib.Responses.GetBlockResponse GetLastBlock()
        {
            throw new NotImplementedException();
        }

        public DnsMessage DnsLookup(string name, RecordType recordType, RecordClass recordClass)
        {
            DnsMessage answer = null;
            bool any = recordType == RecordType.Any;
            if ((any || recordType == RecordType.A) && dnsRecords.ContainsKey(name))
            {
                var ip = dnsRecords[name];
                answer = new DnsMessage();
                answer.AnswerRecords.Add(new ARecord(name, 0, ip));
            }
            return answer;
        }
    }
}
