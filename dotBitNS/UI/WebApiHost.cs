// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System.Web.Http;
using System;
using System.Net.Http;
using System.Web.Http.SelfHost;

namespace dotBitNs.UI
{
    class WebApiHost
    {
        private static int _Port;

        public static int Port
        {
            get { return WebApiHost._Port; }
            private set 
            {
                WebApiHost._Port = value;
                InitializeApiServer();
            }
        }

        private static HttpSelfHostServer server  = null; 

        public static void Initialize()
        {
            Port = dotBitNs.Defaults.DefaultPort;
        }

        private static void InitializeApiServer()
        {
            if (server != null)
            {
                server.Dispose();
                server = null;
            }
            Console.WriteLine("Initializing api on port {0}...", Port);
            try
            {
                string _baseAddress = "http://localhost:" + Port.ToString();
                //server = WebApp.Start<WebApiHost>(url: "http://localhost:" + Port.ToString());
                HttpSelfHostConfiguration config = Configuration(new Uri(_baseAddress));

                // Create server 
                server = new HttpSelfHostServer(config);

                // Start listening 
                server.OpenAsync().Wait();
                Console.WriteLine("Listening on " + _baseAddress); 

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start the API server on port {0}, is the service already running?", Port);
                Console.WriteLine(" - {0}", ex.Message);
            }
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        private static HttpSelfHostConfiguration Configuration(Uri baseaddress)
        {
            // Configure Web API for self-host. 
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseaddress);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            return config;
        }
    }

}
