// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace dotBitNS
{
    class WindowsNameServicesManager
    {

        public enum OSVersionType
        {
            Unknown,
            MacOSX,
            Unix,
            WinCE,
            XBox,
            Win16Bit,
            Win95,
            Win98,
            WinME,
            WinNT4,
            Win2k,
            WinXP,
            WinXP64,
            WinServer2003,
            WinVista,
            Win7,
            Win8,
            Win8_1
        }

        public enum NameServerHookMethodType
        {
            Unsupported,
            CacheHook,
            ChangeNS,
        }

        public static OSVersionType OSVersion { get; private set; }
        public static NameServerHookMethodType NameServerHookMethod{get;private set;}

        static WindowsNameServicesManager()
        {
            OSVersion = GetWindowsVersion();
            DetermineSupport();
        }

        public WindowsNameServicesManager()
        {
        }

        public void Enable()
        {
            switch (NameServerHookMethod)
            {
                case NameServerHookMethodType.CacheHook:
                    InsertCacheHook(); break;
                case NameServerHookMethodType.ChangeNS:
                    HookDnsServers(); break;
            }
        }

        public void Disable()
        {
            switch (NameServerHookMethod)
            {
                case NameServerHookMethodType.CacheHook:
                    RemoveCacheHook(); break;
                case NameServerHookMethodType.ChangeNS:
                    UnhookDnsServers(); break;
            }
        }


        static Dictionary<Guid, List<string>> OriginalDnsConfigs = new Dictionary<Guid, List<string>>();

        public static List<IPAddress> GetCachedDnsServers()
        {
            var toReturn = new List<IPAddress>();

            foreach (var address in OriginalDnsConfigs.Values.SelectMany(m => m))
            {
                IPAddress ip;
                if (address != localip && IPAddress.TryParse(address, out ip) && !ip.IsIPv6LinkLocal)
                {
                    toReturn.Add(ip);
                }
            }
            return toReturn;
        }

        const string localip = "127.0.0.1";
        private void HookDnsServers()
        {
            try 
            {
                ManagementObjectCollection moc = GetNetworkConfigs();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        string[] originaldns = GetCurrentDnsServers(mo);
                        if (originaldns == null || originaldns.Length != 1 || originaldns[0] != localip)
                        {
                            Console.Write("Found unhooked interface: ");
                            Guid config;
                            if (Guid.TryParse(mo["SettingID"] as string, out config))
                            {
                                Console.WriteLine(config);
                                if (originaldns != null)
                                    OriginalDnsConfigs[config] = originaldns.ToList();
                                string interfacename = config.ToString();
                                string[] newdns = { localip };
                                ReplaceDnsOnInterface(mo, interfacename, newdns);
                            }
                        }
                        else
                        {
                            Console.Write("Found hooked interface: ");
                            Console.Write("DNS {0}", originaldns == null ? "<NULL>" : string.Join(",", originaldns));
                            Console.WriteLine(mo["SettingID"]);
                        }
                        //DumpInterfaceProps(mo);
                    }
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
            }
        }

        private void UnhookDnsServers()
        {
            ManagementObjectCollection moc = GetNetworkConfigs();
            foreach (ManagementObject mo in moc)
            {
                Guid config;
                if (Guid.TryParse(mo["SettingID"] as string, out config))
                {
                    if (OriginalDnsConfigs.ContainsKey(config))
                    {
                        string interfacename = config.ToString();
                        string[] newdns = OriginalDnsConfigs[config].Where(m => m != localip).ToArray();
                        ReplaceDnsOnInterface(mo, interfacename, newdns);
                    }
                }
            }
        }

        private static void ReplaceDnsOnInterface(ManagementObject mo, string interfacename, string[] newdns)
        {
            Console.WriteLine("Replacing DNS on Interface {0},", interfacename);
            string[] originaldns = GetCurrentDnsServers(mo);
            if (originaldns == null)
                Console.WriteLine(", was: <NULL>");
            else
                Console.WriteLine(", was: {0}", string.Join(", ", originaldns));

            ManagementBaseObject objdns = mo.GetMethodParameters("SetDNSServerSearchOrder");
            if (objdns != null)
            {
                objdns["DNSServerSearchOrder"] = newdns;
                mo.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                Console.WriteLine("DNS is set to {0}", string.Join(", ", newdns));
            }
        }

        internal static ManagementObjectCollection GetNetworkConfigs()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            return moc;
        }

        private static string[] GetCurrentDnsServers(ManagementObject mo)
        {
            string[] originaldns = (string[])mo["DNSServerSearchOrder"];
            return originaldns;
        }

        internal static void DumpInterfaceProps(ManagementObject mo)
        {
            foreach (var prop in mo.Properties)
            {
                string val;
                if (prop.Value is string[])
                    val = string.Join(", ", (string[])prop.Value);
                else if (prop.Value != null)
                    val = prop.Value.ToString();
                else
                    val = null;
                Console.WriteLine(string.Format(" {0} : {1}", prop.Name, val));
            }
        }

        const string DnsPolicyCacheConfigName = @"SYSTEM\ControlSet001\Services\Dnscache\Parameters\DnsPolicyConfig\";
        const string configGUID = "{74f2c340-2644-4a72-9d81-a61b8f174d7a}";

        private void InsertCacheHook()
        {
            var key = GetDnsCacheConfigKey(true);
            if (key != null)
            {
                var subs = key.GetSubKeyNames();
                RegistryKey found = null;
                foreach (string name in subs)
                {
                    var configKey = Registry.LocalMachine.OpenSubKey(DnsPolicyCacheConfigName + name);
                    var val = configKey.GetValue("Name", null) as string[];
                    if (name == configGUID)
                    {
                        if (found != null) // duplicate found
                            DeleteConfigKey(key, name);
                        else if (!val.Contains(".bit"))
                        {
                            found = null;
                            DeleteConfigKey(key, name);
                        }
                        else
                            found = configKey;
                    }
                    else if (val.Contains(".bit"))
                    {
                        DeleteConfigKey(key, name);
                    }
                }
                if (key != null && found == null)
                    InstallDnsCacheConfigKeys(key);
            }
            else
            {
                RemoveCacheHook();
                NameServerHookMethod = NameServerHookMethodType.ChangeNS;
                Console.WriteLine("CacheConfig Failed: switching to {0}", NameServerHookMethod);

            }
        }

        //    [HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\Dnscache\Parameters\DnsPolicyConfig\{74f2c340-2644-4a72-9d81-a61b8f174d7a}]
        //    "ConfigOptions"=dword:00000008
        //    "Name"=hex(7):2e,00,62,00,69,00,74,00,00,00,00,00
        //    "IPSECCARestriction"=""
        //    "GenericDNSServers"="127.0.0.1"
        //    "Version"=dword:00000002
        private void InstallDnsCacheConfigKeys(RegistryKey parentKey)
        {
            Console.Write("Enabling DNS cache hook: ");
            var key = parentKey.CreateSubKey(configGUID);
            key.SetValue("ConfigOptions", 8, RegistryValueKind.DWord);
            key.SetValue("Name", new string[] { ".bit" }, RegistryValueKind.MultiString);
            key.SetValue("IPSECCARestriction", "");
            key.SetValue("GenericDNSServers", "127.0.0.1");
            key.SetValue("Version", 2, RegistryValueKind.DWord);
            Console.WriteLine("Hooked");
        }

        private void RemoveCacheHook()
        {
            var key = GetDnsCacheConfigKey(true);
            if (key != null)
            {
                var subs = key.GetSubKeyNames();
                foreach (string name in subs)
                {
                    var configKey = Registry.LocalMachine.OpenSubKey(DnsPolicyCacheConfigName + name);
                    var val = configKey.GetValue("Name", null) as string;
                    if (name == configGUID)
                    {
                        Console.WriteLine("Disabling DNS cache hook");
                        DeleteConfigKey(key, name);
                    }
                }
            }
        }

        public static bool AnyDotBitCacheConfigKeys()
        {
            List<RegistryKey> found = new List<RegistryKey>();

            var parentKey = GetDnsCacheConfigKey(false);
            if (parentKey == null) return false;
            var subs = parentKey.GetSubKeyNames();
            var keys = subs.Select(m => Registry.LocalMachine.OpenSubKey(DnsPolicyCacheConfigName + m));
            foreach (var k in keys)
            {
                string[] domains =k.GetValue("Name", null) as string[];
                if(domains==null || !domains.Contains(".bit"))
                    return false;
                string server = k.GetValue("GenericDNSServers", null) as string;
                if (server == null || server != "127.0.0.1")
                    return false;
                object options = k.GetValue("ConfigOptions");
                if (!options.Equals(8))
                    return false;
                object version = k.GetValue("Version");
                if (!version.Equals(2))
                    return false;
                return true;
            }
            return false;
        }

        private static RegistryKey GetDnsCacheConfigKey(bool writeable)
        {
            var key = Registry.LocalMachine.OpenSubKey(DnsPolicyCacheConfigName, writeable);
            return key;
        }

        private void DeleteConfigKey(RegistryKey parentKey, string subkeyname)
        {
            parentKey.DeleteSubKey(subkeyname, false);
        }

        private static void DetermineSupport()
        {
            Console.Write("Dermining client hook method...");
            switch (OSVersion)
            {
                case OSVersionType.Win8:
                    NameServerHookMethod = NameServerHookMethodType.CacheHook; break;
                default: // case OSVersionType.Win7:
                    NameServerHookMethod = NameServerHookMethodType.ChangeNS; break;
            }
            Console.WriteLine(" {0}", NameServerHookMethod);
        }

        // http://support.microsoft.com/kb/304283
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx
        internal static OSVersionType GetWindowsVersion()
        {
            Console.Write("Dermining windows version...");
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            Console.WriteLine(" {0}", osInfo);

            switch (osInfo.Platform)
            {
                case PlatformID.Win32S:
                    return OSVersionType.Win16Bit;
                case PlatformID.Win32Windows:
                    {
                        switch (osInfo.Version.Major)
                        {
                            case 4:
                                switch (osInfo.Version.Minor)
                                {
                                    default: case 0:
                                        return OSVersionType.Win95;
                                    case 10:
                                        return OSVersionType.Win98;
                                    case 90:
                                        return OSVersionType.WinME;
                                }
                            default:
                                return OSVersionType.Unknown;
                        }
                    }
                case PlatformID.Win32NT:
                    {
                        switch (osInfo.Version.Major)
                        {
                            case 4:
                                return OSVersionType.WinNT4;
                            case 5:
                                switch (osInfo.Version.Minor)
                                {
                                    default: case 0: return OSVersionType.Win2k;
                                    case 1: return OSVersionType.WinXP;
                                    case 2: return OSVersionType.WinXP64;
                                }
                            case 6:
                                switch (osInfo.Version.Minor)
                                {
                                    default: case 0: return OSVersionType.WinVista;
                                    case 1: return OSVersionType.Win7;
                                    case 2: return OSVersionType.Win8;
                                    case 3: return OSVersionType.Win8_1;
                                }
                            default: return OSVersionType.Unknown;
                        }
                    }
                case PlatformID.MacOSX: return OSVersionType.MacOSX;
                case PlatformID.Unix: return OSVersionType.Unix;
                case PlatformID.WinCE: return OSVersionType.WinCE;
                case PlatformID.Xbox: return OSVersionType.XBox;
                default: return OSVersionType.Unknown;
            }
        }

    }
}
