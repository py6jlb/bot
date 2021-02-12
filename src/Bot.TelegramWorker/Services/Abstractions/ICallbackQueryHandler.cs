using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface ICallbackQueryHandler
    {
        Task HandleCallbackQuery(CallbackQuery callbackQuery);
    }
}
