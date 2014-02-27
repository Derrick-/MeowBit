using Owin;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;

namespace dotBitNS.UI
{
    class WebApiHost
    {
        public static int Port { get; private set; }

        const string baseAddress = "http://localhost:9000/";
        
        private static IDisposable app;

        public static void Initialize()
        {
            Port = 9000;

            Console.WriteLine("Initializing api on port {0}...",Port);
            app = WebApp.Start<WebApiHost>(url: "http://localhost:" + Port.ToString());
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }

}
