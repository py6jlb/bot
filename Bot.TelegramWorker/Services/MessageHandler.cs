using Bot.TelegramWorker.Extensions;
using Bot.TelegramWorker.Options;
using Bot.TelegramWorker.Services.Abstractions;
using Bot.TelegramWorker.Services.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramWorker.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly TelegramBotClient _bot;
        private readonly IOptions<Categories> _categories;
        private readonly IDataService _dataService;

        public MessageHandler(TelegramBotClient bot, IOptions<Categories> categories, IDataService dataService)
        {
            _bot = bot;
            _categories = categories;
            _dataService = dataService;
        }

        public async Task HandleMessage(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()?.ToLower()) switch
            {
                "/начать" => HandleStart(message),
                "/start" => HandleStart(message),
                "/cancel" => HandleCancel(message),
                "/закрыть" => HandleCancel(message),
                "/помощь" => Usage(message),
                "/help" => Usage(message),
                _ => HandleNonComandMessage(message)
            };
            await action;
        }

        private async Task HandleStart(Message message)
        {
            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await SendBaseKeyboard(message);
            await Usage(message);
        }

        private async Task HandleCancel(Message message)
        {
            await DeleteBaseKeyboard(message);
        }

        private async Task Usage(Message message)
        {
            const string usage = "Что я умею:\n" +
                                    "Мне нужно отправлять суммы расходов в рублях, а затем выбирать к какой категории расходов они относятся. " +
                                    "Если надо сохранить приход, то перед суммой должен ыть занк \"+\"";
            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage
            );
        }

        private async Task SendBaseKeyboard(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[] { new KeyboardButton("/Помощь") },
                resizeKeyboard: true
            ); ;

            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привет!",
                replyMarkup: replyKeyboardMarkup
            );
        }

        private async Task DeleteBaseKeyboard(Message message)
        {
            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Клавиатура удалена.",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private async Task HandleNonComandMessage(Message message)
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
                    RegisterDate = message.Date
                });

                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"{(isIncome ? "" : "-")}{message.Text} руб",
                    replyMarkup: GetInlineKeyboard(isIncome, savedData.Id)
                );
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понимаю что вы от меня хотите."
                );
            }
        }

        private InlineKeyboardMarkup GetInlineKeyboard(bool isIncome, string savedDataId)
        {
            var rows = new List<InlineKeyboardButton[]>();
            var categoryCollection = isIncome ? _categories.Value.IncomeMoneyCategories : _categories.Value.OutMoneyCategories;
            var categoriInLine = _categories.Value.CategoryInLine;
            var categoriesArrays = categoryCollection.SplitArray(categoriInLine);

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
