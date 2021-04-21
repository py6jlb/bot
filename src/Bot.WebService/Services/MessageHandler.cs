using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bot.WebService.Options;
using Bot.WebService.Services.Base;
using Bot.WebService.Services.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.WebService.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly IBotService _botService;
        private readonly IOptions<Categories> _categories;
        private readonly IDataService _dataService;

        public MessageHandler(IBotService botService, ILogger<MessageHandler> logger, 
            IOptions<Categories> categories, IDataService dataService)
        {
            _logger = logger;
            _botService = botService;
            _categories = categories;
            _dataService = dataService;
        }

        public async Task HandleMessageAsync(Message message)
        {
            _logger.LogInformation($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()?.ToLower()) switch
            {
                "/начать" => HandleStartAsync(message),
                "/start" => HandleStartAsync(message),
                "/cancel" => HandleCancelAsync(message),
                "/закрыть" => HandleCancelAsync(message),
                "/помощь" => HandleHelpAsync(message),
                "/help" => HandleHelpAsync(message),
                _ => HandleNonCommandMessageAsync(message)
            };
            await action;
        }

        private async Task HandleStartAsync(Message message)
        {
            await _botService.Client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await SendBaseKeyboardAsync(message);
            await HandleHelpAsync(message);
        }

        private async Task HandleCancelAsync(Message message)
        {
            await DeleteBaseKeyboardAsync(message);
        }

        private async Task HandleHelpAsync(Message message)
        {
            const string usage = "Что я умею:\n" +
                                    "Мне нужно отправлять суммы расходов в рублях, а затем выбирать к какой категории расходов они относятся. " +
                                    "Если надо сохранить приход, то перед суммой должен ыть занк \"+\"";
            await _botService.Client.SendTextMessageAsync(chatId: message.Chat.Id, text: usage);
        }

        private async Task SendBaseKeyboardAsync(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[] { new KeyboardButton("/Помощь") },
                true
            ); ;

            await _botService.Client.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привет!",
                replyMarkup: replyKeyboardMarkup
            );
        }

        private async Task DeleteBaseKeyboardAsync(Message message)
        {
            await _botService.Client.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Клавиатура удалена.",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private async Task HandleNonCommandMessageAsync(Message message)
        {
            var isIncome = message.Text.StartsWith("+");
            var msgText = isIncome ? message.Text.Substring(1) : message.Text;
            var isFloat = float.TryParse(msgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var num);
            if (isFloat)
            {
                var savedData = await _dataService.SaveBaseData(new BaseInfo
                {
                    FromUserName = message.From.Username,
                    Number = num,
                    RegisterDate = message.Date,
                    Sign = isIncome ? "+" : "-"
                });

                await _botService.Client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"{savedData.Sign}{num} руб",
                    replyMarkup: GetInlineKeyboard(isIncome, savedData.Id)
                );
            }
            else
            {
                await _botService.Client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понимаю, что вы от меня хотите."
                );
            }
        }

        private InlineKeyboardMarkup GetInlineKeyboard(bool isIncome, string savedDataId)
        {
            var rows = new List<InlineKeyboardButton[]>();
            var categoryCollection = isIncome ? _categories.Value.IncomeMoneyCategories : _categories.Value.OutMoneyCategories;
            var categoriesInLine = _categories.Value.CategoryInLine;
            var categoriesArrays = categoryCollection.SplitArray(categoriesInLine);

            foreach (var arr in categoriesArrays)
            {
                var data = arr.Select(x => {
                    var callback = new CallbackInfo {Id = savedDataId, Ctg = x.Name };
                    return InlineKeyboardButton.WithCallbackData(x.Icon, callback.ToString());
                }).ToArray();
                rows.Add(data);
            }

            var inlineKeyboard = new InlineKeyboardMarkup(rows.ToArray());
            return inlineKeyboard;
        }

        //private async Task SendFile(Message message)
        //{
        //    await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

        //    const string filePath = @"Files/tux.png";
        //    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
        //    await _bot.SendPhotoAsync(
        //        chatId: message.Chat.Id,
        //        photo: new InputOnlineFile(fileStream, fileName),
        //        caption: "Nice Picture"
        //    );
        //}

        //private async Task RequestContactAndLocation(Message message)
        //{
        //    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
        //    {
        //            KeyboardButton.WithRequestLocation("Местоположение"),
        //            KeyboardButton.WithRequestContact("Контакт"),
        //    });
        //    await _bot.SendTextMessageAsync(
        //        chatId: message.Chat.Id,
        //        text: "Кто или Где ты?",
        //        replyMarkup: RequestReplyKeyboard
        //    );
        //}
    }
}
