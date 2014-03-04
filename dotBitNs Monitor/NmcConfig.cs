// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNs_Monitor
{
    class NmcConfigSettings
    {
        public static string RpcUser { get; set; }
        public static string RpcPass { get; set; }
        public static string RpcPort { get; set; }

        public static bool Ok { get; set; }

        public delegate void ConfigUpdatedEventHandler();
        public delegate void NameCoinConfigInfoEventHandler(string status);

        public static event ConfigUpdatedEventHandler ConfigUpdated;
        public static event NameCoinConfigInfoEventHandler NameCoinConfigInfo;

        private static void InvokeConfigUpdated()
        {
            if (ConfigUpdated != null)
                ConfigUpdated();
        }

        private static void InvokeNameCoinConfigInfo(string status)
        {
            if (NameCoinConfigInfo != null)
                NameCoinConfigInfo(status);
        }

        public static void ValidateNmcConfig()
        {
            InvokeNameCoinConfigInfo("Checking Namecoin Configuration...");

            Ok = CheckRPCConfig(AppDataPath);
        }

        static string AppDataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); } }
        static string NmcConfigFileName { get { return "namecoin.conf"; } }

        static string GetNmcConfigPath(string AppDataPath) { return Path.Combine(AppDataPath, "Namecoin"); }
        static string GetNmcConfigFilePath(string AppDataPath) { return Path.Combine(GetNmcConfigPath(AppDataPath), NmcConfigFileName); }

        private static bool CheckRPCConfig(string AppDataPath)
        {
            bool ok = true;
            try
            {
                var config = new ConfigFile(GetNmcConfigFilePath(AppDataPath));

                RpcUser = config.Settings["rpcuser"];
                RpcPass = config.Settings["rpcpassword"];
                RpcPort = config.Settings["rpcport"];
            }
            catch (IOException ex)
            {
                InvokeNameCoinConfigInfo(string.Format("Failed to read/verify {0}. {1}", GetNmcConfigFilePath(AppDataPath), ex.Message));
                ok = false;
            }
            return ok;
        }

        class ConfigFile
        {
            public string Path { get; private set; }

            public ConfigFile(string path, bool CreateAndVerify = true)
            {
                Path = path;

                if (CreateAndVerify)
                {
                    EnsureFolderExists(System.IO.Path.GetDirectoryName(path));
                    AddMinimumConfigValues();
                }
            }

            public void EnsureFolderExists(string path)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            public readonly Dictionary<string, string> Settings = new Dictionary<string, string>();
            Dictionary<string, string> toupdate = new Dictionary<string, string>();

            public void AddMinimumConfigValues()
            {
                ReadFile();

                EnsureSetting("rpcuser", "dotBitNS", false);
                EnsureSetting("rpcpassword", dotBitNS.StringUtils.SecureRandomString(16), false);
                EnsureSetting("rpcport", "8336", false);
                EnsureSetting("server", "1", true);
                EnsureSetting("rpcallowip", "127.0.0.1", true);

                SaveChanges();
            }

            private void ReadFile()
            {
                Settings.Clear();
                toupdate.Clear();

                string fileText;


                if (File.Exists(Path))
                {
                    using (var fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        fileText = sr.ReadToEnd();
                        sr.Close();
                    }

                    var fileLines = fileText.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in fileLines)
                    {
                        var parts = GetLineParts(line);
                        if (parts != null && parts.Length > 0)
                            Settings.Add(parts[0].ToLower(), parts.Length > 0 ? parts.Length == 1 ? parts[1] : string.Join(" ", parts.Skip(1)) : null);
                    }
                }
                else
                {
                    InvokeNameCoinConfigInfo(string.Format("Creating default config file at {0}", Path));
                }
            }

            private void EnsureSetting(string key, string defaultvalue, bool forceDefaultValue)
            {
                key = key.ToLower();
                if (!Settings.ContainsKey(key) || (forceDefaultValue && Settings[key] != defaultvalue))
                {
                    InvokeNameCoinConfigInfo(string.Format(" Updating: {0}={1}", key, defaultvalue));
                    toupdate.Add(key, defaultvalue);
                }
            }

            private void SaveChanges()
            {
                if (toupdate.Count > 0)
                {
                    InvokeNameCoinConfigInfo(string.Format("Saving {0}", Path));
                    foreach (string key in toupdate.Keys)
                        Settings[key] = toupdate[key];
                    using (var sw = new StreamWriter(Path, false))
                    {
                        foreach (var pair in Settings)
                            sw.WriteLine("{0}={1}", pair.Key, pair.Value);
                    }
                    InvokeConfigUpdated();
                }
                else
                    InvokeNameCoinConfigInfo(string.Format("{0} was up to date", Path));
            }

            private static string[] GetLineParts(string line)
            {
                var parts = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return parts;
            }

        }

    }
}
