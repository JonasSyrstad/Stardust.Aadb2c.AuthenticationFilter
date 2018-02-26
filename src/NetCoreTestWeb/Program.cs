using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NetCoreTestWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args);
            Startup.Host = host;
            return host
                .ConfigureLogging(builder => builder.AddFile(options =>
                {
                    options.FileName = "diagnostics-"; // The log file prefixes
                    options.LogDirectory = "LogFiles"; // The directory to write the logs
                    options.FileSizeLimit = 20 * 1024 * 1024; // The maximum log file size (20MB here)
                }))
                .UseStartup<Startup>()
                .Build();
        }
    }
}
