using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace dotBitNs.Utils
{
    public static class ProcessUtils
    {
        const string regKeyFolders = @"HKEY_USERS\<SID>\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
        const string regValueAppData = @"AppData";

        public static string TryFindOwnerAppDataPath(Process process)
        {
            try
            {
                using (ManagementObjectSearcher searcher = GetProcessSearcherForId(process.Id))
                {
                    foreach (ManagementObject @object in searcher.Get())
                    {
                        Debug.WriteLine("Found running process ");

                        string[] OwnerInfo = new string[2];
                        @object.InvokeMethod("GetOwner", (object[])OwnerInfo);

                        string username = OwnerInfo[0];
                        string domain = OwnerInfo[1];

                        Debug.WriteLine(string.Format(" Owner {0}\\{1}", domain, username));

                        string[] OwnerSid = new string[1];
                        @object.InvokeMethod("GetOwnerSid", (object[])OwnerSid);

                        string sid = OwnerSid[0];
                        Debug.WriteLine(string.Format(" SID: {0}", sid));

                        string path=Registry.GetValue(regKeyFolders.Replace("<SID>", sid), regValueAppData, null) as string;

                        Debug.WriteLine(string.Format("AppData: {0}", path == null ? "<NULL>" : path));

                        return path;
                    }
                }
            }
            catch (Win32Exception ex)
            {
                if ((uint)ex.ErrorCode != 0x80004005)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading process information for {0} : {1}", process.ToString(), ex.Message);
            }
            return null;
        }

        public static string GetProcessCommandline(Process process)
        {
            try
            {
                Debug.Write(string.Format("FileName: {0}", process.MainModule.FileName ?? "<NULL>"));
                using (ManagementObjectSearcher searcher = GetProcessSearcherForId(process.Id))
                {
                    foreach (ManagementObject @object in searcher.Get())
                    {
                        string name = @object["CommandLine"] + " ";
                        Debug.WriteLine(string.Format("CommandLine: {0}", name));
                        return name.Trim();
                    }

                    Debug.WriteLine("");
                }
            }
            catch (Win32Exception ex)
            {
                if ((uint)ex.ErrorCode != 0x80004005)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error reading process information for {0} : {1}", process.ToString(), ex.Message));
            }
            return null;
        }

        private static ManagementObjectSearcher GetProcessSearcherForId(int processId)
        {
            return new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + processId);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static IEnumerable<string> CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

    }
}
