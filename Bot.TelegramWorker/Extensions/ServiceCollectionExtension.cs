using System;
using Bot.TelegramWorker.Services;
using Bot.TelegramWorker.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Bot.TelegramWorker.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection InitWorker(this IServiceCollection services, HostBuilderContext hostContext)
        {
            var config = hostContext.Configuration;
            var botId = config["BOT_ID"] ?? throw new ArgumentNullException("botId",
                "Параметр не может быть null, проверьте перменные окружения.");
            var botApiKey = config["BOT_API_KEY"] ?? throw new ArgumentNullException("botApiKey",
                "Параметр не может быть null, проверьте перменные окружения.");

            services.AddSingleton<TelegramBotClient>((sp) =>
            {
                var bot = new TelegramBotClient(botApiKey);
                return bot;
            });
            services.AddTransient<IUpdateHandler, UpdateHandler>();
            return services;
        }
    }
}