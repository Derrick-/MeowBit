using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Management;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Timers;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace dotBitNs_Monitor
{
    class ServiceMonitor : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        static readonly string ProcessName = typeof(dotBitNS.Program).Assembly.GetName().Name;
        static readonly string ServiceName = dotBitNS.Service.GlobalServiceName;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SystemGoEventArgs> SystemGoChanged;

        public event EventHandler OnStatusUpdated;

        public static DependencyPropertyKey SystemGoProperty = DependencyProperty.RegisterReadOnly("SystemGo", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnSystemGoPropertyChanged));

        public static DependencyPropertyKey ServiceRunningProperty = DependencyProperty.RegisterReadOnly("ServiceRunning", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyPropertyKey ServiceInstalledProperty = DependencyProperty.RegisterReadOnly("ServiceInstalled", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyPropertyKey ServiceIsAutoProperty = DependencyProperty.RegisterReadOnly("ServiceIsAuto", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));

        public static DependencyPropertyKey ApiOnlineProperty = DependencyProperty.RegisterReadOnly("ApiOnline", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyPropertyKey NameCoinOnlineProperty = DependencyProperty.RegisterReadOnly("NameCoinOnline", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyPropertyKey NameServerOnlineProperty = DependencyProperty.RegisterReadOnly("NameServerOnline", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));

        public class SystemGoEventArgs : EventArgs
        {
            public bool OldValue { get; set; }
            public bool NewValue { get; set; }
        }

        private static void OnSystemGoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ServiceMonitor;
            if (target != null)
            {
                target.OnPropertyChanged(e.Property.Name);
                if (target.SystemGoChanged != null)
                    target.SystemGoChanged(target, new SystemGoEventArgs() { OldValue = (bool)e.OldValue, NewValue = (bool)e.NewValue });
            }
        }

        ApiClient apiClient = new ApiClient();

        Timer t;
        public ServiceMonitor()
        {
            t = new Timer(5000);
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            Dispatcher.Invoke(UpdateStatus);
            t.Start();
        }

        void UpdateStatus()
        {
            ServiceRunning = ServiceMonitor.ProcessIsRunning();
            ServiceInstalled = ServiceMonitor.GetServiceController() != null;
            ServiceIsAuto = ServiceMonitor.ServiceIsAutostart();

            if (ServiceRunning)
                UpdateApiStatus();
            else
            {
                ApiOnline = false;
                NameCoinOnline = false;
                NameServerOnline = false;
            }

            SystemGo = ServiceRunning && ApiOnline && NameCoinOnline && NameServerOnline;

            if (OnStatusUpdated != null)
                OnStatusUpdated(this, new EventArgs() { });
        }

        async void UpdateApiStatus()
        {
            dotBitNS.UI.ApiControllers.ApiMonitorResponse status = await apiClient.GetStatus();
            if (ApiOnline = status != null)
            {
                NameCoinOnline = status.Nmc;
                NameServerOnline = status.Ns;
            }
            if (!NameCoinOnline)
            {
                apiClient.SendConfig();
            }
        }

        /// <summary>
        /// This property is set or unset by the monitor to indicate whether nameserver calls should be hooked.
        /// </summary>
        public bool SystemGo
        {
            get { return (bool)GetValue(SystemGoProperty.DependencyProperty); }
            private set { SetValue(SystemGoProperty, value); }
        }

        public bool ServiceRunning
        {
            get { return (bool)GetValue(ServiceRunningProperty.DependencyProperty); }
            private set { SetValue(ServiceRunningProperty, value); }
        }

        public bool ServiceInstalled
        {
            get { return (bool)GetValue(ServiceInstalledProperty.DependencyProperty); }
            private set { SetValue(ServiceInstalledProperty, value); }
        }

        public bool ServiceIsAuto
        {
            get { return (bool)GetValue(ServiceIsAutoProperty.DependencyProperty); }
            private set { SetValue(ServiceIsAutoProperty, value); }
        }

        public bool ApiOnline
        {
            get { return (bool)GetValue(ApiOnlineProperty.DependencyProperty); }
            private set { SetValue(ApiOnlineProperty, value); }
        }

        public bool NameCoinOnline
        {
            get { return (bool)GetValue(NameCoinOnlineProperty.DependencyProperty); }
            private set { SetValue(NameCoinOnlineProperty, value); }
        }

        public bool NameServerOnline
        {
            get { return (bool)GetValue(NameServerOnlineProperty.DependencyProperty); }
            private set { SetValue(NameServerOnlineProperty, value); }
        }

        static bool ProcessIsRunning()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            return processes.Any();

        }

        static bool ServiceIsInstalled()
        {
            return GetServiceController() != null;
        }

        static bool ServiceIsAutostart()
        {
            return GetServiceStartMode(ServiceName) == "Auto";
        }

        static ServiceController GetServiceController()
        {
            var Service = new ServiceController(ServiceName);
            try
            {
                var status = Service.Status;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            return Service;
        }

        static string GetServiceStartMode(string serviceName)
        {
            try
            {
                var svc = GetServiceManagementObject(serviceName);
                if (svc != null)
                    return svc.GetPropertyValue("StartMode").ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("GetServiceStartMode('{0}') threw exception: {1}", serviceName, ex.Message));
            }
            return "<null>";
        }

        static ManagementObject GetServiceManagementObject(string serviceName)
        {
            string filter = String.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName);

            ManagementObjectSearcher query = new ManagementObjectSearcher(filter);

            // No match = failed condition
            if (query == null) return null;

            try
            {
                ManagementObjectCollection services = query.Get();

                foreach (ManagementObject service in services)
                {
                    return service;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("GetServiceManagementObject('{0}') threw exception: {1}", serviceName, ex.Message));
                return null;
            }

            return null;
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ServiceMonitor;
            if (target != null)
                target.OnPropertyChanged(e.Property.Name);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void Dispose()
        {
            if (t != null)
                t.Dispose();
            t = null;
        }

        internal static void TryInstallService()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, "dotBitNS.exe");
            TryServiceProcessRunAs("-install", path);
        }

        internal static void TrySetAutoStartService()
        {
            var svc = GetServiceController();
            if (svc != null)
            {
                TryServiceProcessRunAs("-setauto");
            }
        }

        internal static void TryStartService()
        {
            var svc = GetServiceController();
            if (svc != null)
            {
                try
                {
                    svc.Start();
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Invalid Operation in Starting the service. May not have permissions");
                    TryServiceProcessRunAs("-start");
                }
            }
        }

        internal static void TryStopService()
        {
            var svc = GetServiceController();
            if (svc != null)
            {
                try
                {
                    svc.Stop();
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Invalid Operation in Stopping the service. May not have permissions");
                    TryServiceProcessRunAs("-stop");

                }
            }
        }

        private static void TryServiceProcessRunAs(string args, string command = null)
        {
            string path = null;
            try
            {
                if (command == null)
                {
                    var svc = GetServiceManagementObject(ServiceName);
                    if (svc != null)
                    {
                        path = svc.GetPropertyValue("PathName") as string;
                        path = path.Replace(".exe -service", ".exe");
                    }
                }
                else
                    path = command;
                if (path != null && File.Exists(path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(path, args);
                    startInfo.Verb = "runas";
                    startInfo.ErrorDialog = false;
                    startInfo.CreateNoWindow = true;
                    try
                    {
                        System.Diagnostics.Process.Start(startInfo);
                    }
                    catch (System.Runtime.InteropServices.ExternalException) { }
                }
            }
            catch { }
        }

    }
}
