using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface IInlineQueryHandler
    {
        Task HandleInlineQuery(InlineQuery inlineQuery);
        Task HandleChosenInlineResult(ChosenInlineResult chosenInlineResult);
    }
}
