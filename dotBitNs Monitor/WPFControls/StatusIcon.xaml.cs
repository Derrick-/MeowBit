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

namespace dotBitNs_Monitor.WPFControls
{
    /// <summary>
    /// Interaction logic for StatusIcon.xaml
    /// </summary>
    public partial class StatusIcon : UserControl
    {
        public static DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(StatusType), typeof(StatusIcon), new PropertyMetadata(StatusType.Question, OnStatusChanged));

        public enum StatusType
        {
            Ok,
            Error,
            Warning,
            Question,
            Info
        }

        public StatusType Status 
        {
            get { return (StatusType)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        public StatusIcon()
        {
            InitializeComponent();
            UpdateIcons();
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusIcon target = d as StatusIcon;
            if(target!=null)
                target.UpdateIcons();
        }

        private void UpdateIcons()
        {
            SetVisibility(iconOk, Status == StatusType.Ok);
            SetVisibility(iconError, Status == StatusType.Error);
            SetVisibility(iconWarning, Status == StatusType.Warning);
            SetVisibility(iconQuestion, Status == StatusType.Question);
            SetVisibility(iconInfo, Status == StatusType.Info);
        }

        private static void SetVisibility(Image image, bool show)
        {
            image.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }
        

    }
}
