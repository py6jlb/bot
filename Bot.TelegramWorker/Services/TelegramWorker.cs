using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Bot.TelegramWorker.Services
{
    public class TelegramWorker : BackgroundService
    {
        private readonly TelegramBotClient _client;
        public TelegramWorker(TelegramBotClient client)
        {
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.StartReceiving(null, stoppingToken);
        }
    }
}