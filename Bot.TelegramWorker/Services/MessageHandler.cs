using Bot.TelegramWorker.Options;
using Bot.TelegramWorker.Services.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
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

        public MessageHandler(TelegramBotClient bot, IOptions<Categories> categories)
        {
            _bot = bot;
            _categories = categories;

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
                                    "Мне нужно отправлять суммы расходов, а затем выбирать к какой категории расходов они относятся. " +
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
            var is_income = message.Text.StartsWith("+");
            var msg_text = is_income ? message.Text.Substring(1) : message.Text;
            var is_float = float.TryParse(msg_text, out var num);
            if (is_float)
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вы прислали число",
                    replyMarkup: is_income ? GetIncomeMoneyInlineKeyboard() : GetOutMoneyInlineKeyboard()
                ); ;
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не понимаю что вы от меня хотите(((("
                );
            }
        }



        private InlineKeyboardMarkup GetOutMoneyInlineKeyboard()
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("1.1", "11"),
                    InlineKeyboardButton.WithCallbackData("1.2", "12"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("2.1", "21"),
                    InlineKeyboardButton.WithCallbackData("2.2", "22"),
                }
            });

            return inlineKeyboard;
        }

        private InlineKeyboardMarkup GetIncomeMoneyInlineKeyboard()
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("3.1", "1134"),
                    InlineKeyboardButton.WithCallbackData("4.2", "1sdfsdf2"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("5.1", "sdfgsdf21"),
                    InlineKeyboardButton.WithCallbackData("6.2", "2sdfgsfd2"),
                }
            });

            return inlineKeyboard;
        }


        private async Task SendInlineKeyboard(Message message)
        {
            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            await Task.Delay(500);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("1.1", "11"),
                    InlineKeyboardButton.WithCallbackData("1.2", "12"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("2.1", "21"),
                    InlineKeyboardButton.WithCallbackData("2.2", "22"),
                }
            });

            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard
            );
        } 

        private async Task SendFile(Message message)
        {
            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
            await _bot.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture"
            );
        }

        private async Task RequestContactAndLocation(Message message)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                    KeyboardButton.WithRequestLocation("Местоположение"),
                    KeyboardButton.WithRequestContact("Контакт"),
            });
            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Кто или Где ты?",
                replyMarkup: RequestReplyKeyboard
            );
        }



    }
}
