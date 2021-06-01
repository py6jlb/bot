using System.Linq;
using System.Threading.Tasks;
using Bot.WebService.Options;
using Bot.WebService.Services.Base;
using Bot.WebService.Services.Dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.WebService.Services
{
    public class CallbackQueryHandler : ICallbackQueryHandler
    {
        private readonly IBotService _botService;
        private readonly IOptions<Categories> _categories;
        private readonly IDataService _dataService;

        public CallbackQueryHandler(IBotService botService, IOptions<Categories> categories, IDataService dataService)
        {
            _botService = botService;
            _categories = categories;
            _dataService = dataService;
        }

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            await _botService.Client.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);
            
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

            await _botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id, $"Категория выбрана");
            await _botService.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, 
                callbackQuery.Message.MessageId,
                msg, 
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[0]));
        }

    }
}
