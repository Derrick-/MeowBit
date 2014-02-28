using dotBitNs_Monitor.WPFControls;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string AppName = "MeowBit";

        public MainWindow()
        {
            InitializeComponent();

            Title = AppName + " Monitor";
            lblServiceName.Content = AppName + " Service";
            lblAPIName.Content = AppName + " API";

            UpdateServiceStatus();
        }

        void UpdateServiceStatus()
        {
            bool running = ServiceMonitor.ProcessIsRunning();
            bool installed = ServiceMonitor.FindService() != null;
            bool auto = ServiceMonitor.ServiceIsAutostart();

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
            else if (!installed || !auto)
            {
                if (!installed)
                    textStatus = "Service is not running or installed.";
                else //if (!auto)
                    textStatus = "Service is installed, but not set to auto-start.";
                iconService.Status = StatusIcon.StatusType.Forbidden;
            }
            else
            {
                textStatus = "Service is installed, but not running.";
                iconService.Status = StatusIcon.StatusType.Error;
            }
            iconService.ToolTip = textStatus;
        }

    }
}
