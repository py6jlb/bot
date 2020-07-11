using Bot.TelegramWorker.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.TelegramWorker.HostedServices
{
    public class TelegramWorkerService : BackgroundService
    {
        private readonly IUpdateHandler _handler;
        public TelegramWorkerService(IUpdateHandler handler)
        {
            _handler = handler;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _handler.ReceiveAsync(null, stoppingToken);
        }
    }
}