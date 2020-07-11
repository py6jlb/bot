using Bot.TelegramWorker.Services.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.TelegramWorker.Services
{
    public class CallbackQueryHandler: ICallbackQueryHandler
    {
        private readonly TelegramBotClient _bot;

        public CallbackQueryHandler(TelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"Received {callbackQuery.Data}");

            await _bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Received {callbackQuery.Data}");
        }

    }
}
