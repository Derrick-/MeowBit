using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotBitNS.Models;
using System.Linq;

namespace dotBitDnsTest
{
    [TestClass]
    public class ProductReaderTests
    {
        string Example_V_0_1_Example =
@"{
  ""name"": ""Namecoin"",
  ""url"": [
    ""http://namecoin.info/"",
    ""https://github.com/khalahan/namecoin""
  ],
  ""signer"": ""NDx9uucFbU8a3htyPHa3mRhanjJzNEUXN9"",
  ""producer"": ""id/dotbit"",
  ""author"": ""id/khal"",
  ""version"": ""0.3.72"",
  ""dist"": [
    {
      ""os"": ""w"",
      ""type"": ""i"",
      ""file"": ""namecoin_win_vQ.3.72.zip"",
      ""hash"": {
        ""md5"": ""43281641b85e77c4135bc0d776448547""
      }
    },
    {
      ""type"": ""s"",
      ""file"": ""vQ.3.72.zip"",
      ""hash"": {
        ""md5"": ""19a622d92c04ee492a48c072b02756b5""
      }
    }
  ]
 }";

        [TestMethod]
        public void ReadJson()
        {
            var domain = ProductValue.JsonDeserialize(Example_V_0_1_Example);
            Assert.IsNotNull(domain);
        }


        [TestMethod]
        public void TestMethod1()
        {
            ProductValue value = new ProductValue(Example_V_0_1_Example);

            Assert.AreEqual("Namecoin", value.Name);

            var urls = value.URLs;
            Assert.IsTrue(urls.Contains("http://namecoin.info/"));
            Assert.IsTrue(urls.Contains("https://github.com/khalahan/namecoin"));

            Assert.AreEqual("id/dotbit", value.Producer);
            Assert.AreEqual("id/khal", value.Author);
            Assert.AreEqual("0.3.72", value.Version);
        }
    }
}
