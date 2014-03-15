using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace dotBitNs_Monitor.Converters
{
    public class NullToVisibiltyConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { return null; }
    }

    public class InverseNullToVisibiltyConverter : NullToVisibiltyConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class BoolToVisibiltyConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((bool?)true).Equals(value)) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { return null; }
    }

    public class InverseBoolToVisibiltyConverter : BoolToVisibiltyConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value==null || ((bool?)true).Equals(value)) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

}
