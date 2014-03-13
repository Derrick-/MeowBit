// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs;
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
            System.Diagnostics.Process.Start("notepad.exe", outFile);
        }
    }
}