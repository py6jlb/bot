using System.Threading.Tasks;
using Bot.WebService.Services.Dto;

namespace Bot.WebService.Services.Base
{
    public interface IDataService
    {
        Task<BaseInfo> SaveBaseData(BaseInfo data);
        Task<bool> SetCategory(string id, CategoryInfo data);
    }
}
