using dotBitNs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dotBitNs_Monitor
{
    class ProductInfoManager : DependencyObject
    {
        public static DependencyProperty MeowBitProperty = DependencyProperty.Register("MeowBit", typeof(ProductValue), typeof(ProductInfoManager), new PropertyMetadata(null));
        public static DependencyProperty DotBitNsProperty = DependencyProperty.Register("DotBitNs", typeof(ProductValue), typeof(ProductInfoManager), new PropertyMetadata(null));

        public ProductValue MeowBit
        {
            get { return (ProductValue)GetValue(MeowBitProperty); }
            set { SetValue(MeowBitProperty, value); }
        }

        public ProductValue DotBitNs
        {
            get { return (ProductValue)GetValue(DotBitNsProperty); }
            set { SetValue(DotBitNsProperty, value); }
        }

        public async void UpdateProductInfo()
        {
            var apiClient = new ApiClient();
            var meowbit = await apiClient.GetProduct("meowbit");
            if (meowbit != null)
                MeowBit = meowbit;

            var dotbitns = await apiClient.GetProduct("dotbitns");
            if (dotbitns != null)
                DotBitNs = dotbitns;
        }
    }
}
