using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface IAuthService
    {
        Task<bool> IsAllowedUser(Update update);
    }
}
