// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dotBitNs_Monitor
{
    class NmcConfigSettings
    {
        static string AppDataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); } }
        static string NmcConfigFileName { get { return "namecoin.conf"; } }
        static string DefaultConfigFilePath { get { return Path.Combine(Path.Combine(AppDataPath, "Namecoin"), NmcConfigFileName); } }

        public static string RpcUser { get; set; }
        public static string RpcPass { get; set; }
        public static string RpcPort { get; set; }

        public static void ValidateNmcConfig()
        {
            ConfigFile.InvokeNameCoinConfigInfo("Checking Namecoin Configuration...");

            bool Ok = false;

            var customDataPath = ConfigFile.FindDataPathFromRunningWallet("namecoin-qt");
            if (customDataPath != null)
                Ok = CheckRPCConfig(Path.Combine(customDataPath, NmcConfigFileName));

            Ok = CheckRPCConfig(DefaultConfigFilePath) || Ok;

            if (!Ok)
                ConfigFile.InvokeNameCoinConfigInfo("Namecoin Configuration failed...");
        }

        private static bool CheckRPCConfig(string path)
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
                ConfigFile.InvokeNameCoinConfigInfo(string.Format("Failed to read/verify {0}. {1}", path, ex.Message));
                ok = false;
            }
            return ok;
        }
    }
}
