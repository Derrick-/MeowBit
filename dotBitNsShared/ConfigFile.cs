using dotBitNs.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace dotBitNs
{
    public class ConfigFile
    {
        public static string NmcConfigFileName { get { return "namecoin.conf"; } }
        public static string NmcDataFolder { get { return "Namecoin"; } }

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
            string path = System.IO.Path.GetDirectoryName(this.Path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
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
            
            try
            {
                SaveChanges();
            }
            catch (UnauthorizedAccessException ex)
            {

            }
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
                InvokeNamecoinConfigInfo(string.Format("Creating default config file at {0}", Path));
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
                        InvokeNamecoinConfigInfo(string.Format(" Updating: {0}={1}", key, defaultvalue));
                        data.value = defaultvalue;
                        data.newLine = data.key + '=' + defaultvalue;
                    }
                }
            }
            if (!found)
            {
                InvokeNamecoinConfigInfo(string.Format(" Creating: {0}={1}", key, defaultvalue));
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
                InvokeNamecoinConfigInfo(string.Format("{0} was up to date", Path));
            Read = true;
        }

        private static string[] GetLineParts(string line)
        {
            var parts = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts;
        }

        public string GetSetting(string key)
        {
            if (!Read) ReadFile();

            var data = configData.Where(m => m.key == key).FirstOrDefault();
            if (data == null)
                return null;
            return data.value;
        }

        public delegate void ConfigUpdatedEventHandler();
        public delegate void NamecoinConfigInfoEventHandler(string status, bool important = false);

        public static event ConfigUpdatedEventHandler ConfigUpdated;
        public static event NamecoinConfigInfoEventHandler NamecoinConfigInfo;

        private static void InvokeConfigUpdated()
        {
            if (ConfigUpdated != null)
                ConfigUpdated();
        }

        public static void InvokeNamecoinConfigInfo(string status, bool important = false)
        {
            if (NamecoinConfigInfo != null)
                NamecoinConfigInfo(status, important);
        }

        public static string FindDataPathFromRunningWallet(string ProcessName)
        {
            Debug.WriteLine("Looking for running namecoin wallet...");

            var processes = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            int count = processes.Count();
            if (count <= 0) return null;

            if (count > 1)
                InvokeNamecoinConfigInfo(string.Format("There may be {0} namecoin wallet processes open. Please close {1} and restart service.", count, count - 1), true);

            var process = processes.First();
            string cmdline = ProcessUtils.GetProcessCommandline(process);
            if (cmdline == null)
            {
                InvokeNamecoinConfigInfo("Run Namecoin as current user.", true);
                return null;
            }

            string path = GetCustomDataDirectory(cmdline);
            if (path != null)
                return path;

            path = ProcessUtils.TryFindOwnerAppDataPath(process);
            if (path != null)
                return System.IO.Path.Combine(path, NmcDataFolder);

            return null;
        }

        private static string GetCustomDataDirectory(string cmdline)
        {
            string argName = "-datadir=";

            IEnumerable<string> args;
            try
            {
                args = ProcessUtils.CommandLineToArgs(cmdline);
            }
            catch
            {
                return null;
            }

            foreach (var arg in args)
                if (!string.IsNullOrWhiteSpace(arg) && arg.ToLower().StartsWith(argName) && arg.Length > argName.Length)
                    return arg.Substring(argName.Length);

            return null;
        }

    }
}
