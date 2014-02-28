using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Management;

namespace dotBitNs_Monitor
{
    class ServiceMonitor
    {
        static readonly string ProcessName = typeof(dotBitNS.Program).Assembly.GetName().Name;
        static readonly string ServiceName = dotBitNS.Service.GlobalServiceName;

        public static bool ProcessIsRunning()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            return processes.Any();

        }

        public static bool ServiceIsInstalled()
        {
            return FindService() != null;
        }

        public static bool ServiceIsAutostart()
        {
            var service = FindService();
            return service != null && GetServiceStartMode(service.DisplayName) == "Auto";
        }

        public static ServiceController FindService()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();

            // Display the list of services currently running on this computer.

            Console.WriteLine("Services running on the local computer:");
            foreach (ServiceController scTemp in scServices)
            {
                if (scTemp.Status == ServiceControllerStatus.Running)
                {

                    if (scTemp.ServiceName == ServiceName)
                        return scTemp;

                    // Write the service name and the display name 
                    // for each running service.
                    //Console.WriteLine();
                    //Console.WriteLine("  Service :        {0}", scTemp.ServiceName);
                    //Console.WriteLine("    Display name:    {0}", scTemp.DisplayName);

                    // Query WMI for additional information about this service. 
                    // Display the start name (LocalSytem, etc) and the service 
                    // description.
                    //ManagementObject wmiService;
                    //wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                    //wmiService.Get();
                    //Console.WriteLine("    Start name:      {0}", wmiService["StartName"]);
                    //Console.WriteLine("    Description:     {0}", wmiService["Description"]);
                }
            }
            return null;
        }

        public static string GetServiceStartMode(string serviceName)
        {
            uint success = 1;

            string filter = String.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName);

            ManagementObjectSearcher query = new ManagementObjectSearcher(filter);

            // No match = failed condition
            if (query == null) return "<null>";

            try
            {
                ManagementObjectCollection services = query.Get();

                foreach (ManagementObject service in services)
                {
                    return service.GetPropertyValue("StartMode").ToString();
                }
            }
            catch (Exception ex)
            {
                return "<null>";
            }

            return "<null>";
        }

    }
}
