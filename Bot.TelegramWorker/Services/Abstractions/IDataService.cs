using Bot.TelegramWorker.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface IDataService
    {
        Task<BaseInfo> SaveBaseData(BaseInfo data);
        Task<bool> SetCategory(string id, CategoryInfo data);
    }
}
