using System;
using System.Threading.Tasks;
using Bot.WebService.Services.Base;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Bot.WebService.Services
{
    public class InlineQueryHandler : IInlineQueryHandler
    {
        private readonly ILogger<InlineQueryHandler> _logger;
        private readonly IBotService _botService;

        public InlineQueryHandler(IBotService botService, ILogger<InlineQueryHandler> logger)
        {
            _logger = logger;
            _botService = botService;
        }

        public async Task HandleInlineQuery(InlineQuery inlineQuery)
        {
            _logger.LogInformation($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent("hello")
                )
            };

            await _botService.Client.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }


        public async Task HandleChosenInlineResult(ChosenInlineResult chosenInlineResult)
        {
            _logger.LogInformation($"Received inline result: {chosenInlineResult.ResultId}");
            await Task.CompletedTask;
        }
    }
}
