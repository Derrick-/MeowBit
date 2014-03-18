// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static bool CheckRPCConfig(string path)
        {
            bool ok = true;
            try
            {
                RpcPass = "myPreexistingPassword";

                var config = new ConfigFile(GetNmcConfigFilePath(path));

                config.AddMinimumConfigValues(RpcUser, RpcPass, RpcPort);

                RpcUser = config.GetSetting("rpcuser");
                RpcPass = config.GetSetting("rpcpassword");
                RpcPort = config.GetSetting("rpcport");
            }
            catch (IOException ex)
            {
                InvokeNameCoinConfigInfo(string.Format("Failed to read/verify {0}. {1}", GetNmcConfigFilePath(path), ex.Message));
                ok = false;
            }
            return ok;
        }

        class ConfigFile
        {
            private readonly string DefaultUser;
            private readonly string DefaultPass;
            private readonly string DefaultPort;

            public string Path { get; private set; }

            public bool Read { get; private set; }

            public ConfigFile(string path)
            {
                Path = path;
                Read = false;
            }

            public bool Exists
            {
                get { return File.Exists(Path); }
            }

            private void EnsureFolderExists()
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(this.Path)))
                    Directory.CreateDirectory(Path);
            }

            class configLine
            {
                public string key;
                public string value;
                public string originalLine;
                public string newLine;
            }

            List<configLine> configData = new List<configLine>();

            public void AddMinimumConfigValues(string forceUser = null, string forcePass = null, string forcePort = null)
            {
                EnsureFolderExists();
                
                ReadFile();

                EnsureSetting("rpcuser", forceUser ?? "dotBitNS", forceUser != null);
                EnsureSetting("rpcpassword", forcePass ?? dotBitNs.StringUtils.SecureRandomString(16), forcePass != null);
                EnsureSetting("rpcport", forcePort ?? "8336", forcePort != null);
                EnsureSetting("server", "1", true);
                EnsureSetting("rpcallowip", "127.0.0.1", true);

                SaveChanges();
            }

            private void ReadFile()
            {
                configData.Clear();

                if (File.Exists(Path))
                {
                    List<string> fileLines = new List<string>();

                    using (var fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                            fileLines.Add(sr.ReadLine());
                        sr.Close();
                    }

                    foreach (string line in fileLines)
                    {
                        configLine data = new configLine();
                        data.originalLine = line;

                        var parts = GetLineParts(line);
                        if (parts != null && parts.Length > 0)
                        {
                            data.key = parts[0].ToLower();
                            data.value = parts.Length > 0 ? parts.Length == 1 ? parts[1] : string.Join(" ", parts.Skip(1)) : null;
                        }
                        configData.Add(data);
                    }
                }
                else
                {
                    InvokeNameCoinConfigInfo(string.Format("Creating default config file at {0}", Path));
                }
                Read = true;
            }

            private string PrintFormatKey(string value)
            {
                return value ?? "<NULL>";
            }

            private void EnsureSetting(string key, string defaultvalue, bool forceDefaultValue)
            {
                key = key.ToLower();
                bool found = false;
                foreach (var data in configData.ToList())
                {
                    if (string.IsNullOrWhiteSpace(data.key) || data.key.StartsWith("#"))
                        continue;

                    if (data.key.ToLower() == key)
                    {
                        if (found)
                        {
                            Debug.WriteLine("Duplicate Namecoin key {0}={1}", key, PrintFormatKey(data.value));
                            data.newLine = "# MeowBit Removed Duplicate: " + data.originalLine;
                            continue;
                        }

                        found = true;
                        if (forceDefaultValue || string.IsNullOrWhiteSpace(data.value))
                        {
                            InvokeNameCoinConfigInfo(string.Format(" Updating: {0}={1}", key, defaultvalue));
                            data.value = defaultvalue;
                            data.newLine = data.key + '=' + defaultvalue;
                        }
                    }
                }
                if (!found)
                {
                    InvokeNameCoinConfigInfo(string.Format(" Creating: {0}={1}", key, defaultvalue));
                    configData.Add(new configLine()
                    {
                        key = key,
                        value = defaultvalue,
                        originalLine = null,
                        newLine = key + '=' + defaultvalue
                    });
                }
            }

            private void SaveChanges()
            {
                if (configData.Any(m => m.newLine != null && m.newLine != m.originalLine))
                {
                    using (var sw = new StreamWriter(Path, false))
                    {
                        foreach (var data in configData)
                        {
                            sw.WriteLine(data.newLine ?? data.originalLine ?? string.Empty);
                        }
                    }
                    InvokeConfigUpdated();
                }
                else
                    InvokeNameCoinConfigInfo(string.Format("{0} was up to date", Path));
                Read = true;
            }

            private static string[] GetLineParts(string line)
            {
                var parts = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return parts;
            }


            internal string GetSetting(string key)
            {
                var data = configData.Where(m => m.key == key).FirstOrDefault();
                if (data == null)
                    return null;
                return data.value;
            }
        }

    }
}
