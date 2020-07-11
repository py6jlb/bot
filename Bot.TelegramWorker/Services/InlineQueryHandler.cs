using Bot.TelegramWorker.Services.Abstractions;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Bot.TelegramWorker.Services
{
    public class InlineQueryHandler : IInlineQueryHandler
    {
        private readonly TelegramBotClient _bot;

        public InlineQueryHandler(TelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task HandleInlineQuery(InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent("hello")
                )
            };

            await _bot.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }


        public async Task HandleChosenInlineResult(ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        }
    }
}
