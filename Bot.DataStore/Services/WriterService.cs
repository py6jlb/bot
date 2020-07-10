using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Protos;

namespace Bot.DataStore.Services
{
    public class WriterService : Writer.WriterBase
    {
        private readonly ILogger<WriterService> _logger;
        public WriterService(ILogger<WriterService> logger)
        {
            _logger = logger;
        }

        public override Task<WriteSpendingDataResponce> SaveSpendingData(WriteSpendingDataRequest request, ServerCallContext context)
        {
            return Task.FromResult(new WriteSpendingDataResponce());
        }
    }
}