using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.DataAccessLayer.Abstraction;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Protos;

namespace Bot.DataStore.Services
{
    public class ReadService : Reader.ReaderBase
    {
        private readonly ILogger<ReadService> _logger;
        private readonly IMoneyTransactionRepository _moneyTransactionRepository;

        public ReadService(ILogger<ReadService> logger, IMoneyTransactionRepository moneyTransactionRepository)
        {
            _logger = logger;
            _moneyTransactionRepository = moneyTransactionRepository;
        }

        public override Task<IEnumerable<ReadSpendingDataResponse>> ReadSpendingData(ReadSpendingDataRequest request, IServerStreamWriter<ReadSpendingDataResponse> responseStream, ServerCallContext context)
        {
            var data = _moneyTransactionRepository.FindAll().Select(x =>
                new ReadSpendingDataResponse
                {
                    FromUserName = x.FromUserName,
                    Id = x.Id.ToString(),
                    RegisterDate = (x.RegisterDate - new DateTime(1970, 1, 1)).TotalMilliseconds,
                    Sign = x.Sign,
                    Sum = x.Sum
                }
            );
            return Task.FromResult(data);
        }
    }
}