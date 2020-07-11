using Bot.TelegramWorker.HostedServices;
using Bot.TelegramWorker.Options;
using Bot.TelegramWorker.Services;
using Bot.TelegramWorker.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Telegram.Bot;

namespace Bot.TelegramWorker.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection InitWorker(this IServiceCollection services, HostBuilderContext hostContext)
        {
            var config = hostContext.Configuration;
            var botApiKey = config["BOT_API_KEY"] ?? throw new ArgumentNullException("botApiKey",
                "Параметр не может быть null, проверьте перменные окружения.");

            services.Configure<Categories>(hostContext.Configuration.GetSection("Categories"));
            services.AddTransient<IAuthService, AuthService>();
            services.AddSingleton<TelegramBotClient>((sp) => new TelegramBotClient(botApiKey));
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddTransient<IInlineQueryHandler, InlineQueryHandler>();
            services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
            services.AddTransient<IUpdateHandler, UpdateHandler>();
            services.AddHostedService<TelegramWorkerService>();

            return services;
        }
    }
}