using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.WebService.Services.Base
{
    public interface IAuthService
    {
        Task<bool> IsAllowedUser(Update update);
    }
}
