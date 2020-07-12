using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
       
        public AuthService(IConfiguration config)
        {
            _config = config;
          
        }

        public async Task<bool> IsAllowedUser(Update update)
        {
            var usernames = _config.GetSection("AllowedUsernames").Get<string[]>();
            var result = update.Type switch
            {
                UpdateType.Message => usernames.Contains(update.Message.From.Username),
                UpdateType.EditedMessage => usernames.Contains(update.Message.From.Username),
                UpdateType.CallbackQuery => usernames.Contains(update.CallbackQuery.From.Username),
                // UpdateType.InlineQuery => _inlineHandler.HandleInlineQuery(update.InlineQuery),
                // UpdateType.ChosenInlineResult => _inlineHandler.HandleChosenInlineResult(update.ChosenInlineResult),
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => false
            };
            return await Task.FromResult(result);
        }
    }
}
