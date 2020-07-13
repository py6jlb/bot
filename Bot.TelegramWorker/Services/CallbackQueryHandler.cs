using Bot.TelegramWorker.Options;
using Bot.TelegramWorker.Services.Abstractions;
using Bot.TelegramWorker.Services.Dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramWorker.Services
{
    public class CallbackQueryHandler : ICallbackQueryHandler
    {
        private readonly TelegramBotClient _bot;
        private readonly IOptions<Categories> _categories;
        private readonly IDataService _dataService;

        public CallbackQueryHandler(TelegramBotClient bot, IOptions<Categories> categories, IDataService dataService)
        {
            _bot = bot;
            _categories = categories;
            _dataService = dataService;
        }

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            await _bot.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);
            
            var callbackData = JsonConvert.DeserializeObject<CallbackInfo>(callbackQuery.Data);
            var isIncome = callbackQuery.Message.Text.StartsWith("+");
            var categoryCollection = isIncome ? _categories.Value.IncomeMoneyCategories : _categories.Value.OutMoneyCategories;
            var category = categoryCollection.First(x => x.Name == callbackData.Ctg);
            var msg = $"{callbackQuery.Message.Text}, в категории \"{category.HumanName}\" {category.Icon}";
            
            await _dataService.SetCategory(callbackData.Id, new CategoryInfo
            {
                CategoryName = category.Name,
                CategoryHumanName = category.HumanName,
                Icon = category.Icon
            });

            await _bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"Категория выбрана");
            await _bot.EditMessageTextAsync(callbackQuery.Message.Chat.Id, 
                callbackQuery.Message.MessageId,
                msg, 
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[0]));
        }

    }
}
