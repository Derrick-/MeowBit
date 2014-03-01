using dotBitNS.UI.ApiControllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace dotBitNs_Monitor
{
    class ApiClient : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static DependencyProperty PortProperty = DependencyProperty.Register("Port", typeof(int), typeof(ApiClient), new PropertyMetadata(dotBitNS.UI.WebApiHost.DefaultPort, OnPropertyChanged));

        public static DependencyProperty ApiOnlineProperty = DependencyProperty.Register("ApiOnline", typeof(bool), typeof(ApiClient), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyProperty NameCoinOnlineProperty = DependencyProperty.Register("NameCoinOnline", typeof(bool), typeof(ApiClient), new PropertyMetadata(false, OnPropertyChanged));
        public static DependencyProperty NameServerOnlineProperty = DependencyProperty.Register("NameServerOnline", typeof(bool), typeof(ApiClient), new PropertyMetadata(false, OnPropertyChanged));

        public bool ApiOnline
        {
            get { return (bool)GetValue(ApiOnlineProperty); }
            set { SetValue(ApiOnlineProperty, value); }
        }

        public bool NameCoinOnline
        {
            get { return (bool)GetValue(NameCoinOnlineProperty); }
            set { SetValue(NameCoinOnlineProperty, value); }
        }

        public bool NameServerOnline
        {
            get { return (bool)GetValue(NameServerOnlineProperty); }
            set { SetValue(NameServerOnlineProperty, value); }
        }

        System.Timers.Timer t;
        public ApiClient(PropertyChangedEventHandler propChangeHandler)
        {
            PropertyChanged += propChangeHandler;

            t = new Timer(5000);
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            Dispatcher.Invoke(UpdateStatus);
            t.Start();
        }

        async void UpdateStatus()
        {
            ApiMonitorResponse status = await GetStatus();
            if (ApiOnline = status != null)
            {
                NameCoinOnline = status.Nmc;
                NameServerOnline = status.Ns;
            }
        }

        public async Task<ApiMonitorResponse> GetStatus()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(3000);
                var cts = new System.Threading.CancellationTokenSource();

                HttpResponseMessage response=null;
                try
                {
                    response = await client.GetAsync("http://localhost:" + Port + "/api/monitor", cts.Token);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(string.Format("ApiClient.GetStatus(): {0}: {1}", ex.GetType().ToString(), ex.Message));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("ApiClient.GetStatus(): {0}: {1}", ex.GetType().ToString(), ex.Message));
                }
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Read Json, getting response...");
                        string json = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(json);

                        return JsonConvert.DeserializeObject<ApiMonitorResponse>(json);
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("ApiClient.GetStatus(): Http Error: {0}", response.StatusCode));
                    }
                }
            }
            
            return null;
        }

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ApiClient;
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
