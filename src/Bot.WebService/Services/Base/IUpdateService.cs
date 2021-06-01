using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.WebService.Services.Base
{
    public interface IUpdateService
    {
        Task HandleUpdate(Update update, CancellationToken cancellationToken);
    }
}