using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Payroll
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            CreateHostBuilder(args).Build().Run();

            //Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        //{
        //    return WebHost.CreateDefaultBuilder(args)
        //            .UseSetting("detailedErrors", "true")
        //            .CaptureStartupErrors(true)
        //            .UseKestrel(options =>
        //            {
        //                options.Limits.MaxRequestBodySize = null;
        //            })
        //            .UseContentRoot(Directory.GetCurrentDirectory())
        //            .UseIISIntegration()
        //            .UseStartup<Startup>()

        //            // Add the following lines
        //            .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        //            .ReadFrom.Configuration(hostingContext.Configuration));
        //}
    }
}
