using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public OSVersionType OSVersion { get; private set; }
        public NameServerHookMethodType NameServerHookMethod{get;private set;}

        public WindowsNameServicesManager()
        {
            OSVersion = GetWindowsVersion();
            DetermineSupport();
        }

        public void Enable()
        {
            if (NameServerHookMethod == NameServerHookMethodType.CacheHook)
            {
                InsertCacheHook();
            }
        }

        public void Disable()
        {
            if (NameServerHookMethod == NameServerHookMethodType.CacheHook)
            {
                RemoveCacheHook();
            }
        }

        const string DnsPolicyCacheConfigName = @"SYSTEM\ControlSet001\Services\Dnscache\Parameters\DnsPolicyConfig\";
        const string configGUID = "{74f2c340-2644-4a72-9d81-a61b8f174d7a}";

        private void InsertCacheHook()
        {
            var key = GetDnsCacheConfigKey(true);
            var subs = key.GetSubKeyNames();
            RegistryKey found = null;
            foreach (string name in subs)
            {
                var configKey = Registry.LocalMachine.OpenSubKey(DnsPolicyCacheConfigName + name);
                var val = configKey.GetValue("Name", null) as string[];
                if (name == configGUID)
                {
                    if(found!=null) // duplicate found
                        DeleteConfigKey(key, name);
                    else if (!val.Contains(".bit"))
                    {
                        found = null;
                        DeleteConfigKey(key,name);
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

        //    [HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\Dnscache\Parameters\DnsPolicyConfig\{74f2c340-2644-4a72-9d81-a61b8f174d7a}]
        //    "ConfigOptions"=dword:00000008
        //    "Name"=hex(7):2e,00,62,00,69,00,74,00,00,00,00,00
        //    "IPSECCARestriction"=""
        //    "GenericDNSServers"="127.0.0.1"
        //    "Version"=dword:00000002
        private void InstallDnsCacheConfigKeys(RegistryKey parentKey)
        {
            Console.WriteLine("Enabling DNS cache hook");
            var key = parentKey.CreateSubKey(configGUID);
            key.SetValue("ConfigOptions", 8, RegistryValueKind.DWord);
            key.SetValue("Name", new string[] { ".bit" }, RegistryValueKind.MultiString);
            key.SetValue("IPSECCARestriction", "");
            key.SetValue("GenericDNSServers", "127.0.0.1");
            key.SetValue("Version", 2, RegistryValueKind.DWord);
        }

        private void RemoveCacheHook()
        {
            var key = GetDnsCacheConfigKey(true);
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
            parentKey.DeleteSubKey(subkeyname);
        }

        private void DetermineSupport()
        {
            if (OSVersion >= OSVersionType.Win8)
                NameServerHookMethod = NameServerHookMethodType.CacheHook;
            else if (OSVersion >= OSVersionType.WinXP)
                NameServerHookMethod = NameServerHookMethodType.ChangeNS;
            else
                NameServerHookMethod = NameServerHookMethodType.Unsupported;
        }

        // http://support.microsoft.com/kb/304283
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx
        public OSVersionType GetWindowsVersion()
        {
            System.OperatingSystem osInfo = System.Environment.OSVersion;

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
