﻿using Bot.WebService.Options;
using Bot.WebService.Services.Base;
using Microsoft.Extensions.Options;
using MihaZupan;
using Telegram.Bot;

namespace Bot.WebService.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            Client = string.IsNullOrEmpty(_config.Socks5Host)
                ? new TelegramBotClient(_config.BotToken)
                : new TelegramBotClient(
                    _config.BotToken,
                    new HttpToSocks5Proxy(_config.Socks5Host, _config.Socks5Port));
        }

        public TelegramBotClient Client { get; }
    }
}