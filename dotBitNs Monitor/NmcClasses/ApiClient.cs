// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs;
using dotBitNs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;

namespace dotBitNs_Monitor
{
    class ApiClient : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static DependencyProperty PortProperty = DependencyProperty.Register("Port", typeof(int), typeof(ApiClient), new PropertyMetadata(dotBitNs.Defaults.DefaultPort, OnPropertyChanged));

        public ApiClient()
        {
        }

        public async Task<ApiMonitorResponse> GetStatus()
        {
            string path = "/api/monitor";
            ApiMonitorResponse toReturn = await ApiGet<ApiMonitorResponse>(path);
            return toReturn;
        }

        public async Task<ProductValue> GetProduct(string productname)
        {
            NmcNameValuePair result = await QueryValue("p/" + productname);
            if (result == null)
                return null;
            return new ProductValue(result.Value);
        }

        public async Task<NmcNameValuePair> QueryValue(string namepath)
        {
            string path = "/api/query/?name=" + Uri.EscapeDataString(namepath);
            dynamic toReturn = await ApiGet<NmcNameValuePair>(path);
            return toReturn;
        }

        private async Task<T> ApiGet<T>(string path)
        {
            HttpResponseMessage response = await ApiGetResponse(path);
            T toReturn = default(T);
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Read Json, getting response...");
                    string json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(json);

                    toReturn = JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    Debug.WriteLine(string.Format("ApiClient.GetStatus(): Http Error: {0}", response.StatusCode));
                }
            }
            return toReturn;
        }

        private async Task<HttpResponseMessage> ApiGetResponse(string path)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(3000);
                var cts = new System.Threading.CancellationTokenSource();

                try
                {
                    response = await client.GetAsync("http://localhost:" + Port + path, cts.Token);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(string.Format("ApiClient.GetStatus(): {0}: {1}", ex.GetType().ToString(), ex.Message));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("ApiClient.GetStatus(): {0}: {1}", ex.GetType().ToString(), ex.Message));
                }
            }
            return response;
        }

        public async void SendConfig(NmcConfigJson config)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(3000);
                var cts = new System.Threading.CancellationTokenSource();

                HttpResponseMessage response = null;
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage();

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(config);
                    HttpContent content = new StringContent(json);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync("http://localhost:" + Port + "/api/control", content, cts.Token);
                }
                catch { }
            }
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

    }
}
