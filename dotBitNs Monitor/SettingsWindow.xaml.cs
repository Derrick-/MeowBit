// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static DependencyProperty MonitorProperty = DependencyProperty.Register("Monitor", typeof(ServiceMonitor), typeof(SettingsWindow), new PropertyMetadata(null));
        public static DependencyProperty ProductInfoProperty = DependencyProperty.Register("ProductInfo", typeof(ProductInfoManager), typeof(SettingsWindow), new PropertyMetadata(null));
        public static DependencyProperty ConfigManagerProperty = DependencyProperty.Register("ConfigManager", typeof(ConfigurationManager), typeof(SettingsWindow), new PropertyMetadata(null));

        internal ServiceMonitor Monitor
        {
            get { return (ServiceMonitor)GetValue(MonitorProperty); }
        }

        internal ProductInfoManager ProductInfo
        {
            get { return (ProductInfoManager)GetValue(ProductInfoProperty); }
        }

        internal ConfigurationManager ConfigManager
        {
            get { return (ConfigurationManager)GetValue(ConfigManagerProperty); }
        }
        
        public enum TabName
        {
            DontCare = 0,
            Settings,
            About
        }

        internal SettingsWindow(ServiceMonitor monitor, ProductInfoManager productInfoManager)
        {
            SetValue(MonitorProperty, monitor); ;
            SetValue(ProductInfoProperty, productInfoManager);
            SetValue(ConfigManagerProperty, ConfigurationManager.Instance);
            InitializeComponent();
        }

        private readonly static string _VersionString = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string VersionString
        {
            get { return _VersionString; }
        } 


        public void SwitchToTab(TabName tab)
        {
            object select = null;
            switch (tab)
            {
                case TabName.About:
                    select = About;
                    break;
                case TabName.Settings:
                    select = Settings;
                    break;
            }
            if (select != null)
                Tabs.SelectedValue = select;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ButtonOpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = Monitor.LogFolder;
            if(folder!=null && Directory.Exists(folder))
                Process.Start(new ProcessStartInfo("file://" + folder));
            e.Handled = true;

        }

        private void ButtonOpenLog_Click(object sender, RoutedEventArgs e)
        {
            string filename = GetNewestLogFile();
            if (filename != null && File.Exists(filename))
                Process.Start(new ProcessStartInfo("file://" + filename));
            e.Handled = true;
        }

        private string GetNewestLogFile()
        {
            string folder = Monitor.LogFolder;
            if (folder != null && Directory.Exists(folder))
            {
                var filenames = Directory.EnumerateFiles(folder, "*.log", SearchOption.TopDirectoryOnly);
                if (filenames.Count() > 0)
                {
                    var files = filenames.Select(m => new { name = m, date = Directory.GetLastWriteTime(m) });
                    var newest = files.Max(m => m.date);
                    var file = files.Where(m => m.date == newest).LastOrDefault();
                        return file.name;
                }

            }
            return null;
        }

        private void ButtonCopyLatest_Click(object sender, RoutedEventArgs e)
        {
            string filename = GetNewestLogFile();
            if (filename != null && File.Exists(filename))
            {
                string contents;
                using (StreamReader sr = new StreamReader(filename))
                    contents = sr.ReadToEnd();
                try
                {
                    Clipboard.SetText(contents);
                    Audio.PlaySuccess();
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    Audio.PlayFail();
                }
            }
        }

        private void ProductCheck_Click(object sender, RoutedEventArgs e)
        {
            ProductInfo.UpdateProductInfo();
        }
    }
}
