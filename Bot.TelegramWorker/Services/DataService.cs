using Bot.TelegramWorker.Services.Abstractions;
using Bot.TelegramWorker.Services.Dto;
using System;
using System.Threading.Tasks;

namespace Bot.TelegramWorker.Services
{
    public class DataService : IDataService
    {

        public async Task<BaseInfo> SaveBaseData(BaseInfo data)
        {
            data.Id = "507h096e210a18719ea877a2";
            return await Task.FromResult(data);
        }

        public async Task SetCategory(Guid id, BaseInfo data)
        {
            await Task.CompletedTask;
        }
    }
}
