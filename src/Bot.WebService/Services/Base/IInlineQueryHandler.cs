using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.WebService.Services.Base
{
    public interface IInlineQueryHandler
    {
        Task HandleInlineQuery(InlineQuery inlineQuery);
        Task HandleChosenInlineResult(ChosenInlineResult chosenInlineResult);
    }
}
