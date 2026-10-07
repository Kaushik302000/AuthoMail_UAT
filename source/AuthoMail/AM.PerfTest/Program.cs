using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.PerfTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (args.Length > 0)
                    {
                        config.AddJsonFile($"appsettings.{args[0]}.json", true, true);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    #region snippet3
                    var config = hostContext.Configuration;

                    services.AddSingleton<App>();
                    services.Configure<Config>(config.GetSection(nameof(Config)));
                    #endregion
                })
                .Build();

            await host.StartAsync();

            var app = host.Services.GetRequiredService<App>();

            await app.RunAsync(args);

            await host.WaitForShutdownAsync();

        }
    }
}
