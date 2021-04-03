using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.WebService.Services.Base
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(Message message);
    }
}
