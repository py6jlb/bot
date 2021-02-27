using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.WebService.Services.Base;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.WebService.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;
        private readonly IAuthService _auth;
        private readonly IMessageHandler _messageHandler;
        private readonly IInlineQueryHandler _inlineHandler;
        private readonly ICallbackQueryHandler _callbackHandler;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger,
            IAuthService auth,
            IMessageHandler messageHandler,
            IInlineQueryHandler inlineHandler,
            ICallbackQueryHandler callbackHandler)
        {
            _botService = botService;
            _logger = logger;
            _auth = auth;
            _messageHandler = messageHandler;
            _inlineHandler = inlineHandler;
            _callbackHandler = callbackHandler;
        }

        public async Task HandleUpdate(Update update, CancellationToken cancellationToken)
        {
            var isAllowedUser = await _auth.IsAllowedUser(update);
            if (!isAllowedUser)
            {
                await _botService.Client.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Вы кто такие? Я вас не звал. Идите на х*й!"
                );
                return;
            }

            var handler = update.Type switch
            {
                UpdateType.Message => _messageHandler.HandleMessage(update.Message),
                UpdateType.EditedMessage => _messageHandler.HandleMessage(update.Message),
                UpdateType.CallbackQuery => _callbackHandler.HandleCallbackQuery(update.CallbackQuery),
                // UpdateType.InlineQuery => _inlineHandler.HandleInlineQuery(update.InlineQuery),
                // UpdateType.ChosenInlineResult => _inlineHandler.HandleChosenInlineResult(update.ChosenInlineResult),
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception, cancellationToken);
            }
        }

        private async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            await Task.CompletedTask;
        }

        private static async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.Message
            };

            Console.WriteLine(errorMessage);
            await Task.CompletedTask;
        }
    }
}