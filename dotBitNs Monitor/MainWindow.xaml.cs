// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs_Monitor.WPFControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    interface IMainWindow
    {
        void EnsureVisible();
        void ShowSettingsWindow(SettingsWindow.TabName tab = default(SettingsWindow.TabName));
        void Exit();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {

        public static readonly DateTime GenesisBlockTimeGMT = new DateTime(2011, 4, 19, 12, 59, 40);

        public const string AppName = "MeowBit";

        ServiceMonitor serviceMonitor;
        ProductInfoManager productInfoManager;

        public MainWindow()
        {
            InitializeComponent();

            Title = AppName + " Monitor";
            lblServiceName.Content = AppName + " Service";
            lblAPIName.Content = AppName + " API";

            serviceMonitor = new ServiceMonitor();
            serviceMonitor.OnStatusUpdated += serviceMonitor_OnStatusUpdated;
            serviceMonitor.SystemGoChanged += serviceMonitor_SystemGoChanged;

            productInfoManager = new ProductInfoManager();

            NmcConfigSettings.ConfigUpdated += NmcConfigSetter_ConfigUpdated;
            NmcConfigSettings.ValidateNmcConfig();

            Program.OnAdditionalInstanceSignal += OnRequestShow;

            this.Closing += MainWindow_Closing;
            this.Closed += MainWindow_Closed;

            this.StateChanged += MainWindow_StateChanged;

            MyNotifyIcon.DoubleClickCommandParameter = MyNotifyIcon;
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            var window = sender as MainWindow;
            if (window != null)
                window.ShowInTaskbar = window.WindowState != System.Windows.WindowState.Minimized;
        }

        bool AllowExit=false;
        public void Exit()
        {
            AllowExit=true;
            Close();
        }

        private SettingsWindow about = null;
        public void ShowSettingsWindow(SettingsWindow.TabName tab = default(SettingsWindow.TabName))
        {
            if (about == null)
            {
                about = new SettingsWindow(serviceMonitor, productInfoManager);
                about.Closed += about_Closed;
            }
            EnsureVisible(about);
            about.SwitchToTab(tab);
        }

        void about_Closed(object sender, EventArgs e)
        {
            about = null;
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowExit)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (about != null)
            {
                about.Close();
                about = null;
            }
        }

        private void OnRequestShow(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(EnsureVisible));
        }

        public void EnsureVisible()
        {
            EnsureVisible(this);
        }

        private static void EnsureVisible(Window window)
        {
            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Activate();
            window.Topmost = true;  // important
            window.Topmost = false; // important
            window.Focus();         // important
        }

        void NmcConfigSetter_ConfigUpdated()
        {
            txtNameCoinInfo.Text = "NameCoin config updated: restart wallet.";
        }

        void serviceMonitor_SystemGoChanged(object sender, ServiceMonitor.SystemGoEventArgs e)
        {
            string icoName;
            if (e.NewValue)
                icoName = "Cat48_Ok.ico";
            else
                icoName = "Cat48_Error.ico";

            var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/" + icoName)).Stream;
            var icon = new System.Drawing.Icon(iconStream);
            MyNotifyIcon.Icon = icon;

            productInfoManager.UpdateProductInfo();
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

            btnInstall.Visibility = !installed ? Visibility.Visible : Visibility.Collapsed;
            btnStart.Visibility = !running && installed ? Visibility.Visible : Visibility.Collapsed;
            btnStop.Visibility = running && installed ? Visibility.Visible : Visibility.Collapsed;
            btnAutostart.Visibility = installed && !auto ? Visibility.Visible : Visibility.Collapsed;

            if (serviceMonitor.LastBlockTime.HasValue)
            {
                var now = DateTime.UtcNow.Ticks;
                var start = GenesisBlockTimeGMT.Ticks;
                var current = serviceMonitor.LastBlockTime.Value.Ticks;
                var total = now - start;

                pbStatus.Maximum = total;
                pbStatus.Value = current - start;
                TimeSpan behind = TimeSpan.FromTicks(now - current);
                if(behind > TimeSpan.FromMinutes(10))
                    txtBlockStatus.Text = FriendlyTimeString(behind) + " behind.";
                else
                    txtBlockStatus.Text = "Up to date";
            }
            else
            {
                pbStatus.Value = 0;
                txtBlockStatus.Text = "No Blocks";
            }
                

            if (serviceMonitor.NameCoinOnline)
                txtNameCoinInfo.Text = "";

            iconService.ToolTip = textStatus;
            MyNotifyIcon.ToolTipText = "Meow Bit: " + textStatus;
        }

        private string FriendlyTimeString(TimeSpan ts)
        {
            int days = (int)ts.TotalDays;
            int weeks = days / 7;
            int months = days / 30; //naive guess at month size
            int years = days / 365; //no leap year accounting

            if (years > 1)
                return years.ToString() + " years";
            else if (months > 1)
                return months.ToString() + " months";
            if (weeks > 1)
                return weeks.ToString() + " weeks";
            if (days > 2)
                return days.ToString() + " days";
            if(ts.TotalHours > 1)
                return ((int)ts.TotalHours).ToString() + " hours";
            return ((int)ts.TotalMinutes).ToString() + " minutes";
        }

        const int buttonDisableDurationMs = 20000;

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            btnInstall.IsEnabled = false;
            ServiceMonitor.TryInstallService();
            btnStart.ToolTip = "Trying to install service";
            Timer t = new Timer(buttonDisableDurationMs);
            t.Elapsed += tbtnInstall_Elapsed;
            t.Start();
        }

        private void btnAutostart_Click(object sender, RoutedEventArgs e)
        {
            btnAutostart.IsEnabled = false;
            ServiceMonitor.TrySetAutoStartService();
            Timer t = new Timer(buttonDisableDurationMs);
            t.Elapsed += tbtnAutostart_Elapsed;
            t.Start();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            ServiceMonitor.TryStartService();
            btnStart.ToolTip = "Trying to start service";
            Timer t = new Timer(buttonDisableDurationMs);
            t.Elapsed += tbtnStart_Elapsed;
            t.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            ServiceMonitor.TryStopService();
            btnStop.ToolTip = "Trying to stop service";
            Timer t = new Timer(buttonDisableDurationMs);
            t.Elapsed += tbtnStop_Elapsed;
            t.Start();
        }

        private void tbtnInstall_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(new Action(() => { btnInstall.IsEnabled = true; }));
            btnInstall.ToolTip = "";
        }

        void tbtnStart_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(new Action(() => { btnStart.IsEnabled = true; }));
            btnStart.ToolTip = "";
        }

        private void tbtnStop_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(new Action(() => { btnStop.IsEnabled = true; }));
            btnStop.ToolTip = "";
        }

        private void tbtnAutostart_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.Invoke(new Action(() => { btnAutostart.IsEnabled = true; }));
        }

        private void MyNotifyIcon_TrayContextMenuOpen(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void MyNotifyIcon_PreviewTrayContextMenuOpen(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Hyperlink_OpenSettings(object sender, RequestNavigateEventArgs e)
        {
            ShowSettingsWindow(SettingsWindow.TabName.Settings);
        }

        private void Hyperlink_OpenAbout(object sender, RequestNavigateEventArgs e)
        {
            ShowSettingsWindow(SettingsWindow.TabName.About);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
