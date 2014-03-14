// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DumpInterfaceManagementObjects
{
    class Program
    {
        const string outFile = "interfaces.txt";
        static void Main(string[] args)
        {
            MultiTextWriter m_MultiConOut;
            Console.SetOut(m_MultiConOut = new MultiTextWriter(Console.Out, new FileLogger(outFile, append: false)));
            Console.WriteLine("OS Version {0} : {1}", System.Environment.OSVersion, WindowsNameServicesManager.GetWindowsVersion());
            using (m_MultiConOut)
            {
                try
                {

                    var wnms = new WindowsNameServicesManager();
                    var moc = WindowsNameServicesManager.GetNetworkConfigs();
                    foreach (ManagementObject mo in moc)
                    {
                        if ((bool)mo["IPEnabled"])
                        {
                            Console.WriteLine();
                            Console.WriteLine(mo["Description"]);
                            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                            WindowsNameServicesManager.DumpInterfaceProps(mo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\n");
            Console.WriteLine("\n\nRegistry Info:");
            WriteRegistryInterfaces();

            System.Diagnostics.Process.Start("notepad.exe", outFile);
        }

        public static void WriteRegistryInterfaces()
        {
            string network_card_key = "SOFTWARE\\Microsoft\\Windows NT\\"
                 + "CurrentVersion\\NetworkCards";
            string service_key = "SYSTEM\\CurrentControlSet\\Services\\";

            RegistryKey local_machine = Registry.LocalMachine;
            RegistryKey service_names = local_machine.OpenSubKey(network_card_key);
            if (service_names == null) return; // Invalid Registry

            string[] network_cards = service_names.GetSubKeyNames();
            service_names.Close();
            foreach (string key_name in network_cards)
            {
                string network_card_key_name = network_card_key + "\\" + key_name;
                RegistryKey card_service_name =
                  local_machine.OpenSubKey(network_card_key_name);
                if (card_service_name == null) return; // Invalid Registry

                string device_service_name = (string)card_service_name.GetValue(
                    "ServiceName");
                string device_name = (string)card_service_name.GetValue(
                    "Description");
                Console.WriteLine("Network Card = " + device_name);

                string service_name = service_key + device_service_name +
                      "\\Parameters\\Tcpip";
                RegistryKey network_key = local_machine.OpenSubKey(service_name);
                Console.WriteLine("Service Name Key: {0}", service_name);
                if (network_key != null)
                {
                    // IPAddresses
                    string[] ipaddresses = (string[])network_key.GetValue("IPAddress");
                    if (ipaddresses == null)
                        Console.WriteLine("<NULL>");
                    else
                        foreach (string ipaddress in ipaddresses)
                        {
                            Console.WriteLine("IPAddress = " + ipaddress);
                        }

                    // Subnets
                    string[] subnets = (string[])network_key.GetValue("SubnetMask");
                    if (subnets == null)
                        Console.WriteLine("<NULL>");
                    else
                        foreach (string subnet in subnets)
                    {
                        Console.WriteLine("SubnetMask = " + subnet);
                    }

                    //DefaultGateway
                    string[] defaultgateways = (string[]) network_key.GetValue("DefaultGateway");
                    if (defaultgateways == null)
                        Console.WriteLine("<NULL>");
                    else
                        foreach (string defaultgateway in defaultgateways)
                    {
                        Console.WriteLine("DefaultGateway = " + defaultgateway);
                    }
                    network_key.Close();
                }
            }
            local_machine.Close();
        }

    }
}