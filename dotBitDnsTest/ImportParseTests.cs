using dotBitNs.Models;
using dotBitNs.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace dotBitDnsTest
{

    [TestClass]
    public class ImportParseTests
    {
        [TestMethod]
        public void SimpleImport()
        {
            string json = @"{""import"": ""s/derrick""}";

            DomainValue value = new DomainValue(json);

            var imports = value.Import;

            Assert.AreEqual(1, imports.Count());
            Assert.AreEqual("s/derrick", imports[""]);

        }

        [TestMethod]
        public void SimpleArrayDefaultImport()
        {
            string json = @"{""import"": [""s/derrick"",""""]}";

            DomainValue value = new DomainValue(json);

            var imports = value.Import;

            Assert.AreEqual(1, imports.Count());
            Assert.AreEqual("s/derrick", imports[""]);

        }
      
        [TestMethod]
        public void SimpleArrayNamedImport()
        {
            string json = @"{""import"": [""s/derrick_www"",""www""]}";

            DomainValue value = new DomainValue(json);

            var imports = value.Import;
            Assert.AreEqual("s/derrick_www", imports["www"]);

        }


        [TestMethod]
        public void ArrayImport()
        {
            string json = @"{""import"": [[""s/derrick"",""""],[""s/derrick_www"",""www""]]}";

            DomainValue value = new DomainValue(json);

            var imports = value.Import;

            Assert.AreEqual(2, imports.Count());
            Assert.AreEqual("s/derrick", imports[""]);
            Assert.AreEqual("s/derrick_www", imports["www"]);
        }

        [TestMethod]
        public void ArrayImportWithDefault()
        {
            string json = @"{""import"": [[""s/derrick""],[""s/derrick_www"",""www""]]}";
           
            DomainValue value = new DomainValue(json);

            var imports = value.Import;

            Assert.AreEqual(2, imports.Count());
            Assert.AreEqual("s/derrick", imports[""]);
            Assert.AreEqual("s/derrick_www", imports["www"]);
        }
    }
}
