using dotBitNs_Monitor.WPFControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dotBitNs_Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string AppName = "MeowBit";

        ServiceMonitor serviceMonitor;

        public MainWindow()
        {
            InitializeComponent();

            Title = AppName + " Monitor";
            lblServiceName.Content = AppName + " Service";
            lblAPIName.Content = AppName + " API";

            serviceMonitor = new ServiceMonitor();
            serviceMonitor.OnStatusUpdated += serviceMonitor_OnStatusUpdated;

        }

        void serviceMonitor_OnStatusUpdated(object sender, EventArgs e)
        {
            if (serviceMonitor.ApiOnline)
            {
                iconAPI.Status = StatusIcon.StatusType.Ok;
                iconNmc.Status = serviceMonitor.NameCoinOnline ? StatusIcon.StatusType.Ok : StatusIcon.StatusType.Error;
                iconNs.Status = serviceMonitor.NameServerOnline ? StatusIcon.StatusType.Ok : StatusIcon.StatusType.Error;
            }
            else
            {
                iconAPI.Status = StatusIcon.StatusType.Error;
                iconNmc.Status = StatusIcon.StatusType.Question;
                iconNs.Status = StatusIcon.StatusType.Question;
            }

            bool running = serviceMonitor.ServiceRunning;
            bool installed = serviceMonitor.ServiceInstalled;
            bool auto = serviceMonitor.ServiceIsAuto;

            string textStatus;

            if (running)
            {
                if (!installed || !auto)
                {
                    if (!installed)
                        textStatus = "Service is running, but is not installed.";
                    else //if (!auto)
                        textStatus = "Service is running and installed, but not set to auto-start.";
                    iconService.Status = StatusIcon.StatusType.Warning;
                }
                else
                {
                    textStatus = "Service is running and properly installed.";
                    iconService.Status = StatusIcon.StatusType.Ok;
                }
            }
            else
            {
                iconAPI.Status = StatusIcon.StatusType.Error;
                if (!installed || !auto)
                {
                    if (!installed)
                    {
                        textStatus = "Service is not running or installed.";
                        iconService.Status = StatusIcon.StatusType.Forbidden;
                    }
                    else //if (!auto)
                    {
                        textStatus = "Service is installed but not running, and not set to auto-start.";
                        iconService.Status = StatusIcon.StatusType.Error;
                    }
                }
                else
                {
                    textStatus = "Service is installed, but not running.";
                    iconService.Status = StatusIcon.StatusType.Error;
                }
            }

            btnStart.Visibility = !running && installed ? Visibility.Visible : Visibility.Collapsed;
            btnStop.Visibility = running && installed ? Visibility.Visible : Visibility.Collapsed;
            btnAutostart.Visibility = installed && !auto ? Visibility.Visible : Visibility.Collapsed;

            iconService.ToolTip = textStatus;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            ServiceMonitor.TryStartService();
            btnStart.ToolTip = "Trying to start service";
            Timer t = new Timer(20000);
            t.Elapsed += tbtnStart_Elapsed;
            t.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            ServiceMonitor.TryStopService();
            btnStop.ToolTip = "Trying to stop service";
            Timer t = new Timer(20000);
            t.Elapsed += tbtnStop_Elapsed;
            t.Start();
        }

        private void btnAutostart_Click(object sender, RoutedEventArgs e)
        {
            btnAutostart.IsEnabled = false;
            ServiceMonitor.TrySetAutoStartService();
            Timer t = new Timer(20000);
            t.Elapsed += tbtnAutostart_Elapsed;
            t.Start();
        }
    
        void tbtnStart_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(() => { btnStart.IsEnabled = true; });
            btnStart.ToolTip = "";
        }

        private void tbtnStop_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(() => { btnStop.IsEnabled = true; });
            btnStop.ToolTip = "";
        }

        private void tbtnAutostart_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(() => { btnAutostart.IsEnabled = true; });
        }

    }
}
