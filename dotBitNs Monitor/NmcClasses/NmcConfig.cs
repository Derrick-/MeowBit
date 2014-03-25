// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs;
using System;
using System.IO;

namespace dotBitNs_Monitor
{
    class NmcConfigSettings
    {
        static string AppDataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); } }
        static string DefaultConfigFilePath { get { return Path.Combine(Path.Combine(AppDataPath, ConfigFile.NmcDataFolder), ConfigFile.NmcConfigFileName); } }

        public static string RpcUser { get; set; }
        public static string RpcPass { get; set; }
        public static string RpcPort { get; set; }

        public static void ValidateNmcConfig()
        {
            ConfigFile.InvokeNamecoinConfigInfo("Checking Namecoin Configuration...");

            bool Ok = false;

            var customDataPath = ConfigFile.FindDataPathFromRunningWallet("namecoin-qt");
            if (customDataPath != null)
                Ok = ConfigureAtPath(Path.Combine(customDataPath, ConfigFile.NmcConfigFileName));

            Ok = ConfigureAtPath(DefaultConfigFilePath) || Ok;

            if (!Ok)
                ConfigFile.InvokeNamecoinConfigInfo("Namecoin Configuration failed...");
        }

        private static bool ConfigureAtPath(string path)
        {
            bool ok = true;
            try
            {
                var config = new ConfigFile(path);

                config.AddMinimumConfigValues(RpcUser, RpcPass, RpcPort);
                if (ok = config.Read)
                {
                    RpcUser = config.GetSetting("rpcuser");
                    RpcPass = config.GetSetting("rpcpassword");
                    RpcPort = config.GetSetting("rpcport");
                }
            }
            catch (IOException ex)
            {
                ConfigFile.InvokeNamecoinConfigInfo(string.Format("Failed to read/verify {0}. {1}", path, ex.Message));
                ok = false;
            }
            return ok;
        }
    }
}
