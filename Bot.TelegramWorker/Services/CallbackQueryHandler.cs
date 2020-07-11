using Bot.TelegramWorker.Services.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramWorker.Services
{
    public class CallbackQueryHandler : ICallbackQueryHandler
    {
        private readonly TelegramBotClient _bot;

        public CallbackQueryHandler(TelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"Получено {callbackQuery.Data}");
            await _bot.EditMessageTextAsync(callbackQuery.Message.Chat.Id, 
                callbackQuery.Message.MessageId, 
                $"Получено {callbackQuery.Data}", 
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[0]));
        }

    }
}
