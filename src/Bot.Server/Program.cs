using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Bot.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults( webBuilder =>
            {
                webBuilder.ConfigureAppConfiguration((context, builder) =>
                {
                    var env = context.HostingEnvironment;
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    Console.Write(Directory.GetCurrentDirectory());

                    builder.AddJsonFile(Path.Combine("Configurations", "appsettings.json"), optional: true, reloadOnChange: true)
                        .AddJsonFile(Path.Combine("Configurations",$"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: true);
                    builder.AddEnvironmentVariables();
                    builder.AddCommandLine(args);
                    
                }).UseStartup<Startup>();
            });
    }
}