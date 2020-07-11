using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.TelegramWorker.Services.Abstractions
{
    public interface IMessageHandler
    {
        Task HandleMessage(Message message);
    }
}
