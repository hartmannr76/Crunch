using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Crunch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var url = Environment.GetEnvironmentVariable("RUNNING_URL");
            var port = Environment.GetEnvironmentVariable("PORT");
            Console.Out.WriteLine(string.Format("Running url: {0} ", url));
            Console.Out.WriteLine(string.Format("Port: {0} ", port));
            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                //.UseUrls(url+":"+port)
                .UseStartup<Startup>();
            
            var host = hostBuilder.Build();    
            host.Run();
        }
    }
}
