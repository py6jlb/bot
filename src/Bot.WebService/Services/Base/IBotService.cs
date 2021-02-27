using Telegram.Bot;

namespace Bot.WebService.Services.Base
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}