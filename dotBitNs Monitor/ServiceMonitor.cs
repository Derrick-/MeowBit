using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Management;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Timers;

namespace dotBitNs_Monitor
{
    class ServiceMonitor : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        static readonly string ProcessName = typeof(dotBitNS.Program).Assembly.GetName().Name;
        static readonly string ServiceName = dotBitNS.Service.GlobalServiceName;

        public event PropertyChangedEventHandler PropertyChanged;

        public static DependencyProperty RunningProperty = DependencyProperty.Register("Running", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyProperty InstalledProperty = DependencyProperty.Register("Installed", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyProperty IsAutoProperty = DependencyProperty.Register("IsAuto", typeof(bool), typeof(ServiceMonitor), new PropertyMetadata(false, OnPropertyChanged));

        Timer t;
        public ServiceMonitor(PropertyChangedEventHandler propChangeHandler)
        {
            PropertyChanged += propChangeHandler;

            t = new Timer(5000);
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            Dispatcher.Invoke(Update);
            t.Start();
        }

        void Update()
        {
            Running = ServiceMonitor.ProcessIsRunning();
            Installed = ServiceMonitor.GetServiceController() != null;
            IsAuto = ServiceMonitor.ServiceIsAutostart();
        }

        public bool Running
        {
            get { return (bool)GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }

        public bool Installed
        {
            get { return (bool)GetValue(InstalledProperty); }
            set { SetValue(InstalledProperty, value); }
        }

        public bool IsAuto
        {
            get { return (bool)GetValue(IsAutoProperty); }
            set { SetValue(IsAutoProperty, value); }
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
            return Service;
        }

        static string GetServiceStartMode(string serviceName)
        {
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
                Debug.WriteLine(string.Format("GetServiceStartMode('{0}') threw exception: {1}", serviceName, ex.Message));
                return "<null>";
            }

            return "<null>";
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
    }
}
