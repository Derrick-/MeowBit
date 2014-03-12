// Derrick Slopey | derrick@alienseed.com
// March 11 2014
// ProductValue object

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS.Models
{
    internal class ProductValue : BaseNameValue
    {
        public ProductValue(string json) : base(json) { }

        private string _Name = null;
        private IEnumerable<string> _URLs = null;
        private string _Signer = null;
        private string _Producer = null;
        private string _Author = null;
        private string _Version = null;

        public string Name
        { get { return _Name ?? (_Name = GetString("name")); } }

        public IEnumerable<string> URLs
        { get { return _URLs ?? (_URLs = GetStringList("url")); } }

        public string Signer
        { get { return _Signer ?? (_Signer = GetString("signer")); } }

        public string Producer
        { get { return _Producer ?? (_Producer = GetString("producer")); } }

        public string Author
        { get { return _Author ?? (_Author = GetString("author")); } }

        public string Version
        { get { return _Version ?? (_Version = GetString("version")); } }
    }
}
