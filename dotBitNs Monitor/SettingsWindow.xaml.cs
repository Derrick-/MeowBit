// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static DependencyProperty MonitorProperty = DependencyProperty.Register("Monitor", typeof(ServiceMonitor), typeof(SettingsWindow), new PropertyMetadata(null));

        internal ServiceMonitor Monitor
        {
            get { return (ServiceMonitor)GetValue(MonitorProperty); }
        }

        public enum TabName
        {
            DontCare = 0,
            Settings,
            About
        }

        internal SettingsWindow(ServiceMonitor monitor)
        {
            SetValue(MonitorProperty, monitor); ;
            InitializeComponent();
        }

        private readonly static string _VersionString = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string VersionString
        {
            get { return _VersionString; }
        } 


        public void SwitchToTab(TabName tab)
        {
            //object select=null;
            //switch (tab)
            //{
            //    case TabName.About:
            //        select = About;
            //        break;
            //    case TabName.Settings:
            //        select=Settings;
            //        break;
            //}
            //if (select != null)
            //    Tabs.SelectedValue = select;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
