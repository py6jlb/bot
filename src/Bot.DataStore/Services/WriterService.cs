using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Protos;
using System.Threading.Tasks;
using Bot.DataAccessLayer.Abstraction;
using Bot.DataAccessLayer.Entities;
using LiteDB;

namespace Bot.DataStore.Services
{
    public class WriterService : Writer.WriterBase
    {
        private readonly ILogger<WriterService> _logger;
        private readonly IMoneyTransactionRepository _moneyTransactionRepository;
        public WriterService(ILogger<WriterService> logger, IMoneyTransactionRepository moneyTransactionRepository)
        {
            _logger = logger;
            _moneyTransactionRepository = moneyTransactionRepository;
        }

        public override Task<SaveMoneyTransactionResponse> SaveMoneyTransaction(SaveMoneyTransactionRequest request, ServerCallContext context)
        {
            var newData = new MoneyTransaction
            {
                FromUserName = request.FromUserName,
                Sign = request.Sign,
                RegisterDate = (new DateTime(1970, 1, 1)).AddMilliseconds(request.RegisterDate),
                Sum = request.Sum
            };
            var id = _moneyTransactionRepository.Insert(newData);
            return Task.FromResult(new SaveMoneyTransactionResponse {Id = id.ToString()});
        }

        public override Task<SetCategoryResponse> SetCategory(SetCategoryRequest request, ServerCallContext context)
        {
            var data = _moneyTransactionRepository.FindOne(new ObjectId(request.Id));
            if (data == null) return Task.FromResult(new SetCategoryResponse {Success = false});
            data.CategoryHumanName = request.CategoryHumanName;
            data.CategoryIcon = request.CategoryIcon;
            data.CategoryName = request.CategoryName;
            var res = _moneyTransactionRepository.Update(data);
            return Task.FromResult(new SetCategoryResponse { Success = res});
        }
    }
}