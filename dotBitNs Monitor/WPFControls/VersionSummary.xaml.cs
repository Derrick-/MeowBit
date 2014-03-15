using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace dotBitNs_Monitor.WPFControls
{
    /// <summary>
    /// Interaction logic for VersionSummary.xaml
    /// </summary>
    public partial class VersionSummary : UserControl, INotifyPropertyChanged
    {

        const string DefaultDownloadLocation = "http://meowbit.com/download-and-install";

        public event PropertyChangedEventHandler PropertyChanged;

        public static DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(VersionSummary), new PropertyMetadata("Unknown", OnPropertyChanged));

        public static DependencyProperty CurrentVersionProperty = DependencyProperty.Register("CurrentVersion", typeof(string), typeof(VersionSummary), new PropertyMetadata("Unknown", OnPropertyChanged));
        public static DependencyProperty LatestVersionProperty = DependencyProperty.Register("LatestVersion", typeof(string), typeof(VersionSummary), new PropertyMetadata("Unknown", OnPropertyChanged));
        public static DependencyProperty UpToDateProperty = DependencyProperty.Register("UpToDate", typeof(bool?), typeof(VersionSummary), new PropertyMetadata(null, OnPropertyChanged));

        public static DependencyProperty DownloadUrlProperty = DependencyProperty.Register("DownloadUrl", typeof(string), typeof(VersionSummary), new PropertyMetadata(DefaultDownloadLocation, OnPropertyChanged));

        public string DisplayName
        {
            get { return GetValueOrDefault<string>(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public string CurrentVersion
        {
            get { return GetValueOrDefault<string>(CurrentVersionProperty); }
            set { SetValue(CurrentVersionProperty, value); }
        }

        public string LatestVersion
        {
            get { return GetValueOrDefault<string>(LatestVersionProperty); }
            set { SetValue(LatestVersionProperty, value); }
        }

        public bool? UpToDate
        {
            get { return GetValueOrDefault<bool?>(UpToDateProperty); }
            set { SetValue(UpToDateProperty, value); }
        }

        public string DownloadUrl
        {
            get { return GetValueOrDefault<string>(DownloadUrlProperty); }
            set { SetValue(DownloadUrlProperty, value); }
        }

        private T GetValueOrDefault<T>(DependencyProperty depProperty)
        {
            return (T)(GetValue(depProperty) ?? depProperty.DefaultMetadata.DefaultValue);
        }

        private void OnVersionChange()
        {
            Version current, latest;

            if (Version.TryParse(CurrentVersion, out current) && Version.TryParse(LatestVersion, out latest))
                UpToDate = current >= latest;
            else
                UpToDate = null;
        }

        public VersionSummary()
        {
            InitializeComponent();
        }
        
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as VersionSummary;
            if (target != null)
            {
                target.OnPropertyChanged(e.Property.Name);
                if (e.Property == CurrentVersionProperty || e.Property == LatestVersionProperty)
                    target.OnVersionChange();
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
