using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.WebService.Options;
using Bot.WebService.Services;
using Bot.WebService.Services.Base;
using Microsoft.Extensions.Configuration;

namespace Bot.WebService
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBotService, BotService>();
            services.AddScoped<IUpdateService, UpdateService>();
            services.Configure<BotConfiguration>(configuration =>
            {
                var botApiKey = _config["BOT_API_KEY"] ?? throw new ArgumentNullException("botApiKey",
                    "Параметр не может быть null, проверьте перменные окружения.");
                var socks5Host = _config["BOT_SOCKS_HOST"];
                var socks5PortParseResult = int.TryParse(_config["BOT_SOCKS_PORT"], out var socks5Port);

                if (!string.IsNullOrWhiteSpace(socks5Host) && !socks5PortParseResult)
                    throw new ArgumentNullException("socks5Port", "Параметр не может быть null, проверьте перменные окружения.");

                configuration.BotToken = botApiKey;
                configuration.Socks5Host = socks5Host;
                configuration.Socks5Port = socks5Port;

            });
            services.Configure<Categories>(_config.GetSection("Categories"));
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddTransient<IInlineQueryHandler, InlineQueryHandler>();
            services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
            services.AddTransient<IDataService, DataService>();

            services.AddControllers().AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
