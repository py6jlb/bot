using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface IUpdateHandler
    {
        Task ReceiveAsync(UpdateType[] allowedUpdates, CancellationToken cancellationToken = default);
    }
}