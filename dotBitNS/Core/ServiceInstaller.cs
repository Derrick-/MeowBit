// Thanks to Sachin Nigam for the code.
// http://www.c-sharpcorner.com/UploadFile/sachin.nigam/InstallingWinServiceProgrammatically11262005061332AM/InstallingWinServiceProgrammatically.aspx

using System;
using System.Runtime.InteropServices;
namespace dotBitNS
{
    /// <summary>
    /// Summary description for ServiceInstaller.
    /// </summary>
    class ServiceInstaller
    {
        #region Constants declaration.
        const int SC_MANAGER_CREATE_SERVICE = 0x0002;
        const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        const int SERVICE_DEMAND_START = 0x00000003;
        const int SERVICE_ERROR_NORMAL = 0x00000001;
        const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        const int SERVICE_QUERY_CONFIG = 0x0001;
        const int SERVICE_CHANGE_CONFIG = 0x0002;
        const int SERVICE_QUERY_STATUS = 0x0004;
        const int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
        const int SERVICE_START = 0x0010;
        const int SERVICE_STOP = 0x0020;
        const int SERVICE_PAUSE_CONTINUE = 0x0040;
        const int SERVICE_INTERROGATE = 0x0080;
        const int SERVICE_USER_DEFINED_CONTROL = 0x0100;
        const int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
        SERVICE_QUERY_CONFIG |
        SERVICE_CHANGE_CONFIG |
        SERVICE_QUERY_STATUS |
        SERVICE_ENUMERATE_DEPENDENTS |
        SERVICE_START |
        SERVICE_STOP |
        SERVICE_PAUSE_CONTINUE |
        SERVICE_INTERROGATE |
        SERVICE_USER_DEFINED_CONTROL);
        const int SERVICE_AUTO_START = 0x00000002;
        const int SERVICE_NO_CHANGE = -1;
        #endregion Constants declaration.

        #region Private Variables
        //private string _servicePath;
        //private string _serviceName;
        //private string _serviceDisplayName;

        private bool m_StartOnInstall = false;
        private int m_StartMode = SERVICE_AUTO_START;

        #endregion Private Variables
        #region DLLImport
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);
        [DllImport("Advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
        int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
        string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);
        [DllImport("advapi32.dll")]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("advapi32.dll",
         SetLastError = true, CharSet = CharSet.Auto)]
         private static extern int ChangeServiceConfig(
         IntPtr service,
         int serviceType,
         int startType,
         int errorControl,
         [MarshalAs(UnmanagedType.LPTStr)]
         string binaryPathName,
         [MarshalAs(UnmanagedType.LPTStr)]
         string loadOrderGroup,
         IntPtr tagID,
         [MarshalAs(UnmanagedType.LPTStr)]
         string dependencies,
         [MarshalAs(UnmanagedType.LPTStr)]
         string startName,
         [MarshalAs(UnmanagedType.LPTStr)]
         string password,
         [MarshalAs(UnmanagedType.LPTStr)]
         string displayName);
        #endregion DLLImport
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        #region Main method + testing code
        //[STAThread]
        //static void Main(string[] args)
        //{
        //    // TODO: Add code to start application here
        //    #region Testing
        //    // Testing --------------
        //    string svcPath;
        //    string svcName;
        //    string svcDispName;
        //    //path to the service that you want to install
        //    svcPath = @"C:\build\service\Debug\Service.exe";
        //    svcDispName = "Service Display Name";
        //    svcName = "Service Name";
        //    ServiceInstaller c = new ServiceInstaller();
        //    c.InstallService(svcPath, svcName, svcDispName);
        //    Console.Read();
        //    #endregion Testing
        //}
        #endregion Main method + testing code - Commented
        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="svcPath">The complete path of the service.</param>
        /// <param name="svcName">Name of the service.</param>
        /// <param name="svcDispName">Display name of the service.</param>
        /// <returns>True if the process went thro successfully. False if there was any error.</returns>
        public bool InstallService(string svcPath, string svcName, string svcDispName)
        {
            try
            {
                IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                if (sc_handle.ToInt32() != 0)
                {
                    Console.WriteLine(svcName + " is being installed as a Auto Start Type but will not be started by the installer.");
                    Console.WriteLine("To modify this, use the Services applet in the Control Panel.");
                    Console.WriteLine("To start stop or restart the service, you can run the .exe with args -start -stop or -restart.");
                    IntPtr sv_handle = CreateService(sc_handle, svcName, svcDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, m_StartMode, SERVICE_ERROR_NORMAL, svcPath, null, 0, null, null, null);
                    if (sv_handle.ToInt32() == 0)
                    {
                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        if (m_StartOnInstall)
                        {
                            //now trying to start the service
                            int i = StartService(sv_handle, 0, null);
                            // If the value i is zero, then there was an error starting the service.
                            // note: error may arise if the service is already running or some other problem.
                            if (i == 0)
                            {
                                Console.WriteLine("Couldnt start service");
                                return false;
                            }
                            Console.WriteLine("Success");
                        }
                        CloseServiceHandle(sc_handle);

                        SetServiceDescription();

                        return true;
                    }
                }
                else
                {
                    int errorNum = GetLastError();
                    string error;
                    if (errorNum == 5)
                        error = "ERROR_ACCESS_DENIED";
                    else if (errorNum == 1065)
                        error = "ERROR_DATABASE_DOES_NOT_EXIST";
                    else if (errorNum == 87)
                        error = "ERROR_INVALID_PARAMETER";
                    else
                        error = errorNum.ToString();


                    Console.WriteLine("SCM not opened successfully. Error: {0}", error);
                    
                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SetServiceDescription()
        {
            try
            {
                //Open the HKEY_LOCAL_MACHINE\SYSTEM key
                var system = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System");
                //Open CurrentControlSet
                var currentControlSet = system.OpenSubKey("CurrentControlSet");
                //Go to the services key
                var services = currentControlSet.OpenSubKey("Services");
                //Open the key for your service, and allow writing
                var service = services.OpenSubKey(Service.GlobalServiceName, true);
                //Add your service's description as a REG_SZ value named "Description"
                service.SetValue("Description", Service.GlobalServiceDescription);
                //(Optional) Add some custom information your service will use...
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown while setting service description:\n" + e.Message);
            }
        }

        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public bool UnInstallService(string svcName)
        {
            int GENERIC_WRITE = 0x40000000;
            IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
            if (sc_hndl.ToInt32() != 0)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, svcName, DELETE);
                Console.WriteLine(svc_hndl.ToInt32());
                if (svc_hndl.ToInt32() != 0)
                {
                    int i = DeleteService(svc_hndl);
                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool SetAutoAtart(string svcName)
        {
            int GENERIC_WRITE = 0x40000000;
            IntPtr schSCManager = OpenSCManager(null, null, GENERIC_WRITE);

            if (schSCManager == IntPtr.Zero)
            {
                Console.WriteLine("OpenSCManager failed {0}", GetLastError());
                return false;
            }

            var schService = OpenService(
                 schSCManager,            // SCM database 
                 svcName,               // name of service 
                 SERVICE_CHANGE_CONFIG);  // need change config access 

            if (schService != IntPtr.Zero)
            {
                if (0 == ChangeServiceConfig(
                        schService,      // handle of service 
                        SERVICE_NO_CHANGE, // service type: no change 
                        SERVICE_AUTO_START,// service start type 
                        SERVICE_NO_CHANGE, // error control: no change 
                        null,              // binary path: no change 
                        null,              // load order group: no change 
                        IntPtr.Zero,       // tag ID: no change 
                        null,              // dependencies: no change 
                        null,              // account name: no change 
                        null,              // password: no change 
                        null))             // display name: no change
                     Console.WriteLine("ChangeServiceConfig failed {0}", GetLastError());
                 else 
                     Console.WriteLine("Service change successfully.");

                 CloseServiceHandle(schService);
                 CloseServiceHandle(schSCManager);
                 return true;
            }
            CloseServiceHandle(schSCManager);
            return false;
        }
    }
}
