using Bot.TelegramWorker.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Bot.TelegramWorker.Services
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IConfiguration _config;
        private readonly TelegramBotClient _bot;
        private readonly IAuthService _auth;
        private readonly IMessageHandler _messageHandler;
        private readonly IInlineQueryHandler _inlineHandler;
        private readonly ICallbackQueryHandler _callbackHandler;

        public UpdateHandler( IConfiguration config,
            TelegramBotClient bot,
            IAuthService auth,
            IMessageHandler messageHandler, 
            IInlineQueryHandler inlineHandler, 
            ICallbackQueryHandler callbackHandler)
        {
            _config = config;
            _bot = bot;
            _auth = auth;
            _messageHandler = messageHandler;
            _inlineHandler = inlineHandler;
            _callbackHandler = callbackHandler;
        }

        public async Task ReceiveAsync(UpdateType[] allowedUpdates, CancellationToken cancellationToken = default)
        {
            _bot.IsReceiving = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var timeout = Convert.ToInt32(_bot.Timeout.TotalSeconds);
                var updates = new Update[] { };

                try
                {
                    updates = await _bot.GetUpdatesAsync(
                        _bot.MessageOffset,
                        timeout: timeout,
                        allowedUpdates: allowedUpdates,
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (ApiRequestException apiException)
                {
                    await HandleApiError(apiException);
                }
                catch (Exception generalException)
                {
                    await HandleGeneralError(generalException);
                }

                try
                {
                    foreach (var update in updates)
                    {
                        await HandleUpdate(update, cancellationToken);
                        _bot.MessageOffset = update.Id + 1;
                    }
                }
                catch
                {
                    _bot.IsReceiving = false;
                    throw;
                }
            }

            _bot.IsReceiving = false;
        }

        private async Task HandleApiError(ApiRequestException apiException)
        {
            Console.WriteLine($"{apiException.Message}; {apiException.InnerException}");
            await Task.CompletedTask;
        }

        private async Task HandleGeneralError(Exception generalException)
        {
            Console.WriteLine($"{generalException.Message}; {generalException.InnerException}");
            await Task.CompletedTask;
        }

        private async Task HandleUpdate(Update update, CancellationToken cancellationToken)
        {
            var isAllowedUser = await _auth.IsAllowedUser(update);
            if (!isAllowedUser)
            {
                await _bot.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Вы кто такие? Я вас не звал. Идите на х*й."
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

        private async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.Message
            };

            Console.WriteLine(ErrorMessage);
            await Task.CompletedTask;
        }
    }
}