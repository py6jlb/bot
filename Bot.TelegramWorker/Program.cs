using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.TelegramWorker
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }


        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    var env = hostContext.HostingEnvironment;
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddEnvironmentVariables();
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) => {
                    //services.InitWorker(hostContext);
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                });
        }
    }
}
